using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Surface.Presentation.Controls;
using System.Windows;

namespace ProductBrowser
{
    class PublicMethods
    {

        public static void organizeobjects(ScatterViewItem o, double orientation, Size size)
        {

            o.Orientation = orientation;
            o.Width = size.Width;
            o.Height = size.Height;
            if (orientation == 0)
            {
                o.Center = new System.Windows.Point(o.ActualCenter.X, System.Windows.SystemParameters.PrimaryScreenHeight + (size.Height / 4));

            }
            else if (orientation == 90)
            {
                o.Center = new System.Windows.Point(-(size.Height / 4), o.ActualCenter.Y);
            }
            else if (orientation == 180)
            {
                o.Center = new System.Windows.Point(o.ActualCenter.X, -(size.Height / 4));
            }
            else if (orientation == 270)
            {
                o.Center = new System.Windows.Point(System.Windows.SystemParameters.PrimaryScreenWidth + (size.Height / 4), o.ActualCenter.Y);
            }

        }


    }
}
