using System;
using System.Collections.Generic;
using System.Linq;

namespace CuttingOptimiserDemo
{
    public class PackingService
    {
        public List<Placement> PackPanels(StockSheet sheet, List<Panel> panels)
        {
            // Apply 20mm margin - this creates our initial usable area
            double usableWidth = sheet.Width - 40;
            double usableHeight = sheet.Height - 40;

            var freeRects = new List<Rectangle>
            {
                new Rectangle(20, 20, usableWidth, usableHeight)
            };

            var placements = new List<Placement>();

            // Sort panels by descending area (Best-Area-Fit typically uses largest first)
            var sortedPanels = panels.OrderByDescending(p => p.Width * p.Height).ToList();

            foreach (var panel in sortedPanels)
            {
                var bestFit = FindBestAreaFit(panel, freeRects, usableWidth);

                if (bestFit.rect != null)
                {
                    // Create the placement
                    var placement = new Placement
                    {
                        Panel = panel,
                        StockSheet = sheet,
                        X = bestFit.rect.X,
                        Y = bestFit.rect.Y
                    };

                    placements.Add(placement);

                    // Remove the used rectangle
                    freeRects.Remove(bestFit.rect);

                    // Perform guillotine split
                    var newRects = PerformGuillotineSplit(bestFit.rect, panel, bestFit.splitVertical);
                    freeRects.AddRange(newRects);

                    // Clean up overlapping rectangles
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
                // Check if panel fits in this rectangle
                if (panel.Width <= rect.Width && panel.Height <= rect.Height)
                {
                    // Calculate leftover area after placement
                    double leftoverArea = rect.Area - (panel.Width * panel.Height);

                    // Bonus scoring for full-width placements (enables X-breaker usage)
                    double fullWidthScore = 0;
                    if (rect.X == 20 && panel.Width == usableWidth)
                    {
                        fullWidthScore = 1000; // High bonus for full-width cuts
                    }

                    // Best-Area-Fit: prefer smallest area that fits, but prioritize full-width
                    double totalScore = leftoverArea - fullWidthScore;

                    if (totalScore < bestArea || (totalScore == bestArea && fullWidthScore > bestFullWidthScore))
                    {
                        bestArea = totalScore;
                        bestFullWidthScore = fullWidthScore;
                        bestRect = rect;

                        // Determine optimal split direction for guillotine
                        // Generally prefer the split that creates more useful rectangles
                        double horizontalWaste = (rect.Width - panel.Width) * rect.Height;
                        double verticalWaste = rect.Width * (rect.Height - panel.Height);

                        // Split in the direction that minimizes the larger waste rectangle
                        bestSplitVertical = horizontalWaste > verticalWaste;
                    }
                }

                // Also try rotated orientation if dimensions are different
                if (panel.Width != panel.Height && panel.Height <= rect.Width && panel.Width <= rect.Height)
                {
                    double leftoverArea = rect.Area - (panel.Width * panel.Height);

                    // Note: Rotated panels typically can't achieve full-width cuts
                    double totalScore = leftoverArea;

                    if (totalScore < bestArea)
                    {
                        bestArea = totalScore;
                        bestFullWidthScore = 0;
                        bestRect = rect;

                        // For rotated panels, determine split direction
                        double horizontalWaste = (rect.Width - panel.Height) * rect.Height;
                        double verticalWaste = rect.Width * (rect.Height - panel.Width);
                        bestSplitVertical = horizontalWaste > verticalWaste;

                        // Note: In a full implementation, you'd need to handle panel rotation
                        // For now, we'll assume panels maintain their original orientation
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
                // Vertical split: create right and bottom rectangles

                // Right rectangle (if there's space)
                if (rect.Width > panel.Width)
                {
                    newRects.Add(new Rectangle(
                        rect.X + panel.Width,
                        rect.Y,
                        rect.Width - panel.Width,
                        rect.Height
                    ));
                }

                // Bottom rectangle (if there's space) - only under the placed panel
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
                // Horizontal split: create bottom and right rectangles

                // Bottom rectangle (if there's space)
                if (rect.Height > panel.Height)
                {
                    newRects.Add(new Rectangle(
                        rect.X,
                        rect.Y + panel.Height,
                        rect.Width,
                        rect.Height - panel.Height
                    ));
                }

                // Right rectangle (if there's space) - only to the right of the placed panel
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

                    // If rect1 is completely inside rect2, remove rect1
                    if (IsRectangleInsideAnother(rect1, rect2))
                    {
                        freeRects.RemoveAt(i);
                        break;
                    }
                    // If rect2 is completely inside rect1, remove rect2
                    else if (IsRectangleInsideAnother(rect2, rect1))
                    {
                        freeRects.RemoveAt(j);
                        i--; // Adjust index since we removed an item before current
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