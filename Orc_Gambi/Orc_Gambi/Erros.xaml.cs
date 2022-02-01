using FirstFloor.ModernUI.Windows.Controls;
using System.Linq;
using System.Windows;

namespace PGO
{
    /// <summary>
    /// Interaction logic for Erros.xaml
    /// </summary>
    public partial class Erros : ModernWindow
    {
        public Erros()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Conexoes.Utilz.CSV.SalvarLista(lista.Items.Cast<string>().ToList());
        }
    }
}
