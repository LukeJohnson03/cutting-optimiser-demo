using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingOptimiserDemo
{
    public class CutService
    {
        public List<CutSegment> ExtractCuts(StockSheet sheet,List<Placement> placements)
        {
            var cuts = new List<CutSegment>();

            // Sheet boundary cuts (20mm inset) 
            cuts.Add(new CutSegment
            {
                StartX = 20, StartY = 20,
                EndX = sheet.Width - 20, EndY = 20,
                IsFullLengthX = true,
                StockSheet = sheet,
            });

            cuts.Add(new CutSegment
            {
                StartX = 20, StartY = sheet.Height - 20,
                EndX = sheet.Width - 20, EndY = sheet.Height - 20,
                IsFullLengthX = true,
                StockSheet = sheet,
            });

            // Panel cuts
            foreach(var p in placements)
            {
                double rightX = p.X + p.Panel .Width;
                double bottomY = p.Y + p.Panel .Height;

                // Horizontal Cut (bottom of panel)
                cuts.Add(new CutSegment
                {
                    StartX = p.X, StartY = bottomY,
                    EndX = rightX, EndY = bottomY,
                    IsFullLengthX = (p.X == 20 && rightX == sheet.Width - 20),
                    StockSheet = sheet,
                });

                // Vertical Cut (right of panel)
                cuts.Add(new CutSegment
                {
                    StartX = rightX, StartY = p.Y,
                    EndX = rightX, EndY = bottomY,
                    StockSheet= sheet
                });
              
            }

            return cuts;
        }
    }
}
