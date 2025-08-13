using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace CuttingOptimiserDemo
{
    public partial class MainWindow : Window
    {
        private readonly GlassCuttingContext _context;

        public MainWindow()
        {
            try
            {
                Batteries.Init();

                InitializeComponent();

                _context = new GlassCuttingContext();
                InitializeDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize application: {ex.Message}",
                              "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void InitializeDatabase()
        {
            try
            {
                _context.Database.EnsureCreated();
                StatusText.Text = "Database Ready";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Database Error";
                throw new Exception("Failed to initialize database", ex);
            }
        }

        private void Optimize_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Clear previous results
                _context.Placements.RemoveRange(_context.Placements);
                _context.CutSegments.RemoveRange(_context.CutSegments);

                // Get data and run optimization
                var sheets = _context.StockSheets.ToList();
                var panels = _context.Panels.ToList();
                var packingService = new PackingService();
                var cutService = new CutService();


                foreach (var sheet in sheets)
                {
                    var placements = packingService.PackPanels(sheet, panels);
                    var cuts = cutService.ExtractCuts(sheet, placements);

                    _context.Placements.AddRange(placements);
                    _context.CutSegments.AddRange(cuts);

                    // Visualises each sheet
                    renderLayout(sheet, placements, cuts);
                }

                _context.SaveChanges();
                StatusText.Text = $"Optimized {sheets.Count} sheet(s)";
            }
            catch (Exception ex) 
            {
                StatusText.Text = $"Error: { ex.Message}";
            }

        }

        private void renderLayout(StockSheet sheet, List<Placement> placements, List<CutSegment> cuts)
        {
            LayoutCanvas.Children.Clear();
            LayoutCanvas.Width = sheet.Width;
            LayoutCanvas.Height = sheet.Height;

            // Draws sheet background
            var sheetRect = new System.Windows.Shapes.Rectangle
            {
                Width = sheet.Width,
                Height = sheet.Height,
                Fill = Brushes.White,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            Canvas.SetLeft(sheetRect, 0);
            Canvas.SetTop(sheetRect, 0);
            LayoutCanvas.Children.Add(sheetRect);

            // Draws panels
            foreach (var placement in placements)
            {
                var panelRect = new System.Windows.Shapes.Rectangle
                {
                    Width = placement.Panel.Width,
                    Height = placement.Panel.Height,
                    Fill = Brushes.LightBlue,
                    Stroke = Brushes.DarkBlue,
                    StrokeThickness = 1,
                    ToolTip = $"Panel {placement.Panel.Id} ({placement.Panel.Width}x{placement.Panel.Height}mm)"
                };

                Canvas.SetLeft(panelRect, placement.X);
                Canvas.SetTop(panelRect, placement.Y);
                LayoutCanvas.Children.Add(panelRect);

            }

            // Draws cuts 
            foreach (var cut in cuts)
            {
                var line = new Line
                {
                    X1 = cut.StartX,
                    Y1 = cut.StartY,
                    X2 = cut.EndX,
                    Y2 = cut.EndY,

                    Stroke = cut.IsFullLengthX ? Brushes.Red : Brushes.Green,
                    StrokeThickness = 1,
                    ToolTip = cut.IsFullLengthX ? "Full-length X cut" : "Regular cut"
                };
                LayoutCanvas.Children.Add(line);
            }


        }

    }
}