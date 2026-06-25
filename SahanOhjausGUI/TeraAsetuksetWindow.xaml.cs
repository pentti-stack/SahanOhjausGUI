using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SahanOhjausGUI
{
    public partial class TeraAsetuksetWindow : Window, IDisposable
    {
        private readonly Dictionary<int, TeraParametrit> _teraParametrit;
        private readonly Dictionary<string, TextBox> _teraTextBoxes = new();
        private readonly Dictionary<int, TextBlock> _offsetLabels = new();
        private readonly Dictionary<Color, Style> _nappiStyleCache = new();

        private TextBlock? _statusTextBlock;
        private System.Timers.Timer? _statusTimer;
        private bool _disposed = false;

        public Action<Dictionary<int, TeraParametrit>>? OnParametritChanged;

        public TeraAsetuksetWindow(Dictionary<int, TeraParametrit> terat)
        {
            InitializeComponent();
            _teraParametrit = terat;

            Title = "Teräasetukset";
            Width = 900; Height = 620;
            MinWidth = 750; MinHeight = 500;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.CanResize;
            Background = new SolidColorBrush(Color.FromRgb(20, 20, 20));

            Loaded += (s, e) =>
            {
                try
                {
                    if (TeraPanel == null)
                    {
                        MessageBox.Show(
                            "TeraPanel on null — tarkista XAML x:Name=\"TeraPanel\"",
                            "Virhe", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    LuoKontrollit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Virhe teräasetuksissa:\n\n{ex.Message}\n\n{ex.StackTrace}",
                        "Virhe", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
        }

        private void LuoKontrollit()
        {
            TeraPanel.Children.Clear();
            _teraTextBoxes.Clear();
            _offsetLabels.Clear();
            _nappiStyleCache.Clear();

            TeraPanel.Children.Add(new TextBlock
            {
                Text = "Teräparametrit (mm)",
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(79, 195, 247)),
                Margin = new Thickness(0, 0, 0, 6)
            });

            TeraPanel.Children.Add(new TextBlock
            {
                Text = "Muokkaa terien arvoja ja kätisyyttä. Muutokset päivittyvät heti graafiseen näkymään.",
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 16)
            });

            // OIKEA SAHA
            TeraPanel.Children.Add(LuoSahaOtsikko("● Oikea saha", Color.FromRgb(107, 203, 119)));
            var oikeaGrid = LuoTeraGrid();
            int col = 0;
            foreach (var teraNum in new[] { 2, 4, 6 })
            {
                if (!_teraParametrit.TryGetValue(teraNum, out var tera)) { col += 2; continue; }
                var kortti = LuoTeraKortti(teraNum, tera, new SolidColorBrush(Color.FromRgb(107, 203, 119)));
                Grid.SetColumn(kortti, col);
                oikeaGrid.Children.Add(kortti);
                col += 2;
            }
            TeraPanel.Children.Add(oikeaGrid);

            // VASEN SAHA
            TeraPanel.Children.Add(LuoSahaOtsikko("● Vasen saha", Color.FromRgb(255, 107, 107)));
            var vasenGrid = LuoTeraGrid();
            col = 0;
            foreach (var teraNum in new[] { 1, 3, 5 })
            {
                if (!_teraParametrit.TryGetValue(teraNum, out var tera)) { col += 2; continue; }
                var kortti = LuoTeraKortti(teraNum, tera, new SolidColorBrush(Color.FromRgb(255, 107, 107)));
                Grid.SetColumn(kortti, col);
                vasenGrid.Children.Add(kortti);
                col += 2;
            }
            TeraPanel.Children.Add(vasenGrid);

            TeraPanel.Children.Add(new Border
            {
                Height = 1,
                Background = new SolidColorBrush(Color.FromRgb(60, 60, 60)),
                Margin = new Thickness(0, 4, 0, 12)
            });

            var bottomRow = new Grid();
            bottomRow.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            bottomRow.ColumnDefinitions.Add(
                new ColumnDefinition { Width = GridLength.Auto });

            _statusTextBlock = new TextBlock
            {
                Text = "Valmis",
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                TextWrapping = TextWrapping.Wrap,
                MinHeight = 20
            };
            Grid.SetColumn(_statusTextBlock, 0);
            bottomRow.Children.Add(_statusTextBlock);

            var suljeBtn = new Button
            {
                Content = "✓  Tallenna ja sulje",
                Height = 42,
                Padding = new Thickness(20, 0, 20, 0),
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            suljeBtn.Style = GetOrCreateNappiStyle(Color.FromRgb(76, 175, 80));
            suljeBtn.Click += (s, e) => Close();
            Grid.SetColumn(suljeBtn, 1);
            bottomRow.Children.Add(suljeBtn);

            TeraPanel.Children.Add(bottomRow);
        }

        private TextBlock LuoSahaOtsikko(string teksti, Color vari) =>
            new TextBlock
            {
                Text = teksti,
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(vari),
                Margin = new Thickness(0, 0, 0, 8)
            };

        private Grid LuoTeraGrid()
        {
            var grid = new Grid { Margin = new Thickness(0, 0, 0, 20) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            return grid;
        }

        private Border LuoTeraKortti(int teraNum, TeraParametrit tera, SolidColorBrush accentColor)
        {
            bool isPaaTera = teraNum == 4 || teraNum == 3;

            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(40, 40, 40)),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(14),
                BorderBrush = isPaaTera
                    ? new SolidColorBrush(Color.FromRgb(255, 193, 7))
                    : new SolidColorBrush(Color.FromRgb(70, 70, 70)),
                BorderThickness = new Thickness(isPaaTera ? 2 : 1)
            };

            var stack = new StackPanel();

            var otsikkoRivi = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };
            otsikkoRivi.Children.Add(new TextBlock
            {
                Text = $"Terä {teraNum}",
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Foreground = isPaaTera
                    ? new SolidColorBrush(Color.FromRgb(255, 193, 7))
                    : accentColor,
                VerticalAlignment = VerticalAlignment.Center
            });
            if (isPaaTera)
            {
                otsikkoRivi.Children.Add(new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(255, 193, 7)),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(6, 1, 6, 1),
                    Margin = new Thickness(8, 0, 0, 0),
                    Child = new TextBlock
                    {
                        Text = "PÄÄTERÄ",
                        FontSize = 9,
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                        VerticalAlignment = VerticalAlignment.Center
                    }
                });
            }
            stack.Children.Add(otsikkoRivi);

            stack.Children.Add(new TextBlock
            {
                Text = "Kätisyys:",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                Margin = new Thickness(0, 0, 0, 4)
            });

            var btnPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var btnOikea = new Button
            {
                Content = "⟶  Oikeakätinen",
                Height = 32,
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 6, 0),
                Padding = new Thickness(8, 0, 8, 0),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            var btnVasen = new Button
            {
                Content = "⟵  Vasenkätinen",
                Height = 32,
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(8, 0, 8, 0),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            PaivitaKatisyysNapit(btnOikea, btnVasen, tera.OnkoVasenKatinen);

            btnOikea.Click += (s, e) =>
            {
                if (!_teraParametrit.TryGetValue(teraNum, out var t)) return;
                t.OnkoVasenKatinen = false;
                PaivitaKatisyysNapit(btnOikea, btnVasen, false);
                PaivitaOffsetLabel(teraNum);
                OnParametritChanged?.Invoke(_teraParametrit);
                ShowStatus("✓  Kätisyys päivitetty",
                    new SolidColorBrush(Color.FromRgb(107, 203, 119)), true);
            };

            btnVasen.Click += (s, e) =>
            {
                if (!_teraParametrit.TryGetValue(teraNum, out var t)) return;
                t.OnkoVasenKatinen = true;
                PaivitaKatisyysNapit(btnOikea, btnVasen, true);
                PaivitaOffsetLabel(teraNum);
                OnParametritChanged?.Invoke(_teraParametrit);
                ShowStatus("✓  Kätisyys päivitetty",
                    new SolidColorBrush(Color.FromRgb(107, 203, 119)), true);
            };

            btnPanel.Children.Add(btnOikea);
            btnPanel.Children.Add(btnVasen);
            stack.Children.Add(btnPanel);

            LisaaParametri(stack, teraNum, "Laippa", tera.Laippa);
            LisaaParametri(stack, teraNum, "Runko", tera.Runko);
            LisaaParametri(stack, teraNum, "Rako", tera.Rako);

            var offsetLbl = new TextBlock
            {
                Text = $"Offset: {tera.LaskeOffset():F2} mm",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(150, 200, 150)),
                Margin = new Thickness(0, 8, 0, 0),
                FontStyle = FontStyles.Italic
            };
            _offsetLabels[teraNum] = offsetLbl;
            stack.Children.Add(offsetLbl);

            border.Child = stack;
            return border;
        }

        private void PaivitaKatisyysNapit(Button btnOikea, Button btnVasen, bool onkoVasen)
        {
            btnOikea.Style = GetOrCreateNappiStyle(!onkoVasen
                ? Color.FromRgb(25, 118, 210)
                : Color.FromRgb(55, 55, 55));
            btnVasen.Style = GetOrCreateNappiStyle(onkoVasen
                ? Color.FromRgb(230, 81, 0)
                : Color.FromRgb(55, 55, 55));
        }

        private Style GetOrCreateNappiStyle(Color taustaVari)
        {
            if (_nappiStyleCache.TryGetValue(taustaVari, out var cached))
                return cached;

            var style = new Style(typeof(Button));
            var tausta = new SolidColorBrush(taustaVari);
            tausta.Freeze();

            style.Setters.Add(new Setter(Button.BackgroundProperty, tausta));
            style.Setters.Add(new Setter(Button.ForegroundProperty, Brushes.White));
            style.Setters.Add(new Setter(Button.BorderBrushProperty,
                new SolidColorBrush(Color.FromRgb(80, 80, 80))));
            style.Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(1)));
            style.Setters.Add(new Setter(Button.CursorProperty,
                System.Windows.Input.Cursors.Hand));

            var template = new ControlTemplate(typeof(Button));
            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Border.BackgroundProperty,
                new TemplateBindingExtension(Button.BackgroundProperty));
            borderFactory.SetValue(Border.BorderBrushProperty,
                new TemplateBindingExtension(Button.BorderBrushProperty));
            borderFactory.SetValue(Border.BorderThicknessProperty,
                new TemplateBindingExtension(Button.BorderThicknessProperty));
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));
            borderFactory.SetValue(Border.PaddingProperty, new Thickness(8, 0, 8, 0));

            var contentFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty,
                HorizontalAlignment.Center);
            contentFactory.SetValue(ContentPresenter.VerticalAlignmentProperty,
                VerticalAlignment.Center);
            borderFactory.AppendChild(contentFactory);
            template.VisualTree = borderFactory;
            style.Setters.Add(new Setter(Button.TemplateProperty, template));

            _nappiStyleCache[taustaVari] = style;
            return style;
        }

        private void PaivitaOffsetLabel(int teraNum)
        {
            if (!_teraParametrit.TryGetValue(teraNum, out var tera)) return;
            if (_offsetLabels.TryGetValue(teraNum, out var lbl))
                lbl.Text = $"Offset: {tera.LaskeOffset():F2} mm";
        }

        private void LisaaParametri(StackPanel parent, int teraNum, string param, double value)
        {
            var grid = new Grid { Margin = new Thickness(0, 4, 0, 4) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28) });

            var label = new TextBlock
            {
                Text = $"{param}:",
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200))
            };

            var textBox = new TextBox
            {
                Height = 30,
                Padding = new Thickness(6, 0, 6, 0),
                Text = value.ToString("F1", CultureInfo.InvariantCulture),
                Tag = $"{teraNum}_{param}",
                FontSize = 12,
                Background = new SolidColorBrush(Color.FromRgb(60, 60, 60)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                BorderThickness = new Thickness(1),
                VerticalContentAlignment = VerticalAlignment.Center
            };

            var yksikko = new TextBlock
            {
                Text = "mm",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(140, 140, 140))
            };

            textBox.TextChanged += TextBox_TextChanged;
            _teraTextBoxes[$"{teraNum}_{param}"] = textBox;

            Grid.SetColumn(label, 0);
            Grid.SetColumn(textBox, 1);
            Grid.SetColumn(yksikko, 2);
            grid.Children.Add(label);
            grid.Children.Add(textBox);
            grid.Children.Add(yksikko);
            parent.Children.Add(grid);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox tb || tb.Tag is not string tag) return;
            var parts = tag.Split('_');
            if (parts.Length != 2 || !int.TryParse(parts[0], out int teraNum)) return;

            if (!TryParseValidValue(tb.Text, out double value))
            {
                tb.BorderBrush = new SolidColorBrush(Color.FromRgb(220, 83, 83));
                ShowStatus("⚠  Virheellinen arvo — syötä positiivinen luku",
                    new SolidColorBrush(Color.FromRgb(220, 83, 83)), false);
                return;
            }

            tb.BorderBrush = new SolidColorBrush(Color.FromRgb(100, 100, 100));
            if (!_teraParametrit.TryGetValue(teraNum, out var tera)) return;

            switch (parts[1])
            {
                case "Laippa": tera.Laippa = value; break;
                case "Runko": tera.Runko = value; break;
                case "Rako": tera.Rako = value; break;
            }

            PaivitaOffsetLabel(teraNum);
            OnParametritChanged?.Invoke(_teraParametrit);
            ShowStatus("✓  Päivitetty",
                new SolidColorBrush(Color.FromRgb(107, 203, 119)), true);
        }

        private static bool TryParseValidValue(string input, out double value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(input)) return false;
            if (!double.TryParse(input.Replace(',', '.'),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                return false;
            return !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0;
        }

        private void ShowStatus(string message, Brush brush, bool autoHide)
        {
            if (_statusTextBlock == null) return;
            _statusTextBlock.Text = message;
            _statusTextBlock.Foreground = brush;

            if (!autoHide) return;
            _statusTimer?.Stop();
            _statusTimer?.Dispose();
            _statusTimer = new System.Timers.Timer(2500) { AutoReset = false };
            _statusTimer.Elapsed += (s, e) =>
                Dispatcher.Invoke(() =>
                {
                    if (_statusTextBlock != null)
                    {
                        _statusTextBlock.Text = "Valmis";
                        _statusTextBlock.Foreground =
                            new SolidColorBrush(Color.FromRgb(200, 200, 200));
                    }
                });
            _statusTimer.Start();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _statusTimer?.Stop();
            _statusTimer?.Dispose();
            _disposed = true;
        }

        protected override void OnClosed(EventArgs e)
        {
            Dispose();
            base.OnClosed(e);
        }
    }
}