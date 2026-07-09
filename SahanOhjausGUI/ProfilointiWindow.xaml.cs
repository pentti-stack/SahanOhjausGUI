using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SahanOhjausGUI
{
    public partial class ProfilointiWindow : Window
    {
        // ── Data ─────────────────────────────────────────────────────────────
        private readonly ProfilointiData _data;
        private readonly Dictionary<int, ProfiRajat> _rajat;
        private double[] _offsets; // [0]=T1 … [7]=T8

        public Action<double[], Dictionary<int, ProfiRajat>>? OnOffsetitChanged;

        // ── Constants ────────────────────────────────────────────────────────
        private const double OhjuriLeveysMm = 25.0;
        private const double OhjuriKorkeusMm = 50.0;

        // ── Constructor ───────────────────────────────────────────────────────
        public ProfilointiWindow(ProfilointiData data, double[] profOffsets,
                                 Dictionary<int, ProfiRajat> profRajat)
        {
            InitializeComponent();
            _data = data;
            _offsets = (double[])profOffsets.Clone();
            _rajat = profRajat;

            PaivitaLabekit();
        }

        // ── Calculations ──────────────────────────────────────────────────────
        /// <summary>Returns korkeusMm = max piece leveys from the data bundle.</summary>
        private double LaskeKorkeus()
        {
            if (_data.Kappaleet.Count == 0) return 100.0;
            double max = _data.Kappaleet.Max(k => k.LeveysMm);
            return max > 0 ? max : 100.0;
        }

        /// <summary>Compute all T1–T8 positions in mm.</summary>
        private (double t1, double t2, double t3, double t4, double t5, double t6, double t7, double t8)
            LaskeArvot()
        {
            double halfW = _data.HalfWidthMm;
            double korkeus = LaskeKorkeus();

            double t1 = halfW + _offsets[0];
            double t2 = -halfW + _offsets[1];
            double t3 = korkeus / 2.0 + _offsets[2];
            double t4 = -(korkeus / 2.0) + _offsets[3];
            double t5 = korkeus / 2.0 + _offsets[4];
            double t6 = -(korkeus / 2.0) + _offsets[5];
            double t7 = t1 + OhjuriLeveysMm + _offsets[6];
            double t8 = t2 - OhjuriLeveysMm + _offsets[7];

            return (t1, t2, t3, t4, t5, t6, t7, t8);
        }

        // ── UI updates ────────────────────────────────────────────────────────
        private void PaivitaLabekit()
        {
            var (t1, t2, t3, t4, t5, t6, t7, t8) = LaskeArvot();

            T1ArvoLabel.Text = $"{t1:F1} mm";
            T2ArvoLabel.Text = $"{t2:F1} mm";
            T3ArvoLabel.Text = $"{t3:F1} mm";
            T4ArvoLabel.Text = $"{t4:F1} mm";
            T5ArvoLabel.Text = $"{t5:F1} mm";
            T6ArvoLabel.Text = $"{t6:F1} mm";
            T7ArvoLabel.Text = $"{t7:F1} mm";
            T8ArvoLabel.Text = $"{t8:F1} mm";

            ProfOffsetT1Label.Text = $"Offset: {_offsets[0]:F1} mm";
            ProfOffsetT2Label.Text = $"Offset: {_offsets[1]:F1} mm";
            ProfOffsetT3Label.Text = $"Offset: {_offsets[2]:F1} mm";
            ProfOffsetT4Label.Text = $"Offset: {_offsets[3]:F1} mm";
            ProfOffsetT5Label.Text = $"Offset: {_offsets[4]:F1} mm";
            ProfOffsetT6Label.Text = $"Offset: {_offsets[5]:F1} mm";
            ProfOffsetT7Label.Text = $"Offset: {_offsets[6]:F1} mm";
            ProfOffsetT8Label.Text = $"Offset: {_offsets[7]:F1} mm";

            TarkistaRajat(t1, t2, t3, t4, t5, t6, t7, t8);
            PaivitaArvoStrip(t1, t2, t3, t4, t5, t6, t7, t8);
            PiirraCanvas();
        }

        private void TarkistaRajat(double t1, double t2, double t3, double t4,
                                   double t5, double t6, double t7, double t8)
        {
            var vals = new[] { t1, t2, t3, t4, t5, t6, t7, t8 };
            var warns = new List<string>();
            for (int i = 1; i <= 8; i++)
            {
                if (!_rajat.TryGetValue(i, out var r)) continue;
                double v = vals[i - 1];
                if (v < r.Min || v > r.Max)
                    warns.Add($"T{i}={v:F1} (raja {r.Min:F0}…{r.Max:F0})");
            }
            ProfStatusLabel.Text = warns.Count > 0
                ? "⚠ Raja ylitetty: " + string.Join(", ", warns)
                : "";
        }

        private void PaivitaArvoStrip(double t1, double t2, double t3, double t4,
                                      double t5, double t6, double t7, double t8)
        {
            TArvoStrip.Children.Clear();
            var items = new[]
            {
                ("T1", t1, Color.FromRgb(129, 199, 132)),
                ("T2", t2, Color.FromRgb(239, 154, 154)),
                ("T3", t3, Color.FromRgb(144, 202, 249)),
                ("T4", t4, Color.FromRgb(128, 222, 234)),
                ("T5", t5, Color.FromRgb(255, 224, 130)),
                ("T6", t6, Color.FromRgb(255, 249, 196)),
                ("T7", t7, Color.FromRgb(189, 189, 189)),
                ("T8", t8, Color.FromRgb(189, 189, 189))
            };
            foreach (var (lbl, val, col) in items)
            {
                bool outOfRange = false;
                int idx = int.Parse(lbl[1..]);
                if (_rajat.TryGetValue(idx, out var r)) outOfRange = val < r.Min || val > r.Max;

                var border = new Border
                {
                    Background = new SolidColorBrush(Color.FromArgb(40, col.R, col.G, col.B)),
                    BorderBrush = new SolidColorBrush(outOfRange ? Colors.OrangeRed : col),
                    BorderThickness = new Thickness(outOfRange ? 2 : 1),
                    CornerRadius = new CornerRadius(3),
                    Padding = new Thickness(5, 2, 5, 2),
                    Margin = new Thickness(0, 0, 6, 0)
                };
                var sp = new StackPanel { Orientation = Orientation.Horizontal };
                sp.Children.Add(new TextBlock
                {
                    Text = lbl + " ",
                    Foreground = new SolidColorBrush(col),
                    FontWeight = FontWeights.Bold,
                    FontSize = 11
                });
                sp.Children.Add(new TextBlock
                {
                    Text = $"{val:F1} mm",
                    Foreground = new SolidColorBrush(outOfRange ? Colors.OrangeRed : Colors.White),
                    FontSize = 11
                });
                border.Child = sp;
                TArvoStrip.Children.Add(border);
            }
        }

        // ── Canvas drawing ────────────────────────────────────────────────────
        private void ProfCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PiirraCanvas();
        }

        private void PiirraCanvas()
        {
            ProfCanvas.Children.Clear();
            double W = ProfCanvas.ActualWidth;
            double H = ProfCanvas.ActualHeight;
            if (W < 50 || H < 50) return;

            double scale = Math.Min((W / 2.0 - 40) / 320.0, (H / 2.0 - 40) / 320.0);
            double cX = W / 2.0;
            double cY = H / 2.0;

            double ToCanvasX(double mm) => cX + mm * scale;
            double ToCanvasY(double mm) => cY - mm * scale;

            var (t1, t2, t3, t4, t5, t6, t7, t8) = LaskeArvot();

            // ── 0-level (green horizontal) ──
            AddLine(ProfCanvas, 0, cY, W, cY, Color.FromRgb(76, 175, 80), 1, isDash: false);

            // ── Center line (blue dashed vertical) ──
            AddLine(ProfCanvas, cX, 0, cX, H, Color.FromRgb(79, 195, 247), 1, isDash: true);

            // ── Y-axis scale (left margin) ──
            PiirraYAsteikko(ProfCanvas, cX, cY, scale, W, H);

            // ── Log circle (gold dashed) ──
            if (_data.TukkiHalkaisija > 0)
            {
                double r = _data.TukkiHalkaisija / 2.0 * scale;
                var ell = new Ellipse
                {
                    Width = r * 2,
                    Height = r * 2,
                    Stroke = new SolidColorBrush(Color.FromRgb(210, 170, 100)),
                    StrokeThickness = 1.5,
                    StrokeDashArray = new DoubleCollection { 6, 4 },
                    Fill = Brushes.Transparent
                };
                Canvas.SetLeft(ell, cX - r);
                Canvas.SetTop(ell, cY - r);
                ProfCanvas.Children.Add(ell);
            }

            // ── Pieces (from jakosaha data) ──
            foreach (var kap in _data.Kappaleet)
            {
                double xLeft = ToCanvasX(kap.XmmFromCenter);
                double half = kap.LeveysMm / 2.0 * scale;
                double yTop = cY - half;
                double pxW = kap.PaksuusMm * scale;
                double pxH = kap.LeveysMm * scale;

                var rect = new Rectangle
                {
                    Width = Math.Max(1, pxW),
                    Height = Math.Max(1, pxH),
                    Fill = new SolidColorBrush(kap.Vari),
                    Stroke = new SolidColorBrush(Color.FromArgb(120, 255, 255, 255)),
                    StrokeThickness = 0.5
                };
                Canvas.SetLeft(rect, xLeft);
                Canvas.SetTop(rect, yTop);
                ProfCanvas.Children.Add(rect);

                // Label
                if (pxH > 12 && pxW > 10)
                {
                    var tb = new TextBlock
                    {
                        Text = kap.Label,
                        Foreground = Brushes.White,
                        FontSize = 9,
                        Opacity = 0.8
                    };
                    Canvas.SetLeft(tb, xLeft + 1);
                    Canvas.SetTop(tb, yTop + 1);
                    ProfCanvas.Children.Add(tb);
                }
            }

            // ── T1 – vertical green line ──
            double xT1 = ToCanvasX(t1);
            bool t1Warn = !IsInRange(1, t1);
            AddLine(ProfCanvas, xT1, 0, xT1, H, t1Warn ? Colors.OrangeRed : Color.FromRgb(129, 199, 132), 2);

            // ── T2 – vertical red line ──
            double xT2 = ToCanvasX(t2);
            bool t2Warn = !IsInRange(2, t2);
            AddLine(ProfCanvas, xT2, 0, xT2, H, t2Warn ? Colors.OrangeRed : Color.FromRgb(239, 154, 154), 2);

            // ── T3 – right upper horizontal blue line ──
            double yT3 = ToCanvasY(t3);
            bool t3Warn = !IsInRange(3, t3);
            AddLine(ProfCanvas, xT1, yT3, W, yT3, t3Warn ? Colors.OrangeRed : Color.FromRgb(144, 202, 249), 1.5);

            // ── T4 – right lower horizontal cyan line ──
            double yT4 = ToCanvasY(t4);
            bool t4Warn = !IsInRange(4, t4);
            AddLine(ProfCanvas, xT1, yT4, W, yT4, t4Warn ? Colors.OrangeRed : Color.FromRgb(128, 222, 234), 1.5);

            // ── T5 – left upper horizontal yellow line ──
            double yT5 = ToCanvasY(t5);
            bool t5Warn = !IsInRange(5, t5);
            AddLine(ProfCanvas, 0, yT5, xT2, yT5, t5Warn ? Colors.OrangeRed : Color.FromRgb(255, 224, 130), 1.5);

            // ── T6 – left lower horizontal light-yellow line ──
            double yT6 = ToCanvasY(t6);
            bool t6Warn = !IsInRange(6, t6);
            AddLine(ProfCanvas, 0, yT6, xT2, yT6, t6Warn ? Colors.OrangeRed : Color.FromRgb(255, 249, 196), 1.5);

            // ── T7 – right guide rail (gray rectangle) ──
            double xT7 = ToCanvasX(t7);
            bool t7Warn = !IsInRange(7, t7);
            double t7W = OhjuriLeveysMm * scale;
            double t7H = OhjuriKorkeusMm * scale;
            var t7Rect = new Rectangle
            {
                Width = Math.Max(2, t7W),
                Height = Math.Max(2, t7H),
                Fill = new SolidColorBrush(Color.FromArgb(160, 120, 120, 120)),
                Stroke = new SolidColorBrush(t7Warn ? Colors.OrangeRed : Color.FromRgb(189, 189, 189)),
                StrokeThickness = 1.5
            };
            Canvas.SetLeft(t7Rect, xT7);
            Canvas.SetTop(t7Rect, cY - t7H / 2.0);
            ProfCanvas.Children.Add(t7Rect);

            // ── T8 – left guide rail (gray rectangle) ──
            double xT8Right = ToCanvasX(t8);
            bool t8Warn = !IsInRange(8, t8);
            double t8W = OhjuriLeveysMm * scale;
            double t8H = OhjuriKorkeusMm * scale;
            var t8Rect = new Rectangle
            {
                Width = Math.Max(2, t8W),
                Height = Math.Max(2, t8H),
                Fill = new SolidColorBrush(Color.FromArgb(160, 120, 120, 120)),
                Stroke = new SolidColorBrush(t8Warn ? Colors.OrangeRed : Color.FromRgb(189, 189, 189)),
                StrokeThickness = 1.5
            };
            // T8 left edge = xT8Right - t8W (right edge at T8 position)
            Canvas.SetLeft(t8Rect, xT8Right - t8W);
            Canvas.SetTop(t8Rect, cY - t8H / 2.0);
            ProfCanvas.Children.Add(t8Rect);

            // ── Blade labels on canvas ──
            AddCanvasLabel(ProfCanvas, "T1", xT1 + 3, 5, Color.FromRgb(129, 199, 132));
            AddCanvasLabel(ProfCanvas, "T2", xT2 + 3, 5, Color.FromRgb(239, 154, 154));
            AddCanvasLabel(ProfCanvas, $"T3 {t3:F1}", W - 55, yT3 - 14, Color.FromRgb(144, 202, 249));
            AddCanvasLabel(ProfCanvas, $"T4 {t4:F1}", W - 55, yT4 + 2, Color.FromRgb(128, 222, 234));
            AddCanvasLabel(ProfCanvas, $"T5 {t5:F1}", 4, yT5 - 14, Color.FromRgb(255, 224, 130));
            AddCanvasLabel(ProfCanvas, $"T6 {t6:F1}", 4, yT6 + 2, Color.FromRgb(255, 249, 196));
        }

        private bool IsInRange(int t, double val)
        {
            if (!_rajat.TryGetValue(t, out var r)) return true;
            return val >= r.Min && val <= r.Max;
        }

        private static void AddLine(Canvas canvas, double x1, double y1, double x2, double y2,
                                    Color color, double thickness, bool isDash = false)
        {
            var line = new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = new SolidColorBrush(color),
                StrokeThickness = thickness
            };
            if (isDash) line.StrokeDashArray = new DoubleCollection { 8, 5 };
            canvas.Children.Add(line);
        }

        private static void AddCanvasLabel(Canvas canvas, string text, double x, double y, Color color)
        {
            var tb = new TextBlock
            {
                Text = text,
                Foreground = new SolidColorBrush(color),
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Background = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0))
            };
            Canvas.SetLeft(tb, x);
            Canvas.SetTop(tb, y);
            canvas.Children.Add(tb);
        }

        private static void PiirraYAsteikko(Canvas canvas, double cX, double cY,
                                            double scale, double W, double H)
        {
            int step = 50;
            int maxMm = (int)Math.Ceiling((H / 2.0) / scale / step) * step + step;
            for (int mm = -maxMm; mm <= maxMm; mm += step)
            {
                double cy = cY - mm * scale;
                if (cy < 0 || cy > H) continue;
                bool major = mm % 100 == 0;
                double tickLen = major ? 10 : 6;
                // Tick mark on left
                AddLine(canvas, 0, cy, tickLen, cy, major ? Colors.Gray : Color.FromRgb(60, 60, 60), major ? 1 : 0.5);
                if (major)
                {
                    var tb = new TextBlock
                    {
                        Text = $"{mm}",
                        Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
                        FontSize = 9
                    };
                    Canvas.SetLeft(tb, tickLen + 1);
                    Canvas.SetTop(tb, cy - 7);
                    canvas.Children.Add(tb);
                }
            }
        }

        // ── Offset button handlers ────────────────────────────────────────────
        private void MuutaOffset(int idx, double delta)
        {
            _offsets[idx] = Math.Round(_offsets[idx] + delta, 1);
            PaivitaLabekit();
        }

        private void ProfT1Minus_Click(object s, RoutedEventArgs e) => MuutaOffset(0, -0.1);
        private void ProfT1Plus_Click(object s, RoutedEventArgs e) => MuutaOffset(0, +0.1);
        private void ProfT2Minus_Click(object s, RoutedEventArgs e) => MuutaOffset(1, -0.1);
        private void ProfT2Plus_Click(object s, RoutedEventArgs e) => MuutaOffset(1, +0.1);
        private void ProfT3Minus_Click(object s, RoutedEventArgs e) => MuutaOffset(2, -0.1);
        private void ProfT3Plus_Click(object s, RoutedEventArgs e) => MuutaOffset(2, +0.1);
        private void ProfT4Minus_Click(object s, RoutedEventArgs e) => MuutaOffset(3, -0.1);
        private void ProfT4Plus_Click(object s, RoutedEventArgs e) => MuutaOffset(3, +0.1);
        private void ProfT5Minus_Click(object s, RoutedEventArgs e) => MuutaOffset(4, -0.1);
        private void ProfT5Plus_Click(object s, RoutedEventArgs e) => MuutaOffset(4, +0.1);
        private void ProfT6Minus_Click(object s, RoutedEventArgs e) => MuutaOffset(5, -0.1);
        private void ProfT6Plus_Click(object s, RoutedEventArgs e) => MuutaOffset(5, +0.1);
        private void ProfT7Minus_Click(object s, RoutedEventArgs e) => MuutaOffset(6, -0.1);
        private void ProfT7Plus_Click(object s, RoutedEventArgs e) => MuutaOffset(6, +0.1);
        private void ProfT8Minus_Click(object s, RoutedEventArgs e) => MuutaOffset(7, -0.1);
        private void ProfT8Plus_Click(object s, RoutedEventArgs e) => MuutaOffset(7, +0.1);

        // ── Lähetä ───────────────────────────────────────────────────────────
        private void Laheta_Click(object sender, RoutedEventArgs e)
        {
            OnOffsetitChanged?.Invoke((double[])_offsets.Clone(), _rajat);
            Close();
        }
    }
}