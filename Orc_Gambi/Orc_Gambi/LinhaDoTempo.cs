using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace PGO
{
    public class LinhaDoTempo
    {
        public IEnumerable<Item> Data { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class Item
    {
        public string Titulo { get; set; } = "Testes";
        public SolidColorBrush cor { get; set; } = new SolidColorBrush(Colors.Green);
        public TimeSpan Duration { get; set; }
        public DateTime Date { get; set; }
        public DateTime DateFim { get; set; }
    }
}
