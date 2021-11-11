using FirstFloor.ModernUI.Windows.Controls;
using System.Linq;
using System.Windows;

namespace PGO
{
    /// <summary>
    /// Interaction logic for Listas_Tecnicas_Cronograma.xaml
    /// </summary>
    public partial class Listas_Tecnicas_Cronograma : ModernWindow
    {
        public Listas_Tecnicas_Cronograma()
        {

            InitializeComponent();
            this.Lista_Ranges.ItemsSource = Conexoes.Orcamento.PGOVars.GetDbOrc().GetProdutos_Clean();
        }

        private void salvar_alteracoes(object sender, RoutedEventArgs e)
        {
            Conexoes.Orcamento.Produto sel = ((FrameworkElement)sender).DataContext as Conexoes.Orcamento.Produto;
            if (sel == null) { return; }
            sel.SalvarDatas();
        }

        private void editar_e_dias_1(object sender, RoutedEventArgs e)
        {
            var sel = Lista_Ranges.SelectedItems.Cast<Conexoes.Orcamento.Produto>().ToList();
            if (sel.Count > 0)
            {
                int val_atual = sel[0].DIAS_ENGENHARIA_1;
                var valor = Conexoes.Utilz.Int(Conexoes.Utilz.Prompt("Digite o valor para E Dias 1", "", val_atual.ToString()));
                Conexoes.ControleWait w = Conexoes.Utilz.Wait(sel.Count);
                foreach (var ss in sel)
                {
                    ss.DIAS_ENGENHARIA_1 = valor;
                    ss.SalvarDatas();
                    w.somaProgresso();
                }
                w.Close();
            }
        }

        private void editar_e_dias(object sender, RoutedEventArgs e)
        {
            var sel = Lista_Ranges.SelectedItems.Cast<Conexoes.Orcamento.Produto>().ToList();
            if (sel.Count > 0)
            {
                int val_atual = sel[0].DIAS_ENGENHARIA;
                var valor = Conexoes.Utilz.Int(Conexoes.Utilz.Prompt("Digite o valor para E Dias", "", val_atual.ToString()));
                Conexoes.ControleWait w = Conexoes.Utilz.Wait(sel.Count);

                foreach (var ss in sel)
                {
                    ss.DIAS_ENGENHARIA = valor;
                    ss.SalvarDatas();
                    w.somaProgresso();
                }
                w.Close();
            }
        }

        private void editar_f_dias(object sender, RoutedEventArgs e)
        {
            var sel = Lista_Ranges.SelectedItems.Cast<Conexoes.Orcamento.Produto>().ToList();
            if (sel.Count > 0)
            {
                int val_atual = sel[0].DIAS_FABRICACAO;
                var valor = Conexoes.Utilz.Int(Conexoes.Utilz.Prompt("Digite o valor para F Dias", "", val_atual.ToString()));
                Conexoes.ControleWait w = Conexoes.Utilz.Wait(sel.Count);
                foreach (var ss in sel)
                {
                    ss.DIAS_FABRICACAO = valor;
                    ss.SalvarDatas();
                    w.somaProgresso();
                }
                w.Close();
            }
        }

        private void editar_l_dias(object sender, RoutedEventArgs e)
        {
            var sel = Lista_Ranges.SelectedItems.Cast<Conexoes.Orcamento.Produto>().ToList();
            if (sel.Count > 0)
            {
                int val_atual = sel[0].DIAS_LOGISTICA;
                var valor = Conexoes.Utilz.Int(Conexoes.Utilz.Prompt("Digite o valor para L Dias", "", val_atual.ToString()));
                Conexoes.ControleWait w = Conexoes.Utilz.Wait(sel.Count);
                foreach (var ss in sel)
                {
                    ss.DIAS_LOGISTICA = valor;
                    ss.SalvarDatas();
                    w.somaProgresso();
                }
                w.Close();
            }
        }

        private void editar_m_dias(object sender, RoutedEventArgs e)
        {
            var sel = Lista_Ranges.SelectedItems.Cast<Conexoes.Orcamento.Produto>().ToList();
            if (sel.Count > 0)
            {
                int val_atual = sel[0].DIAS_MONTAGEM;
                var valor = Conexoes.Utilz.Int(Conexoes.Utilz.Prompt("Digite o valor para M Dias", "", val_atual.ToString()));
                Conexoes.ControleWait w = Conexoes.Utilz.Wait(sel.Count);
                foreach (var ss in sel)
                {
                    ss.DIAS_MONTAGEM = valor;
                    ss.SalvarDatas();
                    w.somaProgresso();
                }
                w.Close();
            }
        }

        private void atribuir_ferts(object sender, RoutedEventArgs e)
        {
            var sel = Lista_Ranges.SelectedItems.Cast<Conexoes.Orcamento.Produto>().ToList();
            if (sel.Count > 0)
            {
                var fert = Conexoes.Utilz.Selecao.SelecionarObjeto(Conexoes.Orcamento.PGOVars.GetDbOrc().GetDe_Para(), null, "Selecione");
                if (fert != null)
                {
                    Conexoes.ControleWait w = Conexoes.Utilz.Wait(sel.Count);
                    foreach (var ss in sel)
                    {
                        ss.setFERT(fert.FERT, Conexoes.Utilz.Int(fert.WERKS));
                        //ss.local =Conexoes.Utilz.Int(fert.WERKS);
                        ss.SalvarDatas();
                        w.somaProgresso();
                    }
                    w.Close();
                }
            }
        }
    }
}
