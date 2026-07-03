using System;
using System.Collections.Generic;
using System.Globalization;
using IO = System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SahanOhjausGUI
{
    public class TallennusData
    {
        public Dictionary<int, TeraParametritDto> TeraParametrit { get; set; } = new();
        public Dictionary<int, string> PaksuudetVasen { get; set; } = new();
        public Dictionary<int, string> PaksuudetOikea { get; set; } = new();
        public Dictionary<int, string> PaksuudetYhdistetty { get; set; } = new();
        public int KappaleCount { get; set; } = 4;
        public int YhdistettyCount { get; set; } = 3;
        public string Kuivaus { get; set; } = "0";
        public bool VasenOn { get; set; } = true;
        public bool OikeaOn { get; set; } = true;
        public Dictionary<int, TeraRajatDto> TeraRajat { get; set; } = new();
        public double TurvaEtaisyys { get; set; } = 15.0;
    }

    public class TeraParametritDto
    {
        public bool OnkoVasenKatinen { get; set; }
        public double Rako { get; set; }
        public double Runko { get; set; }
        public double Laippa { get; set; }
    }

    public class TeraRajatDto
    {
        public double Min { get; set; } = -340.0;
        public double Max { get; set; } = 340.0;
        public double Lepopaikka { get; set; } = 340.0;
        public double Vaisto { get; set; } = -140.0;
    }

    public partial class MainWindow : Window
    {
        private readonly Dictionary<int, TeraParametrit> teraParametrit = new();
        private readonly Dictionary<int, TextBox> paksuusTextBoxesVasen = new();
        private readonly Dictionary<int, TextBox> paksuusTextBoxesOikea = new();
        private readonly Dictionary<int, TextBox> paksuusTextBoxesYhdistetty = new();
        private readonly Dictionary<int, TeraRajat> teraRajat = new();

        private double turvaEtaisyys = 15.0;
        private volatile bool _isPiirraVisualRunning = false;

        private const double Vaisto_Sisaterä = -140.0;
        private const double Vaisto_Ulkoterä = 140.0;

        private static readonly string TallennusPolku =
            IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SahanOhjaus", "asetukset.json");

        private static readonly JsonSerializerOptions _jsonOptions =
            new JsonSerializerOptions { WriteIndented = true };

        public MainWindow()
        {
            InitializeComponent();
            InitializeUI();
            LataaTallennus();
            Closing += (s, e) => TallennaTallennus();
        }

        private void InitializeUI()
        {
            teraParametrit[2] = new TeraParametrit { TeraNumero = 2, OnkoVasenKatinen = false, Rako = 4, Runko = 3, Laippa = 6 };
            teraParametrit[4] = new TeraParametrit { TeraNumero = 4, OnkoVasenKatinen = false, Rako = 4, Runko = 3, Laippa = 6 };
            teraParametrit[6] = new TeraParametrit { TeraNumero = 6, OnkoVasenKatinen = false, Rako = 4, Runko = 3, Laippa = 6 };
            teraParametrit[1] = new TeraParametrit { TeraNumero = 1, OnkoVasenKatinen = true, Rako = 4, Runko = 3, Laippa = 6 };
            teraParametrit[3] = new TeraParametrit { TeraNumero = 3, OnkoVasenKatinen = true, Rako = 4, Runko = 3, Laippa = 6 };
            teraParametrit[5] = new TeraParametrit { TeraNumero = 5, OnkoVasenKatinen = true, Rako = 4, Runko = 3, Laippa = 6 };

            teraRajat[1] = new TeraRajat { Min = -210, Max = -18.5, Lepopaikka = -25.0, Vaisto = -140.0 };
            teraRajat[2] = new TeraRajat { Min = -210, Max = -18.5, Lepopaikka = -25.0, Vaisto = -140.0 };
            teraRajat[3] = new TeraRajat { Min = -18.5, Max = 340, Lepopaikka = 340.0, Vaisto = 140.0 };
            teraRajat[4] = new TeraRajat { Min = -18.5, Max = 340, Lepopaikka = 340.0, Vaisto = 140.0 };
            teraRajat[5] = new TeraRajat { Min = 21.7, Max = 146, Lepopaikka = 25.0, Vaisto = 140.0 };
            teraRajat[6] = new TeraRajat { Min = 21.7, Max = 146, Lepopaikka = 25.0, Vaisto = 140.0 };

            // ── Irrotetaan eventit ──
            KappaleCombo.SelectionChanged -= Kappale_Changed;
            YhdistettyCombo.SelectionChanged -= YhdistettyKappale_Changed;
            VasenSahaCheck.Checked -= SahaValinta_Changed;
            VasenSahaCheck.Unchecked -= SahaValinta_Changed;
            OikeaSahaCheck.Checked -= SahaValinta_Changed;
            OikeaSahaCheck.Unchecked -= SahaValinta_Changed;

            KuivausTextBox.Text = "0";
            KappaleCombo.SelectedIndex = 2;
            YhdistettyCombo.SelectedIndex = 0;

            // ── Palautetaan eventit ──
            KappaleCombo.SelectionChanged += Kappale_Changed;
            YhdistettyCombo.SelectionChanged += YhdistettyKappale_Changed;
            VasenSahaCheck.Checked += SahaValinta_Changed;
            VasenSahaCheck.Unchecked += SahaValinta_Changed;
            OikeaSahaCheck.Checked += SahaValinta_Changed;
            OikeaSahaCheck.Unchecked += SahaValinta_Changed;

            UpdateKappaleInfo();
            PaivitaNakymat();
            PiirraVisual();
            SetStatus("Valmis", Colors.LightGray);
        }

        private bool OnYhdistettyTila()
        {
            bool vasenOn = VasenSahaCheck?.IsChecked == true;
            bool oikeaOn = OikeaSahaCheck?.IsChecked == true;
            return vasenOn && oikeaOn;
        }

        private void PaivitaNakymat()
        {
            // LuoParametriKontrollit hoitaa näkyvyydet
            LuoParametriKontrollit();
            PiirraVisual();
        }
        // ── Tallennus ────────────────────────────────────────────────────────

        private void TallennaTallennus()
        {
            try
            {
                var data = new TallennusData
                {
                    TeraParametrit = teraParametrit.ToDictionary(kvp => kvp.Key, kvp => new TeraParametritDto { OnkoVasenKatinen = kvp.Value.OnkoVasenKatinen, Rako = kvp.Value.Rako, Runko = kvp.Value.Runko, Laippa = kvp.Value.Laippa }),
                    PaksuudetVasen = paksuusTextBoxesVasen.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Text),
                    PaksuudetOikea = paksuusTextBoxesOikea.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Text),
                    PaksuudetYhdistetty = paksuusTextBoxesYhdistetty.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Text),
                    KappaleCount = GetSelectedPieceCount(),
                    YhdistettyCount = GetSelectedYhdistettyCount(),
                    Kuivaus = KuivausTextBox?.Text ?? "0",
                    VasenOn = VasenSahaCheck?.IsChecked == true,
                    OikeaOn = OikeaSahaCheck?.IsChecked == true,
                    TurvaEtaisyys = turvaEtaisyys,
                    TeraRajat = teraRajat.ToDictionary(kvp => kvp.Key, kvp => new TeraRajatDto { Min = kvp.Value.Min, Max = kvp.Value.Max, Lepopaikka = kvp.Value.Lepopaikka, Vaisto = kvp.Value.Vaisto })
                };
                IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(TallennusPolku)!);
                IO.File.WriteAllText(TallennusPolku, JsonSerializer.Serialize(data, _jsonOptions));
            }
            catch { }
        }

        private void LataaTallennus()
        {
            try
            {
                if (!IO.File.Exists(TallennusPolku)) return;
                var data = JsonSerializer.Deserialize<TallennusData>(IO.File.ReadAllText(TallennusPolku));
                if (data == null) return;

                foreach (var kvp in data.TeraParametrit)
                {
                    if (!teraParametrit.ContainsKey(kvp.Key)) continue;
                    teraParametrit[kvp.Key].OnkoVasenKatinen = kvp.Value.OnkoVasenKatinen;
                    teraParametrit[kvp.Key].Rako = kvp.Value.Rako;
                    teraParametrit[kvp.Key].Runko = kvp.Value.Runko;
                    teraParametrit[kvp.Key].Laippa = kvp.Value.Laippa;
                }
                foreach (var kvp in data.TeraRajat)
                {
                    if (!teraRajat.ContainsKey(kvp.Key)) continue;
                    teraRajat[kvp.Key].Min = kvp.Value.Min;
                    teraRajat[kvp.Key].Max = kvp.Value.Max;
                    teraRajat[kvp.Key].Lepopaikka = kvp.Value.Lepopaikka;
                    teraRajat[kvp.Key].Vaisto = kvp.Value.Vaisto;
                }

                turvaEtaisyys = data.TurvaEtaisyys > 0 ? data.TurvaEtaisyys : 15.0;

                // ── Irrotetaan eventit latauksen ajaksi ──
                KappaleCombo.SelectionChanged -= Kappale_Changed;
                YhdistettyCombo.SelectionChanged -= YhdistettyKappale_Changed;
                VasenSahaCheck.Checked -= SahaValinta_Changed;
                VasenSahaCheck.Unchecked -= SahaValinta_Changed;
                OikeaSahaCheck.Checked -= SahaValinta_Changed;
                OikeaSahaCheck.Unchecked -= SahaValinta_Changed;

                int idx = data.KappaleCount switch { 2 => 0, 3 => 1, _ => 2 };
                if (KappaleCombo != null) KappaleCombo.SelectedIndex = idx;
                int yIdx = data.YhdistettyCount switch { 3 => 0, 4 => 1, 5 => 2, 6 => 3, _ => 4 };
                if (YhdistettyCombo != null) YhdistettyCombo.SelectedIndex = yIdx;
                if (KuivausTextBox != null) KuivausTextBox.Text = data.Kuivaus;
                if (VasenSahaCheck != null) VasenSahaCheck.IsChecked = data.VasenOn;
                if (OikeaSahaCheck != null) OikeaSahaCheck.IsChecked = data.OikeaOn;

                // ── Palautetaan eventit ──
                KappaleCombo.SelectionChanged += Kappale_Changed;
                YhdistettyCombo.SelectionChanged += YhdistettyKappale_Changed;
                VasenSahaCheck.Checked += SahaValinta_Changed;
                VasenSahaCheck.Unchecked += SahaValinta_Changed;
                OikeaSahaCheck.Checked += SahaValinta_Changed;
                OikeaSahaCheck.Unchecked += SahaValinta_Changed;

                PaivitaNakymat();
                LuoParametriKontrollit();

                Dispatcher.InvokeAsync(() =>
                {
                    foreach (var kvp in data.PaksuudetVasen)
                        if (paksuusTextBoxesVasen.TryGetValue(kvp.Key, out var tb)) tb.Text = kvp.Value;
                    foreach (var kvp in data.PaksuudetOikea)
                        if (paksuusTextBoxesOikea.TryGetValue(kvp.Key, out var tb)) tb.Text = kvp.Value;
                    foreach (var kvp in data.PaksuudetYhdistetty)
                        if (paksuusTextBoxesYhdistetty.TryGetValue(kvp.Key, out var tb)) tb.Text = kvp.Value;
                }, System.Windows.Threading.DispatcherPriority.Loaded);

                SetStatus("✓  Asetukset ladattu", Colors.LightGreen);
            }
            catch { }
        }

        // ── Parametrikontrollit ──────────────────────────────────────────────

        private void LuoParametriKontrollit()
        {
            if (PaksuusPanel == null) return;
            while (PaksuusPanel.Children.Count > 4)
                PaksuusPanel.Children.RemoveAt(PaksuusPanel.Children.Count - 1);
            paksuusTextBoxesVasen.Clear();
            paksuusTextBoxesOikea.Clear();
            paksuusTextBoxesYhdistetty.Clear();

            bool vasenOn = VasenSahaCheck?.IsChecked == true;
            bool oikeaOn = OikeaSahaCheck?.IsChecked == true;

            // Piilota kappaleiden määrä jos sahat pois
            if (ErillinenPanel != null)
                ErillinenPanel.Visibility = (!vasenOn && !oikeaOn) ? Visibility.Collapsed
                    : OnYhdistettyTila() ? Visibility.Collapsed : Visibility.Visible;
            if (YhdistettyPanel != null)
                YhdistettyPanel.Visibility = OnYhdistettyTila() ? Visibility.Visible : Visibility.Collapsed;

            if (!vasenOn && !oikeaOn) return;

            if (OnYhdistettyTila()) LuoYhdistettyKontrollit();
            else LuoErillisetKontrollit();
        }

        private void LuoErillisetKontrollit()
        {
            int count = GetSelectedPieceCount();
            bool vasenOn = VasenSahaCheck?.IsChecked == true;
            bool oikeaOn = OikeaSahaCheck?.IsChecked == true;
            var root = new StackPanel { Orientation = Orientation.Vertical };

            if (vasenOn)
            {
                root.Children.Add(new TextBlock
                {
                    Text = $"🔴 Vasen: {string.Join(", ", Enumerable.Range(1, count).Select(i => $"K{i}"))}",
                    FontSize = 11,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(255, 107, 107)),
                    Margin = new Thickness(0, 0, 0, 4)
                });
                var vasenStack = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
                for (int i = 1; i <= count; i++)
                {
                    var card = LuoPaksuusKorttiKapea($"K{i}", i);
                    vasenStack.Children.Add(card);
                    var tb = EtsiKortinTextBox(card);
                    if (tb != null) paksuusTextBoxesVasen[i] = tb;
                }
                root.Children.Add(vasenStack);
            }

            if (oikeaOn)
            {
                root.Children.Add(new TextBlock
                {
                    Text = $"🟢 Oikea: {string.Join(", ", Enumerable.Range(1, count).Select(i => $"K{i + 4}"))}",
                    FontSize = 11,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(107, 203, 119)),
                    Margin = new Thickness(0, 0, 0, 4)
                });
                var oikeaStack = new StackPanel();
                for (int i = 5; i < 5 + count; i++)
                {
                    var card = LuoPaksuusKorttiKapea($"K{i}", i);
                    oikeaStack.Children.Add(card);
                    var tb = EtsiKortinTextBox(card);
                    if (tb != null) paksuusTextBoxesOikea[i] = tb;
                }
                root.Children.Add(oikeaStack);
            }

            PaksuusPanel.Children.Add(root);
        }
        private void LuoYhdistettyKontrollit()
        {
            int count = GetSelectedYhdistettyCount();
            var root = new StackPanel { Orientation = Orientation.Vertical };

            for (int i = 1; i <= count; i++)
            {
                var card = LuoPaksuusKorttiKapea($"K{i}", i);
                root.Children.Add(card);
                var tb = EtsiKortinTextBox(card);
                if (tb != null) paksuusTextBoxesYhdistetty[i] = tb;
            }

            var turvaPanel = new Border { Margin = new Thickness(0, 8, 0, 0), Background = new SolidColorBrush(Color.FromRgb(40, 40, 40)), CornerRadius = new CornerRadius(6), Padding = new Thickness(8, 6, 8, 6) };
            var turvaStack = new StackPanel();
            turvaStack.Children.Add(new TextBlock { Text = "T1-T2 turvaetäisyys:", FontSize = 11, Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180)) });
            var turvaGrid = new Grid();
            turvaGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            turvaGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
            var turvaTb = new TextBox { Height = 28, Text = turvaEtaisyys.ToString("F1", CultureInfo.InvariantCulture), Background = new SolidColorBrush(Color.FromRgb(50, 50, 50)), Foreground = Brushes.White, BorderBrush = new SolidColorBrush(Color.FromRgb(100, 100, 100)), Padding = new Thickness(6, 0, 6, 0), VerticalContentAlignment = VerticalAlignment.Center };
            turvaTb.TextChanged += (s, e) => { if (double.TryParse(turvaTb.Text.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture, out double v) && v > 0) { turvaEtaisyys = v; PiirraVisual(); } };
            var turvaMm = new TextBlock { Text = "mm", FontSize = 10, Foreground = new SolidColorBrush(Color.FromRgb(140, 140, 140)), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 0, 0) };
            Grid.SetColumn(turvaTb, 0); Grid.SetColumn(turvaMm, 1);
            turvaGrid.Children.Add(turvaTb); turvaGrid.Children.Add(turvaMm);
            turvaStack.Children.Add(turvaGrid);
            turvaPanel.Child = turvaStack;
            root.Children.Add(turvaPanel);
            PaksuusPanel.Children.Add(root);
        }

        private Border LuoPaksuusKorttiKapea(string otsikko, int tagId)
        {
            var border = new Border { Margin = new Thickness(0, 0, 0, 4), Background = new SolidColorBrush(Color.FromRgb(50, 50, 50)), CornerRadius = new CornerRadius(6), Padding = new Thickness(6, 4, 6, 4) };
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(32) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(24) });
            var lbl = new TextBlock { Text = otsikko, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(255, 217, 61)), VerticalAlignment = VerticalAlignment.Center, FontSize = 12 };
            var textBox = new TextBox { Height = 28, Padding = new Thickness(6, 0, 6, 0), Text = "30", Tag = tagId, FontSize = 12, VerticalContentAlignment = VerticalAlignment.Center, Background = new SolidColorBrush(Color.FromRgb(40, 40, 40)), Foreground = Brushes.White, BorderBrush = new SolidColorBrush(Color.FromRgb(100, 100, 100)) };
            var mm = new TextBlock { Text = "mm", FontSize = 10, Foreground = new SolidColorBrush(Color.FromRgb(140, 140, 140)), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
            textBox.TextChanged += (s, e) =>
            {
                bool valid = double.TryParse(textBox.Text.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture, out double v) && v > 0;
                textBox.BorderBrush = valid ? new SolidColorBrush(Color.FromRgb(100, 100, 100)) : new SolidColorBrush(Color.FromRgb(220, 83, 83));
                if (!valid) SetStatus("⚠  Paksuuden täytyy olla positiivinen luku", Colors.Orange);
                else SetStatus("Valmis", Colors.LightGray);
            };
            textBox.TextChanged += PaksuusTextBox_Changed;
            Grid.SetColumn(lbl, 0); Grid.SetColumn(textBox, 1); Grid.SetColumn(mm, 2);
            grid.Children.Add(lbl); grid.Children.Add(textBox); grid.Children.Add(mm);
            border.Child = grid;
            return border;
        }

        private static TextBox? EtsiKortinTextBox(Border border)
        {
            if (border.Child is not Grid g) return null;
            foreach (var child in g.Children)
                if (child is TextBox tb) return tb;
            return null;
        }

        // ── Event-handlerit ──────────────────────────────────────────────────

        private int GetHalkaisuTeraOikea() => HalkaisuOikeaCombo?.SelectedIndex == 1 ? 2 : 4;
        private int GetHalkaisuTeraVasen() => HalkaisuVasenCombo?.SelectedIndex == 1 ? 1 : 3;
        private void PaksuusTextBox_Changed(object sender, TextChangedEventArgs e) => PiirraVisual();
        private void SahaValinta_Changed(object sender, RoutedEventArgs e) { PaivitaNakymat(); LuoParametriKontrollit(); PiirraVisual(); }
        private void Halkaisu_Changed(object sender, SelectionChangedEventArgs e) => PiirraVisual();
        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e) => PiirraVisual();

        private void Kuivaus_Changed(object sender, TextChangedEventArgs e)
        {
            string text = KuivausTextBox?.Text ?? "0";
            if (!double.TryParse(text.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture, out double val) || val < 0)
                SetStatus("⚠  Kuivausprosentin täytyy olla positiivinen numero", Colors.Orange);
            else SetStatus("Valmis", Colors.LightGray);
            PiirraVisual();
        }

        private void Kappale_Changed(object sender, SelectionChangedEventArgs e)
        {
            UpdateKappaleInfo();
            if (HalkaisuPanel != null)
                HalkaisuPanel.Visibility = GetSelectedPieceCount() == 2 ? Visibility.Visible : Visibility.Collapsed;
            LuoParametriKontrollit();
            PiirraVisual();
        }

        private void YhdistettyKappale_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (YhdistettyInfo != null) YhdistettyInfo.Text = $"{GetSelectedYhdistettyCount()} kpl";
            LuoParametriKontrollit();
            PiirraVisual();
        }

        private void Laheta_Click(object sender, RoutedEventArgs e)
        {
            if (StatusTextBlock?.Text.StartsWith("⚠") == true)
            {
                MessageBox.Show("Rajoja on ylitetty — arvoja ei lähetetä logiikkaan!\n\n" + StatusTextBlock.Text, "Lähetys estetty", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            SetStatus("✓  Lähetetty logiikkaan", Colors.LightGreen);
        }

        private void AvaaTerapAsetukset_Click(object sender, RoutedEventArgs e)
        {
            if (teraParametrit == null || teraParametrit.Count == 0) { MessageBox.Show("Teräparametrit ei ole alustettu!", "Virhe", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            try
            {
                var teraWindow = new TeraAsetuksetWindow(teraParametrit) { Owner = this };
                teraWindow.OnParametritChanged += paivitetyt =>
                {
                    foreach (var kvp in paivitetyt)
                    {
                        if (!teraParametrit.ContainsKey(kvp.Key)) continue;
                        teraParametrit[kvp.Key].OnkoVasenKatinen = kvp.Value.OnkoVasenKatinen;
                        teraParametrit[kvp.Key].Laippa = kvp.Value.Laippa;
                        teraParametrit[kvp.Key].Runko = kvp.Value.Runko;
                        teraParametrit[kvp.Key].Rako = kvp.Value.Rako;
                    }
                    PiirraVisual();
                    SetStatus("✓  Teräasetukset päivitetty", Colors.LightGreen);
                };
                teraWindow.ShowDialog();
                PiirraVisual();
            }
            catch (Exception ex) { MessageBox.Show($"Teräasetusikkuna kaatui:\n\n{ex.Message}\n\n{ex.StackTrace}", "Virhe", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void AvaaRajatAsetukset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var rajatWindow = new RajatAsetuksetWindow(teraRajat) { Owner = this };
                rajatWindow.OnRajatChanged += paivitetyt =>
                {
                    foreach (var kvp in paivitetyt)
                    {
                        if (!teraRajat.ContainsKey(kvp.Key)) continue;
                        teraRajat[kvp.Key].Min = kvp.Value.Min;
                        teraRajat[kvp.Key].Max = kvp.Value.Max;
                        teraRajat[kvp.Key].Lepopaikka = kvp.Value.Lepopaikka;
                        teraRajat[kvp.Key].Vaisto = kvp.Value.Vaisto;
                    }
                    PiirraVisual();
                    SetStatus("✓  Rajasetukset päivitetty", Colors.LightGreen);
                };
                rajatWindow.ShowDialog();
            }
            catch (Exception ex) { MessageBox.Show($"Rajoitusikkuna kaatui:\n\n{ex.Message}\n\n{ex.StackTrace}", "Virhe", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        // ── Piirrä ────────────────────────────────────────────────────────────

        private void PiirraVisual()
        {
            if (_isPiirraVisualRunning) return;
            _isPiirraVisualRunning = true;
            try
            {
                if (KuivausTextBox == null) return;
                if (!double.TryParse(KuivausTextBox.Text.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture, out double kuivausProsentti) || kuivausProsentti < 0)
                    kuivausProsentti = 0;
                double kuivausKerroin = 1.0 + (kuivausProsentti / 100.0);
                if (KuivausValue != null) KuivausValue.Text = $"{kuivausProsentti:F0} %";

                bool vasenOn = VasenSahaCheck?.IsChecked == true;
                bool oikeaOn = OikeaSahaCheck?.IsChecked == true;
                bool yhdistetty = vasenOn && oikeaOn;

                double plc_T4 = teraRajat.TryGetValue(4, out var r4) ? r4.Lepopaikka : 340.0;
                double plc_T2 = teraRajat.TryGetValue(2, out var r2) ? r2.Lepopaikka : -25.0;
                double plc_T6 = teraRajat.TryGetValue(6, out var r6) ? r6.Lepopaikka : 25.0;
                double plc_T3 = teraRajat.TryGetValue(3, out var r3) ? r3.Lepopaikka : 340.0;
                double plc_T1 = teraRajat.TryGetValue(1, out var r1) ? r1.Lepopaikka : -25.0;
                double plc_T5 = teraRajat.TryGetValue(5, out var r5) ? r5.Lepopaikka : 25.0;

                var paksuudetVasen = new List<double>();
                var paksuudetOikea = new List<double>();
                var paksuudetYhd = new List<double>();

                if (yhdistetty)
                {
                    paksuudetYhd = GetThicknessValuesYhdistetty().Select(p => p * kuivausKerroin).ToList();
                    if (paksuudetYhd.Count > 0)
                        LaskeYhdistetty(paksuudetYhd, ref plc_T1, ref plc_T2, ref plc_T3, ref plc_T4, ref plc_T5, ref plc_T6);
                }
                else
                {
                    paksuudetVasen = GetThicknessValuesVasen().Select(p => p * kuivausKerroin).ToList();
                    paksuudetOikea = GetThicknessValuesOikea().Select(p => p * kuivausKerroin).ToList();
                    if (oikeaOn && paksuudetOikea.Count > 0)
                        LaskeOikeaPuoli(paksuudetOikea, GetHalkaisuTeraOikea(), ref plc_T4, ref plc_T2, ref plc_T6);
                    if (vasenOn && paksuudetVasen.Count > 0)
                        LaskeVasenPuoli(paksuudetVasen, GetHalkaisuTeraVasen(), ref plc_T3, ref plc_T1, ref plc_T5);
                }

                var rajaVaroitukset = new List<string>();
                void TarkistaRaja(string nimi, double arvo, int teraNumero)
                {
                    if (!teraRajat.TryGetValue(teraNumero, out var raja)) return;
                    if (arvo < raja.Min || arvo > raja.Max)
                        rajaVaroitukset.Add($"{nimi}: {arvo:F1} (raja {raja.Min:F1}…{raja.Max:F1})");
                }
                TarkistaRaja("T4", plc_T4, 4); TarkistaRaja("T2", plc_T2, 2); TarkistaRaja("T6", plc_T6, 6);
                TarkistaRaja("T3", plc_T3, 3); TarkistaRaja("T1", plc_T1, 1); TarkistaRaja("T5", plc_T5, 5);

                if (yhdistetty)
                {
                    double t1Abs = plc_T4 + plc_T1;
                    double t2Abs = plc_T4 + plc_T2;
                    double vali = t2Abs - t1Abs;
                    if (vali < turvaEtaisyys)
                        rajaVaroitukset.Add($"T1-T2 väli liian pieni: {vali:F1} mm (min {turvaEtaisyys:F1} mm)");
                }

                if (rajaVaroitukset.Count > 0) SetStatus($"⚠  Raja ylitetty: {string.Join("  |  ", rajaVaroitukset)}", Colors.OrangeRed);
                else SetStatus("Valmis", Colors.LightGray);

                if (Tera4_Value != null) Tera4_Value.Text = $"T4: {plc_T4:F1}";
                if (Tera2_Value != null) Tera2_Value.Text = $"T2: {plc_T2:F1}";
                if (Tera6_Value != null) Tera6_Value.Text = $"T6: {plc_T6:F1}";
                if (Tera3_Value != null) Tera3_Value.Text = $"T3: {plc_T3:F1}";
                if (Tera1_Value != null) Tera1_Value.Text = $"T1: {plc_T1:F1}";
                if (Tera5_Value != null) Tera5_Value.Text = $"T5: {plc_T5:F1}";

                if (YhteisCanvas != null)
                {
                    if (yhdistetty)
                        PiirraYhteisCanvasYhdistetty(YhteisCanvas, paksuudetYhd, plc_T1, plc_T2, plc_T3, plc_T4, plc_T5, plc_T6);
                    else
                        PiirraYhteisCanvas(YhteisCanvas, paksuudetVasen, paksuudetOikea, vasenOn, oikeaOn, plc_T3, plc_T1, plc_T5, plc_T4, plc_T2, plc_T6);
                }
            }
            finally { _isPiirraVisualRunning = false; }
        }

        // ── Yhdistetty laskenta ──────────────────────────────────────────────

        private void LaskeYhdistetty(List<double> paksuudet,
            ref double plc_T1, ref double plc_T2, ref double plc_T3,
            ref double plc_T4, ref double plc_T5, ref double plc_T6)
        {
            int n = paksuudet.Count;
            double RakoT(int num) => teraParametrit.TryGetValue(num, out var t) ? t.Rako : 4.0;
            double VaistoT(int num) => teraRajat.TryGetValue(num, out var r) ? r.Vaisto : (num <= 2 ? -140.0 : 140.0);
            double OffsetT(int num)
            {
                if (!teraParametrit.TryGetValue(num, out var t)) return 1.5;
                return t.OnkoVasenKatinen ? t.Laippa - t.Runko / 2.0 : t.Runko / 2.0;
            }
            double r1 = RakoT(1), r2 = RakoT(2), r3 = RakoT(3);
            double r4 = RakoT(4), r5 = RakoT(5), r6 = RakoT(6);
            double kl = n switch
            {
                3 => (paksuudet[0] + r1 + paksuudet[1] + r2 + paksuudet[2]) / 2.0,
                4 => (paksuudet[0] + r1 + paksuudet[1] + r2 + paksuudet[2] + r4 + paksuudet[3]) / 2.0,
                5 => (paksuudet[0] + r3 + paksuudet[1] + r1 + paksuudet[2] + r2 + paksuudet[3] + r4 + paksuudet[4]) / 2.0,
                6 => (paksuudet[0] + r3 + paksuudet[1] + r1 + paksuudet[2] + r2 + paksuudet[3] + r4 + paksuudet[4] + r6 + paksuudet[5]) / 2.0,
                7 => (paksuudet[0] + r5 + paksuudet[1] + r3 + paksuudet[2] + r1 + paksuudet[3] + r2 + paksuudet[4] + r4 + paksuudet[5] + r6 + paksuudet[6]) / 2.0,
                _ => 0
            };
            plc_T4 = n switch
            {
                3 => VaistoT(4),
                4 => paksuudet[0] + r1 + paksuudet[1] + r2 + paksuudet[2] + r4 / 2.0 + OffsetT(4) - kl,
                5 => paksuudet[0] + r3 + paksuudet[1] + r1 + paksuudet[2] + r2 + paksuudet[3] + r4 / 2.0 + OffsetT(4) - kl,
                6 => paksuudet[0] + r3 + paksuudet[1] + r1 + paksuudet[2] + r2 + paksuudet[3] + r4 / 2.0 + OffsetT(4) - kl,
                7 => paksuudet[0] + r5 + paksuudet[1] + r3 + paksuudet[2] + r1 + paksuudet[3] + r2 + paksuudet[4] + r4 / 2.0 + OffsetT(4) - kl,
                _ => VaistoT(4)
            };
            plc_T2 = n switch
            {
                3 => -(paksuudet[2] + r2 / 2.0 + OffsetT(2) - kl + kl - plc_T4 + paksuudet[2]),
                4 => -(paksuudet[2] + r2 / 2.0 + OffsetT(2) + OffsetT(4) - r4 / 2.0),
                5 => -(paksuudet[3] + r2 / 2.0 + OffsetT(2) + OffsetT(4) - r4 / 2.0),
                6 => -(paksuudet[3] + r2 / 2.0 + OffsetT(2) + OffsetT(4) - r4 / 2.0),
                7 => -(paksuudet[4] + r2 / 2.0 + OffsetT(2) + OffsetT(4) - r4 / 2.0),
                _ => VaistoT(2)
            };
            if (n == 3)
            {
                double t2Abs = paksuudet[0] + r1 + paksuudet[1] + r2 / 2.0 + OffsetT(2) - kl;
                plc_T2 = t2Abs - plc_T4;
            }
            plc_T1 = n switch
            {
                3 => VaistoT(1) - plc_T4,
                4 => -(paksuudet[2] + r2 + paksuudet[1] + r1 / 2.0 + OffsetT(1) + OffsetT(4) - r4 / 2.0),
                5 => -(paksuudet[3] + r2 + paksuudet[2] + r1 / 2.0 + OffsetT(1) + OffsetT(4) - r4 / 2.0),
                6 => -(paksuudet[3] + r2 + paksuudet[2] + r1 / 2.0 + OffsetT(1) + OffsetT(4) - r4 / 2.0),
                7 => -(paksuudet[4] + r2 + paksuudet[3] + r1 / 2.0 + OffsetT(1) + OffsetT(4) - r4 / 2.0),
                _ => VaistoT(1)
            };
            if (n == 3)
            {
                double t1Abs = paksuudet[0] + r1 / 2.0 + OffsetT(1) - kl;
                plc_T1 = t1Abs - plc_T4;
            }
            plc_T3 = n switch
            {
                3 => VaistoT(3) - plc_T4,
                4 => VaistoT(3) - plc_T4,
                5 => -(paksuudet[3] + r2 + paksuudet[2] + r1 + paksuudet[1] + r3 / 2.0 + OffsetT(3) + OffsetT(4) - r4 / 2.0),
                6 => -(paksuudet[3] + r2 + paksuudet[2] + r1 + paksuudet[1] + r3 / 2.0 + OffsetT(3) + OffsetT(4) - r4 / 2.0),
                7 => -(paksuudet[4] + r2 + paksuudet[3] + r1 + paksuudet[2] + r3 / 2.0 + OffsetT(3) + OffsetT(4) - r4 / 2.0),
                _ => VaistoT(3)
            };
            plc_T6 = n switch
            {
                3 => VaistoT(6) - plc_T4,
                4 => VaistoT(6) - plc_T4,
                5 => VaistoT(6) - plc_T4,
                6 => paksuudet[4] + r6 / 2.0 + OffsetT(6) - OffsetT(4) + r4 / 2.0,
                7 => paksuudet[5] + r6 / 2.0 + OffsetT(6) - OffsetT(4) + r4 / 2.0,
                _ => VaistoT(6)
            };
            plc_T5 = n switch
            {
                7 => -(paksuudet[4] + r2 + paksuudet[3] + r1 + paksuudet[2] + r3 + paksuudet[1] + r5 / 2.0 + OffsetT(5) + OffsetT(4) - r4 / 2.0),
                _ => VaistoT(5) - plc_T4
            };
        }

        // ── Laskenta erillinen ───────────────────────────────────────────────

        private static double LaskeKeskilinja2(List<double> paksuudet, double rako1) =>
            (paksuudet.Sum() + rako1) / 2.0;

        private static double LaskeKeskilinja3(List<double> paksuudet, double rako1, double rako2) =>
            (paksuudet.Sum() + rako1 + rako2) / 2.0;

        private static double LaskeKeskilinja4(List<double> paksuudet, double rako1, double rako2, double rako3) =>
            (paksuudet.Sum() + rako1 + rako2 + rako3) / 2.0;

        private static double LaskeOffset(bool vasenKatinen, double laippa, double runko) =>
            vasenKatinen ? laippa - runko / 2.0 : runko / 2.0;

        private static double LaskeSivuTeraPlc(
            double kappale, double rakoSivu, double rakoPaa,
            double offsetSivu, double offsetPaa) =>
            -((rakoSivu / 2.0 + kappale + rakoPaa / 2.0) - offsetSivu + offsetPaa);

        private static double LaskeUlkoTeraPlc(
            double kappale, double rakoUlko, double rakoPaa,
            double offsetUlko, double offsetPaa) =>
            (rakoUlko / 2.0 + kappale + rakoPaa / 2.0) - offsetPaa + offsetUlko;

        private void LaskeOikeaPuoli(List<double> paksuudet, int halkaisuTera,
            ref double plc_T4, ref double plc_T2, ref double plc_T6)
        {
            if (paksuudet.Count == 0) return;

            double vaistoSisa = teraRajat.TryGetValue(2, out var rv2) ? rv2.Vaisto : Vaisto_Sisaterä;
            double vaistoUlko = teraRajat.TryGetValue(6, out var rv6) ? rv6.Vaisto : Vaisto_Ulkoterä;

            if (paksuudet.Count == 2)
            {
                if (halkaisuTera == 4)
                {
                    if (teraParametrit.TryGetValue(4, out var tera4))
                    {
                        double rako1 = tera4.Rako;
                        double kl = LaskeKeskilinja2(paksuudet, rako1);
                        double off4 = LaskeOffset(tera4.OnkoVasenKatinen, tera4.Laippa, tera4.Runko);
                        plc_T4 = paksuudet[0] + rako1 / 2.0 + off4 - kl;
                        plc_T2 = vaistoSisa;
                        plc_T6 = vaistoUlko;
                    }
                }
                else if (halkaisuTera == 2)
                {
                    if (teraParametrit.TryGetValue(2, out var tera2))
                    {
                        double rako1 = tera2.Rako;
                        double kl = LaskeKeskilinja2(paksuudet, rako1);
                        double off2 = LaskeOffset(tera2.OnkoVasenKatinen, tera2.Laippa, tera2.Runko);
                        plc_T2 = -(paksuudet[0] + rako1 / 2.0 + off2 - kl);
                        plc_T4 = vaistoUlko;
                        plc_T6 = vaistoUlko;
                    }
                }
            }
            else if (paksuudet.Count == 3)
            {
                teraParametrit.TryGetValue(4, out var tera4);
                teraParametrit.TryGetValue(2, out var tera2);

                double rako1 = tera2?.Rako ?? 4.0;
                double rako2 = tera4?.Rako ?? 4.0;
                double kl = LaskeKeskilinja3(paksuudet, rako1, rako2);

                if (tera4 != null)
                {
                    double off4 = LaskeOffset(tera4.OnkoVasenKatinen, tera4.Laippa, tera4.Runko);
                    plc_T4 = paksuudet[0] + rako1 + paksuudet[1] + rako2 / 2.0 + off4 - kl;
                }

                if (tera2 != null && tera4 != null)
                {
                    double off4 = LaskeOffset(tera4.OnkoVasenKatinen, tera4.Laippa, tera4.Runko);
                    double off2 = LaskeOffset(tera2.OnkoVasenKatinen, tera2.Laippa, tera2.Runko);
                    plc_T2 = LaskeSivuTeraPlc(paksuudet[1], rako1, rako2, off2, off4);
                }

                plc_T6 = vaistoUlko;
            }
            else if (paksuudet.Count == 4)
            {
                teraParametrit.TryGetValue(4, out var tera4);
                teraParametrit.TryGetValue(2, out var tera2);
                teraParametrit.TryGetValue(6, out var tera6);
                if (tera4 == null) return;

                double k1 = paksuudet[0], k2 = paksuudet[1], k3 = paksuudet[2];
                double rako1 = tera2?.Rako ?? 4.0;
                double rako2 = tera4.Rako;
                double rako3 = tera6?.Rako ?? 4.0;
                double kl = LaskeKeskilinja4(paksuudet, rako1, rako2, rako3);

                double off4 = LaskeOffset(tera4.OnkoVasenKatinen, tera4.Laippa, tera4.Runko);
                plc_T4 = k1 + rako1 + k2 + rako2 / 2.0 + off4 - kl;

                if (tera2 != null)
                {
                    double off2 = LaskeOffset(tera2.OnkoVasenKatinen, tera2.Laippa, tera2.Runko);
                    plc_T2 = LaskeSivuTeraPlc(k2, rako1, rako2, off2, off4);
                }

                if (tera6 != null)
                {
                    double off6 = LaskeOffset(tera6.OnkoVasenKatinen, tera6.Laippa, tera6.Runko);
                    plc_T6 = LaskeUlkoTeraPlc(k3, rako3, rako2, off6, off4);
                }
            }
        }

        private void LaskeVasenPuoli(List<double> paksuudet, int halkaisuTera,
            ref double plc_T3, ref double plc_T1, ref double plc_T5)
        {
            if (paksuudet.Count == 0) return;

            double vaistoSisa = teraRajat.TryGetValue(1, out var rv1) ? rv1.Vaisto : Vaisto_Sisaterä;
            double vaistoUlko = teraRajat.TryGetValue(5, out var rv5) ? rv5.Vaisto : Vaisto_Ulkoterä;

            if (paksuudet.Count == 2)
            {
                if (halkaisuTera == 3)
                {
                    if (teraParametrit.TryGetValue(3, out var tera3))
                    {
                        double rako1 = tera3.Rako;
                        double kl = LaskeKeskilinja2(paksuudet, rako1);
                        double off3 = LaskeOffset(tera3.OnkoVasenKatinen, tera3.Laippa, tera3.Runko);
                        plc_T3 = paksuudet[0] + rako1 / 2.0 + off3 - kl;
                        plc_T1 = vaistoSisa;
                        plc_T5 = vaistoUlko;
                    }
                }
                else if (halkaisuTera == 1)
                {
                    if (teraParametrit.TryGetValue(1, out var tera1))
                    {
                        double rako1 = tera1.Rako;
                        double kl = LaskeKeskilinja2(paksuudet, rako1);
                        double off1 = LaskeOffset(tera1.OnkoVasenKatinen, tera1.Laippa, tera1.Runko);
                        plc_T1 = -(paksuudet[0] + rako1 / 2.0 + off1 - kl);
                        plc_T3 = vaistoSisa;
                        plc_T5 = vaistoUlko;
                    }
                }
            }
            else if (paksuudet.Count == 3)
            {
                teraParametrit.TryGetValue(3, out var tera3);
                teraParametrit.TryGetValue(1, out var tera1);

                double rako1 = tera1?.Rako ?? 4.0;
                double rako2 = tera3?.Rako ?? 4.0;
                double kl = LaskeKeskilinja3(paksuudet, rako1, rako2);

                if (tera3 != null)
                {
                    double off3 = LaskeOffset(tera3.OnkoVasenKatinen, tera3.Laippa, tera3.Runko);
                    plc_T3 = paksuudet[0] + rako1 + paksuudet[1] + rako2 / 2.0 + off3 - kl;
                }

                if (tera1 != null && tera3 != null)
                {
                    double off3 = LaskeOffset(tera3.OnkoVasenKatinen, tera3.Laippa, tera3.Runko);
                    double off1 = LaskeOffset(tera1.OnkoVasenKatinen, tera1.Laippa, tera1.Runko);
                    plc_T1 = LaskeSivuTeraPlc(paksuudet[1], rako1, rako2, off1, off3);
                }

                plc_T5 = vaistoUlko;
            }
            else if (paksuudet.Count == 4)
            {
                teraParametrit.TryGetValue(3, out var tera3);
                teraParametrit.TryGetValue(1, out var tera1);
                teraParametrit.TryGetValue(5, out var tera5);
                if (tera3 == null) return;

                double k1 = paksuudet[0], k2 = paksuudet[1], k3 = paksuudet[2];
                double rako1 = tera1?.Rako ?? 4.0;
                double rako2 = tera3.Rako;
                double rako3 = tera5?.Rako ?? 4.0;
                double kl = LaskeKeskilinja4(paksuudet, rako1, rako2, rako3);

                double off3 = LaskeOffset(tera3.OnkoVasenKatinen, tera3.Laippa, tera3.Runko);
                plc_T3 = k1 + rako1 + k2 + rako2 / 2.0 + off3 - kl;

                if (tera1 != null)
                {
                    double off1 = LaskeOffset(tera1.OnkoVasenKatinen, tera1.Laippa, tera1.Runko);
                    plc_T1 = LaskeSivuTeraPlc(k2, rako1, rako2, off1, off3);
                }

                if (tera5 != null)
                {
                    double off5 = LaskeOffset(tera5.OnkoVasenKatinen, tera5.Laippa, tera5.Runko);
                    plc_T5 = LaskeUlkoTeraPlc(k3, rako3, rako2, off5, off3);
                }
            }
        }

        // ── Canvas ───────────────────────────────────────────────────────────

        private Brush TeraViivaBrush(double arvo, int teraNumero, Color normaali)
        {
            if (teraRajat.TryGetValue(teraNumero, out var raja))
                if (arvo < raja.Min || arvo > raja.Max)
                    return new SolidColorBrush(Colors.OrangeRed);
            return new SolidColorBrush(normaali);
        }

        private void PiirraYhteisCanvasYhdistetty(Canvas canvas,
            List<double> paksuudet,
            double plc_T1, double plc_T2, double plc_T3,
            double plc_T4, double plc_T5, double plc_T6)
        {
            canvas.Children.Clear();
            int n = paksuudet.Count;
            double canvasWidth = canvas.ActualWidth > 20 ? canvas.ActualWidth : 900;
            double canvasHeight = canvas.ActualHeight > 20 ? canvas.ActualHeight : 400;
            double centerY = canvasHeight / 2.0;
            double centerX = canvasWidth / 2.0;
            double pixelsPerMm = (canvasWidth / 2.0 - 20) / 350.0;
            double rectHeight = canvasHeight * 0.45;
            double rectY = centerY - rectHeight / 2.0;

            canvas.Children.Add(new Rectangle { Width = canvasWidth, Height = canvasHeight, Fill = new SolidColorBrush(Color.FromRgb(13, 13, 13)) });
            PiirraAsteikko(canvas, canvasWidth, canvasHeight, centerX, pixelsPerMm);
            canvas.Children.Add(new Line { X1 = centerX, Y1 = 20, X2 = centerX, Y2 = canvasHeight - 30, Stroke = new SolidColorBrush(Color.FromRgb(79, 195, 247)), StrokeThickness = 1.5, StrokeDashArray = new DoubleCollection { 4, 3 } });

            var teraJarjestys = n switch
            {
                3 => new[] { 1, 2 },
                4 => new[] { 1, 2, 4 },
                5 => new[] { 3, 1, 2, 4 },
                6 => new[] { 3, 1, 2, 4, 6 },
                7 => new[] { 5, 3, 1, 2, 4, 6 },
                _ => Array.Empty<int>()
            };

            double RakoT(int num) => teraParametrit.TryGetValue(num, out var t) ? t.Rako : 4.0;

            var pieceColors = new[]
            {
                Color.FromRgb(76, 175, 80),  Color.FromRgb(33, 150, 243),
                Color.FromRgb(233, 30, 99),  Color.FromRgb(255, 193, 7),
                Color.FromRgb(156, 39, 176), Color.FromRgb(0, 188, 212),
                Color.FromRgb(255, 87, 34)
            };

            double kokonaisLeveys = paksuudet.Sum() + teraJarjestys.Select(t => RakoT(t)).Sum();
            double curMm = -kokonaisLeveys / 2.0;

            for (int i = 0; i < n; i++)
            {
                double paksuus = paksuudet[i];
                double pieceW = paksuus * pixelsPerMm;
                double pieceX = centerX + curMm * pixelsPerMm;

                var piece = new Rectangle
                {
                    Width = pieceW,
                    Height = rectHeight,
                    Fill = new SolidColorBrush(Color.FromArgb(180, pieceColors[i % pieceColors.Length].R, pieceColors[i % pieceColors.Length].G, pieceColors[i % pieceColors.Length].B)),
                    Stroke = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                    StrokeThickness = 1,
                    RadiusX = 3,
                    RadiusY = 3
                };
                Canvas.SetLeft(piece, pieceX); Canvas.SetTop(piece, rectY);
                canvas.Children.Add(piece);

                var tl = new TextBlock { Text = $"K{i + 1}\n{paksuus:F1}", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = Brushes.White, TextAlignment = TextAlignment.Center, Width = Math.Max(20, pieceW) };
                Canvas.SetLeft(tl, pieceX + pieceW / 2.0 - tl.Width / 2.0); Canvas.SetTop(tl, centerY - 14);
                canvas.Children.Add(tl);

                curMm += paksuus;

                if (i < teraJarjestys.Length)
                {
                    double rako = RakoT(teraJarjestys[i]);
                    double gapX = centerX + curMm * pixelsPerMm;
                    double gapW = rako * pixelsPerMm;
                    var gap = new Rectangle { Width = gapW, Height = rectHeight, Fill = new SolidColorBrush(Color.FromArgb(80, 50, 50, 50)), Stroke = new SolidColorBrush(Color.FromRgb(80, 80, 80)), StrokeThickness = 1, StrokeDashArray = new DoubleCollection { 3, 2 } };
                    Canvas.SetLeft(gap, gapX); Canvas.SetTop(gap, rectY);
                    canvas.Children.Add(gap);
                    curMm += rako;
                }
            }

            Color normC = Color.FromRgb(255, 217, 61);
            double t4X = centerX + plc_T4 * pixelsPerMm;
            double t2X = t4X + plc_T2 * pixelsPerMm;
            double t1X = t4X + plc_T1 * pixelsPerMm;
            double t3X = t4X + plc_T3 * pixelsPerMm;
            double t6X = t4X + plc_T6 * pixelsPerMm;
            double t5X = t4X + plc_T5 * pixelsPerMm;

            if (n >= 4) PiirraTeraViiva(canvas, t4X, rectY, rectHeight, canvasWidth, TeraViivaBrush(plc_T4, 4, normC), $"T4\n{plc_T4:F1}", true);
            PiirraTeraViiva(canvas, t2X, rectY, rectHeight, canvasWidth, TeraViivaBrush(plc_T4 + plc_T2, 2, normC), $"T2\n{plc_T4 + plc_T2:F1}", false);
            PiirraTeraViiva(canvas, t1X, rectY, rectHeight, canvasWidth, TeraViivaBrush(plc_T4 + plc_T1, 1, normC), $"T1\n{plc_T4 + plc_T1:F1}", true);
            if (n >= 5) PiirraTeraViiva(canvas, t3X, rectY, rectHeight, canvasWidth, TeraViivaBrush(plc_T4 + plc_T3, 3, normC), $"T3\n{plc_T4 + plc_T3:F1}", false);
            if (n >= 6) PiirraTeraViiva(canvas, t6X, rectY, rectHeight, canvasWidth, TeraViivaBrush(plc_T4 + plc_T6, 6, normC), $"T6\n{plc_T4 + plc_T6:F1}", true);
            if (n >= 7) PiirraTeraViiva(canvas, t5X, rectY, rectHeight, canvasWidth, TeraViivaBrush(plc_T4 + plc_T5, 5, normC), $"T5\n{plc_T4 + plc_T5:F1}", false);

            double t1AbsX = t4X + plc_T1 * pixelsPerMm;
            double t2AbsX = t4X + plc_T2 * pixelsPerMm;
            double valiPx = t2AbsX - t1AbsX;
            double turvaVali_mm = Math.Abs(valiPx) / pixelsPerMm;

            var turvaRect = new Rectangle { Width = Math.Abs(valiPx), Height = 6, Fill = new SolidColorBrush(turvaVali_mm < turvaEtaisyys ? Color.FromArgb(180, 255, 50, 50) : Color.FromArgb(100, 50, 255, 50)) };
            Canvas.SetLeft(turvaRect, Math.Min(t1AbsX, t2AbsX)); Canvas.SetTop(turvaRect, rectY - 10);
            canvas.Children.Add(turvaRect);

            var turvaLbl = new TextBlock { Text = $"{turvaVali_mm:F1} mm", FontSize = 9, Foreground = new SolidColorBrush(turvaVali_mm < turvaEtaisyys ? Colors.OrangeRed : Colors.LightGreen) };
            Canvas.SetLeft(turvaLbl, Math.Min(t1AbsX, t2AbsX)); Canvas.SetTop(turvaLbl, rectY - 22);
            canvas.Children.Add(turvaLbl);

            var otsikko = new TextBlock { Text = $"🔀 Yhdistetty sahaus — {n} kpl", FontSize = 12, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(255, 217, 61)) };
            Canvas.SetLeft(otsikko, centerX - 100); Canvas.SetTop(otsikko, 6);
            canvas.Children.Add(otsikko);
        }

        private void PiirraYhteisCanvas(Canvas canvas,
            List<double> paksuudetVasen, List<double> paksuudetOikea,
            bool vasenOn, bool oikeaOn,
            double plc_T3, double plc_T1, double plc_T5,
            double plc_T4, double plc_T2, double plc_T6)
        {
            canvas.Children.Clear();
            double canvasWidth = canvas.ActualWidth > 20 ? canvas.ActualWidth : canvas.Width > 0 ? canvas.Width : 900;
            double canvasHeight = canvas.ActualHeight > 20 ? canvas.ActualHeight : canvas.Height > 0 ? canvas.Height : 400;
            double centerY = canvasHeight / 2.0;
            double centerX = canvasWidth / 2.0;
            double pixelsPerMm = (canvasWidth / 2.0 - 20) / 350.0;
            double rectHeight = canvasHeight * 0.45;
            double rectY = centerY - rectHeight / 2.0;

            canvas.Children.Add(new Rectangle { Width = canvasWidth, Height = canvasHeight, Fill = new SolidColorBrush(Color.FromRgb(13, 13, 13)) });
            PiirraAsteikko(canvas, canvasWidth, canvasHeight, centerX, pixelsPerMm);
            canvas.Children.Add(new Line { X1 = centerX, Y1 = 20, X2 = centerX, Y2 = canvasHeight - 30, Stroke = new SolidColorBrush(Color.FromRgb(79, 195, 247)), StrokeThickness = 1.5, StrokeDashArray = new DoubleCollection { 4, 3 } });

            var cLbl = new TextBlock { Text = "0", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(79, 195, 247)) };
            Canvas.SetLeft(cLbl, centerX + 3); Canvas.SetTop(cLbl, 4);
            canvas.Children.Add(cLbl);

            var pieceColors = new[] { Color.FromRgb(76, 175, 80), Color.FromRgb(33, 150, 243), Color.FromRgb(233, 30, 99), Color.FromRgb(255, 193, 7) };

            if (oikeaOn && paksuudetOikea.Count > 0)
            {
                var raotOikea = new[] { teraParametrit.TryGetValue(2, out var t2r) ? t2r.Rako : 4.0, teraParametrit.TryGetValue(4, out var t4r) ? t4r.Rako : 4.0, teraParametrit.TryGetValue(6, out var t6r) ? t6r.Rako : 4.0 };
                double totalMm = paksuudetOikea.Sum() + raotOikea.Take(paksuudetOikea.Count - 1).Sum();
                PiirraPuoliRaot(canvas, paksuudetOikea, raotOikea, -(totalMm / 2.0), centerX, rectY, rectHeight, centerY, pixelsPerMm, pieceColors, vasemmalle: false, border: Color.FromRgb(107, 203, 119), kappaleOffset: 5);
            }

            if (vasenOn && paksuudetVasen.Count > 0)
            {
                var raotVasen = new[] { teraParametrit.TryGetValue(1, out var t1r) ? t1r.Rako : 4.0, teraParametrit.TryGetValue(3, out var t3r) ? t3r.Rako : 4.0, teraParametrit.TryGetValue(5, out var t5r) ? t5r.Rako : 4.0 };
                double totalMm = paksuudetVasen.Sum() + raotVasen.Take(paksuudetVasen.Count - 1).Sum();
                PiirraPuoliRaot(canvas, paksuudetVasen, raotVasen, -(totalMm / 2.0), centerX, rectY, rectHeight, centerY, pixelsPerMm, pieceColors, vasemmalle: true, border: Color.FromRgb(255, 107, 107), kappaleOffset: 1);
            }

            {
                Color normO = oikeaOn ? Color.FromRgb(107, 203, 119) : Color.FromRgb(50, 90, 50);
                double t4X = centerX + plc_T4 * pixelsPerMm;
                double t2X = t4X + plc_T2 * pixelsPerMm;
                double t6X = t4X + plc_T6 * pixelsPerMm;
                PiirraTeraViiva(canvas, t4X, rectY, rectHeight, canvasWidth, TeraViivaBrush(plc_T4, 4, normO), $"T4\n{plc_T4:F1}", true);
                PiirraTeraViiva(canvas, t2X, rectY, rectHeight, canvasWidth, TeraViivaBrush(plc_T2, 2, normO), $"T2\n{plc_T2:F1}", false);
                PiirraTeraViiva(canvas, t6X, rectY, rectHeight, canvasWidth, TeraViivaBrush(plc_T6, 6, normO), $"T6\n{plc_T6:F1}", true);
                var ol = new TextBlock { Text = "Oikea saha ▶", Foreground = new SolidColorBrush(normO), FontSize = 11, FontWeight = FontWeights.Bold };
                Canvas.SetLeft(ol, canvasWidth - 140); Canvas.SetTop(ol, 6);
                canvas.Children.Add(ol);
            }

            {
                Color normV = vasenOn ? Color.FromRgb(255, 107, 107) : Color.FromRgb(90, 50, 50);
                double t3X = centerX - plc_T3 * pixelsPerMm;
                double t1X = t3X - plc_T1 * pixelsPerMm;
                double t5X = t3X - plc_T5 * pixelsPerMm;
                PiirraTeraViiva(canvas, t3X, rectY, rectHeight, canvasWidth, TeraViivaBrush(plc_T3, 3, normV), $"T3\n{plc_T3:F1}", true);
                PiirraTeraViiva(canvas, t1X, rectY, rectHeight, canvasWidth, TeraViivaBrush(plc_T1, 1, normV), $"T1\n{plc_T1:F1}", false);
                PiirraTeraViiva(canvas, t5X, rectY, rectHeight, canvasWidth, TeraViivaBrush(plc_T5, 5, normV), $"T5\n{plc_T5:F1}", true);
                var vl = new TextBlock { Text = "◀ Vasen saha", Foreground = new SolidColorBrush(normV), FontSize = 11, FontWeight = FontWeights.Bold };
                Canvas.SetLeft(vl, 6); Canvas.SetTop(vl, 6);
                canvas.Children.Add(vl);
            }
        }

        private static void PiirraPuoliRaot(
            Canvas canvas, List<double> paksuudet, double[] raot, double startMm,
            double centerX, double rectY, double rectHeight, double centerY,
            double pixelsPerMm, Color[] pieceColors, bool vasemmalle, Color border, int kappaleOffset)
        {
            int n = paksuudet.Count;
            double curMm = startMm;
            double kokonaisLeveys = paksuudet.Sum() + raot.Take(n - 1).Sum();

            for (int i = 0; i < n; i++)
            {
                double paksuus = paksuudet[i];
                double pieceW = paksuus * pixelsPerMm;
                double pieceX = vasemmalle ? centerX - curMm * pixelsPerMm - pieceW : centerX + curMm * pixelsPerMm;

                var piece = new Rectangle { Width = pieceW, Height = rectHeight, Fill = new SolidColorBrush(Color.FromArgb(180, pieceColors[i % pieceColors.Length].R, pieceColors[i % pieceColors.Length].G, pieceColors[i % pieceColors.Length].B)), Stroke = new SolidColorBrush(border), StrokeThickness = 1, RadiusX = 3, RadiusY = 3 };
                Canvas.SetLeft(piece, pieceX); Canvas.SetTop(piece, rectY);
                canvas.Children.Add(piece);

                var tl = new TextBlock { Text = $"{paksuus:F1}", FontSize = 11, FontWeight = FontWeights.Bold, Foreground = Brushes.White, TextAlignment = TextAlignment.Center, Width = Math.Max(20, pieceW) };
                Canvas.SetLeft(tl, pieceX + pieceW / 2.0 - tl.Width / 2.0); Canvas.SetTop(tl, centerY - 10);
                canvas.Children.Add(tl);

                var nl = new TextBlock { Text = $"K{kappaleOffset + i}", FontSize = 10, Foreground = new SolidColorBrush(Color.FromArgb(220, 255, 255, 255)) };
                Canvas.SetLeft(nl, pieceX + 4); Canvas.SetTop(nl, rectY + 4);
                canvas.Children.Add(nl);

                curMm += paksuus;

                if (i < n - 1)
                {
                    double rako = raot[i % raot.Length];
                    double gapX = vasemmalle ? centerX - curMm * pixelsPerMm - rako * pixelsPerMm : centerX + curMm * pixelsPerMm;
                    double gapW = rako * pixelsPerMm;

                    var gap = new Rectangle { Width = gapW, Height = rectHeight, Fill = new SolidColorBrush(Color.FromArgb(120, 30, 30, 30)), Stroke = new SolidColorBrush(Color.FromRgb(100, 100, 100)), StrokeThickness = 1, StrokeDashArray = new DoubleCollection { 3, 2 } };
                    Canvas.SetLeft(gap, gapX); Canvas.SetTop(gap, rectY);
                    canvas.Children.Add(gap);

                    if (gapW > 8)
                    {
                        var rakoLbl = new TextBlock { Text = $"{rako:F1}", FontSize = 8, Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180)), TextAlignment = TextAlignment.Center, Width = Math.Max(10, gapW) };
                        Canvas.SetLeft(rakoLbl, gapX + gapW / 2.0 - rakoLbl.Width / 2.0); Canvas.SetTop(rakoLbl, centerY + 4);
                        canvas.Children.Add(rakoLbl);
                    }

                    curMm += rako;
                }
            }

            double totalStartX = vasemmalle ? centerX - (startMm + kokonaisLeveys) * pixelsPerMm : centerX + startMm * pixelsPerMm;
            double totalW = kokonaisLeveys * pixelsPerMm;
            double arrowY = rectY + rectHeight + 10;

            canvas.Children.Add(new Line { X1 = totalStartX, Y1 = arrowY, X2 = totalStartX + totalW, Y2 = arrowY, Stroke = new SolidColorBrush(border), StrokeThickness = 1 });
            canvas.Children.Add(new Line { X1 = totalStartX, Y1 = arrowY - 4, X2 = totalStartX, Y2 = arrowY + 4, Stroke = new SolidColorBrush(border), StrokeThickness = 1 });
            canvas.Children.Add(new Line { X1 = totalStartX + totalW, Y1 = arrowY - 4, X2 = totalStartX + totalW, Y2 = arrowY + 4, Stroke = new SolidColorBrush(border), StrokeThickness = 1 });

            var kokoLbl = new TextBlock { Text = $"⟵ {kokonaisLeveys:F1} mm ⟶", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(border), TextAlignment = TextAlignment.Center, Width = Math.Max(60, totalW) };
            Canvas.SetLeft(kokoLbl, totalStartX + totalW / 2.0 - kokoLbl.Width / 2.0); Canvas.SetTop(kokoLbl, arrowY + 5);
            canvas.Children.Add(kokoLbl);
        }

        private static void PiirraAsteikko(Canvas canvas, double canvasWidth, double canvasHeight, double centerX, double pixelsPerMm)
        {
            canvas.Children.Add(new Line { X1 = 10, Y1 = canvasHeight - 30, X2 = canvasWidth - 10, Y2 = canvasHeight - 30, Stroke = new SolidColorBrush(Color.FromRgb(80, 80, 80)), StrokeThickness = 1 });

            for (int mm = 0; mm <= 350; mm += 10)
            {
                bool isMajor = mm % 100 == 0;
                bool isMedium = mm % 50 == 0;
                double tickH = isMajor ? 14 : isMedium ? 8 : 3;

                foreach (int sign in (mm == 0 ? new[] { 1 } : new[] { 1, -1 }))
                {
                    double x = centerX + sign * mm * pixelsPerMm;
                    canvas.Children.Add(new Line { X1 = x, Y1 = canvasHeight - 30, X2 = x, Y2 = canvasHeight - 30 - tickH, Stroke = new SolidColorBrush(isMajor ? Color.FromRgb(160, 160, 160) : Color.FromRgb(80, 80, 80)), StrokeThickness = 1 });
                    if (isMajor)
                    {
                        var lbl = new TextBlock { Text = mm.ToString(), FontSize = 9, Foreground = new SolidColorBrush(Color.FromRgb(140, 140, 140)), TextAlignment = TextAlignment.Center, Width = 40 };
                        Canvas.SetLeft(lbl, x - 20); Canvas.SetTop(lbl, canvasHeight - 28);
                        canvas.Children.Add(lbl);
                    }
                }
            }

            var vasenLbl = new TextBlock { Text = "← Vasen", FontSize = 9, Foreground = new SolidColorBrush(Color.FromRgb(255, 107, 107)) };
            Canvas.SetLeft(vasenLbl, 4); Canvas.SetTop(vasenLbl, canvasHeight - 28);
            canvas.Children.Add(vasenLbl);

            var oikeaLbl = new TextBlock { Text = "Oikea →", FontSize = 9, Foreground = new SolidColorBrush(Color.FromRgb(107, 203, 119)) };
            Canvas.SetLeft(oikeaLbl, canvasWidth - 55); Canvas.SetTop(oikeaLbl, canvasHeight - 28);
            canvas.Children.Add(oikeaLbl);
        }

        private static void PiirraTeraViiva(Canvas canvas, double bladeX,
            double rectY, double rectHeight, double canvasWidth,
            Brush brush, string label, bool labelRight)
        {
            if (bladeX < 0 || bladeX > canvasWidth) return;

            canvas.Children.Add(new Line { X1 = bladeX, Y1 = rectY - 20, X2 = bladeX, Y2 = rectY + rectHeight + 20, Stroke = brush, StrokeThickness = 3 });

            double labelX = labelRight ? Math.Min(bladeX + 4, canvasWidth - 50) : Math.Max(bladeX - 44, 4);

            var parts = label.Split('\n');
            var tb = new TextBlock { Foreground = brush, FontSize = 10, FontWeight = FontWeights.Bold, TextAlignment = labelRight ? TextAlignment.Left : TextAlignment.Right };
            tb.Inlines.Add(new Run(parts[0] + "\n"));
            if (parts.Length > 1) tb.Inlines.Add(new Run(parts[1]));
            Canvas.SetLeft(tb, labelX); Canvas.SetTop(tb, rectY - 38);
            canvas.Children.Add(tb);
        }

        // ── Apufunktiot ──────────────────────────────────────────────────────

        private int GetSelectedPieceCount()
        {
            if (KappaleCombo?.SelectedItem is ComboBoxItem item && int.TryParse(item.Content?.ToString(), out int count)) return count;
            return 4;
        }

        private int GetSelectedYhdistettyCount()
        {
            if (YhdistettyCombo?.SelectedItem is ComboBoxItem item && int.TryParse(item.Content?.ToString(), out int count)) return count;
            return 3;
        }

        private void UpdateKappaleInfo()
        {
            if (KappaleInfo != null) KappaleInfo.Text = $"{GetSelectedPieceCount()} kpl / saha";
        }

        private List<double> GetThicknessValuesVasen()
        {
            int count = GetSelectedPieceCount();
            var result = new List<double>();
            for (int i = 1; i <= count; i++)
            {
                if (paksuusTextBoxesVasen.TryGetValue(i, out var tb) && double.TryParse(tb.Text.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture, out double v) && v > 0) result.Add(v);
                else result.Add(30.0);
            }
            return result;
        }

        private List<double> GetThicknessValuesOikea()
        {
            int count = GetSelectedPieceCount();
            var result = new List<double>();
            for (int i = 5; i < 5 + count; i++)
            {
                if (paksuusTextBoxesOikea.TryGetValue(i, out var tb) && double.TryParse(tb.Text.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture, out double v) && v > 0) result.Add(v);
                else result.Add(30.0);
            }
            return result;
        }

        private List<double> GetThicknessValuesYhdistetty()
        {
            int count = GetSelectedYhdistettyCount();
            var result = new List<double>();
            for (int i = 1; i <= count; i++)
            {
                if (paksuusTextBoxesYhdistetty.TryGetValue(i, out var tb) && double.TryParse(tb.Text.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture, out double v) && v > 0) result.Add(v);
                else result.Add(30.0);
            }
            return result;
        }

        private void SetStatus(string message, Color color)
        {
            if (StatusTextBlock != null)
            {
                StatusTextBlock.Text = message;
                StatusTextBlock.Foreground = new SolidColorBrush(color);
            }
        }
    }
}