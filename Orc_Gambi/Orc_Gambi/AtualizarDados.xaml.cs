using DLM.orc;
using FirstFloor.ModernUI.Windows.Controls;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;


namespace PGO
{
    /// <summary>
    /// Interaction logic for AtualizarDados.xaml
    /// </summary>
    public partial class AtualizarDados : ModernWindow
    {
        public PGO_Obra Obra { get; set; }
        public AtualizarDados(PGO_Obra Ob)
        {
            if (Ob.Nacional)
            {
                this.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);

            }
            else
            {
                this.Language = XmlLanguage.GetLanguage(new CultureInfo("en-US").IetfLanguageTag);
            }
            this.Obra = Ob;
            InitializeComponent();
            this.DataContext = this;
            this.Lista.ItemsSource = this.Obra.GetRanges();
            this.listaErros.ItemsSource = this.Obra.ErrosRange;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Obra.AtualizarDadosDeCustos();
            this.Close();
        }
    }
}
