using Conexoes;
using DLM.orc;
using DLM.vars;
using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace PGO
{
    /// <summary>
    /// Interaction logic for Propriedades.xaml
    /// </summary>
    public partial class EditarObjetoOrcamento : ModernWindow
    {
        public enum Tipologia
        {
            Tratamento,
            Tipo_Pintura,
            //Produto,
            Segmento,
            Grupo_De_Mercadoria,
            Grupo,
            Local,
            FERT,
            Frente,
            Produto,
            _,
        }
        public EditarObjetoOrcamento()
        {
            InitializeComponent();
        }

        private Tipologia Tipo = Tipologia._;


        public EditarObjetoOrcamento(Tipologia Tipo)
        {
            InitializeComponent();
            this.Tipo = Tipo;
            Update();
            this.DataContext = this;

        }

        private void Update()
        {
            this.Lista.ItemsSource = null;

            if (Tipo == Tipologia.Tratamento)
            {
               
                this.Lista.ItemsSource = DBases.GetDbOrc().GetTratamentos(true);

                this.Propriedadesm.ShowAdvancedOptions = false;
                this.Propriedadesm.ShowSearchBox = false;
                this.Propriedadesm.ShowTitle = true;
                this.Propriedadesm.ShowSortOptions = false;
                this.Tipo = Tipologia.Tratamento;
                this.Title = "Cadastrar / Editar Segmentos";

            }
            else if (Tipo == Tipologia.Segmento)
            {

                this.Lista.ItemsSource = Conexoes.DBases.GetSegmentos(true);
                this.Propriedadesm.ShowAdvancedOptions = false;
                this.Propriedadesm.ShowSearchBox = false;
                this.Propriedadesm.ShowTitle = true;
                this.Propriedadesm.ShowSortOptions = false;
                this.Tipo = Tipologia.Segmento;
                this.Title = "Cadastrar / Editar Segmentos";
            }
            else if (Tipo == Tipologia.Grupo_De_Mercadoria)
            {

                this.Lista.ItemsSource = DBases.GetDbOrc().GetGrupos_De_Mercadoria(true);
                this.Propriedadesm.ShowAdvancedOptions = false;
                this.Propriedadesm.ShowSearchBox = false;
                this.Propriedadesm.ShowTitle = true;
                this.Propriedadesm.ShowSortOptions = false;
                this.Tipo = Tipologia.Grupo_De_Mercadoria;
                this.Title = "Cadastrar / Editar Grupos de Mercadoria";
                this.bt_apagar.Visibility = Visibility.Collapsed;
            }
            else if (Tipo == Tipologia.Grupo)
            {

                this.Lista.ItemsSource = DBases.GetDbOrc().GetGrupos(true);
                this.Propriedadesm.ShowAdvancedOptions = false;
                this.Propriedadesm.ShowSearchBox = false;
                this.Propriedadesm.ShowTitle = true;
                this.Propriedadesm.ShowSortOptions = false;
                this.Tipo = Tipologia.Grupo;
                this.Title = "Cadastrar / Editar Grupos";
                this.bt_apagar.Visibility = Visibility.Collapsed;
            }
            else if (Tipo == Tipologia.Local)
            {

                this.Lista.ItemsSource = DBases.GetDbOrc().GetLocais(true);
                this.Propriedadesm.ShowAdvancedOptions = false;
                this.Propriedadesm.ShowSearchBox = false;
                this.Propriedadesm.ShowTitle = true;
                this.Propriedadesm.ShowSortOptions = false;
                this.Tipo = Tipologia.Local;
                this.Title = "Cadastrar / Editar Locais";
                this.bt_apagar.Visibility = Visibility.Collapsed;
            }
            else if (Tipo == Tipologia.Tipo_Pintura)
            {

                this.Lista.ItemsSource = DBases.GetDbOrc().GetTipo_Pinturas(true);
                this.Propriedadesm.ShowAdvancedOptions = false;
                this.Propriedadesm.ShowSearchBox = false;
                this.Propriedadesm.ShowTitle = true;
                this.Propriedadesm.ShowSortOptions = false;
                this.Tipo = Tipologia.Tipo_Pintura;
                this.Title = "Cadastrar / Editar Tipos de Pintura";
                this.bt_apagar.Visibility = Visibility.Collapsed;
            }
            else if (Tipo == Tipologia.FERT)
            {

                this.Lista.ItemsSource = DBases.GetDbOrc().Get_PEP_FERT(true);
                this.Propriedadesm.ShowAdvancedOptions = false;
                this.Propriedadesm.ShowSearchBox = false;
                this.Propriedadesm.ShowTitle = true;
                this.Propriedadesm.ShowSortOptions = false;
                this.Tipo = Tipologia.FERT;
                this.Title = "Cadastrar / Editar FERT";
                this.bt_apagar.Visibility = Visibility.Collapsed;
            }
            else if (Tipo == Tipologia.Frente)
            {

                this.Lista.ItemsSource = DBases.GetDbOrc().GetFrentes(true);
                this.Propriedadesm.ShowAdvancedOptions = false;
                this.Propriedadesm.ShowSearchBox = false;
                this.Propriedadesm.ShowTitle = true;
                this.Propriedadesm.ShowSortOptions = false;
                this.Tipo = Tipologia.Frente;
                this.Title = "Cadastrar / Editar Frentes";
                this.bt_apagar.Visibility = Visibility.Collapsed;
            }
            else if (Tipo == Tipologia.Produto)
            {

                this.Lista.ItemsSource = DBases.GetDbOrc().GetProdutos_Clean();
                this.Propriedadesm.ShowAdvancedOptions = false;
                this.Propriedadesm.ShowSearchBox = false;
                this.Propriedadesm.ShowTitle = true;
                this.Propriedadesm.ShowSortOptions = false;
                this.Tipo = Tipologia.Produto;
                this.Title = "Cadastrar / Editar Listas Técnicas";
                this.bt_apagar.Visibility = Visibility.Collapsed;
                this.bt_novo.Visibility = Visibility.Collapsed;
            }
        }



        private void Selecionar(object sender, SelectionChangedEventArgs e)
        {
            if (Lista.SelectedItem != null)
            {
                Propriedadesm.SelectedObject = Lista.SelectedItem;
                this.Title = "Editar " + Lista.SelectedItem.ToString();
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (Propriedadesm.SelectedObject != null)
            {
                Salvar(Propriedadesm.SelectedObject);

            }
            Update();
        }
        public void Salvar(object Selecao)
        {
            if (Selecao == null)
            {
                return;
            }
            if (Selecao is Tratamento)
            {
                var t = Selecao as Tratamento;
                if (t.id <= 0 && DBases.GetDbOrc().GetTratamentos().Find(x => x.Descricao == t.Descricao) != null)
                {
                    Conexoes.Utilz.Alerta("Já existe um tratamento com esta descrição");
                    return;
                }
                if (t.Descricao == "")
                {
                    Conexoes.Utilz.Alerta("Não é possível criar/editar um tratamento com descrição em branco.");
                    return;
                }

                t.Salvar();
            }
            else if (Selecao is Segmento)
            {
                var t = Selecao as Segmento;
                DBases.GetDbOrc().CadastrarAtualizar(t);
            }
            else if (Selecao is Grupo_De_Mercadoria)
            {
                var t = Selecao as Grupo_De_Mercadoria;
                t.Salvar();
            }
            else if (Selecao is OrcamentoGrupo)
            {
                var t = Selecao as OrcamentoGrupo;
                t.Salvar();
            }
            else if (Selecao is OrcamentoLocal)
            {
                var t = Selecao as OrcamentoLocal;
                t.Salvar();
            }
            else if (Selecao is Tipo_Pintura)
            {
                var t = Selecao as Tipo_Pintura;
                t.Salvar();
            }
            else if (Selecao is PGO_PEP_FERT)
            {
                var t = Selecao as PGO_PEP_FERT;
                t.Salvar();
            }
            else if (Selecao is PGO_Frente)
            {
                var t = Selecao as PGO_Frente;
                t.Salvar();
            }
            else if (Selecao is Produto)
            {
                var t = Selecao as Produto;
                t.Salvar();
            }
        }


        private void Filtrar_txt(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(Lista.ItemsSource).Refresh();
        }

        private void Inicializar(object sender, RoutedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(Lista.ItemsSource).Filter = UserFilter;

        }
        private bool UserFilter(object item)
        {
            if (String.IsNullOrEmpty(Filtro.Text))
                return true;
            if (Filtro.Text == "Filtrar...")
                return true;
            return Conexoes.Utilz.Contem(item, Filtro.Text);
        }

        private void Arquivo_Novo(object sender, RoutedEventArgs e)
        {
            object tt = null;
            if (Tipo == Tipologia.Tratamento)
            {
                tt = new Tratamento();
                //Propriedadesm.SelectedObject = tt;

            }
            else if (Tipo == Tipologia.Segmento)
            {
                tt = new Segmento();
                //Propriedadesm.SelectedObject = tt;
            }
            else if (Tipo == Tipologia.Grupo_De_Mercadoria)
            {
                tt = new Grupo_De_Mercadoria();
                //Propriedadesm.SelectedObject = tt;
            }
            else if (Tipo == Tipologia.Grupo)
            {
                tt = new OrcamentoGrupo();
                //Propriedadesm.SelectedObject = tt;
            }
            else if (Tipo == Tipologia.Local)
            {
                tt = new OrcamentoLocal();
                //Propriedadesm.SelectedObject = tt;
            }
            else if (Tipo == Tipologia.Tipo_Pintura)
            {
                tt = new Tipo_Pintura();
                //Propriedadesm.SelectedObject = tt;
            }
            else if (Tipo == Tipologia.FERT)
            {
                tt = new PGO_PEP_FERT();
                //Propriedadesm.SelectedObject = tt;
            }
            else if (Tipo == Tipologia.Frente)
            {
                tt = new PGO_Frente();
                //Propriedadesm.SelectedObject = tt;
            }
            if (tt == null)
            {
                return;
            }

            bool confirmado = tt.Propriedades();
            if (confirmado)
            {
                Salvar(tt);
                Update();
            }

        }

        private void apagar_item(object sender, RoutedEventArgs e)
        {
            if (Conexoes.Utilz.Pergunta("Tem certeza que deseja apagar o item selecionado?"))
            {
                if (Propriedadesm.SelectedObject is Segmento)
                {
                    var ss = Propriedadesm.SelectedObject as Segmento;
                    DBases.GetDbOrc().Apagar(ss);
                }
                else if (Propriedadesm.SelectedObject is Tratamento)
                {
                    var ss = Propriedadesm.SelectedObject as Tratamento;
                    DBases.GetDbOrc().Apagar(ss);
                }
            }
        }
    }
}
