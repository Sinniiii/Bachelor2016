﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace TagExpansionPrototype
{
    /// <summary>
    /// Interaction logic for CombinedTagScatterItem.xaml
    /// </summary>
    public partial class CombinedTagScatterItem : ScatterViewItem, INotifyPropertyChanged
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

        private TagVisualizationDummy
            tag1,
            tag2;

        #endregion

        #region Properties

        private long totalTagValue;
        public long TotalTagValue
        {
            get { return totalTagValue; }
            private set { totalTagValue = value; NotifyPropertyChanged(); }
        }

        #endregion

        private void Initialize()
        {
            Center = (Point)(((Vector)tag1.Center + (Vector)tag2.Center) / 2d);
            Orientation = (tag1.Orientation + tag2.Orientation) / 2d;

            TotalTagValue = tag1.VisualizedTag.Value * 256 + tag2.VisualizedTag.Value;
        }

        private void DetermineLeftMostTag()
        {
            // Assume tag1 is leftmost
            if (tag1.Orientation >= 0d && tag1.Orientation < 90d)
            {
                // Second tag should be to the right of first tag
                if (tag2.Center.X < tag1.Center.X)
                    SwitchTags();
            }
            else if (tag1.Orientation >= 90d && tag1.Orientation < 180d)
            {
                // Second tag should be below the first tag
                if (tag2.Center.Y < tag1.Center.Y)
                    SwitchTags();
            }
            else if(tag1.Orientation >= 180d && tag1.Orientation < 270d)
            {
                // Second tag should be to the left of first tag
                if (tag2.Center.X > tag1.Center.X)
                    SwitchTags();
            }
            else
            {
                // Second tag should be above the first tag
                if (tag2.Center.Y > tag1.Center.Y)
                    SwitchTags();
            }
        }

        private void SwitchTags()
        {
            TagVisualizationDummy temp = tag1;
            tag1 = tag2;
            tag2 = temp;
        }

        public CombinedTagScatterItem(TagVisualizationDummy tag1, TagVisualizationDummy tag2)
        {
            InitializeComponent();

            this.tag1 = tag1;
            this.tag2 = tag2;

            tag1.ConnectedCombinedTag = this;
            tag2.ConnectedCombinedTag = this;

            DetermineLeftMostTag();
            Initialize();
        }
    }
}
