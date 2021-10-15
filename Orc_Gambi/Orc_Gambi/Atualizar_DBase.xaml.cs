﻿using Conexoes.Orcamento;
using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace Orc_Gambi
{
    /// <summary>
    /// Interaction logic for Atualizar_DBase.xaml
    /// </summary>
    public partial class Atualizar_DBase : ModernWindow
    {
        public Atualizar_DBase()
        {
            InitializeComponent();
        }
        public List<Produto> Produtos { get; set; } = new List<Produto>();
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            arquivo.Text = Conexoes.Utilz.Abrir_String("xlsx", "Selecione o arquivo", "Arquivo DBase");
            if (File.Exists(arquivo.Text))
            {
                string planilha = "";
                List<List<object>> Linhas = Conexoes.Funcoes.GetLista(Conexoes.Funcoes.LerExcel(arquivo.Text, out planilha, false));
                Produtos = new List<Produto>();
                if (Linhas.Count > 0)
                {
                    Conexoes.ControleWait w = Conexoes.Utilz.Wait(Linhas.Count, "Lendo Excel...");
                    foreach (var l in Linhas)
                    {
                        try
                        {

                            Produtos.Add(PGOVars.DbOrc.GetProduto(l));
                            w.somaProgresso();
                        }
                        catch (Exception ex)
                        {
                            Conexoes.Utilz.Alerta(ex.Message + "\n" + ex.StackTrace);
                        }
                    }
                    w.Close();
                    Lista_Ranges.ItemsSource = Produtos.Select(x => x.Novo);
                }

            }
        }

        private void ModernWindow_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void realizar_backup(object sender, RoutedEventArgs e)
        {
            string destino = Conexoes.Utilz.SalvarArquivo("sql");
            if (destino != "")
            {
                Conexoes.DBases.GetDB_Orcamento().Backup(destino, Conexoes.Cfg.Init.db_orcamento, new List<string> { PGOVars.Config.tabela_id_produtos });
                Conexoes.Utilz.Alerta("Backup realizado!");
            }
        }

        private void Executa_Update(object sender, RoutedEventArgs e)
        {
            if (Lista_Ranges.Items.Count == 0) { Conexoes.Utilz.Alerta("Nenhum item na lista. Carregue um arquivo de itens antes de iniciar."); }
            if (Conexoes.Utilz.Pergunta("Tem certeza que deseja atualizar/criar os " + Lista_Ranges.Items.Count + " produtos?"))
            {
                Conexoes.ControleWait w = Conexoes.Utilz.Wait(Lista_Ranges.Items.Count, "Atualizando...");
                foreach (Produto pd in Lista_Ranges.Items)
                {
                    pd.Novo.Salvar(true);

                    w.somaProgresso();
                }
                w.Close();
                Conexoes.Utilz.Alerta("Dados atualizados!");
                Lista_Ranges.ItemsSource = null;
                PGOVars.DbOrc.GetProdutos(false);
            }
        }
    }
}