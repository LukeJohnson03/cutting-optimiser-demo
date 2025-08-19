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
                var segments = GetHorizontalCutSegments(y, sortedX, placements);
                foreach (var segment in segments)
                {
                    cuts.Add(new CutSegment
                    {
                        StartX = segment.StartX,
                        StartY = y,
                        EndX = segment.EndX,
                        EndY = y,
                        IsFullLengthX = false,
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
                        IsFullLengthX = false,
                        StockSheet = sheet
                    });
                }
            }

            // Merge adjacent segments and properly set IsFullLengthX
            return MergeAdjacentSegments(cuts, sheet);
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

        // Merges adjacent cut segments into continuous cuts and properly sets IsFullLengthX flag
        private List<CutSegment> MergeAdjacentSegments(List<CutSegment> cuts, StockSheet sheet)
        {
            var mergedCuts = new List<CutSegment>();

            // Group horizontal cuts by Y coordinate
            var horizontalGroups = cuts.Where(c => c.StartY == c.EndY)
                                      .GroupBy(c => c.StartY)
                                      .ToList();

            foreach (var group in horizontalGroups)
            {
                var segments = group.OrderBy(c => c.StartX).ToList();
                var merged = MergeHorizontalSegments(segments, sheet);
                mergedCuts.AddRange(merged);
            }

            // Group vertical cuts by X coordinate  
            var verticalGroups = cuts.Where(c => c.StartX == c.EndX)
                                    .GroupBy(c => c.StartX)
                                    .ToList();

            foreach (var group in verticalGroups)
            {
                var segments = group.OrderBy(c => c.StartY).ToList();
                var merged = MergeVerticalSegments(segments, sheet);
                mergedCuts.AddRange(merged);
            }

            return mergedCuts;
        }

        // Merges adjacent horizontal segments on the same Y coordinate
        private List<CutSegment> MergeHorizontalSegments(List<CutSegment> segments, StockSheet sheet)
        {
            if (!segments.Any()) return new List<CutSegment>();

            var merged = new List<CutSegment>();
            var current = segments[0];

            for (int i = 1; i < segments.Count; i++)
            {
                var next = segments[i];

                // If segments are adjacent, merge them
                if (Math.Abs(current.EndX - next.StartX) < 0.1)
                {
                    current = new CutSegment
                    {
                        StartX = current.StartX,
                        StartY = current.StartY,
                        EndX = next.EndX,
                        EndY = current.EndY,
                        StockSheet = sheet
                    };
                }
                else
                {
                    current.IsFullLengthX = Math.Abs(current.StartX - 20) < 0.1 &&
                                           Math.Abs(current.EndX - (sheet.Width - 20)) < 0.1;
                    merged.Add(current);
                    current = next;
                }
            }

            current.IsFullLengthX = Math.Abs(current.StartX - 20) < 0.1 &&
                                   Math.Abs(current.EndX - (sheet.Width - 20)) < 0.1;
            merged.Add(current);

            return merged;
        }

        // Merges adjacent vertical segments on the same X coordinate
        private List<CutSegment> MergeVerticalSegments(List<CutSegment> segments, StockSheet sheet)
        {
            if (!segments.Any()) return new List<CutSegment>();

            var merged = new List<CutSegment>();
            var current = segments[0];

            for (int i = 1; i < segments.Count; i++)
            {
                var next = segments[i];

                // If segments are adjacent, merge them
                if (Math.Abs(current.EndY - next.StartY) < 0.1)
                {
                    current = new CutSegment
                    {
                        StartX = current.StartX,
                        StartY = current.StartY,
                        EndX = current.EndX,
                        EndY = next.EndY,
                        StockSheet = sheet
                    };
                }
                else
                {
                    current.IsFullLengthX = false;
                    merged.Add(current);
                    current = next;
                }
            }

            current.IsFullLengthX = false;
            merged.Add(current);

            return merged;
        }
    }
}