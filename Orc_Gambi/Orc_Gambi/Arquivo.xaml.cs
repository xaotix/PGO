using Conexoes;
using Conexoes.Orcamento;
using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace Orc_Gambi
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
            var t1 = PGOVars.DbOrc.GetObras_Arquivadas();
            var t2 = PGOVars.DbOrc.GetObras_Atuais();
            this.Lista_Arquivo.ItemsSource = t1;
            this.Lista.ItemsSource = t2;
        }

        private void desarquivar_obras(object sender, RoutedEventArgs e)
        {
            List<OrcamentoObra> sel = Lista_Arquivo.SelectedItems.Cast<OrcamentoObra>().ToList();
            if (sel.Count > 0)
            {
                if (Utilz.Pergunta("Tem certeza que deseja desarquivar as " + sel.Count + " obras selecionadas?"))
                {
                    ControleWait w = Conexoes.Utilz.Wait(sel.SelectMany(x => x.Revisoes).Count(), "Desarquivando obras...");
                    foreach (var t in sel)
                    {
                        foreach (var ss in t.Revisoes)
                        {
                            PGOVars.DbOrc.Desarquivar(ss);
                            w.somaProgresso();
                        }
                    }
                    w.Close();
                    Update();
                    //this.Close();
                }
            }
        }

        private void arquivar_obras(object sender, RoutedEventArgs e)
        {
            List<OrcamentoObra> sel = Lista.SelectedItems.Cast<OrcamentoObra>().ToList();
            if (sel.Count > 0)
            {
                if (Utilz.Pergunta("Tem certeza que deseja arquivar as " + sel.Count + " obras selecionadas?"))
                {
                    ControleWait w = Conexoes.Utilz.Wait(sel.SelectMany(x => x.Revisoes).Count(), "Arquivando obras...");
                    foreach (var t in sel)
                    {
                        foreach (var ss in t.Revisoes)
                        {
                            PGOVars.DbOrc.Arquivar(ss);
                            w.somaProgresso();
                        }
                    }
                    w.Close();
                    Update();
                    //this.Close();

                }
            }
        }

        private void backup_arquivo(object sender, RoutedEventArgs e)
        {
            string destino = Conexoes.Utilz.SelecionarPasta("Selecione o Destino");
            if (Directory.Exists(destino))
            {
                ControleWait w = Conexoes.Utilz.Wait(10, "Carregando...");
                w.somaProgresso();
                Conexoes.DBases.GetDB().Backup(destino + @"\backup_principal." + DateTime.Now.ToShortDateString().Replace("/", ".") + ".sql", PGOVars.Config.Dbase_Arquivo, new List<string> { PGOVars.Config.tabela_id_obra, PGOVars.Config.tabela_id_predio });
                w.Close();
                Conexoes.Utilz.Alerta("Backup realizado!");
            }
            else if (destino != "")
            {
                Conexoes.Utilz.Alerta("Destino inválido ou inexistente");
            }
        }

        private void backup_principal(object sender, RoutedEventArgs e)
        {
            string destino = Conexoes.Utilz.SelecionarPasta("Selecione o Destino");
            if (Directory.Exists(destino))
            {
                ControleWait w = Conexoes.Utilz.Wait(10, "Carregando...");
                w.somaProgresso();
                Conexoes.DBases.GetDB_Orcamento().Backup(destino + @"\backup_principal." + DateTime.Now.ToShortDateString().Replace("/", ".") + ".sql", Cfg.Init.db_orcamento, new List<string> { PGOVars.Config.tabela_id_obra, PGOVars.Config.tabela_id_predio });
                w.Close();
                Conexoes.Utilz.Alerta("Backup realizado!");
            }
            else if (destino != "")
            {
                Conexoes.Utilz.Alerta("Destino inválido ou inexistente");
            }
        }

        private void restaurar_principal(object sender, RoutedEventArgs e)
        {
            string destino = Conexoes.Utilz.Abrir_String("sql", "Selecione", "");
            if (File.Exists(destino))
            {
                ControleWait w = Conexoes.Utilz.Wait(10, "Carregando...");
                w.somaProgresso();
                Conexoes.DBases.GetDB_Orcamento().Importar(destino, Cfg.Init.db_orcamento);
                w.Close();
                Conexoes.Utilz.Alerta("Backup realizado!");
            }
            else if (destino != "")
            {
                Conexoes.Utilz.Alerta("Arquivo Inexistente");
            }
        }

        private void restaurar_arquivo(object sender, RoutedEventArgs e)
        {
            string destino = Conexoes.Utilz.Abrir_String("sql", "Selecione", "");
            if (File.Exists(destino))
            {
                ControleWait w = Conexoes.Utilz.Wait(10, "Carregando...");
                w.somaProgresso();
                Conexoes.DBases.GetDB().Importar(destino, PGOVars.Config.Dbase_Arquivo);
                w.Close();
                Conexoes.Utilz.Alerta("Backup realizado!");
            }
            else if (destino != "")
            {
                Conexoes.Utilz.Alerta("Arquivo Inexistente");
            }
        }
    }
}
