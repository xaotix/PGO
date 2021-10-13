using Conexoes;
using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PGO
{
    /// <summary>
    /// Interaction logic for Justin_Tela.xaml
    /// </summary>
    public partial class Justin_Tela : ModernWindow
    {
        public string var { get; set; } = "";
        public Conexoes.Orcamento.Consulta_Justin Dados { get; set; } = new Conexoes.Orcamento.Consulta_Justin();
        public Justin_Tela()
        {
            InitializeComponent();
            navegador.Navigate("about:blank");
            Dados = new Conexoes.Orcamento.Consulta_Justin(Conexoes.Orcamento.PGOVars.DbOrc.Justin);
            this.DataContext = this;

            //this.vao_principal.ItemsSource = Dados.Justin.vaos_principais;
            //this.vao_secundaria.ItemsSource = Dados.Justin.vaos_secundaria;
            //this.carga_de_utiliades.ItemsSource = Dados.Justin.cargas_de_utilidade;
            //this.carga_de_vento.ItemsSource = Dados.Justin.cargas_de_vento;


            //this.vao_principal.Text = Dados.vao_estrutura_principal.ToString();
            //this.vao_secundaria.ItemsSource = Dados.vao_estrutura_secundaria.ToString();
            //this.carga_de_utiliades.ItemsSource = Dados.carga_de_utilidades.ToString();
            //this.carga_de_vento.ItemsSource = Dados.carga_de_vento.ToString();
            this.var = Utilz.RandomString(3);
        }

        private void calcular(object sender, SelectionChangedEventArgs e)
        {

            Calcular();
        }

        private void Calcular()
        {
            if (Dados == null)
            {
                return;
            }
            if (Dados.vao_estrutura_principal == 0 | Dados.vao_estrutura_secundaria == 0 | Dados.vao_estrutura_secundaria_fechamento == 0 | /*Dados.carga_de_utilidades == 0 |*/ Dados.carga_de_vento == 0)
            {
                return;
            }


            this.Dados.Calcular();
            string raiz = System.Windows.Forms.Application.StartupPath;
            var p = Conexoes.Utilz.CriarPasta(raiz, "estimativos");
            var template = raiz + @"\template_justin.htm";
            var destino = p + @"\calculo_margem_" + var + ".htm";
            if (!File.Exists(template))
            {
                Conexoes.Utilz.Alerta("Arquivo de sistema não encontrado. " + template, "", MessageBoxImage.Error);
            }
        retentar:
            if (File.Exists(destino))
            {
                try
                {
                    File.Delete(destino);
                }
                catch (Exception ex)
                {
                    if (Conexoes.Utilz.Pergunta("Algo de errado aconteceu. Tentar novamente?\n" + ex.Message))
                    {
                        goto retentar;
                    }
                    return;
                }
            }

            var t = Conexoes.Utilz.Arquivo.Ler(template);
            for (int i = 0; i < t.Count; i++)
            {
                t[i] = t[i].Replace("$software$",
                    System.Windows.Forms.Application.ProductName + " - v." + System.Windows.Forms.Application.ProductVersion + " - User: " + Vars.UsuarioAtual + " - "
                    + DateTime.Now.ToLongDateString()
                    + "<br>Consulta realizada num total de " + this.Dados.Justin.Justin_db.Count + " análises."
                    );


                t[i] = t[i].Replace("$A$", Dados.vao_estrutura_principal.ToString("#.##"));
                t[i] = t[i].Replace("$B$", Dados.vao_estrutura_secundaria.ToString("#.##"));
                t[i] = t[i].Replace("$C$", Dados.carga_de_utilidades.ToString("#.##"));
                t[i] = t[i].Replace("$D$", Dados.carga_de_vento.ToString("#.##"));
                t[i] = t[i].Replace("$E$", Dados.vao_estrutura_secundaria_fechamento.ToString("#.##"));
                t[i] = t[i].Replace("$F$", Dados.exportacao ? "SIM" : "NÃO");
                t[i] = t[i].Replace("$G$", Dados.sismo ? "SIM" : "NÃO");


                t[i] = t[i].Replace("$00$", Dados.terca_0t_kgm2.ToString("#.##"));
                t[i] = t[i].Replace("$01$", Dados.terca_0t_kgm.ToString("#.##"));
                t[i] = t[i].Replace("$02$", Dados.terca_tipo.ToString("#.##"));

                t[i] = t[i].Replace("$10$", Dados.terca_fech_0t_km2.ToString("#.##"));
                t[i] = t[i].Replace("$11$", Dados.terca_fech_0t_km.ToString("#.##"));
                t[i] = t[i].Replace("$12$", Dados.terca_tipo_fechamento.ToString("#.##"));

                t[i] = t[i].Replace("$30$", Dados.mj_pintada_0t_kgm2.ToString("#.##"));
                t[i] = t[i].Replace("$31$", Dados.mj_pintada_0t_kgm.ToString("#.##"));

                t[i] = t[i].Replace("$32$", Dados.mj_pintada_3t_kgm2.ToString("#.##"));
                t[i] = t[i].Replace("$33$", Dados.mj_pintada_3t_kgm.ToString("#.##"));

                t[i] = t[i].Replace("$34$", Dados.mj_pintada_5t_kgm2.ToString("#.##"));
                t[i] = t[i].Replace("$35$", Dados.mj_pintada_5t_kgm.ToString("#.##"));



                t[i] = t[i].Replace("$40$", Dados.mj_galvanizada_0t_kgm2.ToString("#.##"));
                t[i] = t[i].Replace("$41$", Dados.mj_galvanizada_0t_kgm.ToString("#.##"));

                t[i] = t[i].Replace("$42$", Dados.mj_galvanizada_3t_kgm2.ToString("#.##"));
                t[i] = t[i].Replace("$43$", Dados.mj_galvanizada_3t_kgm.ToString("#.##"));

                t[i] = t[i].Replace("$44$", Dados.mj_galvanizada_5t_kgm2.ToString("#.##"));
                t[i] = t[i].Replace("$45$", Dados.mj_galvanizada_5t_kgm.ToString("#.##"));



                t[i] = t[i].Replace("$50$", Dados.medabar_0t_kgm2.ToString("#.##"));
                t[i] = t[i].Replace("$51$", Dados.medabar_0t_kgm.ToString("#.##"));

                t[i] = t[i].Replace("$52$", Dados.medabar_3t_kgm2.ToString("#.##"));
                t[i] = t[i].Replace("$53$", Dados.medabar_3t_kgm.ToString("#.##"));

                t[i] = t[i].Replace("$54$", Dados.medabar_5t_kgm2.ToString("#.##"));
                t[i] = t[i].Replace("$55$", Dados.medabar_5t_kgm.ToString("#.##"));




                t[i] = t[i].Replace("$60$", Dados.pilar_metalico_kgm2.ToString("#.##"));
                t[i] = t[i].Replace("$61$", Dados.pilar_metalico_kgm.ToString("#.##"));

                t[i] = t[i].Replace("$70$", Dados.pilar_de_concreto_kgm2.ToString("#.##"));
                t[i] = t[i].Replace("$71$", Dados.pilar_de_concreto_kgm.ToString("#.##"));
            }

            Conexoes.Utilz.Arquivo.Gravar(destino, t);

            navegador.Navigate(String.Format("file:///{0}" + "calculo_margem_" + var + ".htm", p.Replace(@"\\", @"\")));
        }

        private void calcular(object sender, RoutedEventArgs e)
        {
            Calcular();
        }

        private void ModernWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Utilz.LerVars(this.setup, Vars.ArqASetupUser, "Justin");
        }

        private void ModernWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Utilz.GravaVars(this.setup, Vars.ArqASetupUser, "Justin");
        }
    }

}
