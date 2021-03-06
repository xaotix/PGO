using Conexoes;
using DLM.encoder;
using DLM.orc;
using DLM.vars;
using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace PGO
{
    /// <summary>
    /// Interaction logic for JanelaObra.xaml
    /// </summary>
    public partial class JanelaObra : ModernWindow
    {
        public Visibility Menus_Orcamento { get; set; } = Visibility.Visible;
        public Visibility Menus_Editar { get; set; } = Visibility.Visible;
        public bool Menus_Editar_Bool { get; set; } = true;
        public JanelaObra(PGO_Obra Ob)
        {
            this.Obra = Ob;
            InitializeComponent();

         

            if (!DBases.GetUserAtual().orcamento_ver_obras)
            {
                Menus_Orcamento = Visibility.Collapsed;
            }


            this.SetIcones();


            if (this.Obra.Bloqueado)
            {
                Menus_Editar = Visibility.Collapsed;
                Menus_Editar_Bool = false;

            }
            this.DataContext = this;

            this.Update();


            if (Ob.Nacional)
            {
                this.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
            }
            else
            {
                this.Language = XmlLanguage.GetLanguage(new CultureInfo("en-US").IetfLanguageTag);
            }
            this.DataContext = this;

            Input.FontSize = 11;
        }

        private void GetArvore()
        {
            Arvore.Items.Clear();
            var raiz = Conexoes.Utilz.Forms.AddTreeview(this.Obra.Contrato, this.Obra);
            raiz.SetBinding(TreeViewItem.ToolTipProperty, Utilz.GetBinding("ToolTip", raiz.Tag));

            foreach (var predio in this.Obra.GetPredios(true))
            {
                string nomepredio = predio.numero + " - " + predio.nome + " [" + predio.Area_Predio.ToString() + " m²]";
                var treeview_item = Conexoes.Utilz.Forms.AddTreeview(nomepredio, nomepredio, predio.Imagem, predio, "");
                treeview_item.ItemsSource = null;
                treeview_item.SetBinding(TreeViewItem.ToolTipProperty, Utilz.GetBinding("ToolTip", treeview_item.Tag));
                foreach (var local in predio.Locais)
                {
                    var treeview_item2 = Conexoes.Utilz.Forms.AddTreeview(local.ToString(), local.nome, local.Imagem, local, "");
                    treeview_item2.ItemsSource = null;
                    treeview_item2.SetBinding(TreeViewItem.ToolTipProperty, Utilz.GetBinding("ToolTip", treeview_item2.Tag));

                    treeview_item.Items.Add(treeview_item2);

                    foreach (var grupo in local.Grupos)
                    {

                        var treeview_item3 = Conexoes.Utilz.Forms.AddTreeview(grupo.ToString(), grupo.nome, grupo.Imagem, grupo, "");
                        treeview_item3.SetBinding(TreeViewItem.ToolTipProperty, Utilz.GetBinding("ToolTip", treeview_item3.Tag));

                        treeview_item2.Items.Add(treeview_item3);
                        treeview_item3.ItemsSource = null;
                        treeview_item3.Items.Clear();
                    }
                }
                raiz.ItemsSource = null;
                raiz.Items.Add(treeview_item);
            }
            Arvore.Items.Add(raiz);
            (Arvore.Items[0] as TreeViewItem).IsExpanded = true;
        }


        private void Update()
        {
            this.Obra.Recarregar();

            this.Tratamentos = this.Obra.GetTratamentos();
            GetArvore();
            Atualizar_Lista();
        }
        private void Selecionar(bool bb)
        {
            //foreach (Predio t in this.Predios.Items)
            //{
            //    t.Selecionado = bb;
            //}
        }

        private List<string> Grupos_Selecionados { get; set; } = new List<string>();
        private List<Range> RangesFiltro { get; set; } = new List<Range>();
        public List<Tratamento> Tratamentos { get; set; } = new List<Tratamento>();
        public PGO_Obra Obra { get; set; } = new PGO_Obra();
        public List<Range> RangesSelecionados { get; set; } = new List<Range>();
        private List<string> Locais { get; set; } = new List<string>();

        public double ValorTotal { get; set; } = 0;


        private void salva_contrato(object sender, RoutedEventArgs e)
        {
            string novo = Utilz.Prompt("Digite o contrato", "", this.Obra.Contrato_SAP);
            if (novo.Length == 6)
            {
                this.Obra.SetContrato_SAP(novo);

            }
            else
            {
                Conexoes.Utilz.Alerta("Valor inválido. O contrato deve conter 6 caracteres", "Abortado", MessageBoxImage.Error);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Obra.GetCsv();
        }

        private void FazerAnaliseClick(object sender, RoutedEventArgs e)
        {
            this.Obra.GerarProposta(true);
        }

        private void gerar_lista_pecas(object sender, RoutedEventArgs e)
        {
            List<Report> Reports = new List<Report>();
            var sem_tratamento = this.Tratamentos.FindAll(x => !x.Tipo.descricao.Contains("GALV") && x.Codigo == "");
            if (sem_tratamento.Count > 0)
            {
                Reports.AddRange(sem_tratamento.Select(x => new Report("Falta definir o esquema de pintura.", x.Descricao)).ToList());
            }
            if (this.Obra.Contrato_SAP == "")
            {
                Reports.Add(new Report("Faltam dados", "Falta preencher o contrato sap"));
            }

            if (this.Tratamentos.FindAll(x => x.Tipo.descricao == "").Count > 0)
            {
                Reports.Add(new Report("Faltam dados", "Há esquemas de pintura faltando definição de tratamento. Preencha-os antes de continuar."));
            }

            if (this.Tratamentos.FindAll(x => !x.Tipo.descricao.Contains("GALV") && x.Codigo == "").Count > 0)
            {
                Reports.Add(new Report("Faltam dados", "HHá esquemas de pintura faltando definição de código. Preencha-os antes de continuar."));

            }

            if (Reports.Count > 0)
            {
                Conexoes.Utilz.ShowReports(Reports);
                return;
            }


 
        }

  

        private void seleciona_tudo(object sender, RoutedEventArgs e)
        {
            Selecionar(true);
        }

        private void limpar_selecao(object sender, RoutedEventArgs e)
        {
            Selecionar(false);

        }

        private void ver_props(object sender, RoutedEventArgs e)
        {
            List<Range> Ranges = ListaRanges.Selecoes<Range>();
            if (Ranges.Count > 0)
            {
                foreach (var range in Ranges)
                {
                    Utilz.Propriedades(range);
                }
            }
        }



        private void define_fert(object sender, RoutedEventArgs e)
        {
            List<Range> Ranges = ListaRanges.Selecoes<Range>();
            if (Ranges.Count > 0)
            {
                SetFert(Ranges);
            }
        }

        private void SetFert(List<Range> Ranges)
        {
            var mts = Ranges.Select(x => x.WERK).Distinct().ToList();

            var sel = Conexoes.Utilz.Selecao.SelecionarObjeto(DBases.GetDbOrc().GetFerts(this.Obra.GetSegmento().COD, null), null, "Selecione");
            if (sel != null)
            {
                Conexoes.ControleWait w = Conexoes.Utilz.Wait(Ranges.Count, "Atualizando...");
                foreach (var s in Ranges)
                {
                    s.setFert_User(sel.FERT, sel.WERKS);
                    w.somaProgresso();
                }
                w.Close();
            }
        }

        private void retorna_fert(object sender, RoutedEventArgs e)
        {
            RetornaFert(ListaRanges.Selecoes<Range>());
        }

        private static void RetornaFert(List<Range> Ranges)
        {
            if (Ranges.Count > 0)
            {
                if (Conexoes.Utilz.Pergunta("Tem certeza que deseja alterar os itens selecionados para o valor default da LT?"))
                {
                    Conexoes.ControleWait w = Conexoes.Utilz.Wait(Ranges.Count, "Atualizando...");
                    foreach (var s in Ranges)
                    {
                        s.setFert_User("", "");
                        w.somaProgresso();
                    }
                    w.Close();
                }
            }
        }

        private void Atribuir_material_range(object sender, RoutedEventArgs e)
        {
            if (Obra.Bloqueado)
            {
                Conexoes.Utilz.Alerta("Obra está bloqueada para edições", "Obra Bloqueada", MessageBoxImage.Error);
                return;
            }
            var ranges = ListaRanges.Selecoes<Range>().FindAll(x => x.Verba);

            if (ranges.Count > 0)
            {
                var rm = Conexoes.DBases.GetBancoRM().CriaRetornaSAP();
                if (rm != null)
                {
                retentar:
                    double qtd = Utilz.Double(Utilz.Prompt("Digite a quantidade", rm.ToString(), "", false, "", false, 14));
                    if (!rm.MultiploOk(qtd))
                    {
                        if (Conexoes.Utilz.Pergunta("O valor digitado não é múltiplo da quantidade do material [" + rm.Multiplo + " ]" + " Quer tentar novamente? Se clicar em não, o processo será cancelado."))
                        {
                            goto retentar;
                        }
                        else
                        {
                            return;
                        }
                    }
                    if (qtd <= 0) { return; }
                    foreach (var range in ranges)
                    {
                        range.setMaterial_User(rm.id_db, qtd);
                    }
                }
            }
        }

        private void exportar_excel(object sender, RoutedEventArgs e)
        {
            ListaRanges.Exportar();

        }


        private void ModernWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var tvi = Arvore.ItemContainerGenerator.ContainerFromItem(Arvore.Items[0])
                      as TreeViewItem;

            if (tvi != null)
            {
                tvi.IsSelected = true;
            }

            //Utilz.LerVars(visualizar, Vars.ArqASetupUser, "Orcamento.JanelaObra");
        }


        private void gerar_materiais_manual(object sender, RoutedEventArgs e)
        {
            if (!Utilz.Acesso(DBases.GetUserAtual().orcamento_consolidar_obra))
            {
                return;
            }
            List<Report> Reports_pre = new List<Report>();
            var sem_tratamento = this.Tratamentos.FindAll(x => !x.Tipo.descricao.Contains("GALV") && x.Codigo == "");
            if (sem_tratamento.Count > 0)
            {
                Reports_pre.AddRange(sem_tratamento.Select(x => new Report("Falta definir o esquema de pintura.", x.Descricao)).ToList());
            }
            if (this.Obra.Contrato_SAP == "")
            {
                Reports_pre.Add(new Report("Faltam dados", "Falta preencher o contrato sap"));
            }

            if (this.Tratamentos.FindAll(x => x.Tipo.descricao == "").Count > 0)
            {
                Reports_pre.Add(new Report("Faltam dados", "Há esquemas de pintura faltando definição de tratamento. Preencha-os antes de continuar."));
            }


            JanelaSelecionarRanges mm = new JanelaSelecionarRanges(this.Obra.GetPredios().ToList(), true);
            mm.ShowDialog();
            if (!(bool)mm.DialogResult)
            {
                return;
            }


            var t = this.Obra.GetRanges().ToList().FindAll(x => x.Selecionado).FindAll(x => x.Material_User > 0);

            var t2 = this.Obra.GetRanges().ToList().FindAll(x => x.Verba).FindAll(x => x.Material_User <= 0);

            var t3 = this.Obra.GetRanges().ToList().FindAll(x => x.Verba).FindAll(x => x.De_Para.FERT == "00");
            var t4 = this.Obra.GetRanges().ToList().FindAll(x => x.Verba).FindAll(x => x.PesoTotal <= 0);
            Reports_pre.AddRange(t.FindAll(x => x.Material_User_Qtd <= 0).Select(y => new Report(y.ToString(), "Setado material, porém quantidade está zerada.", TipoReport.Crítico)));
            Reports_pre.AddRange(t.FindAll(x => x.GetMaterialUser() == null).Select(y => new Report(y.ToString(), "Material setado não encontrado.", TipoReport.Crítico)));
            Reports_pre.AddRange(t2.Select(y => new Report(y.ToString(), "Range sem material, necessita definição de verba.", TipoReport.Crítico)));

            Reports_pre.AddRange(t3.Select(y => new Report(y.ToString(), "Range sem FERT, necessita definição de FERT.", TipoReport.Crítico)));

            //Reports_pre.AddRange(t4.Select(y => new Report(y.ToString(), "Verba sem definição de peso.", TipoReport.Crítico)));


            if (this.Tratamentos.FindAll(x => !x.Tipo.descricao.Contains("GALV") && x.Codigo == "").Count > 0)
            {
                Reports_pre.Add(new Report("Faltam dados", "Há esquemas de pintura faltando definição de código. Preencha-os antes de continuar."));

            }

            if (Reports_pre.Count > 0)
            {
                Conexoes.Utilz.ShowReports(Reports_pre);
                return;
            }


            //PRECISO IMPLEMENTAR ISSO NOS PACOTES
            List<Range> pp = new List<Range>();
            pp.AddRange(RangesSelecionados);

            ExplorerPLM.Utilidades.MostrarLista(this.Obra, System.Windows.Forms.Application.ProductName + " - [" + this.Obra.ToString() + "]");
        }

        private void salvar_propriedades_pacote(object sender, RoutedEventArgs e)
        {
        }

        private void get_material_range(object sender, RoutedEventArgs e)
        {
            var ranges = ListaRanges.Selecoes<Range>().FindAll(x => x.Verba);
            if (ranges.Count > 0)
            {
                foreach (var range in ranges)
                {
                    var t = range.GetMaterialUser();
                    if (t != null)
                    {
                        Utilz.Propriedades(t);
                    }
                }
            }
        }

        private void propriedades_obra(object sender, RoutedEventArgs e)
        {
            //Utilz.Propriedades(this.Obra);
            NovaObra MenuNovaObra = new NovaObra(this.Obra);
            MenuNovaObra.Closed += Mm_Closed;
            MenuNovaObra.Show();
        }

        private void Mm_Closed(object sender, EventArgs e)
        {
            Update();
        }



        private void ver_acessos(object sender, RoutedEventArgs e)
        {
            Utilz.VerAcessos(DBases.GetUserAtual());
        }

        private void atribuir_quantidade_mp(object sender, RoutedEventArgs e)
        {
            var ranges = ListaRanges.Selecoes<Range>().FindAll(x => x.Verba);

            if (ranges.Count > 0)
            {
                double qtd = Utilz.Double(Utilz.Prompt("Digite a quantidade", "Digite a quantidade", ranges[0].Material_User_Qtd.ToString(), false, "", false, 14));
                foreach (var range in ranges)
                {
                    range.setMaterial_User_Qtd(qtd);
                }
            }
        }

        private void remover_mp(object sender, RoutedEventArgs e)
        {
            var ranges = ListaRanges.Selecoes<Range>().FindAll(x => x.Verba);

            if (ranges.Count > 0)
            {
                foreach (var range in ranges)
                {
                    range.setMaterial_User(-1, 0);
                }
            }
        }

        private void EditarRota(object sender, RoutedEventArgs e)
        {
            EditarRota();
        }

        private void EditarRota()
        {
            if (Obra.Bloqueado)
            {
                Conexoes.Utilz.Alerta("Obra está bloqueada para edições", "Obra Bloqueada", MessageBoxImage.Error);
                return;
            }
            this.Visibility = Visibility.Collapsed;
            ExplorerPLM.Menus.Fretes mm = new ExplorerPLM.Menus.Fretes(this.Obra);
            mm.Show();

            mm.Closed += Mm_Closed1;
        }

        private void Mm_Closed1(object sender, EventArgs e)
        {
            this.Visibility = Visibility.Visible;
            this.Update();
        }

        private void calculo_frete(object sender, RoutedEventArgs e)
        {
            if(!Obra.Calcular_Rotas)
            {
                Conexoes.Utilz.Alerta("Obra está com o cálculo de rotas desabilitado. Habilite-o nos dados da Obra e edite o endereço.");
                return;
            }
            if (Obra.GetRotas().GetLista(Obra.Calcular_Rotas).Count == 0  && Obra.Calcular_Rotas)
            {
                Conexoes.Utilz.Alerta("Não foi possível encontrar rotas para o endereço da obra. Abra a tela de definição de endereço e ajuste", "Obra sem rotas", MessageBoxImage.Error);
                EditarRota();
                return;
            }
            ExplorerPLM.Menus.Custo_Frete mm = new ExplorerPLM.Menus.Custo_Frete(this.Obra);
            mm.Show();
        }

        private void editar_informacoes(object sender, RoutedEventArgs e)
        {
            var p = ((FrameworkElement)sender).DataContext as DLM.orc.PGO_Predio;
            Utilz.Propriedades(p, true);
            p.Salvar();
        }

        private void duplicar_predio(object sender, RoutedEventArgs e)
        {
            if (Obra.Bloqueado)
            {
                Conexoes.Utilz.Alerta("Obra está bloqueada para edições", "Obra Bloqueada", MessageBoxImage.Error);
                return;
            }
            if (ObjetoArvore is PGO_Predio == false) { return; }
            var copiar_de = ObjetoArvore as PGO_Predio;



            PGO_Predio novo_predio = new PGO_Predio(copiar_de, Obra);
            novo_predio.numero = (this.Obra.GetPredios().Count + 1).ToString().PadLeft(3, '0');

            if (!Criar_Predio(novo_predio))
            {
                return;
            }


            if (novo_predio.id > 0)
            {
                DBases.GetDbOrc().CopiarRanges(copiar_de, novo_predio);
                novo_predio.GetLocais(new List<Range>());
                this.Obra.GetRanges();
                GetArvore();
            }
            else
            {
                Conexoes.Utilz.Alerta("Algo de errado aconteceu ao tentar criar o prédio. Contacte suporte.");
            }

        }

        private void apaga_revisao(object sender, RoutedEventArgs e)
        {
            var p = ((FrameworkElement)sender).DataContext as DLM.orc.PGO_Predio;
            if (Utilz.Pergunta("Você tem certeza que deseja apagar o prédio " + p))
            {
                DBases.GetDbOrc().Apagar(p);
                Update();
            }
        }
        public bool Criar_Predio(PGO_Predio nPredio)
        {

        retentar:
            if(!nPredio.Propriedades())
            {
                return false;
            }
            if (this.Obra.GetPredios().ToList().Find(x => x.nome == nPredio.nome) != null)
            {
                if (Conexoes.Utilz.Pergunta(("Já existe um prédio com esse nome. Tentar novamente?")))
                {
                    goto retentar;
                }
                else
                {
                    return false;
                }
            }
            if (nPredio.nome == "")
            {
                if (Conexoes.Utilz.Pergunta(("Campo nome não pode estar em branco. Tentar novamente?")))
                {
                    goto retentar;
                }
                else
                {
                    return false;
                }
            }
            if (nPredio.numero == "")
            {
                if (Conexoes.Utilz.Pergunta(("Campo número não pode estar em branco. Tentar novamente?")))
                {
                    goto retentar;
                }
                else
                {
                    return false;
                }
            }
            if (Utilz.Int(nPredio.numero) == 0)
            {
                if (Conexoes.Utilz.Pergunta(("Campo número com valor inválido. Tentar novamente?")))
                {
                    goto retentar;
                }
                else
                {
                    return false;
                }
            }
            if (this.Obra.GetPredios().ToList().Find(x => x.numero == nPredio.numero) != null)
            {
                if (Conexoes.Utilz.Pergunta(("Já existe um prédio com esse número. Tentar novamente?")))
                {
                    goto retentar;
                }
                else
                {
                    return false;
                }
            }
            if (Utilz.Pergunta("Criar o Prédio " + nPredio.ToString() + " ?"))
            {
                nPredio.Salvar();
                return true;
            }
            else
            {
                return false;
            }

        }
        private void adicionar_predio(object sender, RoutedEventArgs e)
        {
            AddPredio();
        }

        private void AddPredio()
        {
            if (Obra.Bloqueado)
            {
                Conexoes.Utilz.Alerta("Obra está bloqueada para edições", "Obra Bloqueada", MessageBoxImage.Error);
                return;
            }
            PGO_Predio copiar_para = new PGO_Predio(this.Obra, "", "");
            copiar_para.numero = (this.Obra.GetPredios().Count + 1).ToString().PadLeft(3, '0');
            copiar_para.nome = this.Obra.GetPredios().Count == 0 ? "PRINCIPAL" : ("ANEXO " + this.Obra.GetPredios().Count.ToString().PadLeft(2, '0'));
            if (Criar_Predio(copiar_para))
            {
                if (this.Obra.GetPredios().ToList().FindAll(X => X.id != copiar_para.id).Count > 0)
                {
                    if (Utilz.Pergunta("Deseja copiar os Locais de outro prédio?"))
                    {
                        var copiar_de = Conexoes.Utilz.Selecao.SelecionarObjeto(this.Obra.GetPredios().ToList(), null) as PGO_Predio;
                        if (copiar_de != null)
                        {
                            DBases.GetDbOrc().CopiarRanges(copiar_de, copiar_para);
                            this.Obra.GetRanges();
                            GetArvore();
                            return;
                        }
                    }
                }
                this.Obra.GetRanges();
                GetArvore();
            }
        }

        private void adicionar_range(object sender, RoutedEventArgs e)
        {

            if (ObjetoArvore is OrcamentoGrupo == false) { return; }
            AddProduto();
        }

        private void AddProduto()
        {
            var p = ObjetoArvore as OrcamentoGrupo;
            AddRange pp = new AddRange(p, this.Obra);
            pp.ShowDialog();
            if ((bool)pp.DialogResult)
            {
                Atualizar_Lista();

            }
        }

        private void editar_financeiro(object sender, RoutedEventArgs e)
        {
            Controles.Dados_obra mm = new Controles.Dados_obra(this.Obra);
            mm.Show();
        }


        private void arvore_selecao(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Atualizar_Lista();
        }

        private void Atualizar_Lista()
        {
            Lista_Ranges_Padrao.ItemsSource = null;

            Grupo_Add_Barra.Visibility = Visibility.Collapsed;
            Predio_Editar.Visibility = Visibility.Collapsed;
            Predio_Exlcuir.Visibility = Visibility.Collapsed;
            Predio_Duplicar.Visibility = Visibility.Collapsed;
            Grupo_AdicionarRange.Visibility = Visibility.Collapsed;
            TreeViewItem sel = (TreeViewItem)Arvore.SelectedItem;
            if (sel == null) { return; }
            var t = sel.Tag;
            this.ObjetoArvore = t;
            if (t is PGO_Obra)
            {
                var item = t as PGO_Obra;
                ListaRanges.ItemsSource = null;
                ListaRanges.ItemsSource = item.GetRanges();
                Imagem_Sel.Source = item.Imagem;
                Label_Sel.Content = item.Contrato;


            }
            else if (t is PGO_Predio)
            {
                var ob = t as PGO_Predio;
                ListaRanges.ItemsSource = null;
                ListaRanges.ItemsSource = ob.Ranges;
                Predio_Editar.Visibility = Visibility.Visible;
                Predio_Exlcuir.Visibility = Visibility.Visible;
                Predio_Duplicar.Visibility = Visibility.Visible;
                Imagem_Sel.Source = ob.Imagem;
                Label_Sel.Content = ob.ToString();



            }
            else if (t is OrcamentoLocal)
            {
                var item = t as OrcamentoLocal;
                ListaRanges.ItemsSource = null;
                ListaRanges.ItemsSource = item.Ranges;
                Imagem_Sel.Source = item.Imagem;
                Label_Sel.Content = item.Predio.Obra.Contrato + "/" + item.Predio.ToString() + "/" + item.ToString();


            }
            else if (t is OrcamentoGrupo)
            {
                var ob = t as OrcamentoGrupo;
                ListaRanges.ItemsSource = null;
                ListaRanges.ItemsSource = ob.Ranges;
                Imagem_Sel.Source = ob.Imagem;
                Label_Sel.Content = ob.Local.Predio.Obra.Contrato + "/" + ob.Local.Predio.ToString() + "/" + ob.Local.ToString() + "/" + ob.ToString();
                Grupo_AdicionarRange.Visibility = Visibility.Visible;
                Grupo_Add_Barra.Visibility = Visibility.Visible;
                Lista_Ranges_Padrao.ItemsSource = ob.Itens;

            }
            UpdateLabels();
            CollectionViewSource.GetDefaultView(ListaRanges.ItemsSource).Filter = FiltroFuncao;
        }

        private void UpdateLabels()
        {
            this.total_valor.Content = $"{ListaRanges.Items.Cast<Range>().Sum(x => x.Atual.Valor_Total).ToString("C")}";
            this.total_peso.Content = $"{ListaRanges.Items.Cast<Range>().Sum(x => x.PesoTotal).ToString("N2")} Kg";
        }

        private bool FiltroFuncao(object item)
        {
            if (Filtrar.Text == "Pesquisar...") { return true; }
            if (String.IsNullOrEmpty(Filtrar.Text))
                return true;

            return Conexoes.Utilz.Contem(item, Filtrar.Text);

        }
        private void botao_direito(object sender, MouseButtonEventArgs e)
        {
            Predio_Editar.Visibility = Visibility.Collapsed;
            Predio_Exlcuir.Visibility = Visibility.Collapsed;
            Predio_Duplicar.Visibility = Visibility.Collapsed;
            if (sender is TreeView)
            {
                var s = sender as TreeView;
                var ob = s.Tag;
                if (ob is PGO_Predio)
                {
                    Predio_Editar.Visibility = Visibility.Visible;
                    Predio_Exlcuir.Visibility = Visibility.Visible;
                }
                else if (ob is OrcamentoLocal)
                {

                }
                else if (ob is OrcamentoGrupo)
                {

                }


            }

        }
        public object ObjetoArvore { get; set; } = null;
        private void editar_dados(object sender, RoutedEventArgs e)
        {
            if (ObjetoArvore is PGO_Predio)
            {
                var pr = ObjetoArvore as PGO_Predio;
                if (pr.Propriedades())
                { pr.Salvar(); }

                GetArvore();
            }
        }

        private void excluir_predio(object sender, RoutedEventArgs e)
        {
            if (ObjetoArvore is PGO_Predio == false) { return; }
            var p = ObjetoArvore as PGO_Predio;

            if (Utilz.Pergunta("Você tem certeza que deseja apagar o prédio " + p))
            {
                DBases.GetDbOrc().Apagar(p);
                GetArvore();
            }
        }

        private void excluir_ranges(object sender, RoutedEventArgs e)
        {
            Excluir();
        }

        private void Excluir()
        {
            List<Range> Ranges = ListaRanges.Selecoes<Range>();

            if (Ranges.Count > 0)
            {
                Excluir(Ranges);
            }
        }

        private void Excluir(List<Range> Ranges)
        {
            if (Utilz.Pergunta("Você tem certeza que deseja excluir o(s) " + Ranges.Count + " Range(s) selecionado(s)?"))
            {
                ControleWait w = Conexoes.Utilz.Wait(Ranges.Count, "Apagando itens...");
                w.somaProgresso();
                w.somaProgresso();
                this.Obra.Apagar(Ranges);
                w.Close();
                Atualizar_Lista();
            }
        }



        private void set_esquema(object sender, RoutedEventArgs e)
        {
            List<Range> Ranges = ListaRanges.Selecoes<Range>().FindAll(x => x.Produto.pintura > 0);
            if (Ranges.Count == 0) { return; }
            var esquema = this.Obra.GetTratamento();
            if (!Utilz.Pergunta("Atribuir o esquema padrão da obra? [" + esquema.ToString() + "]"))
            {
                esquema = Conexoes.Utilz.Selecao.SelecionarObjeto(DBases.GetDbOrc().GetTratamentos(), null) as Tratamento;
            }
            if (esquema != null)
            {
                if (Utilz.Pergunta("Deseja aplicar o esquema " + esquema.ToString() + " nos ranges selecionados?"))
                {
                    foreach (var r in Ranges)
                    {
                        r.Tratamento = esquema;
                        r.Salvar();
                    }
                }
            }
        }

        private void editar_produto(object sender, RoutedEventArgs e)
        {
            if (Obra.Bloqueado)
            {
                Conexoes.Utilz.Alerta("Obra está bloqueada para edições", "Obra Bloqueada", MessageBoxImage.Error);
                return;
            }
            DLM.orc.Range sel = ((FrameworkElement)sender).DataContext as DLM.orc.Range;
            if (sel != null)
            {
                if (sel.Grupo_de_Mercadoria == "VERBA")
                {
                    string valor = Utilz.Prompt("Digite", "", sel.Descricao);
                    if (valor != "")
                    {
                        sel.Descricao = valor;
                        sel.Salvar();
                    }

                }
                else
                {
                    var prod = Conexoes.Utilz.Selecao.SelecionarObjeto(sel.Produto.Grupo_De_Mercadoria.Produtos.FindAll(x => x.ativo).OrderBy(X => X.id).ToList(), null) as Produto;
                    if (prod != null)
                    {
                        sel.SetProduto(prod);
                        sel.Salvar();
                    }
                }


            }
        }

        private void Editar_Produtos(List<Range> ranges)
        {
            var prods = ranges.Select(x => x.Produto.id).Distinct().ToList();
            foreach (var p in prods)
            {
                var rs = ranges.FindAll(x => x.Produto.id == p);

                if (rs.Count > 0)
                {
                    var sel = rs[0];
                    if (sel != null)
                    {
                        if (sel.Verba)
                        {
                            string valor = Utilz.Prompt("Digite", "", sel.Descricao);
                            if (valor != "")
                            {
                                if (!Utilz.Pergunta("Tem certeza que deseja alterar a descrição das verbas para " + valor.ToString()))
                                {
                                    continue;
                                }
                                foreach (var r in rs)
                                {
                                    r.Descricao = valor;
                                    r.Salvar();
                                }
                            }

                        }
                        else
                        {
                            var prod = Conexoes.Utilz.Selecao.SelecionarObjeto(sel.Produto.Grupo_De_Mercadoria.Produtos.FindAll(x => x.ativo), null, "Selecione o substituto para " + sel.Produto.ToString()) as Produto;

                            if (prod != null)
                            {
                                if (!Utilz.Pergunta("Tem certeza que deseja alterar os Ranges de " + sel.Produto.ToString() + " para " + prod.ToString()))
                                {
                                    continue;
                                }
                                foreach (var r in rs)
                                {
                                    r.SetProduto(prod);
                                    r.Salvar();
                                }
                            }
                        }


                    }
                }

            }
        }

        private void editar_quantidade(object sender, RoutedEventArgs e)
        {
            if (Obra.Bloqueado)
            {
                Conexoes.Utilz.Alerta("Obra está bloqueada para edições", "Obra Bloqueada", MessageBoxImage.Error);
                return;
            }
            List<Range> Ranges = ListaRanges.Selecoes<Range>();

            if (Ranges.Count > 0)
            {
                double Quantidade = Utilz.Double(Utilz.Prompt("Digite a quantidade", "", Ranges[0].Quantidade.ToString(), false, "", false, 14));
                if (Quantidade <= 0)
                {
                    return;
                }
                foreach (var r in Ranges)
                {
                    r.Quantidade = Quantidade;
                    r.Salvar();
                }
            }
        }

        private void editar_carreta(object sender, RoutedEventArgs e)
        {
            if (Obra.Bloqueado)
            {
                Conexoes.Utilz.Alerta("Obra está bloqueada para edições", "Obra Bloqueada", MessageBoxImage.Error);
                return;
            }
            DLM.orc.Range sel = ((FrameworkElement)sender).DataContext as DLM.orc.Range;
            if (sel == null) { return; }
            var Carreta = Conexoes.Utilz.Selecao.SelecionarObjeto(DBases.GetDbOrc().GetTipo_Carreta(), sel.Tipo_De_Carreta) as Tipo_Carreta;
            if (Carreta == null) { return; }

            sel.setCarreta_User(Carreta);
            sel.Salvar();
        }

        private void editar_carreta_multiplo(object sender, RoutedEventArgs e)
        {
            List<Range> Ranges = ListaRanges.Selecoes<Range>();

            if (Ranges.Count > 0)
            {
                var Carreta = Conexoes.Utilz.Selecao.SelecionarObjeto(DBases.GetDbOrc().GetTipo_Carreta(), null) as Tipo_Carreta;
                if (Carreta == null) { return; }

                foreach (var r in Ranges)
                {
                    r.setCarreta_User(Carreta);
                    r.Salvar();
                }
            }
        }

        private void editar_tratamento(object sender, RoutedEventArgs e)
        {
            if (Obra.Bloqueado)
            {
                Conexoes.Utilz.Alerta("Obra está bloqueada para edições", "Obra Bloqueada", MessageBoxImage.Error);
                return;
            }
            DLM.orc.Range sel = ((FrameworkElement)sender).DataContext as DLM.orc.Range;
            if (sel == null) { return; }
            var esquema = this.Obra.GetTratamento();
            if (!Utilz.Pergunta("Atribuir o esquema padrão da obra? [" + esquema.ToString() + "]"))
            {
                esquema = Conexoes.Utilz.Selecao.SelecionarObjeto(DBases.GetDbOrc().GetTratamentos(), null) as Tratamento;
            }
            if (esquema != null)
            {
                sel.Tratamento = esquema;
                sel.Salvar();
            }
        }

        private void adicionar_item(object sender, RoutedEventArgs e)
        {
            if (ObjetoArvore is OrcamentoGrupo)
            {
                AddProduto();

            }
        }

        private void gerar_proposta(object sender, RoutedEventArgs e)
        {

            Obra.GerarProposta();
        }

        private void SetFolha_Margem()
        {
            if (this.Obra.Folha_Margem.Data == "")
            {
                this.Obra.Folha_Margem.Calcular(this.Obra);
                this.Obra.Salvar_Folha_Margem();
            }
            //else if (Utilz.Pergunta("Deseja recalcular? O último cálculo é de " + this.Obra.Folha_Margem.Data + " realizado por " + this.Obra.Folha_Margem.User))
            //{
            //    this.Obra.Folha_Margem.Calcular(this.Obra);
            //    this.Obra.Salvar_Folha_Margem();
            //}
        }

        private void adicionar_item_padrao(object sender, RoutedEventArgs e)
        {
            if (ObjetoArvore is OrcamentoGrupo)
            {
                var grupo = ObjetoArvore as OrcamentoGrupo;
                DLM.orc.OrcamentoItem_Arvore sel = ((FrameworkElement)sender).DataContext as DLM.orc.OrcamentoItem_Arvore;
                AddRange pp = new AddRange(grupo, sel, this.Obra);
                pp.ShowDialog();
                if ((bool)pp.DialogResult)
                {
                    Atualizar_Lista();

                }

            }
        }

        private void editar_fert(object sender, RoutedEventArgs e)
        {
            //if (Obra.Bloqueado)
            //{
            //    Conexoes.Utilz.Alerta("Obra está bloqueada para edições", "Obra Bloqueada", MessageBoxImage.Error);
            //    return;
            //}
            DLM.orc.Range sel = ((FrameworkElement)sender).DataContext as DLM.orc.Range;
            if (sel == null) { return; }
            SetFert(new List<Range> { sel });
        }

        private void excluir_range(object sender, RoutedEventArgs e)
        {
            DLM.orc.Range sel = ((FrameworkElement)sender).DataContext as DLM.orc.Range;
            if (sel == null) { return; }
            Excluir(new List<Range> { sel });

        }

        private void duplicar_range(object sender, RoutedEventArgs e)
        {
            if (Obra.Bloqueado)
            {
                Conexoes.Utilz.Alerta("Obra está bloqueada para edições", "Obra Bloqueada", MessageBoxImage.Error);
                return;
            }
            DLM.orc.Range sel = ((FrameworkElement)sender).DataContext as DLM.orc.Range;
            if (sel == null) { return; }
            AddRange mm = new AddRange(sel, this.Obra);
            mm.ShowDialog();
            if ((bool)mm.DialogResult)
            {
                Atualizar_Lista();

            }
        }

        private void novo_predio(object sender, RoutedEventArgs e)
        {
            AddPredio();
        }

        private void add_verba(object sender, RoutedEventArgs e)
        {
            if (ObjetoArvore is OrcamentoGrupo)
            {
                var p = ObjetoArvore as OrcamentoGrupo;
                Range s = new Range(this.Obra, "", 0, 0, p.Local.Predio, p);
                AddRange pp = new AddRange(s, this.Obra);
                pp.ShowDialog();
                if ((bool)pp.DialogResult)
                {
                    Atualizar_Lista();

                }
            }
        }

        private void editar_quantidade_sel(object sender, RoutedEventArgs e)
        {
            if (Obra.Bloqueado)
            {
                Conexoes.Utilz.Alerta("Obra está bloqueada para edições", "Obra Bloqueada", MessageBoxImage.Error);
                return;
            }
            DLM.orc.Range r = ((FrameworkElement)sender).DataContext as DLM.orc.Range;
            if (r == null) { return; }
            double Quantidade = Utilz.Double(Utilz.Prompt("Digite a quantidade", "", r.Quantidade.ToString(), false, "", false, 14));
            if (Quantidade > 0)
            {
                r.Quantidade = Quantidade;
                r.Salvar();
            }

        }

        private void monitora_key(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                Excluir();
            }
        }

        private void calculo_folha_margem(object sender, RoutedEventArgs e)
        {

            SetFolha_Margem();
            PGO.Tela_Folha_Margem mm = new PGO.Tela_Folha_Margem(this.Obra);
            mm.Show();
        }

        private void atualizar_custos(object sender, RoutedEventArgs e)
        {
            var t = ListaRanges.Selecoes<Range>();
            if (t.Count() > 0)
            {
                if (Utilz.Pergunta("Tem certeza que deseja atualizar o custo dos " + t.Count() + " itens selecionados?"))
                {
                    ControleWait w = Conexoes.Utilz.Wait(t.Count(), "Atualizando");
                    foreach (var s in t)
                    {
                        s.Atualizado.Salvar();
                        s.Atual = s.Atualizado;
                        w.somaProgresso();
                    }
                    w.Close();
                }
            }
        }

        private void Editar_Mercadoria_Externa(object sender, RoutedEventArgs e)
        {
            var rs = ListaRanges.Selecoes<Range>();
            Editar_Produtos(rs);
        }

        private void Atualizar_estrutura(object sender, RoutedEventArgs e)
        {
            this.Obra.GetRanges();
            GetArvore();
        }

        private void exportar_folha_margem(object sender, RoutedEventArgs e)
        {
            this.Obra.GerarProposta();
        }

        private void teste(object sender, RoutedEventArgs e)
        {
            PGO.Justin_Tela mm = new PGO.Justin_Tela();
            mm.Show();
        }

        private void ModernWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Utilz.GravaVars(visualizar, Vars.ArqASetupUser, "Orcamento.JanelaObra");
        }

        private void ver_etapas(object sender, RoutedEventArgs e)
        {
            PGO.Etapas mm = new PGO.Etapas(this.Obra);
            mm.Show();
            mm.Closing += Mm_Closing;
            this.Visibility = Visibility.Collapsed;
        }

        private void Mm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Visibility = Visibility.Visible;
        }

        private void ver_materiais_etapas(object sender, RoutedEventArgs e)
        {
            Funcoes.VerMateriais(this.Obra);
        }

        private void ver_pecas_fim(object sender, RoutedEventArgs e)
        {
            DLM.orc.Range sel = ((FrameworkElement)sender).DataContext as DLM.orc.Range;
            if (sel == null) { return; }
            Funcoes.VerMateriais(this.Obra, sel.GetPecas(true));
        }

        private void ver_pecas_varios(object sender, RoutedEventArgs e)
        {
            List<Range> Ranges = ListaRanges.Selecoes<Range>();
            if (Ranges.Count > 0)
            {
                Funcoes.VerMateriais(this.Obra, Conexoes.Utilz.GetPecas_Orcamento(this.Obra, true, Ranges));

            }
        }

        private void editar_peso_verba(object sender, RoutedEventArgs e)
        {
            var ranges = ListaRanges.Selecoes<Range>().FindAll(x => x.Verba);

            if (ranges.Count > 0)
            {
                double qtd = Utilz.Double(Utilz.Prompt("Digite o peso unitário", "Digite o peso", ranges[0].Peso.ToString(), false, "", false, 14));

                if(qtd>0)
                {
                    foreach (var range in ranges)
                    {
                        range.SetPesoVerba(qtd);
                    }
                }
                else
                {
                    Conexoes.Utilz.Alerta("Peso deve ser maior que zero");
                }

            }
        }

        private void editar_observacoes(object sender, RoutedEventArgs e)
        {
            this.Obra.EditarObservacoes();
        }

        private ICommand _Colar;

        public ICommand Colar
        {
            get
            {
                if (_Colar == null)
                {
                    _Colar = new FirstFloor.ModernUI.Presentation.RelayCommand(
                        param => this.ExecutarColar()
                    );
                }
                return _Colar;
            }
        }
        public void ExecutarColar()
        {
            //var t = Encoding.Default.GetBytes(((string)Clipboard.GetData("Text")));
            //Input.Text += Encoding.Default.GetString(t);
            //Input.Text += Clipboard.GetText();

            Input.Selection.Text = Clipboard.GetText().Replace('\t', ' ');





            // Input.Document.Blocks.Add(new Paragraph(new Run(Clipboard.GetText().Replace(@"\t\t",@"\t"))));
        }

        private void add_verba_pintura(object sender, RoutedEventArgs e)
        {
            if (ObjetoArvore is OrcamentoGrupo)
            {
                var p = ObjetoArvore as OrcamentoGrupo;
                var rm = Conexoes.DBases.GetBancoRM().CriaRetornaSAP(Acessos_Criterio.ORC, "TINTA");
                if (rm == null) { return; }
                double peso_total = 0;
                double quantidade = 0;
                bool calcular = Conexoes.Utilz.Pergunta("Deseja calcular automaticamente? \nO PGO buscará todas as peças com esquema de pintura e somará o peso, depois considerará 3,6L de tinta para cada 10 Ton");
                bool tudo = false;
                if (calcular)
                {
                    tudo = Conexoes.Utilz.Pergunta("Calcular para toda a obra? \n Se clicar em não, será calculado somente para o prédio " + p.Local.Predio.ToString());

                    List<DLM.orc.PGO_Peca> pecas = new List<DLM.orc.PGO_Peca>();
                    if (tudo)
                    {
                        pecas = this.Obra.GetPecasRanges();
                    }
                    else
                    {
                        pecas = p.Local.Predio.GetPecasRanges();
                    }
                    peso_total = pecas.FindAll(x => x.Tratamento.Contains("PINTURA")).Sum(x => x.PesoTotal);

                    quantidade = Conexoes.Utilz.ArredondarMultiplo(peso_total / 1000 / 10 * 3.6, 3.6);

                    if (peso_total > 0 && quantidade > 0)
                    {
                        Conexoes.Utilz.Alerta("Peso total encontrado: " + Math.Round(peso_total, 2) + " quantidade considerada:" + quantidade);
                    }
                }



                Range s = new Range(this.Obra, "Tinta Para Retoque", quantidade, quantidade, p.Local.Predio, p);
                s.Produto.unidade = "L";
                AddRange pp = new AddRange(s, this.Obra);
                pp.ShowDialog();
                if ((bool)pp.DialogResult)
                {
                    if (pp.Range.id > 0 && pp.Range.Quantidade > 0)
                    {
                        pp.Range.setMaterial_User(rm.id_db, pp.Range.Quantidade);
                        SetFert(new List<Range> { s });
                        Atualizar_Lista();
                    }

                }


            }
        }

        private void editar_peso_verba_zerar(object sender, RoutedEventArgs e)
        {
            var ranges = ListaRanges.Selecoes<Range>().FindAll(x => x.Verba);

            if (ranges.Count > 0)
            {
                foreach (var range in ranges)
                {
                    range.SetPesoVerba(0);
                }

            }
        }

        private void set_material_range(object sender, RoutedEventArgs e)
        {
            var ranges = ListaRanges.Selecoes<Range>().FindAll(x => x.Verba);
            if (ranges.Count > 0)
            {
                foreach (var range in ranges)
                {
                    var t = range.GetMaterialUser();
                    if (t != null)
                    {
                        if(range.Material_User_Qtd == 0)
                        {
                            range.setMaterial_User_Qtd(1);
                        }

                        range.SetPesoVerba(range.Material_User_Qtd * t.PesoUnit);
                    }
                }
            }
        }

        private void Filtrar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Filtrar.Text != "Pesquisar...")
            {
                CollectionViewSource.GetDefaultView(ListaRanges.ItemsSource).Refresh();
            }
        }
    }

}
