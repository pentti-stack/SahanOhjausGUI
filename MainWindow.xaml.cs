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
        public int KappaleCount { get; set; } = 4;
        public string Kuivaus { get; set; } = "0";
        public bool VasenOn { get; set; } = true;
        public bool OikeaOn { get; set; } = true;
    }

    public class TeraParametritDto
    {
        public bool OnkoVasenKatinen { get; set; }
        public double Rako { get; set; }
        public double Runko { get; set; }
        public double Laippa { get; set; }
    }

    public partial class MainWindow : Window
    {
        private readonly Dictionary<int, TeraParametrit> teraParametrit = new();
        private readonly Dictionary<int, TextBox> paksuusTextBoxesVasen = new();
        private readonly Dictionary<int, TextBox> paksuusTextBoxesOikea = new();

        private volatile bool _isPiirraVisualRunning = false;

        private const double Lepo_Paaterä = 340.0;
        private const double Lepo_Keskiterä = -20.0;
        private const double Lepo_Ulkoterä = 20.0;
        private const double Vaisto_Sisaterä = -140.0;
        private const double Vaisto_Ulkoterä = 140.0;

        private static readonly string TallennusPolku =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SahanOhjaus", "asetukset.json");

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

            KuivausTextBox.Text = "0";
            KappaleCombo.SelectedIndex = 2;

            UpdateKappaleInfo();
            LuoParametriKontrollit();
            PiirraVisual();
            SetStatus("Valmis", Colors.LightGray);
        }

        // ── Tallennus ─────────────────────────────────────────────────────────

        private void TallennaTallennus()
        {
            try
            {
                var data = new TallennusData
                {
                    TeraParametrit = teraParametrit.ToDictionary(
                        kvp => kvp.Key,
                        kvp => new TeraParametritDto
                        {
                            OnkoVasenKatinen = kvp.Value.OnkoVasenKatinen,
                            Rako = kvp.Value.Rako,
                            Runko = kvp.Value.Runko,
                            Laippa = kvp.Value.Laippa
                        }),
                    PaksuudetVasen = paksuusTextBoxesVasen.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Text),
                    PaksuudetOikea = paksuusTextBoxesOikea.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Text),
                    KappaleCount = GetSelectedPieceCount(),
                    Kuivaus = KuivausTextBox?.Text ?? "0",
                    VasenOn = VasenSahaCheck?.IsChecked == true,
                    OikeaOn = OikeaSahaCheck?.IsChecked == true
                };

                Directory.CreateDirectory(Path.GetDirectoryName(TallennusPolku)!);
                File.WriteAllText(TallennusPolku,
                    JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch { }
        }

        private void LataaTallennus()
        {
            try
            {
                if (!File.Exists(TallennusPolku)) return;

                var data = JsonSerializer.Deserialize<TallennusData>(
                    File.ReadAllText(TallennusPolku));
                if (data == null) return;

                foreach (var kvp in data.TeraParametrit)
                {
                    if (!teraParametrit.ContainsKey(kvp.Key)) continue;
                    teraParametrit[kvp.Key].OnkoVasenKatinen = kvp.Value.OnkoVasenKatinen;
                    teraParametrit[kvp.Key].Rako = kvp.Value.Rako;
                    teraParametrit[kvp.Key].Runko = kvp.Value.Runko;
                    teraParametrit[kvp.Key].Laippa = kvp.Value.Laippa;
                }

                int idx = data.KappaleCount switch { 2 => 0, 3 => 1, _ => 2 };
                if (KappaleCombo != null) KappaleCombo.SelectedIndex = idx;

                if (KuivausTextBox != null) KuivausTextBox.Text = data.Kuivaus;
                if (VasenSahaCheck != null) VasenSahaCheck.IsChecked = data.VasenOn;
                if (OikeaSahaCheck != null) OikeaSahaCheck.IsChecked = data.OikeaOn;

                Dispatcher.InvokeAsync(() =>
                {
                    foreach (var kvp in data.PaksuudetVasen)
                        if (paksuusTextBoxesVasen.TryGetValue(kvp.Key, out var tb))
                            tb.Text = kvp.Value;

                    foreach (var kvp in data.PaksuudetOikea)
                        if (paksuusTextBoxesOikea.TryGetValue(kvp.Key, out var tb))
                            tb.Text = kvp.Value;
                }, System.Windows.Threading.DispatcherPriority.Loaded);

                SetStatus("✓  Asetukset ladattu", Colors.LightGreen);
            }
            catch { }
        }

        // ── Parametrikontrollit ───────────────────────────────────────────────

        private void LuoParametriKontrollit()
        {
            if (PaksuusPanel == null) return;

            while (PaksuusPanel.Children.Count > 1)
                PaksuusPanel.Children.RemoveAt(PaksuusPanel.Children.Count - 1);

            paksuusTextBoxesVasen.Clear();
            paksuusTextBoxesOikea.Clear();

            int count = GetSelectedPieceCount();
            var root = new StackPanel { Orientation = Orientation.Vertical };

            root.Children.Add(LuoSahaOtsikkoTeksti(
                $"🔴 Vasen: {string.Join(", ", Enumerable.Range(1, count).Select(i => $"K{i}"))}",
                Color.FromRgb(255, 107, 107)));

            var vasenStack = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
            for (int i = 1; i <= count; i++)
            {
                var card = LuoPaksuusKorttiKapea($"K{i}", i);
                vasenStack.Children.Add(card);
                var tb = EtsiKortinTextBox(card);
                if (tb != null) paksuusTextBoxesVasen[i] = tb;
            }
            root.Children.Add(vasenStack);

            root.Children.Add(LuoSahaOtsikkoTeksti(
                $"🟢 Oikea: {string.Join(", ", Enumerable.Range(1, count).Select(i => $"K{i + 4}"))}",
                Color.FromRgb(107, 203, 119)));

            var oikeaStack = new StackPanel();
            for (int i = 5; i < 5 + count; i++)
            {
                var card = LuoPaksuusKorttiKapea($"K{i}", i);
                oikeaStack.Children.Add(card);
                var tb = EtsiKortinTextBox(card);
                if (tb != null) paksuusTextBoxesOikea[i] = tb;
            }
            root.Children.Add(oikeaStack);

            PaksuusPanel.Children.Add(root);
        }

        private static TextBlock LuoSahaOtsikkoTeksti(string teksti, Color vari) =>
            new TextBlock
            {
                Text = teksti,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(vari),
                Margin = new Thickness(0, 0, 0, 6),
                TextWrapping = TextWrapping.Wrap
            };

        private Border LuoPaksuusKorttiKapea(string otsikko, int tagId)
        {
            var border = new Border
            {
                Margin = new Thickness(0, 0, 0, 4),
                Background = new SolidColorBrush(Color.FromRgb(50, 50, 50)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(8, 6, 8, 6)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(32) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(24) });

            var lbl = new TextBlock
            {
                Text = otsikko,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(255, 217, 61)),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 12
            };

            var textBox = new TextBox
            {
                Height = 28,
                Padding = new Thickness(6, 0, 6, 0),
                Text = "30",
                Tag = tagId,
                FontSize = 12,
                VerticalContentAlignment = VerticalAlignment.Center,
                BorderBrush = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(4, 0, 4, 0)
            };

            var mm = new TextBlock
            {
                Text = "mm",
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(140, 140, 140)),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            textBox.TextChanged += (s, e) =>
            {
                bool valid = double.TryParse(textBox.Text.Replace(",", "."),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out double v) && v > 0;
                textBox.BorderBrush = valid
                    ? new SolidColorBrush(Color.FromRgb(100, 100, 100))
                    : new SolidColorBrush(Color.FromRgb(220, 83, 83));
                if (!valid)
                    SetStatus("⚠  Paksuuden täytyy olla positiivinen luku", Colors.Orange);
                else
                    SetStatus("Valmis", Colors.LightGray);
            };
            textBox.TextChanged += PaksuusTextBox_Changed;

            Grid.SetColumn(lbl, 0);
            Grid.SetColumn(textBox, 1);
            Grid.SetColumn(mm, 2);
            grid.Children.Add(lbl);
            grid.Children.Add(textBox);
            grid.Children.Add(mm);

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

        private int GetHalkaisuTeraOikea() =>
            HalkaisuOikeaCombo?.SelectedIndex == 1 ? 2 : 4;

        private int GetHalkaisuTeraVasen() =>
            HalkaisuVasenCombo?.SelectedIndex == 1 ? 1 : 3;

        private void PaksuusTextBox_Changed(object sender, TextChangedEventArgs e) => PiirraVisual();
        private void SahaValinta_Changed(object sender, RoutedEventArgs e) => PiirraVisual();
        private void Halkaisu_Changed(object sender, SelectionChangedEventArgs e) => PiirraVisual();

        private void Kuivaus_Changed(object sender, TextChangedEventArgs e)
        {
            string text = KuivausTextBox?.Text ?? "0";
            if (!double.TryParse(text.Replace(",", "."), NumberStyles.Float,
                    CultureInfo.InvariantCulture, out double val) || val < 0)
                SetStatus("⚠  Kuivausprosentin täytyy olla positiivinen numero", Colors.Orange);
            else
                SetStatus("Valmis", Colors.LightGray);
            PiirraVisual();
        }

        private void Kappale_Changed(object sender, SelectionChangedEventArgs e)
        {
            UpdateKappaleInfo();
            if (HalkaisuPanel != null)
                HalkaisuPanel.Visibility = GetSelectedPieceCount() == 2
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            LuoParametriKontrollit();
            PiirraVisual();
        }

        private void AvaaTerapAsetukset_Click(object sender, RoutedEventArgs e)
        {
            if (teraParametrit == null || teraParametrit.Count == 0)
            {
                MessageBox.Show("Teräparametrit ei ole alustettu!", "Virhe",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

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
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Teräasetusikkuna kaatui:\n\n{ex.Message}\n\n{ex.StackTrace}",
                    "Virhe", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Piirrä ───────────────────────────────────────────────────────────

        private void PiirraVisual()
        {
            if (_isPiirraVisualRunning) return;
            _isPiirraVisualRunning = true;

            try
            {
                if (KuivausTextBox == null) return;

                if (!double.TryParse(KuivausTextBox.Text.Replace(",", "."),
                        NumberStyles.Float, CultureInfo.InvariantCulture, out double kuivausProsentti)
                    || kuivausProsentti < 0)
                    kuivausProsentti = 0;

                double kuivausKerroin = 1.0 + (kuivausProsentti / 100.0);

                if (KuivausValue != null)
                    KuivausValue.Text = $"{kuivausProsentti:F0} %";

                bool vasenOn = VasenSahaCheck?.IsChecked == true;
                bool oikeaOn = OikeaSahaCheck?.IsChecked == true;

                var paksuudetVasen = GetThicknessValuesVasen()
                    .Select(p => p * kuivausKerroin).ToList();
                var paksuudetOikea = GetThicknessValuesOikea()
                    .Select(p => p * kuivausKerroin).ToList();

                double plc_T4 = Lepo_Paaterä, plc_T2 = Lepo_Keskiterä, plc_T6 = Lepo_Ulkoterä;
                double plc_T3 = Lepo_Paaterä, plc_T1 = Lepo_Keskiterä, plc_T5 = Lepo_Ulkoterä;

                if (oikeaOn && paksuudetOikea.Count > 0)
                    LaskeOikeaPuoli(paksuudetOikea, GetHalkaisuTeraOikea(),
                        ref plc_T4, ref plc_T2, ref plc_T6);

                if (vasenOn && paksuudetVasen.Count > 0)
                    LaskeVasenPuoli(paksuudetVasen, GetHalkaisuTeraVasen(),
                        ref plc_T3, ref plc_T1, ref plc_T5);

                if (Tera4_Value != null) Tera4_Value.Text = $"T4: {plc_T4:F1}";
                if (Tera2_Value != null) Tera2_Value.Text = $"T2: {plc_T2:F1}";
                if (Tera6_Value != null) Tera6_Value.Text = $"T6: {plc_T6:F1}";
                if (Tera3_Value != null) Tera3_Value.Text = $"T3: {plc_T3:F1}";
                if (Tera1_Value != null) Tera1_Value.Text = $"T1: {plc_T1:F1}";
                if (Tera5_Value != null) Tera5_Value.Text = $"T5: {plc_T5:F1}";

                if (YhteisCanvas != null)
                {
                    PiirraYhteisCanvas(YhteisCanvas,
                        paksuudetVasen, paksuudetOikea,
                        vasenOn, oikeaOn,
                        plc_T3, plc_T1, plc_T5,
                        plc_T4, plc_T2, plc_T6);
                }
            }
            finally
            {
                _isPiirraVisualRunning = false;
            }
        }

        // ── Laskenta ─────────────────────────────────────────────────────────

        private static double LaskeKeskilinja4(List<double> paksuudet, double rako) =>
            (paksuudet.Sum() + 3 * rako) / 2.0;

        private void LaskeOikeaPuoli(List<double> paksuudet, int halkaisuTera,
            ref double plc_T4, ref double plc_T2, ref double plc_T6)
        {
            if (paksuudet.Count == 0) return;

            if (paksuudet.Count == 2)
            {
                if (halkaisuTera == 4)
                {
                    if (teraParametrit.TryGetValue(4, out var tera4))
                    {
                        double offset = tera4.LaskeOffset();
                        plc_T4 = offset;
                        plc_T2 = -(Vaisto_Sisaterä + offset);
                        plc_T6 = Vaisto_Ulkoterä + offset;
                    }
                }
                else if (halkaisuTera == 2)
                {
                    if (teraParametrit.TryGetValue(2, out var tera2))
                    {
                        plc_T2 = Vaisto_Sisaterä;
                        plc_T4 = Vaisto_Ulkoterä + tera2.LaskeOffset();
                        plc_T6 = plc_T4;
                    }
                }
            }
            else if (paksuudet.Count == 3)
            {
                double k2 = paksuudet[1];
                teraParametrit.TryGetValue(4, out var tera4);
                teraParametrit.TryGetValue(2, out var tera2);

                if (tera4 != null)
                {
                    if (tera4.OnkoVasenKatinen)
                        plc_T4 = k2 / 2.0 + (tera4.Laippa - tera4.Runko / 2.0) + tera4.Rako / 2.0;
                    else
                        plc_T4 = k2 / 2.0 + tera4.Runko / 2.0 + tera4.Rako / 2.0;
                }

                if (tera2 != null && tera4 != null)
                {
                    if (tera2.OnkoVasenKatinen)
                        plc_T2 = -(k2 / 2.0 - ((tera2.Laippa - tera2.Runko / 2.0) - tera2.Rako / 2.0) + plc_T4);
                    else
                        plc_T2 = -(k2 / 2.0 + (tera2.Rako / 2.0 - tera2.Runko / 2.0) + plc_T4);
                }

                plc_T6 = Vaisto_Ulkoterä;
            }
            else if (paksuudet.Count == 4)
            {
                teraParametrit.TryGetValue(4, out var tera4);
                teraParametrit.TryGetValue(2, out var tera2);
                teraParametrit.TryGetValue(6, out var tera6);

                if (tera4 == null) return;

                double k1 = paksuudet[0];
                double k2 = paksuudet[1];
                double k3 = paksuudet[2];
                double kl = LaskeKeskilinja4(paksuudet, tera4.Rako);

                if (tera4.OnkoVasenKatinen)
                    plc_T4 = k1 + k2 + tera4.Rako + tera4.Rako / 2.0 + (tera4.Laippa - tera4.Runko / 2.0) - kl;
                else
                    plc_T4 = k1 + k2 + tera4.Rako + tera4.Rako / 2.0 + tera4.Runko / 2.0 - kl;

                if (tera2 != null)
                {
                    if (tera4.OnkoVasenKatinen)
                    {
                        if (tera2.OnkoVasenKatinen)
                        {
                            double vasenT2Offset = tera2.Laippa + tera2.Rako / 2.0 - tera2.Runko / 2.0;
                            double vasenT4Offset = tera4.Laippa - (tera4.Runko / 2.0 + tera4.Rako / 2.0);
                            plc_T2 = -(k2 - vasenT4Offset + vasenT2Offset);
                        }
                        else
                        {
                            double oikeaT2Offset = tera2.Rako / 2.0 - tera2.Runko / 2.0;
                            double vasenT4Offset = tera4.Laippa + (tera4.Rako / 2.0 - tera4.Runko / 2.0);
                            plc_T2 = -(k2 + vasenT4Offset + oikeaT2Offset);
                        }
                    }
                    else
                    {
                        if (tera2.OnkoVasenKatinen)
                        {
                            double vasenT2Offset = tera2.Laippa - tera2.Rako / 2.0 - tera2.Runko / 2.0;
                            double oikeaT4Offset = tera4.Rako / 2.0 + tera4.Runko / 2.0;
                            plc_T2 = -(oikeaT4Offset + k2 - vasenT2Offset);
                        }
                        else
                        {
                            double oikeaT2Offset = tera2.Runko / 2.0 + tera2.Rako / 2.0;
                            double oikeaT4Offset = tera4.Rako / 2.0 - tera4.Runko / 2.0;
                            plc_T2 = -(oikeaT4Offset + k2 + oikeaT2Offset);
                        }
                    }
                }

                if (tera6 != null)
                {
                    if (tera4.OnkoVasenKatinen)
                    {
                        if (tera6.OnkoVasenKatinen)
                        {
                            double vasenT6Offset = tera6.Laippa + tera6.Rako / 2.0 - tera6.Runko / 2.0;
                            double vasenT4Offset = tera4.Laippa - (tera4.Runko / 2.0 + tera4.Rako / 2.0);
                            plc_T6 = k3 - vasenT4Offset + vasenT6Offset;
                        }
                        else
                        {
                            double oikeaT6Offset = tera6.Runko / 2.0 + tera6.Rako / 2.0;
                            double vasenT4Offset = tera4.Laippa - (tera4.Runko / 2.0 + tera4.Rako / 2.0);
                            plc_T6 = k3 - vasenT4Offset + oikeaT6Offset;
                        }
                    }
                    else
                    {
                        if (tera6.OnkoVasenKatinen)
                        {
                            double vasenT6Offset = tera6.Laippa + tera6.Rako / 2.0 - tera6.Runko / 2.0;
                            double oikeaT4Offset = tera4.Rako / 2.0 - tera4.Runko / 2.0;
                            plc_T6 = oikeaT4Offset + k3 + vasenT6Offset;
                        }
                        else
                        {
                            double oikeaT6Offset = tera6.Runko / 2.0 + tera6.Rako / 2.0;
                            double oikeaT4Offset = tera4.Rako / 2.0 - tera4.Runko / 2.0;
                            plc_T6 = oikeaT4Offset + k3 + oikeaT6Offset;
                        }
                    }
                }
            }
        }

        private void LaskeVasenPuoli(List<double> paksuudet, int halkaisuTera,
            ref double plc_T3, ref double plc_T1, ref double plc_T5)
        {
            if (paksuudet.Count == 0) return;

            if (paksuudet.Count == 2)
            {
                if (halkaisuTera == 3)
                {
                    if (teraParametrit.TryGetValue(3, out var tera3))
                    {
                        double offset = tera3.LaskeOffset();
                        plc_T3 = offset;
                        plc_T1 = -(Vaisto_Sisaterä + offset);
                        plc_T5 = Vaisto_Ulkoterä + offset;
                    }
                }
                else if (halkaisuTera == 1)
                {
                    if (teraParametrit.TryGetValue(1, out var tera1))
                    {
                        plc_T1 = Vaisto_Sisaterä;
                        plc_T3 = Vaisto_Ulkoterä + tera1.LaskeOffset();
                        plc_T5 = plc_T3;
                    }
                }
            }
            else if (paksuudet.Count == 3)
            {
                double k2 = paksuudet[1];
                teraParametrit.TryGetValue(3, out var tera3);
                teraParametrit.TryGetValue(1, out var tera1);

                if (tera3 != null)
                {
                    if (tera3.OnkoVasenKatinen)
                        plc_T3 = k2 / 2.0 + (tera3.Laippa - tera3.Runko / 2.0) + tera3.Rako / 2.0;
                    else
                        plc_T3 = k2 / 2.0 + tera3.Runko / 2.0 + tera3.Rako / 2.0;
                }

                if (tera1 != null && tera3 != null)
                {
                    if (tera1.OnkoVasenKatinen)
                        plc_T1 = -(k2 / 2.0 - ((tera1.Laippa - tera1.Runko / 2.0) - tera1.Rako / 2.0) + plc_T3);
                    else
                        plc_T1 = -(k2 / 2.0 + (tera1.Rako / 2.0 - tera1.Runko / 2.0) + plc_T3);
                }

                plc_T5 = Vaisto_Ulkoterä;
            }
            else if (paksuudet.Count == 4)
            {
                teraParametrit.TryGetValue(3, out var tera3);
                teraParametrit.TryGetValue(1, out var tera1);
                teraParametrit.TryGetValue(5, out var tera5);

                if (tera3 == null) return;

                double k1 = paksuudet[0];
                double k2 = paksuudet[1];
                double k3 = paksuudet[2];
                double kl = LaskeKeskilinja4(paksuudet, tera3.Rako);

                if (tera3.OnkoVasenKatinen)
                    plc_T3 = k1 + k2 + tera3.Rako + tera3.Rako / 2.0 + (tera3.Laippa - tera3.Runko / 2.0) - kl;
                else
                    plc_T3 = k1 + k2 + tera3.Rako + tera3.Rako / 2.0 + tera3.Runko / 2.0 - kl;

                if (tera1 != null)
                {
                    if (tera3.OnkoVasenKatinen)
                    {
                        if (tera1.OnkoVasenKatinen)
                        {
                            double vasenT1Offset = tera1.Laippa + tera1.Rako / 2.0 - tera1.Runko / 2.0;
                            double vasenT3Offset = tera3.Laippa - (tera3.Runko / 2.0 + tera3.Rako / 2.0);
                            plc_T1 = -(k2 - vasenT3Offset + vasenT1Offset);
                        }
                        else
                        {
                            double oikeaT1Offset = tera1.Rako / 2.0 - tera1.Runko / 2.0;
                            double vasenT3Offset = tera3.Laippa + (tera3.Rako / 2.0 - tera3.Runko / 2.0);
                            plc_T1 = -(k2 + vasenT3Offset + oikeaT1Offset);
                        }
                    }
                    else
                    {
                        if (tera1.OnkoVasenKatinen)
                        {
                            double vasenT1Offset = tera1.Laippa - tera1.Rako / 2.0 - tera1.Runko / 2.0;
                            double oikeaT3Offset = tera3.Rako / 2.0 + tera3.Runko / 2.0;
                            plc_T1 = -(oikeaT3Offset + k2 - vasenT1Offset);
                        }
                        else
                        {
                            double oikeaT1Offset = tera1.Runko / 2.0 + tera1.Rako / 2.0;
                            double oikeaT3Offset = tera3.Rako / 2.0 - tera3.Runko / 2.0;
                            plc_T1 = -(oikeaT3Offset + k2 + oikeaT1Offset);
                        }
                    }
                }

                if (tera5 != null)
                {
                    if (tera3.OnkoVasenKatinen)
                    {
                        if (tera5.OnkoVasenKatinen)
                        {
                            double vasenT5Offset = tera5.Laippa + tera5.Rako / 2.0 - tera5.Runko / 2.0;
                            double vasenT3Offset = tera3.Laippa - (tera3.Runko / 2.0 + tera3.Rako / 2.0);
                            plc_T5 = k3 - vasenT3Offset + vasenT5Offset;
                        }
                        else
                        {
                            double oikeaT5Offset = tera5.Runko / 2.0 + tera5.Rako / 2.0;
                            double vasenT3Offset = tera3.Laippa - (tera3.Runko / 2.0 + tera3.Rako / 2.0);
                            plc_T5 = k3 - vasenT3Offset + oikeaT5Offset;
                        }
                    }
                    else
                    {
                        if (tera5.OnkoVasenKatinen)
                        {
                            double vasenT5Offset = tera5.Laippa + tera5.Rako / 2.0 - tera5.Runko / 2.0;
                            double oikeaT3Offset = tera3.Rako / 2.0 - tera3.Runko / 2.0;
                            plc_T5 = oikeaT3Offset + k3 + vasenT5Offset;
                        }
                        else
                        {
                            double oikeaT5Offset = tera5.Runko / 2.0 + tera5.Rako / 2.0;
                            double oikeaT3Offset = tera3.Rako / 2.0 - tera3.Runko / 2.0;
                            plc_T5 = oikeaT3Offset + k3 + oikeaT5Offset;
                        }
                    }
                }
            }
        }

        // ── Canvas ───────────────────────────────────────────────────────────

        private void PiirraYhteisCanvas(Canvas canvas,
            List<double> paksuudetVasen, List<double> paksuudetOikea,
            bool vasenOn, bool oikeaOn,
            double plc_T3, double plc_T1, double plc_T5,
            double plc_T4, double plc_T2, double plc_T6)
        {
            canvas.Children.Clear();

            double canvasWidth = canvas.ActualWidth > 20 ? canvas.ActualWidth
                : canvas.Width > 0 ? canvas.Width : 900;
            double canvasHeight = canvas.ActualHeight > 20 ? canvas.ActualHeight
                : canvas.Height > 0 ? canvas.Height : 400;
            double centerY = canvasHeight / 2.0;
            double centerX = canvasWidth / 2.0;
            double pixelsPerMm = (canvasWidth / 2.0 - 20) / 350.0;

            double rectHeight = canvasHeight * 0.45;
            double rectY = centerY - rectHeight / 2.0;

            canvas.Children.Add(new Rectangle
            {
                Width = canvasWidth,
                Height = canvasHeight,
                Fill = new SolidColorBrush(Color.FromRgb(13, 13, 13))
            });

            PiirraAsteikko(canvas, canvasWidth, canvasHeight, centerX, pixelsPerMm);

            canvas.Children.Add(new Line
            {
                X1 = centerX,
                Y1 = 20,
                X2 = centerX,
                Y2 = canvasHeight - 30,
                Stroke = new SolidColorBrush(Color.FromRgb(79, 195, 247)),
                StrokeThickness = 1.5,
                StrokeDashArray = new DoubleCollection { 4, 3 }
            });
            var cLbl = new TextBlock
            {
                Text = "0",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(79, 195, 247))
            };
            Canvas.SetLeft(cLbl, centerX + 3);
            Canvas.SetTop(cLbl, 4);
            canvas.Children.Add(cLbl);

            var pieceColors = new[]
            {
                Color.FromRgb(76, 175, 80),
                Color.FromRgb(33, 150, 243),
                Color.FromRgb(233, 30, 99),
                Color.FromRgb(255, 193, 7)
            };

            // ── Oikea saha kappaleet ──────────────────────────────────────────
            if (oikeaOn && paksuudetOikea.Count > 0)
            {
                var raotOikea = new[]
                {
                    teraParametrit.TryGetValue(2, out var t2r) ? t2r.Rako : 4.0,
                    teraParametrit.TryGetValue(4, out var t4r) ? t4r.Rako : 4.0,
                    teraParametrit.TryGetValue(6, out var t6r) ? t6r.Rako : 4.0,
                };
                double totalMm = paksuudetOikea.Sum()
                    + raotOikea.Take(paksuudetOikea.Count - 1).Sum();
                PiirraPuoliRaot(canvas, paksuudetOikea, raotOikea, -(totalMm / 2.0),
                    centerX, rectY, rectHeight, centerY, pixelsPerMm,
                    pieceColors, vasemmalle: false, border: Color.FromRgb(107, 203, 119),
                    kappaleOffset: 5);
            }

            // ── Vasen saha kappaleet ──────────────────────────────────────────
            if (vasenOn && paksuudetVasen.Count > 0)
            {
                var raotVasen = new[]
                {
                    teraParametrit.TryGetValue(1, out var t1r) ? t1r.Rako : 4.0,
                    teraParametrit.TryGetValue(3, out var t3r) ? t3r.Rako : 4.0,
                    teraParametrit.TryGetValue(5, out var t5r) ? t5r.Rako : 4.0,
                };
                double totalMm = paksuudetVasen.Sum()
                    + raotVasen.Take(paksuudetVasen.Count - 1).Sum();
                PiirraPuoliRaot(canvas, paksuudetVasen, raotVasen, -(totalMm / 2.0),
                    centerX, rectY, rectHeight, centerY, pixelsPerMm,
                    pieceColors, vasemmalle: true, border: Color.FromRgb(255, 107, 107),
                    kappaleOffset: 1);
            }

            // ── Terä-viivat oikea — piirretään aina ──────────────────────────
            {
                var brushO = new SolidColorBrush(oikeaOn
                    ? Color.FromRgb(107, 203, 119)
                    : Color.FromRgb(50, 90, 50));
                double t4X = centerX + plc_T4 * pixelsPerMm;
                double t2X = t4X + plc_T2 * pixelsPerMm;
                double t6X = t4X + plc_T6 * pixelsPerMm;

                PiirraTeraViiva(canvas, t4X, rectY, rectHeight, canvasWidth, brushO, $"T4\n{plc_T4:F1}", true);
                PiirraTeraViiva(canvas, t2X, rectY, rectHeight, canvasWidth, brushO, $"T2\n{plc_T2:F1}", false);
                PiirraTeraViiva(canvas, t6X, rectY, rectHeight, canvasWidth, brushO, $"T6\n{plc_T6:F1}", true);

                var ol = new TextBlock
                {
                    Text = "Oikea saha ▶",
                    Foreground = brushO,
                    FontSize = 11,
                    FontWeight = FontWeights.Bold
                };
                Canvas.SetLeft(ol, canvasWidth - 140);
                Canvas.SetTop(ol, 6);
                canvas.Children.Add(ol);
            }

            // ── Terä-viivat vasen — piirretään aina ──────────────────────────
            {
                var brushV = new SolidColorBrush(vasenOn
                    ? Color.FromRgb(255, 107, 107)
                    : Color.FromRgb(90, 50, 50));
                double t3X = centerX - plc_T3 * pixelsPerMm;
                double t1X = t3X - plc_T1 * pixelsPerMm;
                double t5X = t3X - plc_T5 * pixelsPerMm;

                PiirraTeraViiva(canvas, t3X, rectY, rectHeight, canvasWidth, brushV, $"T3\n{plc_T3:F1}", true);
                PiirraTeraViiva(canvas, t1X, rectY, rectHeight, canvasWidth, brushV, $"T1\n{plc_T1:F1}", false);
                PiirraTeraViiva(canvas, t5X, rectY, rectHeight, canvasWidth, brushV, $"T5\n{plc_T5:F1}", true);

                var vl = new TextBlock
                {
                    Text = "◀ Vasen saha",
                    Foreground = brushV,
                    FontSize = 11,
                    FontWeight = FontWeights.Bold
                };
                Canvas.SetLeft(vl, 6);
                Canvas.SetTop(vl, 6);
                canvas.Children.Add(vl);
            }
        }

        private static void PiirraPuoliRaot(
            Canvas canvas, List<double> paksuudet, double[] raot, double startMm,
            double centerX, double rectY, double rectHeight, double centerY,
            double pixelsPerMm, Color[] pieceColors, bool vasemmalle, Color border,
            int kappaleOffset)
        {
            int n = paksuudet.Count;
            double curMm = startMm;

            double kokonaisLeveys = paksuudet.Sum() + raot.Take(n - 1).Sum();

            for (int i = 0; i < n; i++)
            {
                double paksuus = paksuudet[i];
                double pieceW = paksuus * pixelsPerMm;
                double pieceX = vasemmalle
                    ? centerX - curMm * pixelsPerMm - pieceW
                    : centerX + curMm * pixelsPerMm;

                int kappaleNro = kappaleOffset + i;

                var piece = new Rectangle
                {
                    Width = pieceW,
                    Height = rectHeight,
                    Fill = new SolidColorBrush(Color.FromArgb(180,
                        pieceColors[i % pieceColors.Length].R,
                        pieceColors[i % pieceColors.Length].G,
                        pieceColors[i % pieceColors.Length].B)),
                    Stroke = new SolidColorBrush(border),
                    StrokeThickness = 1,
                    RadiusX = 3,
                    RadiusY = 3
                };
                Canvas.SetLeft(piece, pieceX);
                Canvas.SetTop(piece, rectY);
                canvas.Children.Add(piece);

                var tl = new TextBlock
                {
                    Text = $"{paksuus:F0}",
                    FontSize = 11,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White,
                    TextAlignment = TextAlignment.Center,
                    Width = Math.Max(20, pieceW)
                };
                Canvas.SetLeft(tl, pieceX + pieceW / 2.0 - tl.Width / 2.0);
                Canvas.SetTop(tl, centerY - 10);
                canvas.Children.Add(tl);

                var nl = new TextBlock
                {
                    Text = $"K{kappaleNro}",
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Color.FromArgb(220, 255, 255, 255))
                };
                Canvas.SetLeft(nl, pieceX + 4);
                Canvas.SetTop(nl, rectY + 4);
                canvas.Children.Add(nl);

                curMm += paksuus;

                if (i < n - 1)
                {
                    double rako = raot[i % raot.Length];
                    double gapX = vasemmalle
                        ? centerX - curMm * pixelsPerMm - rako * pixelsPerMm
                        : centerX + curMm * pixelsPerMm;
                    double gapW = rako * pixelsPerMm;

                    var gap = new Rectangle
                    {
                        Width = gapW,
                        Height = rectHeight,
                        Fill = new SolidColorBrush(Color.FromArgb(120, 30, 30, 30)),
                        Stroke = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                        StrokeThickness = 1,
                        StrokeDashArray = new DoubleCollection { 3, 2 }
                    };
                    Canvas.SetLeft(gap, gapX);
                    Canvas.SetTop(gap, rectY);
                    canvas.Children.Add(gap);

                    if (gapW > 8)
                    {
                        var rakoLbl = new TextBlock
                        {
                            Text = $"{rako:F1}",
                            FontSize = 8,
                            Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                            TextAlignment = TextAlignment.Center,
                            Width = Math.Max(10, gapW)
                        };
                        Canvas.SetLeft(rakoLbl, gapX + gapW / 2.0 - rakoLbl.Width / 2.0);
                        Canvas.SetTop(rakoLbl, centerY + 4);
                        canvas.Children.Add(rakoLbl);
                    }

                    curMm += rako;
                }
            }

            // Kokonaisleveys mitta-jana
            double totalStartX = vasemmalle
                ? centerX - (startMm + kokonaisLeveys) * pixelsPerMm
                : centerX + startMm * pixelsPerMm;
            double totalW = kokonaisLeveys * pixelsPerMm;
            double arrowY = rectY + rectHeight + 10;

            canvas.Children.Add(new Line
            {
                X1 = totalStartX,
                Y1 = arrowY,
                X2 = totalStartX + totalW,
                Y2 = arrowY,
                Stroke = new SolidColorBrush(border),
                StrokeThickness = 1
            });
            canvas.Children.Add(new Line
            {
                X1 = totalStartX,
                Y1 = arrowY - 4,
                X2 = totalStartX,
                Y2 = arrowY + 4,
                Stroke = new SolidColorBrush(border),
                StrokeThickness = 1
            });
            canvas.Children.Add(new Line
            {
                X1 = totalStartX + totalW,
                Y1 = arrowY - 4,
                X2 = totalStartX + totalW,
                Y2 = arrowY + 4,
                Stroke = new SolidColorBrush(border),
                StrokeThickness = 1
            });

            var kokoLbl = new TextBlock
            {
                Text = $"⟵ {kokonaisLeveys:F1} mm ⟶",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(border),
                TextAlignment = TextAlignment.Center,
                Width = Math.Max(60, totalW)
            };
            Canvas.SetLeft(kokoLbl, totalStartX + totalW / 2.0 - kokoLbl.Width / 2.0);
            Canvas.SetTop(kokoLbl, arrowY + 5);
            canvas.Children.Add(kokoLbl);
        }

        private static void PiirraAsteikko(Canvas canvas, double canvasWidth,
            double canvasHeight, double centerX, double pixelsPerMm)
        {
            canvas.Children.Add(new Line
            {
                X1 = 10,
                Y1 = canvasHeight - 30,
                X2 = canvasWidth - 10,
                Y2 = canvasHeight - 30,
                Stroke = new SolidColorBrush(Color.FromRgb(80, 80, 80)),
                StrokeThickness = 1
            });

            for (int mm = 0; mm <= 350; mm += 10)
            {
                bool isMajor = mm % 100 == 0;
                bool isMedium = mm % 50 == 0;
                double tickH = isMajor ? 14 : isMedium ? 8 : 3;

                foreach (int sign in (mm == 0 ? new[] { 1 } : new[] { 1, -1 }))
                {
                    double x = centerX + sign * mm * pixelsPerMm;
                    canvas.Children.Add(new Line
                    {
                        X1 = x,
                        Y1 = canvasHeight - 30,
                        X2 = x,
                        Y2 = canvasHeight - 30 - tickH,
                        Stroke = new SolidColorBrush(isMajor
                            ? Color.FromRgb(160, 160, 160)
                            : Color.FromRgb(80, 80, 80)),
                        StrokeThickness = 1
                    });

                    if (isMajor)
                    {
                        var lbl = new TextBlock
                        {
                            Text = mm.ToString(),
                            FontSize = 9,
                            Foreground = new SolidColorBrush(Color.FromRgb(140, 140, 140)),
                            TextAlignment = TextAlignment.Center,
                            Width = 40
                        };
                        Canvas.SetLeft(lbl, x - 20);
                        Canvas.SetTop(lbl, canvasHeight - 28);
                        canvas.Children.Add(lbl);
                    }
                }
            }

            var vasenLbl = new TextBlock
            {
                Text = "← Vasen",
                FontSize = 9,
                Foreground = new SolidColorBrush(Color.FromRgb(255, 107, 107))
            };
            Canvas.SetLeft(vasenLbl, 4);
            Canvas.SetTop(vasenLbl, canvasHeight - 28);
            canvas.Children.Add(vasenLbl);

            var oikeaLbl = new TextBlock
            {
                Text = "Oikea →",
                FontSize = 9,
                Foreground = new SolidColorBrush(Color.FromRgb(107, 203, 119))
            };
            Canvas.SetLeft(oikeaLbl, canvasWidth - 55);
            Canvas.SetTop(oikeaLbl, canvasHeight - 28);
            canvas.Children.Add(oikeaLbl);
        }

        private static void PiirraTeraViiva(Canvas canvas, double bladeX,
            double rectY, double rectHeight, double canvasWidth,
            Brush brush, string label, bool labelRight)
        {
            if (bladeX < 0 || bladeX > canvasWidth) return;

            canvas.Children.Add(new Line
            {
                X1 = bladeX,
                Y1 = rectY - 20,
                X2 = bladeX,
                Y2 = rectY + rectHeight + 20,
                Stroke = brush,
                StrokeThickness = 3
            });

            double labelX = labelRight
                ? Math.Min(bladeX + 4, canvasWidth - 50)
                : Math.Max(bladeX - 44, 4);

            var parts = label.Split('\n');
            var tb = new TextBlock
            {
                Foreground = Brushes.White,
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                TextAlignment = labelRight ? TextAlignment.Left : TextAlignment.Right
            };
            tb.Inlines.Add(new Run(parts[0] + "\n"));
            if (parts.Length > 1) tb.Inlines.Add(new Run(parts[1]));

            Canvas.SetLeft(tb, labelX);
            Canvas.SetTop(tb, rectY - 38);
            canvas.Children.Add(tb);
        }

        // ── Apufunktiot ──────────────────────────────────────────────────────

        private int GetSelectedPieceCount()
        {
            if (KappaleCombo?.SelectedItem is ComboBoxItem item &&
                int.TryParse(item.Content?.ToString(), out int count))
                return count;
            return 4;
        }

        private void UpdateKappaleInfo()
        {
            if (KappaleInfo != null)
                KappaleInfo.Text = $"{GetSelectedPieceCount()} kpl / saha";
        }

        private List<double> GetThicknessValuesVasen()
        {
            int count = GetSelectedPieceCount();
            var result = new List<double>();
            for (int i = 1; i <= count; i++)
            {
                if (paksuusTextBoxesVasen.TryGetValue(i, out var tb) &&
                    double.TryParse(tb.Text.Replace(",", "."),
                        NumberStyles.Float, CultureInfo.InvariantCulture, out double v) && v > 0)
                    result.Add(v);
                else
                    result.Add(30.0);
            }
            return result;
        }

        private List<double> GetThicknessValuesOikea()
        {
            int count = GetSelectedPieceCount();
            var result = new List<double>();
            for (int i = 5; i < 5 + count; i++)
            {
                if (paksuusTextBoxesOikea.TryGetValue(i, out var tb) &&
                    double.TryParse(tb.Text.Replace(",", "."),
                        NumberStyles.Float, CultureInfo.InvariantCulture, out double v) && v > 0)
                    result.Add(v);
                else
                    result.Add(30.0);
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