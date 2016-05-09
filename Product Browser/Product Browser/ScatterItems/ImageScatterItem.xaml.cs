﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Surface.Presentation.Controls;
using DatabaseModel.Model;

namespace Product_Browser.ScatterItems
{
    /// <summary>
    /// Interaction logic for ImageScatterItem.xaml
    /// </summary>
    public partial class ImageScatterItem : ScatterViewItem
    {
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            e.Handled = false; // Continue upwards, to notify tagWindow of movement
        }

        protected override void OnTouchDown(TouchEventArgs e)
        {
            base.OnTouchDown(e);
            e.Handled = false; // Continue upwards, to notify tagWindow of movement
        }

        public ImageScatterItem(SmartCardDataItem image)
        {
            InitializeComponent();
            
            mainImage.Source = image.GetImageSource();
        }
    }
}
