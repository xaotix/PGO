using DLMorc;
using FirstFloor.ModernUI.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Orc_Gambi
{
    /// <summary>
    /// Interaction logic for JanelaSelecionarRanges.xaml
    /// </summary>
    public partial class JanelaSelecionarRanges : ModernWindow
    {
        public List<OrcamentoPredio> Predios { get; set; } = new List<OrcamentoPredio>();
        public JanelaSelecionarRanges(List<OrcamentoPredio> lista, bool check_visivel)
        {
            InitializeComponent();
            Container_Obra mm = new Container_Obra(lista.OrderBy(x => x.numero).ToList(), check_visivel);
            this.Predios = lista;
            this.Container.Children.Add(mm);
            if (!check_visivel)
            {
                this.selecao.Visibility = Visibility.Collapsed;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void muda_selecao(object sender, RoutedEventArgs e)
        {
            bool valor = (bool)selecao.IsChecked;
            if (valor)
            {
                selecao.Content = "Limpar seleção";
            }
            else
            {
                selecao.Content = "Selecionar tudo";
            }

            foreach (var t in Predios)
            {
                t.Selecionado = valor;
            }
        }

        private void cancelar(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
