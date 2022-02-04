using DLM.orc;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Markup;

namespace PGO
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
        public List<PGO_Predio> Predios { get; set; } = new List<PGO_Predio>();

        public PGO_Obra Obra { get; set; } = new PGO_Obra();
        public Container_Obra(List<PGO_Predio> lista, bool check_visivel)
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
