using Conexoes;
using DLM.orc;
using DLM.vars;
using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Markup;

namespace PGO
{
    /// <summary>
    /// Interaction logic for Tela_Folha_Margem.xaml
    /// </summary>
    public partial class Tela_Folha_Margem : ModernWindow
    {
        public DLM.orc.Folha_Margem Margem { get; set; } = new DLM.orc.Folha_Margem();
        public DLM.orc.PGO_Obra Obra { get; set; } = new DLM.orc.PGO_Obra();
        public Tela_Folha_Margem(PGO_Obra Ob)
        {
            if (Ob.Nacional)
            {
                this.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
            }
            else
            {
                this.Language = XmlLanguage.GetLanguage(new CultureInfo("en-US").IetfLanguageTag);
            }

            this.Obra = Ob;
            if (this.Obra.Folha_Margem.Data == "")
            {
                this.Obra.Folha_Margem.Calcular(this.Obra);
                this.Obra.Salvar_Folha_Margem();
            }
            InitializeComponent();
            this.Title = "Folha Margem [" + this.Obra.ToString() + "]";
            GetMargemHTML(Ob);
            this.DataContext = this;
        }

        private void GetMargemHTML(PGO_Obra Ob)
        {

            string raiz = System.Windows.Forms.Application.StartupPath;
            var p = Utilz.CriarPasta(System.Windows.Forms.Application.StartupPath, "FOLHAS_MARGEM");
            var template = raiz + @"\FOLHA_MARGEM_MODELO.htm";
            if (!Ob.Nacional)
            {
                template = raiz + @"\FOLHA_MARGEM_MODELO_EXPO.htm";
            }
            var destino = p + @"\FOLHA_MARGEM_$ID$.htm".Replace("$ID$", Ob.id.ToString());
            if (!File.Exists(template))
            {
                Conexoes.Utilz.Alerta("Arquivo de sistema não encontrado. " + template, "", MessageBoxImage.Error);
            }

            var t = Conexoes.Utilz.Arquivo.Ler(template);
            for (int i = 0; i < t.Count; i++)
            {
                t[i] = t[i].Replace("$DOLAR$", Math.Round(Obra.cotacao, 2).ToString("R$0,0.00"));

                if (Ob.Folha_Margem.Passiva)
                {
                    t[i] = t[i].Replace("$PASSIVA1$", "Proteção Passiva: " + (Ob.Folha_Margem.Passiva_Faturamento_Direto ? "<i> (Faturamento direto)</i>" : "") + Ob.Folha_Margem.Passiva_Valor.ToString("R$0,0.00"));
                    if (Ob.Folha_Margem.Passiva_Faturamento_Direto)
                    {
                        t[i] = t[i].Replace("$PASSIVA2$", "Valor da proteção NÃO somada no valor total do contrato");

                    }
                    else
                    {
                        t[i] = t[i].Replace("$PASSIVA2$", "Do valor de proteção, foi considerado $M$% MATERIAL e $S$% SERVIÇOS"
                            .Replace("$M$", Math.Round(Ob.Folha_Margem.Passiva_Material * 100, 2).ToString())
                            .Replace("$S$", Math.Round(Ob.Folha_Margem.Passiva_Servicos * 100, 2).ToString()))
                            ;
                    }
                }
                else
                {
                    t[i] = t[i].Replace("$PASSIVA1$", "");
                    t[i] = t[i].Replace("$PASSIVA2$", "");

                }



                t[i] = t[i].Replace("$SOFTWARE$", "Tipo de cáclulo: " + this.Obra.Folha_Margem.Tipo_Margem.ToString().Replace("_", " ") + "<br>" + System.Windows.Forms.Application.ProductName + " - v." + System.Windows.Forms.Application.ProductVersion + " - User: " + Global.UsuarioAtual + " - " + DateTime.Now.ToLongDateString());
                t[i] = t[i].Replace("$TITULO$", Obra.ToString() + " - Folha Margem");
                t[i] = t[i].Replace("$DATA$", Obra.Folha_Margem.Data.ToString());
                t[i] = t[i].Replace("$CLIENTE$", Obra.Cliente.ToString());
                t[i] = t[i].Replace("$ORCAMENTISTA$", Obra.Orcamentista.ToString());
                t[i] = t[i].Replace("$PAIS$", Obra.GetRotas().Pais.ToString());
                t[i] = t[i].Replace("$CIDADE_ESTADO$", Obra.GetRotas().Cidade + " - " + Obra.GetRotas().Estado);
                t[i] = t[i].Replace("$VENDEDOR$", Obra.Comercial.ToString());
                t[i] = t[i].Replace("$SEGMENTO$", Obra.GetSegmento().ToString());
                t[i] = t[i].Replace("$CONTRATO$", Obra.Contrato.ToString());
                t[i] = t[i].Replace("$REVISAO$", Obra.Revisao.ToString());
                t[i] = t[i].Replace("$NOME_DA_OBRA$", Obra.Nome.ToString());
                t[i] = t[i].Replace("$PESO$", Math.Round(Obra.Folha_Margem.Peso_Total, 2).ToString());
                t[i] = t[i].Replace("$AREA$", Math.Round(Obra.Folha_Margem.Area_Total, 2).ToString());

                t[i] = t[i].Replace("$MONT_RS_KG$", Math.Round(Obra.Folha_Margem.Montagem_RS_KG, 2).ToString("C"));
                t[i] = t[i].Replace("$MONT_RS_M$", Math.Round(Obra.Folha_Margem.Montagem_RS_M2, 2).ToString("C"));

                t[i] = t[i].Replace("$TAXA_KG$", Math.Round(Obra.Folha_Margem.Taxa_RS_KG, 2).ToString("C"));
                t[i] = t[i].Replace("$TAXA_M2$", Math.Round(Obra.Folha_Margem.Taxa_RS_KG, 2).ToString("C"));
                t[i] = t[i].Replace("$TOTAL_CONTRATO$", Math.Round(Obra.Folha_Margem.Total_Contrato, 2).ToString("C"));

                /*RECEITA BRUTA*/
                t[i] = t[i].Replace("$RB_MATERIAL$", Math.Round(Obra.Folha_Margem.Material * 100, 2).ToString() + "%");
                t[i] = t[i].Replace("$RB_MONTAGEM$", Math.Round(Obra.Folha_Margem.Montagem * 100, 2).ToString() + "%");
                t[i] = t[i].Replace("$RB_PROJETO$", Math.Round(Obra.Folha_Margem.Projeto * 100, 2).ToString() + "%");

                t[i] = t[i].Replace("$RB_MATERIAL_VL$", Math.Round(Obra.Folha_Margem.Receita_Bruta_Material, 2).ToString("C"));
                t[i] = t[i].Replace("$RB_MONTAGEM_VL$", Math.Round(Obra.Folha_Margem.Receita_Bruta_Montagem, 2).ToString("C"));
                t[i] = t[i].Replace("$RB_PROJETO_VL$", Math.Round(Obra.Folha_Margem.Receita_Bruta_Projeto, 2).ToString("C"));


                /*IMPOSTOS*/
                t[i] = t[i].Replace("$TOTAL_IMPOSTOS$", Math.Round(Obra.Folha_Margem.Impostos_Total, 2).ToString("C"));

                t[i] = t[i].Replace("$ICMS_MATERIAL$", Math.Round(Obra.Folha_Margem.ICMS_Material * 100, 2).ToString() + "%");
                t[i] = t[i].Replace("$PIS_MATERIAL$", Math.Round(Obra.Folha_Margem.PIS_COFINS_Material * 100, 2).ToString() + "%");
                t[i] = t[i].Replace("$PIS_MONTAGEM$", Math.Round(Obra.Folha_Margem.PIS_COFINS_Montagem * 100, 2).ToString() + "%");
                t[i] = t[i].Replace("$PIS_PROJETO$", Math.Round(Obra.Folha_Margem.PIS_COFINS_Projeto * 100, 2).ToString() + "%");
                t[i] = t[i].Replace("$ISS_MONTAGEM$", Math.Round(Obra.Folha_Margem.ISS_Montagem * 100, 2).ToString() + "%");
                t[i] = t[i].Replace("$ISS_PROJETO$", Math.Round(Obra.Folha_Margem.ISS_Projeto * 100, 2).ToString() + "%");

                t[i] = t[i].Replace("$ICMS_MATERIAL_VL$", Math.Round(Obra.Folha_Margem.Impostos_ICMS_Material, 2).ToString("C"));
                t[i] = t[i].Replace("$PIS_MATERIAL_VL$", Math.Round(Obra.Folha_Margem.Impostos_PIS_COFINS_Material, 2).ToString("C"));
                t[i] = t[i].Replace("$PIS_MONTAGEM_VL$", Math.Round(Obra.Folha_Margem.Impostos_PIS_COFINS_Montagem, 2).ToString("C"));
                t[i] = t[i].Replace("$PIS_PROJETO_VL$", Math.Round(Obra.Folha_Margem.Impostos_PIS_COFINS_Projeto, 2).ToString("C"));
                t[i] = t[i].Replace("$ISS_MONTAGEM_VL$", Math.Round(Obra.Folha_Margem.Impostos_ISS_Montagem, 2).ToString("C"));
                t[i] = t[i].Replace("$ISS_PROJETO_VL$", Math.Round(Obra.Folha_Margem.Impostos_ISS_Projeto, 2).ToString("C"));


                /*RECEITA LIQUIDA*/
                t[i] = t[i].Replace("$RL_MATERIAL_VL$", Math.Round(Obra.Folha_Margem.Receita_Liquida_Material, 2).ToString("C"));
                t[i] = t[i].Replace("$RL_MONTAGEM_VL$", Math.Round(Obra.Folha_Margem.Receita_Liquida_Montagem, 2).ToString("C"));
                t[i] = t[i].Replace("$RL_PROJETO_VL$", Math.Round(Obra.Folha_Margem.Receita_Liquida_Projeto, 2).ToString("C"));

                t[i] = t[i].Replace("$RL_TOTAL$", Math.Round(Obra.Folha_Margem.Receita_Liquida_Total, 2).ToString("C"));


                /*CUSTOS_MATERIAIS*/
                t[i] = t[i].Replace("$MAT_MP$", Math.Round(Obra.Folha_Margem.Custos_Materiais_Materia_Prima, 2).ToString("C"));
                t[i] = t[i].Replace("$MAT_MODGGF$", Math.Round(Obra.Folha_Margem.Custos_Materiais_MOD_GGF, 2).ToString("C"));
                t[i] = t[i].Replace("$MAT_CONTINGENCIA$", Math.Round(Obra.Folha_Margem.Custos_Materiais_Contingencia, 2).ToString("C"));

                t[i] = t[i].Replace("$MAT_TOTAL$", Math.Round(Obra.Folha_Margem.Custos_Materiais_Total, 2).ToString("C"));

                t[i] = t[i].Replace("$CP_TOTAL$", Math.Round(Obra.Folha_Margem.Total_Custos_de_Projeto, 2).ToString("C"));


                /*CUSTOS_MONTAGEM*/
                t[i] = t[i].Replace("$MO_CUSTOS$", Math.Round(Obra.Folha_Margem.Montagem_Custos, 2).ToString("C"));
                t[i] = t[i].Replace("$MO_SUPERVISAO$", Math.Round(Obra.Folha_Margem.Montagem_Supervisao, 2).ToString("C"));
                t[i] = t[i].Replace("$MO_OUTROS$", Math.Round(Obra.Folha_Margem.Montagem_Outros, 2).ToString("C"));

                t[i] = t[i].Replace("$MO_TOTAL$", Math.Round(Obra.Folha_Margem.Montagem_Total, 2).ToString("C"));

                t[i] = t[i].Replace("$RA_TOTAL$", Math.Round(Obra.Folha_Margem.Total_de_Rateio, 2).ToString("C"));

                t[i] = t[i].Replace("$GL_TOTAL$", Math.Round(Obra.Folha_Margem.Total_de_Gastos_Logisticos, 2).ToString("C"));


                /*DESPESAS_GERAIS*/
                t[i] = t[i].Replace("$SEGURO$", Math.Round(Obra.Folha_Margem.Seguro * 100, 2).ToString() + "%");
                t[i] = t[i].Replace("$COMISSAO$", Math.Round(Obra.Folha_Margem.Comissao * 100, 2).ToString() + "%");

                t[i] = t[i].Replace("$DG_COMISSAO$", Math.Round(Obra.Folha_Margem.Despesas_Gerais_Comissao, 2).ToString("C"));
                t[i] = t[i].Replace("$DG_SEGURO$", Math.Round(Obra.Folha_Margem.Despesas_Gerais_Seguro, 2).ToString("C"));
                t[i] = t[i].Replace("$DG_TOTAL$", Math.Round(Obra.Folha_Margem.Despesas_Gerais_Total, 2).ToString("C"));


                t[i] = t[i].Replace("$M_ABSOLUTA$", Math.Round(Obra.Folha_Margem.Margem_Absoluta * 100, 2).ToString() + "%");

                t[i] = t[i].Replace("$M_ABSOLUTA_VL$", Math.Round(Obra.Folha_Margem.Margem_Absoluta_Valor, 2).ToString("C"));

                //07/02/2020
                if (Obra.Folha_Margem.Tipo_Margem == Tipo_Margem.Margem_Absoluta)
                {
                    t[i] = t[i].Replace("$M_CONTRIBUICAO$", Math.Round(Obra.Nacional ? Obra.Folha_Margem.Margem_de_Contribuicao : Obra.Folha_Margem.Margem_de_Contribuicao * Obra.Folha_Margem.Cotacao, 2).ToString("R$0,0.00"));
                }
                else
                {
                    t[i] = t[i].Replace("$M_CONTRIBUICAO$", Math.Round(Obra.Folha_Margem.Margem_de_Contribuicao, 2).ToString("R$0,0.00"));
                }
                t[i] = t[i].Replace("$M_CONTRIBUICAO_VL$", Math.Round(Obra.Folha_Margem.Margem_de_Contribuicao_Valor, 2).ToString("C"));

            }

            Conexoes.Utilz.Arquivo.Gravar(destino, t);

            this.Margem = this.Obra.Folha_Margem;
            navegador.Navigate(String.Format("file:///{0}" + "FOLHA_MARGEM_$ID$.htm".Replace("$ID$", Ob.id.ToString()), p.Replace(@"\\", @"\")));
        }

        private void gerar_proposta(object sender, RoutedEventArgs e)
        {
            this.Obra.GerarProposta();
            GetMargemHTML(this.Obra);

        }

        private void recalcular(object sender, RoutedEventArgs e)
        {
            if (this.Obra.GetRanges().Count == 0)
            {
                Conexoes.Utilz.Alerta("Não há nenhum range adicionado nessa obra.", "Não é possível gerar proposta.", MessageBoxImage.Error);
                return;
            }
            if (Utilz.Pergunta("Deseja alterar o método de cálculo? (Atual: " + this.Obra.Folha_Margem.Tipo_Margem.ToString() + ")"))
            {
                PGO.Funcoes.setdados_Folha(this.Obra);

            }
            this.Obra.Folha_Margem.Calcular(this.Obra);
            GetMargemHTML(this.Obra);
            Conexoes.Utilz.Alerta("Dados recalculados!", "", MessageBoxImage.Asterisk);
        }

        private void ver_propriedades(object sender, RoutedEventArgs e)
        {
            Utilz.Propriedades(this.Margem);
        }
    }
}
