using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace PGO
{
    /// <summary>
    /// Interaction logic for Ponderadores_Determinar.xaml
    /// </summary>
    public partial class Ponderadores_Determinar : Window
    {
        public List<DLM.orc.PGO_Etapa_Ponderador> Ponderadores { get; set; } = new List<DLM.orc.PGO_Etapa_Ponderador>();
        public DLM.orc.PGO_Predio Predio { get; set; } = new DLM.orc.PGO_Predio();
        public Ponderadores_Determinar(DLM.orc.PGO_Predio Predio)
        {
            this.Predio = Predio;

            InitializeComponent();
            this.Ponderadores = this.Predio.GetPonderadores();
            this.lista.ItemsSource = this.Predio.GetPonderadores();
            this.saldo.Content = "Disp. no prédio: " + this.Predio.Saldo_Etapa + "%";
        }
        public Ponderadores_Determinar(List<DLM.orc.PGO_Etapa_Ponderador> ponderadors, DLM.orc.PGO_Predio Predio)
        {
            InitializeComponent();
            this.Predio = Predio;
            this.lista.ItemsSource = ponderadors;
            this.Ponderadores = ponderadors;
            this.Title = "Criar Ponderadores - Prédio " + this.Predio.ToString();
            this.saldo.Content = "Disp. no prédio: " + this.Predio.Saldo_Etapa + "%";
        }

        private void adicionar_ponderador(object sender, RoutedEventArgs e)
        {
            var peso = Math.Round(this.Ponderadores.Sum(x => x.ponderador));
            var saldo = Math.Round(Predio.Saldo_Etapa);

            if (peso > saldo)
            {
                Conexoes.Utilz.Alerta($"A soma dos ponderadores dá {peso}% mas  o saldo disponível do prédio é de {saldo}% Revise as considerações de ponderadores antes de continuar.", "", MessageBoxImage.Exclamation);
                return;
            }

            this.DialogResult = true;

        }

        private void cancelar(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
