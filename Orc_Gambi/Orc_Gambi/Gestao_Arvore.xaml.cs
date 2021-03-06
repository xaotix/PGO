using Conexoes;
using DLM.orc;
using DLM.vars;
using FirstFloor.ModernUI.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PGO
{
    /// <summary>
    /// Interaction logic for Gestao_Arvore.xaml
    /// </summary>
    public partial class Gestao_Arvore : ModernWindow
    {
        public Gestao_Arvore()
        {
            InitializeComponent();
            Templates = DBases.GetDbOrc().GetTemplates(true);
            if (Templates.Count > 0)
            {
                this.Templates_Selecao.ItemsSource = Templates;

                this.Templates_Selecao.SelectedIndex = 0;
            }

            UpdateAll();
        }
        public Template Selecao { get; set; } = new Template();
        private void UpdateAll()
        {
            if (Selecao != null)
            {
                Selecao.Update();
                Locais = Selecao.Arvore;
                this.Lista.ItemsSource = null;
                this.Lista.ItemsSource = Locais;
            }
            else
            {
                this.Lista.ItemsSource = null;
                this.Locais = new List<OrcamentoLocal>();
            }


        }


        public List<OrcamentoLocal> Locais { get; set; } = new List<OrcamentoLocal>();
        public List<Template> Templates { get; set; } = new List<Template>();

        private void editar_local(object sender, RoutedEventArgs e)
        {

        }




        private void adicionar_grupo(object sender, RoutedEventArgs e)
        {
            DLM.orc.OrcamentoLocal sel = ((FrameworkElement)sender).DataContext as DLM.orc.OrcamentoLocal;
            if (sel != null)
            {
                OrcamentoItem_Arvore TT = new OrcamentoItem_Arvore( sel, Selecao.id);
                Adicionar_Editar_Item mm = new Adicionar_Editar_Item(TT);
                mm.ShowDialog();
                if (mm.DialogResult == true)
                {
                    UpdateAll();
                }
            }

        }

        private void adicionar_grupo_mercadoria(object sender, RoutedEventArgs e)
        {
            DLM.orc.OrcamentoGrupo sel = ((FrameworkElement)sender).DataContext as DLM.orc.OrcamentoGrupo;
            if (sel != null)
            {
                OrcamentoItem_Arvore TT = new OrcamentoItem_Arvore(sel.Local, sel, Selecao.id);
                Adicionar_Editar_Item mm = new Adicionar_Editar_Item(TT);
                mm.ShowDialog();
                if (mm.DialogResult == true)
                {
                    UpdateAll();
                }
            }
        }

        private void editar_produto_padrao(object sender, RoutedEventArgs e)
        {
            DLM.orc.OrcamentoItem_Arvore sel = ((FrameworkElement)sender).DataContext as DLM.orc.OrcamentoItem_Arvore;
            var prod = Conexoes.Utilz.Selecao.SelecionarObjeto(sel.Grupo_De_Mercadoria.Produtos.FindAll(x => x.ativo), null) as Produto;
            if (prod != null)
            {
                sel.Produto_Padrao = prod;
            }

        }

        private void remover_item_arvore(object sender, RoutedEventArgs e)
        {
            DLM.orc.OrcamentoItem_Arvore sel = ((FrameworkElement)sender).DataContext as DLM.orc.OrcamentoItem_Arvore;
            if (sel != null)
            {
                if (Utilz.Pergunta("Tem certeza que deseja remover o item " + sel + " ?"))
                {
                    DBases.GetDbOrc().Apagar(sel);
                    UpdateAll();
                }
            }
        }

        private void copiar_grupo_mercadoria(object sender, RoutedEventArgs e)
        {
            DLM.orc.OrcamentoGrupo sel = ((FrameworkElement)sender).DataContext as DLM.orc.OrcamentoGrupo;
            if (sel != null)
            {
                var loc = Conexoes.Utilz.Selecao.SelecionarObjeto(Locais, null) as OrcamentoLocal;
                if (loc != null)
                {
                    var sels = Conexoes.Utilz.Selecao.SelecionarObjeto(loc.Grupos, null) as OrcamentoGrupo;

                    if (sels != null)
                    {
                        if (Utilz.Pergunta("Deseja copiar os grupos de mercadoria do grupo " + sel.Local + "/" + sel + " para o grupo " + loc + "/" + sels + "?"))
                        {
                            foreach (var t in sel.Grupos_De_Mercadoria.Select(x => x.Item_Arvore))
                            {
                                OrcamentoItem_Arvore tnew = new OrcamentoItem_Arvore(t);
                                tnew.Grupo = sels;
                                tnew.Local = loc;
                                tnew.Salvar();
                            }
                            UpdateAll();
                        }
                    }
                }

            }
        }

        private void copiar_local(object sender, RoutedEventArgs e)
        {
            DLM.orc.OrcamentoLocal sel = ((FrameworkElement)sender).DataContext as DLM.orc.OrcamentoLocal;
            if (sel != null)
            {

                var sels = Conexoes.Utilz.Selecao.SelecionarObjeto(Locais, null) as OrcamentoLocal;
                if (sels != null)
                {
                    if (Utilz.Pergunta("Deseja copiar todos os grupos e grupos de mercadoria do local " + sel + " para o local " + sels + "?"))
                    {
                        foreach (var t in sel.Grupos.SelectMany(x => x.Itens))
                        {
                            OrcamentoItem_Arvore item = new OrcamentoItem_Arvore(t);
                            item.Local = sels;
                            item.Salvar();
                        }
                        UpdateAll();
                    }
                }
            }
        }

        private void apagar_grupo(object sender, RoutedEventArgs e)
        {
            DLM.orc.OrcamentoGrupo sel = ((FrameworkElement)sender).DataContext as DLM.orc.OrcamentoGrupo;
            if (sel != null)
            {

                if (Utilz.Pergunta("Deseja apagar o grupo " + sel + " e todos os sub-itens?"))
                {
                    foreach (var t in sel.Itens)
                    {
                        DBases.GetDbOrc().Apagar(t);
                    }
                    UpdateAll();
                }
            }
        }

        private void apagar_local(object sender, RoutedEventArgs e)
        {
            DLM.orc.OrcamentoLocal sel = ((FrameworkElement)sender).DataContext as DLM.orc.OrcamentoLocal;
            if (sel != null)
            {

                if (Utilz.Pergunta("Deseja apagar o local [" + sel + "] e todos os sub-itens?"))
                {
                    foreach (var t in sel.Grupos.SelectMany(x => x.Itens))
                    {
                        DBases.GetDbOrc().Apagar(t);
                    }
                    UpdateAll();
                }
            }
        }

        private void adicionar_local(object sender, RoutedEventArgs e)
        {
            var ls = DBases.GetDbOrc().GetLocais().FindAll(x => Locais.Find(y => y.id == x.id) == null);
            if (ls.Count == 0) { Conexoes.Utilz.Alerta("Já foram adicionados todos os locais cadastrados."); return; }
            var sel = Conexoes.Utilz.Selecao.SelecionarObjeto(ls, null) as OrcamentoLocal;
            if (sel != null)
            {
                OrcamentoItem_Arvore TT = new OrcamentoItem_Arvore( sel, Selecao.id);
                Adicionar_Editar_Item mm = new Adicionar_Editar_Item(TT);
                mm.ShowDialog();
                if (mm.DialogResult == true)
                {
                    UpdateAll();
                }
            }
        }

        private void move_local(object sender, RoutedEventArgs e)
        {
            DLM.orc.OrcamentoLocal sel = ((FrameworkElement)sender).DataContext as DLM.orc.OrcamentoLocal;
            if (sel != null)
            {

                var sels = Conexoes.Utilz.Selecao.SelecionarObjeto(Locais.FindAll(x => x.id != sel.id), null) as OrcamentoLocal;
                if (sels != null)
                {
                    if (Utilz.Pergunta("Deseja mover os grupos do local " + sel + " para o local " + sels + "?"))
                    {
                        foreach (var t in sel.Grupos.SelectMany(x => x.Grupos_De_Mercadoria).Select(x => x.Item_Arvore))
                        {
                            //Item_Arvore item = new Item_Arvore(t.db, t);
                            t.Local = sels;
                            t.Salvar();
                        }
                        UpdateAll();
                    }
                }
            }
        }

        private void mover_grupo_de_mercadoria(object sender, RoutedEventArgs e)
        {
            DLM.orc.OrcamentoGrupo sel = ((FrameworkElement)sender).DataContext as DLM.orc.OrcamentoGrupo;
            if (sel != null)
            {
                var loc = Conexoes.Utilz.Selecao.SelecionarObjeto(Locais.FindAll(x => x.id != sel.Local.id), null) as OrcamentoLocal;
                if (loc != null)
                {
                    var sels = Conexoes.Utilz.Selecao.SelecionarObjeto(loc.Grupos, null) as OrcamentoGrupo;

                    if (sels != null)
                    {
                        if (Utilz.Pergunta("Deseja mover os grupos de mercadoria do grupo " + sel.Local + "/" + sel + " para o grupo " + loc + "/" + sels + "?"))
                        {
                            foreach (var t in sel.Itens)
                            {

                                t.Grupo = sels;
                                t.Local = loc;
                                t.Salvar();
                            }
                            UpdateAll();
                        }
                    }
                }

            }
        }

        private void editar_grupo_de_mercadoria(object sender, RoutedEventArgs e)
        {
            DLM.orc.OrcamentoItem_Arvore sel = ((FrameworkElement)sender).DataContext as DLM.orc.OrcamentoItem_Arvore;
            if (sel != null)
            {

                var grp = Conexoes.Utilz.Selecao.SelecionarObjeto(DBases.GetDbOrc().GetGrupos_De_Mercadoria().FindAll(x => x.id != sel.Grupo_De_Mercadoria.id), null) as Grupo_De_Mercadoria;
                if (grp != null)
                {
                    var prod_padrao = Conexoes.Utilz.Selecao.SelecionarObjeto(grp.Produtos.FindAll(x => x.ativo), null) as Produto;
                    if (prod_padrao != null)
                    {
                        if (Utilz.Pergunta("Alterar o grupo " + sel + " para " + grp + "?"))
                        {
                            sel.Grupo_De_Mercadoria = grp;
                            sel.Produto_Padrao = prod_padrao;
                            sel.Salvar();
                            //UpdateAll();
                        }
                    }
                }

            }
        }

        private void editar_multiplicador(object sender, RoutedEventArgs e)
        {
            DLM.orc.OrcamentoItem_Arvore sel = ((FrameworkElement)sender).DataContext as DLM.orc.OrcamentoItem_Arvore;

            double valor = Utilz.Double(Utilz.Prompt("Digite", "", sel.multiplicador.ToString()));
            sel.setmultiplicador(valor);

        }

        private void editar_observacoes(object sender, RoutedEventArgs e)
        {
            DLM.orc.OrcamentoItem_Arvore sel = ((FrameworkElement)sender).DataContext as DLM.orc.OrcamentoItem_Arvore;

            string valor = Utilz.Prompt("Digite", "", sel.observacoes.ToString());
            sel.setobservacoes(valor);
        }

        private void selecionar(object sender, SelectionChangedEventArgs e)
        {
            Selecao = Templates_Selecao.SelectedItem as Template;
            UpdateAll();
        }

        private void adicionar_template(object sender, RoutedEventArgs e)
        {
            Template pp = new Template();
            Utilz.Propriedades(pp, true);
            if (Utilz.Pergunta("Deseja criar o Template " + pp + "?"))
            {
                if (Utilz.Pergunta("Quer copiar os itens de outro template para ele?"))
                {
                    var sel = Conexoes.Utilz.Selecao.SelecionarObjeto(DBases.GetDbOrc().GetTemplates(), null) as Template;
                    if (sel != null)
                    {
                        pp.Clonar(sel);
                    }

                }
                pp.Salvar();

                UpdateArvore();
            }
        }

        private void UpdateArvore()
        {
            this.Templates_Selecao.ItemsSource = null;
            this.Templates_Selecao.ItemsSource = DBases.GetDbOrc().GetTemplates(true);
            if (this.Templates_Selecao.Items.Count > 0)
            {
                this.Templates_Selecao.SelectedIndex = this.Templates_Selecao.Items.Count - 1;
            }
        }

        private void editar_propriedades(object sender, RoutedEventArgs e)
        {
            if (Selecao != null)
            {
                Conexoes.Utilz.Propriedades(Selecao, true);
                Selecao.Salvar();
                UpdateArvore();
            }
        }

        private void subir_item_arvore(object sender, RoutedEventArgs e)
        {
            DLM.orc.OrcamentoItem_Arvore sel = ((FrameworkElement)sender).DataContext as DLM.orc.OrcamentoItem_Arvore;
            if (sel != null)
            {
                sel.DiminuiOrdem();
            }
        }

        private void descer_item_arvore(object sender, RoutedEventArgs e)
        {
            DLM.orc.OrcamentoItem_Arvore sel = ((FrameworkElement)sender).DataContext as DLM.orc.OrcamentoItem_Arvore;
            if (sel != null)
            {
                sel.SomaOrdem();
            }
        }
    }
}

