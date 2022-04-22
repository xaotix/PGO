using Conexoes;
using DLM.orc;
using DLM.vars;
using FirstFloor.ModernUI.Presentation;
using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Telerik.Windows.Controls;

namespace PGO
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow
    {
        private NovaObra MenuNovaObra { get; set; }
        private PGO_Obra ObraSelecionada { get; set; }
        public Visibility Menus_Orcamento { get; set; } = Visibility.Visible;
        public MainWindow()
        {
            InitializeComponent();
            this.Title = $"PGO Aguarde..."; 
           this.DataContext = this;
            if (!DBases.GetUserAtual().orcamento_ver_obras)
            {
                Menus_Orcamento = Visibility.Collapsed;
            }

            Conexoes.Utilz.SetIcones(this.menu_principal);


            Update(true);
        }

        private void ModernWindow_Closed(object sender, EventArgs e)
        {
            Cfg.Init.Salvar();
            DBases.GetUserAtual().Salva_Status(false);
            Environment.Exit(0);
        }
        private void Fecha(object sender, EventArgs e)
        {
            this.Show();
        }
        public List<PGO_Obra> Obras { get; set; } = new List<PGO_Obra>();
        public List<Rotas> Enderecos { get; set; } = new List<Rotas>();
        private void ModernWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DBases.GetUserAtual().Salva_Status(true);
            Task.Factory.StartNew(() => Funcoes.VerificarVersao());
        }
        private void Update(bool update)
        {
            this.Obras = PGOVars.GetDbOrc().GetObrasOrcamento(update);
            this.lista.ItemsSource = null;

            this.Title = $"PGO" +
                $"{(Cfg.Init.Nova_Folha_Margem ? "" : "Sem Novo Cálculo Folha Margem")}" +
                $"v {System.Windows.Forms.Application.ProductVersion}" +
                $"Dbase: [{DLM.vars.Cfg.Init.MySQL_Servidor}]";

            Funcoes_Mapa.Localizacoes = new Localizacoes(System.Windows.Forms.Application.StartupPath + @"\Locais.setup");

            this.Enderecos = Funcoes_Mapa.AgruparEmRotas(Obras, myMap);
            myMap.ZoomLevel = 3;
            filtrar_obras();
        }
        private void FecharObra(object sender, EventArgs e)
        {
            this.WindowState = WindowState.Maximized;
            this.Visibility = Visibility.Visible;
            JanelaObra mm = (JanelaObra)sender;
            this.Abertas.Remove(mm.Obra);
            this.Update(true);
        }
        public bool VerificarTemplate(PGO_Obra ob)
        {
            if (PGOVars.GetDbOrc().GetTemplates().Find(x => x.id == ob.id_template) == null)
            {
                if (PGOVars.GetDbOrc().GetTemplates().FindAll(x => x.ativo).Count == 1)
                {
                    ob.SetTemplate(PGOVars.GetDbOrc().GetTemplates().FindAll(x => x.ativo)[0]);
                }
                else
                {
                    var sel = Conexoes.Utilz.Selecao.SelecionarObjeto(PGOVars.GetDbOrc().GetTemplates().FindAll(x => x.ativo), null);
                    if (sel != null)
                    {
                        ob.SetTemplate(sel);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

            }
            return true;
        }
        private void AbrirObra(object sender, MouseButtonEventArgs e)
        {
            if ((sender as DataGrid).SelectedItem is PGO_Obra)
            {
                PGO_Obra ob = (sender as DataGrid).SelectedItem as PGO_Obra;
                AbrirObra(ob);
            }
        }
        public void AbrirObra(PGO_Obra ob)
        {
            if (Abertas.Find(x => x.ContratoRevisao == ob.ContratoRevisao) != null)
            {
                Conexoes.Utilz.Alerta("Obra já está aberta em outra janela.", "Obra já aberta", MessageBoxImage.Asterisk);
                return;
            }

            if (!VerificarTemplate(ob))
            {
                return;
            }

            if(ob.Calcular_Rotas)
            {
                var rota = ob.GetRotas();
                if (rota.GetLista(ob.Calcular_Rotas).Count == 0 | rota.Cidade == "" | rota.Pais == "" | rota.id<=0)
                {
                    if (!ob.Nacional && rota.Enderecostr != "")
                    {
                        if (Utilz.Pergunta("A obra selecionada está sem a rota logística calculada. \n" +
                            "É uma obra exportação. \n\nDeseja desabilitar o cálculo da rota logística?"))
                        {
                            ob.Calcular_Rotas = false;
                            ob.Salvar("calcular_rotas", "False");
                            goto Abrir;
                        }
                    }
                    else if (rota.Lista.Count == 0)
                    {
                        if (Utilz.Pergunta("Falta calcular a rota logística. Para isso é necessário definir o endereço de destino. Deseja fazer isso agora?"))
                        {
                            EditarRota(ob);
                            return;
                        }
                    }
                }
            }
            
        Abrir:

            JanelaObra mm = new JanelaObra(ob);
            mm.Title = (ob.Bloqueado ? "[BLOQUEADA!]" : "") + (ob.Tipo == Tipo_Orcamento.SEC ? " [SEC] " : "") + ob.ToString();
            Abertas.Add(ob);
            mm.Closed += FecharObra;
            mm.Show();
            mm.Focus();
            this.Visibility = Visibility.Collapsed;
        }
        private void EditarRota(PGO_Obra ob)
        {
            ExplorerPLM.Menus.Fretes mmc = new ExplorerPLM.Menus.Fretes(ob);
            this.ObraSelecionada = ob;
            mmc.Closed += recarregar;
            this.Visibility = Visibility.Collapsed;
            mmc.Show();
        }
        private void recarregar(object sender, EventArgs e)
        {
            this.Visibility = Visibility.Visible;
            AbrirObra(ObraSelecionada);
        }
        private List<PGO_Obra> Abertas { get; set; } = new List<PGO_Obra>();
        private void ZoomNaObra(PGO_Obra ob)
        {
            if (ob.GetRotas() != null)
            {
                myMap.Center = ob.GetRotas().Pin.Location;
                myMap.ZoomLevel = 10;
            }
            else
            {
                myMap.ZoomLevel = 2;
            }
        }
        private void atualizar_dbase(object sender, RoutedEventArgs e)
        {

            if (!Utilz.Acesso(DBases.GetUserAtual().orcamento_atualizar_db))
            {
                return;
            }
            Atualizar_DBase mm = new Atualizar_DBase();
            mm.Closed += Fecha;
            mm.Show();
            this.Hide();
        }
        private void Cadastro_Tratamentos(object sender, RoutedEventArgs e)
        {
            if (!Utilz.Acesso(DBases.GetUserAtual().orcamento_cadastro_lista_tecnica))
            {
                return;
            }
            EditarObjetoOrcamento mm = new EditarObjetoOrcamento(EditarObjetoOrcamento.Tipologia.Tratamento);
            mm.Show();
        }
        private void Cadastro_listas_tecnicas(object sender, RoutedEventArgs e)
        {

            if (!Utilz.Acesso(DBases.GetUserAtual().orcamento_cadastro_lista_tecnica))
            {
                return;
            }

            Listas_Tecnicas mm = new Listas_Tecnicas();
            mm.Show();

        }
        private void consulta_cadastos(object sender, RoutedEventArgs e)
        {
            ExplorerPLM.Menus.ItensPadrao mm = new ExplorerPLM.Menus.ItensPadrao();
            mm.Show();
        }
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            if (!Utilz.Acesso(DBases.GetUserAtual().tela_cadastro))
            {
                return;
            }
            ExplorerPLM.RM2._Principal mm = new ExplorerPLM.RM2._Principal();
            mm.Show();
        }
 
        private void Arquivo_Obras(object sender, RoutedEventArgs e)
        {
            if (!Utilz.Acesso(DBases.GetUserAtual().orcamento_arquivar))
            {
                return;
            }
            Arquivo mm = new Arquivo();
            mm.Closed += Mm_Closed1;
            mm.ShowDialog();
        }
        private void Mm_Closed1(object sender, EventArgs e)
        {
            Update(true);
        }
        private void Cadastro_Segmentos(object sender, RoutedEventArgs e)
        {
            if (!Utilz.Acesso(DBases.GetUserAtual().orcamento_cadastro_lista_tecnica))
            {
                return;
            }
            EditarObjetoOrcamento mm = new EditarObjetoOrcamento(EditarObjetoOrcamento.Tipologia.Segmento);
            mm.Show();
        }
        private void criar_revisao(object sender, RoutedEventArgs e)
        {
            if (!Utilz.Acesso(DBases.GetUserAtual().orcamento_revisar_obra))
            {
                return;
            }
            this.ObraSelecionada = ((FrameworkElement)sender).DataContext as DLM.orc.PGO_Obra;
            if (this.ObraSelecionada != null)
            {
                string revisao = Conexoes.Utilz.Prompt("Digite o nome da revisão", "", this.ObraSelecionada.Revisao, false, "", false, 3).ToUpper();
                if (revisao == "")
                {
                    Conexoes.Utilz.Alerta("Cancelado", "", MessageBoxImage.Asterisk);
                    return;
                }
                if (revisao.Length != 3)
                {
                    Conexoes.Utilz.Alerta("Revisão deve conter 3 caracteres.", "", MessageBoxImage.Asterisk);
                    return;
                }
                if (this.ObraSelecionada.Revisoes.Find(X => X.Revisao.ToUpper() == revisao) != null)
                {
                    Conexoes.Utilz.Alerta("Já existe uma revisão com esse nome", "", MessageBoxImage.Asterisk);
                    return;
                }
                var t = PGOVars.GetDbOrc().CriarRevisao(this.ObraSelecionada, revisao);
                if (t != null)
                {
                    this.ObraSelecionada.Pai.Revisoes.Add(t);
                }
                this.Update(true);
                this.Filtrar.Text = ObraSelecionada.Contrato;
            }
        }
        private void meus_acessos(object sender, RoutedEventArgs e)
        {
            Utilz.VerAcessos(DBases.GetUserAtual());
        }
        private void mostra_menu_endereco(object sender, RoutedEventArgs e)
        {
            ExplorerPLM.Menus.Fretes mm = new ExplorerPLM.Menus.Fretes(new Rotas());
            mm.Show();
        }
        private void gestao_fretes(object sender, RoutedEventArgs e)
        {
            if (!Utilz.Acesso(DBases.GetUserAtual().gestao_fretes))
            {
                return;
            }
            ExplorerPLM.Menus.Gestao_Fretes mm = new ExplorerPLM.Menus.Gestao_Fretes();
            mm.Show();
        }
        private void EditarRota(object sender, RoutedEventArgs e)
        {
            DLM.orc.PGO_Obra sel = ((FrameworkElement)sender).DataContext as DLM.orc.PGO_Obra;
            if (sel != null)
            {
                EditarRota(sel);
            }
        }


        private async void calcular_fretes(object sender, RoutedEventArgs e)
        {
            List<PGO_Obra> ObrasSel = new List<PGO_Obra>();
            if(Conexoes.Utilz.Pergunta("Calcular somente rotas que não foram calculadas?"))
            {
                ObrasSel = this.Obras.FindAll(x => x.GetRotas().id <= 0);
            }
            else
            {
                ObrasSel = Conexoes.Utilz.Selecao.SelecionarObjetos(this.Obras, null);
            }
       


            if (ObrasSel.Count > 0)
            {
                if (Utilz.Pergunta("Atualizar o frete das " + ObrasSel.Count + " obras selecionadas?"))
                {
                    Conexoes.Utilz.Wait().Show();
                    var obras_end = ObrasSel.GroupBy(x => x.Cidade + ";" + x.Estado + ";" + x.Pais).ToList();
                    Conexoes.Utilz.Wait().SetProgresso(1, obras_end.Count, "Procurando rotas das obras...");

                    foreach (var s in obras_end)
                    {
                        var chave = s.Key.Split(';');

                        var endereco = Conexoes.DBases.GetRota(chave[0], chave[1], chave[2]);
                        if (endereco != null)
                        {
                            if (endereco.id <= 0)
                            {
                                await endereco.Pesquisar();
                                await endereco.GetRotas(true);
                            }
                            endereco.RecalcularFrete();
                            endereco.Salvar();
                            var obras = s.ToList();
                            foreach (var ob in obras)
                            {
                                ob.SetSalvaRota(endereco);
                            }
                        }

                        Conexoes.Utilz.Wait().somaProgresso();
                    }

                    Conexoes.Utilz.Wait().Close();
                }
            }
        }

        private void add_obra(object sender, RoutedEventArgs e)
        {
            MenuNovaObra = new NovaObra();
            MenuNovaObra.Obra.Tipo = Tipo_Orcamento.Orçamento;
            NovaObra();
        }

        private void NovaObra()
        {
            MenuNovaObra.ShowDialog();
            if (MenuNovaObra.Editado)
            {
                VerificarTemplate(MenuNovaObra.Obra);
                EditarRota(MenuNovaObra.Obra);
                Update(true);
                this.Filtrar.Text = MenuNovaObra.Obra.Contrato;
            }
        }
        private void editar_informacoes(object sender, RoutedEventArgs e)
        {
            DLM.orc.PGO_Obra sel = ((FrameworkElement)sender).DataContext as DLM.orc.PGO_Obra;
            if (sel == null) { return; }
            MenuNovaObra = new NovaObra(sel);
            MenuNovaObra.ShowDialog();
        }
        private void Cadastro_Grupo_de_Mercadorias(object sender, RoutedEventArgs e)
        {
            if (!Utilz.Acesso(DBases.GetUserAtual().orcamento_cadastro_lista_tecnica))
            {
                return;
            }
            EditarObjetoOrcamento mm = new EditarObjetoOrcamento(EditarObjetoOrcamento.Tipologia.Grupo_De_Mercadoria);
            mm.Show();
        }
        private void Cadastro_Grupos(object sender, RoutedEventArgs e)
        {
            if (!Utilz.Acesso(DBases.GetUserAtual().orcamento_cadastro_lista_tecnica))
            {
                return;
            }
            EditarObjetoOrcamento mm = new EditarObjetoOrcamento(EditarObjetoOrcamento.Tipologia.Grupo);
            mm.Show();
        }
        private void Cadastro_Locais(object sender, RoutedEventArgs e)
        {
            if (!Utilz.Acesso(DBases.GetUserAtual().orcamento_cadastro_lista_tecnica))
            {
                return;
            }
            EditarObjetoOrcamento mm = new EditarObjetoOrcamento(EditarObjetoOrcamento.Tipologia.Local);
            mm.Show();
        }
        private void gestao_arvore(object sender, RoutedEventArgs e)
        {
            if (!Utilz.Acesso(DBases.GetUserAtual().orcamento_cadastro_lista_tecnica))
            {
                return;
            }
            Gestao_Arvore mm = new Gestao_Arvore();
            mm.Show();
        }
        private void ajustar_ranges(object sender, RoutedEventArgs e)
        {
            List<PGO_Obra> Obs = lista.SelectedItems.Cast<PGO_Obra>().ToList().SelectMany(x => x.Revisoes).ToList();
            //Obs.AddRange(lista.SelectedItems.Cast<Obra>().ToList());

            //Obs.AddRange(lista.SelectedItems.Cast<Obra>().ToList());
            if (Obs.Count > 0)
            {
                if (Utilz.Pergunta("Tem certeza que deseja atualizar as " + Obs.Count + " obras selecionadas?"))
                {
                    bool apaga_zeradas = Utilz.Pergunta("Apagar ranges inválidos? (Resquícios do ORC que só geram linhas inúteis no banco de dados. \nAtenção!!!! \n Ao selecionar essa opção, a obra fica incompatível com o ORC antigo.)");
                    ControleWait w = Conexoes.Utilz.Wait(Obs.Count, "Atualizando...");
                    foreach (var ob in Obs)
                    {
                        w.somaProgresso(ob.ToString());
                        ob.GetRanges();
                        Apagar(apaga_zeradas, ob);

                    }
                    w.Close();

                }
            }
        }
        private static void Apagar(bool apaga_zeradas, PGO_Obra ob)
        {
            if (apaga_zeradas)
            {
                Conexoes.DBases.GetDB_Orcamento().Comando($"DELETE FROM {Cfg.Init.db_orcamento}.{Cfg.Init.tb__ranges} where id_obra={ob.id} and quantidade is null");
                ob.SetValor("nova", true.ToString());
                ob.nova = true;
            }
        }
        private void Cadastro_Tipos_Pintura(object sender, RoutedEventArgs e)
        {
            if (!Utilz.Acesso(DBases.GetUserAtual().orcamento_cadastro_lista_tecnica))
            {
                return;
            }
            EditarObjetoOrcamento mm = new EditarObjetoOrcamento(EditarObjetoOrcamento.Tipologia.Tipo_Pintura);
            mm.Show();
        }
        private void apaga_revisao(object sender, RoutedEventArgs e)
        {
            if (!Utilz.Acesso(DBases.GetUserAtual().orcamento_apagar_obra))
            {
                return;
            }
            if (Abertas.Count > 0)
            {
                Conexoes.Utilz.Alerta("Há Obras Abertas. Só é possível apagar revisões com todas as obras fechadas.");
                return;
            }

            this.ObraSelecionada = ((FrameworkElement)sender).DataContext as DLM.orc.PGO_Obra;
            if (this.ObraSelecionada != null)
            {

                if (!Utilz.Pergunta("Você tem certeza que deseja apagar a revisão " + this.ObraSelecionada + " não é possível desfazer."))
                {
                    return;
                }
                PGOVars.GetDbOrc().Apagar(this.ObraSelecionada);
                this.Update(true);

            }
        }
        private void arquivar_obra(object sender, RoutedEventArgs e)
        {
            if (!Utilz.Acesso(DBases.GetUserAtual().orcamento_arquivar))
            {
                return;
            }
            this.ObraSelecionada = ((FrameworkElement)sender).DataContext as DLM.orc.PGO_Obra;
            if (this.ObraSelecionada != null)
            {

                if (!Utilz.Pergunta("Você tem certeza que deseja arquivar a revisão " + this.ObraSelecionada + " não é possível desfazer."))
                {
                    return;
                }
                PGOVars.GetDbOrc().Arquivar(this.ObraSelecionada);
                this.Update(true);
            }
        }
        private void editar_padroes_nacional(object sender, RoutedEventArgs e)
        {
            if (!Utilz.Acesso(DBases.GetUserAtual().orcamento_cadastro_lista_tecnica))
            {
                return;
            }
            MenuNovaObra = new NovaObra(PGOVars.GetDbOrc().GetPadrao_Nacional());
            MenuNovaObra.nome.IsEnabled = false;
            MenuNovaObra.check_nacional.IsEnabled = false;
            MenuNovaObra.contrato.IsEnabled = false;
            MenuNovaObra.Show();
        }
        private void editar_padroes_exportacao(object sender, RoutedEventArgs e)
        {
            if (!Utilz.Acesso(DBases.GetUserAtual().orcamento_cadastro_lista_tecnica))
            {
                return;
            }
            MenuNovaObra = new NovaObra(PGOVars.GetDbOrc().GetPadrao_Exportacao());
            MenuNovaObra.nome.IsEnabled = false;
            MenuNovaObra.check_nacional.IsEnabled = false;
            MenuNovaObra.contrato.IsEnabled = false;
            MenuNovaObra.Show();
        }
        private void ver_folha_margem(object sender, RoutedEventArgs e)
        {
            DLM.orc.PGO_Obra sel = ((FrameworkElement)sender).DataContext as DLM.orc.PGO_Obra;
            PGO.Tela_Folha_Margem mm = new PGO.Tela_Folha_Margem(sel);
            mm.Show();
        }
        private void Atualizar_estrutura(object sender, RoutedEventArgs e)
        {
            Update(true);

        }
        private void consulta_estimativo(object sender, RoutedEventArgs e)
        {
            try
            {
                PGO.Justin_Tela mm = new PGO.Justin_Tela();
                mm.Show();
            }
            catch (Exception ex)
            {
                Conexoes.Utilz.Alerta(ex);
            }
        }
        private void abre_team_viewer(object sender, RoutedEventArgs e)
        {
            var s = $@"{Cfg.Init.RaizBinarios}Lisps\TeamViewer\Updater.exe";
            if (File.Exists(s))
            {
                Conexoes.Utilz.Abrir(s);
            }
            else
            {
                Conexoes.Utilz.Alerta("Arquivo não encontrado.\n" + s);
            }

        }
        private void abre_etapas(object sender, RoutedEventArgs e)
        {
            DLM.orc.PGO_Obra sel = ((FrameworkElement)sender).DataContext as DLM.orc.PGO_Obra;
            if (sel == null) { return; }

            PGO.Etapas mm = new PGO.Etapas(sel);
            mm.Closed += Mm_Closed2;
            this.Visibility = Visibility.Collapsed;
            mm.Show();
        }
        private void Mm_Closed2(object sender, EventArgs e)
        {
            this.Visibility = Visibility.Visible;
        }
        private void gerar_etapas(object sender, RoutedEventArgs e)
        {
            var sel = Conexoes.Utilz.Selecao.SelecionarObjetos(this.Obras.SelectMany(x => x.Revisoes).ToList(), true);
            if (sel.Count > 0)
            {
                Conexoes.ControleWait w = Conexoes.Utilz.Wait(sel.Count, "Gerando...");
                foreach (PGO_Obra s in sel)
                {
                    if (s.GetEtapas().Count == 0)
                    {
                        s.CriarEtapas();

                    }
                    w.somaProgresso();
                }
                w.Close();
                Update(true);
            }
        }
        private void pmp_orcamento(object sender, RoutedEventArgs e)
        {
            PGO.TelaPMP mm = new PGO.TelaPMP(this.Obras);
            mm.Show();
        }
        private void Cadastro_FERTS(object sender, RoutedEventArgs e)
        {
            if (!Utilz.Acesso(DBases.GetUserAtual().orcamento_cadastro_lista_tecnica))
            {
                return;
            }
            EditarObjetoOrcamento mm = new EditarObjetoOrcamento(EditarObjetoOrcamento.Tipologia.FERT);
            mm.Show();
        }
        private void cadastro_frentes(object sender, RoutedEventArgs e)
        {
            EditarObjetoOrcamento mm = new EditarObjetoOrcamento(EditarObjetoOrcamento.Tipologia.Frente);
            mm.Show();
        }
        private void cadastro_ferts_e_cronograma(object sender, RoutedEventArgs e)
        {
            PGO.Listas_Tecnicas_Cronograma mm = new PGO.Listas_Tecnicas_Cronograma();
            mm.Show();
        }

        private void add_sec(object sender, RoutedEventArgs e)
        {
            MenuNovaObra = new NovaObra();
            MenuNovaObra.Obra.Revisao = "R00";
            MenuNovaObra.Obra.Contrato = "SEC" + (this.Obras.FindAll(x => x.Tipo == Tipo_Orcamento.SEC).Max(x=> Conexoes.Utilz.Int(x.Contrato.Replace("SEC","")) + 1).ToString().PadLeft(9, '0'));
            MenuNovaObra.Obra.Tipo = Tipo_Orcamento.SEC;
            NovaObra();
        }
        private void abre_etapas_lista_de_peças(object sender, RoutedEventArgs e)
        {
            DLM.orc.PGO_Obra sel = ((FrameworkElement)sender).DataContext as DLM.orc.PGO_Obra;
            if (sel == null) { return; }

            ExplorerPLM.Utilidades.VerMateriais(sel);
        }
        private void editar_observacoes(object sender, RoutedEventArgs e)
        {
            DLM.orc.PGO_Obra sel = ((FrameworkElement)sender).DataContext as DLM.orc.PGO_Obra;
            if (sel == null) { return; }
            sel.EditarObservacoes();
        }
        private void testar_mps(object sender, RoutedEventArgs e)
        {
            if (!Utilz.Acesso(DBases.GetUserAtual().orcamento_atualizar_db))
            {
                return;
            }
            DBases.GetBancoRM().SetMP_Custom(true, true);
        }
        private void lista_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lista.SelectedItem != null)
            {
                PGO_Obra ob = lista.SelectedItem as PGO_Obra;
                ZoomNaObra(ob);
            }
        }
        private void Filtrar_TextChanged(object sender, TextChangedEventArgs e)
        {
           if (Filtrar.Text != "Pesquisar...")
            {
                CollectionViewSource.GetDefaultView(lista.ItemsSource).Refresh();
            }
        }
        private bool FiltroFuncao(object item)
        {
            if (Filtrar.Text == "Pesquisar...") { return true; }
            if (String.IsNullOrEmpty(Filtrar.Text))
                return true;

            return Conexoes.Utilz.Contem(item, Filtrar.Text);

        }

        private void ver_logs(object sender, RoutedEventArgs e)
        {
            DLM.log.PastaLog.Abrir();
        }

        private void filtrar_obras(object sender, RoutedEventArgs e)
        {
            filtrar_obras();
        }

        private void filtrar_obras()
        {
            if (ch_ext == null) { return; }
            var obras = this.Obras.FindAll(x => x.Nome != "PADRÃO EXPORTAÇÃO" && x.Nome != "PADRÃO NACIONAL");
            List<PGO_Obra> filtro = new List<PGO_Obra>();
            if ((bool)ch_ext.IsChecked)
            {
                filtro = obras.FindAll(x => x.Tipo == Tipo_Orcamento.SEC);
            }
            else if ((bool)ch_meu.IsChecked)
            {
                filtro = obras.FindAll(x => x.Orcamentista == Global.UsuarioAtual);
            }
            else if ((bool)ch_orc.IsChecked)
            {
                filtro = obras.FindAll(x => x.Tipo == Tipo_Orcamento.Orçamento);
            }
            else if ((bool)ch_tudo.IsChecked)
            {
                filtro.AddRange(obras);
            }

            this.lista.ItemsSource = null;
            this.lista.ItemsSource = filtro;
            CollectionViewSource.GetDefaultView(lista.ItemsSource).Filter = FiltroFuncao;
        }
    }
}
