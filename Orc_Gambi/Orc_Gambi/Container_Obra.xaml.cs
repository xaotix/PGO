using Conexoes.Orcamento;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Orc_Gambi
{
    /// <summary>
    /// Interação lógica para Container_Obra.xam
    /// </summary>
    public partial class Container_Obra : UserControl
    {
        public Container_Obra(bool check_visivel)
        {
            InitializeComponent();
            this.DataContext = this;
            this.chkraiz.IsVisible = check_visivel;
        }
        public List<OrcamentoPredio> Predios { get; set; } = new List<OrcamentoPredio>();

        public OrcamentoObra Obra { get; set; } = new OrcamentoObra();
        public Container_Obra(List<OrcamentoPredio> lista, bool check_visivel)
        {
            InitializeComponent();
            this.Predios = lista;
            this.Lista.ItemsSource = this.Predios;
            this.chkraiz.IsVisible = check_visivel;

            if (this.Predios.Count > 0)
            {
                this.Obra = this.Predios[0].Obra;
            }

            if (this.Obra.Nacional)
            {
                this.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
            }
            else
            {
                this.Language = XmlLanguage.GetLanguage(new CultureInfo("en-US").IetfLanguageTag);
            }

            this.DataContext = this;
        }

        private void teste_selecao(object sender, Telerik.Windows.Controls.GridViewCellEditEndedEventArgs e)
        {

        }
    }
}
