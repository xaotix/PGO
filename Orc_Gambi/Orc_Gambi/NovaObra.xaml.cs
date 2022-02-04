using Conexoes;
using DLM.orc;
using DLM.vars;
using ExplorerPLM;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace PGO
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
        public DLM.orc.PGO_Obra Obra { get; set; } = new DLM.orc.PGO_Obra();
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
            var segmentos = Conexoes.DBases.GetSegmentos();
            if(segmentos.Count>0)
            {
                this.Obra.Segmento = segmentos.FindAll(x => x.ATIVO)[0];
            }

            this.Obra.Orcamentista = Global.UsuarioAtual;
            this.Obra.Revisao = "R00";
            this.Obra.getDados(PGOVars.GetDbOrc().GetPadrao_Nacional());

            if (this.Obra.Tipo == Tipo_Orcamento.SEC)
            {
                contrato.MaxLength = 12;
            }
            else
            {
                contrato.MaxLength = 6;

            }
        }

        public NovaObra(DLM.orc.PGO_Obra Obra)
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
            var t = Conexoes.Utilz.Selecao.SelecionarObjeto(Conexoes.DBases.GetSegmentos().FindAll(x => x.ATIVO), null);

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
                if (DLM.vars.PGOVars.GetDbOrc().GetObrasOrcamento().Find(x => x.Contrato == this.Obra.Contrato && x.Revisao == this.Obra.Revisao) != null)
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

            this.Obra.Custos.Tipo_Margem = (Tipo_Margem)tipo_de_calculo.SelectedItem;

            this.Obra.SalvarTudo();
            if (this.Obra.id > 0)
            {
                if (mudou_cotacao)
                {
                    this.Obra.AtualizarDadosDeCustos(false);
                }
            }
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
            var sel = Conexoes.Utilz.Selecao.SelecionarObjeto(DLM.vars.PGOVars.GetDbOrc().GetTratamentos(), null) as DLM.orc.Tratamento;
            if (sel != null)
            {
                this.Obra.SetTratamento(sel);
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
                Templates.IsEnabled = false;
                check_nacional.IsEnabled = false;
            }

            this.tipo_de_calculo.ItemsSource = Enum.GetValues(typeof(Tipo_Margem)).Cast<Tipo_Margem>();
            txt_cotacao.IsEnabled = (bool)check_nacional.IsChecked != true;

        }

        private void define_template(object sender, RoutedEventArgs e)
        {
            var sel = Conexoes.Utilz.Selecao.SelecionarObjeto(DLM.vars.PGOVars.GetDbOrc().GetTemplates(), null, "Selecione um Template");
            if (sel != null)
            {
                this.Obra.SetTemplate(sel as Template);
            }
        }

        private void set_dados(object sender, RoutedEventArgs e)
        {
            if (this.Obra.id > 0) { return; }
            this.Obra.getDados(this.Obra.Nacional ? PGOVars.GetDbOrc().GetPadrao_Nacional() : PGOVars.GetDbOrc().GetPadrao_Exportacao());
            txt_cotacao.IsEnabled = (bool)check_nacional.IsChecked != true;
        }

        private void mudou(object sender, TextChangedEventArgs e)
        {
            mudou_cotacao = true;
        }


    }
}
