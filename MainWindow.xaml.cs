using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;

namespace CuttingOptimiserDemo
{
    public partial class MainWindow : Window
    {
        private readonly GlassCuttingContext _context = new();
        private StockSheet _currentSheet;

        public MainWindow()
        {
            InitializeComponent();
            InitializeDatabase();
            LoadSheets();

            // Attach resize handler
            this.SizeChanged += MainWindow_SizeChanged;
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            EnsureVisible();
        }

        private void InitializeDatabase()
        {
            try
            {
                _context.Database.EnsureCreated();
                StatusText.Text = "Database ready";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}");
            }
        }

        private void LoadSheets()
        {
            var sheets = _context.StockSheets.ToList();

            var displaySheets = sheets.Select(s => new
            {
                Sheet = s,
                DisplayText = $"Sheet {s.Id}: {s.Width:F0}×{s.Height:F0}mm ({s.Width / 1000:F1}×{s.Height / 1000:F1}m - {(s.Width * s.Height) / 1000000:F2}m²)"
            }).ToList();

            SheetSelector.ItemsSource = displaySheets;
            SheetSelector.DisplayMemberPath = "DisplayText";
            SheetSelector.SelectedValuePath = "Sheet";
            SheetSelector.SelectedIndex = 0;
        }

        private async void Optimize_Click(object sender, RoutedEventArgs e)
        {
            OptimizeButton.IsEnabled = false;

            try
            {
                var selectedItem = SheetSelector.SelectedItem;
                var selectedSheet = selectedItem?.GetType().GetProperty("Sheet")?.GetValue(selectedItem) as StockSheet;

                if (selectedSheet == null) return;

                StatusText.Text = $"Optimizing sheet {selectedSheet.Id}...";
                await Task.Run(() => RunOptimization(selectedSheet));
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error: {ex.Message}";
                MessageBox.Show(ex.Message, "Optimization Error");
            }
            finally
            {
                OptimizeButton.IsEnabled = true;
            }
        }

        private void RunOptimization(StockSheet sheet)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _currentSheet = sheet;

                // Clear previous results
                _context.Placements.RemoveRange(_context.Placements.Where(p => p.StockSheetId == sheet.Id));
                _context.CutSegments.RemoveRange(_context.CutSegments.Where(c => c.StockSheetId == sheet.Id));
                _context.SaveChanges();

                StatusText.Text = $"Processing sheet {sheet.Id}...";
            });

            var panels = _context.Panels.ToList();
            var packingService = new PackingService();
            var cutService = new CutService();

            var placements = packingService.PackPanels(sheet, panels);
            var cuts = cutService.ExtractCuts(sheet, placements);

            Application.Current.Dispatcher.Invoke(() =>
            {
                _context.Placements.AddRange(placements);
                _context.CutSegments.AddRange(cuts);
                _context.SaveChanges();

                RenderLayout(sheet, placements, cuts);
                EnsureVisible();

                StatusText.Text = $"Sheet {sheet.Id} optimized - {placements.Count} panels placed";
            });
        }

        private void RenderLayout(StockSheet sheet, List<Placement> placements, List<CutSegment> cuts)
        {
            LayoutCanvas.Children.Clear();
            LayoutCanvas.Width = sheet.Width;
            LayoutCanvas.Height = sheet.Height;

            // Sheet background
            var sheetRect = new System.Windows.Shapes.Rectangle
            {
                Width = sheet.Width,
                Height = sheet.Height,
                Fill = Brushes.WhiteSmoke,
                Stroke = Brushes.Gray,
                StrokeThickness = 1
            };
            Canvas.SetLeft(sheetRect, 0);
            Canvas.SetTop(sheetRect, 0);
            LayoutCanvas.Children.Add(sheetRect);

            // Usable area (20mm margin)
            var usableArea = new System.Windows.Shapes.Rectangle
            {
                Width = sheet.Width - 40,
                Height = sheet.Height - 40,
                Fill = Brushes.White,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            Canvas.SetLeft(usableArea, 20);
            Canvas.SetTop(usableArea, 20);
            LayoutCanvas.Children.Add(usableArea);

            // Panels
            foreach (var placement in placements)
            {
                var panelRect = new System.Windows.Shapes.Rectangle
                {
                    Width = placement.Panel.Width,
                    Height = placement.Panel.Height,
                    Fill = new SolidColorBrush(Color.FromArgb(150, 173, 216, 230)),
                    Stroke = Brushes.DarkBlue,
                    StrokeThickness = 0.5
                };
                Canvas.SetLeft(panelRect, placement.X);
                Canvas.SetTop(panelRect, placement.Y);
                LayoutCanvas.Children.Add(panelRect);

                // Panel label
                var label = new TextBlock
                {
                    Text = $"Panel ID: {placement.Panel.Id}, Dimensions: {placement.Panel.Width}x{placement.Panel.Height}",
                    FontSize = 12,
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.Bold
                };
                Canvas.SetLeft(label, placement.X + 5);
                Canvas.SetTop(label, placement.Y + 5);
                LayoutCanvas.Children.Add(label);
            }

            // Cuts
            foreach (var cut in cuts)
            {
                var line = new Line
                {
                    X1 = cut.StartX,
                    Y1 = cut.StartY,
                    X2 = cut.EndX,
                    Y2 = cut.EndY,
                    Stroke = cut.IsFullLengthX ? Brushes.Red : Brushes.Green,
                    StrokeThickness = 2,
                    StrokeDashArray = cut.IsFullLengthX ? null : new DoubleCollection { 4, 2 }
                };
                LayoutCanvas.Children.Add(line);
            }
        }

        private void EnsureVisible()
        {
            if (LayoutCanvas.Children.Count == 0 || _currentSheet == null) return;

            var contentWidth = _currentSheet.Width;
            var contentHeight = _currentSheet.Height;
            var viewportWidth = LayoutCanvas.ActualWidth;
            var viewportHeight = LayoutCanvas.ActualHeight;

            if (contentWidth > 0 && contentHeight > 0 && viewportWidth > 0 && viewportHeight > 0)
            {
                var scaleX = viewportWidth / contentWidth;
                var scaleY = viewportHeight / contentHeight;
                var scale = Math.Min(scaleX, scaleY) * 0.9;

                ZoomSlider.Value = Math.Min(Math.Max(scale, ZoomSlider.Minimum), ZoomSlider.Maximum);
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PNG Image|*.png",
                Title = "Save Layout as PNG",
                FileName = $"GlassCuttingLayout_{DateTime.Now:yyyyMMddHHmmss}.png"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    if (LayoutCanvas.Children.Count == 0)
                    {
                        MessageBox.Show("No layout to export. Please run an optimization first.", "Export Error");
                        return;
                    }

                    LayoutCanvas.UpdateLayout();

                    // Determine content bounds
                    double minX = double.MaxValue, minY = double.MaxValue;
                    double maxX = double.MinValue, maxY = double.MinValue;
                    bool hasContent = false;

                    foreach (UIElement child in LayoutCanvas.Children)
                    {
                        if (child is FrameworkElement element)
                        {
                            double x = double.IsNaN(Canvas.GetLeft(element)) ? 0 : Canvas.GetLeft(element);
                            double y = double.IsNaN(Canvas.GetTop(element)) ? 0 : Canvas.GetTop(element);
                            double width = element.ActualWidth > 0 ? element.ActualWidth : element.Width;
                            double height = element.ActualHeight > 0 ? element.ActualHeight : element.Height;

                            if (!double.IsNaN(width) && !double.IsNaN(height) && width > 0 && height > 0)
                            {
                                minX = Math.Min(minX, x);
                                minY = Math.Min(minY, y);
                                maxX = Math.Max(maxX, x + width);
                                maxY = Math.Max(maxY, y + height);
                                hasContent = true;
                            }
                        }
                    }

                    if (!hasContent) return;

                    // Add padding around content
                    double padding = 50;
                    double contentWidth = maxX - minX;
                    double contentHeight = maxY - minY;
                    double exportWidth = contentWidth + (padding * 2);
                    double exportHeight = contentHeight + (padding * 2);

                    // Render target
                    var renderTarget = new RenderTargetBitmap(
                        (int)Math.Ceiling(exportWidth),
                        (int)Math.Ceiling(exportHeight),
                        96, 96, PixelFormats.Pbgra32);

                    // Render canvas into image
                    var visual = new DrawingVisual();
                    using (var context = visual.RenderOpen())
                    {
                        context.DrawRectangle(Brushes.White, null, new Rect(0, 0, exportWidth, exportHeight));

                        var canvasBrush = new VisualBrush(LayoutCanvas)
                        {
                            Stretch = Stretch.None,
                            AlignmentX = AlignmentX.Center,
                            AlignmentY = AlignmentY.Center
                        };

                        context.DrawRectangle(canvasBrush, null,
                            new Rect(padding - minX, padding - minY, contentWidth, contentHeight));
                    }

                    renderTarget.Render(visual);

                    // Save PNG
                    var pngEncoder = new PngBitmapEncoder();
                    pngEncoder.Frames.Add(BitmapFrame.Create(renderTarget));
                    using (var stream = File.Create(saveDialog.FileName))
                    {
                        pngEncoder.Save(stream);
                    }

                    StatusText.Text = $"Layout saved to {System.IO.Path.GetFileName(saveDialog.FileName)}";
                }
                catch (Exception ex)
                {
                    StatusText.Text = $"Export failed: {ex.Message}";
                    MessageBox.Show($"Error saving image: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
