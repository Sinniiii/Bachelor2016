using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace ProductBrowser
{
    /// <summary>
    /// Interaction logic for WatchedLibrary.xaml
    /// </summary>
    public partial class WatchedLibrary : Window
    {
        public ObservableCollection<FileList> list = new ObservableCollection<FileList>();
        public WatchedLibrary()
        {
            InitializeComponent();
        }

        public Grid WatchedGrid()
        {
           
           
            ICollectionView objects = CollectionViewSource.GetDefaultView(list);
            objects.GroupDescriptions.Add(new PropertyGroupDescription("CategoryType"));

            watchedContainer.ItemsSource = objects;
            //CreateDocumentFrame(tagWindow);
           
            return WatchedBox;
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            list.Clear();
        }

       
    }
}
