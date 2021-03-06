﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Surface.Presentation.Controls;
using System.Windows.Input;
using Microsoft.Surface.Presentation.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace Product_Browser.ScatterItems
{
    public enum RunState
    {
        HighPriority,
        LowPriority,
        Spawn,
        Locked
    }

    /// <summary>
    /// A simple class for containing the physics details of a scatter item and animation
    /// stuff. Inherit this in scatterviewitems.
    /// </summary>
    public abstract class ABBScatterItem : ScatterViewItem, INotifyPropertyChanged
    {
        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region Fields

        // Physics-related constants
        private const double
            ACCELERATION = 0.5d,
            SPAWN_DECELERATION = 0.15d,
            SPEED_ANGULAR = 4d,
            SPEED_SIZE = 6d,
            DEGREES_TO_RADIANS = (2 * Math.PI) / 360d;

        #endregion

        #region Properties

        // The offset, to smartcard position, where we should start pulling this card in
        public Point PullOffset { get; set; } = new Point();

        // The offset, to smartcard position, where this card is to be placed
        public Point OriginalPositionOffset { get; set; }

        // The offset, to smartcard orientation, deciding how this card should be rotated
        public double OriginalOrientationOffset { get; set; }

        public Size OriginalSize { get; set; }

        public Vector Speed { get; set; } = new Vector();

        private Color gradientColor;
        public Color GradientColor
        {
            get { return gradientColor; }
            set { gradientColor = value; ColorThemeBrush = new SolidColorBrush(value); NotifyPropertyChanged(); }
        }

        private SolidColorBrush colorThemeBrush;
        public SolidColorBrush ColorThemeBrush
        {
            get { return colorThemeBrush; }
            set { this.colorThemeBrush = value; NotifyPropertyChanged(); }
        }

        private bool deleting = true;
        public bool Deleting
        {
            get { return deleting; }
            set
            {
                deleting = value;

                if (deleting)
                    Delete();

                NotifyPropertyChanged();
            }
        }

        #endregion

        #region EventHandlers

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            e.Handled = false; // Continue upwards, to notify tagWindow of movement

            Speed = new Vector(0, 0);
        }

        protected override void OnTouchDown(TouchEventArgs e)
        {
            base.OnTouchDown(e);
            e.Handled = false; // Continue upwards, to notify tagWindow of movement

            Speed = new Vector(0, 0);
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (!Deleting)
                base.OnPreviewMouseDown(e);
            else
                e.Handled = true;
        }

        protected override void OnPreviewTouchDown(TouchEventArgs e)
        {
            if (!Deleting)
                base.OnPreviewTouchDown(e);
            else
                e.Handled = true;
        }

        protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
        {
            if (!Deleting)
                base.OnManipulationDelta(e);
            else
                e.Handled = true;
        }

        virtual public void AnimationPulseHandler(object sender, EventArgs args)
        { }

        #endregion

        #region Methods

        /// <summary>
        /// Runs in catch-up mode, trying to achieve the original position around the smartcard.
        /// Returns the new run state
        /// </summary>
        /// <param name="tagPosition"></param>
        /// <param name="tagRotation"></param>
        /// <returns></returns>
        public RunState RunHighPriority(Point tagPosition, double tagRotation)
        {
            if (AreAnyTouchesCapturedWithin || IsMouseCaptured) // User interferred, turn to low priority
                return RunState.LowPriority;

            Vector screenTargetPosition = GetConvertedPosition(tagPosition, OriginalPositionOffset, tagRotation);
            double screenTargetOrientation = (tagRotation + OriginalOrientationOffset + 360d) % 360d; // Add 360 and mod 360 to ensure target angle always positive

            if (TryLockPosition(screenTargetPosition, screenTargetOrientation)) // Locked in place?
                return RunState.Locked;

            Vector relativeTargetPosition = screenTargetPosition - (Vector)Center;

            // Recalculate speed
            Speed = CalculateSpeed(relativeTargetPosition);

            // Move this scatter item based on speed and target orientation, resize as needed
            Transform(relativeTargetPosition, screenTargetOrientation);

            return RunState.HighPriority;
        }

        /// <summary>
        /// Runs in range-check mode, determining if the distance from capture point is low enough that
        /// we should switch to high priority mode. Returns the new run state
        /// </summary>
        /// <param name="tagPosition"></param>
        /// <param name="tagRotation"></param>
        /// <param name="pullRadius"></param>
        /// <returns></returns>
        public RunState RunLowPriority(Point tagPosition, double tagRotation, double pullRadius)
        {
            Vector circlePosition = GetConvertedPosition(tagPosition, PullOffset, tagRotation);

            double distance = ((Vector)Center - circlePosition).Length;

            if (distance >= pullRadius || AreAnyTouchesCapturedWithin || IsMouseCaptured)
                if(Center.X > 0 && Center.X < Microsoft.Surface.Core.InteractiveSurface.PrimarySurfaceDevice.Width
                    && Center.Y > 0 && Center.Y < Microsoft.Surface.Core.InteractiveSurface.PrimarySurfaceDevice.Height)
                return RunState.LowPriority;

            return RunState.HighPriority;
        }

        public RunState RunSpawn()
        {
            if (Speed.X == 0 && Speed.Y == 0)
                return RunState.LowPriority;

            Vector deceleration = Speed;
            deceleration.Negate();
            deceleration.Normalize();
            deceleration *= SPAWN_DECELERATION;

            Speed = new Vector(Speed.X + deceleration.X, Speed.Y + deceleration.Y);

            if (Speed.Length < SPAWN_DECELERATION)
                Speed = new Vector();

            Center = new Point(Center.X + Speed.X, Center.Y + Speed.Y);

            return RunState.Spawn;
        }

        public void MoveToOriginalPosition(Point tagPosition, double tagRotation)
        {
            Vector screenTargetPosition = GetConvertedPosition(tagPosition, OriginalPositionOffset, tagRotation);
            double screenTargetOrientation = (tagRotation + OriginalOrientationOffset + 360d) % 360d; // Add 360 and mod 360 to ensure target angle always positive

            Center = (Point)screenTargetPosition;
            Orientation = screenTargetOrientation;
        }

        /// <summary>
        /// Calculates new speed based on max acceleration and relative position
        /// </summary>
        /// <param name="relativePosition"></param>
        private Vector CalculateSpeed(Vector targetPosition)
        {
            double distance = targetPosition.Length;

            double travelTime = Math.Sqrt(Math.Abs(4 * distance / ACCELERATION));

            double tempMaxSpeed = distance * 2 / travelTime; // Max speed = average speed * 2(since linear acceleration then deceleration)

            targetPosition.Normalize();

            double deltaSpeedX = targetPosition.X * tempMaxSpeed - Speed.X;
            double deltaSpeedY = targetPosition.Y * tempMaxSpeed - Speed.Y;

            if (deltaSpeedX > ACCELERATION)
                deltaSpeedX = ACCELERATION;
            else if (deltaSpeedX < -ACCELERATION)
                deltaSpeedX = -ACCELERATION;

            if (deltaSpeedY > ACCELERATION)
                deltaSpeedY = ACCELERATION;
            else if (deltaSpeedY < -ACCELERATION)
                deltaSpeedY = -ACCELERATION;

            return new Vector(Speed.X + deltaSpeedX, Speed.Y + deltaSpeedY);
        }

        /// <summary>
        /// Moves and resizes this object based on target position/orientation and original values
        /// </summary>
        /// <param name="relativeTargetPosition"></param>
        /// <param name="tagOrientation"></param>
        private void Transform(Vector relativeTargetPosition, double tagOrientation)
        {
            Center = new Point(Center.X + Speed.X, Center.Y + Speed.Y);

            double targetAngle = (tagOrientation + OriginalOrientationOffset) * DEGREES_TO_RADIANS;
            double currentAngle = Orientation * DEGREES_TO_RADIANS;

            double deltaAngle = Math.Atan2(Math.Sin(targetAngle - currentAngle), Math.Cos(targetAngle - currentAngle)) / DEGREES_TO_RADIANS;

            if (deltaAngle > SPEED_ANGULAR)
                deltaAngle = SPEED_ANGULAR;
            else if (deltaAngle < -SPEED_ANGULAR)
                deltaAngle = -SPEED_ANGULAR;

            Orientation = (Orientation + deltaAngle + 360d) % 360d;

            double deltaWidth = OriginalSize.Width - Width;

            if (deltaWidth > SPEED_SIZE)
                deltaWidth = SPEED_SIZE;
            else if (deltaWidth < -SPEED_SIZE)
                deltaWidth = -SPEED_SIZE;

            Width += deltaWidth;
            
            // Keep aspect ratio constant throughout size change
            double deltaHeight = deltaWidth * (OriginalSize.Height / OriginalSize.Width);

            Height += deltaHeight;
        }

        /// <summary>
        /// Attempts to lock this scatteritem in its original position around the smartcard. Returns true when
        /// position, orientation and size are all at their original values.
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetAngle"></param>
        /// <returns></returns>
        private bool TryLockPosition(Vector targetPosition, double targetAngle)
        {
            double deltaX = Math.Abs(targetPosition.X - Center.X);

            if (deltaX > 1d)
                return false;

            double deltaY = Math.Abs(targetPosition.Y - Center.Y);

            if (deltaY > 1d)
                return false;

            // Orientation can be < 0 due to our movement, but targetAngle won't be. Thus, use ActualOrientation, which is also never < 0
            double deltaZ = Math.Abs(targetAngle - ActualOrientation);

            if (deltaZ > 1d)
                return false;

            double deltaWidth = Math.Abs(OriginalSize.Width - Width);

            if (deltaWidth > 1d)
                return false;

            double deltaHeight = Math.Abs(OriginalSize.Height - Height);

            if (deltaHeight > 1d)
                return false;

            Center = new Point(targetPosition.X, targetPosition.Y);
            Orientation = targetAngle;
            Width = OriginalSize.Width;
            Height = OriginalSize.Height;

            return true;
        }

        /// <summary>
        /// Converts a position based on rotation
        /// </summary>
        /// <param name="visualizerPos"></param>
        /// <param name="Offset"></param>
        /// <param name="orientation"></param>
        /// <returns></returns>
        public static Vector GetConvertedPosition(Point visualizerPos, Point Offset, double orientation)
        {
            double radians = orientation * DEGREES_TO_RADIANS;
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);

            double convertedX = visualizerPos.X + Offset.X * cos - Offset.Y * sin;
            double convertedY = visualizerPos.Y + Offset.Y * cos + Offset.X * sin;

            return new Vector(convertedX, convertedY);
        }

        /// <summary>
        /// Should be overwritten in any class that uses a dispatcher timer, to stop the timer upon delete
        /// to prevent memory leak.
        /// </summary>
        protected virtual void Delete()
        {

        }

        #endregion

        public ABBScatterItem() { }
    }
}