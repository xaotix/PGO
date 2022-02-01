using Conexoes;
using System;
using System.Windows;

namespace PGO
{
    /// <summary>
    /// Interaction logic for Loading.xaml
    /// </summary>
    public partial class Loading : Window
    {

        public Loading()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Versao.Content = "V" + System.Windows.Forms.Application.ProductVersion;
        }
    }
}
