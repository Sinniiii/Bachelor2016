using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Surface.Presentation.Controls;

namespace Product_Browser.ScatterItems
{
    /// <summary>
    /// A simple class for containing the physics details of a scatter item.
    /// </summary>
    public class ScatterItemPhysics
    {
        public const double
            ACCELERATION = 0.5d,
            SPEED_ANGULAR = 2d,
            SPEED_SIZE = 2d;

        public ScatterViewItem Item { get; private set; }

        public delegate void PositionLockedHandler(ScatterItemPhysics e);
        public event PositionLockedHandler PositionLocked;

        public delegate void PriorityHandler(ScatterItemPhysics e);
        public event PriorityHandler LowPriority, HighPriority;

        #region Properties
        public Point OriginalPositionOffset { get; set; } = new Point();

        public double OriginalOrientationOffset { get; set; }

        public double OriginalWidth { get; set; }

        public double OriginalHeight { get; set; }

        public double SpeedX { get; set; }

        public double SpeedY { get; set; }

        #endregion

        public void Run(Point tagPosition, double tagRotation, double yPullOffset, double pullRadius)
        {
            Point circlePosition = GetConvertedPosition(tagPosition, new Point(0d, yPullOffset), tagRotation);

            double distance = (Item.Center - circlePosition).Length;

            if (distance >= pullRadius || Item.AreAnyTouchesCapturedWithin || Item.IsMouseCaptured)
            {
                LowPriority(this);
                return;
            }

            Point targetPosition = GetConvertedPosition(tagPosition, OriginalPositionOffset, tagRotation);

            Vector relativePosition = targetPosition - Item.Center;
               
            CalculateSpeed(relativePosition);

            Move(tagRotation);

            TryLockPosition(new Vector(targetPosition.X, targetPosition.Y), (tagRotation + OriginalOrientationOffset) % 360d);
        }

        public void RunLowPriority(Point tagPosition, double tagRotation, double yPullOffset, double pullRadius)
        {
            Point circlePosition = GetConvertedPosition(tagPosition, new Point(0d, yPullOffset), tagRotation);

            double distance = (Item.Center - circlePosition).Length;

            if (distance >= pullRadius || Item.AreAnyTouchesCapturedWithin || Item.IsMouseCaptured)
                return;

            HighPriority(this);
        }

        private void CalculateSpeed(Vector relativePosition)
        {
            double distance = relativePosition.Length;

            double travelTime = Math.Sqrt(Math.Abs(4 * distance / ACCELERATION));
            
            double tempMaxSpeed = distance * 2 / travelTime; // Max speed = average speed * 2(since linear acceleration then deceleration)

            relativePosition.Normalize();

            double deltaSpeedX = relativePosition.X * tempMaxSpeed - SpeedX;
            double deltaSpeedY = relativePosition.Y * tempMaxSpeed - SpeedY;

            if (deltaSpeedX > ACCELERATION)
                deltaSpeedX = ACCELERATION;
            else if (deltaSpeedX < -ACCELERATION)
                deltaSpeedX = -ACCELERATION;

            if (deltaSpeedY > ACCELERATION)
                deltaSpeedY = ACCELERATION;
            else if (deltaSpeedY < -ACCELERATION)
                deltaSpeedY = -ACCELERATION;

            SpeedX += deltaSpeedX;
            SpeedY += deltaSpeedY;
        }

        private void Move(double tagOrientation)
        {
            Item.Center = new Point(Item.Center.X + SpeedX, Item.Center.Y + SpeedY);

            double deltaAngle = (tagOrientation + OriginalOrientationOffset) - Item.Orientation;
            deltaAngle = (deltaAngle + 180) % 360 - 180;

            if (deltaAngle > SPEED_ANGULAR)
                deltaAngle = SPEED_ANGULAR;
            else if (deltaAngle < -SPEED_ANGULAR)
                deltaAngle = -SPEED_ANGULAR;

            Item.Orientation += deltaAngle;

            double deltaWidth = OriginalWidth - Item.Width;

            if (deltaWidth > SPEED_SIZE)
                deltaWidth = SPEED_SIZE;
            else if (deltaWidth < -SPEED_SIZE)
                deltaWidth = -SPEED_SIZE;

            Item.Width += deltaWidth;

            double deltaHeight = OriginalHeight - Item.Height;

            if (deltaHeight > SPEED_SIZE)
                deltaHeight = SPEED_SIZE;
            else if (deltaHeight < -SPEED_SIZE)
                deltaHeight = -SPEED_SIZE;

            Item.Height += deltaHeight;
        }

        private void TryLockPosition(Vector targetPosition, double targetAngle)
        {
            double deltaX = Math.Abs(targetPosition.X - Item.Center.X);

            if (deltaX > 3d)
                return;

            double deltaY = Math.Abs(targetPosition.Y - Item.Center.Y);

            if (deltaY > 3d)
                return;

            // Orientation can be < 0 due to our movement, but targetAngle won't be. Thus, use ActualOrientation, which is also never < 0
            double deltaZ = Math.Abs(targetAngle - Item.ActualOrientation);

            if (deltaZ > 3d)
                return;

            double deltaWidth = Math.Abs(OriginalWidth - Item.Width);

            if (deltaWidth > 3d)
                return;

            double deltaHeight = Math.Abs(OriginalHeight - Item.Height);

            if (deltaHeight > 3d)
                return;
            
            Item.Center = new Point(targetPosition.X, targetPosition.Y);
            Item.Orientation = targetAngle;
            Item.Width = OriginalWidth;
            Item.Height = OriginalHeight;

            PositionLocked(this);
        }

        public void ResetToDefault(Point visualizerPosition, double visualizerOrientation)
        {
            Item.Center = GetConvertedPosition(visualizerPosition, OriginalPositionOffset, visualizerOrientation);
            Item.Orientation = OriginalOrientationOffset + visualizerOrientation;
            Item.Width = OriginalWidth;
            Item.Height = OriginalHeight;
        }

        private static Point GetConvertedPosition(Point visualizerPos, Point Offset, double orientation)
        {
            double radians = orientation / 57.295d;
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);

            double convertedX = visualizerPos.X + Offset.X * cos - Offset.Y * sin;
            double convertedY = visualizerPos.Y + Offset.Y * cos + Offset.X * sin;

            return new Point(convertedX, convertedY);
        }

        public ScatterItemPhysics(ScatterViewItem item, double xOffset, double yOffset, double orientationOffset, double startWidth, double startHeight)
        {
            this.Item = item;

            OriginalOrientationOffset = orientationOffset;
            OriginalPositionOffset = new Point(xOffset, yOffset);
            OriginalWidth = startWidth;
            OriginalHeight = startHeight;
        }
    }
}
