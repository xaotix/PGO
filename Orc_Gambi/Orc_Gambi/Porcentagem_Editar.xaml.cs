using FirstFloor.ModernUI.Windows.Controls;
using System.Linq;
using System.Windows;

namespace PGO
{
    /// <summary>
    /// Interaction logic for Porcentagem_Editar.xaml
    /// </summary>
    public partial class Porcentagem_Editar : ModernWindow
    {
        public DLM.orc.Porcentagem_Grupo Grupo { get; set; } = new DLM.orc.Porcentagem_Grupo();

        public Porcentagem_Editar(DLM.orc.Porcentagem_Grupo Grupo)
        {
            this.Grupo = Grupo;
            this.DataContext = this;
            InitializeComponent();
            this.Title = "Editar " + Grupo.Origem.ToString();
        }

        private void adicionar_ponderador(object sender, RoutedEventArgs e)
        {
            //if(Math.Round(this.Grupo.Saldo,0)!=100)
            //{
            //    Conexoes.Utilz.Alerta("Não é possível salvar, saldo total deve fechar 100%","Saldo Atual: " + this.Grupo.Saldo + "%", MessageBoxImage.Error);
            //    return;
            //}
            this.DialogResult = true;
        }

        private void cancelar(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void colocar_resto(object sender, RoutedEventArgs e)
        {
            var sels = ((FrameworkElement)sender).DataContext;
            if (sels is DLM.orc.Porcentagem)
            {
                var p = sels as DLM.orc.Porcentagem;
                var tot = p.Externas.Sum(x => x.Valor);
                var ss = 100 - tot;
                if (ss >= 0)
                {
                    p.SetPorcentagem(ss, false);
                }
                else
                {
                    Conexoes.Utilz.Alerta("O Total é maior que 100%. Não é possível ajustar com o saldo disponível. Reduza os valores até atingir menos que 100% e depois utilize essa ferramenta.");
                }
            }
        }

        private void distribuir(object sender, RoutedEventArgs e)
        {
            Grupo.Ratear();
        }

        private void zerar(object sender, RoutedEventArgs e)
        {
            Grupo.Zerar();
        }
    }
}
