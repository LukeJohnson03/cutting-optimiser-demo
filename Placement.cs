using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingOptimiserDemo
{
    public class Placement
    {
        public int Id { get; set; }
        public double X { get; set; } // from left
        public double Y { get; set; } // from top
        public int PanelId { get; set; }
        public Panel Panel { get; set; }
        public int StockSheetId { get; set; }
        public StockSheet StockSheet { get; set; }
    }
}

