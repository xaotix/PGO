using BingMapsRESTToolkit;
using Conexoes;
using DLM.orc;
using DLM.vars;
using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using DLM.encoder;

namespace PGO
{
    public class Funcoes_Mapa
    {
        public static DLM.orc.Localizacoes Localizacoes { get; set; }
        public static List<Rotas> AgruparEmRotas(List<PGO_Obra> Obras, Map myMap)
        {

            Funcoes_Mapa.Localizacoes.Ler();
            myMap.Children.Clear();


            var rotas = Obras.Select(x => x.GetRotas()).GroupBy(x => x.Latitude + ";" + x.Longitude)
    .Select(g => g.First())
    .ToList();

            foreach (var t in rotas)
            {
                myMap.Children.Add(t.Pin);
            }


            return rotas;

        }
        public static async Task GetLocalizacaoAsync(string pesquisa, Pushpin pin, Map myMap)
        {

            var request = new GeocodeRequest()
            {
                //Query = "New York, NY",
                Query = pesquisa.Trim(),
                Culture = "pt-BR",
                UserRegion = "BR",
                IncludeIso2 = true,
                IncludeNeighborhood = true,
                MaxResults = 25,
                BingMapsKey = "BCDjp0N1tTZL3qwMSWhT~0et6Ey48jFxvQ-vMPFUEIA~AkidRT2WorGGqZmeiZmXsYvpAm9Ni3Jrjugs6cU7ePsX5RLYw2bWFzsjJgjjoNYw"
            };

            var response = await ServiceManager.GetResponseAsync(request);

            if (response != null &&
                response.ResourceSets != null &&
                response.ResourceSets.Length > 0 &&
                response.ResourceSets[0].Resources != null &&
                response.ResourceSets[0].Resources.Length > 0)
            {
                var result = response.ResourceSets[0].Resources[0] as BingMapsRESTToolkit.Location;

                if (result.GeocodePoints.Count() > 0)
                {
                    setPonto(result.GeocodePoints[0].Coordinates[0], result.GeocodePoints[0].Coordinates[1], pin);
                    pin.Tag = pesquisa;

                }


                //Do something with the result.
            }
        }
        private static void setPonto(double latitude, double longitude, Pushpin pin)
        {

            pin.Location = new Microsoft.Maps.MapControl.WPF.Location(latitude, longitude);
        }
        private static System.Globalization.CultureInfo US = new System.Globalization.CultureInfo("en-US");
        private static System.Globalization.CultureInfo BR = new System.Globalization.CultureInfo("pt-BR");
    }
    public class Funcoes
    {
        public static void ExportarPlanilhaCJ20N()
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

                var pecas = PGO.Funcoes.getPecas(excels, out erros);

                var pacote_atual = new Conexoes.Pacote_PMP(pecas, erros);

                Conexoes.Utilz.ShowReports(pacote_atual.erros);

                var pedidos = pacote_atual.pedidos;

                if (pedidos.Count > 1)
                {
                    pedidos = new List<Conexoes.Pedido_PMP> { Conexoes.Utilz.Selecao.SelecionarObjeto(pedidos, null, "Selecione o pedido") };
                    pedidos = pedidos.FindAll(x => x != null);
                }

                if (pedidos.Count == 1)
                {
                    Conexoes.Utilz.ExportarPlanilhaCJ20N(pedidos[0]);
                }
            }
        }
        public static bool Editar(DLM.orc.PEP_Agrupador agrupador, List<PEP_Agrupador> brothers = null)
        {
            var porcentagens = new DLM.orc.Porcentagem_Grupo(agrupador, brothers == null);
            PGO.Porcentagem_Editar po = new PGO.Porcentagem_Editar(porcentagens);
            po.ShowDialog();
            if ((bool)po.DialogResult)
            {
                if (Conexoes.Utilz.Pergunta("Salvar edições?"))
                {
                    return Conexoes.Utilz.SetPorcentagens(agrupador, brothers, porcentagens);
                }
            }
            return false;
        }
        public static void importarPecas(List<Conexoes.Pedido_PMP> pedidos_importar, bool tudo)
        {
            Conexoes.ControleWait w = Conexoes.Utilz.Wait(pedidos_importar.Count, "Importando...");

            foreach (var pedido in pedidos_importar)
            {
                pedido.Limpar(tudo);
                pedido.Gravar();
                w.somaProgresso();
            }
            w.Close();
        }
        public static void VerMateriais(DLM.orc.PGO_Obra Obra)
        {
            var w = Conexoes.Utilz.Wait();
            ExplorerPLM.Menus.Lista_Pecas_ORC menu = new ExplorerPLM.Menus.Lista_Pecas_ORC(
                Obra.GetPecasEtapas(), Obra
                );
            menu.Title = Obra.ToString() + " - Etapas - Orçamento";
            menu.Show();
            w.Close();
        }
        public static void VerMateriais(DLM.orc.PGO_Obra Obra, List<DLM.orc.PGO_Peca> pecas)
        {
            if (pecas.Count == 0)
            {
                Conexoes.Utilz.Alerta("Nenhuma Peça");
                return;
            }
            ExplorerPLM.Menus.Lista_Pecas_ORC menu = new ExplorerPLM.Menus.Lista_Pecas_ORC(
                pecas, Obra
                );
            menu.Title = Obra.ToString() + " - Etapas - Orçamento";
            menu.Show();
        }
        public static void VerMateriais(DLM.orc.PGO_Obra Obra, List<object> pecas)
        {
            if (pecas.Count == 0)
            {
                Conexoes.Utilz.Alerta("Nenhuma Peça");
                return;
            }
            ExplorerPLM.Menus.Lista_Pecas_ORC menu = new ExplorerPLM.Menus.Lista_Pecas_ORC(
                pecas.FindAll(x => x is DLM.orc.PGO_Peca).Cast<DLM.orc.PGO_Peca>().ToList(), Obra
                );
            menu.Title = Obra.ToString() + " - Etapas - Orçamento";
            menu.Show();
        }
        public static void VerMateriais(List<DLM.orc.PGO_Peca> pecas)
        {

            if (pecas.Count == 0)
            {
                Conexoes.Utilz.Alerta("Nenhuma Peça");
                return;
            }
            ExplorerPLM.Menus.Lista_Pecas_ORC menu = new ExplorerPLM.Menus.Lista_Pecas_ORC(pecas);
            menu.Title = "Etapas - Orçamento";
            menu.Show();
        }
        public static void setdados_Folha(PGO_Obra Obra)
        {
            var sel = Enum.GetNames(typeof(Tipo_Margem));
            var sel_user = Conexoes.Utilz.Selecao.SelecionarObjeto(sel.ToList(), null, "Selecione o tipo de cálculo");
            if (sel_user != null)
            {
                Tipo_Margem tipo;
                Enum.TryParse(sel_user.ToString(), out tipo);
                Obra.Custos.Tipo_Margem = tipo;
                if (tipo == Tipo_Margem.Margem_Absoluta)
                {
                    Obra.Custos.Margem_Absoluta = Conexoes.Utilz.Double(Conexoes.Utilz.Prompt("Dige o valor da margem absoluta (%)", "", Obra.Custos.Margem_Absoluta.ToString()));
                }
                else if (tipo == Tipo_Margem.Margem_Contribuicao)
                {
                    Obra.Margem = Conexoes.Utilz.Double(Conexoes.Utilz.Prompt("Dige o valor da margem de contribuição (R$/Kg)", "", Obra.Margem.ToString()));

                }
                else if (tipo == Tipo_Margem.Valor_de_Venda)
                {
                    Obra.Custos.Valor_de_Venda = Conexoes.Utilz.Double(Conexoes.Utilz.Prompt("Dige o valor de venda (R$)", "", Obra.Custos.Valor_de_Venda.ToString()));

                }
                Obra.SalvarTudo();
            }

        }
        public static void VerificarVersao()
        {
            int minutos = 1;


            //
            int wait = minutos * 1000 * 60;

            while (true == true)
            {
                Conexoes.DBases.ResetUser();
                Conexoes.DBases.GetUserAtual().Salva_Status(true);
                string versao_atual = System.Windows.Forms.Application.ProductVersion;


                string versao_sistema = Conexoes.DBases.GetBancoRM().VERSAO_ORCAMENTO;

                if (versao_atual != versao_sistema && versao_sistema!="")
                {
                    Conexoes.Utilz.Alerta("Há uma nova versão disponível do programa. Feche o programa e abra-o novamente para atualizar. Caso essa mensagem continue aparecendo, verifique se você está executando o aplicativo através do Updater. \n\nPara maiores informações, entre em contato com o suporte (Daniel Lins Maciel).\nSua versão: " + versao_atual + "\n\nNova Versão:" + versao_sistema, "Nova Versão " + versao_sistema, MessageBoxImage.Asterisk);
                }


                System.Threading.Thread.Sleep(wait);
            }
        }
        public static List<Conexoes.Pedido_PMP> getPedidos(List<string> codigos_obras, out List<Report> erros, bool prompt = true)
        {
            erros = new List<Report>();

            List<Conexoes.Pedido_PMP> retorno = new List<Conexoes.Pedido_PMP>();
            List<Conexoes.Arquivo> excels = new List<Conexoes.Arquivo>();
            List<string> locais = new List<string>();
            List<string> peps = new List<string>();
            int tot = 5;


            Conexoes.ControleWait w = Conexoes.Utilz.Wait(tot, "Pesquisando " + DLM.vars.PGOVars.GetConfig().pasta_consolidadas);
            w.somaProgresso();
            var pastas = Conexoes.Utilz.GetPastas(DLM.vars.PGOVars.GetConfig().pasta_consolidadas);
            w.somaProgresso("Buscando pedidos...");

            foreach (var s in codigos_obras)
            {
                var contrato = Conexoes.Utilz.PEP.Get.Contrato(s);
                var pedido = Conexoes.Utilz.PEP.Get.Pedido(s);
                List<string> atual = new List<string>();
                List<string> subs = new List<string>();


                atual.AddRange(pastas.FindAll(x => x.ToUpper().Replace(@"\", "").Replace(" ", "").Replace(".", "").Contains(contrato)));

                foreach (var at in atual)
                {
                    subs.AddRange(Conexoes.Utilz.GetPastas(at, "*" + pedido + "*", SearchOption.AllDirectories));
                }
                if (subs.Count > 0)
                {
                    atual = subs;
                }
                //atual.AddRange(subs);

                if (atual.Count == 0 && contrato != "")
                {
                    atual = pastas.FindAll(x => x.ToUpper().Replace(" ", "").Replace(".", "").Contains(contrato));
                    if (pedido != "")
                    {
                        atual = atual.FindAll(x => x.Contains(pedido) | x.Contains(pedido.Replace("P", "C")));
                    }
                }
                locais.AddRange(atual);
            }
            w.SetProgresso(1, locais.Count);
            foreach (var loc in locais)
            {

                excels.AddRange(Conexoes.Utilz.GetArquivos(loc, "*.xlsx", SearchOption.AllDirectories).Select(x => new Conexoes.Arquivo(x)));
                w.somaProgresso(loc);
            }
            excels = excels.GroupBy(x => x.Endereco.ToUpper()).Select(x => x.First()).ToList();

            w.Close();
            if (prompt)
            {
                if (excels.Count > 0)
                {
                    if (Conexoes.Utilz.Pergunta("Deseja carregar os " + excels.Count + " encontrados?"))
                    {
                        var pcs = getPecas(excels, out erros);
                        var pacote_atual = new Conexoes.Pacote_PMP(pcs, erros);

                        return pacote_atual.pedidos;
                    }
                    else
                    {
                        erros.Add(new Report("Cancelado", "Cancelado pelo usuário", TipoReport.Alerta));
                    }
                }
            }
            else
            {
                if (excels.Count > 0)
                {
                    var pcs = getPecas(excels, out erros);
                    var pacote_atual = new Conexoes.Pacote_PMP(pcs, erros);

                    return pacote_atual.pedidos.FindAll(x => codigos_obras.Find(y => x.pep.ToUpper().Replace(" ", "").Replace(".", "").Replace("-", "").Contains(y.ToUpper().Replace(" ", "").Replace(".", "").Replace("-", ""))) != null);
                }
                else
                {
                    erros.Add(new Report("Vazio", "Nenhum pedido encontrado.", TipoReport.Alerta));
                }
            }
            return new List<Conexoes.Pedido_PMP>();
        }
        public static List<Conexoes.Pedido_PMP> getPedidos(List<DLM.orc.PGO_Obra> codigos, out List<Report> erros)
        {
            erros = new List<Report>();
            List<string> codigos_obras = codigos.Select(x => x.PedidoSAP).Distinct().ToList();
            if (!Directory.Exists(PGOVars.GetConfig().pasta_consolidadas))
            {
                erros.Add(new Report("Pasta não existe", DLM.vars.PGOVars.GetConfig().pasta_consolidadas, TipoReport.Crítico));

                return new List<Conexoes.Pedido_PMP>();
            }

            var pedidos = getPedidos(codigos_obras, out erros);


            foreach (var ped in pedidos)
            {
                var ob = codigos.Find(x => x.Contrato_SAP == ped.contrato_sap);
                if (ob != null)
                {
                    ped.nome = ob.Nome;
                }

            }

            return pedidos;
        }
        public static List<Conexoes.Peca_PMP> getPecas(List<string> excels, List<string> peps = null)
        {
            List<Report> erros = new List<Report>();
            excels = excels.FindAll(x => File.Exists(x));
            if (peps == null)
            {
                peps = new List<string>();
            }
            return getPecas(excels.Select(x => new Conexoes.Arquivo(x)).ToList(), out erros, peps);
        }
        public static List<Conexoes.Peca_PMP> getPecas(List<Conexoes.Arquivo> arquivos, out List<Report> erros, List<string> peps = null)
        {
            double max_tamanho = 5000;
            erros = new List<Report>();
            if (peps == null)
            {
                peps = new List<string>();
            }
            List<Conexoes.Arquivo> arqs = new List<Conexoes.Arquivo>();
            arqs = arquivos
                        .FindAll(x => x.TamKB < max_tamanho)
                        .FindAll(x => !x.Nome.Contains("~") && x.Nome.Contains("SAP") && x.Nome.Contains("RME")).ToList();


            List<Conexoes.Peca_PMP> pecas = new List<Conexoes.Peca_PMP>();
            Conexoes.ControleWait w = Conexoes.Utilz.Wait(arqs.Count, "Buscando arquivos excel...");
            w.somaProgresso();
            w.SetProgresso(1, arqs.Count);

            arqs = arqs.OrderByDescending(x => x.TamKB).ToList();
            var arquivos_fora = arquivos.FindAll(x => arqs.Find(y => y.Endereco == x.Endereco) == null);
            erros.AddRange(arquivos_fora.Select(z => new Report(z.Endereco, "Arquivo inválido, não contém no nome SAP - RME ou é maior que o tamanho máximo (" + max_tamanho + ") " + "Tam. arq.:(" + z.Tamanho + ")", TipoReport.Crítico)));


            var saps_rmes = arqs.Select(x => new DLM.ep.SAPRME(x.Endereco, false)).ToList();
            var sub_lista = saps_rmes.Quebrar(5);
            w.SetProgresso(1, saps_rmes.Count);
            var max = 20;
            int maximo = 15;
            if (max == 0)
            {
                max = 1;
            }
            else if (max > maximo)
            {
                max = maximo;
            }
            else if (max < maximo)
            {
                if (saps_rmes.Count < maximo)
                {

                    max = saps_rmes.Count;
                }
                else
                {
                    max = maximo;
                }
            }
            int atual = 0;
            int pack_ct = 0;
            var list_pack = saps_rmes.Quebrar(max);
            w.SetProgresso(1, saps_rmes.Count);
            foreach (var pack in list_pack)
            {
                pack_ct++;
                List<Task> Tarefas = new List<Task>();
                List<Thread> trs = new List<Thread>();
                foreach (var s in pack)
                {
                    atual++;
                    w.somaProgresso(pack_ct + "/" + list_pack.Count + " Pacotes, " + atual + "/" + saps_rmes.Count + " arqs");

                    try
                    {
                        Thread t = new Thread(() => s.CarregarExcel());
                        t.Start();
                        trs.Add(t);
                        if (!t.Join(TimeSpan.FromSeconds(30)))
                        {
                            t.Abort();
                            erros.Add(new Report("Abortado - Arquivo demorou demais.", s.Arquivo, TipoReport.Crítico));
                        }
                    }
                    catch (Exception ex)
                    {

                        erros.Add(new Report("Erro ao tentar ler o arquivo " + ex.Message, s.Arquivo, TipoReport.Crítico));
                    }

                }

                for (int i = 0; i < trs.Count; i++)
                {
                    trs[i].Join();
                }

                trs.Clear();

                w.somaProgresso(pack_ct + "/" + list_pack.Count + " Pacotes, " + atual + "/" + saps_rmes.Count + " arqs");
            }

            w.SetProgresso(1, arqs.Count);

            foreach (var t in saps_rmes)
            {
                try
                {
                    List<Report> erros_pcs = new List<Report>();
                    var pcs = PGO.Funcoes.getPecas(t, out erros_pcs).ToList().FindAll(x => x.pep.Length > 13 && x.pep.Contains(".P"));
                    pecas.AddRange(pcs);
                    peps.AddRange(pcs.Select(x => x.pep).Distinct().ToList());
                    w.somaProgresso(t.Arquivo.ToUpper().Replace(DLM.vars.PGOVars.GetConfig().pasta_consolidadas.ToUpper(), ""));
                    erros.AddRange(erros_pcs);

                    if (pcs.Count > 0)
                    {
                        erros.AddRange(t.Reports);
                        erros.AddRange(t.Input.Reports);
                    }
                    else
                    {
                        // erros.Add(new Report("Arquivo sem peças com PEPs de consolidação", t.Arquivo));
                    }
                }
                catch (Exception ex)
                {
                    erros.Add(new Report(ex));

                }

            }
            w.Close();
            return pecas;
        }
        public static List<Conexoes.Peca_PMP> getPecas(string arquivo_excel, out List<Report> erros)
        {
            erros = new List<Report>();
            if (!File.Exists(arquivo_excel))
            {
                erros.Add(new Report(arquivo_excel, "Arquivo não existe", TipoReport.Crítico));
                return new List<Conexoes.Peca_PMP>();
            }
            DLM.ep.SAPRME rme = new DLM.ep.SAPRME(arquivo_excel);

            List<Conexoes.Peca_PMP> retorno = getPecas(rme, out erros);

            return retorno;
        }
        public static List<Conexoes.Peca_PMP> getPecas(DLM.ep.SAPRME rme, out List<Report> erros)
        {
            erros = new List<Report>();
            List<Conexoes.Peca_PMP> retorno = new List<Conexoes.Peca_PMP>();
            foreach (var marca in rme.Marcas)
            {
                try
                {
                    Conexoes.Peca_PMP nova = new Conexoes.Peca_PMP();
                    nova.arquivo = rme.Arquivo.ToUpper();
                    nova.comp = marca.ZPP_COMPR;
                    nova.corte = marca.ZPP_CORTE;
                    nova.descricao = marca.MAKTX;
                    var espess = marca.Posicoes.Select(x => x.ZPP_ESPES).Distinct().ToList().FindAll(x => x > 0);
                    if (espess.Count > 0)
                    {
                        nova.esp = espess[0];

                    }
                    nova.esquema = marca.ZPP_ESQPIN;
                    nova.furos = marca.Posicoes.Sum(x => x.ZPP_QUANT);
                    nova.grupo_mercadoria = marca.MAKTX.ToString().Replace("_", " ");
                    nova.marca = marca.ZPP_CODMAR;
                    var matprimas = marca.Posicoes.Select(x => x.ZPP_CODMATP).ToList().FindAll(x => x != "").Distinct().ToList();
                    if (matprimas.Count > 0)
                    {
                        nova.materia_prima = matprimas[0];
                    }
                    nova.pep = marca.PS_POSID;
                    nova.pep_inicial = marca.PS_POSID;
                    var TESTE = Conexoes.Utilz.PEP.SetContrato("123456", marca.PS_POSID);
                    if (Conexoes.Utilz.NORMT.GetTipoBase(marca.NORMT) == CAM_TIPO_BASE.Almox)
                    {
                        nova.peso = marca.Posicoes.Sum(x => x.ZPP_PESOPOS);
                        nova.quantidade = marca.Posicoes.Sum(x => x.ZPP_QTDPOS);
                    }
                    else
                    {
                        nova.peso = marca.Posicoes.FindAll(x => x.NORMT != TAB_NORMT.PERFIL_I_SOLDADO).Sum(x => x.ZPP_PESOPOS * x.ZPP_QTDPOS * x.Qtd_Pai);
                        nova.quantidade = marca.ZPP_QTDMAR;
                        nova.complexidade = marca.Posicoes.Sum(x => x.ZPP_QTDPOS).ToString() + " Posicções";

                    }
                    nova.range = "";
                    nova.superficie = marca.ZPP_SUPER;
                    nova.tipo = Tipo_PMP.C;
                    var mats = marca.Posicoes.Select(x => x.ZPP_TIPOACO).Distinct().ToList().FindAll(x => x != "").ToList();
                    if (mats.Count > 0)
                    {
                        nova.tipo_aco = mats[0];

                    }
                    nova.tratamento = marca.ZPP_TIPOPIN;
                    nova.unidade_fabril = marca.WERKS;
                    //nova.dbase = Conexoes.DBases.GetDB();
                    retorno.Add(nova);
                }
                catch (Exception ex)
                {
                    erros.Add(new Report(ex));
                }
            }

            var rms = retorno.FindAll(x => x.marca.Contains("KIT"));
            var cods = rms.Select(x => x.materia_prima).Distinct().ToList();
            foreach (var cod in cods)
            {
                var rm = Conexoes.DBases.GetBancoRM().GetRMAt().Find(x => x.SAP == cod);
                if (rm != null)
                {
                    foreach (var pc in rms.FindAll(x => x.materia_prima == cod))
                    {
                        pc.descricao = rm.DESC;
                        pc.marca = cod;
                    }
                }
            }
            return retorno;
        }
        public static List<Range> SelecionarRanges(List<PGO_Predio> Selecao, bool editavel = true)
        {
            List<Range> retorno = new List<Range>();

            JanelaSelecionarRanges tt = new JanelaSelecionarRanges(Selecao, editavel);

            tt.ShowDialog();

            if (tt.DialogResult.HasValue && tt.DialogResult.Value)
            {
                //retorno = tt.Predios.SelectMany(x => x.Ranges).ToList().FindAll(x => x.Selecionado);
                retorno = tt.Predios.SelectMany(x => x.Ranges).ToList();
            }

            return retorno;
        }
    }
}
