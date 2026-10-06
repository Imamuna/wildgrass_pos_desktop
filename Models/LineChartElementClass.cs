using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WildGrass_Desktop_f8.Models
{
    internal class LineChartElementClass
    {
        public double value { get; set; }
        public string title { get; set; }
        public bool previousData { get; set; }
        public string change { get; set; }
        public SolidColorBrush Color { get; set; }
        public string valueUnit { get; set; }
        public string dateUnit { get; set; }
        public double date { get; set; }//should we turn this to double?
    }
}
