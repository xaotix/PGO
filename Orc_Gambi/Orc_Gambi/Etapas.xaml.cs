using Conexoes;
using DLM.encoder;
using DLM.vars;
using DLM.orc;
using ExplorerPLM.Menus;
using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PGO
{
    /// <summary>
    /// Interaction logic for Etapas.xaml
    /// </summary>
    public partial class Etapas : ModernWindow
    {
        public DLM.orc.PGO_Obra Obra { get; set; } = new DLM.orc.PGO_Obra();
        public int etapas { get; set; } = 1;
        public int etapa_inicial { get; set; } = 1;
        public Etapas(DLM.orc.PGO_Obra Obra)
        {
            this.Obra = Obra;

            InitializeComponent();
            this.DataContext = this;
            this.Title = "Etapas [" + Obra.ToString() + "]";
            var Produtos = DBases.GetDbOrc().GetProdutos();
            var Pecas = DBases.GetDbOrc().GetProdutos().SelectMany(x => x.PecasDB).ToList();

            this.Obra.GetPredios(true);
            Conexoes.Utilz.GetPecas_Orcamento(this.Obra, true, this.Obra.GetRanges().GroupBy(x => x.id).Select(x => x.First()).ToList(), true, true);
            UpdateAll();

        }
        private void UpdateAll()
        {

            Conexoes.ControleWait w = Conexoes.Utilz.Wait(16, "Atualizando Listas...");
            w.somaProgresso();
            try
            {

                this.Lista_Etapas.ItemsSource = null;
                this.Rad_Lista_Ranges.ItemsSource = null;
                this.Rad_Lista_SubEtapas_Selecao.ItemsSource = null;
                this.Rad_Lista_Ponderadores.ItemsSource = null;
                this.lista_sub_etapas_selecao2.ItemsSource = null;
                this.lista_predios.ItemsSource = null;
                this.Rad_Lista_Subetapas_Fim.ItemsSource = null;
                this.Lista_PEPs.ItemsSource = null;
                this.lista_subetapas.ItemsSource = null;
                this.Rad_Lista_Predios_2.ItemsSource = null;
                this.Rad_Lista_Predios_3.ItemsSource = null;
                this.lista_predios_peps.ItemsSource = null;

                w.somaProgresso("Mapeando Ranges...");
                this.Rad_Lista_Ranges.ItemsSource = this.Obra.GetRanges();

              
                this.Obra.GetEtapas(true);
             

                /*validando se tem etapa criada*/
                if (this.Obra.GetRanges().Count == 0)
                {
                    this.tab_principal_etapas.Visibility = Visibility.Collapsed;
                    Conexoes.Utilz.Alerta("Obra sem ranges cadastrados. Não é possível criar etapas.");
                    w.Close();
                    return;
                }
                else
                {
                    this.tab_principal_etapas.Visibility = Visibility.Visible;
                }

                w.somaProgresso("Mapeando Etapas...");
                this.Lista_Etapas.ItemsSource = Obra.GetEtapas();

                w.somaProgresso("Mapeando Sub-Ponderadores...");
                this.Rad_Lista_Ponderadores.ItemsSource = this.Obra.Getsubponderadores().FindAll(x=>x.valor>0);



                w.somaProgresso("Mapeando Agrupadores...");
                foreach (var pr in this.Obra.GetPredios()) 
                {
                    pr.CarregarAgrupadores(true);
                }


                w.somaProgresso("Mapeando Agrupadores de PEPs...1/4");
                this.Lista_PEPs.ItemsSource = this.Obra.Getpep_agrupadores();

                w.somaProgresso("Mapeando Agrupadores de PEPs...2/4");
                var agrupadores = this.Obra.Getpep_agrupadores().FindAll(x => x.GetFerts().Count > 0);

                w.somaProgresso("Mapeando Agrupadores de PEPs...3/4");
                this.lista_subetapas.ItemsSource = this.Obra.Getpep_agrupadores().FindAll(x => x.GetFerts().Count > 0);

                w.somaProgresso("Mapeando Agrupadores de PEPs...4/4");
                this.lista_predios_peps.ItemsSource = this.Obra.GetPredios().ToList().FindAll(x => x.GetAgrupadores().Count > 0);



                w.somaProgresso("Mapeando Peças...1/3");
                this.lista_predios.ItemsSource = this.Obra.GetPredios();

                w.somaProgresso("Mapeando Peças...2/3");
                var predios_edicao = this.Obra.GetPredios().ToList().FindAll(x => x.GetPonderadores().Count > 0);

                this.Rad_Lista_Predios_2.ItemsSource = predios_edicao;
                w.somaProgresso("Mapeando Peças...3/3");
                this.Rad_Lista_Predios_3.ItemsSource = predios_edicao;

                w.somaProgresso("Mapeando Sub-Etapas e ordenando...");
                this.Rad_Lista_Subetapas_Fim.ItemsSource = this.Obra.GetEtapas().SelectMany(x => x.Getsubetapas_ordenadas()).ToList();

                w.somaProgresso("Mapeando peças...");
                this.Obra.GetPecasEtapas();


                if (predios_edicao.Count > 0)
                {
                    foreach (var predio in predios_edicao)
                    {
                        this.Rad_Lista_Predios_3.SelectedItems.Add(predio);
                        this.Rad_Lista_Predios_2.SelectedItems.Add(predio);
                    }

                }



                w.somaProgresso("Validando...");
                if (this.Obra.GetEtapas().Count == 0)
                {
                    this.tab_principal_etapas.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.tab_principal_etapas.Visibility = Visibility.Visible;
                }
                var sem_fert = this.Obra.GetRanges().ToList().FindAll(x => x.De_Para.FERT == "00");
                if (sem_fert.Count > 0)
                {
                    this.tab_principal_etapas.Visibility = Visibility.Collapsed;
                    List<Report> Reports = new List<Report>();
                    Reports.AddRange(sem_fert.Select(x => new Report("Falta definir o FERT", x.ToString(), TipoReport.Crítico)).ToList());
                    Conexoes.Utilz.ShowReports(Reports);
                }
                else
                {
                    this.tab_principal_etapas.Visibility = Visibility.Visible;
                }
                w.somaProgresso();

                ExplorerPLM.Utilidades.LimparFiltros(Rad_Lista_Ranges);
                ExplorerPLM.Utilidades.LimparFiltros(Rad_Lista_SubEtapas_Selecao);
                ExplorerPLM.Utilidades.LimparFiltros(lista_sub_etapas_selecao2);
                ExplorerPLM.Utilidades.LimparFiltros(Rad_Lista_Subetapas_Fim);
                ExplorerPLM.Utilidades.LimparFiltros(lista_subetapas);
                ExplorerPLM.Utilidades.LimparFiltros(Rad_Lista_Predios_2);
                ExplorerPLM.Utilidades.LimparFiltros(Rad_Lista_Predios_3);
                //ExplorerPLM.Utilidades.LimparFiltros(lista_predios_peps);

                w.somaProgresso();

            }
            catch (Exception)
            {

            }
            w.Close();

            var peso_total = Math.Round(this.Obra.GetRanges().Sum(x => x.PesoTotal),2);
            var peso_pecas = Math.Round(this.Obra.GetRanges().Sum(x => x.PesoPecas),2);

            if(peso_total==0)
            {
                Conexoes.Utilz.Alerta("O peso todal dos ranges está zerado. Não é possível criar etapas.");
                this.Close();
                return;
            }
            if(peso_pecas==0)
            {
                Conexoes.Utilz.Alerta("O peso total das peças (listas técnicas) está zerado. Não é possível criar etapas.");
                this.tab_principal_etapas.Visibility = Visibility.Collapsed;
                return;
            }
            else
            {
            var dif = 100-Math.Abs(Math.Round(peso_pecas * 100 / peso_total,2));
                if(dif>3)
                {
                    Conexoes.Utilz.Alerta($"O peso dos Ranges está divergente mais de {dif}% do peso das Peças. Verifique." +
                        $"\nPeso da soma dos Ranges:{peso_total}\n" +
                        $"Peso da soma das Peças: {peso_pecas}\n" +
                        $"Isso pode ser causado por problemas de listas técnicas ou atribuição de código em verbas.\n" +
                        $"Fale com o Orçamentista ou com o responsável pelo cadastro das listas técnicas antes de continuar esse processo.");
                }
            }


            this.resumo.Content = 
                $"Ranges: {this.Obra.Ranges.Sum(x => x.PesoTotal/1000).ToString("0,0.00")} Kg " +
                $"\n Consolidada: {this.Obra.Ranges.Sum(x => x.PesoPecas / 1000).ToString("0,0.00")} Kg" +
                $"\n Etapas: {this.Obra.Getsubponderadores().Sum(x=>x.peso).ToString("0,0.00")} Kg";

        }
        private void RetornaFert(List<DLM.orc.Range> Ranges)
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

                    UpdateAll();

                }

            }
        }
        private void SetFert(List<DLM.orc.Range> Ranges)
        {
            var mts = Ranges.Select(x => x.WERK).Distinct().ToList();
            var ferts = this.Obra.GetFerts_Etapas();
            var sel = Conexoes.Utilz.Selecao.SelecionarObjeto(DBases.GetDbOrc().Get_PEP_FERT()
                , null, "Selecione");
            if (sel != null)
            {
                Conexoes.ControleWait w = Conexoes.Utilz.Wait(Ranges.Count, "Atualizando...");
                foreach (var s in Ranges)
                {
                    s.setFert_User(sel.FERT, sel.WERKS);
                    w.somaProgresso();
                }
                w.Close();
                UpdateAll();
            }
        }
        public void AddEtapa(DLM.orc.PGO_Predio predio)
        {
            this.etapas = Conexoes.Utilz.Int(Conexoes.Utilz.Prompt("Digite o número de etapas que deseja criar", predio.ToString() + "[" + Math.Round(predio.PesoTotal, 2) + "Ton]", Utilz.Int(predio.peso_disponivel / 75).ToString()));

            if (this.etapas == 0) { return; }
            this.etapa_inicial = Conexoes.Utilz.Int(Conexoes.Utilz.Prompt("Iniciar com qual etapa?", predio.ToString() + "[" + Math.Round(predio.PesoTotal, 2) + "Ton]", this.etapa_inicial.ToString()));

            if (this.etapa_inicial == 0) { return; }

            if (this.etapas > 1)
            {
                Conexoes.ControleWait w = Conexoes.Utilz.Wait(etapas + 1, predio.ToString() + "\nAnalisando....");
                w.somaProgresso();
                List<DLM.orc.PGO_Etapa> novas_etapas = new List<DLM.orc.PGO_Etapa>();
                double porcentagem = Math.Round(predio.Saldo_Etapa / etapas, 3);
                for (int i = 0; i < etapas; i++)
                {
                    DLM.orc.PGO_Etapa nova_etapa = new DLM.orc.PGO_Etapa(this.Obra);
                    nova_etapa.descricao = predio.nome;
                    nova_etapa.nome = i + etapa_inicial;
                    if (i < etapas - 1)
                    {
                        nova_etapa.AddPonderador(predio, porcentagem);

                    }
                    else
                    {
                        nova_etapa.AddPonderador(predio, 100 - porcentagem * i);

                    }
                    novas_etapas.Add(nova_etapa);
                    w.somaProgresso();
                }
                w.Close();
                Ponderadores_Determinar po = new Ponderadores_Determinar(novas_etapas.SelectMany(x => x.GetPonderadores()).ToList(), predio);
                po.ShowDialog();
                if ((bool)po.DialogResult)
                {
                    var atual = this.Obra.GetEtapas().Count + 1;
                    w = Conexoes.Utilz.Wait(atual, predio.ToString() + "\nCriando Etapas...");
                    foreach (var et in novas_etapas)
                    {
                        //et.nome = atual;
                        atual++;
                        et.Salvar(true, true);
                        w.somaProgresso();
                    }
                    w.Close();
                    UpdateAll();
                }
            }
            else if (this.etapas == 1)
            {
                DLM.orc.PGO_Etapa pp = new DLM.orc.PGO_Etapa(this.Obra);
                pp.nome = this.Obra.GetEtapas().Count + 1;
                if (predio != null)
                {
                    pp.descricao = predio.nome;
                }
                pp.nome = etapa_inicial;
                //Utilz.Propriedades(pp, true, true);

                this.Obra.AddEtapa(pp);
                if (predio != null)
                {
                    double porcentagem = 100;

                retentar:
                    if (porcentagem > predio.Saldo_Etapa | porcentagem == 0 | porcentagem < 0)
                    {
                        if (Utilz.Pergunta(predio.ToString() + "\nValor digitado é invalido ou maior que o saldo disponível (" + predio.Saldo_Etapa.ToString() + "%) tentar novamente?"))
                        {
                            porcentagem = Conexoes.Utilz.Double(Conexoes.Utilz.Prompt("Digite a porcentagem a ser considerada", predio.ToString(), predio.Saldo_Etapa.ToString()));
                            goto retentar;
                        }
                    }
                    else
                    {
                        pp.AddPonderador(predio, porcentagem, true);

                    }
                }
                UpdateAll();
            }
        }
        private void move_material(DLM.orc.PGO_SubEtapa_Ponderador sel)
        {
            Set100(sel);
            foreach (var ss in sel.Getsubponderadores_externos())
            {
                Set100(ss);
            }

        }
        public void move_materiais(List<DLM.orc.PEP_Agrupador> peps_selecionados)
        {
            if (peps_selecionados.Count > 0)
            {
                var sss = peps_selecionados.SelectMany(x => x.GetFerts()).ToList().GroupBy(x => x.ToString()).Select(x => x.First()).ToList();
                var peps_mover = Conexoes.Utilz.Selecao.SelecionarObjetos(sss, true," => Seleciones os Ferts para mover");
                if (peps_mover.Count == 0) { goto saifora; }

                var lista_peps = DBases.GetDbOrc().Get_PEP_FERT().FindAll(x => peps_selecionados.Find(y => y.PEP == x.PEP && y.fabrica == x.FAB) == null /*&& this.Obra.GetFerts_Etapas().Find(y => y.PEP == x.PEP && y.FAB == x.FAB) == null*/);

                DLM.orc.PGO_PEP_FERT NDE_PARA = new DLM.orc.PGO_PEP_FERT();
                NDE_PARA.DESC = "Criar novo";
                NDE_PARA.FERT = "XX";
                NDE_PARA.MT = "MT2";
                NDE_PARA.PEP = "XXX";

                lista_peps.Insert(0, NDE_PARA);
                bool faturamento = false;
                if (lista_peps.Count > 0)
                {
                    var novo = Conexoes.Utilz.Selecao.SelecionarObjeto(lista_peps, null, "Selecione o destino dos PEPs Selecionados");


                    if (novo != null)
                    {

                   

                        if (novo.PEP == "XXX")
                        {
                        retentar:
                            var npep = Conexoes.Utilz.Prompt("Digite o PEP", "", "30A", true, "pep_pgo", false, 3);
                            if (npep.Length != 3)
                            {
                                goto saifora;
                            }
                            var ndesc = Conexoes.Utilz.Prompt("Digite a descrição", "", "", true, "pep_pgo_desc", false, 20);
                            var fabs = peps_selecionados.Select(x => x.fabrica).Distinct().ToList();
                            string mt = DBases.GetDbOrc().GetMTs().ListaSelecionar();
                            if (mt == null)
                            {
                                goto saifora;
                            }
                            var fab = mt.Replace("MT", "120");

                            faturamento = Conexoes.Utilz.Pergunta("Forçar peças a usarem mesma unidade de fabricação que a de faturamento?");

                            novo.PEP = npep;
                            novo.DESC = ndesc;
                            novo.MT = mt;
                            if (DBases.GetDbOrc().Get_PEP_FERT().Find(x => x.PEP == npep && x.FAB == fab) != null | peps_selecionados.SelectMany(x => x.Getpep_agrupadores_fora()).ToList().Find(x => x.PEP == npep && x.fabrica == fab) != null)
                            {
                                if (Conexoes.Utilz.Pergunta("Já existe um PEP com este nome. Tentar novamente?"))
                                {
                                    goto retentar;
                                }
                                goto saifora;
                            }

                        }
                    
                    
                        foreach (var agrupador in peps_selecionados)
                        {
                            agrupador.AddSubPonderador(novo.PEP, novo.WERKS, null, faturamento);
                        }
                        UpdateAll();
                        var criado = this.Obra.Getpep_agrupadores().Find(x => x.PEP == novo.PEP && x.fabrica == novo.FAB);
                        if (criado != null)
                        {
                            foreach (var s in peps_mover)
                            {
                                criado.moveFert(s.FERT, s.WERKS, faturamento);
                            }
                            UpdateAll();
                        }
                    }
                }
            }
        saifora:
            return;
        }
        private void move_sub_etapas(DLM.orc.SubEtapa_Agrupador destino, List<DLM.orc.SubEtapa_Agrupador> selecao)
        {
            if (selecao.Count > 0)
            {
                destino.PegarSubPonderador(selecao.SelectMany(x => x.GetSubPonderadores()).ToList());

                UpdateAll();

            }
        }
        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var t = tabela.SelectedItem;
            if (t is TabItem)
            {
                var s = t as TabItem;

            }
        }
        private void editar_fert_tab_fert(object sender, RoutedEventArgs e)
        {
            DLM.orc.Range sel = ((FrameworkElement)sender).DataContext as DLM.orc.Range;
            if (sel == null) { return; }
            SetFert(new List<DLM.orc.Range> { sel });
        }
        private void volta_fert_normal(object sender, RoutedEventArgs e)
        {
            DLM.orc.Range sel = ((FrameworkElement)sender).DataContext as DLM.orc.Range;
            if (sel == null) { return; }
            RetornaFert(new List<DLM.orc.Range> { sel });
        }
        private void TabControl_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            //UpdateLista();
        }
        private void altera_fert_varios(object sender, RoutedEventArgs e)
        {
            SetFert(Rad_Lista_Ranges.SelectedItems.Cast<DLM.orc.Range>().ToList());
        }
        private void reset_fert_varios(object sender, RoutedEventArgs e)
        {

            RetornaFert(Rad_Lista_Ranges.SelectedItems.Cast<DLM.orc.Range>().ToList());
        }
        private void add_novo_pep(object sender, RoutedEventArgs e)
        {
            var peps_selecionados = Lista_PEPs.Selecao<DLM.orc.PEP_Agrupador>().FindAll(x => x.GetFerts().Count > 0);
            move_materiais(peps_selecionados);
        }
        private void edit_nome_pep(object sender, RoutedEventArgs e)
        {
            object sel = ((FrameworkElement)sender).DataContext;
            DLM.orc.PEP_Agrupador pp = null;
            if (sel is DLM.orc.PEP_Agrupador)
            {
                pp = sel as DLM.orc.PEP_Agrupador;
            }
            else if (sel is DLM.orc.SubEtapa_Agrupador)
            {
                DLM.orc.SubEtapa_Agrupador sel2 = ((FrameworkElement)sender).DataContext as DLM.orc.SubEtapa_Agrupador;
                pp = sel2.agrupador;
            }
            if (pp != null)
            {
                var novo = Conexoes.Utilz.Prompt("Digite o nome do PEP", "", pp.PEP, false, "", false, 3).ToUpper();
                if (novo == null | novo == pp.PEP) { return; }
                if (novo.Length != 3) { return; }

                bool faturamento = Utilz.Pergunta("Forçar o mesmo PEP de fabricação que o de faturamento?");


                SetPEP(pp, novo, faturamento);

                UpdateAll();

            }


        }
        private void SetPEP(DLM.orc.PEP_Agrupador sel, string novo, bool faturamento)
        {
            List<DLM.orc.PEP_Agrupador> peps = new List<DLM.orc.PEP_Agrupador>();
            peps.AddRange(sel.GetPEPAgrupadores());
            foreach (var agrups in peps)
            {
                var corr = agrups.Getpep_agrupadores_fora().Find(x => x.PEP == novo && x.werk == sel.werk);
                if (corr != null)
                {
                    foreach (var p in sel.GetFerts())
                    {
                        corr.moveFert(p.FERT, p.WERKS, faturamento);
                    }
                }
                else
                {
                    foreach (var p in sel.Getsubponderadores_sem_filtro())
                    {
                        p.chave = novo;
                        p.Salvar();
                    }
                }
            }
        }
        private void ver_pecas_fim(object sender, RoutedEventArgs e)
        {
            var sels = ((FrameworkElement)sender).DataContext;
            if (sels is DLM.orc.PGO_Etapa)
            {
                var s = sels as DLM.orc.PGO_Etapa;
                PGO.Funcoes.VerMateriais(this.Obra, s.GetPecas());
            }
            else if (sels is DLM.orc.PGO_Subetapa)
            {
                var s = sels as DLM.orc.PGO_Subetapa;
                PGO.Funcoes.VerMateriais(this.Obra, s.GetPecas());
            }
            else if (sels is DLM.orc.PGO_PEP)
            {
                var s = sels as DLM.orc.PGO_PEP;
                PGO.Funcoes.VerMateriais(this.Obra, s.GetPecas());
            }
            else if (sels is DLM.orc.Range)
            {
                var s = sels as DLM.orc.Range;
                PGO.Funcoes.VerMateriais(this.Obra, s.GetPecas());
            }
        }
        private void criar_novo_pep(object sender, RoutedEventArgs e)
        {
            var sels = ((FrameworkElement)sender).DataContext;
            if (sels is DLM.orc.PGO_Predio)
            {
                var ss = sels as DLM.orc.PGO_Predio;
                var peps = Conexoes.Utilz.Selecao.SelecionarObjetos(ss.GetAgrupadores().FindAll(x=>x.GetFerts().Count>0),true,"Selecione os PEPs que contenham os FERTs que deseja mover");
                if (peps.Count > 0)
                {
                    move_materiais(peps);
                }
            }
        }
        private void move_material_subetapas_global(object sender, RoutedEventArgs e)
        {
            var sels = ((FrameworkElement)sender).DataContext;
            if (sels is DLM.orc.PEP_Agrupador)
            {
                var pp = sels as DLM.orc.PEP_Agrupador;
                var sel = Conexoes.Utilz.Selecao.SelecionarObjeto(pp.GetSubEtapaAgrupadores(), null, "Selecione a Etapa");
                if (sel != null)
                {
                    move_sub_etapas(sel, sel.Getagrupadores_externos());
                }
            }
        }
        private void editar_varios(object sender, RoutedEventArgs e)
        {
            var sels = ((FrameworkElement)sender).DataContext;

            if (sels is DLM.orc.SubEtapa_Agrupador)
            {
                DLM.orc.SubEtapa_Agrupador sel = ((FrameworkElement)sender).DataContext as DLM.orc.SubEtapa_Agrupador;
                if (PGO.Funcoes.Editar(sel.agrupador))
                {
                    UpdateAll();
                }

            }
            else if (sels is DLM.orc.SubEtapa_AgrupadorBase)
            {
                DLM.orc.SubEtapa_AgrupadorBase sel = ((FrameworkElement)sender).DataContext as DLM.orc.SubEtapa_AgrupadorBase;
                if (PGO.Funcoes.Editar(sel.agrupador, sel.Getbrothers()))
                {
                    UpdateAll();
                }
            }
        }
        private void ver_pecas_ranges(object sender, RoutedEventArgs e)
        {
            var s = Rad_Lista_Ranges.SelectedItems.Cast<DLM.orc.Range>().ToList();
            PGO.Funcoes.VerMateriais(this.Obra, s.SelectMany(x => x.GetPecas()).ToList());

        }
        private void extrair_relatorio(object sender, RoutedEventArgs e)
        {
            var pcs_range = this.Obra.GetPecasRanges();
            var pcs_etapas = this.Obra.GetPecasEtapas();
            var cods = pcs_etapas.Select(x => x.id_peca + "." + x.Destino).Distinct().ToList();
            cods.AddRange(pcs_range.Select(x => x.id_peca + "." + x.Destino).Distinct().ToList());
            cods = cods.Distinct().ToList();
            List<List<string>> linhas = new List<List<string>>();
            List<string> header = new List<string>();
            header.Add("Marca");
            header.Add("SAP");
            header.Add("Descricao");
            header.Add("Qtd. Range");
            header.Add("Peso Unit. Range");
            header.Add("Peso Tot. Range");

            header.Add("Qtd. Etapa");
            header.Add("Peso Unit. Etapa");
            header.Add("Peso Tot. Etapa");
            linhas.Add(header);
            foreach (var c in cods)
            {
                var ranges = pcs_range.FindAll(x => x.id_peca + "." + x.Destino == c).ToList();
                var etapas = pcs_etapas.FindAll(x => x.id_peca + "." + x.Destino == c).ToList();
                string marca = "";
                string sap = "";
                string descricao = "";
                double peso_unit_etapas = 0;
                double peso_unit_range = 0;
                if (ranges.Count > 0)
                {
                    marca = ranges[0].Codigo;
                    sap = ranges[0].MaterialSAP;
                    descricao = ranges[0].Descricao;
                    peso_unit_range = ranges[0].PesoUnit;

                }
                else if (etapas.Count > 0)
                {
                    marca = etapas[0].Codigo;
                    sap = etapas[0].MaterialSAP;
                    descricao = etapas[0].Descricao;
                    peso_unit_etapas = etapas[0].PesoUnit;

                }
                double peso_ranges = ranges.Sum(x => x.PesoTotal);
                double peso_etapas = etapas.Sum(x => x.PesoTotal);

                double qtd_ranges = ranges.Sum(x => x.Quantidade);
                double qtd_etapas = etapas.Sum(x => x.Quantidade);

                List<string> l = new List<string>();
                l.Add(marca);
                l.Add(sap);
                l.Add(descricao);
                l.Add(qtd_ranges.ToString().Replace(".", ","));
                l.Add(peso_unit_range.ToString().Replace(".", ","));
                l.Add(peso_ranges.ToString().Replace(".", ","));

                l.Add(qtd_etapas.ToString().Replace(".", ","));
                l.Add(peso_unit_etapas.ToString().Replace(".", ","));
                l.Add(peso_etapas.ToString().Replace(".", ","));
                linhas.Add(l);
            }
            Conexoes.Utilz.Arquivo.SalvarLista(linhas);
        }
        private void ver_pecas_selecao_peps(object sender, RoutedEventArgs e)
        {
            var s = Rad_Lista_Subetapas_Fim.SelectedItems.Cast<DLM.orc.PGO_Subetapa>().ToList().SelectMany(x => x.GetPecas()).ToList();
            PGO.Funcoes.VerMateriais(this.Obra, s);
        }
        private void ativar_faturamento(object sender, RoutedEventArgs e)
        {
            var s = Lista_PEPs.Selecao<DLM.orc.PEP_Agrupador>().FindAll(x => x.GetFerts().Count > 0);
            bool ativar = true;
            foreach (var t in s)
            {
                foreach (var p in t.GetSubPonderadores())
                {
                    p.SetFaturamento(ativar);
                }
            }
        }
        private void desativar_faturamento(object sender, RoutedEventArgs e)
        {
            var s = Lista_PEPs.Selecao<DLM.orc.PEP_Agrupador>().FindAll(x => x.GetFerts().Count > 0);
            bool ativar = false;
            foreach (var t in s)
            {
                foreach (var p in t.GetSubPonderadores())
                {
                    p.SetFaturamento(ativar);
                }
            }
        }
        private void resetar_peps(object sender, RoutedEventArgs e)
        {
            var sels = ((FrameworkElement)sender).DataContext;
            if (sels is DLM.orc.PGO_Predio)
            {
                var predio = sels as DLM.orc.PGO_Predio;
                predio.ApagarSubponderadores();
            }
            UpdateAll();
        }
        private void edit_fab_pep(object sender, RoutedEventArgs e)
        {
            DLM.orc.PEP_Agrupador sel = ((FrameworkElement)sender).DataContext as DLM.orc.PEP_Agrupador;
            if (sel != null)
            {
                var novo = DBases.GetDbOrc().GetWERKs().ListaSelecionar();
                if (novo == null) { return; }
                if (novo== sel.werk) { return; }
                bool faturamento = Utilz.Pergunta("Forçar o mesmo PEP de fabricação que o de faturamento?");

                List<DLM.orc.PEP_Agrupador> peps = new List<DLM.orc.PEP_Agrupador>();
                peps.AddRange(sel.GetPEPAgrupadores());
                foreach (var agrups in peps)
                {
                    var corr = agrups.Getpep_agrupadores_fora().Find(x => x.PEP == sel.PEP && x.werk == novo);
                    if (corr != null)
                    {
                        foreach (var p in sel.GetFerts())
                        {
                            corr.moveFert(p.FERT, p.WERKS, faturamento);
                        }
                    }
                    else
                    {
                        foreach (var p in sel.Getsubponderadores_sem_filtro())
                        {
                            p.werk = novo;
                            p.Salvar();
                        }
                    }
                }



                UpdateAll();

            }
        }
        private void reset_peps_sel(object sender, RoutedEventArgs e)
        {
            var predios = lista_predios_peps.Selecao<DLM.orc.PGO_Predio>();
            foreach (DLM.orc.PGO_Predio predio in predios)
            {
                predio.ApagarSubponderadores();
            }
            UpdateAll();

        }
        private void monta_lista(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
        {
            var sel = Rad_Lista_Predios_2.SelectedItems.Cast<DLM.orc.PGO_Predio>().ToList();
            this.Rad_Lista_SubEtapas_Selecao.ItemsSource = null;
            this.Rad_Lista_SubEtapas_Selecao.ItemsSource = sel.SelectMany(x => x.GetSubEtapaAgrupadores());
        }
        private void monta_lista2(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
        {
            var sel = Rad_Lista_Predios_3.SelectedItems.Cast<DLM.orc.PGO_Predio>().ToList();
            this.lista_sub_etapas_selecao2.ItemsSource = null;
            this.lista_sub_etapas_selecao2.ItemsSource = sel.SelectMany(x => x.GetSubEtapaAgrupadoresBase()).OrderBy(x => x.chave);
        }
        private void juntar_subs_subetapa_agrupador(object sender, RoutedEventArgs e)
        {
            var sels = ((FrameworkElement)sender).DataContext;

            if (sels is DLM.orc.SubEtapa_Agrupador)
            {
                var subetapa = sels as DLM.orc.SubEtapa_Agrupador;


                var porcentagens = new DLM.orc.Porcentagem_Grupo(subetapa.agrupador, true);

                var por_sub = porcentagens.Lista.Find(x => x.Objeto == subetapa.GetSubEtapas()[0]);
                if (por_sub != null)
                {
                    por_sub.SetPorcentagem(100);
                }



                if (Conexoes.Utilz.Pergunta("Mover todo o material para a etapa " + subetapa.ToString() + "?"))
                {
                    Utilz.SetPorcentagens(subetapa.agrupador, null, porcentagens);
                    UpdateAll();
                }

            }
            else if (sels is DLM.orc.SubEtapa_AgrupadorBase)
            {
                var subetapa = sels as DLM.orc.SubEtapa_AgrupadorBase;
                var brothers = subetapa.Getbrothers();

                var porcentagens = new DLM.orc.Porcentagem_Grupo(subetapa.agrupador, brothers == null);

                var por_sub = porcentagens.Lista.Find(x => x.Objeto == subetapa.subetapas[0]);
                if (por_sub != null)
                {
                    por_sub.SetPorcentagem(100);
                }



                if (Conexoes.Utilz.Pergunta("Mover todo o material para a etapa " + subetapa.ToString() + "?"))
                {
                    Utilz.SetPorcentagens(subetapa.agrupador, brothers, porcentagens);
                    UpdateAll();
                }
            }
        }
        private void distribuir_igualmente_subs_agrupador(object sender, RoutedEventArgs e)
        {
            var sels = ((FrameworkElement)sender).DataContext;

            if (sels is DLM.orc.SubEtapa_Agrupador)
            {
                var subetapa = sels as DLM.orc.SubEtapa_Agrupador;


                var porcentagens = new DLM.orc.Porcentagem_Grupo(subetapa.agrupador, true);

                porcentagens.Ratear();



                if (Conexoes.Utilz.Pergunta("Salvar Alterações ?"))
                {
                    Utilz.SetPorcentagens(subetapa.agrupador, null, porcentagens);
                    UpdateAll();
                }

            }
            else if (sels is DLM.orc.SubEtapa_AgrupadorBase)
            {
                var subetapa = sels as DLM.orc.SubEtapa_AgrupadorBase;
                var brothers = subetapa.Getbrothers();

                var porcentagens = new DLM.orc.Porcentagem_Grupo(subetapa.agrupador, brothers == null);

                porcentagens.Ratear();

                if (Conexoes.Utilz.Pergunta("Salvar Alterações ?"))
                {
                    Utilz.SetPorcentagens(subetapa.agrupador, brothers, porcentagens);
                    UpdateAll();
                }
            }
        }
        private void adicionar_subs_agrupador(object sender, RoutedEventArgs e)
        {
            var sels = ((FrameworkElement)sender).DataContext;

            if (sels is DLM.orc.SubEtapa_Agrupador)
            {
                var subetapa = sels as DLM.orc.SubEtapa_Agrupador;
                var porcentagens = new DLM.orc.Porcentagem_Grupo(subetapa.agrupador, true);

                var selecao = Conexoes.Utilz.Selecao.SelecionarObjetos(subetapa.Getagrupadores_externos(),true);
                if (selecao.Count == 0) { return; }

              
                if(subetapa.GetSubEtapas().Count==0)
                {
                    return;
                }

                var por_sub = porcentagens.Lista.Find(x => x.Objeto == subetapa.GetSubEtapas()[0]);
                if (por_sub != null)
                {
                    double somar = 0;
                    foreach (var p in selecao)
                    {
                        var por_mov_sub = porcentagens.Lista.Find(x => x.Objeto == p.GetSubEtapas()[0]);
                        if (por_mov_sub != null)
                        {
                            somar = somar + por_mov_sub.Valor;
                            por_mov_sub.SetPorcentagem(0, false);
                        }
                    }
                    if (somar > 0)
                    {
                        por_sub.SetPorcentagem(por_sub.Valor + somar, false);
                    }

                }

                if (Conexoes.Utilz.Pergunta("Salvar Alterações ?"))
                {
                    Utilz.SetPorcentagens(subetapa.agrupador, null, porcentagens);
                    UpdateAll();
                }

            }
            else if (sels is DLM.orc.SubEtapa_AgrupadorBase)
            {
                var subetapa = sels as DLM.orc.SubEtapa_AgrupadorBase;
                var brothers = subetapa.Getbrothers();

                var porcentagens = new DLM.orc.Porcentagem_Grupo(subetapa.agrupador, brothers == null);
                var selecao = Conexoes.Utilz.Selecao.SelecionarObjetos(subetapa.Getagrupadores_externos(), true);
                if (selecao.Count == 0) { return; }

                var por_sub = porcentagens.Lista.Find(x => x.Objeto == subetapa.subetapas[0]);
                if (por_sub != null)
                {
                    double somar = 0;
                    foreach (var p in selecao)
                    {
                        var por_mov_sub = porcentagens.Lista.Find(x => x.Objeto == p.subetapas[0]);
                        if (por_mov_sub != null)
                        {
                            somar = somar + por_mov_sub.Valor;
                            por_mov_sub.SetPorcentagem(0, false);
                        }
                    }
                    if (somar > 0)
                    {
                        por_sub.SetPorcentagem(por_sub.Valor + somar, false);
                    }

                }

                if (Conexoes.Utilz.Pergunta("Salvar Alterações ?"))
                {
                    Utilz.SetPorcentagens(subetapa.agrupador, brothers, porcentagens);
                    UpdateAll();
                }
            }
        }
        private void editar_numero(object sender, RoutedEventArgs e)
        {
            DLM.orc.PGO_Etapa sel = ((FrameworkElement)sender).DataContext as DLM.orc.PGO_Etapa;
            if (sel == null) { return; }
            int etapa = Conexoes.Utilz.Int(Conexoes.Utilz.Prompt("Digite a etapa", sel.ToString(), sel.nome.ToString()));

            if (etapa > 0 && sel.nome!=etapa)
            {
                sel.nome = etapa;
                sel.Salvar(false, false);
                UpdateAll();
            }
        }
        private void sobe_etapas(object sender, RoutedEventArgs e)
        {
            List<DLM.orc.PGO_Etapa> etapas = Lista_Etapas.Selecao<DLM.orc.PGO_Etapa>();
            int etapa = Conexoes.Utilz.Int(Conexoes.Utilz.Prompt("Digite quanto você quer reduzir", "Editar Etapas", "1"));
            if (etapa == 0) { return; }

            if (etapas.Count > 0 && etapa > 0)
            {

                foreach (var sel in etapas)
                {
                    var novo = sel.nome - etapa;
                    if (novo <= 0)
                    {
                        novo = 1;
                    }
                    sel.nome = novo;
                    sel.Salvar(false, false);
                }
                UpdateAll();

            }
        }
        private void desce_etapas(object sender, RoutedEventArgs e)
        {
            List<DLM.orc.PGO_Etapa> etapas = Lista_Etapas.Selecao<DLM.orc.PGO_Etapa>();
            int etapa = Conexoes.Utilz.Int(Conexoes.Utilz.Prompt("Digite quanto você quer aumentar", "Editar Etapas", "1"));
            if (etapa == 0) { return; }
            if (etapas.Count > 0)
            {
                foreach (var sel in etapas)
                {
                    var novo = sel.nome + etapa;
                    if (novo <= 0)
                    {
                        novo = 1;
                    }
                    sel.nome = novo;
                    sel.Salvar(false, false);
                }
                UpdateAll();

            }
        }
        private void editar_pep_varios(object sender, RoutedEventArgs e)
        {
            var s = Lista_PEPs.Selecao<DLM.orc.PEP_Agrupador>().FindAll(x => x.GetFerts().Count > 0);
            if (s.Count == 0) { return; }
            var novo = Conexoes.Utilz.Prompt("Digite o nome do PEP", "", s[0].PEP, false, "", false, 3).ToUpper();
            if (novo == null) { return; }
            if (novo.Length != 3) { return; }

            //bool faturamento = Utilz.Pergunta("Forçar o mesmo PEP de fabricação que o de faturamento?");

            foreach (var sel in s)
            {
                SetPEP(sel, novo, true);

            }

            UpdateAll();
        }
        private void edita_pep_subetapas(object sender, RoutedEventArgs e)
        {
            var sel = Rad_Lista_SubEtapas_Selecao.SelectedItems.Cast<DLM.orc.SubEtapa_Agrupador>().ToList();
            if (sel.Count == 0) { return; }

            var novo = Conexoes.Utilz.Prompt("Digite o nome do PEP", "", sel[0].agrupador.PEP, false, "", false, 3).ToUpper();
            if (novo == null) { return; }
            if (novo.Length != 3) { return; }

            bool faturamento = Utilz.Pergunta("Forçar o mesmo PEP de fabricação que o de faturamento?");


            foreach (var pp in sel)
            {
                SetPEP(pp.agrupador, novo, faturamento);
            }

            UpdateAll();
        }
        private void edita_pep_subpodenradores(object sender, RoutedEventArgs e)
        {
            var sel = Rad_Lista_Ponderadores.SelectedItems.Cast<DLM.orc.PGO_SubEtapa_Ponderador>().ToList();

            if (sel.Count > 0)
            {
                var pep = Conexoes.Utilz.Prompt("Digite o PEP", "", sel[0].chave);
                if (pep == null | pep == "") { return; }
                if (Conexoes.Utilz.Pergunta("Tem certeza que deseja alterar?"))
                {
                    foreach (var p in sel)
                    {
                        var ss = p.Getsubponderadores_mesmo_predio().FindAll(x => x.ponderador_fim.id == p.ponderador_fim.id).Find(x => x.chave == pep);
                        if (ss == null)
                        {
                            p.chave = pep;
                            p.Salvar();
                        }
                        else
                        {
                            ss.valor = ss.valor + p.valor;
                            foreach (var f in p.GetFerts())
                            {
                                ss.AddFert(f.FERT, f.WERKS, false, false);
                            }
                            ss.Salvar();
                            p.valor = 0;
                            p.ClearFerts(true);
                        }

                    }
                    UpdateAll();
                }

            }
        }
        private void set_sequencial(object sender, RoutedEventArgs e)
        {
            var sel = Lista_Etapas.Selecao<DLM.orc.PGO_Etapa>();
            if (sel.Count > 0)
            {
                int inicio = Conexoes.Utilz.Int(Conexoes.Utilz.Prompt("Digite a etapa inicial", "", "1"));
                if (inicio > 0)
                {
                    if (Conexoes.Utilz.Pergunta("Tem certeza?"))
                    {
                        for (int i = 0; i < sel.Count; i++)
                        {
                            sel[i].SetNumero(inicio + i);
                        }
                        UpdateAll();
                    }
                }
            }
        }
        private void apagar_selecao(object sender, RoutedEventArgs e)
        {
            var s = Lista_Etapas.Selecao<DLM.orc.PGO_Etapa>();

            foreach (var st in s)
            {
                this.Obra.Apagar(st);
            }
            UpdateAll();
        }
        private void add_etapa_sel(object sender, RoutedEventArgs e)
        {
            DLM.orc.PGO_Predio sel = ((FrameworkElement)sender).DataContext as DLM.orc.PGO_Predio;
            if (sel == null) { return; }
            if (sel.Saldo_Etapa <= 0)
            {
                return;
            }
            AddEtapa(sel);

        }
        private void editar_descricao(object sender, RoutedEventArgs e)
        {
            DLM.orc.PGO_Etapa sel = ((FrameworkElement)sender).DataContext as DLM.orc.PGO_Etapa;

            if (sel == null) { return; }
            sel.SetDescricao(sel.descricao);
        }
        private void mover_etapa_varios(object sender, RoutedEventArgs e)
        {
            List<DLM.orc.PGO_Etapa> etapas = Lista_Etapas.Selecao<DLM.orc.PGO_Etapa>();
            if (etapas.Count > 0)
            {

                int etapa = Conexoes.Utilz.Int(Conexoes.Utilz.Prompt("Digite a etapa", etapas[0].nome.ToString()));

                if (etapa > 0)
                {
                    foreach (var sel in etapas)
                    {
                        sel.nome = etapa;
                        sel.Salvar(false, false);
                    }

                    UpdateAll();
                }

            }


        }
        private void ver_materiais(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                this.Visibility = Visibility.Hidden;
                PGO.Funcoes.VerMateriais(this.Obra);
                this.Visibility = Visibility.Visible;
                //this.UpdatePredios();
            }));
            //Conexoes.Utilz.VerPecas(Conexoes.Utilz.consolidar(Obra));
        }
        private void editar_contrato_sap(object sender, RoutedEventArgs e)
        {
            this.Obra.SetContrato_SAP(this.Obra.Contrato_SAP);
            this.Obra.SetPedido_User(this.Obra.pedido_user);
            UpdateAll();
        }
        private void move_material(object sender, RoutedEventArgs e)
        {
            var sels = ((FrameworkElement)sender).DataContext;
            if (sels is DLM.orc.PGO_SubEtapa_Ponderador)
            {
                DLM.orc.PGO_SubEtapa_Ponderador sel = ((FrameworkElement)sender).DataContext as DLM.orc.PGO_SubEtapa_Ponderador;
                if (Conexoes.Utilz.Pergunta("Tem certeza que deseja liberar todo o material somente nesta etapa?"))
                {
                    move_material(sel);
                    UpdateAll();
                }
            }
            else if (sels is DLM.orc.Predio_Ponderador)
            {
                DLM.orc.Predio_Ponderador sel = ((FrameworkElement)sender).DataContext as DLM.orc.Predio_Ponderador;
                if (Conexoes.Utilz.Pergunta("Tem certeza que deseja liberar todo o material somente nesta etapa?"))
                {
                    move_material(sel.subponderadores[0]);
                    UpdateAll();
                }
            }
            else if (sels is DLM.orc.SubEtapa_Agrupador)
            {
                DLM.orc.SubEtapa_Agrupador sel = ((FrameworkElement)sender).DataContext as DLM.orc.SubEtapa_Agrupador;
                if (Conexoes.Utilz.Pergunta("Tem certeza que deseja liberar todo o material de todos os prédios somente nesta etapa?"))
                {
                    foreach (var p in sel.GetPredios())
                    {
                        var subs = sel.GetSubPonderadores(p);
                        if (subs.Count > 0)
                        {
                            move_material(subs[0]);
                        }
                    }
                    UpdateAll();

                }
            }
            else if (sels is DLM.orc.SubEtapa_AgrupadorBase)
            {
                DLM.orc.SubEtapa_AgrupadorBase sss = ((FrameworkElement)sender).DataContext as DLM.orc.SubEtapa_AgrupadorBase;
                if (Conexoes.Utilz.Pergunta("Tem certeza que deseja liberar todo o material de todos os prédios somente nesta etapa?"))
                {
                    foreach (var sel in sss.subetapas)
                    {
                        foreach (var p in sel.GetPredios())
                        {
                            var subs = sel.GetSubPonderadores(p);
                            if (subs.Count > 0)
                            {
                                move_material(subs[0]);
                            }
                        }
                    }

                    UpdateAll();

                }
            }
        }
        private void Set100(DLM.orc.PGO_SubEtapa_Ponderador sel)
        {
            sel.valor = 100;
            sel.Salvar();
            foreach (var subs in sel.Getsubponderadores_mesmo_predio())
            {
                subs.valor = 0;
                subs.Salvar();
            }
        }
        private void reset_material(object sender, RoutedEventArgs e)
        {


            var sels = ((FrameworkElement)sender).DataContext;
            if (sels is DLM.orc.PGO_SubEtapa_Ponderador)
            {
                DLM.orc.PGO_SubEtapa_Ponderador sel = ((FrameworkElement)sender).DataContext as DLM.orc.PGO_SubEtapa_Ponderador;
                if (Conexoes.Utilz.Pergunta("Tem certeza que deseja retornar o valor do ponderador padrão em todo o material somente nesta etapa?"))
                {
                    Reset_SubPonderador(sel);
                    UpdateAll();
                }
            }
            else if (sels is DLM.orc.PEP_Agrupador)
            {
                DLM.orc.PEP_Agrupador sel = ((FrameworkElement)sender).DataContext as DLM.orc.PEP_Agrupador;
                if (Conexoes.Utilz.Pergunta("Tem certeza que deseja retornar o valor do ponderador padrão em todas as etapas " + sel.ToString() + "?"))
                {
                    Conexoes.ControleWait w = Conexoes.Utilz.Wait(sel.GetSubPonderadores().Count, "Carregando...");
                    foreach (var s in sel.GetSubPonderadores())
                    {
                        Reset_SubPonderador(s);
                        w.somaProgresso();
                    }
                    w.Close();

                    UpdateAll();
                };

            }
            else if (sels is DLM.orc.Predio_Ponderador)
            {
                DLM.orc.Predio_Ponderador sel = ((FrameworkElement)sender).DataContext as DLM.orc.Predio_Ponderador;
                if (Conexoes.Utilz.Pergunta("Tem certeza que deseja retornar o valor do ponderador padrão em todo o material somente nesta etapa?"))
                {

                    foreach (var s in sel.subponderadores)
                    {
                        Reset_SubPonderador(s);

                    }
                    UpdateAll();
                }
            }
            else if (sels is DLM.orc.SubEtapa_Agrupador)
            {
                DLM.orc.SubEtapa_Agrupador sel = ((FrameworkElement)sender).DataContext as DLM.orc.SubEtapa_Agrupador;
                if (Conexoes.Utilz.Pergunta("Tem certeza que deseja retornar o valor do ponderador padrão em todo o material somente nesta etapa?"))
                {
                    foreach (var p in sel.GetPredios())
                    {
                        var subs = sel.GetSubPonderadores(p).FindAll(x => x.werk == sel.werk);
                        if (subs.Count > 0)
                        {
                            Reset_SubPonderador(subs[0]);
                        }
                    }
                    UpdateAll();

                }
            }
            else if (sels is DLM.orc.SubEtapa_AgrupadorBase)
            {
                DLM.orc.SubEtapa_AgrupadorBase sss = ((FrameworkElement)sender).DataContext as DLM.orc.SubEtapa_AgrupadorBase;
                if (Conexoes.Utilz.Pergunta("Tem certeza que deseja retornar o valor do ponderador padrão em todo o material somente nesta etapa?"))
                {
                    foreach (var sel in sss.subetapas)
                    {
                        foreach (var p in sel.GetPredios())
                        {
                            var subs = sel.GetSubPonderadores(p);
                            if (subs.Count > 0)
                            {
                                Reset_SubPonderador(subs[0]);
                            }
                        }
                    }

                    UpdateAll();

                }
            }

        }
        private void Reset_SubPonderador(DLM.orc.PGO_SubEtapa_Ponderador selecao)
        {
            var ps = selecao.Getpredios_ponderadores().SelectMany(x => x.subponderadores).ToList();
            int c = 1;
            foreach (var sel in ps)
            {
                var subss = sel.ponderador.GetPredio().GetPonderadores().SelectMany(x => x.GetSubPonderadores()).ToList().FindAll(x => x.chave == sel.chave).ToList();
                subss.AddRange(sel.Getsubponderadores_mesmo_predio());
                Conexoes.ControleWait w = Conexoes.Utilz.Wait(subss.Count, $"{c}/{ps.Count()} - Movendo Sub-Ponderadores...{subss.Count}");
                foreach (var subs in subss)
                {
                    subs.valor = subs.ponderador.ponderador;
                    subs.ResetPonderadorUser();
                    subs.Salvar();
                    w.somaProgresso();
                }
                w.Close();
                c++;
            }

        }
        private void add_fert_peps_obra(object sender, RoutedEventArgs e)
        {
            DLM.orc.PEP_Agrupador sel = ((FrameworkElement)sender).DataContext as DLM.orc.PEP_Agrupador;
            var opcoes = sel.Getferts_fora().ToList();
            if (opcoes.Count == 0) { return; }
            //if (Conexoes.Utilz.Pergunta("Filtrar somente PEPs de mesma unidade fabril?"))
            //{
            //    opcoes = opcoes.FindAll(x => x.FAB == sel.fabrica);
            //}
            if (opcoes.Count == 0) { return; }

            var selecao = Conexoes.Utilz.Selecao.SelecionarObjetos(opcoes);
            if (selecao.Count > 0)
            {

                if (Conexoes.Utilz.Pergunta("Tem certeza que deseja mover todo o material do(s) fert(s) selecionado(s) para essa etapa?"))
                {
                    var faturamento = Utilz.Pergunta("Forçar unidade de fabricação igual a de faturamento?");
                    var agrupadores = new List<DLM.orc.PEP_Agrupador>();
                    agrupadores.AddRange(sel.GetPEPAgrupadores());
                    foreach (var agru in agrupadores)
                    {
                        foreach (var subsel in selecao)
                        {
                            agru.moveFert(subsel.FERT, subsel.WERKS, faturamento);
                        }
                    }
                    UpdateAll();
                }

            }
        }
        private void editar_frentes(object sender, RoutedEventArgs e)
        {
            var sel = Lista_Etapas.Selecao<DLM.orc.PGO_Etapa>();

            if (sel.Count == 0) { return; }

            var frente = Conexoes.Utilz.Selecao.SelecionarObjeto(DBases.GetDbOrc().GetFrentes(), null, "Selecione");
            if (frente != null)
            {
                foreach (var s in sel)
                {
                    s.SetFrente(frente);
                }
                //UpdateDatas();
            }
        }
        private void juntar_subs(object sender, RoutedEventArgs e)
        {
            var sel = ((FrameworkElement)sender).DataContext;

            if (sel is DLM.orc.SubEtapa_Agrupador)
            {
                var destino = sel as DLM.orc.SubEtapa_Agrupador;

                List<DLM.orc.SubEtapa_Agrupador> selecao = Conexoes.Utilz.Selecao.SelecionarObjetos(destino.Getagrupadores_externos());
                move_sub_etapas(destino, selecao);

            }




        }
        private void apaga_etapa(object sender, RoutedEventArgs e)
        {
            DLM.orc.PGO_Etapa sel = ((FrameworkElement)sender).DataContext as DLM.orc.PGO_Etapa;
            if (Conexoes.Utilz.Pergunta("Tem certeza que deseja apagar a etapa " + sel.ToString() + " ?"))
            {
                this.Obra.Apagar(sel);
                UpdateAll();
            }


        }
        private void add_etapa(object sender, RoutedEventArgs e)
        {
            var prediossel = this.lista_predios.Selecao<DLM.orc.PGO_Predio>();
            prediossel = prediossel.FindAll(x => x.Saldo_Etapa > 0).ToList();
            if (prediossel.Count == 1)
            {
                AddEtapa(prediossel[0]);
            }
            else
            {
                foreach (var predio in prediossel)
                {
                    AddEtapa(predio);
                }
            }
        }
        private void criar_etapas_automatico(object sender, RoutedEventArgs e)
        {
            if (Obra.GetEtapas().Count > 0)
            {
                if (Utilz.Pergunta("Já existem etapas criadas. Deseja limpar as etapas atuais e criar?"))
                {
                    this.Obra.ClearEtapas();
                }
                else
                {
                    return;
                }
            }

            this.Obra.CriarEtapas();
            UpdateAll();
        }
        private void clear_etapas(object sender, RoutedEventArgs e)
        {
            if (Conexoes.Utilz.Pergunta("Tem Certeza? Não é possível desfazer"))
            {
                this.Obra.ClearEtapas();
                UpdateAll();
            }
        }

        private void exporta_cj20n(object sender, RoutedEventArgs e)
        {
            Conexoes.Utilz.ExportarPlanilhaCJ20N(this.Obra);
        }

        private void resetar_subponderadores(object sender, RoutedEventArgs e)
        {
            if(!Conexoes.Utilz.Pergunta("Tem certeza? Não é possível desfazer")) { return; }
            foreach(var predio in this.Obra.Predios)
            {
                predio.ApagarSubponderadores();
            }
            UpdateAll();
        }
    }
}
