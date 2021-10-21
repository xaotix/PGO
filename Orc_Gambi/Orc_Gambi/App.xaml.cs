﻿using Conexoes;
using System;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;

namespace Orc_Gambi
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            //var s = Conexoes.Utilz.Agora;

        }
        protected override void OnStartup(StartupEventArgs e)
        {
            //var s = Conexoes.Utilz.Agora;

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            base.OnStartup(e);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Cfg.Init.JanelaWaitMultiThread = true;
            LocalizationManager.Manager = new LocalizationManager()
            {
                ResourceManager = PGO.GridTraducao.ResourceManager
            };
            DBases.Criterio = Criterio.Orcamento;
            if (!Utilz.DecimalOk())
            {
                Conexoes.Utilz.Alerta("Sistema regional de casas decimais configurado incorretamente. Favor ajustar antes de utilizar a ferramenta.", "", MessageBoxImage.Error);
                System.Windows.Application.Current.Shutdown();
            }
            Conexoes.Orcamento.PGOVars.GetDbOrc();
            Conexoes.DBases.GetSegmentos();
            MainWindow mm = new MainWindow();
            mm.Show();

        }
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Conexoes.Utilz.Alerta(e.ExceptionObject.ToString());
            DBases.GetDB().Log(DateTime.Now.ToString() + "=========================>" + e.ExceptionObject.ToString(), Conexoes.Utilz.RaizAppData() + @"\SIE.log");
            return;
        }

        private void seleciona_tudo(object sender, RoutedEventArgs e)
        {
            TextBox textBox = null;
            if (sender is TextBox)
            {
                textBox = ((TextBox)sender);

            }


            if (textBox != null)
            {
                textBox.Dispatcher.BeginInvoke(new Action(() =>
                {
                    textBox.SelectAll();
                }));
            }
        }
    }
}
