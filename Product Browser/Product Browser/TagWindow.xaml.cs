using System;
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
using System.Windows.Shapes;
using Microsoft.Surface.Presentation.Controls;
using DatabaseModel;
using DatabaseModel.Model;
using System.Data.Entity;
using Product_Browser.ScatterItems;

namespace Product_Browser
{
    /// <summary>
    /// Interaction logic for TagWindow.xaml
    /// </summary>
    public partial class TagWindow : TagVisualization, INotifyPropertyChanged
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

        SmartCard smartCard = null;

        bool firstLoad = true;

        #endregion

        #region Properties

        private long _value;
        public long Value {
            get { return _value; }
            set { _value = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        public TagWindow()
        {
            InitializeComponent();
            DataContext = this;

            Loaded += TagWindowLoaded;
        }

        private async void TagWindowLoaded(object sender, EventArgs args)
        {
            if (!firstLoad) // Event can be fired more than once; skip out early if we already did this!
                return;

            firstLoad = false;

            Value = VisualizedTag.Value;

            ABBDataContext context = new ABBDataContext();
            
            smartCard = await context.SmartCards.FirstAsync(a => a.TagId == Value);

            if (smartCard == null)
                return; // Handle this better, maybe visual feedback

            var dataItems = smartCard.DataItems;

            foreach(SmartCardDataItem item in dataItems)
            {
                switch (item.Category)
                {
                    case SmartCardDataItemCategory.Document:
                        scatterView.Items.Add(new DocumentScatterItem(item));
                        break;
                    case SmartCardDataItemCategory.Image:
                        scatterView.Items.Add(new ImageScatterItem(item)); // Temporary, for final all images should be sent to same imagescatteritem
                        break;
                    case SmartCardDataItemCategory.Video:
                        scatterView.Items.Add(new VideoScatterItem(item));
                        break;
                }
            }
        }
    }
}
