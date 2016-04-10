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

        private long _value;
        public long Value {
            get { return _value; }
            set { _value = value;
                NotifyPropertyChanged();
            }
        }

        public TagWindow()
        {
            InitializeComponent();
            DataContext = this;

            Loaded += WindowLoaded;
        }

        private void WindowLoaded(object sender, EventArgs e)
        {
            Value = VisualizedTag.Value;
        }
    }
}
