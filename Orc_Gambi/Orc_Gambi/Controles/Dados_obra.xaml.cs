using Conexoes;
using DLM.orc;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace PGO.Controles
{
    /// <summary>
    /// Interaction logic for Dados_obra.xaml
    /// </summary>
    public partial class Dados_obra : Window
    {
        public DLM.orc.PGO_Obra Obra { get; set; } = new DLM.orc.PGO_Obra();
        public Dados_obra(DLM.orc.PGO_Obra Obra)
        {
            this.Obra = Obra;
            InitializeComponent();
            this.DataContext = this;
            this.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
            this.Esquemas_de_Pintura.ItemsSource = null;
            this.Esquemas_de_Pintura.ItemsSource = Obra.GetTratamentos();
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
            var sel = Esquemas_de_Pintura.Selecoes<Tratamento>();
            if (sel.Count() > 0)
            {
                foreach (var s in sel)
                {
                    bool confirmado = s.Propriedades();
                    if (!confirmado) { return; }
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
