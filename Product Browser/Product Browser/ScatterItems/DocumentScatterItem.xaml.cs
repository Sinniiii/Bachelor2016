using System;
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
using Microsoft.Surface.Presentation.Input;
using System.Windows.Threading;

namespace Product_Browser.ScatterItems
{
    /// <summary>
    /// Interaction logic for DocumentScatterItem.xaml
    /// </summary>
    public partial class DocumentScatterItem : ABBScatterItem
    {
        #region Fields

        SmartCardDataItem dataItem;

        int previousActive = -1;

        #endregion

        #region EventHandlers

        public override void AnimationPulseHandler(object sender, EventArgs args)
        {
            grad.Angle = (grad.Angle + 0.5d) % 360d;
        }

        private void OnStackPanelLoaded(object obj, EventArgs args)
        {
            var pages = dataItem.GetDocumentAsThumbnailImageSources();

            Binding binding = new Binding("ActualHeight");
            binding.BindsDirectlyToSource = true;
            binding.Source = stackPanel;
            slider.SetBinding(SurfaceSlider.MaximumProperty, binding);
            slider.Minimum = 0d;

            for (int i = 0; i < pages.Count; i++)
            {
                UserControl control = new UserControl();
                control.IsHitTestVisible = false;
                
                binding = new Binding("ActualWidth");
                binding.BindsDirectlyToSource = true;
                binding.Source = scrollBar;
                control.SetBinding(UserControl.WidthProperty, binding);

                binding = new Binding("ActualHeight");
                binding.BindsDirectlyToSource = true;
                binding.Source = scrollBar;
                control.SetBinding(UserControl.HeightProperty, binding);

                Image image = new Image();
                image.IsHitTestVisible = false;
                image.Stretch = Stretch.Uniform;
                control.Content = image;

                if(i < 2)
                {
                    image.Source = dataItem.GetPageFromDocumentAsImageSource(i);
                }

                stackPanel.Children.Add(control);
            }
        }

        private void OnSlider(object obj, RoutedPropertyChangedEventArgs<double> args)
        {
            scrollBar.ScrollToVerticalOffset(args.NewValue);

            int count = stackPanel.Children.Count;
            int activeImage = (int)(args.NewValue / (stackPanel.ActualHeight / count));

            if (activeImage == previousActive)
                return;

            previousActive = activeImage;

            for(int i = 0; i < count; i++)
            {
                if (i > activeImage - 2 && i < activeImage + 2)
                    ((Image)((UserControl)stackPanel.Children[i]).Content).Source = dataItem.GetPageFromDocumentAsImageSource(i);
                else
                    ((Image)((UserControl)stackPanel.Children[i]).Content).Source = null;
            }
        }

        private void OnScrollBarSizeChanged(object o, SizeChangedEventArgs args)
        {
            if (args.PreviousSize.Height == 0) 
                return;

            double ratio = args.NewSize.Height / args.PreviousSize.Height;

            slider.Value *= ratio;
        }

        #endregion


        public DocumentScatterItem(SmartCardDataItem document)
        {
            InitializeComponent();

            dataItem = document;

            stackPanel.Loaded += OnStackPanelLoaded;
            slider.ValueChanged += OnSlider;

            scrollBar.SizeChanged += OnScrollBarSizeChanged;
        }
    }
}
