using Conexoes;
using Conexoes.Orcamento;
using ExplorerPLM;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Orc_Gambi
{
    /// <summary>
    /// Interaction logic for NovaObra.xaml
    /// </summary>

    public partial class NovaObra : Window
    {
        public bool Menus_Orcamento_Bool { get; set; } = true;
        public Visibility Menus_Orcamento { get; set; } = Visibility.Visible;
        bool mudou_cotacao { get; set; } = false;
        public bool Editado { get; set; } = false;
        public Conexoes.Orcamento.OrcamentoObra Obra { get; set; } = new Conexoes.Orcamento.OrcamentoObra();
        public NovaObra()
        {
            InitializeComponent();
            this.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
            if (!DBases.GetUserAtual().orcamento_ver_obras)
            {
                Menus_Orcamento = Visibility.Collapsed;
                Menus_Orcamento_Bool = false;
            }


            this.DataContext = this;
            var segmentos = Conexoes.Orcamento.PGOVars.DbOrc.GetSegmentos();
            if(segmentos.Count>0)
            {
                this.Obra.Segmento = segmentos.FindAll(x => x.ATIVO)[0];
            }

            this.Obra.Orcamentista = Vars.UsuarioAtual;
            this.Obra.Revisao = "R00";
            this.Obra.getDados(PGOVars.DbOrc.Padrao_Nacional);

            if (this.Obra.Tipo == Tipo_Orcamento.SEC)
            {
                contrato.MaxLength = 12;
            }
            else
            {
                contrato.MaxLength = 6;

            }
        }
        public NovaObra(Tipo_Orcamento tipo, string contraton)
        {
            InitializeComponent();
            this.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
            if (!DBases.GetUserAtual().orcamento_ver_obras)
            {
                Menus_Orcamento = Visibility.Collapsed;
                Menus_Orcamento_Bool = false;
            }

            this.DataContext = this;




            if (this.Obra.Tipo == Tipo_Orcamento.SEC)
            {
                contrato.MaxLength = 12;
            }
            else
            {
                contrato.MaxLength = 6;

            }
        }
        public NovaObra(Conexoes.Orcamento.OrcamentoObra Obra)
        {
            this.Obra = Obra;
            InitializeComponent();
            this.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
            this.DataContext = this;
            this.Title = "Editar Obra [" + this.Obra.ToString() + "]";
            if (this.Obra.Tipo == Tipo_Orcamento.SEC)
            {
                contrato.MaxLength = 12;
            }
            else
            {
                contrato.MaxLength = 6;

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var t = Conexoes.Utilz.SelecionarObjeto(Conexoes.Orcamento.PGOVars.DbOrc.GetSegmentos().FindAll(x => x.ATIVO), null);

            if (t != null)
            {
                if (this.Obra.Segmento.id != t.id)
                {
                    if (this.Obra.GetRanges().Count > 0)
                    {
                        if (Utilz.Pergunta("Já existem itens inseridos na obra. " +
                            "Ao alterar o setor de atividade, é necessário atualizar os custos." +
                            " Você tem certeza que deseja continuar?"))
                        {
                            this.Obra.Segmento = t;
                            this.Obra.AtualizarDadosDeCustos(false);
                        }
                        else
                        {
                            return;
                        }
                    }
                    this.Obra.Segmento = t;
                }

            }
        }
        private ExplorerPLM.Menus.Fretes menu_Fretes;
        private void editar_rota(object sender, RoutedEventArgs e)
        {
            menu_Fretes = new ExplorerPLM.Menus.Fretes(this.Obra.GetRotas());
            menu_Fretes.Show();
            menu_Fretes.Closed += rotas_fechou;
        }

        private void rotas_fechou(object sender, EventArgs e)
        {
            this.Obra.SetSalvaRota(menu_Fretes.Rotas);

        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            if (this.Obra.id > 0)
            {
                this.Obra.CarregarDados();
            }
            this.Close();

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (Obra.Bloqueado)
            {
                Conexoes.Utilz.Alerta("Obra está bloqueada para edições", "Obra Bloqueada", MessageBoxImage.Error);
                return;
            }
            
            if (this.Obra.Contrato.Length < 6)
            {
                Conexoes.Utilz.Alerta("Contrato deve conter pelo menos 6 caracteres", "Faltam dados", MessageBoxImage.Asterisk);
                return;
            }
            if (this.Obra.id <= 0)
            {
                if (this.Obra.Nome.ToUpper() == "PADRÃO NACIONAL")
                {
                    Conexoes.Utilz.Alerta("Nome da obra Inválido", "Faltam dados", MessageBoxImage.Asterisk);
                    return;
                }
                if (this.Obra.Nome.ToUpper() == "PADRÃO EXPORTAÇÃO")
                {
                    Conexoes.Utilz.Alerta("Nome da obra Inválido", "Faltam dados", MessageBoxImage.Asterisk);
                    return;
                }
                if (this.Obra.Contrato == "00E000" | this.Obra.Contrato == "NA0000")
                {
                    Conexoes.Utilz.Alerta("Contrato Inválido", "Faltam dados", MessageBoxImage.Asterisk);
                    return;
                }
                if (Conexoes.Orcamento.PGOVars.DbOrc.Obras.Find(x => x.Contrato == this.Obra.Contrato && x.Revisao == this.Obra.Revisao) != null)
                {
                    Conexoes.Utilz.Alerta("Já existe uma revisão com este nome neste contrato", "", MessageBoxImage.Asterisk);
                    return;
                }
            }
            if (this.Obra.Nome.Replace(" ", "").Length == 0)
            {
                Conexoes.Utilz.Alerta("Nome da obra não pode estar em branco.", "Faltam dados", MessageBoxImage.Asterisk);
                return;
            }
            if (this.Obra.Revisao.Replace(" ", "").Length < 3)
            {
                Conexoes.Utilz.Alerta("Revisão deve conter 3 caracteres", "Faltam dados", MessageBoxImage.Asterisk);
                return;
            }

            if (this.Obra.Tipo == Tipo_Orcamento.SEC && !this.Obra.Contrato.StartsWith("S"))
            {
                Conexoes.Utilz.Alerta("SECs devem começar com S", "Código de orçamento inválido", MessageBoxImage.Asterisk);
                return;
            }

            this.Obra.Custos.Tipo_Margem = (Conexoes.Orcamento.Tipo_Margem)tipo_de_calculo.SelectedItem;
            this.Obra.SalvarTudo();
            if (this.Obra.id > 0)
            {
                if (mudou_cotacao)
                {
                    this.Obra.AtualizarDadosDeCustos(false);
                }
            }
            //await this.Obra.Rotas.GetRotas();
            // this.Obra.Rotas.Salvar();
            // this.Obra.SetRota(this.Obra.Rotas);
            this.Editado = true;
            this.Close();

        }

        private void seleciona_tudo(object sender, RoutedEventArgs e)
        {
            TextBox tb = (sender as TextBox);
            tb.Focus();
            tb.SelectAll();
        }

        private void define_tratamento(object sender, RoutedEventArgs e)
        {
            var sel = Conexoes.Utilz.SelecionarObjeto(Conexoes.Orcamento.PGOVars.DbOrc.Tratamentos, null) as Conexoes.Orcamento.Tratamento;
            if (sel != null)
            {
                this.Obra.Tratamento = sel;
            }
        }

        private void ModernWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Obra.Tipo == Tipo_Orcamento.SEC)
            {
                header_principal.Header = "Dados da SEC";
            }
            if (this.Obra.id > 0)
            {
                //contrato.IsEnabled = false;
                //revisao.IsEnabled = false;
                //nome.IsEnabled = false;
                //Templates.IsEnabled = false;
                Templates.IsEnabled = false;
                check_nacional.IsEnabled = false;
                //revisao.IsEnabled = false;
            }

            this.tipo_de_calculo.ItemsSource = Enum.GetValues(typeof(Conexoes.Orcamento.Tipo_Margem)).Cast<Conexoes.Orcamento.Tipo_Margem>();
            txt_cotacao.IsEnabled = (bool)check_nacional.IsChecked != true;

        }

        private void define_template(object sender, RoutedEventArgs e)
        {
            var sel = Conexoes.Utilz.SelecionarObjeto(Conexoes.Orcamento.PGOVars.DbOrc.Templates, null, "Selecione um Template");
            if (sel != null)
            {
                this.Obra.SetTemplate(sel as Template);
            }
        }

        private void set_dados(object sender, RoutedEventArgs e)
        {
            if (this.Obra.id > 0) { return; }
            this.Obra.getDados(this.Obra.Nacional ? PGOVars.DbOrc.Padrao_Nacional : PGOVars.DbOrc.Padrao_Exportacao);
            txt_cotacao.IsEnabled = (bool)check_nacional.IsChecked != true;
        }

        private void mudou(object sender, TextChangedEventArgs e)
        {
            mudou_cotacao = true;
        }

        private void editar_observacoes(object sender, RoutedEventArgs e)
        {
            this.Obra.EditarObservacoes();
        }
    }
}
