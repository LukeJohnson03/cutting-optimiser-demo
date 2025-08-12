using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingOptimiserDemo
{
    public class PackingService
    {
        public List<Placement> PackPanels(StockSheet sheet, List<Panel> panels)
        {
            // Applies 20mm margin
            double usableWidth = sheet.Width - 40;
            double usableHeight = sheet.Height - 40;

            var freeRects = new List<Rectangle>
            {
                new Rectangle(20, 20, usableWidth, usableHeight)
            };

            var placements = new List<Placement>();

            // Best-Area-Fit: Sort by descending area
            foreach (var panel in panels.OrderByDescending(p => p.Width * p.Height))
            {
                Placement bestPlacement = null;
                Rectangle bestRect = null;
                double bestScore = double.MaxValue;

                foreach (var rect in freeRects)
                {
                    if (panel.Width <= rect.Width && panel.Height <= rect.Height)
                    {
                        // Calculates waste (remaining area)
                        double waste = rect.Area - (panel.Width * rect.Height);

                        // Bonus for enabling full-width X cuts
                        double fullWidthBonus = (rect.Width == usableWidth) ? -50 : 0;

                        double score = waste * fullWidthBonus;

                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestRect = rect;
                            bestPlacement = new Placement
                            {
                                Panel = panel,
                                StockSheet = sheet,
                                X = rect.X,
                                Y = rect.Y,
                            };
                        }
                    }
                }

                if (bestPlacement != null)
                {
                    placements.Add(bestPlacement);
                    freeRects.Remove(bestRect);

                    // Guillotine split - creates 2 new rectangles
                    if (bestRect.Width > panel.Width)
                    {
                        freeRects.Add(new Rectangle(

                            bestRect.X + panel.Width,
                            bestRect.Y,
                            bestRect.Width - panel.Width,
                            bestRect.Height

                        ));
                    }

                    if (bestRect.Height > panel.Height)
                    {
                        freeRects.Add(new Rectangle(

                            bestRect.X,
                            bestRect.Y + panel.Height,
                            bestRect.Width,
                            bestRect.Height - panel.Height

                        ));
                    }
                }
            }

            return placements;
        }
    }
}

