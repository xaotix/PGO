using Conexoes;
using DLM.vars;
using System;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;

namespace PGO
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            base.OnStartup(e);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Cfg.Init.JanelaWaitMultiThread = false;
            Cfg.Init.Nova_Folha_Margem = true;
       
            LocalizationManager.Manager = new LocalizationManager()
            {
                ResourceManager = PGO.GridTraducao.ResourceManager
            };
            DBases.Criterio = Acessos_Criterio.ORC;
            if (!Utilz.DecimalOk())
            {
                Conexoes.Utilz.Alerta("Sistema regional de casas decimais configurado incorretamente. Favor ajustar antes de utilizar a ferramenta.", "", MessageBoxImage.Error);
                System.Windows.Application.Current.Shutdown();
                Environment.Exit(0);
            }
           
            

            MainWindow mm = new MainWindow();
            mm.Show();

        }
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Conexoes.Utilz.Alerta(e);
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
