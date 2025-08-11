using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingOptimiserDemo
{
    public class StockSheet
    {
        public int Id { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public List<Placement> Placements { get; set; } = new();
        public List<CutSegment> CutSegments { get; set; } = new();

    }
}
