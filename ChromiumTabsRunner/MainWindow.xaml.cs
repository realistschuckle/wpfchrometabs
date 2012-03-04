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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChromiumTabsRunner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void HandleAddTab(object sender, RoutedEventArgs e)
        {
            this.chrometabs.AddTab(new Button { Content = "MOO!" }, false);
        }

        private void HandleAddTabAndSelect(object sender, RoutedEventArgs e)
        {
            this.chrometabs.AddTab(new Button { Content = "MOO!" }, true);
        }

        private void HandleRemoveTab(object sender, RoutedEventArgs e)
        {
            this.chrometabs.RemoveTab(this.chrometabs.SelectedItem);
        }
    }
}
