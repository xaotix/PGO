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
        public List<DLMorc.Etapa_Ponderador> Ponderadores { get; set; } = new List<DLMorc.Etapa_Ponderador>();
        public DLMorc.OrcamentoPredio Predio { get; set; } = new DLMorc.OrcamentoPredio();
        public Ponderadores_Determinar(DLMorc.OrcamentoPredio Predio)
        {
            this.Predio = Predio;

            InitializeComponent();
            this.Ponderadores = this.Predio.GetPonderadores();
            this.lista.ItemsSource = this.Predio.GetPonderadores();
            this.saldo.Content = "Disp. no prédio: " + this.Predio.Saldo_Etapa + "%";
        }
        public Ponderadores_Determinar(List<DLMorc.Etapa_Ponderador> ponderadors, DLMorc.OrcamentoPredio Predio)
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
