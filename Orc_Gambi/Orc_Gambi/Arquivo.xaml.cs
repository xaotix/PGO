using Conexoes;
using DLM.orc;
using DLM.vars;
using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace PGO
{
    /// <summary>
    /// Interaction logic for Arquivo.xaml
    /// </summary>
    public partial class Arquivo : ModernWindow
    {
        public Arquivo()
        {
            InitializeComponent();
            Update();

        }
        private void Update()
        {
            this.Lista_Arquivo.ItemsSource = null;
            this.Lista.ItemsSource = null;
            var t1 = DBases.GetDbOrc().GetObras_Arquivadas();
            var t2 = DBases.GetDbOrc().GetObrasOrcamento();
            this.Lista_Arquivo.ItemsSource = t1;
            this.Lista.ItemsSource = t2;
            CollectionViewSource.GetDefaultView(Lista.ItemsSource).Filter = Filtro_Sistema_Funcao;
            CollectionViewSource.GetDefaultView(Lista_Arquivo.ItemsSource).Filter = Filtro_Arquivo_Funcao;
        }
        private void desarquivar_obras(object sender, RoutedEventArgs e)
        {
            List<PGO_Obra> sel = Lista_Arquivo.SelectedItems.Cast<PGO_Obra>().ToList();
            if (sel.Count > 0)
            {
                if (Utilz.Pergunta("Tem certeza que deseja desarquivar as " + sel.Count + " obras selecionadas?"))
                {
                    ControleWait w = Conexoes.Utilz.Wait(sel.SelectMany(x => x.Revisoes).Count(), "Desarquivando obras...");
                    foreach (var t in sel)
                    {
                        foreach (var ss in t.Revisoes)
                        {
                            DBases.GetDbOrc().Desarquivar(ss);
                            w.somaProgresso();
                        }
                    }
                    w.Close();
                    Update();
                }
            }
        }
        private void arquivar_obras(object sender, RoutedEventArgs e)
        {
            List<PGO_Obra> sel = Lista.SelectedItems.Cast<PGO_Obra>().ToList();
            if (sel.Count > 0)
            {
                if (Utilz.Pergunta("Tem certeza que deseja arquivar as " + sel.Count + " obras selecionadas?"))
                {
                    ControleWait w = Conexoes.Utilz.Wait(sel.SelectMany(x => x.Revisoes).Count(), "Arquivando obras...");
                    foreach (var t in sel)
                    {
                        foreach (var ss in t.Revisoes)
                        {
                            DBases.GetDbOrc().Arquivar(ss);
                            w.somaProgresso();
                        }
                    }
                    w.Close();
                    Update();
                }
            }
        }

        private void Filtro_Sistema_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (Filtro_Sistema.Text != "Pesquisar...")
            {
                CollectionViewSource.GetDefaultView(Lista.ItemsSource).Refresh();
            }
        }
        private bool Filtro_Sistema_Funcao(object item)
        {
            if (Filtro_Sistema.Text == "Pesquisar...") { return true; }
            if (String.IsNullOrEmpty(Filtro_Sistema.Text))
                return true;

            return Conexoes.Utilz.Contem(item, Filtro_Sistema.Text);

        }
        private bool Filtro_Arquivo_Funcao(object item)
        {
            if (Filtro_Arquivo.Text == "Pesquisar...") { return true; }
            if (String.IsNullOrEmpty(Filtro_Arquivo.Text))
                return true;

            return Conexoes.Utilz.Contem(item, Filtro_Arquivo.Text);

        }
        private void Filtro_Arquivo_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (Filtro_Arquivo.Text != "Pesquisar...")
            {
                CollectionViewSource.GetDefaultView(Lista_Arquivo.ItemsSource).Refresh();
            }
        }
    }
}
