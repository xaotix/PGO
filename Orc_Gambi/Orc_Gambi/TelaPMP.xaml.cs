using DLM.orc;
using DLM.vars;
using ExplorerPLM;
using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DLM.encoder;

namespace PGO
{
    /// <summary>
    /// Interaction logic for TelaPMP.xaml
    /// </summary>
    public partial class TelaPMP : ModernWindow
    {
        public List<DLM.orc.OrcamentoObra> obras { get; set; } = new List<DLM.orc.OrcamentoObra>();
        public List<DLM.orc.Pacote_Obra> pacotes { get; set; } = new List<DLM.orc.Pacote_Obra>();
        public List<DLM.orc.Pacote_Obra> pacotes_consolidados { get; set; } = new List<DLM.orc.Pacote_Obra>();
        public List<DLM.orc.OrcamentoObra> selecao { get; set; } = new List<DLM.orc.OrcamentoObra>();
        public TelaPMP(List<DLM.orc.OrcamentoObra> obras)
        {


            InitializeComponent();
            this.obras = obras;

            this.UpdatePacotes();



        }
        public TelaPMP(List<DLM.orc.OrcamentoObra> obras, List<Conexoes.Pedido_PMP> pedidos_buffer)
        {
            this.obras = obras;
            this.pacotes = PGOVars.GetDbOrc().GetPacotes();
            this.pacotes_consolidados = PGOVars.GetDbOrc().GetPacotes_Consolidadas();
            this.pedidos_importar = pedidos_buffer;

            InitializeComponent();
            this.Lista_Criados.ItemsSource = this.pacotes;
            this.Lista_Consolidadas.ItemsSource = this.pacotes_consolidados;


            this.Lista_Criacao.ItemsSource = null;
            this.Lista_Criacao.ItemsSource = this.pedidos_importar;
            this.tab_principal.SelectedIndex = 1;
        }

        private void editar_contrato_sap(object sender, RoutedEventArgs e)
        {
            DLM.orc.OrcamentoObra sel = ((FrameworkElement)sender).DataContext as DLM.orc.OrcamentoObra;
            if (sel == null) { return; }

            sel.SetContrato_SAP(sel.Contrato_SAP);
            sel.SetPedido_User(sel.pedido_user);
        }

        private void editar_etapas(object sender, RoutedEventArgs e)
        {
            DLM.orc.OrcamentoObra sel = ((FrameworkElement)sender).DataContext as DLM.orc.OrcamentoObra;
            if (sel == null) { return; }
            PGO.Etapas mm = new PGO.Etapas(sel);
            mm.Show();
        }
        public List<DLM.orc.OrcamentoObra> Obras()
        {
            return this.obras;
        }
        private void adicionar_obras(object sender, RoutedEventArgs e)
        {
            bool somente_consolidadas = Conexoes.Utilz.Pergunta("Mostrar somente obras que tenham consolidação?");
            List<DLM.orc.OrcamentoObra> ss = ListarOrcamentos(somente_consolidadas);
            selecao.AddRange(ss);
            Update();
        }

        private List<DLM.orc.OrcamentoObra> ListarOrcamentos(bool somente_consolidadas, bool revisoes = false)
        {
            var lista = Obras();
            if (revisoes)
            {
                lista = Obras().SelectMany(x => x.Revisoes).ToList();
            }
            if (somente_consolidadas)
            {
                lista = lista.FindAll(x => x.Consolidacao);
            }
            var s = Conexoes.Utilz.Selecao.SelecionarObjetos(lista,true);
            var ss = s.Cast<DLM.orc.OrcamentoObra>().ToList().FindAll(x => selecao.Find(y => y == x) == null);
            return ss;
        }

        private void limpar_obras(object sender, RoutedEventArgs e)
        {
            this.selecao.Clear();
            Update();
        }
        public List<Report> Validar()
        {
            List<Report> pre_testes = new List<Report>();

            var obras_sem_contrato_sap = selecao.FindAll(x => x.Contrato_SAP.Replace("0", "").Replace(" ", "") == "" /*| !Conexoes.Utilz.ESoNumero(x.Contrato_SAP)*/ | x.Contrato_SAP.Length != 6);
            var pedidos_com_pacote = selecao.FindAll(x => pacotes.Find(y => y.pedido == x.PedidoSAP) != null);
            var obras_duplicadas = selecao.FindAll(x => selecao.FindAll(y => y.PedidoSAP == x.PedidoSAP).Count > 1).FindAll(x => obras_sem_contrato_sap.Find(y => y == x) == null);

            pre_testes.AddRange(obras_sem_contrato_sap.Select(x => x.Contrato + "." + x.Revisao + " - " + x.Nome).Distinct().ToList().Select(x => new Report(x, "Obra com contrato SAP em branco ou inválido", TipoReport.Crítico)));
            pre_testes.AddRange(obras_duplicadas.Select(x => x.Contrato + "." + x.Revisao + " - " + x.Nome + " Pedido: " + x.PedidoSAP).Distinct().ToList().Select(x => new Report(x, "Mais de Uma obra com o mesmo contrato SAP", TipoReport.Crítico)));
            pre_testes.AddRange(pedidos_com_pacote.Select(x => x.Contrato + "." + x.Revisao + " - " + x.Nome + " Pedido: " + x.PedidoSAP).Distinct().ToList().Select(x => new Report(x, "Já existe um pacote com este pedido.", TipoReport.Crítico)));

            return pre_testes;

        }
        private void gerar_material(object sender, RoutedEventArgs e)
        {
            if (this.selecao.Count == 0)
            {
                return;
            }
            if (!Conexoes.Utilz.Pergunta("Rodar " + this.selecao.Count + " obras ?"))
            {
                return;
            }

            var pre_testes = Validar();
            if (pre_testes.Count > 0)
            {
                Conexoes.Utilz.ShowReports(pre_testes);
                return;
            }



            List<Report> erros = new List<Report>();
            List<DLM.orc.Orcamento_Peca> pecas = new List<DLM.orc.Orcamento_Peca>();
            Conexoes.ControleWait w = Conexoes.Utilz.Wait(this.selecao.Count, "Lendo Materiais...");
            foreach (var ob in this.selecao)
            {
                try
                {
                    if (ob.GetEtapas().Count == 0)
                    {
                        erros.Add(new Report(ob.ToString(), "Obra sem etapas. Criado automaticamente", TipoReport.Status));
                        ob.CriarEtapas();

                    }
                    pecas.AddRange(ob.GetPecasEtapas(true));
                }
                catch (Exception ex)
                {
                    erros.Add(new Report(ob.ToString(), ex.ToString(), TipoReport.Crítico));

                }

                w.somaProgresso();
            }
            w.Close();
            Conexoes.Utilz.ShowReports(erros);
            PGO.Funcoes.VerMateriais(pecas);
        }

        private void trocar_revisao(object sender, RoutedEventArgs e)
        {
            DLM.orc.OrcamentoObra sel = ((FrameworkElement)sender).DataContext as DLM.orc.OrcamentoObra;
            if (sel == null)
            {
                return;
            }
            var nova = Conexoes.Utilz.Selecao.SelecionarObjeto(sel.Revisoes.FindAll(x => x != sel), null);
            if (nova != null)
            {
                selecao.Remove(sel);
                selecao.Add(nova);
                Update();
            }
        }

        private void Update()
        {
            selecao = selecao.OrderBy(x => x.ToString()).ToList();
            Lista.ItemsSource = null;
            Lista.ItemsSource = selecao;
        }

        private void gerar_pmp(object sender, RoutedEventArgs e)
        {
            if (this.selecao.Count == 0)
            {
                return;
            }
            if (!Conexoes.Utilz.Pergunta("Criar pacote(s) da(s) " + this.selecao.Count + " obra(s) ?"))
            {
                return;
            }

            var pre_testes = Validar();
            if (pre_testes.Count > 0)
            {
                Conexoes.Utilz.ShowReports(pre_testes);
                return;
            }

            if (this.selecao.Count == 0)
            {
                return;
            }

            Conexoes.ControleWait w = Conexoes.Utilz.Wait(this.selecao.Count);

            foreach (var obra in this.selecao)
            {
               var s = obra.GravarPMP_ORC();
                if(!s)
                {
                    w.Close();
                    return;
                }
                w.somaProgresso();
            }
            w.Close();
            Lista.ItemsSource = null;
            Lista.Items.Clear();
            UpdatePacotes();
            Conexoes.Utilz.Alerta("Pacotes criados!");
        }

        private void editar_setor_atividade(object sender, RoutedEventArgs e)
        {

        }

        private void excluir_pacote(object sender, RoutedEventArgs e)
        {
            DLM.orc.Pacote_Obra sel = ((FrameworkElement)sender).DataContext as DLM.orc.Pacote_Obra;
            if (sel == null)
            {
                return;
            }

            if (Conexoes.Utilz.Pergunta("Tem certeza que deseja excluir o pacote [" + sel.ToString() + "] ?"))
            {
                if (sel.Obra != null)
                {
                    DLM.vars.PGOVars.LimparPMP_Consolidada(sel.pedido);
                    //sel.Obra.LimparPMP();
                    UpdatePacotes();
                }
                else
                {
                    Conexoes.Utilz.Alerta("Obra ~[id=" + sel.id_obra + "] não encontrada. Contacte suporte\n(Daniel Maciel).", "", MessageBoxImage.Error);
                }

            }
        }
        public List<Conexoes.Pedido_PMP> pedidos_importar { get; set; } = new List<Conexoes.Pedido_PMP>();
        private void UpdatePacotes()
        {
            this.pacotes = PGOVars.GetDbOrc().GetPacotes(true);
            this.pacotes_consolidados = PGOVars.GetDbOrc().GetPacotes_Consolidadas(true);

            this.Lista_Criados.ItemsSource = null;
            this.Lista_Consolidadas.ItemsSource = null;

            this.Lista_Criados.ItemsSource = this.pacotes;
            this.Lista_Consolidadas.ItemsSource = this.pacotes_consolidados;
        }

        private void atualizar_pacote(object sender, RoutedEventArgs e)
        {
            DLM.orc.Pacote_Obra sel = ((FrameworkElement)sender).DataContext as DLM.orc.Pacote_Obra;
            if (sel == null)
            {
                return;
            }

            if (Conexoes.Utilz.Pergunta("Tem certeza que deseja atualizar o pacote [" + sel.ToString() + "] ?"))
            {
                if (sel.Obra != null)
                {
                    sel.Obra.GravarPMP_ORC();
                    UpdatePacotes();
                }
                else
                {
                    Conexoes.Utilz.Alerta("Obra ~[id=" + sel.id_obra + "] não encontrada. Contacte suporte\n(Daniel Maciel).", "", MessageBoxImage.Error);
                }

            }
        }

        private void editar_etapas_pacote(object sender, RoutedEventArgs e)
        {
            DLM.orc.Pacote_Obra sel = ((FrameworkElement)sender).DataContext as DLM.orc.Pacote_Obra;
            if (sel == null)
            {
                return;
            }

            if (Conexoes.Utilz.Pergunta("Tem certeza que editar as etapas do pacote [" + sel.ToString() + "] ?"))
            {
                if (sel.Obra != null)
                {
                    this.Visibility = Visibility.Collapsed;
                    PGO.Etapas mm = new PGO.Etapas(sel.Obra);
                    mm.Closed += Mm_Closed;
                    mm.ShowDialog();
                    if (Conexoes.Utilz.Pergunta("Atualizar pacote? \nSe clicar em não, o pacote atual será mantido, não acompanhando as etapas alteradas/criadas/excluídas."))
                    {
                        sel.Obra.GravarPMP_ORC();

                    }
                    UpdatePacotes();
                }
                else
                {
                    Conexoes.Utilz.Alerta("Obra ~[id=" + sel.id_obra + "] não encontrada. Contacte suporte\n(Daniel Maciel).", "", MessageBoxImage.Error);
                }

            }
        }

        private void Mm_Closed(object sender, EventArgs e)
        {
            this.Visibility = Visibility.Visible;
        }

        private void excluir_pacotes(object sender, RoutedEventArgs e)
        {
            var sel = Lista_Criados.SelectedItems.Cast<DLM.orc.Pacote_Obra>().ToList();
            if (sel.Count > 0)
            {
                if (Conexoes.Utilz.Pergunta("Tem certeza que deseja excluir os pacotes selecionados?"))
                {
                    foreach (var s in sel)
                    {
                        if (s.Obra != null)
                        {
                            s.Obra.LimparPMP();

                        }
                        else
                        {
                            DLM.vars.PGOVars.LimparPMP_Orcamento(s.pedido);
                            //Conexoes.Utilz.Alerta("Obra ~[id=" + s.id_obra + "] não encontrada. Contacte suporte\n(Daniel Maciel).", "", MessageBoxImage.Error);

                        }
                    }
                    UpdatePacotes();
                    Conexoes.Utilz.Alerta("Pacotes removidos");


                }
            }
        }

        private void autlizar_pacotes(object sender, RoutedEventArgs e)
        {
            var sel = Lista_Criados.SelectedItems.Cast<DLM.orc.Pacote_Obra>().ToList();
            if (sel.Count > 0)
            {
                if (Conexoes.Utilz.Pergunta("Tem certeza que deseja atualizar os pacotes selecionados?"))
                {
                    foreach (var s in sel)
                    {
                        if (s.Obra != null)
                        {
                            s.Obra.GravarPMP_ORC();

                        }
                        else
                        {
                            Conexoes.Utilz.Alerta("Obra ~[id=" + s.id_obra + "] não encontrada. Contacte suporte\n(Daniel Maciel).", "", MessageBoxImage.Error);

                        }
                    }
                    UpdatePacotes();
                    Conexoes.Utilz.Alerta("Pacotes atualizados");
                }
            }
        }
        List<Conexoes.Peca_PMP> pecas = new List<Conexoes.Peca_PMP>();
        public List<string> peps
        {
            get
            {
                return pecas.Select(x => x.pep).Distinct().ToList();
            }
        }
        public List<string> arquivos = new List<string>();
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

            List<Conexoes.Arquivo> excels = ExplorerPLM.Utilidades.ExplorerArquivos(new Conexoes.Pasta(DLM.vars.PGOVars.GetConfig().pasta_consolidadas), "XLSX");
            if (excels.Count > 0)
            {


                if (excels.Count > 0)
                {
                    excels = excels.OrderBy(x => x.TamKB).ToList();
                    if (!Conexoes.Utilz.Pergunta("Carregar os " + excels.Count + " arquivos encontrados?"))
                    {
                        return;
                    }
                }

                List<Report> erros = new List<Report>();

                this.pecas = PGO.Funcoes.getPecas(excels, out erros);

                var pacote_atual = new Conexoes.Pacote_PMP(pecas, erros);

                Conexoes.Utilz.ShowReports(pacote_atual.erros);

                this.pedidos_importar = pacote_atual.pedidos;


                this.arquivos.AddRange(excels.Select(x => x.Endereco.ToUpper()));
                this.Lista_Criacao.ItemsSource = null;
                this.Lista_Criacao.ItemsSource = this.pedidos_importar;


            }
        }

        private void importar_consolidadas(object sender, RoutedEventArgs e)
        {
            if (pedidos_importar.Count == 0) { return; }
            if (Conexoes.Utilz.Pergunta("Foram encontrados " + pedidos_importar.Count + " pedidos e " + pedidos_importar.Sum(x => x.pecas.Count) + " peças nos arquivos importados. Deseja cadastra-los agora?"))
            {
                bool tudo = Conexoes.Utilz.Pergunta("Substituir todo o pedido? \nSe clicar em não, o sistema substituirá somente peças em etapas existentes, criando as nvoas etapas.");

                PGO.Funcoes.importarPecas(this.pedidos_importar, tudo);
                UpdatePacotes();
                limpar_carga_consolidada();
                Conexoes.Utilz.Alerta("Arquivos Importados!");


            }
        }

        private void limpar_carga_consolidada()
        {
            this.pedidos_importar.Clear();
            this.pecas.Clear();
            this.arquivos.Clear();
            this.Lista_Criacao.ItemsSource = null;
            this.Lista_Criacao.ItemsSource = this.pedidos_importar;
        }

        private void limpar_carga(object sender, RoutedEventArgs e)
        {
            if (Conexoes.Utilz.Pergunta("Tem certeza?"))
            {
                limpar_carga_consolidada();
            }
        }

        private void consolidada_add_pedido(object sender, RoutedEventArgs e)
        {
            if (this.pedidos_importar.Count > 0 | this.pecas.Count > 0 | this.arquivos.Count > 0)
            {
                if (Conexoes.Utilz.Pergunta("Há pacotes carregados. Para poder realizar esse processo é necessário limpar o buffer. deseja continuar?"))
                {
                    limpar_carga_consolidada();
                }
            }
            List<DLM.orc.OrcamentoObra> ss = ListarOrcamentos(true, true).FindAll(x => x.ContratoSAPValido);
            if (ss.Count > 0)
            {
                List<Report> erros = new List<Report>();
                this.pedidos_importar = PGO.Funcoes.getPedidos(ss, out erros);

                this.arquivos.AddRange(this.pedidos_importar.SelectMany(x => x.arquivos()).Distinct().ToList());
                this.Lista_Criacao.ItemsSource = null;
                this.Lista_Criacao.ItemsSource = this.pedidos_importar;
                Conexoes.Utilz.ShowReports(erros);
            }

        }

        private void cadastro_frentes(object sender, RoutedEventArgs e)
        {
            PGO.EditarObjetoOrcamento mm = new PGO.EditarObjetoOrcamento(PGO.EditarObjetoOrcamento.Tipologia.Frente);
            mm.Show();
        }

        private void Cadastro_Tipos_Pintura(object sender, RoutedEventArgs e)
        {
            PGO.EditarObjetoOrcamento mm = new PGO.EditarObjetoOrcamento(PGO.EditarObjetoOrcamento.Tipologia.Tipo_Pintura);
            mm.Show();
        }

        private void Cadastro_FERTS(object sender, RoutedEventArgs e)
        {
            PGO.EditarObjetoOrcamento mm = new PGO.EditarObjetoOrcamento(PGO.EditarObjetoOrcamento.Tipologia.FERT);
            mm.Show();
        }

        private void Cadastro_listas_tecnicas(object sender, RoutedEventArgs e)
        {
            PGO.Listas_Tecnicas_Cronograma mm = new Listas_Tecnicas_Cronograma();
            mm.Show();
        }

        private void editar_pacote(object sender, RoutedEventArgs e)
        {
            Conexoes.Pedido_PMP sel = ((FrameworkElement)sender).DataContext as Conexoes.Pedido_PMP;

            if (sel != null)
            {
                EditarPacote mm = new EditarPacote();
                mm.lista.ItemsSource = sel.pecas;
                mm.ShowDialog();
                this.Lista_Criacao.ItemsSource = null;
                this.Lista_Criacao.ItemsSource = this.pedidos_importar;
            }
        }

        private void excluir_pacotes_consolidadas(object sender, RoutedEventArgs e)
        {
            var sel = Lista_Consolidadas.SelectedItems.Cast<DLM.orc.Pacote_Obra>().ToList();
            if (sel.Count > 0)
            {
                if (Conexoes.Utilz.Pergunta("Tem certeza que deseja excluir os pacotes selecionados?"))
                {
                    foreach (var s in sel)
                    {
                        DLM.vars.PGOVars.LimparPMP_Consolidada(s.pedido);
                    }
                    UpdatePacotes();
                    Conexoes.Utilz.Alerta("Pacotes removidos");
                }
            }
        }

        private void excluir_pacote_consolidada(object sender, RoutedEventArgs e)
        {
            Conexoes.Pedido_PMP sel = ((FrameworkElement)sender).DataContext as Conexoes.Pedido_PMP;

            if (sel != null)
            {
                if (Conexoes.Utilz.Pergunta($"Tem certeza que deseja excluir {sel}"))
                {
                    sel.Limpar(true);
                    UpdatePacotes();
                }
            }
        }
    }
}
