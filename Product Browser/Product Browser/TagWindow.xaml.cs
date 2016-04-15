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
using System.Collections.ObjectModel;

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

        /// <summary>
        /// These are helpers to determine visibility of UI elements in TagWindow.xaml. Start by displaying loading
        /// </summary>
        bool foundSmartCard = false,
            notFoundSmartCard = false,
            loadingSmartCard = true;

        bool alreadyLoaded = false; // Keep track of whether the Loaded event has already fired, so we don't reload data from db

        #endregion

        #region Properties

        /// <summary>
        /// These are states, and should only ever be set true
        /// </summary>
        public bool FoundSmartCard { get { return foundSmartCard; }
            set
            {
                if (!value)
                    return;

                foundSmartCard = value;
                loadingSmartCard = !value;
                foundSmartCard = !value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("NotFoundSmartCard");
                NotifyPropertyChanged("LoadingSmartCard");
            }
        }

        public bool NotFoundSmartCard { get { return notFoundSmartCard; }
            set {
                if (!value)
                    return;

                notFoundSmartCard = value;
                foundSmartCard = !value;
                loadingSmartCard = !value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("LoadingSmartCard");
                NotifyPropertyChanged("FoundSmartCard");
            }
        }

        public bool LoadingSmartCard { get { return loadingSmartCard; }
            set
            {
                if (!value)
                    return;

                loadingSmartCard = value;
                notFoundSmartCard = !value;
                foundSmartCard = !value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("NotFoundSmartCard");
                NotifyPropertyChanged("FoundSmartCard");
            }
        }

        public ObservableCollection<ScatterViewItem> ScatterViewItems { get; set; } = new ObservableCollection<ScatterViewItem>();
        #endregion

        public TagWindow()
        {
            InitializeComponent();

            // Have to wait until Loaded event fires before we can initialize, otherwise
            // the VisualizedTag.Value may not be set and we won't recognize the tag.
            Loaded += InitializeSmartCard;
        }

        private async void InitializeSmartCard(object sender, EventArgs e)
        {
            if (alreadyLoaded) // Don't want to load a Smart Card's data more than once; possible because Loaded can fire several times
                return;

            alreadyLoaded = true;

            ABBDataContext context = new ABBDataContext();
            
            smartCard = await context.SmartCards.FirstOrDefaultAsync(a => a.TagId == VisualizedTag.Value);

            List<SmartCardDataItem> dataItems = null;

            if (smartCard == null || (dataItems = smartCard.DataItems) == null || dataItems.Count == 0)
            {
                NotFoundSmartCard = true;
                return;
            }

            FoundSmartCard = true;

            foreach (SmartCardDataItem item in dataItems)
            {
                switch (item.Category)
                {
                    case SmartCardDataItemCategory.Document:
                        ScatterViewItems.Add(new DocumentScatterItem(item));
                        break;
                    case SmartCardDataItemCategory.Image:
                        ScatterViewItems.Add(new ImageScatterItem(item)); // Temporary, for final all images should be sent to same imagescatteritem
                        break;
                    case SmartCardDataItemCategory.Video:
                        ScatterViewItems.Add(new VideoScatterItem(item));
                        break;
                }
            }
        }
    }
}
