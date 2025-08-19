using System;
using System.Collections.Generic;
using System.Linq;

namespace CuttingOptimiserDemo
{
    public class PackingService
    {
        public List<Placement> PackPanels(StockSheet sheet, List<Panel> panels)
        {
            // Usable area inside 20mm margins
            double usableWidth = sheet.Width - 40;
            double usableHeight = sheet.Height - 40;

            var freeRects = new List<Rectangle>
            {
                new Rectangle(20, 20, usableWidth, usableHeight)
            };

            var placements = new List<Placement>();

            // Panels sorted by descending area
            var sortedPanels = panels.OrderByDescending(p => p.Width * p.Height).ToList();

            foreach (var panel in sortedPanels)
            {
                var bestFit = FindBestAreaFit(panel, freeRects, usableWidth);

                if (bestFit.rect != null)
                {
                    var placement = new Placement
                    {
                        Panel = panel,
                        StockSheet = sheet,
                        X = bestFit.rect.X,
                        Y = bestFit.rect.Y
                    };

                    placements.Add(placement);

                    freeRects.Remove(bestFit.rect);

                    // Split free space using guillotine cut
                    var newRects = PerformGuillotineSplit(bestFit.rect, panel, bestFit.splitVertical);
                    freeRects.AddRange(newRects);

                    // Remove any overlapping free rectangles
                    RemoveOverlappingRectangles(freeRects);
                }
            }

            return placements;
        }

        private (Rectangle rect, bool splitVertical) FindBestAreaFit(Panel panel, List<Rectangle> freeRects, double usableWidth)
        {
            Rectangle bestRect = null;
            bool bestSplitVertical = false;
            double bestArea = double.MaxValue;
            double bestFullWidthScore = 0;

            foreach (var rect in freeRects)
            {
                // Check normal orientation
                if (panel.Width <= rect.Width && panel.Height <= rect.Height)
                {
                    double leftoverArea = rect.Area - (panel.Width * panel.Height);

                    double fullWidthScore = 0;
                    if (rect.X == 20 && panel.Width == usableWidth)
                    {
                        fullWidthScore = 1000; // Favor full-width cuts
                    }

                    double totalScore = leftoverArea - fullWidthScore;

                    if (totalScore < bestArea || (totalScore == bestArea && fullWidthScore > bestFullWidthScore))
                    {
                        bestArea = totalScore;
                        bestFullWidthScore = fullWidthScore;
                        bestRect = rect;

                        double horizontalWaste = (rect.Width - panel.Width) * rect.Height;
                        double verticalWaste = rect.Width * (rect.Height - panel.Height);
                        bestSplitVertical = horizontalWaste > verticalWaste;
                    }
                }

                // Check rotated orientation
                if (panel.Width != panel.Height && panel.Height <= rect.Width && panel.Width <= rect.Height)
                {
                    double leftoverArea = rect.Area - (panel.Width * panel.Height);
                    double totalScore = leftoverArea;

                    if (totalScore < bestArea)
                    {
                        bestArea = totalScore;
                        bestFullWidthScore = 0;
                        bestRect = rect;

                        double horizontalWaste = (rect.Width - panel.Height) * rect.Height;
                        double verticalWaste = rect.Width * (rect.Height - panel.Width);
                        bestSplitVertical = horizontalWaste > verticalWaste;
                    }
                }
            }

            return (bestRect, bestSplitVertical);
        }

        private List<Rectangle> PerformGuillotineSplit(Rectangle rect, Panel panel, bool splitVertical)
        {
            var newRects = new List<Rectangle>();

            if (splitVertical)
            {
                // Right rectangle
                if (rect.Width > panel.Width)
                {
                    newRects.Add(new Rectangle(
                        rect.X + panel.Width,
                        rect.Y,
                        rect.Width - panel.Width,
                        rect.Height
                    ));
                }

                // Bottom rectangle
                if (rect.Height > panel.Height)
                {
                    newRects.Add(new Rectangle(
                        rect.X,
                        rect.Y + panel.Height,
                        panel.Width,
                        rect.Height - panel.Height
                    ));
                }
            }
            else
            {
                // Bottom rectangle
                if (rect.Height > panel.Height)
                {
                    newRects.Add(new Rectangle(
                        rect.X,
                        rect.Y + panel.Height,
                        rect.Width,
                        rect.Height - panel.Height
                    ));
                }

                // Right rectangle
                if (rect.Width > panel.Width)
                {
                    newRects.Add(new Rectangle(
                        rect.X + panel.Width,
                        rect.Y,
                        rect.Width - panel.Width,
                        panel.Height
                    ));
                }
            }

            return newRects;
        }

        private void RemoveOverlappingRectangles(List<Rectangle> freeRects)
        {
            for (int i = freeRects.Count - 1; i >= 0; i--)
            {
                var rect1 = freeRects[i];

                for (int j = i - 1; j >= 0; j--)
                {
                    var rect2 = freeRects[j];

                    if (IsRectangleInsideAnother(rect1, rect2))
                    {
                        freeRects.RemoveAt(i);
                        break;
                    }
                    else if (IsRectangleInsideAnother(rect2, rect1))
                    {
                        freeRects.RemoveAt(j);
                        i--;
                    }
                }
            }
        }

        private bool IsRectangleInsideAnother(Rectangle inner, Rectangle outer)
        {
            return inner.X >= outer.X &&
                   inner.Y >= outer.Y &&
                   inner.X + inner.Width <= outer.X + outer.Width &&
                   inner.Y + inner.Height <= outer.Y + outer.Height;
        }
    }
}
