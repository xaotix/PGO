using System.Windows;
using System.Windows.Controls;

namespace Orc_Gambi
{
    /// <summary>
    /// Interação lógica para JanelaPacote.xam
    /// </summary>
    public partial class JanelaPacote : UserControl
    {
        public JanelaPacote()
        {
            InitializeComponent();
        }

        private void ver_ranges(object sender, RoutedEventArgs e)
        {
            //var t = this.DataContext as Orcamento.PacoteRange;
            //Funcoes.SelecionarRanges(t.Obra.GetPredios(t, false),false);
        }

        private void editar_propriedades(object sender, RoutedEventArgs e)
        {
            //var t = this.DataContext as Orcamento.PacoteRange;
            //Conexoes.Utilz.Propriedades(t, true, true);
            //if(Conexoes.Utilz.Pergunta("Salvar alterações?"))
            //{
            //    t.Salvar();
            //}
        }

        private void gerar_materiais(object sender, RoutedEventArgs e)
        {
            //var t = this.DataContext as Orcamento.PacoteRange;
            //Conexoes.Utilz.ShowReports(t.GerarMateriais());
        }

        private void editar_observacoes(object sender, RoutedEventArgs e)
        {
            //var t = this.DataContext as Orcamento.PacoteRange;
            //t.SetObservacoes();

        }

        private void apagar_pacote(object sender, RoutedEventArgs e)
        {
            //var t = this.DataContext as Orcamento.PacoteRange;

            //if (Conexoes.Utilz.Pergunta("Apagar item"))
            //{
            //    t.Obra.Remover(t);
            //}
        }

        private void editar_ranges(object sender, RoutedEventArgs e)
        {
            //  var t = this.DataContext as Orcamento.PacoteRange;
            //if(t.importado_sap)
            //  {
            //      if(Conexoes.Utilz.Pergunta("Esse pacote já foi importado no SAP. Deseja alterá-lo?"))
            //      {

            //      }
            //  }
            //  var ts = Funcoes.SelecionarRanges(t.Obra.GetPredios(t, true));
            //  if (Conexoes.Utilz.Pergunta("Salvar alterações?"))
            //  {

            //      t.Obra.AddEdita(t, ts);

            //      if(t.importado_sap)
            //      {
            //          if (Conexoes.Utilz.Pergunta("Enviar e-mail para " + t.email_importou + " avisando da alteração?"))
            //          {
            //              Conexoes.Email.Enviar(t.Obra.ToString() + "Pacote alterado", t.ToString() +  " Pacote foi revisado no sistema.", new List<string> { t.email_importou }, new List<string> { Conexoes.Vars.EmailAtual });
            //              t.importado_sap = false;
            //          }
            //      }

            //  }
        }
    }
}
