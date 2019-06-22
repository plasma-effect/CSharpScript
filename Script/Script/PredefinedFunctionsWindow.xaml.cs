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
using System.Windows.Shapes;

namespace Script
{
    /// <summary>
    /// PredefinedFunctionsWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class PredefinedFunctionsWindow : Window
    {
        public PredefinedFunctionsWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.document.Content = Properties.Settings.Default.PredefinedFunctions;
            this.SizeToContent = SizeToContent.Height;
        }
    }
}
