using Conexoes;
using DLM.orc;
using DLM.vars;
using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PGO
{
    /// <summary>
    /// Interaction logic for Adicionar_Editar_Item.xaml
    /// </summary>
    public partial class Adicionar_Editar_Item : ModernWindow
    {
        private OrcamentoItem_Arvore novo { get; set; } = new OrcamentoItem_Arvore(-1);
        public Adicionar_Editar_Item(OrcamentoItem_Arvore item_Arvore)
        {
            InitializeComponent();
            this.novo = item_Arvore;
            this.DataContext = this;
            UpdateAll();
        }
        private void getproduto()
        {
            var t = Grupos_De_Mercadoria.SelectedItem;
            if (t is Grupo_De_Mercadoria)
            {
                this.Produto_Padrao.ItemsSource = null;
                this.Produto_Padrao.ItemsSource = DBases.GetDbOrc().GetProdutos(t as Grupo_De_Mercadoria);
                if (this.Produto_Padrao.Items.Count > 0)
                {
                    this.Produto_Padrao.SelectedItem = this.Produto_Padrao.Items.Cast<Produto>().ToList().Find(x => x.id == novo.Produto_Padrao.id); ;
                }
                if (this.Produto_Padrao.Items.Count > 0 && this.Produto_Padrao.SelectedItem == null)
                {
                    this.Produto_Padrao.SelectedItem = this.Produto_Padrao.Items[0];
                }

            }
        }
        private void UpdateAll()
        {
            this.Grupos.ItemsSource = DBases.GetDbOrc().GetGrupos();
            this.Locais.ItemsSource = DBases.GetDbOrc().GetLocais();
            this.Grupos_De_Mercadoria.ItemsSource = DBases.GetDbOrc().GetGrupos_De_Mercadoria().FindAll(x => x.descricao != "VERBA");
            this.Grupos.SelectedValue = this.Grupos.Items.Cast<OrcamentoGrupo>().ToList().Find(x => x.id == novo.Grupo.id);
            this.Locais.SelectedItem = this.Locais.Items.Cast<OrcamentoLocal>().ToList().Find(x => x.id == novo.Local.id);
            this.Grupos_De_Mercadoria.SelectedItem = novo.Grupo_De_Mercadoria;
            try
            {
                getproduto();
            }
            catch (Exception)
            {

            }
            if (this.Grupos.SelectedItem == null && this.Grupos.Items.Count > 0)
            {
                this.Grupos.SelectedItem = this.Grupos.Items[0];
            }
            if (this.Locais.SelectedItem == null && this.Locais.Items.Count > 0)
            {
                this.Locais.SelectedItem = this.Locais.Items[0];
            }
            if (this.Grupos_De_Mercadoria.SelectedItem == null && this.Grupos_De_Mercadoria.Items.Count > 0)
            {
                this.Grupos_De_Mercadoria.SelectedItem = this.Grupos_De_Mercadoria.Items[0];
            }
            if (this.Produto_Padrao.SelectedItem == null && this.Produto_Padrao.Items.Count > 0)
            {
                this.Produto_Padrao.SelectedItem = this.Produto_Padrao.Items[0];
            }
        }

        private void adicionar_item_arvore(object sender, RoutedEventArgs e)
        {
            var local = Locais.SelectedItem as OrcamentoLocal;
            var grupo = Grupos.SelectedItem as OrcamentoGrupo;
            var grupo_mercadoria = Grupos_De_Mercadoria.SelectedItem as Grupo_De_Mercadoria;
            var produto_padrao = Produto_Padrao.SelectedItem as Produto;
            double multi = Utilz.Double(multilpicador.Text);
            bool carregar = (bool)Carregar_Padrao.IsChecked;
            bool fixo = (bool)Valor_Fixo.IsChecked;
            bool at = (bool)Ativo.IsChecked;
            string obs = observacoes.Text;
            if (local == null)
            {
                Conexoes.Utilz.Alerta("Selecione um Local");
                return;
            }
            if (grupo == null)
            {
                Conexoes.Utilz.Alerta("Selecione um grupo");
                return;
            }
            if (grupo_mercadoria == null)
            {
                Conexoes.Utilz.Alerta("Selecione um grupo de mercadoria");
                return;
            }
            if (produto_padrao == null)
            {
                Conexoes.Utilz.Alerta("Selecione um produto padrão");
                return;
            }

            novo.Local = local;
            novo.Grupo = grupo;
            novo.Grupo_De_Mercadoria = grupo_mercadoria;
            novo.Produto_Padrao = produto_padrao;
            novo.setmultiplicador(multi);
            novo.carregar_padrao = carregar;
            novo.valor_fixo = fixo;
            novo.ativo = at;
            novo.setobservacoes(obs);
            novo.Salvar();
            this.DialogResult = true;
        }

        private void set_ranges(object sender, SelectionChangedEventArgs e)
        {
            getproduto();
        }
    }
}
