using System;
using System.Collections.Generic;
using System.Linq;

namespace CuttingOptimiserDemo
{
    public class CutService
    {
        public List<CutSegment> ExtractCuts(StockSheet sheet, List<Placement> placements)
        {
            var cuts = new List<CutSegment>();

            // Return empty if no placements
            if (!placements.Any()) return cuts;

            // Track unique X and Y coordinates for cuts
            var xCoordinates = new HashSet<double>();
            var yCoordinates = new HashSet<double>();

            // Add sheet boundaries (20mm margin)
            xCoordinates.Add(20);
            xCoordinates.Add(sheet.Width - 20);
            yCoordinates.Add(20);
            yCoordinates.Add(sheet.Height - 20);

            // Add panel boundaries
            foreach (var placement in placements)
            {
                xCoordinates.Add(placement.X);
                xCoordinates.Add(placement.X + placement.Panel.Width);
                yCoordinates.Add(placement.Y);
                yCoordinates.Add(placement.Y + placement.Panel.Height);
            }

            var sortedX = xCoordinates.OrderBy(x => x).ToList();
            var sortedY = yCoordinates.OrderBy(y => y).ToList();

            // Horizontal cuts
            foreach (var y in sortedY)
            {
                // Skip top/bottom sheet edges
                if (y == 20 || y == sheet.Height - 20) continue;

                var segments = GetHorizontalCutSegments(y, sortedX, placements);
                foreach (var segment in segments)
                {
                    cuts.Add(new CutSegment
                    {
                        StartX = segment.StartX,
                        StartY = y,
                        EndX = segment.EndX,
                        EndY = y,
                        IsFullLengthX = segment.StartX == 20 && segment.EndX == sheet.Width - 20,
                        StockSheet = sheet
                    });
                }
            }

            // Vertical cuts
            foreach (var x in sortedX)
            {
                // Skip left/right sheet edges
                if (x == 20 || x == sheet.Width - 20) continue;

                var segments = GetVerticalCutSegments(x, sortedY, placements);
                foreach (var segment in segments)
                {
                    cuts.Add(new CutSegment
                    {
                        StartX = x,
                        StartY = segment.StartY,
                        EndX = x,
                        EndY = segment.EndY,
                        IsFullLengthX = false, // Vertical cuts never full X
                        StockSheet = sheet
                    });
                }
            }

            return cuts;
        }

        private List<(double StartX, double EndX)> GetHorizontalCutSegments(double y, List<double> sortedX, List<Placement> placements)
        {
            var segments = new List<(double StartX, double EndX)>();

            for (int i = 0; i < sortedX.Count - 1; i++)
            {
                var startX = sortedX[i];
                var endX = sortedX[i + 1];
                var midX = (startX + endX) / 2;

                // Skip if segment intersects a panel
                bool intersectsPanel = placements.Any(p =>
                    midX >= p.X && midX <= p.X + p.Panel.Width &&
                    y > p.Y && y < p.Y + p.Panel.Height);

                if (!intersectsPanel)
                {
                    segments.Add((startX, endX));
                }
            }

            return segments;
        }

        private List<(double StartY, double EndY)> GetVerticalCutSegments(double x, List<double> sortedY, List<Placement> placements)
        {
            var segments = new List<(double StartY, double EndY)>();

            for (int i = 0; i < sortedY.Count - 1; i++)
            {
                var startY = sortedY[i];
                var endY = sortedY[i + 1];
                var midY = (startY + endY) / 2;

                // Skip if segment intersects a panel
                bool intersectsPanel = placements.Any(p =>
                    midY >= p.Y && midY <= p.Y + p.Panel.Height &&
                    x > p.X && x < p.X + p.Panel.Width);

                if (!intersectsPanel)
                {
                    segments.Add((startY, endY));
                }
            }

            return segments;
        }
    }
}
