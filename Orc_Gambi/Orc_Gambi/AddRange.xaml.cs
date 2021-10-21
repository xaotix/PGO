using Conexoes.Orcamento;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Orc_Gambi
{
    /// <summary>
    /// Interaction logic for AddRange.xaml
    /// </summary>
    public partial class AddRange : Window
    {
        public Grupo Selecao { get; set; } = new Grupo();
        private OrcamentoObra Obra { get; set; } = new OrcamentoObra();

        public Range Range { get; set; }

        public Tipo_Carreta Carreta_User { get; set; }
        public AddRange(Grupo Selecao, OrcamentoObra Ob)
        {
            this.Selecao = Selecao;
            this.Obra = Ob;
            InitializeComponent();
            this.Tipo_Tratamento.ItemsSource = PGOVars.GetDbOrc().GetTratamentos();
            this.Tipo_Carreta.ItemsSource = PGOVars.GetDbOrc().GetTipo_Carreta();
            Carregar_Dados();
        }

        public AddRange(Grupo Selecao, Item_Arvore Item, OrcamentoObra Ob)
        {
            this.Obra = Ob;
            this.Selecao = Selecao;
            InitializeComponent();
            this.Tipo_Tratamento.ItemsSource = PGOVars.GetDbOrc().GetTratamentos();
            this.Tipo_Carreta.ItemsSource = PGOVars.GetDbOrc().GetTipo_Carreta();
            Carregar_Dados();
            this.Grupos_De_Mercadoria.ItemsSource = Selecao.Itens;
            this.Grupos_De_Mercadoria.SelectedItem = Item;
            this.Produto_Selecionado.SelectedItem = this.Produto_Selecionado.Items.Cast<Produto>().ToList().Find(x => x.id == Item.Produto_Padrao.id);
            this.Grupos_De_Mercadoria.IsEnabled = false;
            this.Produto_Selecionado.IsEnabled = true;
        }
        public AddRange(Range Range, OrcamentoObra Ob)
        {
            this.Range = Range;
            this.Title = "Duplicar " + Range.ToString();
            this.Obra = Ob;
            this.Selecao = Range.Grupo;
            InitializeComponent();
            this.Tipo_Tratamento.ItemsSource = PGOVars.GetDbOrc().GetTratamentos();
            this.Tipo_Carreta.ItemsSource = PGOVars.GetDbOrc().GetTipo_Carreta();
            Carregar_Dados();
            this.Grupos_De_Mercadoria.ItemsSource = PGOVars.GetDbOrc().GetGrupos_De_Mercadoria();
            this.Grupos_De_Mercadoria.SelectedItem = Range.Produto.Grupo_De_Mercadoria;


            this.Quantidade.Text = Range.Quantidade.ToString();
            this.Tipo_Tratamento.SelectedItem = Range.Tratamento;
            this.Tipo_Carreta.SelectedItem = Range.Tipo_De_Carreta;
            this.Grupos_De_Mercadoria.IsEnabled = false;
            this.Produto_Selecionado.IsEnabled = false;
            if ((this.Grupos_De_Mercadoria.SelectedItem as Grupo_De_Mercadoria).id == 0)
            {
                //quando é verba
                this.lbl_range.Content = "Descrição";
                this.Produto_Selecionado.IsEditable = true;
                this.Peso.IsEnabled = true;
                this.Produto_Selecionado.IsEnabled = true;
                this.Produto_Selecionado.Text = Range.Produto.produtos;
                this.Tipo_Tratamento.IsEnabled = false;
                this.Tipo_Carreta.IsEnabled = false;
                this.Peso.Text = Range.Peso.ToString();
            }
            else
            {
                this.Peso.IsEnabled = false;

                this.Produto_Selecionado.ItemsSource = Range.Produto.Grupo_De_Mercadoria.Produtos.FindAll(x => x.ativo);
                this.Produto_Selecionado.SelectedItem = this.Produto_Selecionado.Items.Cast<Produto>().ToList().Find(x => x.id == Range.Produto.id);
            }
            this.unidade.Content = Range.Unidade;
        }
        private void Carregar_Dados()
        {
            this.Grupos_De_Mercadoria.ItemsSource = null;
            this.Produto_Selecionado.ItemsSource = null;
            this.lblGrupo.Content = this.Selecao.nome;
            this.imgGrupo.Source = this.Selecao.Imagem;
            this.lblLocal.Content = this.Selecao.Local.nome;
            this.imglocal.Source = this.Selecao.Local.Imagem;
            this.lbl_predio.Content = this.Selecao.Local.Predio.nome;
            this.img_predio.Source = this.Selecao.Local.Predio.Imagem;
            this.Grupos_De_Mercadoria.ItemsSource = PGOVars.GetDbOrc().GetGrupos_De_Mercadoria().FindAll(x => x.descricao != "VERBA");
            if (this.Grupos_De_Mercadoria.Items.Count > 0)
            {
                this.Grupos_De_Mercadoria.SelectedIndex = 0;
            }

        }

        private void set_ranges(object sender, SelectionChangedEventArgs e)
        {
            this.Produto_Selecionado.ItemsSource = null;
            if (this.Grupos_De_Mercadoria.SelectedItem is Item_Arvore)
            {
                var t = this.Grupos_De_Mercadoria.SelectedItem as Item_Arvore;
                this.Produto_Selecionado.ItemsSource = t.Grupo_De_Mercadoria.Produtos.FindAll(x => x.ativo);
                if (this.Produto_Selecionado.Items.Count > 0)
                {
                    this.Produto_Selecionado.SelectedItem = this.Produto_Selecionado.Items.Cast<Produto>().ToList().Find(x => x.id == t.Produto_Padrao.id);

                }
            }
            else if (this.Grupos_De_Mercadoria.SelectedItem is Grupo_De_Mercadoria)
            {
                var t = this.Grupos_De_Mercadoria.SelectedItem as Grupo_De_Mercadoria;
                this.Produto_Selecionado.ItemsSource = t.Produtos.FindAll(x => x.ativo);
                if (this.Produto_Selecionado.Items.Count > 0)
                {
                    this.Produto_Selecionado.SelectedIndex = 0;
                }

            }
        }

        private void adicionar_item_arvore(object sender, RoutedEventArgs e)
        {

            var pr = this.Produto_Selecionado.SelectedItem as Produto;
            var tr = this.Tipo_Tratamento.SelectedItem as Tratamento;
            var tc = this.Tipo_Carreta.SelectedItem as Tipo_Carreta;
            var predio = this.Selecao.Local.Predio;

            double quantidade = Conexoes.Utilz.Double(this.Quantidade.Text);
            if (this.Grupos_De_Mercadoria.SelectedItem.ToString() != "VERBA")
            {
                if (pr == null)
                {
                    Conexoes.Utilz.Alerta("Selecione um Produto.", "Atenção!", MessageBoxImage.Error);
                    return;
                }
                if (tr == null)
                {
                    Conexoes.Utilz.Alerta("Selecione um Tratamento.", "Atenção!", MessageBoxImage.Error);
                    return;
                }
                if (tc == null)
                {
                    Conexoes.Utilz.Alerta("Selecione um tipo de carreta.", "Atenção!", MessageBoxImage.Error);
                    return;
                }
                if (predio == null)
                {
                    Conexoes.Utilz.Alerta("Prédio Inválido.", "Atenção!", MessageBoxImage.Error);
                    return;
                }
                if (quantidade <= 0)
                {
                    Conexoes.Utilz.Alerta("Digite uma quantidade.", "Atenção!", MessageBoxImage.Error);
                    return;
                }

                this.Range = new Range(this.Selecao.Local.Predio.Obra, pr, tr, tc, predio, this.Selecao, quantidade);
                this.Range.Salvar();
                if (this.Range.id == 0)
                {
                    Conexoes.Utilz.Alerta("Algo de errado aconteceu ao salvar. Contacte suporte");
                }
                else
                {
                    this.Selecao.Add(this.Range);
                }
            }
            else
            {
                if (this.Produto_Selecionado.Text == "")
                {
                    Conexoes.Utilz.Alerta("Digite uma descrição.");
                    return;
                }
                if (this.Produto_Selecionado.Text.Length > 40)
                {
                    Conexoes.Utilz.Alerta("Descrição deve conter no máximo 40 caracteres.");
                    return;
                }
                if (quantidade <= 0)
                {
                    Conexoes.Utilz.Alerta("Digite um valor.");
                    return;
                }
                this.Range = Obra.AddVerba(this.Produto_Selecionado.Text, quantidade, Conexoes.Utilz.Double(Peso.Text), predio, this.Selecao);

                //Range p = new Range(this.Selecao.Local.Predio.Obra,this.Produto_Selecionado.Text, quantidade, Conexoes.Utilz.Double(Peso.Text),predio,this.Selecao);
                //p.Salvar();
                if (this.Range.id == 0)
                {
                    Conexoes.Utilz.Alerta("Algo de errado aconteceu ao salvar. Contacte suporte");
                }
                else
                {
                    this.Selecao.Add(this.Range);
                }
            }


            this.DialogResult = true;
        }

        private void set_carreta(object sender, SelectionChangedEventArgs e)
        {

            if (this.Produto_Selecionado.SelectedItem is Produto)
            {
                var t = this.Produto_Selecionado.SelectedItem as Produto;
                this.Carreta_User = t.Grupo_Logistico.Tipo_Carreta;
                if (this.Tipo_Carreta.Items.Count > 0)
                {
                    this.Tipo_Carreta.SelectedItem = t.Grupo_Logistico.Tipo_Carreta;
                }

                if (this.Tipo_Tratamento.Items.Count > 0)
                {
                    if (t.pintura > 0)
                    {

                        this.Tipo_Tratamento.SelectedItem = this.Obra.GetTratamento();
                        this.Tipo_Tratamento.IsEnabled = true;

                    }
                    else
                    {
                        this.Tipo_Tratamento.SelectedItem = PGOVars.GetDbOrc().GetTratamentos().Find(x => x.Descricao == "0");
                        this.Tipo_Tratamento.IsEnabled = false;
                    }
                }

                this.unidade.Content = t.unidade;
                GetPeso();
            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Quantidade.Focus();
        }

        private void cancelar(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void set_local(object sender, RoutedEventArgs e)
        {

        }

        private void set_grupo(object sender, RoutedEventArgs e)
        {

        }

        private void Quantidade_TextChanged(object sender, TextChangedEventArgs e)
        {
            GetPeso();

        }

        private void GetPeso()
        {
            if (Grupos_De_Mercadoria.Text == "VERBA") { return; }
            var p = Produto_Selecionado.SelectedItem as Produto;
            Peso.Text = Math.Round(p.peso_unitario * Conexoes.Utilz.Double(this.Quantidade.Text), 2).ToString();
        }
    }
}
