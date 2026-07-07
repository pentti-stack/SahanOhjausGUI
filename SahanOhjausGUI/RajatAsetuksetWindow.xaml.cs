using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SahanOhjausGUI
{
    public class TeraRajat
    {
        public double Min { get; set; } = -340.0;
        public double Max { get; set; } = 340.0;
        public double Lepopaikka { get; set; } = 340.0;
        public double Vaisto { get; set; } = 140.0;
    }

    public class PhRajat
    {
        public double LepoVasen { get; set; } = 0.0;
        public double LepoOikea { get; set; } = 0.0;
    }

    public partial class RajatAsetuksetWindow : Window
    {
        private readonly Dictionary<int, TeraRajat> rajat;
        private readonly Dictionary<string, PhRajat> phRajat;
        private double turvaEtaisyys;

        private readonly Dictionary<int, (TextBox min, TextBox max, TextBox lepopaikka, TextBox vaisto)> textBoxes = new();
        private readonly Dictionary<string, (TextBox vasen, TextBox oikea)> phBoxes = new();
        private TextBox? turvaTb;

        public event Action<Dictionary<int, TeraRajat>>? OnRajatChanged;
        public event Action<Dictionary<string, PhRajat>, double>? OnPhRajatChanged;

        private static readonly (int numero, string nimi, Color vari)[] TeraInfo =
        {
            (1, "T1  \u25c0 Vasen",  Color.FromRgb(255, 107, 107)),
            (2, "T2  \u25b6 Oikea",  Color.FromRgb(107, 203, 119)),
            (3, "T3  \u25c0 Vasen",  Color.FromRgb(255, 107, 107)),
            (4, "T4  \u25b6 Oikea",  Color.FromRgb(107, 203, 119)),
            (5, "T5  \u25c0 Vasen",  Color.FromRgb(255, 107, 107)),
            (6, "T6  \u25b6 Oikea",  Color.FromRgb(107, 203, 119)),
        };

        private static readonly (string key, string nimi, Color vari)[] PhInfo =
        {
            ("PH1V", "PH1  \u25c0 Vasen", Color.FromRgb(255, 200, 80)),
            ("PH1O", "PH1  \u25b6 Oikea", Color.FromRgb(255, 200, 80)),
            ("PH2V", "PH2  \u25c0 Vasen", Color.FromRgb(80, 200, 255)),
            ("PH2O", "PH2  \u25b6 Oikea", Color.FromRgb(80, 200, 255)),
        };

        public RajatAsetuksetWindow(Dictionary<int, TeraRajat> nykyisetRajat,
            Dictionary<string, PhRajat> nykyisetPhRajat, double nykyinenTurva)
        {
            InitializeComponent();
            rajat = new Dictionary<int, TeraRajat>(nykyisetRajat);
            phRajat = new Dictionary<string, PhRajat>(nykyisetPhRajat);
            turvaEtaisyys = nykyinenTurva;
            LuoTeraRivit();
            LuoPhRivit();
            LuoTurvaRivi();
        }

        private void LuoTeraRivit()
        {
            TeratPanel.Children.Clear();
            textBoxes.Clear();

            TeratPanel.Children.Add(LuoOtsikkoRivi("Ter\u00e4", "Min (mm)", "Max (mm)", "Lepopaikka (mm)", "V\u00e4ist\u00f6 (mm)"));

            foreach (var (numero, nimi, vari) in TeraInfo)
            {
                if (!rajat.ContainsKey(numero))
                    rajat[numero] = new TeraRajat();

                var minBox = LuoTextBox(rajat[numero].Min.ToString("F1", CultureInfo.InvariantCulture));
                var maxBox = LuoTextBox(rajat[numero].Max.ToString("F1", CultureInfo.InvariantCulture));
                var lepoBox = LuoTextBox(rajat[numero].Lepopaikka.ToString("F1", CultureInfo.InvariantCulture));
                var vaistoBox = LuoTextBox(rajat[numero].Vaisto.ToString("F1", CultureInfo.InvariantCulture));

                TeratPanel.Children.Add(LuoNelikenttaRivi(nimi, vari, minBox, maxBox, lepoBox, vaistoBox));
                textBoxes[numero] = (minBox, maxBox, lepoBox, vaistoBox);
            }
        }

        private void LuoPhRivit()
        {
            TeratPanel.Children.Add(new Border { Height = 12 });
            TeratPanel.Children.Add(new TextBlock
            {
                Text = "\u2699 Pelkkahakkurit \u2014 leposijainnit",
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(255, 217, 61)),
                Margin = new Thickness(0, 0, 0, 6)
            });
            TeratPanel.Children.Add(LuoOtsikkoRivi2("Laite", "Lepopaikka vasen (mm)", "Lepopaikka oikea (mm)"));

            foreach (var (key, nimi, vari) in PhInfo)
            {
                if (!phRajat.ContainsKey(key))
                    phRajat[key] = new PhRajat();

                var vasenBox = LuoTextBox(phRajat[key].LepoVasen.ToString("F1", CultureInfo.InvariantCulture));
                var oikeaBox = LuoTextBox(phRajat[key].LepoOikea.ToString("F1", CultureInfo.InvariantCulture));

                TeratPanel.Children.Add(LuoKaksikentkaRivi(nimi, vari, vasenBox, oikeaBox));
                phBoxes[key] = (vasenBox, oikeaBox);
            }
        }

        private void LuoTurvaRivi()
        {
            TeratPanel.Children.Add(new Border { Height = 12 });
            TeratPanel.Children.Add(new TextBlock
            {
                Text = "\U0001f6e1 T1-T2 turvae\u0074\u00e4isyys",
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(255, 107, 107)),
                Margin = new Thickness(0, 0, 0, 6)
            });

            var border = new Border
            {
                Margin = new Thickness(0, 0, 0, 6),
                Background = new SolidColorBrush(Color.FromRgb(40, 40, 40)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(10, 8, 10, 8)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var lbl = new TextBlock
            {
                Text = "Turvae\u0074\u00e4isyys (mm):",
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(255, 107, 107)),
                FontSize = 12,
                VerticalAlignment = VerticalAlignment.Center
            };

            turvaTb = LuoTextBox(turvaEtaisyys.ToString("F1", CultureInfo.InvariantCulture));

            Grid.SetColumn(lbl, 0);
            Grid.SetColumn(turvaTb, 1);
            grid.Children.Add(lbl);
            grid.Children.Add(turvaTb);
            border.Child = grid;
            TeratPanel.Children.Add(border);
        }

        // ── UI-apufunktiot ───────────────────────────────────────────────────

        private static Border LuoOtsikkoRivi(string col0, string col1, string col3, string col5, string col7)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            void Add(int col, string text)
            {
                var tb = new TextBlock
                {
                    Text = text,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                    HorizontalAlignment = col == 0 ? HorizontalAlignment.Left : HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 4)
                };
                Grid.SetColumn(tb, col);
                grid.Children.Add(tb);
            }

            Add(0, col0); Add(1, col1); Add(3, col3); Add(5, col5); Add(7, col7);
            return new Border { Child = grid };
        }

        private static Border LuoOtsikkoRivi2(string col0, string col1, string col2)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            void Add(int col, string text)
            {
                var tb = new TextBlock
                {
                    Text = text,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                    HorizontalAlignment = col == 0 ? HorizontalAlignment.Left : HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 4)
                };
                Grid.SetColumn(tb, col);
                grid.Children.Add(tb);
            }

            Add(0, col0); Add(1, col1); Add(3, col2);
            return new Border { Child = grid };
        }

        private static Border LuoNelikenttaRivi(string nimi, Color vari,
            TextBox b1, TextBox b2, TextBox b3, TextBox b4)
        {
            var border = new Border
            {
                Margin = new Thickness(0, 0, 0, 6),
                Background = new SolidColorBrush(Color.FromRgb(40, 40, 40)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(10, 8, 10, 8)
            };
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var lbl = new TextBlock
            {
                Text = nimi,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(vari),
                FontSize = 12,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(lbl, 0); Grid.SetColumn(b1, 1);
            Grid.SetColumn(b2, 3); Grid.SetColumn(b3, 5); Grid.SetColumn(b4, 7);
            grid.Children.Add(lbl); grid.Children.Add(b1);
            grid.Children.Add(b2); grid.Children.Add(b3); grid.Children.Add(b4);
            border.Child = grid;
            return border;
        }

        private static Border LuoKaksikentkaRivi(string nimi, Color vari, TextBox b1, TextBox b2)
        {
            var border = new Border
            {
                Margin = new Thickness(0, 0, 0, 6),
                Background = new SolidColorBrush(Color.FromRgb(40, 40, 40)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(10, 8, 10, 8)
            };
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var lbl = new TextBlock
            {
                Text = nimi,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(vari),
                FontSize = 12,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(lbl, 0); Grid.SetColumn(b1, 1); Grid.SetColumn(b2, 3);
            grid.Children.Add(lbl); grid.Children.Add(b1); grid.Children.Add(b2);
            border.Child = grid;
            return border;
        }

        private static TextBox LuoTextBox(string arvo) => new TextBox
        {
            Text = arvo,
            TextAlignment = TextAlignment.Center
        };

        private void Tallenna_Click(object sender, RoutedEventArgs e)
        {
            var paivitetyt = new Dictionary<int, TeraRajat>();
            var paivitetytPh = new Dictionary<string, PhRajat>();
            bool virhe = false;

            foreach (var kvp in textBoxes)
            {
                bool minOk = double.TryParse(kvp.Value.min.Text.Replace(",", "."),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out double minVal);
                bool maxOk = double.TryParse(kvp.Value.max.Text.Replace(",", "."),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out double maxVal);
                bool lepoOk = double.TryParse(kvp.Value.lepopaikka.Text.Replace(",", "."),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out double lepoVal);
                bool vaistoOk = double.TryParse(kvp.Value.vaisto.Text.Replace(",", "."),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out double vaistoVal);

                bool rangeOk = minOk && maxOk && minVal < maxVal;
                kvp.Value.min.BorderBrush = minOk && rangeOk ? Brushes.Gray : Brushes.OrangeRed;
                kvp.Value.max.BorderBrush = maxOk && rangeOk ? Brushes.Gray : Brushes.OrangeRed;
                kvp.Value.lepopaikka.BorderBrush = lepoOk ? Brushes.Gray : Brushes.OrangeRed;
                kvp.Value.vaisto.BorderBrush = vaistoOk ? Brushes.Gray : Brushes.OrangeRed;

                if (!minOk || !maxOk || !lepoOk || !vaistoOk || !rangeOk) { virhe = true; continue; }

                paivitetyt[kvp.Key] = new TeraRajat
                { Min = minVal, Max = maxVal, Lepopaikka = lepoVal, Vaisto = vaistoVal };
            }

            foreach (var kvp in phBoxes)
            {
                bool vasenOk = double.TryParse(kvp.Value.vasen.Text.Replace(",", "."),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out double vasenVal);
                bool oikeaOk = double.TryParse(kvp.Value.oikea.Text.Replace(",", "."),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out double oikeaVal);

                kvp.Value.vasen.BorderBrush = vasenOk ? Brushes.Gray : Brushes.OrangeRed;
                kvp.Value.oikea.BorderBrush = oikeaOk ? Brushes.Gray : Brushes.OrangeRed;

                if (!vasenOk || !oikeaOk) { virhe = true; continue; }
                paivitetytPh[kvp.Key] = new PhRajat { LepoVasen = vasenVal, LepoOikea = oikeaVal };
            }

            double turvaVal = turvaEtaisyys;
            if (turvaTb != null)
            {
                bool turvaOk = double.TryParse(turvaTb.Text.Replace(",", "."),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out turvaVal) && turvaVal > 0;
                turvaTb.BorderBrush = turvaOk ? Brushes.Gray : Brushes.OrangeRed;
                if (!turvaOk) virhe = true;
            }

            if (virhe)
            {
                MessageBox.Show("Tarkista arvot \u2014 min t\u00e4ytyy olla pienempi kuin max.",
                    "Virhe", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            OnRajatChanged?.Invoke(paivitetyt);
            OnPhRajatChanged?.Invoke(paivitetytPh, turvaVal);
            Close();
        }

        private void Peruuta_Click(object sender, RoutedEventArgs e) => Close();
    }
}