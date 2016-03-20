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
using System.Windows.Navigation;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using System.Windows.Resources;
using System.Threading;
using pdftron.PDF;
using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using SysImages = System.Windows.Controls.Image;

namespace ProductBrowser
{
    /// <summary>
    /// Interaction logic for DocumentLibrary.xaml
    /// </summary>
    /// 

    public partial class DocumentLibrary : SurfaceWindow
    {

       

        public DocumentLibrary()
        {
            InitializeComponent();
        }


        public Grid DocumentGrid(string offering, TagWindow tagWindow, MainWindow mainWindow)
        { 
            ObservableCollection<FileList> documentList = BlobConverter.addDocumentsByOfferingName(offering, tagWindow, mainWindow);


            ICollectionView objects = CollectionViewSource.GetDefaultView(documentList);
            objects.GroupDescriptions.Add(new PropertyGroupDescription("CategoryType"));

            documentLibraryBar.ItemsSource = objects;
            CreateDocumentFrame(tagWindow);
            return libGrid;
        }

        

        private void CreateDocumentFrame(TagWindow tw)
        {
          
            documentLibraryBar.BorderThickness = new Thickness ((int)MainWindow.configurationData.BoxColorFrameThickness);
            documentLibraryBar.BorderBrush = new SolidColorBrush(tw.GetTagColour());
         
        }

        private void SurfaceWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }



    }

}
