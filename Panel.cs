using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingOptimiserDemo
{
    public class Panel
    {
        public int Id {  get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public int Quanity { get; set; }
        public List<Placement> Placements { get; set; } = new();
    }
}
