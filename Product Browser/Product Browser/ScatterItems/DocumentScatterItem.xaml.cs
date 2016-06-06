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

        int previousActivePage = -1;

        #endregion

        #region EventHandlers

        public override void AnimationPulseHandler(object sender, EventArgs args)
        {
            grad.Angle = (grad.Angle + 0.5d) % 360d;
        }

        private void OnStackPanelLoaded(object obj, EventArgs args)
        {
            // Remove shadow of this smartcard, or we get an ugly effect
            var ssc = this.GetTemplateChild("shadow") as Microsoft.Surface.Presentation.Generic.SurfaceShadowChrome;
            ssc.Visibility = Visibility.Hidden;

            var count = dataItem.GetDocumentPageCount();
            
            surfaceSlider.Minimum = 0d;
            surfaceSlider.Maximum = 10000d;
            pageNumber.Content = "1";

            previousActivePage = 0;

            documentName.Content = dataItem.Name;

            Binding actualWidthBinding = new Binding("ActualWidth");
            actualWidthBinding.BindsDirectlyToSource = true;
            actualWidthBinding.Source = scrollBar;

            Binding actualHeightBinding = new Binding("ActualHeight");
            actualHeightBinding.BindsDirectlyToSource = true;
            actualHeightBinding.Source = scrollBar;

            for (int i = 0; i < count; i++)
            {
                UserControl control = new UserControl();
                control.IsHitTestVisible = false;

                control.SetBinding(UserControl.WidthProperty, actualWidthBinding);
                control.SetBinding(UserControl.HeightProperty, actualHeightBinding);

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
            scrollBar.ScrollToVerticalOffset(args.NewValue / surfaceSlider.Maximum * stackPanel.ActualHeight);

            int count = stackPanel.Children.Count;
            int activePage = (int)Math.Round((args.NewValue / surfaceSlider.Maximum * count));

            if (activePage == previousActivePage)
                return;
            
            for(int i = Math.Max(0, previousActivePage - 2); (i < previousActivePage + 2 && i < count); i++)
            {
                if (i <= activePage - 2 || i >= activePage + 2)
                    ((Image)((UserControl)stackPanel.Children[i]).Content).Source = null;
            }

            for(int i = Math.Max(0, activePage - 2); (i < activePage + 2 && i < count); i++)
            {
                if(((Image)((UserControl)stackPanel.Children[i]).Content).Source == null)
                    ((Image)((UserControl)stackPanel.Children[i]).Content).Source = dataItem.GetPageFromDocumentAsImageSource(i);
            }

            previousActivePage = activePage;

            if (activePage == count)
                pageNumber.Content = (activePage).ToString();
            else
                pageNumber.Content = (activePage + 1).ToString();
        }

        private void OnSizeChanged(object o, SizeChangedEventArgs args)
        {
            if (args.PreviousSize.Height == 0)
                return;

            double ratio = args.NewSize.Height / args.PreviousSize.Height;

            scrollBar.ScrollToVerticalOffset(scrollBar.VerticalOffset * ratio);
        }

        #endregion


        public DocumentScatterItem(SmartCardDataItem document)
        {
            InitializeComponent();

            dataItem = document;

            stackPanel.Loaded += OnStackPanelLoaded;
            surfaceSlider.ValueChanged += OnSlider;

            scrollBar.SizeChanged += OnSizeChanged;
        }
    }
}
