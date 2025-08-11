using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingOptimiserDemo
{
    public class CutSegment
    {
        public int Id { get; set; }
        public double StartX { get; set; }
        public double StartY { get; set; }
        public double EndX { get; set; }
        public double EndY { get; set; }
        public bool IsFullLengthX { get; set; }
        public int StockSheetId { get; set; }
        public StockSheet StockSheet { get; set; }
    }
}
