using Conexoes;
using Conexoes.Orcamento;
using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Orc_Gambi
{
    /// <summary>
    /// Interaction logic for Adiciona_Peca.xaml
    /// </summary>
    public partial class Adiciona_Peca : ModernWindow
    {
        public View Selecao { get; set; } = new View();
        public Adiciona_Peca(PecaDB Peca, Produto Produto)
        {
            InitializeComponent();
            this.Selecao = new View();
            this.Selecao.Peca = new PecaDB(Peca, Produto);
            this.Selecao.Produto = Produto;
            this.DataContext = null;
            this.DataContext = this.Selecao;
            MenuPDF = new ExplorerPLM.Preview.PDF_Preview();
            this.container_pdf.Children.Add(MenuPDF);

        }
        public ExplorerPLM.Preview.PDF_Preview MenuPDF;

        private void Procurar_Peca(object sender, RoutedEventArgs e)
        {


            switch (Selecao.Peca.Tipo)
            {

                default: break;
                case "RMA":
                    RMA RMA = Conexoes.Utilz.SelecionarObjeto(DBases.GetBancoRM().GetRMAt(), null);
                    if (RMA != null)
                    {
                        this.Selecao.Peca.id_peca = RMA.id_db;
                        this.Selecao.Peca.GetPeca();
      
                    }
                    break;
                case "RME":
                    RME RME = Conexoes.Utilz.SelecionarObjeto(DBases.GetBancoRM().GetRMEt().FindAll(x => x.DESTINO == "RME"), null);
                    if (RME != null)
                    {
                        this.Selecao.Peca.id_peca = RME.id_db;
                        this.Selecao.Peca.GetPeca();
                        AbrePDF();

              
                    }
                    break;
                case "RMU":
                    RMU RMU = Conexoes.Utilz.SelecionarObjeto(DBases.GetBancoRM().GetRMUt(), null);
                    if (RMU != null)
                    {
                        this.Selecao.Peca.id_peca = RMU.id_db;
                        this.Selecao.Peca.GetPeca();
                        AbrePDF();

                   

                    }
                    break;
                case "RMT":
                    RMT RMT = Conexoes.Utilz.SelecionarObjeto(DBases.GetBancoRM().GetRMTt(), null);
                    if (RMT != null)
                    {
                        this.Selecao.Peca.id_peca = RMT.id_telha;
                        this.Selecao.Peca.GetPeca();
                        AbrePDF();


                    }
                    break;
                case "Macros.Escada.Marinheiro":
                    Conexoes.Utilz.Alerta("Para escada apenas preencha o comprimento total desejado. Será considerado pontaletes de apoio a cada 2 metros.");
                    break;
            }



        }

        private void Bt_Acao_Click(object sender, RoutedEventArgs e)
        {

            this.DialogResult = true;
        }

        private void Tipo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Bobina sel = Conexoes.Utilz.SelecionarObjeto(DBases.GetBancoRM().GetBobinas());
            if (sel != null)
            {
                Selecao.Peca.Bobina = sel;
            }
        }

        private void ModernWindow_Closing(object sender, CancelEventArgs e)
        {
            
            Conexoes.INI.Set(Vars.ArqASetupUser, "ORCLT", "QTD", Selecao.Peca.Quantidade.ToString());
            Conexoes.INI.Set(Vars.ArqASetupUser, "ORCLT", "COMP", Selecao.Peca.Comprimento.ToString());
            Conexoes.INI.Set(Vars.ArqASetupUser, "ORCLT", "TIPO", Selecao.Peca.Tipo.ToString());
        }

        private void ModernWindow_Loaded(object sender, RoutedEventArgs e)
        {

            if (this.Selecao.Peca.Tipo == "")
            {
                this.Selecao.Peca.Tipo = Conexoes.INI.Get(Vars.ArqASetupUser, "ORCLT", "TIPO", "0");
                if (this.Selecao.Peca.Ficha == "")
                {
                    this.Selecao.Peca.Ficha = "SEM PINTURA";
                }
                if (this.Selecao.Peca.Quantidade == 0)
                {
                    this.Selecao.Peca.Quantidade = Utilz.Double(Conexoes.INI.Get(Vars.ArqASetupUser, "ORCLT", "QTD", "0"));
                }
                if (this.Selecao.Peca.Comprimento == 0)
                {
                    this.Selecao.Peca.Comprimento = Utilz.Double(Conexoes.INI.Get(Vars.ArqASetupUser, "ORCLT", "COMP", "0"));
                }
            }

            AbrePDF();
            //else
            //{
            //    this.tipo.SelectedItem = this.Selecao.Peca.Tipo;

            //}
        }

        private void AbrePDF()
        {
            if (File.Exists(this.Selecao.Peca.Peca.GetPDF()))
            {
                MenuPDF.Abrir(this.Selecao.Peca.Peca.GetPDF());
            }
        }
    }
    public class View : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public PecaDB Peca
        {
            get
            {
                return _Peca;
            }
            set
            {
                _Peca = value;
                NotifyPropertyChanged("Peca");
            }
        }
        public Produto Produto
        {
            get
            {
                return _Produto;
            }
            set
            {
                _Produto = value;
                NotifyPropertyChanged("Produto");
            }
        }
        public List<string> Tratamentos { get; set; } = new List<string>();
        public List<string> Tipos { get; set; } = new List<string>();
        private PecaDB _Peca { get; set; } = new PecaDB();
        private Produto _Produto { get; set; }
        public View()
        {
            this.Tratamentos = new its.TipoPintura().GetValues().ToList().Select(x => x.Value.ToString()).ToList();
            this.Tipos = new its.Destino().GetValues().ToList().Select(x => x.Value.ToString()).ToList();

        }
    }
}
