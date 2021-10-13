using Conexoes;
using Conexoes.Orcamento;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace Orc_Gambi.Controles
{
    /// <summary>
    /// Interaction logic for Dados_obra.xaml
    /// </summary>
    public partial class Dados_obra : Window
    {
        public Conexoes.Orcamento.OrcamentoObra Obra { get; set; } = new Conexoes.Orcamento.OrcamentoObra();
        public Dados_obra(Conexoes.Orcamento.OrcamentoObra Obra)
        {
            this.Obra = Obra;
            InitializeComponent();
            this.DataContext = this;
            this.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);

        }

        private void salvar_propriedades_pacote(object sender, RoutedEventArgs e)
        {
            if (Obra.Bloqueado)
            {
                Conexoes.Utilz.Alerta("Obra está bloqueada para edições", "Obra Bloqueada", MessageBoxImage.Error);
                return;
            }
            this.Obra.Salvar_custos();
            this.Obra.SetContrato_SAP(this.Obra.Contrato_SAP);
            Conexoes.Utilz.Alerta("Dados Salvos", "Alterações Salvas", MessageBoxImage.Information);
            this.Close();
        }

        private void atribuir_codigo_esquema_pintura(object sender, RoutedEventArgs e)
        {
            atribuir_esquema();
        }

        private void atribuir_esquema()
        {
            var sel = Esquemas_de_Pintura.SelectedItems.Cast<Tratamento>();
            if (sel.Count() > 0)
            {
                int c = 1;
                foreach (var s in sel)
                {
                    Utilz.Propriedades(s, true, true);
                    if (s.Codigo != "" && s.Tipo.descricao != "")
                    {

                        s.Salvar();
                    }
                    else if (s.Codigo == "")
                    {
                        Conexoes.Utilz.Alerta("Falta preencher o código.");
                    }
                    else if (s.Tipo.descricao == "")
                    {
                        Conexoes.Utilz.Alerta("Falta preencher o tratamento.");

                    }
                }
                //Update();
            }
        }

        private void Esquemas_de_Pintura_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            atribuir_esquema();
        }

        private void editar_tratamento(object sender, RoutedEventArgs e)
        {
            atribuir_esquema();
        }
    }
}
