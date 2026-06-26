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

    public partial class RajatAsetuksetWindow : Window
    {
        private readonly Dictionary<int, TeraRajat> rajat;
        private readonly Dictionary<int, (TextBox min, TextBox max, TextBox lepopaikka, TextBox vaisto)> textBoxes = new();

        public event Action<Dictionary<int, TeraRajat>>? OnRajatChanged;

        private static readonly (int numero, string nimi, Color vari)[] TeraInfo =
        {
            (1, "T1  ◀ Vasen",  Color.FromRgb(255, 107, 107)),
            (2, "T2  ▶ Oikea",  Color.FromRgb(107, 203, 119)),
            (3, "T3  ◀ Vasen",  Color.FromRgb(255, 107, 107)),
            (4, "T4  ▶ Oikea",  Color.FromRgb(107, 203, 119)),
            (5, "T5  ◀ Vasen",  Color.FromRgb(255, 107, 107)),
            (6, "T6  ▶ Oikea",  Color.FromRgb(107, 203, 119)),
        };

        public RajatAsetuksetWindow(Dictionary<int, TeraRajat> nykyisetRajat)
        {
            InitializeComponent();
            rajat = new Dictionary<int, TeraRajat>(nykyisetRajat);
            LuoTeraRivit();
        }

        private void LuoTeraRivit()
        {
            TeratPanel.Children.Clear();
            textBoxes.Clear();

            foreach (var (numero, nimi, vari) in TeraInfo)
            {
                if (!rajat.ContainsKey(numero))
                    rajat[numero] = new TeraRajat();

                var border = new Border
                {
                    Margin = new Thickness(0, 0, 0, 6),
                    Background = new SolidColorBrush(Color.FromRgb(40, 40, 40)),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(10, 8, 10, 8)
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var nimiLbl = new TextBlock
                {
                    Text = nimi,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(vari),
                    FontSize = 12,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var minBox = LuoTextBox(rajat[numero].Min.ToString("F1", CultureInfo.InvariantCulture));
                var maxBox = LuoTextBox(rajat[numero].Max.ToString("F1", CultureInfo.InvariantCulture));
                var lepoBox = LuoTextBox(rajat[numero].Lepopaikka.ToString("F1", CultureInfo.InvariantCulture));
                var vaistoBox = LuoTextBox(rajat[numero].Vaisto.ToString("F1", CultureInfo.InvariantCulture));

                Grid.SetColumn(nimiLbl, 0);
                Grid.SetColumn(minBox, 1);
                Grid.SetColumn(maxBox, 3);
                Grid.SetColumn(lepoBox, 5);
                Grid.SetColumn(vaistoBox, 7);

                grid.Children.Add(nimiLbl);
                grid.Children.Add(minBox);
                grid.Children.Add(maxBox);
                grid.Children.Add(lepoBox);
                grid.Children.Add(vaistoBox);

                border.Child = grid;
                TeratPanel.Children.Add(border);

                textBoxes[numero] = (minBox, maxBox, lepoBox, vaistoBox);
            }
        }

        private static TextBox LuoTextBox(string arvo) => new TextBox
        {
            Text = arvo,
            TextAlignment = TextAlignment.Center
        };

        private void Tallenna_Click(object sender, RoutedEventArgs e)
        {
            var paivitetyt = new Dictionary<int, TeraRajat>();
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

                if (!minOk || !maxOk || !lepoOk || !vaistoOk || !rangeOk)
                {
                    virhe = true;
                    continue;
                }

                paivitetyt[kvp.Key] = new TeraRajat
                {
                    Min = minVal,
                    Max = maxVal,
                    Lepopaikka = lepoVal,
                    Vaisto = vaistoVal
                };
            }

            if (virhe)
            {
                MessageBox.Show("Tarkista arvot — min täytyy olla pienempi kuin max.",
                    "Virhe", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            OnRajatChanged?.Invoke(paivitetyt);
            Close();
        }

        private void Peruuta_Click(object sender, RoutedEventArgs e) => Close();
    }
}