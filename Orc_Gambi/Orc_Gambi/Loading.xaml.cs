using Conexoes;
using System;
using System.Windows;

namespace Orc_Gambi
{
    /// <summary>
    /// Interaction logic for Loading.xaml
    /// </summary>
    public partial class Loading : Window
    {
        public bool carrega_principal { get; set; } = false;
        public Loading(bool carrega_principal = true)
        {
            this.carrega_principal = carrega_principal;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Versao.Content = "V" + System.Windows.Forms.Application.ProductVersion;

            if(carrega_principal)
            {
                if (!Utilz.DecimalOk())
                {
                    Conexoes.Utilz.Alerta("Sistema regional de casas decimais configurado incorretamente. \nFavor ajustar  para {,} (vírgula) antes de utilizar ferramenta.", "", MessageBoxImage.Error);
                    System.Windows.Application.Current.Shutdown();
                }
                MainWindow mm = new MainWindow();
                mm.Show();
                //Funcoes_Mapa.GetEndereco(mm.Obras, mm.myMap);
                this.Close();
            }

        }
    }
}
