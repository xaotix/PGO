using Conexoes;
using DLM.orc;
using DLM.vars;
using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Telerik.Charting;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.ChartView;
using DLM.encoder;

namespace PGO
{
    /// <summary>
    /// Interaction logic for Listas_Tecnicas.xaml
    /// </summary>
    public partial class Listas_Tecnicas : ModernWindow
    {
        public Listas_Tecnicas()
        {
            InitializeComponent();

        }

        private Produto Produto_Selecionado { get; set; }
        private void Selecao_Lista_Tecnica(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

            //if((Lista.SelectedItem as TreeViewItem).Tag is Produto)
            //{
            //    Produto_Selecionado = ((Lista.SelectedItem as TreeViewItem).Tag as Produto);
            //    Lista_Pecas.ItemsSource = Produto_Selecionado.GetPecas();
            //    Lista_Pecas.IsEnabled =true;
            //    Edicao.Visibility = Visibility.Visible;

            //    return;

            //}
            //Edicao.Visibility = Visibility.Collapsed;
            //Lista_Pecas.ItemsSource = null;
            //Produto_Selecionado = null;
            //Lista_Pecas.IsEnabled = false;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Adiciona_Peca mm = new Adiciona_Peca(new PecaDB(), Produto_Selecionado);
            mm.ShowDialog();
            if (mm.DialogResult.HasValue && mm.DialogResult.Value)
            {
                if (mm.Selecao.Peca.id_peca > 0)
                {

                    mm.Selecao.Produto.AddPeca(mm.Selecao.Peca);
                    Rad_Lista_Pecas.ItemsSource = null;
                    Rad_Lista_Pecas.ItemsSource = Produto_Selecionado.PecasDB;
                }
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            if (Rad_Lista_Pecas.SelectedItems.Count > 0)
            {
                if (Conexoes.Utilz.Pergunta("Tem certeza que deseja remover os " + Rad_Lista_Pecas.SelectedItems.Count))
                {
                    if ((bool)Um_Item.IsChecked | (bool)ver_agrupado.IsChecked)
                    {
                        DBases.GetDbOrc().Apagar(Rad_Lista_Pecas.SelectedItems.Cast<PecaDB>().ToList());
                        DBases.SetDbOrc(null);
                        setFiltro();
                    }
                    else
                    {
                        foreach (var t in Rad_Lista_Pecas.SelectedItems.Cast<PecaDB>())
                        {
                            Produto_Selecionado.ApagarPeca(t);
                        }

                        Rad_Lista_Pecas.ItemsSource = null;
                        Rad_Lista_Pecas.ItemsSource = Produto_Selecionado.PecasDB;
                    }
                }
            }
        }

        private void Lista_Pecas_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
        {

        }

        private void Lista_Produtos_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
        {
            //Edicao.Visibility = Visibility.Collapsed;
            //Lista_Pecas.ItemsSource = null;
            //Produto_Selecionado = null;
            //Lista_Pecas.IsEnabled = false;
        }

        private void ListaProdutos_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
        {
            if ((sender as RadGridView).SelectedItem is Produto)
            {
                Produto_Selecionado = ((sender as RadGridView).SelectedItem as Produto);



                Input.Text = Produto_Selecionado.Observacoes;
                Rad_Lista_Pecas.ItemsSource = Produto_Selecionado.PecasDB;
                propriedades_produto.SelectedObject = Produto_Selecionado;

                //fert.Text = Produto_Selecionado.FERT;
                //unidade_produto.Content = Produto_Selecionado.unidade;
                nome_selecao.Content = Produto_Selecionado.Chave;
                Rad_Lista_Pecas.IsEnabled = true;
                Edicao.Visibility = Visibility.Visible;

                return;

            }
            Edicao.Visibility = Visibility.Collapsed;
            Rad_Lista_Pecas.ItemsSource = null;
            Produto_Selecionado = null;
            Rad_Lista_Pecas.IsEnabled = false;
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            if (Rad_Lista_Pecas.SelectedItem is PecaDB)
            {
                var p = Rad_Lista_Pecas.SelectedItem as PecaDB;
                Adiciona_Peca mm = new Adiciona_Peca(p, Produto_Selecionado);
                mm.Title = "Editar " + p.ToString();
                mm.ShowDialog();

                if (mm.DialogResult.HasValue && mm.DialogResult.Value)
                {
                    if (mm.Selecao.Peca.id_peca > 0)
                    {
                        if (mm.Selecao.Produto != null)
                        {
                            mm.Selecao.Produto.AddPeca(mm.Selecao.Peca);
                            Rad_Lista_Pecas.ItemsSource = null;
                            Rad_Lista_Pecas.ItemsSource = Produto_Selecionado.PecasDB;
                        }

                    }
                    else
                    {
                        Conexoes.Utilz.Alerta("Peça inválida. Não pode ser adicionada ou ativada.");
                    }
                }
            }

        }

        private void Importar_Almox(object sender, RoutedEventArgs e)
        {
            string ss = Conexoes.Utilz.Abrir_String("xlsx", "", "");
            if (File.Exists(ss))
            {
                string planilha = "";
                List<List<object>> Linhas = Conexoes.Utilz.Excel.GetLista(Conexoes.Utilz.Excel.GetDataTable(ss, out planilha, false));
                if (Linhas.Count > 0)
                {
                    Conexoes.ControleWait w = Conexoes.Utilz.Wait(Linhas.Count, "Lendo Excel...");

                    var ids = Linhas.Select(x => x[0].ToString()).Distinct().ToList();
                    int tt = 1;
                    var lista = Linhas.Select(x => x.Select(y => y.ToString()).ToList()).ToList();
                    List<Report> reports = new List<Report>();
                    foreach (var id in ids)
                    {
                        Produto corr = DBases.GetDbOrc().GetProdutos().Find(x => x.id == Conexoes.Utilz.Int(id));
                        var lts = lista.FindAll(x => x[0] == id.ToString());
                        if (corr != null)
                        {
                            w.SetProgresso(1, lts.Count);
                            int c = 1;
                            foreach (var t in lts)
                            {
                                string cod = t[3].ToString();
                                double qtd = Conexoes.Utilz.Double(t[2]);


                                var pc = Conexoes.DBases.GetBancoRM().GetRMA(cod);
                                if (pc != null)
                                {
                                    PecaDB nova = new PecaDB();

                                    var pcatual = corr.PecasDB.Find(x => x.id_peca == pc.id_db);
                                    if (pcatual != null)
                                    {
                                        nova = pcatual;
                                    }

                                    nova.Arredondar = true;
                                    nova.Ativo = true;
                                    nova.id_peca = pc.id_db;
                                    nova.Quantidade = qtd;
                                    nova.Tipo = "RMA";
                                    corr.AddPeca(nova);

                                }
                                else
                                {
                                    reports.Add(new Report("Item não encontrado.", cod + " LT=" + corr.Chave, TipoReport.Alerta));
                                }
                                w.SetProgresso(c, lts.Count);
                                c++;

                            }
                        }
                        else
                        {
                            reports.Add(new Report("LT não encontrada.", " id =" + id, TipoReport.Alerta));
                        }
                        tt++;
                    }
                    Rad_Lista_Pecas.ItemsSource = null;
                    w.Close();
                    Conexoes.Utilz.ShowReports(reports);

                }

            }
        }

        private void Importar_Arremates(object sender, RoutedEventArgs e)
        {
            string ss = Conexoes.Utilz.Abrir_String("xlsx", "", "");
            if (File.Exists(ss))
            {
                string planilha = "";
                var tbl = Conexoes.Utilz.Excel.GetLista(Conexoes.Utilz.Excel.GetDataTable(ss, out planilha, false));
                //var tbl = Conexoes.Utilz.Excel.GetTabela(ss, false);


                if (tbl.Count > 0)
                {
                    Conexoes.ControleWait w = Conexoes.Utilz.Wait(tbl.Count, "Lendo Excel...");


                    var ids = tbl.Select(x => x[0].ToString()).Distinct().ToList();
                    int tt = 1;
                    var lista = tbl.Select(x => x.Select(y => y.ToString()).ToList()).ToList();
                    List<Report> reports = new List<Report>();
                    int id_codigo = 10;
                    int id_marca = 3;
                    int id_pos = 4;
                    int id_quantidade = 5;
                    int id_maktx = 13;
                    int id_unidade = 14;
                    int id_compri = 9;

                    foreach (var id in ids)
                    {
                        Produto corr = DBases.GetDbOrc().GetProdutos().Find(x => x.id == Conexoes.Utilz.Int(id));
                        var lts = lista.FindAll(x => x[0] == id.ToString());
                        if (corr != null)
                        {
                            w.SetProgresso(1, lts.Count);
                            int c = 1;
                            for (int i = 0; i < lts.Count; i++)
                            {
                                string cod = lts[i][id_codigo].ToString();
                                string marca = lts[i][id_marca].ToString();
                                string unidade_fabril = "";
                                string pos = lts[i][id_pos].ToString();
                                double qtd = Conexoes.Utilz.Double(lts[i][id_quantidade]);
                                if (qtd == 0)
                                {
                                    qtd = Conexoes.Utilz.Double(lts[i][id_quantidade + 1]);
                                }
                                string maktx = "";
                                double comp = Conexoes.Utilz.Double(lts[i][id_compri]);
                                if (lts[i].Count - 1 >= id_maktx)
                                {
                                    maktx = lts[i][id_maktx];
                                }
                                if (lts[i].Count - 1 >= id_unidade)
                                {
                                    unidade_fabril = lts[i][id_unidade];
                                }

                                //pula linha de marcas de almox
                                if (marca.Contains(Cfg.Init.RM_MARCA_ALMOX))
                                {

                                    continue;
                                }
                                //importando um almox
                                if (pos.Contains("PIE"))
                                {
                                    var pc = Conexoes.DBases.GetBancoRM().GetRMA(cod);
                                    if (pc != null)
                                    {
                                        PecaDB nova = new PecaDB();

                                        var pcatual = corr.PecasDB.Find(x => x.id_peca == pc.id_db);
                                        if (pcatual != null)
                                        {
                                            nova = pcatual;
                                        }
                                        nova.Arredondar = true;
                                        nova.Ativo = true;
                                        nova.id_peca = pc.id_db;
                                        nova.Quantidade = qtd;
                                        nova.Tipo = "RMA";

                                        if (nova.Codigo != cod)
                                        {
                                            Bobina bob = DBases.GetBancoRM().GetBobina(cod);
                                            nova.id_materia_prima = bob.id;
                                        }
                                        corr.AddPeca(nova);
                                    }

                                    else
                                    {
                                        AddDummy(corr, cod, marca, qtd, comp, maktx);
                                    }
                                    w.SetProgresso(c, lts.Count);
                                }
                                else if (marca.Replace(" ", "").Replace("0", "") != "")
                                {
                                    //SETA O CÓDIGO SE ESTIVER EM BRANCO
                                    if (cod.Replace("0", "").Replace(" ", "") == "" && i + 1 < lts.Count)
                                    {
                                        cod = lts[i + 1][id_codigo].ToString();
                                    }
                                    var pc = Conexoes.DBases.GetBancoRM().GetPeca(new MBS_Ship(new Marca() { Nome = marca.Replace(" ", ""), Comprimento = comp }) { Maktx = maktx, MateriaPrima = cod });
                                    if (pc != null)
                                    {
                                        if (pc is Conexoes.RMU)
                                        {
                                            PecaDB nova = new PecaDB();

                                            var pcatual = corr.PecasDB.Find(x => x.id_peca == (pc as Conexoes.RME).id_codigo);
                                            if (pcatual != null)
                                            {
                                                nova = pcatual;
                                            }

                                            nova.Comprimento = comp;
                                            nova.Arredondar = true;
                                            nova.Ativo = true;
                                            nova.id_peca = (pc as Conexoes.RME).id_codigo;
                                            nova.Quantidade = qtd;
                                            nova.Tipo = "RMU";

                                            Bobina bob = DBases.GetBancoRM().GetBobina(cod);
                                            nova.id_materia_prima = bob.id;


                                            corr.AddPeca(nova);


                                        }
                                        else if (pc is Conexoes.RME)
                                        {
                                            PecaDB nova = new PecaDB();

                                            var pcatual = corr.PecasDB.Find(x => x.id_peca == (pc as Conexoes.RME).id_codigo);
                                            if (pcatual != null)
                                            {
                                                nova = pcatual;
                                            }
       
                                            nova.Comprimento = comp;
                                            nova.Arredondar = true;
                                            nova.Ativo = true;
                                            nova.id_peca = (pc as Conexoes.RME).id_codigo;

                                            nova.Quantidade = qtd;
                                            nova.Tipo = "RME";
                                            corr.AddPeca(nova);
                                        }
                                        else if (pc is Conexoes.RMA)
                                        {
                                            PecaDB nova = new PecaDB();

                                            var pcatual = corr.PecasDB.Find(x => x.id_peca == (pc as Conexoes.RMA).id_db);
                                            if (pcatual != null)
                                            {
                                                nova = pcatual;
                                            }

                                            nova.Arredondar = true;
                                            nova.Ativo = true;
                                            //nova.Descricao = pc.ToString();
                                            nova.id_peca = (pc as Conexoes.RMA).id_db;
                                            //nova.Unidade_Fabril = unidade_fabril;

                                            nova.Quantidade = qtd;
                                            nova.Tipo = "RMA";
                                            corr.AddPeca(nova);
                                        }
                                        else if (pc is Conexoes.RMT)
                                        {
                                            PecaDB nova = new PecaDB();

                                            var pcatual = corr.PecasDB.Find(x => x.id_peca == (pc as Conexoes.RMT).id_telha);
                                            if (pcatual != null)
                                            {
                                                nova = pcatual;
                                            }

                                            nova.Arredondar = true;
                                            nova.Ativo = true;
                                            //nova.Descricao = pc.ToString();
                                            nova.id_materia_prima = (pc as Conexoes.RMT).Bobina.id;
                                            nova.id_peca = (pc as Conexoes.RMT).id_telha;
                                            nova.Comprimento = comp;
                                            //nova.Unidade_Fabril = unidade_fabril;

                                            nova.Quantidade = qtd;
                                            nova.Tipo = "RMT";


                                            corr.AddPeca(nova);
                                        }
                                        else if (pc is Report)
                                        {
                                            var t = pc as Report;
                                            reports.Add(new Report("LT=" + corr.id + " - " + corr.Chave, t.Propriedades + " - " + t.Descricao, t.Tipo));
                                            AddDummy(corr, cod, marca, qtd, comp, maktx);

                                        }
                                        else
                                        {
                                            reports.Add(new Report("LT=" + corr.id + " - " + corr.Chave, "Item não encontrado = " + marca, TipoReport.Alerta));
                                            AddDummy(corr, cod, marca, qtd, comp, maktx);

                                        }

                                    }
                                    else
                                    {
                                        reports.Add(new Report("LT=" + corr.id + " - " + corr.Chave, "Item não encontrado = " + marca, TipoReport.Alerta));
                                        AddDummy(corr, cod, marca, qtd, comp, maktx);

                                    }
                                    //pula a linha da posicao
                                    if (i < lts.Count - 1)
                                    {
                                        if (lts[i + 1][id_marca].ToString().Replace(" ", "").Replace("0", "") == "")
                                        {

                                            i++;
                                        }
                                    }

                                }
                                else if (!pos.Contains("PIE"))
                                {

                                }
                                else
                                {
                                    reports.Add(new Report("Linha Ignorada. Nada encontrado correspondente.", " id = " + id + " Linha = " + i, TipoReport.Alerta));

                                }

                                c++;
                            }

                        }
                        else
                        {
                            reports.Add(new Report("LT não encontrada.", " id =" + id, TipoReport.Alerta));
                        }
                        tt++;
                    }
                    Rad_Lista_Pecas.ItemsSource = null;
                    w.Close();
                    Conexoes.Utilz.ShowReports(reports);

                }

            }
        }

        private static void AddDummy(Produto corr, string cod, string marca, double qtd, double comp, string maktx, string observacoes = null)
        {
            PecaDB nova = new PecaDB();

            //nova.Codigo = marca;
            //if(nova.Codigo.Replace(" ","").Replace("0","") == "")
            //{
            //    nova.Codigo = cod;
            //}
            if (observacoes == null)
            {
                nova.Observacoes = "Peça não encontrada.";

            }
            else
            {
                nova.Observacoes = observacoes;

            }
            nova.Comprimento = comp;
            nova.Arredondar = true;
            //nova.Descricao = maktx;
            nova.id_peca = -1;
            nova.Quantidade = qtd;
            nova.Ativo = false;
            if (nova.Codigo != cod)
            {

                Bobina bob = DBases.GetBancoRM().GetBobina(cod);
                nova.id_materia_prima = bob.id;
            }
            corr.AddPeca(nova);
        }

        private void Exportar(object sender, RoutedEventArgs e)
        {
            ExplorerPLM.Utilidades.Exportar(Rad_Lista_Pecas);
        }

        private void editar_quantidade(object sender, RoutedEventArgs e)
        {
            var lits = Rad_Lista_Pecas.SelectedItems.Cast<PecaDB>().ToList();
            if (lits.Count > 0)
            {
                double qtd = Conexoes.Utilz.Double(Conexoes.Utilz.Prompt("Digite a quantidade", "", lits[0].Quantidade.ToString()));
                    foreach (var t in lits)
                    {
                        t.SetQuantidade(qtd);
                    }
            }
        }




        private void editar_observacoes(object sender, RoutedEventArgs e)
        {
            var lits = Rad_Lista_Pecas.SelectedItems.Cast<PecaDB>().ToList();
            if (lits.Count > 0)
            {
                string qtd = Conexoes.Utilz.Prompt("Digite a observação", "", lits[0].Observacoes, false, "", true);
                    foreach (var t in lits)
                    {

                        t.SetObservacoes(qtd);
                    }
            }
        }



        private void set_arredondar1(object sender, RoutedEventArgs e)
        {
            var lits = Rad_Lista_Pecas.SelectedItems.Cast<PecaDB>().ToList();

                foreach (var t in lits)
                {
                    t.SetArredondar(true);
                }
        }

        private void set_arredondar2(object sender, RoutedEventArgs e)
        {
            var lits = Rad_Lista_Pecas.SelectedItems.Cast<PecaDB>().ToList();
            foreach (var t in lits)
            {

                t.SetArredondar(false);
            }
        }

        private void set_ativar1(object sender, RoutedEventArgs e)
        {
            var lits = Rad_Lista_Pecas.SelectedItems.Cast<PecaDB>().ToList();

                foreach (var t in lits)
                {
                    t.SetAtivo(true);
                }
        }

        private void setativo2(object sender, RoutedEventArgs e)
        {
            var lits = Rad_Lista_Pecas.SelectedItems.Cast<PecaDB>().ToList();

                foreach (var t in lits)
                {

                    t.SetAtivo(false);
                }
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            Conexoes.ControleWait w = Conexoes.Utilz.Wait(10, "Aguarde...");
            w.somaProgresso();
            var ss = DBases.GetDbOrc().GetProdutos().FindAll(x => x.PecasDB.Count == 0);
            w.Close();
            Conexoes.Utilz.ShowReports(ss.Select(x => new Report(x.id.ToString(), x.Chave)).ToList());
        }



        private void editar_comprimento(object sender, RoutedEventArgs e)
        {
            var lits = Rad_Lista_Pecas.SelectedItems.Cast<PecaDB>().ToList();
            if (lits.Count > 0)
            {
                string qtd = Conexoes.Utilz.Prompt("Digite o comprimento", "", lits[0].Comprimento.ToString());
    
                    foreach (var t in lits)
                    {
                        t.SetComprimento(Conexoes.Utilz.Double(qtd));
                    }
                
            }
        }


        //private static Dados Dbase { get; set; }
        private void troca_filtro(object sender, RoutedEventArgs e)
        {
            setFiltro();

        }

        private List<PecaDB> Pecas { get; set; } = new List<PecaDB>();
        public List<Grupo_De_Mercadoria> Grupos_De_Mercadoria { get; set; } = new List<Grupo_De_Mercadoria>();
        //private List<Grupo> Grupos { get; set; } = new List<Grupo>();
        private void setFiltro()
        {
            try
            {
                lista_esquerda.Visibility = Visibility.Collapsed;
                Edicao.Visibility = Visibility.Collapsed;
                edicao_completa.Visibility = Visibility.Collapsed;
                Rad_Lista_Pecas.ItemsSource = null;

                Produto_Selecionado = null;
                Rad_Lista_Pecas.IsEnabled = false;
                adicionar_item.Visibility = Visibility.Visible;
                if (DBases.GetDbOrc() == null)
                {
                    DBases.SetDbOrc(new PGO_Dados());

                }
                //Orcamento.Variaveis.DbOrc.GetProdutos(true);
                var lista_n = DBases.GetDbOrc().GetGrupos_De_Mercadoria();

                //if ((bool)ver_arvore.IsChecked)
                //{
                //    Dbase.GetProdutos(false);
                //}
                //else
                //{
                //    Dbase.GetProdutos(true);
                //}

                Grupos_De_Mercadoria = new List<Grupo_De_Mercadoria>();


                if ((bool)Tudo.IsChecked)
                {
                    Produtos = DBases.GetDbOrc().GetProdutos();
                    Pecas = DBases.GetDbOrc().GetProdutos().SelectMany(x => x.PecasDB).ToList();
                    //Grupos = new List<Grupo>();
                    Grupos_De_Mercadoria.AddRange(lista_n);
                    foreach (var t in Grupos_De_Mercadoria)
                    {
                        t.GetAllProdutos();
                    }

                    Lista_Produtos.ItemsSource = Grupos_De_Mercadoria;
                    Lista_Produtos.IsEnabled = true;
                    Importar.Visibility = Visibility.Visible;
                    ferts.Visibility = Visibility.Visible;

                }
                else if ((bool)Sem_Materiais.IsChecked)
                {
                    List<string> itens = GetItensPeca();
                    Produtos = DBases.GetDbOrc().GetProdutos().FindAll(x => x.PecasDB.Count == 0).ToList();
                    Pecas = new List<PecaDB>();
                    Alimenta_Arvore(lista_n);

                }
                else if ((bool)Fert_em_branco.IsChecked)
                {
                    List<string> itens = GetItensPeca();
                    Produtos = DBases.GetDbOrc().GetProdutos().FindAll(x => x.FERT == "").ToList();
                    Pecas = new List<PecaDB>();
                    Alimenta_Arvore(lista_n);


                }
                else if ((bool)Falta_Verificar.IsChecked)
                {
                    List<string> itens = GetItensPeca();
                    Produtos = DBases.GetDbOrc().GetProdutos().FindAll(x => !x.Verificado).ToList();
                    Pecas = new List<PecaDB>();
                    Alimenta_Arvore(lista_n);


                }
                else if ((bool)Verificado.IsChecked)
                {
                    List<string> itens = GetItensPeca();
                    Produtos = DBases.GetDbOrc().GetProdutos().FindAll(x => x.Verificado).ToList();
                    Pecas = new List<PecaDB>();
                    Alimenta_Arvore(lista_n);
                }
                else if ((bool)Falta_Enviar_SAP.IsChecked)
                {
                    List<string> itens = GetItensPeca();
                    Produtos = DBases.GetDbOrc().GetProdutos().FindAll(x => !x.Enviado_SAP).ToList();
                    Alimenta_Arvore(lista_n);


                }
                else if ((bool)Enviado_SAP.IsChecked)
                {
                    List<string> itens = GetItensPeca();
                    Produtos = DBases.GetDbOrc().GetProdutos().FindAll(x => x.Enviado_SAP).ToList();
                    Alimenta_Arvore(lista_n);
                }
                else if ((bool)Material_Invalido.IsChecked)
                {
                    List<string> itens = GetItensPeca();
                    Produtos = DBases.GetDbOrc().GetProdutos().FindAll(x => x.PecasDB.FindAll(y => y.id_peca <= 0).Count > 0).ToList();
                    Alimenta_Arvore(lista_n);
                }
             
                else if ((bool)Um_Item.IsChecked)
                {
                    List<string> itens = GetItensPeca();
                    itens = itens.Distinct().ToList();
                    string sel = Conexoes.Utilz.Selecao.SelecionarObjeto(itens, null);


                    if (sel != null)
                    {
                        var ch = sel.Split('@');
                        Pecas = DBases.GetDbOrc().GetProdutos().SelectMany(x => x.PecasDB).ToList().FindAll(x => x.id_peca == Conexoes.Utilz.Int(ch[0]) && x.Tipo == ch[1].ToString());
                        Rad_Lista_Pecas.ItemsSource = Pecas;
                        Rad_Lista_Pecas.IsEnabled = true;
                    }
                    Lista_Produtos.IsEnabled = false;
                    Lista_Produtos.ItemsSource = null;
                    ferts.Visibility = Visibility.Collapsed;
                    adicionar_item.Visibility = Visibility.Collapsed;
                    Importar.Visibility = Visibility.Collapsed;
                    Edicao.Visibility = Visibility.Visible;


                }

                if ((bool)ver_arvore.IsChecked)
                {
                    fert_grid.IsVisible = false;
                    grupo_grid.IsVisible = false;
                    nome_grid.IsVisible = false;
                    unidade_grid.IsVisible = false;
                    titulo_range.Visibility = Visibility.Visible;
                    adicionar_item.Visibility = Visibility.Visible;
                    edicao_completa.Visibility = Visibility.Visible;
                    lista_esquerda.Visibility = Visibility.Visible;
                    ferts.Visibility = Visibility.Visible;

                }
                else
                {
                    if ((bool)Tudo.IsChecked)
                    {
                        Rad_Lista_Pecas.ItemsSource = Pecas;
                    }
                    fert_grid.IsVisible = true;
                    grupo_grid.IsVisible = true;
                    nome_grid.IsVisible = true;
                    unidade_grid.IsVisible = true;
                    container_memoriais.Children.Clear();


                    titulo_range.Visibility = Visibility.Collapsed;
                    ferts.Visibility = Visibility.Collapsed;
                    adicionar_item.Visibility = Visibility.Collapsed;
                    Importar.Visibility = Visibility.Collapsed;
                    Edicao.Visibility = Visibility.Visible;
                    edicao_completa.Visibility = Visibility.Collapsed;
                    lista_esquerda.Visibility = Visibility.Collapsed;
                    ferts.Visibility = Visibility.Collapsed;
                    Rad_Lista_Pecas.IsEnabled = true;

                }

                this.Status.Content = "[ " + Grupos_De_Mercadoria.Count + " Grupos - " + Grupos_De_Mercadoria.Sum(x => x.Produtos.Count()) + " Produtos ] ";
            }
            catch (Exception)
            {
                //Conexoes.Utilz.Alerta(ex.Message  + "\n" + ex.StackTrace );
                //throw;
            }

        }

        private void Alimenta_Arvore(List<Grupo_De_Mercadoria> lista_n)
        {
            var ts = Produtos.Select(x => x.Grupo_De_Mercadoria.id).ToList().Distinct().ToList();
            foreach (var t in ts)
            {
                var grp = lista_n.Find(x => x.id == t);
                if (grp != null)
                {
                    grp.SetProdutos(Produtos.FindAll(x => x.Grupo_De_Mercadoria.id == t));
                    Grupos_De_Mercadoria.Add(grp);
                }

            }
            Lista_Produtos.ItemsSource = Grupos_De_Mercadoria;
            Lista_Produtos.IsEnabled = true;
            Importar.Visibility = Visibility.Visible;
            ferts.Visibility = Visibility.Visible;
        }

        List<Produto> Produtos = new List<Produto>();
        private static List<string> GetItensPeca()
        {
            List<string> itens = new List<string>();
            Conexoes.ControleWait w = Conexoes.Utilz.Wait(DBases.GetDbOrc().GetProdutos().Count(), "Mapeando Peças...");
            foreach (var p in DBases.GetDbOrc().GetProdutos())
            {
                foreach (var x in p.PecasDB)
                {
                    itens.Add(x.id_peca + "@" + x.Tipo + "@" + x.Descricao);
                }
                w.somaProgresso();
            }
            w.Close();
            return itens;
        }


        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            List<PecaDB> sel = Rad_Lista_Pecas.SelectedItems.Cast<PecaDB>().ToList();
            if (sel.Count > 0)
            {
                Adiciona_Peca mm = new Adiciona_Peca(new PecaDB(), null);
                mm.ShowDialog();
                if (mm.DialogResult.HasValue && mm.DialogResult.Value)
                {
                    bool manter_qtd = Conexoes.Utilz.Pergunta("Manter quantidades? (se clicar em não, será substituído pelo valor inserido na tela anterior");
                    bool manter_observacoes = Conexoes.Utilz.Pergunta("Manter observações? (se clicar em não, será substituído pelo valor inserido na tela anterior");
                    bool manter_bobina = Conexoes.Utilz.Pergunta("Manter bobina? (se clicar em não, será substituído pelo valor inserido na tela anterior");
                    if (Conexoes.Utilz.Pergunta("Tem certeza que deseja alterar os " + sel.Count + " para " + mm.Selecao.Peca.Descricao + "?"))
                    {
                        Conexoes.ControleWait w = Conexoes.Utilz.Wait(sel.Count, "Atualizando");
                        foreach (var t in sel)
                        {
                            t.Transformar(mm.Selecao.Peca);
                            if (!manter_qtd)
                            {
                                t.Quantidade = mm.Selecao.Peca.Quantidade;

                            }
                            if (!manter_observacoes)
                            {
                                t.Observacoes = mm.Selecao.Peca.Observacoes;

                            }

                            if (!manter_bobina)
                            {



                                t.Bobina = mm.Selecao.Peca.Bobina;

                            }
                            t.Gravar();
                            w.somaProgresso();
                        }
                        w.Close();
                    }

                }
            }

        }

        private void Importar_itens(object sender, RoutedEventArgs e)
        {



            var p = Conexoes.Utilz.Selecao.SelecionarObjeto(DBases.GetDbOrc().GetProdutos(), null);
            if (p != null)
            {
                if (Conexoes.Utilz.Pergunta("Tem certeza que deseja importar os itens do range " + p.Chave + " para o range \n" + Produto_Selecionado.Chave))
                {
                    foreach (var t in p.PecasDB)
                    {
                        Produto_Selecionado.AddPeca(t);
                    }
                    Rad_Lista_Pecas.ItemsSource = null;
                    Rad_Lista_Pecas.ItemsSource = Produto_Selecionado.PecasDB;
                }
            }
        }

        private void simular(object sender, RoutedEventArgs e)
        {
            if (Produto_Selecionado == null) { return; }
            double qtd = Conexoes.Utilz.Double(Conexoes.Utilz.Prompt("Digite um valor (" + Produto_Selecionado.unidade + ")", Produto_Selecionado.Chave, Produto_Selecionado.Range.Quantidade.ToString()));
            if (qtd > 0)
            {
                Produto_Selecionado.Range.Quantidade = qtd;
                var prods = new List<Produto> { Produto_Selecionado };

                Produto_Selecionado.getPecasRM();
                ExplorerPLM.Utilidades.MostrarLista(prods, Produto_Selecionado.Chave + " Quantidade: " + qtd + " " + Produto_Selecionado.unidade);
            }
        }

        private void duplica_pcs(object sender, RoutedEventArgs e)
        {
            List<PecaDB> lista = Rad_Lista_Pecas.SelectedItems.Cast<PecaDB>().ToList();

            int c = 1;
            foreach (var t in lista)
            {
                Adiciona_Peca mm = new Adiciona_Peca(new PecaDB().Transformar(t), Produto_Selecionado);
                mm.Title = "Duplicar " + c + "/" + lista.Count + " - " + mm.Selecao.Peca.Descricao;
                mm.ShowDialog();
                if (mm.DialogResult.HasValue && mm.DialogResult.Value)
                {
                    if (mm.Selecao.Peca.id_peca > 0)
                    {

                        mm.Selecao.Produto.AddPeca(mm.Selecao.Peca);
                        Rad_Lista_Pecas.ItemsSource = null;
                        Rad_Lista_Pecas.ItemsSource = Produto_Selecionado.PecasDB;
                    }
                }
                c++;
            }

        }


        private void get_pecas_por_data(object sender, RoutedEventArgs e)
        {
            ControleWait w = Conexoes.Utilz.Wait(DBases.GetDbOrc().GetProdutos().Count, "Consultando...");
            Produtos_Uso.Clear();
            Produtos_Uso = DBases.GetDbOrc().GetUso((DateTime)DataDe.SelectedDate, (DateTime)DataAte.SelectedDate);
            this.Lista_Uso.ItemsSource = null;
            this.Lista_Uso.ItemsSource = Produtos_Uso;
            w.Close();
        }
        public List<Produto> Produtos_Uso { get; set; } = new List<Produto>();

        private void ModernWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DateTime agr = DateTime.Now;
            DataDe.SelectedDate = agr.AddMonths(-1);
            DataAte.SelectedDate = agr.AddMonths(1);
            DataDeIndicador.SelectedDate = agr.AddMonths(-1);
            DataAteIndicador.SelectedDate = agr.AddMonths(1);
        }

        private void exportar_report_uso(object sender, RoutedEventArgs e)
        {
            ExplorerPLM.Utilidades.Exportar(Lista_Uso);
        }

        private void set_forcar_peso1(object sender, RoutedEventArgs e)
        {
            var lits = Rad_Lista_Pecas.SelectedItems.Cast<PecaDB>().ToList();

            var com_mais_de_um = lits.FindAll(x => x.Produto.PecasDB.Count > 1);
            if (com_mais_de_um.Count > 0)
            {
                Conexoes.Utilz.Alerta("Não é possível aplicar o forçar peso em ranges que contenham mais de uma peça, pois o forçar peso preenche o peso total do range na peça.", "Abortado", MessageBoxImage.Error);

                return;
            }

                foreach (var t in lits)
                {
                    t.SetForcarPeso(true);
                }
        }

        private void set_forcar_peso2(object sender, RoutedEventArgs e)
        {
            var lits = Rad_Lista_Pecas.SelectedItems.Cast<PecaDB>().ToList();

                foreach (var t in lits)
                {
                    t.SetForcarPeso(false);
                }
        }

        private DateTime De { get; set; } = Conexoes.Utilz.Calendario.DataDummy();
        private DateTime Ate { get; set; } = Conexoes.Utilz.Calendario.DataDummy();
        public Resumo Resumo { get; set; } = new Resumo();
        private void get_indicadores(object sender, RoutedEventArgs e)
        {
            var De_Sel = (DateTime)DataDeIndicador.SelectedDate;
            var Ate_Sel = (DateTime)DataAteIndicador.SelectedDate;
            if (Resumo.Lista.Count == 0 | ((De_Sel.ToShortDateString() + " - " + Ate_Sel.ToShortDateString()) != Resumo.ToString()))
            {
                De = De_Sel;
                Ate = Ate_Sel;

                Resumo = new Resumo(De, Ate);

            }


            if (Selecao.SelectedItem == PorSetor)
            {


                CategoricalAxis catAxis = new CategoricalAxis();
                LinearAxis lineAxis = new LinearAxis();
                grafico.HorizontalAxis = catAxis;
                grafico.VerticalAxis = lineAxis;
                lineAxis.LabelFormat = "###.###.##,##";
                lineAxis.ShowLabels = true;
                BarSeries barSeries = new BarSeries();
                foreach (var t in Resumo.PesoPorSetorAtividade())
                {
                    barSeries.DataPoints.Add(new CategoricalDataPoint() { Category = t.Item1, Value = t.Item2 });
                }

                grafico.Series.Clear();
                grafico.Series.Add(barSeries);


            }
            else if (Selecao.SelectedItem == PorValor)
            {
                CategoricalAxis catAxis = new CategoricalAxis();
                LinearAxis lineAxis = new LinearAxis();
                grafico.HorizontalAxis = catAxis;
                grafico.VerticalAxis = lineAxis;
                lineAxis.LabelFormat = "R$ #######,##";
                lineAxis.ShowLabels = true;
                BarSeries barSeries = new BarSeries();
                foreach (var t in Resumo.ValorPorSetorAtividade())
                {
                    barSeries.DataPoints.Add(new CategoricalDataPoint() { Category = t.Item1, Value = t.Item2 });
                }

                grafico.Series.Clear();
                grafico.Series.Add(barSeries);

            }
        }



        private void salvar_produto(object sender, RoutedEventArgs e)
        {
                Produto_Selecionado.Observacoes = Input.Text;
                Produto_Selecionado.Salvar(true);
        }

        private void editar_ficha(object sender, RoutedEventArgs e)
        {
            var lits = Rad_Lista_Pecas.SelectedItems.Cast<PecaDB>().ToList();
            if (lits.Count > 0)
            {
                string ficha = Conexoes.Utilz.Selecao.SelecionarObjeto(new its.TipoPintura().GetValues().ToList().Select(x => x.Value.ToString()).ToList(), null);
                if (ficha == null) { return; }
                    foreach (var t in lits)
                    {

                        t.SetFicha(ficha);
                    }
            }
        }

        private void set_forcar_area_pintura(object sender, RoutedEventArgs e)
        {
            var lits = Rad_Lista_Pecas.SelectedItems.Cast<PecaDB>().ToList();

                foreach (var t in lits)
                {
                    t.Set_Forcar_Area_Pintura(true);
                }
        }

        private void set_forcar_area_pintura_false(object sender, RoutedEventArgs e)
        {
            var lits = Rad_Lista_Pecas.SelectedItems.Cast<PecaDB>().ToList();

                foreach (var t in lits)
                {
                    t.Set_Forcar_Area_Pintura(false);
                }
        }

        private void area_x_kilo_valor(object sender, RoutedEventArgs e)
        {
            var lits = Rad_Lista_Pecas.SelectedItems.Cast<PecaDB>().ToList();

            if (lits.Count == 0) { return; }

            double valor = Conexoes.Utilz.Double(Conexoes.Utilz.Prompt("Digite o valor", "", lits[0].Area_x_Kilo.ToString()));

                foreach (var t in lits)
                {
                    t.Set_Area_x_Kilo(valor);
                }
        }

        private void abre_pecas(object sender, RoutedEventArgs e)
        {
            var lits = Rad_Lista_Pecas.SelectedItems.Cast<PecaDB>().ToList();

            var pdfs = lits.Select(x => x.Peca.GetPDF()).Distinct().ToList();
            foreach (var t in pdfs.FindAll(x => File.Exists(x)))
            {
                Conexoes.Utilz.Abrir(t);
            }
        }

        private void propriedades_pecas(object sender, RoutedEventArgs e)
        {
            var lits = Rad_Lista_Pecas.SelectedItems.Cast<PecaDB>().ToList();

            foreach (var L in lits)
            {
                Conexoes.Utilz.Propriedades(L);
            }
        }

        private void alterar_grupo_de_Mercadoria(object sender, RoutedEventArgs e)
        {
            if (Produto_Selecionado == null) { return; }
            else
            {
                var t = Conexoes.Utilz.Selecao.SelecionarObjeto(DBases.GetDbOrc().GetGrupos_De_Mercadoria(), null);
                if (t != null)
                {
                    if (t.id != Produto_Selecionado.Grupo_De_Mercadoria.id)
                    {
                            Produto_Selecionado.setGrupo_De_Mercadoria(t);
                            setFiltro();
                    }
                }

            }
        }

        private void MenuItem_Click_6(object sender, RoutedEventArgs e)
        {
            var t = Conexoes.Utilz.SalvarArquivo("xlsx");
            if (t == "") { return; }
        retentar:
            if (File.Exists(t))
            {
                try
                {
                    File.Delete(t);
                }
                catch (Exception ex)
                {
                    DLM.log.Log(ex);
                    if (Conexoes.Utilz.Pergunta("Não foi possível substituir o arquivo. Tentar novamente?\n" + ex.Message))
                    {
                        goto retentar;
                    }
                    return;

                }
            }
            List<List<string>> retorno = new List<List<string>>();
            retorno.Add(new List<string> { "id_banco", "Grupo", "Produdo", "Grupo Logístico", "Unidade", "Custo_MP", "Overhead", "Unidade" });
            retorno.AddRange(this.Produtos.Select(x => new List<string> { x.id.ToString(), x.Grupo_De_Mercadoria.descricao.ToString(), x.produtos, x.Grupo_Logistico.ToString(), x.unidade, x.custo_mp.ToString(), x.overhead.ToString(), x.unidade }));
            Conexoes.Utilz.Excel.CriarPlanilha(t, "RANGES", retorno);

        }

        private void ativar_selecao(object sender, RoutedEventArgs e)
        {

            setavivos(true);
        }

        private static void setavivos(bool acao)
        {
            var t = Conexoes.Utilz.Selecao.SelecionarObjetos(DBases.GetDbOrc().GetProdutos().FindAll(x => x.ativo != acao).ToList());
            if (t.Count > 0)
            {
                foreach (var s in t)
                {
                    s.setativo(acao);
                }
            }
        }

        private void desativar_lista(object sender, RoutedEventArgs e)
        {
            setavivos(false);

        }

        private void alterar_fert(object sender, RoutedEventArgs e)
        {
            if (Produto_Selecionado == null) { return; }
            else
            {
                var fert = Conexoes.Utilz.Selecao.SelecionarObjeto(DBases.GetDbOrc().Get_PEP_FERT(), null, "Selecione");
                if (fert != null)
                {
                    Produto_Selecionado.setFERT(fert.FERT, fert.WERKS.Int());
                }
            }
        }
    }
}
