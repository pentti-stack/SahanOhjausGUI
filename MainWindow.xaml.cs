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
using System.Windows.Media.Animation;

namespace SahanOhjausGUI
{
    public class TallennusData
    {
        public Dictionary<int, TeraParametritDto> TeraParametrit { get; set; } = new();
        public Dictionary<int, string> PaksuudetVasen { get; set; } = new();
        public Dictionary<int, string> PaksuudetOikea { get; set; } = new();
        public Dictionary<int, string> PaksuudetYhdistetty { get; set; } = new();
        public Dictionary<int, string> LeveysVasen { get; set; } = new();
        public Dictionary<int, string> LeveysOikea { get; set; } = new();
        public Dictionary<int, string> LeveysYhdistetty { get; set; } = new();
        public int KappaleCount { get; set; } = 4;
        public int YhdistettyCount { get; set; } = 3;
        public string Kuivaus { get; set; } = "0";
        public bool VasenOn { get; set; } = true;
        public bool OikeaOn { get; set; } = true;
        public bool Ph1On { get; set; } = false;
        public bool Ph2On { get; set; } = false;
        public bool ProfilointiOn { get; set; } = true;
        public string PhLevinKappale { get; set; } = "150";
        public string PhKokonaisLeveys { get; set; } = "600";
        public string PhKuivaus { get; set; } = "0";
        public Dictionary<int, TeraRajatDto> TeraRajat { get; set; } = new();
        public Dictionary<string, ProfilointiRajaDto> ProfilointiRajat { get; set; } = new();
        public Dictionary<string, PhRajatDto> PhRajat { get; set; } = new();
        public double Ph1Offset { get; set; } = 0.0;
        public double Ph2Offset { get; set; } = 0.0;
        public double TurvaEtaisyys { get; set; } = 15.0;
        public double OffsetT1 { get; set; } = 0.0;
        public double OffsetT2 { get; set; } = 0.0;
        public double OffsetT3 { get; set; } = 0.0;
        public double OffsetT4 { get; set; } = 0.0;
        public double OffsetT5 { get; set; } = 0.0;
        public double OffsetT6 { get; set; } = 0.0;
        public double ProfOffsetT1 { get; set; } = 0.0;
        public double ProfOffsetT2 { get; set; } = 0.0;
        public double ProfOffsetT3 { get; set; } = 0.0;
        public double ProfOffsetT4 { get; set; } = 0.0;
        public double ProfOffsetT5 { get; set; } = 0.0;
        public double ProfOffsetT6 { get; set; } = 0.0;
        public double ProfOffsetT7 { get; set; } = 0.0;
        public double ProfOffsetT8 { get; set; } = 0.0;
        public int YhdistettyT1RefKappale { get; set; } = 0;  // 0 = automaattinen default
        public int YhdistettyT2RefKappale { get; set; } = 0;  // 0 = automaattinen default
        public bool KokoRuutuTila { get; set; } = true;
        public Dictionary<string, List<DimensioAsete>> TallennetutDimensiot { get; set; } = new();
    }

    public class DimensioAsete
    {
        public string Nimi { get; set; } = "";
        public Dictionary<int, string> Paksuudet { get; set; } = new();
        public Dictionary<int, string> Leveydet { get; set; } = new();
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

    public class ProfilointiRajaDto
    {
        public double Min { get; set; }
        public double Max { get; set; }
        public double Lepopaikka { get; set; }
    }

    public class PhRajatDto
    {
        public double LepoVasen { get; set; } = 0.0;
        public double LepoOikea { get; set; } = 0.0;
    }

    public partial class MainWindow : Window
    {
        private readonly Dictionary<int, TeraParametrit> teraParametrit = new();
        private readonly Dictionary<int, TextBox> paksuusTextBoxesVasen = new();
        private readonly Dictionary<int, TextBox> paksuusTextBoxesOikea = new();
        private readonly Dictionary<int, TextBox> paksuusTextBoxesYhdistetty = new();
        private readonly Dictionary<int, TextBox> leveysTextBoxesVasen = new();
        private readonly Dictionary<int, TextBox> leveysTextBoxesOikea = new();
        private readonly Dictionary<int, TextBox> leveysTextBoxesYhdistetty = new();
        private readonly Dictionary<int, TeraRajat> teraRajat = new();
        private readonly Dictionary<string, ProfilointiRaja> profilointiRajat = new();
        private readonly Dictionary<string, PhRajat> phRajat = new();
        private readonly Dictionary<string, List<DimensioAsete>> tallennetutDimensiot = new();

        private double _ph1Offset = 0.0;
        private double _ph2Offset = 0.0;
        private double _tukkiHalkaisija = 200.0;
        private bool _kokoRuutuTila = true;

        private double _offsetT1 = 0.0, _offsetT2 = 0.0, _offsetT3 = 0.0;
        private double _offsetT4 = 0.0, _offsetT5 = 0.0, _offsetT6 = 0.0;

        private double _profOffsetT1 = 0.0, _profOffsetT2 = 0.0;
        private double _profOffsetT3 = 0.0, _profOffsetT4 = 0.0;
        private double _profOffsetT5 = 0.0, _profOffsetT6 = 0.0;
        private double _profOffsetT7 = 0.0, _profOffsetT8 = 0.0;

        private int _yhdistettyT1RefKappale = 0;  // 0 = automaattinen
        private int _yhdistettyT2RefKappale = 0;  // 0 = automaattinen

        // Stored from PiirraVisual for use by PiirraProfilointiCanvas
        private List<double> _currentPaksuudet = new();
        private List<double> _currentLeveydet = new();

        // Viimeisin laskettu PLC-arvo WinCC-ikkunaa varten
        private double _lastPlcT1, _lastPlcT2, _lastPlcT3, _lastPlcT4, _lastPlcT5, _lastPlcT6;
        private double _lastPh1V, _lastPh1O, _lastPh2V, _lastPh2O;
        private double _lastProfT1, _lastProfT2, _lastProfT3, _lastProfT4;
        private double _lastProfT5, _lastProfT6, _lastProfT7, _lastProfT8;

        private bool _profilointiOn = true;
        private double turvaEtaisyys = 15.0;
        private volatile bool _isPiirraVisualRunning = false;
        private WinCCIntegrationWindow? _winCCWindow;

        private const double Vaisto_Sisaterä = -140.0;
        private const double Vaisto_Ulkoterä = 140.0;
        private const double DefaultProfilointiRako = 4.0;
        private const double DefaultProfilointiLeveys = 100.0;

        private double GetProfilointiRako(int teraNumero) =>
            teraParametrit.TryGetValue(teraNumero, out var t) ? t.Rako : DefaultProfilointiRako;

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
            PaivitaKokoRuutuTila();

            // Estä sulkeminen — tallenna silti
            Closing += (s, e) =>
            {
                //e.Cancel = true;        // ei sulkeudu koskaan
                TallennaTallennus();    // tallentaa silti aina
            };
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

            foreach (var key in ProfilointiRaja.KaikkiAvaimet)
                profilointiRajat[key] = ProfilointiRaja.LuoOletus(key);

            phRajat["PH1V"] = new PhRajat { LepoVasen = 0.0, LepoOikea = 0.0 };
            phRajat["PH1O"] = new PhRajat { LepoVasen = 0.0, LepoOikea = 0.0 };
            phRajat["PH2V"] = new PhRajat { LepoVasen = 0.0, LepoOikea = 0.0 };
            phRajat["PH2O"] = new PhRajat { LepoVasen = 0.0, LepoOikea = 0.0 };

            KappaleCombo.SelectionChanged -= Kappale_Changed;
            YhdistettyCombo.SelectionChanged -= YhdistettyKappale_Changed;
            VasenSahaCheck.Checked -= SahaValinta_Changed;
            VasenSahaCheck.Unchecked -= SahaValinta_Changed;
            OikeaSahaCheck.Checked -= SahaValinta_Changed;
            OikeaSahaCheck.Unchecked -= SahaValinta_Changed;
            Ph1Check.Checked -= PhValinta_Changed;
            Ph1Check.Unchecked -= PhValinta_Changed;
            Ph2Check.Checked -= PhValinta_Changed;
            Ph2Check.Unchecked -= PhValinta_Changed;

            KuivausTextBox.Text = "0";
            KappaleCombo.SelectedIndex = 2;
            YhdistettyCombo.SelectedIndex = 0;

            KappaleCombo.SelectionChanged += Kappale_Changed;
            YhdistettyCombo.SelectionChanged += YhdistettyKappale_Changed;
            VasenSahaCheck.Checked += SahaValinta_Changed;
            VasenSahaCheck.Unchecked += SahaValinta_Changed;
            OikeaSahaCheck.Checked += SahaValinta_Changed;
            OikeaSahaCheck.Unchecked += SahaValinta_Changed;
            Ph1Check.Checked += PhValinta_Changed;
            Ph1Check.Unchecked += PhValinta_Changed;
            Ph2Check.Checked += PhValinta_Changed;
            Ph2Check.Unchecked += PhValinta_Changed;

            UpdateKappaleInfo();
            PaivitaNakymat();
            PiirraVisual();
            SetStatus("Valmis", Colors.LightGray);
        }

        private bool OnYhdistettyTila() =>
            VasenSahaCheck?.IsChecked == true && OikeaSahaCheck?.IsChecked == true;

        private bool OnJakosahaKaytossa() =>
            VasenSahaCheck?.IsChecked == true || OikeaSahaCheck?.IsChecked == true;

        private bool OnPhKaytossa() =>
            Ph1Check?.IsChecked == true && Ph2Check?.IsChecked == true;

        private static DimensioAsete KopioiDimensioAsete(DimensioAsete asete) =>
            new DimensioAsete
            {
                Nimi = asete.Nimi,
                Paksuudet = asete.Paksuudet.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                Leveydet = asete.Leveydet.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            };

        private void PaivitaKokoRuutuTila()
        {
            if (_kokoRuutuTila)
            {
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
                ResizeMode = ResizeMode.NoResize;
            }
            else
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
                ResizeMode = ResizeMode.CanResize;
                WindowState = WindowState.Normal;
            }
        }

        private void PaivitaHalkaisuNakyvyys()
        {
            bool vasenOn = VasenSahaCheck?.IsChecked == true;
            bool oikeaOn = OikeaSahaCheck?.IsChecked == true;
            bool naytaHalkaisuPanel = GetSelectedPieceCount() == 2 && vasenOn != oikeaOn;

            if (HalkaisuPanel != null)
                HalkaisuPanel.Visibility = naytaHalkaisuPanel ? Visibility.Visible : Visibility.Collapsed;
            if (HalkaisuOikeaContainer != null)
                HalkaisuOikeaContainer.Visibility = oikeaOn && !vasenOn ? Visibility.Visible : Visibility.Collapsed;
            if (HalkaisuVasenContainer != null)
                HalkaisuVasenContainer.Visibility = vasenOn && !oikeaOn ? Visibility.Visible : Visibility.Collapsed;
        }

        private void PaivitaNakymat()
        {
            LuoParametriKontrollit();
            PaivitaHalkaisuNakyvyys();
            PaivitaDimensioUi();
            PaivitaPhLukitus();
            PiirraVisual();
        }

        private void PaivitaPhLukitus()
        {
            bool jakosahaOn = OnJakosahaKaytossa();

            if (PhLevinKappaleBox != null)
            {
                PhLevinKappaleBox.IsReadOnly = jakosahaOn;
                PhLevinKappaleBox.Background = new SolidColorBrush(
                    jakosahaOn ? Color.FromRgb(25, 25, 25) : Color.FromRgb(61, 61, 61));
            }
            if (PhKokonaisLeveysBox != null)
            {
                PhKokonaisLeveysBox.IsReadOnly = jakosahaOn;
                PhKokonaisLeveysBox.Background = new SolidColorBrush(
                    jakosahaOn ? Color.FromRgb(25, 25, 25) : Color.FromRgb(61, 61, 61));
            }
            if (Ph1LukittuLabel != null)
                Ph1LukittuLabel.Visibility = jakosahaOn ? Visibility.Visible : Visibility.Collapsed;
            if (Ph2LukittuLabel != null)
                Ph2LukittuLabel.Visibility = jakosahaOn ? Visibility.Visible : Visibility.Collapsed;
        }

        private static Dictionary<int, string> HaeTextArvot(Dictionary<int, TextBox> textBoxes) =>
            textBoxes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Text);

        private static void AsetaTextArvot(Dictionary<int, TextBox> textBoxes, Dictionary<int, string> arvot)
        {
            foreach (var kvp in arvot)
                if (textBoxes.TryGetValue(kvp.Key, out var tb))
                    tb.Text = kvp.Value;
        }

        private void TallennaDimensio(string key, string nimi, Dictionary<int, string> paksuudet, Dictionary<int, string> leveydet)
        {
            if (!tallennetutDimensiot.TryGetValue(key, out var lista))
            {
                lista = new List<DimensioAsete>();
                tallennetutDimensiot[key] = lista;
            }

            var olemassaOleva = lista.FirstOrDefault(x => string.Equals(x.Nimi, nimi, StringComparison.OrdinalIgnoreCase));
            var paksuusArvot = paksuudet.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            var leveysArvot = leveydet.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            if (olemassaOleva != null)
            {
                olemassaOleva.Paksuudet = paksuusArvot;
                olemassaOleva.Leveydet = leveysArvot;
            }
            else
            {
                lista.Add(new DimensioAsete
                {
                    Nimi = nimi,
                    Paksuudet = paksuusArvot,
                    Leveydet = leveysArvot
                });
            }

            tallennetutDimensiot[key] = lista.OrderBy(x => x.Nimi, StringComparer.CurrentCultureIgnoreCase).ToList();
            TallennaTallennus();
            PaivitaDimensioUi();
        }

        private DimensioAsete? HaeValittuDimensio(string key, ComboBox? combo)
        {
            if (combo?.SelectedItem is not string nimi || !tallennetutDimensiot.TryGetValue(key, out var lista))
                return null;

            return lista.FirstOrDefault(x => string.Equals(x.Nimi, nimi, StringComparison.OrdinalIgnoreCase));
        }

        private void PaivitaDimensioCombo(ComboBox? combo, string key)
        {
            if (combo == null)
                return;

            string? aiempiValinta = combo.SelectedItem as string;
            var nimet = tallennetutDimensiot.TryGetValue(key, out var lista)
                ? lista.OrderBy(x => x.Nimi, StringComparer.CurrentCultureIgnoreCase).Select(x => x.Nimi).ToList()
                : new List<string>();

            combo.ItemsSource = nimet;
            combo.SelectedItem = aiempiValinta != null && nimet.Contains(aiempiValinta)
                ? aiempiValinta
                : nimet.FirstOrDefault();
        }

        private string? HaeAktiivinenJakosahaDimensioKey()
        {
            if (OnYhdistettyTila())
                return $"yhdistetty_{GetSelectedYhdistettyCount()}kpl";
            if (VasenSahaCheck?.IsChecked == true && OikeaSahaCheck?.IsChecked == false)
                return $"vasen_{GetSelectedPieceCount()}kpl";
            if (OikeaSahaCheck?.IsChecked == true && VasenSahaCheck?.IsChecked == false)
                return $"oikea_{GetSelectedPieceCount()}kpl";
            return null;
        }

        private string HaeJakosahaDimensioOtsikko(string key)
        {
            if (key.StartsWith("yhdistetty_", StringComparison.Ordinal))
                return $"Yhdistetty {key["yhdistetty_".Length..]}";
            if (key.StartsWith("vasen_", StringComparison.Ordinal))
                return $"Vasen {key["vasen_".Length..]}";
            if (key.StartsWith("oikea_", StringComparison.Ordinal))
                return $"Oikea {key["oikea_".Length..]}";
            return "Dimensiot";
        }

        private bool TryHaeAktiivisetJakosahaKentat(out Dictionary<int, TextBox> paksuudet, out Dictionary<int, TextBox> leveydet)
        {
            if (OnYhdistettyTila())
            {
                paksuudet = paksuusTextBoxesYhdistetty;
                leveydet = leveysTextBoxesYhdistetty;
                return true;
            }

            if (VasenSahaCheck?.IsChecked == true && OikeaSahaCheck?.IsChecked == false)
            {
                paksuudet = paksuusTextBoxesVasen;
                leveydet = leveysTextBoxesVasen;
                return true;
            }

            if (OikeaSahaCheck?.IsChecked == true && VasenSahaCheck?.IsChecked == false)
            {
                paksuudet = paksuusTextBoxesOikea;
                leveydet = leveysTextBoxesOikea;
                return true;
            }

            paksuudet = new Dictionary<int, TextBox>();
            leveydet = new Dictionary<int, TextBox>();
            return false;
        }

        private void PaivitaDimensioUi()
        {
            string? jakosahaKey = HaeAktiivinenJakosahaDimensioKey();
            if (JakosahaDimensioPanel != null)
                JakosahaDimensioPanel.Visibility = jakosahaKey != null ? Visibility.Visible : Visibility.Collapsed;
            if (JakosahaDimensioTitle != null)
                JakosahaDimensioTitle.Text = jakosahaKey != null ? $"{HaeJakosahaDimensioOtsikko(jakosahaKey)} dimensiot" : "Dimensiot";
            if (jakosahaKey != null)
                PaivitaDimensioCombo(JakosahaDimensioCombo, jakosahaKey);

            if (Ph1DimensioPanel != null)
                Ph1DimensioPanel.Visibility = Ph1Check?.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            if (Ph2DimensioPanel != null)
                Ph2DimensioPanel.Visibility = Ph2Check?.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

            PaivitaDimensioCombo(Ph1DimensioCombo, "ph1");
            PaivitaDimensioCombo(Ph2DimensioCombo, "ph2");
        }

        private void TallennaAktiivinenJakosahaDimensio()
        {
            string nimi = JakosahaDimensioNimiBox?.Text?.Trim() ?? "";
            string? key = HaeAktiivinenJakosahaDimensioKey();
            if (string.IsNullOrWhiteSpace(nimi) || key == null || !TryHaeAktiivisetJakosahaKentat(out var paksuudet, out var leveydet))
                return;

            TallennaDimensio(key, nimi, HaeTextArvot(paksuudet), HaeTextArvot(leveydet));
            if (JakosahaDimensioCombo != null)
                JakosahaDimensioCombo.SelectedItem = nimi;
        }

        private void LataaAktiivinenJakosahaDimensio()
        {
            string? key = HaeAktiivinenJakosahaDimensioKey();
            if (key == null || !TryHaeAktiivisetJakosahaKentat(out var paksuudet, out var leveydet))
                return;

            var asete = HaeValittuDimensio(key, JakosahaDimensioCombo);
            if (asete == null)
                return;

            AsetaTextArvot(paksuudet, asete.Paksuudet);
            AsetaTextArvot(leveydet, asete.Leveydet);
            PiirraVisual();
        }

        private void PoistaAktiivinenJakosahaDimensio()
        {
            string? key = HaeAktiivinenJakosahaDimensioKey();
            var asete = key != null ? HaeValittuDimensio(key, JakosahaDimensioCombo) : null;
            if (key == null || asete == null || !tallennetutDimensiot.TryGetValue(key, out var lista))
                return;

            lista.RemoveAll(x => string.Equals(x.Nimi, asete.Nimi, StringComparison.OrdinalIgnoreCase));
            if (lista.Count == 0)
                tallennetutDimensiot.Remove(key);
            else
                tallennetutDimensiot[key] = lista;

            TallennaTallennus();
            PaivitaDimensioUi();
        }

        private void TallennaPhDimensio(string key, TextBox? nimiTextBox, ComboBox? combo)
        {
            string nimi = nimiTextBox?.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(nimi))
                return;

            TallennaDimensio(key, nimi, new Dictionary<int, string>
            {
                [1] = PhLevinKappaleBox?.Text ?? "",
                [2] = PhKokonaisLeveysBox?.Text ?? "",
                [3] = PhKuivausBox?.Text ?? ""
            }, new Dictionary<int, string>());

            if (combo != null)
                combo.SelectedItem = nimi;
        }

        private void LataaPhDimensio(string key, ComboBox? combo)
        {
            var asete = HaeValittuDimensio(key, combo);
            if (asete == null)
                return;

            if (PhLevinKappaleBox != null && asete.Paksuudet.TryGetValue(1, out var levin))
                PhLevinKappaleBox.Text = levin;
            if (PhKokonaisLeveysBox != null && asete.Paksuudet.TryGetValue(2, out var kokonais))
                PhKokonaisLeveysBox.Text = kokonais;
            if (PhKuivausBox != null && asete.Paksuudet.TryGetValue(3, out var kuivaus))
                PhKuivausBox.Text = kuivaus;

            PiirraVisual();
        }

        private void PoistaPhDimensio(string key, ComboBox? combo)
        {
            var asete = HaeValittuDimensio(key, combo);
            if (asete == null || !tallennetutDimensiot.TryGetValue(key, out var lista))
                return;

            lista.RemoveAll(x => string.Equals(x.Nimi, asete.Nimi, StringComparison.OrdinalIgnoreCase));
            if (lista.Count == 0)
                tallennetutDimensiot.Remove(key);
            else
                tallennetutDimensiot[key] = lista;

            TallennaTallennus();
            PaivitaDimensioUi();
        }

        // ── Tallennus ────────────────────────────────────────────────────────

        private void TallennaTallennus()
        {
            try
            {
                var data = new TallennusData
                {
                    TeraParametrit = teraParametrit.ToDictionary(kvp => kvp.Key, kvp => new TeraParametritDto
                    {
                        OnkoVasenKatinen = kvp.Value.OnkoVasenKatinen,
                        Rako = kvp.Value.Rako,
                        Runko = kvp.Value.Runko,
                        Laippa = kvp.Value.Laippa
                    }),
                    PaksuudetVasen = paksuusTextBoxesVasen.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Text),
                    PaksuudetOikea = paksuusTextBoxesOikea.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Text),
                    PaksuudetYhdistetty = paksuusTextBoxesYhdistetty.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Text),
                    LeveysVasen = leveysTextBoxesVasen.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Text),
                    LeveysOikea = leveysTextBoxesOikea.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Text),
                    LeveysYhdistetty = leveysTextBoxesYhdistetty.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Text),
                    KappaleCount = GetSelectedPieceCount(),
                    YhdistettyCount = GetSelectedYhdistettyCount(),
                    Kuivaus = KuivausTextBox?.Text ?? "0",
                    VasenOn = VasenSahaCheck?.IsChecked == true,
                    OikeaOn = OikeaSahaCheck?.IsChecked == true,
                    Ph1On = Ph1Check?.IsChecked == true,
                    Ph2On = Ph2Check?.IsChecked == true,
                    ProfilointiOn = _profilointiOn,
                    Ph1Offset = _ph1Offset,
                    Ph2Offset = _ph2Offset,
                    OffsetT1 = _offsetT1,
                    OffsetT2 = _offsetT2,
                    OffsetT3 = _offsetT3,
                    OffsetT4 = _offsetT4,
                    OffsetT5 = _offsetT5,
                    OffsetT6 = _offsetT6,
                    ProfOffsetT1 = _profOffsetT1,
                    ProfOffsetT2 = _profOffsetT2,
                    ProfOffsetT3 = _profOffsetT3,
                    ProfOffsetT4 = _profOffsetT4,
                    ProfOffsetT5 = _profOffsetT5,
                    ProfOffsetT6 = _profOffsetT6,
                    ProfOffsetT7 = _profOffsetT7,
                    ProfOffsetT8 = _profOffsetT8,


                    PhLevinKappale = PhLevinKappaleBox?.Text ?? "150",
                    PhKokonaisLeveys = PhKokonaisLeveysBox?.Text ?? "600",
                    PhKuivaus = PhKuivausBox?.Text ?? "0",
                    TurvaEtaisyys = turvaEtaisyys,
                    YhdistettyT1RefKappale = _yhdistettyT1RefKappale,
                    YhdistettyT2RefKappale = _yhdistettyT2RefKappale,
                    KokoRuutuTila = _kokoRuutuTila,
                    TallennetutDimensiot = tallennetutDimensiot.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Select(KopioiDimensioAsete).ToList()),
                    TeraRajat = teraRajat.ToDictionary(kvp => kvp.Key, kvp => new TeraRajatDto
                    {
                        Min = kvp.Value.Min,
                        Max = kvp.Value.Max,
                        Lepopaikka = kvp.Value.Lepopaikka,
                        Vaisto = kvp.Value.Vaisto
                    }),
                    ProfilointiRajat = profilointiRajat.ToDictionary(kvp => kvp.Key, kvp => new ProfilointiRajaDto
                    {
                        Min = kvp.Value.Min,
                        Max = kvp.Value.Max,
                        Lepopaikka = kvp.Value.Lepopaikka
                    }),
                    PhRajat = phRajat.ToDictionary(kvp => kvp.Key, kvp => new PhRajatDto
                    {
                        LepoVasen = kvp.Value.LepoVasen,
                        LepoOikea = kvp.Value.LepoOikea
                    })
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
                foreach (var kvp in data.PhRajat)
                {
                    if (!phRajat.ContainsKey(kvp.Key)) continue;
                    phRajat[kvp.Key].LepoVasen = kvp.Value.LepoVasen;
                    phRajat[kvp.Key].LepoOikea = kvp.Value.LepoOikea;
                }
                foreach (var kvp in data.ProfilointiRajat)
                {
                    if (!profilointiRajat.ContainsKey(kvp.Key))
                        profilointiRajat[kvp.Key] = new ProfilointiRaja();
                    profilointiRajat[kvp.Key].Min = kvp.Value.Min;
                    profilointiRajat[kvp.Key].Max = kvp.Value.Max;
                    profilointiRajat[kvp.Key].Lepopaikka = kvp.Value.Lepopaikka;
                }

                turvaEtaisyys = data.TurvaEtaisyys > 0 ? data.TurvaEtaisyys : 15.0;
                _yhdistettyT1RefKappale = data.YhdistettyT1RefKappale;
                _yhdistettyT2RefKappale = data.YhdistettyT2RefKappale;
                _ph1Offset = data.Ph1Offset;
                _ph2Offset = data.Ph2Offset;
                _kokoRuutuTila = data.KokoRuutuTila;
                _offsetT1 = data.OffsetT1; _offsetT2 = data.OffsetT2; _offsetT3 = data.OffsetT3;
                _offsetT4 = data.OffsetT4; _offsetT5 = data.OffsetT5; _offsetT6 = data.OffsetT6;
                _profOffsetT1 = data.ProfOffsetT1; _profOffsetT2 = data.ProfOffsetT2;
                _profOffsetT3 = data.ProfOffsetT3; _profOffsetT4 = data.ProfOffsetT4;
                _profOffsetT5 = data.ProfOffsetT5; _profOffsetT6 = data.ProfOffsetT6;
                _profOffsetT7 = data.ProfOffsetT7; _profOffsetT8 = data.ProfOffsetT8;
                if (ProfT1OffsetLabel != null) ProfT1OffsetLabel.Text = $"{_profOffsetT1:F1}";
                if (ProfT2OffsetLabel != null) ProfT2OffsetLabel.Text = $"{_profOffsetT2:F1}";
                if (ProfT3OffsetLabel != null) ProfT3OffsetLabel.Text = $"{_profOffsetT3:F1}";
                if (ProfT4OffsetLabel != null) ProfT4OffsetLabel.Text = $"{_profOffsetT4:F1}";
                if (ProfT5OffsetLabel != null) ProfT5OffsetLabel.Text = $"{_profOffsetT5:F1}";
                if (ProfT6OffsetLabel != null) ProfT6OffsetLabel.Text = $"{_profOffsetT6:F1}";
                if (ProfT7OffsetLabel != null) ProfT7OffsetLabel.Text = $"{_profOffsetT7:F1}";
                if (ProfT8OffsetLabel != null) ProfT8OffsetLabel.Text = $"{_profOffsetT8:F1}";

                KappaleCombo.SelectionChanged -= Kappale_Changed;
                YhdistettyCombo.SelectionChanged -= YhdistettyKappale_Changed;
                VasenSahaCheck.Checked -= SahaValinta_Changed; VasenSahaCheck.Unchecked -= SahaValinta_Changed;
                OikeaSahaCheck.Checked -= SahaValinta_Changed; OikeaSahaCheck.Unchecked -= SahaValinta_Changed;
                Ph1Check.Checked -= PhValinta_Changed; Ph1Check.Unchecked -= PhValinta_Changed;
                Ph2Check.Checked -= PhValinta_Changed; Ph2Check.Unchecked -= PhValinta_Changed;

                int idx = data.KappaleCount switch { 2 => 0, 3 => 1, _ => 2 };
                int yIdx = data.YhdistettyCount switch { 3 => 0, 4 => 1, 5 => 2, 6 => 3, _ => 4 };
                if (KappaleCombo != null) KappaleCombo.SelectedIndex = idx;
                if (YhdistettyCombo != null) YhdistettyCombo.SelectedIndex = yIdx;
                if (KuivausTextBox != null) KuivausTextBox.Text = data.Kuivaus;
                if (VasenSahaCheck != null) VasenSahaCheck.IsChecked = data.VasenOn;
                if (OikeaSahaCheck != null) OikeaSahaCheck.IsChecked = data.OikeaOn;
                if (Ph1Check != null) Ph1Check.IsChecked = data.Ph1On;
                if (Ph2Check != null) Ph2Check.IsChecked = data.Ph2On;
                _profilointiOn = data.ProfilointiOn;
                if (ProfilointiCheck != null) ProfilointiCheck.IsChecked = _profilointiOn;
                if (PhLevinKappaleBox != null) PhLevinKappaleBox.Text = data.PhLevinKappale;
                if (PhKokonaisLeveysBox != null) PhKokonaisLeveysBox.Text = data.PhKokonaisLeveys;
                if (PhKuivausBox != null) PhKuivausBox.Text = data.PhKuivaus;
                if (KokoRuutuCheck != null) KokoRuutuCheck.IsChecked = _kokoRuutuTila;

                tallennetutDimensiot.Clear();
                foreach (var kvp in data.TallennetutDimensiot)
                    tallennetutDimensiot[kvp.Key] = kvp.Value.Select(KopioiDimensioAsete).ToList();

                KappaleCombo.SelectionChanged += Kappale_Changed;
                YhdistettyCombo.SelectionChanged += YhdistettyKappale_Changed;
                VasenSahaCheck.Checked += SahaValinta_Changed; VasenSahaCheck.Unchecked += SahaValinta_Changed;
                OikeaSahaCheck.Checked += SahaValinta_Changed; OikeaSahaCheck.Unchecked += SahaValinta_Changed;
                Ph1Check.Checked += PhValinta_Changed; Ph1Check.Unchecked += PhValinta_Changed;
                Ph2Check.Checked += PhValinta_Changed; Ph2Check.Unchecked += PhValinta_Changed;

                PaivitaNakymat();
                LuoParametriKontrollit();
                PaivitaDimensioUi();
                PaivitaKokoRuutuTila();

                Dispatcher.InvokeAsync(() =>
                {
                    foreach (var kvp in data.PaksuudetVasen)
                        if (paksuusTextBoxesVasen.TryGetValue(kvp.Key, out var tb)) tb.Text = kvp.Value;
                    foreach (var kvp in data.PaksuudetOikea)
                        if (paksuusTextBoxesOikea.TryGetValue(kvp.Key, out var tb)) tb.Text = kvp.Value;
                    foreach (var kvp in data.PaksuudetYhdistetty)
                        if (paksuusTextBoxesYhdistetty.TryGetValue(kvp.Key, out var tb)) tb.Text = kvp.Value;
                    foreach (var kvp in data.LeveysVasen)
                        if (leveysTextBoxesVasen.TryGetValue(kvp.Key, out var tb)) tb.Text = kvp.Value;
                    foreach (var kvp in data.LeveysOikea)
                        if (leveysTextBoxesOikea.TryGetValue(kvp.Key, out var tb)) tb.Text = kvp.Value;
                    foreach (var kvp in data.LeveysYhdistetty)
                        if (leveysTextBoxesYhdistetty.TryGetValue(kvp.Key, out var tb)) tb.Text = kvp.Value;
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
            leveysTextBoxesVasen.Clear();
            leveysTextBoxesOikea.Clear();
            leveysTextBoxesYhdistetty.Clear();

            bool vasenOn = VasenSahaCheck?.IsChecked == true;
            bool oikeaOn = OikeaSahaCheck?.IsChecked == true;

            if (ErillinenPanel != null)
                ErillinenPanel.Visibility = (!vasenOn && !oikeaOn) ? Visibility.Collapsed
                    : OnYhdistettyTila() ? Visibility.Collapsed : Visibility.Visible;
            if (YhdistettyPanel != null) YhdistettyPanel.Visibility = OnYhdistettyTila() ? Visibility.Visible : Visibility.Collapsed;
            if (HalkaisuVasenPanel != null) HalkaisuVasenPanel.Visibility = vasenOn ? Visibility.Visible : Visibility.Collapsed;
            if (HalkaisuOikeaPanel != null) HalkaisuOikeaPanel.Visibility = oikeaOn ? Visibility.Visible : Visibility.Collapsed;
            PaivitaHalkaisuNakyvyys();

            if (!vasenOn && !oikeaOn)
            {

                return;
            }

            if (OnYhdistettyTila()) LuoYhdistettyKontrollit();
            else LuoErillisetKontrollit();

            PaivitaProfRefPanel();
            PaivitaDimensioUi();
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
                    var (card, paksuusTb, leveysTb) = LuoPaksuusKorttiKapea($"K{i}", i);
                    vasenStack.Children.Add(card);
                    paksuusTextBoxesVasen[i] = paksuusTb;
                    leveysTextBoxesVasen[i] = leveysTb;
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
                    var (card, paksuusTb, leveysTb) = LuoPaksuusKorttiKapea($"K{i}", i);
                    oikeaStack.Children.Add(card);
                    paksuusTextBoxesOikea[i] = paksuusTb;
                    leveysTextBoxesOikea[i] = leveysTb;
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
                var (card, paksuusTb, leveysTb) = LuoPaksuusKorttiKapea($"K{i}", i);
                root.Children.Add(card);
                paksuusTextBoxesYhdistetty[i] = paksuusTb;
                leveysTextBoxesYhdistetty[i] = leveysTb;
            }

            if (count >= 5)
            {
                var refBorder = new Border
                {
                    Margin = new Thickness(0, 8, 0, 0),
                    Background = new SolidColorBrush(Color.FromRgb(40, 40, 40)),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(8, 6, 8, 6)
                };
                var refStack = new StackPanel();
                refStack.Children.Add(new TextBlock
                {
                    Text = "Profiloinnin referenssikappale:",
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                    Margin = new Thickness(0, 0, 0, 4)
                });

                int defIdx = (count == 7) ? 3 : 2; // 0-based index: 3=>K4 for 7kpl, 2=>K3 for 5/6kpl

                // T1 reference row
                var t1Grid = new Grid { Margin = new Thickness(0, 0, 0, 4) };
                t1Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
                t1Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var t1Lbl = new TextBlock
                {
                    Text = "T1 ref:",
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(255, 107, 107)),
                    VerticalAlignment = VerticalAlignment.Center
                };
                var t1Combo = new ComboBox
                {
                    Height = 26,
                    FontSize = 11,
                    Background = new SolidColorBrush(Color.FromRgb(50, 50, 50)),
                    Foreground = Brushes.White
                };
                for (int i = 1; i <= count; i++)
                    t1Combo.Items.Add(new ComboBoxItem { Content = $"K{i}" });
                int t1Sel = (_yhdistettyT1RefKappale >= 1 && _yhdistettyT1RefKappale <= count)
                    ? _yhdistettyT1RefKappale - 1
                    : defIdx;
                t1Combo.SelectedIndex = t1Sel;
                t1Combo.SelectionChanged += (s, e) =>
                {
                    _yhdistettyT1RefKappale = t1Combo.SelectedIndex + 1;
                    PiirraProfilointiCanvas();  // ← vain profilointi päivittyy
                };

                Grid.SetColumn(t1Lbl, 0); Grid.SetColumn(t1Combo, 1);
                t1Grid.Children.Add(t1Lbl); t1Grid.Children.Add(t1Combo);
                refStack.Children.Add(t1Grid);

                // T2 reference row
                var t2Grid = new Grid();
                t2Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
                t2Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var t2Lbl = new TextBlock
                {
                    Text = "T2 ref:",
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(107, 203, 119)),
                    VerticalAlignment = VerticalAlignment.Center
                };
                var t2Combo = new ComboBox
                {
                    Height = 26,
                    FontSize = 11,
                    Background = new SolidColorBrush(Color.FromRgb(50, 50, 50)),
                    Foreground = Brushes.White
                };
                for (int i = 1; i <= count; i++)
                    t2Combo.Items.Add(new ComboBoxItem { Content = $"K{i}" });
                int t2Sel = (_yhdistettyT2RefKappale >= 1 && _yhdistettyT2RefKappale <= count)
                    ? _yhdistettyT2RefKappale - 1
                    : defIdx;
                t2Combo.SelectedIndex = t2Sel;
                t2Combo.SelectionChanged += (s, e) =>
                {
                    _yhdistettyT2RefKappale = t2Combo.SelectedIndex + 1;
                    PiirraProfilointiCanvas();  // ← vain profilointi
                };

                Grid.SetColumn(t2Lbl, 0); Grid.SetColumn(t2Combo, 1);
                t2Grid.Children.Add(t2Lbl); t2Grid.Children.Add(t2Combo);
                refStack.Children.Add(t2Grid);

                refBorder.Child = refStack;
                root.Children.Add(refBorder);
            }

            PaksuusPanel.Children.Add(root);
        }

        private void PaivitaProfRefPanel()
        {
            if (ProfRefPanel == null || ProfRefStack == null) return;

            int count = GetSelectedYhdistettyCount();
            bool nayta = OnYhdistettyTila() && count >= 5;
            ProfRefPanel.Visibility = nayta ? Visibility.Visible : Visibility.Collapsed;
            ProfRefStack.Children.Clear();
            if (!nayta) return;

            ProfRefStack.Children.Add(new TextBlock
            {
                Text = "Profiloinnin referenssikappale:",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                Margin = new Thickness(0, 0, 0, 4)
            });

            int defIdx = (count == 7) ? 3 : 2; // 0-based index: 3=>K4 for 7kpl, 2=>K3 for 5/6kpl

            var t1Grid = new Grid { Margin = new Thickness(0, 0, 0, 4) };
            t1Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
            t1Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var t1Lbl = new TextBlock
            {
                Text = "T1 ref:",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(255, 107, 107)),
                VerticalAlignment = VerticalAlignment.Center
            };
            var t1Combo = new ComboBox
            {
                Height = 26,
                FontSize = 11,
                Background = new SolidColorBrush(Color.FromRgb(50, 50, 50)),
                Foreground = Brushes.White
            };
            for (int i = 1; i <= count; i++)
                t1Combo.Items.Add(new ComboBoxItem { Content = $"K{i}" });
            int t1Sel = (_yhdistettyT1RefKappale >= 1 && _yhdistettyT1RefKappale <= count)
                ? _yhdistettyT1RefKappale - 1
                : defIdx;
            t1Combo.SelectedIndex = t1Sel;
            t1Combo.SelectionChanged += (_, __) =>
            {
                _yhdistettyT1RefKappale = t1Combo.SelectedIndex + 1;
                PiirraProfilointiCanvas();
            };
            Grid.SetColumn(t1Lbl, 0);
            Grid.SetColumn(t1Combo, 1);
            t1Grid.Children.Add(t1Lbl);
            t1Grid.Children.Add(t1Combo);
            ProfRefStack.Children.Add(t1Grid);

            var t2Grid = new Grid();
            t2Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
            t2Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var t2Lbl = new TextBlock
            {
                Text = "T2 ref:",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(107, 203, 119)),
                VerticalAlignment = VerticalAlignment.Center
            };
            var t2Combo = new ComboBox
            {
                Height = 26,
                FontSize = 11,
                Background = new SolidColorBrush(Color.FromRgb(50, 50, 50)),
                Foreground = Brushes.White
            };
            for (int i = 1; i <= count; i++)
                t2Combo.Items.Add(new ComboBoxItem { Content = $"K{i}" });
            int t2Sel = (_yhdistettyT2RefKappale >= 1 && _yhdistettyT2RefKappale <= count)
                ? _yhdistettyT2RefKappale - 1
                : defIdx;
            t2Combo.SelectedIndex = t2Sel;
            t2Combo.SelectionChanged += (_, __) =>
            {
                _yhdistettyT2RefKappale = t2Combo.SelectedIndex + 1;
                PiirraProfilointiCanvas();
            };
            Grid.SetColumn(t2Lbl, 0);
            Grid.SetColumn(t2Combo, 1);
            t2Grid.Children.Add(t2Lbl);
            t2Grid.Children.Add(t2Combo);
            ProfRefStack.Children.Add(t2Grid);
        }

        private (Border card, TextBox paksuusTb, TextBox leveysTb) LuoPaksuusKorttiKapea(string otsikko, int tagId)
        {
            var border = new Border
            {
                Margin = new Thickness(0, 0, 0, 4),
                Background = new SolidColorBrush(Color.FromRgb(50, 50, 50)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(6, 4, 6, 4)
            };
            var sp = new StackPanel();

            var grid1 = new Grid();
            grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28) });
            grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(22) });

            var lbl = new TextBlock
            {
                Text = otsikko,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(255, 217, 61)),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 11
            };
            var paksuusTb = new TextBox
            {
                Height = 26,
                Padding = new Thickness(4, 0, 4, 0),
                Text = "30",
                Tag = tagId,
                FontSize = 12,
                VerticalContentAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(Color.FromRgb(35, 35, 35)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(100, 100, 100))
            };
            var mmLbl = new TextBlock
            {
                Text = "mm",
                FontSize = 9,
                Foreground = new SolidColorBrush(Color.FromRgb(140, 140, 140)),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            Grid.SetColumn(lbl, 0); Grid.SetColumn(paksuusTb, 1); Grid.SetColumn(mmLbl, 2);
            grid1.Children.Add(lbl); grid1.Children.Add(paksuusTb); grid1.Children.Add(mmLbl);

            var grid2 = new Grid();
            grid2.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28) });
            grid2.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid2.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(22) });

            var leveysLbl = new TextBlock
            {
                Text = "lev",
                FontSize = 9,
                Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
                VerticalAlignment = VerticalAlignment.Center
            };
            var leveysTb = new TextBox
            {
                Height = 24,
                Padding = new Thickness(4, 0, 4, 0),
                Text = "100",
                Tag = tagId,
                FontSize = 11,
                VerticalContentAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(80, 80, 80))
            };
            var mmLbl2 = new TextBlock
            {
                Text = "mm",
                FontSize = 9,
                Foreground = new SolidColorBrush(Color.FromRgb(120, 120, 120)),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            Grid.SetColumn(leveysLbl, 0); Grid.SetColumn(leveysTb, 1); Grid.SetColumn(mmLbl2, 2);
            grid2.Children.Add(leveysLbl); grid2.Children.Add(leveysTb); grid2.Children.Add(mmLbl2);

            paksuusTb.TextChanged += (s, e) =>
            {
                bool valid = double.TryParse(paksuusTb.Text.Replace(",", "."),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out double v) && v > 0;
                paksuusTb.BorderBrush = valid
                    ? new SolidColorBrush(Color.FromRgb(100, 100, 100))
                    : new SolidColorBrush(Color.FromRgb(220, 83, 83));
                if (!valid) SetStatus("⚠  Paksuuden täytyy olla positiivinen luku", Colors.Orange);
                else SetStatus("Valmis", Colors.LightGray);
            };
            paksuusTb.TextChanged += PaksuusTextBox_Changed;
            leveysTb.TextChanged += PaksuusTextBox_Changed;

            sp.Children.Add(grid1); sp.Children.Add(grid2);
            border.Child = sp;
            return (border, paksuusTb, leveysTb);
        }

        // ── Event-handlerit ──────────────────────────────────────────────────

        private int GetHalkaisuTeraOikea() => HalkaisuOikeaCombo?.SelectedIndex == 1 ? 2 : 4;
        private int GetHalkaisuTeraVasen() => HalkaisuVasenCombo?.SelectedIndex == 1 ? 1 : 3;

        private void PaksuusTextBox_Changed(object sender, TextChangedEventArgs e) => PiirraVisual();
        private void Halkaisu_Changed(object sender, SelectionChangedEventArgs e) => PiirraVisual();
        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e) => PiirraVisual();
        private void Ph_Changed(object sender, TextChangedEventArgs e) => PiirraVisual();
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e) { PiirraPhCanvasit(); PiirraProfilointiCanvas(); }
        private void PhCanvas_SizeChanged(object sender, SizeChangedEventArgs e) => PiirraPhCanvasit();
        private void ProfilointiCanvas_SizeChanged(object sender, SizeChangedEventArgs e) => PiirraProfilointiCanvas();

        private void ProfLaheta_Click(object sender, RoutedEventArgs e)
        {
            SetStatus("✓  Profilointi lähetetty logiikkaan", Colors.LightGreen);
        }

        private void PhValinta_Changed(object sender, RoutedEventArgs e)
        {
            PaivitaPhLukitus();
            PaivitaDimensioUi();
            PiirraVisual();
            PiirraPhCanvasit();
        }

        private void ProfilointiValinta_Changed(object sender, RoutedEventArgs e)
        {
            _profilointiOn = ProfilointiCheck?.IsChecked == true;
            PiirraProfilointiCanvas();
        }

        private void KokoRuutuTila_Changed(object sender, RoutedEventArgs e)
        {
            _kokoRuutuTila = KokoRuutuCheck?.IsChecked != false;
            PaivitaKokoRuutuTila();
        }

        private void TallennaJakosahaDimensio_Click(object sender, RoutedEventArgs e) => TallennaAktiivinenJakosahaDimensio();
        private void LataaJakosahaDimensio_Click(object sender, RoutedEventArgs e) => LataaAktiivinenJakosahaDimensio();
        private void PoistaJakosahaDimensio_Click(object sender, RoutedEventArgs e) => PoistaAktiivinenJakosahaDimensio();
        private void TallennaPh1Dimensio_Click(object sender, RoutedEventArgs e) => TallennaPhDimensio("ph1", Ph1DimensioNimiBox, Ph1DimensioCombo);
        private void LataaPh1Dimensio_Click(object sender, RoutedEventArgs e) => LataaPhDimensio("ph1", Ph1DimensioCombo);
        private void PoistaPh1Dimensio_Click(object sender, RoutedEventArgs e) => PoistaPhDimensio("ph1", Ph1DimensioCombo);
        private void TallennaPh2Dimensio_Click(object sender, RoutedEventArgs e) => TallennaPhDimensio("ph2", Ph2DimensioNimiBox, Ph2DimensioCombo);
        private void LataaPh2Dimensio_Click(object sender, RoutedEventArgs e) => LataaPhDimensio("ph2", Ph2DimensioCombo);
        private void PoistaPh2Dimensio_Click(object sender, RoutedEventArgs e) => PoistaPhDimensio("ph2", Ph2DimensioCombo);

        private void SahaValinta_Changed(object sender, RoutedEventArgs e)
        {
            if (HalkaisuVasenPanel != null)
                HalkaisuVasenPanel.Visibility = VasenSahaCheck?.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            if (HalkaisuOikeaPanel != null)
                HalkaisuOikeaPanel.Visibility = OikeaSahaCheck?.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            PaivitaHalkaisuNakyvyys();
            PaivitaNakymat();
            PaivitaProfRefPanel();
            PaivitaPhLukitus();
            PiirraVisual();
        }

        private void Kuivaus_Changed(object sender, TextChangedEventArgs e)
        {
            string text = KuivausTextBox?.Text ?? "0";
            if (!double.TryParse(text.Replace(",", "."), NumberStyles.Float,
                CultureInfo.InvariantCulture, out double val) || val < 0)
                SetStatus("⚠  Kuivausprosentin täytyy olla positiivinen numero", Colors.Orange);
            else SetStatus("Valmis", Colors.LightGray);
            PiirraVisual();
        }

        private void Kappale_Changed(object sender, SelectionChangedEventArgs e)
        {
            UpdateKappaleInfo();
            PaivitaHalkaisuNakyvyys();
            LuoParametriKontrollit();
            PaivitaDimensioUi();
            PiirraVisual();
        }

        private void YhdistettyKappale_Changed(object sender, SelectionChangedEventArgs e)
        {

            _yhdistettyT1RefKappale = 0;  // ← lisää
            _yhdistettyT2RefKappale = 0;  // ← lisää
            if (YhdistettyInfo != null) YhdistettyInfo.Text = $"{GetSelectedYhdistettyCount()} kpl";
            LuoParametriKontrollit();
            PaivitaDimensioUi();
            PaivitaProfRefPanel();
            PiirraVisual();
        }

        private void Laheta_Click(object sender, RoutedEventArgs e)
        {
            if (StatusTextBlock?.Text.StartsWith("⚠") == true)
            {
                MessageBox.Show("Rajoja on ylitetty — arvoja ei lähetetä logiikkaan!\n\n" + StatusTextBlock.Text,
                    "Lähetys estetty", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            SetStatus("✓  Lähetetty logiikkaan", Colors.LightGreen);

            // Lähetä myös WinCC:hen jos ikkuna on auki ja yhdistetty
            _winCCWindow?.LahetaKuvioWinCC();
        }

        private void AvaaTerapAsetukset_Click(object sender, RoutedEventArgs e)
        {
            if (teraParametrit == null || teraParametrit.Count == 0)
            {
                MessageBox.Show("Teräparametrit ei ole alustettu!", "Virhe", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show($"Teräasetusikkuna kaatui:\n\n{ex.Message}\n\n{ex.StackTrace}",
                    "Virhe", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AvaaRajatAsetukset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var rajatWindow = new RajatAsetuksetWindow(teraRajat, profilointiRajat, phRajat, turvaEtaisyys) { Owner = this };
                rajatWindow.OnRajatChanged += paivitetyt =>
                {
                    foreach (var kvp in paivitetyt)
                    {
                        if (!teraRajat.ContainsKey(kvp.Key)) continue;
                        teraRajat[kvp.Key].Min = kvp.Value.Min; teraRajat[kvp.Key].Max = kvp.Value.Max;
                        teraRajat[kvp.Key].Lepopaikka = kvp.Value.Lepopaikka; teraRajat[kvp.Key].Vaisto = kvp.Value.Vaisto;
                    }
                    PiirraVisual();
                    SetStatus("✓  Rajasetukset päivitetty", Colors.LightGreen);
                };
                rajatWindow.OnProfilointiRajatChanged += paivitetyt =>
                {
                    foreach (var kvp in paivitetyt)
                    {
                        if (!profilointiRajat.ContainsKey(kvp.Key))
                            profilointiRajat[kvp.Key] = new ProfilointiRaja();
                        profilointiRajat[kvp.Key].Min = kvp.Value.Min;
                        profilointiRajat[kvp.Key].Max = kvp.Value.Max;
                        profilointiRajat[kvp.Key].Lepopaikka = kvp.Value.Lepopaikka;
                    }
                    PiirraProfilointiCanvas();
                    SetStatus("✓  Profiloinnin rajasetukset päivitetty", Colors.LightGreen);
                };
                rajatWindow.OnPhRajatChanged += (paivitetytPh, uusiTurva) =>
                {
                    foreach (var kvp in paivitetytPh)
                    {
                        if (!phRajat.ContainsKey(kvp.Key)) continue;
                        phRajat[kvp.Key].LepoVasen = kvp.Value.LepoVasen;
                        phRajat[kvp.Key].LepoOikea = kvp.Value.LepoOikea;
                    }
                    turvaEtaisyys = uusiTurva;
                    PiirraVisual();
                    SetStatus("✓  PH-rajat päivitetty", Colors.LightGreen);
                };
                rajatWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Rajoitusikkuna kaatui:\n\n{ex.Message}\n\n{ex.StackTrace}",
                    "Virhe", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AvaaWinCCIntegration_Click(object sender, RoutedEventArgs e)
        {
            if (_winCCWindow == null || !_winCCWindow.IsVisible)
            {
                _winCCWindow = new WinCCIntegrationWindow { Owner = this };
                _winCCWindow.Show();
                KeraaKaikkiPlcArvot(); 
            }
            else
            {
                _winCCWindow.Activate();
            }
        }

        // ── PH laskenta ──────────────────────────────────────────────────────

        private (double ph1Vasen, double ph1Oikea, double ph2Vasen, double ph2Oikea, double kuivaPh1, double kuivaPh2, double tuore1, double tuore2) LaskePhArvot()
        {
            bool jakosahaOn = OnJakosahaKaytossa();
            double levinKappale, kokonaisLeveys;
            double kuivaPh1, kuivaPh2;

            if (jakosahaOn)
            {
                double.TryParse(KuivausTextBox?.Text.Replace(",", ".") ?? "0",
                    NumberStyles.Float, CultureInfo.InvariantCulture, out double phKuivaus);
                double kerroin = 1.0 + phKuivaus / 100.0;

                var leveydet = GetLeveysValues();
                levinKappale = leveydet.Count > 0 ? leveydet.Max() * kerroin : 0;
                kuivaPh1 = leveydet.Count > 0 ? leveydet.Max() : 0;

                double RakoT(int num) => teraParametrit.TryGetValue(num, out var t) ? t.Rako : 4.0;

                if (OnYhdistettyTila())
                {
                    var p = GetThicknessValuesYhdistetty().Select(x => x * kerroin).ToList();
                    int n = GetSelectedYhdistettyCount();
                    var rakoJarjestys = n switch { 3 => new[] { 1, 2 }, 4 => new[] { 1, 2, 4 }, 5 => new[] { 1, 3, 2, 4 }, 6 => new[] { 3, 1, 2, 4, 6 }, 7 => new[] { 5, 3, 1, 2, 4, 6 }, _ => Array.Empty<int>() };
                    kokonaisLeveys = p.Sum() + rakoJarjestys.Select(t => RakoT(t)).Sum();
                }
                else if (VasenSahaCheck?.IsChecked == true)
                {
                    var p = GetThicknessValuesVasen().Select(x => x * kerroin).ToList();
                    kokonaisLeveys = p.Count switch
                    {
                        1 => p[0],
                        2 => p[0] + RakoT(1) + p[1],
                        3 => p[0] + RakoT(1) + p[1] + RakoT(3) + p[2],
                        4 => p[0] + RakoT(1) + p[1] + RakoT(3) + p[2] + RakoT(5) + p[3],
                        _ => p.Sum()
                    };
                }
                else
                {
                    var p = GetThicknessValuesOikea().Select(x => x * kerroin).ToList();
                    kokonaisLeveys = p.Count switch
                    {
                        1 => p[0],
                        2 => p[0] + RakoT(2) + p[1],
                        3 => p[0] + RakoT(2) + p[1] + RakoT(4) + p[2],
                        4 => p[0] + RakoT(2) + p[1] + RakoT(4) + p[2] + RakoT(6) + p[3],
                        _ => p.Sum()
                    };
                }

                kuivaPh2 = kokonaisLeveys / kerroin;

                if (PhLevinKappaleBox != null)
                {
                    PhLevinKappaleBox.TextChanged -= Ph_Changed;
                    PhLevinKappaleBox.Text = levinKappale.ToString("F1", CultureInfo.InvariantCulture);
                    PhLevinKappaleBox.TextChanged += Ph_Changed;
                }
                if (PhKokonaisLeveysBox != null)
                {
                    PhKokonaisLeveysBox.TextChanged -= Ph_Changed;
                    PhKokonaisLeveysBox.Text = kokonaisLeveys.ToString("F1", CultureInfo.InvariantCulture);
                    PhKokonaisLeveysBox.TextChanged += Ph_Changed;
                }
            }
            else
            {
                double.TryParse(PhLevinKappaleBox?.Text.Replace(",", ".") ?? "150", NumberStyles.Float, CultureInfo.InvariantCulture, out levinKappale);
                double.TryParse(PhKokonaisLeveysBox?.Text.Replace(",", ".") ?? "600", NumberStyles.Float, CultureInfo.InvariantCulture, out kokonaisLeveys);
                kuivaPh1 = levinKappale;
                kuivaPh2 = kokonaisLeveys;
                double.TryParse(PhKuivausBox?.Text.Replace(",", ".") ?? "0", NumberStyles.Float, CultureInfo.InvariantCulture, out double phKuivaus);
                double kerroin = 1.0 + phKuivaus / 100.0;
                levinKappale *= kerroin;
                kokonaisLeveys *= kerroin;
            }

            double ph1V = levinKappale / 2.0 + _ph1Offset / 2.0;
            double ph1O = levinKappale / 2.0 + _ph1Offset / 2.0;
            double ph2V = kokonaisLeveys / 2.0 + _ph2Offset / 2.0;
            double ph2O = kokonaisLeveys / 2.0 + _ph2Offset / 2.0;
            return (ph1V, ph1O, ph2V, ph2O, kuivaPh1, kuivaPh2, levinKappale, kokonaisLeveys);
        }
        private List<double> GetLeveysValues()
        {
            var result = new List<double>();
            var boxes = OnYhdistettyTila() ? leveysTextBoxesYhdistetty
                       : VasenSahaCheck?.IsChecked == true ? leveysTextBoxesVasen
                       : leveysTextBoxesOikea;
            foreach (var kvp in boxes.OrderBy(k => k.Key))
                if (double.TryParse(kvp.Value.Text.Replace(",", "."),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out double v) && v > 0)
                    result.Add(v);
            return result;
        }
        private List<double> GetPaksuusValues()
        {
            var result = new List<double>();
            var boxes = OnYhdistettyTila() ? paksuusTextBoxesYhdistetty
                       : VasenSahaCheck?.IsChecked == true ? paksuusTextBoxesVasen
                       : paksuusTextBoxesOikea;
            foreach (var kvp in boxes.OrderBy(k => k.Key))
                if (double.TryParse(kvp.Value.Text.Replace(",", "."),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out double v) && v > 0)
                    result.Add(v);
            return result;
        }

        // ── PH Canvas piirto ─────────────────────────────────────────────────

        private void PiirraPhCanvasit()
        {
            double ph1V = 0, ph1O = 0, ph2V = 0, ph2O = 0, kuivaPh1 = 0, kuivaPh2 = 0, tuore1 = 0, tuore2 = 0;
            if (Ph1Check?.IsChecked == true || Ph2Check?.IsChecked == true)
                (ph1V, ph1O, ph2V, ph2O, kuivaPh1, kuivaPh2, tuore1, tuore2) = LaskePhArvot();

            double ph1Leveys = ph1V + ph1O;
            double ph2Leveys = ph2V + ph2O;
            double halkaisija = Math.Sqrt(ph1Leveys * ph1Leveys + ph2Leveys * ph2Leveys);

            double lepoPh1V = phRajat.TryGetValue("PH1V", out var lp1v) ? lp1v.LepoVasen : 0.0;
            double lepoPh1O = phRajat.TryGetValue("PH1O", out var lp1o) ? lp1o.LepoOikea : 0.0;
            double lepoPh2V = phRajat.TryGetValue("PH2V", out var lp2v) ? lp2v.LepoVasen : 0.0;
            double lepoPh2O = phRajat.TryGetValue("PH2O", out var lp2o) ? lp2o.LepoOikea : 0.0;

            bool ph1On = Ph1Check?.IsChecked == true;
            bool ph2On = Ph2Check?.IsChecked == true;

            double use1V = ph1On ? ph1V : lepoPh1V;
            double use1O = ph1On ? ph1O : lepoPh1O;
            double use2V = ph2On ? ph2V : lepoPh2V;
            double use2O = ph2On ? ph2O : lepoPh2O;

            double use1Leveys = use1V + use1O;
            double use2Leveys = use2V + use2O;
            double useHalkaisija = Math.Sqrt(use1Leveys * use1Leveys + use2Leveys * use2Leveys);

            double W1 = Ph1Canvas?.ActualWidth > 20 ? Ph1Canvas.ActualWidth : 400;
            double H1 = Ph1Canvas?.ActualHeight > 20 ? Ph1Canvas.ActualHeight : 400;
            double W2 = Ph2Canvas?.ActualWidth > 20 ? Ph2Canvas.ActualWidth : 400;
            double H2 = Ph2Canvas?.ActualHeight > 20 ? Ph2Canvas.ActualHeight : 400;

            double scale1X = W1 * 0.45 / Math.Max(use1Leveys / 2.0, 1.0);
            double scale1Y = H1 * 0.42 / Math.Max(useHalkaisija / 2.0, 1.0);
            double scale2X = W2 * 0.38 / Math.Max(useHalkaisija / 2.0, 1.0);
            double scale2Y = H2 * 0.38 / Math.Max(useHalkaisija / 2.0, 1.0);
            double scale = Math.Min(Math.Min(scale1X, scale1Y), Math.Min(scale2X, scale2Y));

            if (Ph1Canvas != null)
            {
                if (ph1On)
                    PiirraPh1Canvas(Ph1Canvas, use1V, use1O, use2Leveys, useHalkaisija, scale, kuivaPh1, tuore1);
                else
                    PiirraPhLepopaikkaCanvas(Ph1Canvas, use1V, use1O, true);
            }

            if (Ph2Canvas != null)
            {
                if (ph2On)
                    PiirraPh2Canvas(Ph2Canvas, use2V, use2O, use1Leveys, useHalkaisija, scale, kuivaPh2, tuore2,
    OnJakosahaKaytossa());
                else
                    PiirraPhLepopaikkaCanvas(Ph2Canvas, use2V, use2O, false);
            }
        }
        private static void PiirraPh1Canvas(Canvas canvas,
    double ph1V, double ph1O, double ph2Leveys,
    double halkaisija, double scale, double kuivaLeveys, double tuoreLeveysIlmanOffset)
        {
            canvas.Children.Clear();
            double W = canvas.ActualWidth > 20 ? canvas.ActualWidth : 400;
            double H = canvas.ActualHeight > 20 ? canvas.ActualHeight : 400;
            double cx = W / 2.0, cy = H / 2.0;
            double ph1Leveys = ph1V + ph1O;
            double tukkiR = halkaisija / 2.0 * scale;
            double pelkkaW = ph1Leveys * scale;
            double pelkkaH = ph2Leveys * scale;
            double pelkkaX = cx - pelkkaW / 2.0;
            double pelkkaY = cy - pelkkaH / 2.0;
            var taustaBrush = new SolidColorBrush(Color.FromRgb(18, 15, 12));
            canvas.Children.Add(new Rectangle { Width = W, Height = H, Fill = taustaBrush });
            PiirraAsteikko(canvas, W, H, W / 2.0, scale);
            canvas.Children.Add(new Ellipse
            {
                Width = tukkiR * 2,
                Height = tukkiR * 2,
                Fill = new SolidColorBrush(Color.FromArgb(190, 175, 125, 75)),
                Stroke = new SolidColorBrush(Color.FromRgb(210, 170, 110)),
                StrokeThickness = 1.5
            }.Also(e => { Canvas.SetLeft(e, cx - tukkiR); Canvas.SetTop(e, cy - tukkiR); }));
            canvas.Children.Add(new Rectangle
            {
                Width = pelkkaW,
                Height = pelkkaH,
                Fill = new SolidColorBrush(Color.FromArgb(210, 140, 95, 50)),
                Stroke = new SolidColorBrush(Color.FromRgb(255, 210, 60)),
                StrokeThickness = 2.0
            }.Also(r => { Canvas.SetLeft(r, pelkkaX); Canvas.SetTop(r, pelkkaY); }));
            canvas.Children.Add(new Rectangle
            {
                Width = Math.Max(0, pelkkaX - (cx - tukkiR)),
                Height = pelkkaH,
                Fill = taustaBrush
            }.Also(r => { Canvas.SetLeft(r, cx - tukkiR); Canvas.SetTop(r, pelkkaY); }));
            canvas.Children.Add(new Rectangle
            {
                Width = Math.Max(0, (cx + tukkiR) - (pelkkaX + pelkkaW)),
                Height = pelkkaH,
                Fill = taustaBrush
            }.Also(r => { Canvas.SetLeft(r, pelkkaX + pelkkaW); Canvas.SetTop(r, pelkkaY); }));
            canvas.Children.Add(new Line
            {
                X1 = cx,
                Y1 = cy - tukkiR - 10,
                X2 = cx,
                Y2 = cy + tukkiR + 10,
                Stroke = new SolidColorBrush(Color.FromRgb(79, 195, 247)),
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 4, 3 }
            });
            canvas.Children.Add(new Line
            {
                X1 = cx - tukkiR - 10,
                Y1 = cy,
                X2 = cx + tukkiR + 10,
                Y2 = cy,
                Stroke = new SolidColorBrush(Color.FromRgb(79, 195, 247)),
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 4, 3 }
            });
            double ph1VX = cx - ph1V * scale;
            double ph1OX = cx + ph1O * scale;
            var sahaBrush = new SolidColorBrush(Color.FromRgb(255, 80, 80));
            canvas.Children.Add(new Line { X1 = ph1VX, Y1 = pelkkaY - 10, X2 = ph1VX, Y2 = pelkkaY + pelkkaH + 10, Stroke = sahaBrush, StrokeThickness = 2.5 });
            canvas.Children.Add(new Line { X1 = ph1OX, Y1 = pelkkaY - 10, X2 = ph1OX, Y2 = pelkkaY + pelkkaH + 10, Stroke = sahaBrush, StrokeThickness = 2.5 });

            var tuoreLbl = new TextBlock
            {
                Text = $"{tuoreLeveysIlmanOffset:F1} mm",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(255, 210, 60)),
                TextAlignment = TextAlignment.Center,
                Width = Math.Max(80, pelkkaW)
            };
            Canvas.SetLeft(tuoreLbl, cx - tuoreLbl.Width / 2.0);
            Canvas.SetTop(tuoreLbl, cy - 10);
            canvas.Children.Add(tuoreLbl);
            double arrowY = cy + tukkiR + 22;
            var ph1Br = new SolidColorBrush(Color.FromRgb(255, 210, 60));
            canvas.Children.Add(new Line { X1 = ph1VX, Y1 = arrowY, X2 = ph1OX, Y2 = arrowY, Stroke = ph1Br, StrokeThickness = 1 });
            canvas.Children.Add(new Line { X1 = ph1VX, Y1 = arrowY - 4, X2 = ph1VX, Y2 = arrowY + 4, Stroke = ph1Br, StrokeThickness = 1 });
            canvas.Children.Add(new Line { X1 = ph1OX, Y1 = arrowY - 4, X2 = ph1OX, Y2 = arrowY + 4, Stroke = ph1Br, StrokeThickness = 1 });
            var mittaLbl = new TextBlock
            {
                Text = $"Ph1 leveys  {ph1Leveys:F1} mm  (kuiva: {kuivaLeveys:F1} mm)",
                FontSize = 11,
                Foreground = ph1Br,
                TextAlignment = TextAlignment.Center,
                Width = Math.Max(120, pelkkaW + 20)
            };


            Canvas.SetLeft(mittaLbl, cx - mittaLbl.Width / 2.0);
            Canvas.SetTop(mittaLbl, arrowY + 5);
            canvas.Children.Add(mittaLbl);
            var otsikko = new TextBlock
            {
                Text = "PH1 — Leveys",
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(255, 210, 60))
            };
            Canvas.SetLeft(otsikko, cx - 130); Canvas.SetTop(otsikko, 6);
            canvas.Children.Add(otsikko);
        }

        private void PiirraPh2Canvas(Canvas canvas,
      double ph2V, double ph2O, double ph1Leveys,
      double halkaisija, double scale, double kuivaLeveys,
      double tuoreLeveysIlmanOffset, bool jakosahaOn = false)
        {
            canvas.Children.Clear();
            double W = canvas.ActualWidth > 20 ? canvas.ActualWidth : 400;
            double H = canvas.ActualHeight > 20 ? canvas.ActualHeight : 400;
            double cx = W / 2.0, cy = H / 2.0;
            double ph2Leveys = ph2V + ph2O;
            double tukkiR = halkaisija / 2.0 * scale;
            double pelkkaW = ph2Leveys * scale;
            double pelkkaH = ph1Leveys * scale;
            double pelkkaX = cx - pelkkaW / 2.0;
            double pelkkaY = cy - pelkkaH / 2.0;
            canvas.Children.Add(new Rectangle { Width = W, Height = H, Fill = new SolidColorBrush(Color.FromRgb(18, 15, 12)) });
            PiirraAsteikko(canvas, W, H, cx, scale);
            canvas.Children.Add(new Ellipse
            {
                Width = tukkiR * 2,
                Height = tukkiR * 2,
                Fill = Brushes.Transparent,
                Stroke = new SolidColorBrush(Color.FromRgb(140, 110, 70)),
                StrokeThickness = 1.2
            }.Also(e => { Canvas.SetLeft(e, cx - tukkiR); Canvas.SetTop(e, cy - tukkiR); }));
            canvas.Children.Add(new Rectangle
            {
                Width = pelkkaW,
                Height = pelkkaH,
                Fill = new SolidColorBrush(Color.FromArgb(190, 175, 125, 75)),
                Stroke = new SolidColorBrush(Color.FromRgb(210, 170, 110)),
                StrokeThickness = 1.5
            }.Also(r => { Canvas.SetLeft(r, pelkkaX); Canvas.SetTop(r, pelkkaY); }));
            canvas.Children.Add(new Line { X1 = cx, Y1 = pelkkaY - 12, X2 = cx, Y2 = pelkkaY + pelkkaH + 12, Stroke = new SolidColorBrush(Color.FromRgb(79, 195, 247)), StrokeThickness = 1, StrokeDashArray = new DoubleCollection { 4, 3 } });
            canvas.Children.Add(new Line { X1 = pelkkaX - 12, Y1 = cy, X2 = pelkkaX + pelkkaW + 12, Y2 = cy, Stroke = new SolidColorBrush(Color.FromRgb(79, 195, 247)), StrokeThickness = 1, StrokeDashArray = new DoubleCollection { 4, 3 } });
            double ph2VX = cx - ph2V * scale;
            double ph2OX = cx + ph2O * scale;
            var ph2Br = new SolidColorBrush(Color.FromRgb(60, 200, 255));
            canvas.Children.Add(new Line { X1 = ph2VX, Y1 = pelkkaY - 10, X2 = ph2VX, Y2 = pelkkaY + pelkkaH + 10, Stroke = ph2Br, StrokeThickness = 2.5 });
            canvas.Children.Add(new Line { X1 = ph2OX, Y1 = pelkkaY - 10, X2 = ph2OX, Y2 = pelkkaY + pelkkaH + 10, Stroke = ph2Br, StrokeThickness = 2.5 });

            var tuoreLbl2 = new TextBlock
            {
                Text = $"{tuoreLeveysIlmanOffset:F1} mm",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(60, 200, 255)),
                TextAlignment = TextAlignment.Center,
                Width = Math.Max(80, pelkkaW)
            };
            Canvas.SetLeft(tuoreLbl2, cx - tuoreLbl2.Width / 2.0);
            Canvas.SetTop(tuoreLbl2, cy - 10);
            canvas.Children.Add(tuoreLbl2);

            if (jakosahaOn && _tukkiHalkaisija > 0)
            {
                double tukkiR2 = _tukkiHalkaisija / 2.0 * scale;
                canvas.Children.Add(new Ellipse
                {
                    Width = tukkiR2 * 2,
                    Height = tukkiR2 * 2,
                    Fill = Brushes.Transparent,
                    Stroke = new SolidColorBrush(Color.FromRgb(180, 140, 60)),
                    StrokeThickness = 1.5,
                    StrokeDashArray = new DoubleCollection { 6, 3 }
                }.Also(e => { Canvas.SetLeft(e, cx - tukkiR2); Canvas.SetTop(e, cy - tukkiR2); }));
            }

            double ph1VY = cy - ph1Leveys / 2.0 * scale;
            double ph1OY = cy + ph1Leveys / 2.0 * scale;
            var ph1Br = new SolidColorBrush(Color.FromRgb(255, 210, 60));
            canvas.Children.Add(new Line { X1 = pelkkaX - 10, Y1 = ph1VY, X2 = pelkkaX + pelkkaW + 10, Y2 = ph1VY, Stroke = ph1Br, StrokeThickness = 2.5 });
            canvas.Children.Add(new Line { X1 = pelkkaX - 10, Y1 = ph1OY, X2 = pelkkaX + pelkkaW + 10, Y2 = ph1OY, Stroke = ph1Br, StrokeThickness = 2.5 });
            double arrowX = pelkkaX - 30;
            canvas.Children.Add(new Line { X1 = arrowX, Y1 = ph1VY, X2 = arrowX, Y2 = ph1OY, Stroke = ph1Br, StrokeThickness = 1 });
            canvas.Children.Add(new Line { X1 = arrowX - 4, Y1 = ph1VY, X2 = arrowX + 4, Y2 = ph1VY, Stroke = ph1Br, StrokeThickness = 1 });
            canvas.Children.Add(new Line { X1 = arrowX - 4, Y1 = ph1OY, X2 = arrowX + 4, Y2 = ph1OY, Stroke = ph1Br, StrokeThickness = 1 });
            var ph1MittaLbl = new TextBlock
            {
                Text = $"Ph1 leveys\n{ph1Leveys:F1} mm",
                FontSize = 10,
                Foreground = ph1Br,
                TextAlignment = TextAlignment.Center,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform(-90),
                Width = 80
            };
            Canvas.SetLeft(ph1MittaLbl, arrowX - 42); Canvas.SetTop(ph1MittaLbl, cy - 20);
            canvas.Children.Add(ph1MittaLbl);
            double arrowY = pelkkaY + pelkkaH + 22;
            canvas.Children.Add(new Line { X1 = ph2VX, Y1 = arrowY, X2 = ph2OX, Y2 = arrowY, Stroke = ph2Br, StrokeThickness = 1 });
            canvas.Children.Add(new Line { X1 = ph2VX, Y1 = arrowY - 4, X2 = ph2VX, Y2 = arrowY + 4, Stroke = ph2Br, StrokeThickness = 1 });
            canvas.Children.Add(new Line { X1 = ph2OX, Y1 = arrowY - 4, X2 = ph2OX, Y2 = arrowY + 4, Stroke = ph2Br, StrokeThickness = 1 });
            var ph2MittaLbl = new TextBlock
            {
                Text = $"Ph2 leveys  {ph2Leveys:F1} mm  (kuiva: {kuivaLeveys:F1} mm)",
                FontSize = 11,
                Foreground = ph2Br,
                TextAlignment = TextAlignment.Center,
                Width = Math.Max(120, pelkkaW)
            };
            Canvas.SetLeft(ph2MittaLbl, cx - ph2MittaLbl.Width / 2.0);
            Canvas.SetTop(ph2MittaLbl, arrowY + 5);
            canvas.Children.Add(ph2MittaLbl);
            var otsikko = new TextBlock
            {
                Text = "PH2 \u2014 Leveys",
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = ph2Br
            };
            Canvas.SetLeft(otsikko, 8); Canvas.SetTop(otsikko, 6);
            canvas.Children.Add(otsikko);

            var tayslaatuBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(210, 25, 25, 25)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(10, 6, 10, 6),
                BorderBrush = new SolidColorBrush(Color.FromRgb(160, 130, 90)),
                BorderThickness = new Thickness(1)
            };
            var bsp = new StackPanel();
            bsp.Children.Add(new TextBlock { Text = "Mitta t\u00e4yslaadulle", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(190, 190, 190)), TextAlignment = TextAlignment.Center });
            bsp.Children.Add(new TextBlock { Text = $"\u2205 {halkaisija:F1} mm", FontSize = 15, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(210, 170, 100)), TextAlignment = TextAlignment.Center });
            tayslaatuBorder.Child = bsp;
            Canvas.SetLeft(tayslaatuBorder, W - 155); Canvas.SetTop(tayslaatuBorder, 8);
            canvas.Children.Add(tayslaatuBorder);
        }
        // ── Piirrä jakosaha ──────────────────────────────────────────────────

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
                double kuivausKerroin = 1.0 + kuivausProsentti / 100.0;
                if (KuivausValue != null) KuivausValue.Text = $"{kuivausProsentti:F0} %";

                bool vasenOn = VasenSahaCheck?.IsChecked == true;
                bool oikeaOn = OikeaSahaCheck?.IsChecked == true;
                bool jakosahaOn = vasenOn || oikeaOn;
                bool yhdistetty = vasenOn && oikeaOn;
                bool phOn = OnPhKaytossa();

                double plc_T4 = teraRajat.TryGetValue(4, out var r4) ? r4.Lepopaikka : 340.0;
                double plc_T2 = teraRajat.TryGetValue(2, out var r2) ? r2.Lepopaikka : -25.0;
                double plc_T6 = teraRajat.TryGetValue(6, out var r6) ? r6.Lepopaikka : 25.0;
                double plc_T3 = teraRajat.TryGetValue(3, out var r3) ? r3.Lepopaikka : 340.0;
                double plc_T1 = teraRajat.TryGetValue(1, out var r1) ? r1.Lepopaikka : -25.0;
                double plc_T5 = teraRajat.TryGetValue(5, out var r5) ? r5.Lepopaikka : 25.0;



                var paksuudetVasen = new List<double>();
                var paksuudetOikea = new List<double>();
                var paksuudetYhd = new List<double>();
                var leveydetYhd = new List<double>();
                var leveydetVasen = new List<double>();
                var leveydetOikea = new List<double>();

                if (jakosahaOn)
                {
                    if (yhdistetty)
                    {
                        paksuudetYhd = GetThicknessValuesYhdistetty().Select(p => p * kuivausKerroin).ToList();
                        leveydetYhd = GetLeveysValuesYhdistetty().Select(l => l * kuivausKerroin).ToList();
                        if (paksuudetYhd.Count > 0)
                           LaskeYhdistetty(paksuudetYhd, ref plc_T1, ref plc_T2, ref plc_T3, ref plc_T4, ref plc_T5, ref plc_T6,
    0, 0);  // ← aina 0 = automaattinen, ei ref-valinnasta
                    }
                    else
                    {
                        paksuudetVasen = GetThicknessValuesVasen().Select(p => p * kuivausKerroin).ToList();
                        paksuudetOikea = GetThicknessValuesOikea().Select(p => p * kuivausKerroin).ToList();
                        leveydetVasen = GetLeveysValuesVasen().Select(l => l * kuivausKerroin).ToList();
                        leveydetOikea = GetLeveysValuesOikea().Select(l => l * kuivausKerroin).ToList();
                        if (oikeaOn && paksuudetOikea.Count > 0)
                            LaskeOikeaPuoli(paksuudetOikea, GetHalkaisuTeraOikea(), ref plc_T4, ref plc_T2, ref plc_T6);
                        if (vasenOn && paksuudetVasen.Count > 0)
                            LaskeVasenPuoli(paksuudetVasen, GetHalkaisuTeraVasen(), ref plc_T3, ref plc_T1, ref plc_T5);
                    }
                }


                double ph1V = 0, ph1O = 0, ph2V = 0, ph2O = 0, kuivaPh1 = 0, kuivaPh2 = 0, tuore1 = 0, tuore2 = 0;
                if (phOn) (ph1V, ph1O, ph2V, ph2O, kuivaPh1, kuivaPh2, tuore1, tuore2) = LaskePhArvot();
                plc_T1 += _offsetT1;
                plc_T2 += _offsetT2;
                plc_T3 += _offsetT3;
                plc_T4 += _offsetT4;
                plc_T5 += _offsetT5;
                plc_T6 += _offsetT6;

                var rajaVaroitukset = new List<string>();
                void TarkistaRaja(string nimi, double arvo, int teraNumero)
                {
                    if (!teraRajat.TryGetValue(teraNumero, out var raja)) return;
                    if (arvo < raja.Min || arvo > raja.Max)
                        rajaVaroitukset.Add($"{nimi}: {arvo:F1} (raja {raja.Min:F1}\u2026{raja.Max:F1})");
                }

                if (jakosahaOn)
                {
                    TarkistaRaja("T4", plc_T4, 4); TarkistaRaja("T2", plc_T2, 2);
                    TarkistaRaja("T6", plc_T6, 6); TarkistaRaja("T3", plc_T3, 3);
                    TarkistaRaja("T1", plc_T1, 1); TarkistaRaja("T5", plc_T5, 5);
                    if (yhdistetty)
                    {
                        double vali = Math.Abs(plc_T1) + Math.Abs(plc_T2);
                        if (vali < turvaEtaisyys)
                            rajaVaroitukset.Add($"T1-T2 v\u00e4li liian pieni: {vali:F1} mm (min {turvaEtaisyys:F1} mm)");
                    }
                }

                if (rajaVaroitukset.Count > 0)
                    SetStatus($"\u26a0  Raja ylitetty: {string.Join("  |  ", rajaVaroitukset)}", Colors.OrangeRed);
                else
                    SetStatus("Valmis", Colors.LightGray);

                if (Tera4_Value != null) Tera4_Value.Text = $"T4: {plc_T4:F1}";
                if (Tera2_Value != null) Tera2_Value.Text = $"T2: {plc_T2:F1}";
                if (Tera6_Value != null) Tera6_Value.Text = $"T6: {plc_T6:F1}";
                if (Tera3_Value != null) Tera3_Value.Text = $"T3: {plc_T3:F1}";
                if (Tera1_Value != null) Tera1_Value.Text = $"T1: {plc_T1:F1}";
                if (Tera5_Value != null) Tera5_Value.Text = $"T5: {plc_T5:F1}";


                double showPh1V = phOn ? ph1V : (phRajat.TryGetValue("PH1V", out var rph1v) ? rph1v.LepoVasen : 0.0);
                double showPh1O = phOn ? ph1O : (phRajat.TryGetValue("PH1O", out var rph1o) ? rph1o.LepoOikea : 0.0);
                double showPh2V = phOn ? ph2V : (phRajat.TryGetValue("PH2V", out var rph2v) ? rph2v.LepoVasen : 0.0);
                double showPh2O = phOn ? ph2O : (phRajat.TryGetValue("PH2O", out var rph2o) ? rph2o.LepoOikea : 0.0);
                if (Ph1V_Value != null) Ph1V_Value.Text = $"PH1V: {showPh1V:F1}";
                if (Ph1O_Value != null) Ph1O_Value.Text = $"PH1O: {showPh1O:F1}";
                if (Ph2V_Value != null) Ph2V_Value.Text = $"PH2V: {showPh2V:F1}";
                if (Ph2O_Value != null) Ph2O_Value.Text = $"PH2O: {showPh2O:F1}";

                // Tallenna jakosaha- ja PH-arvot KeraaKaikkiPlcArvot()-metodia varten
                _lastPlcT1 = plc_T1; _lastPlcT2 = plc_T2; _lastPlcT3 = plc_T3;
                _lastPlcT4 = plc_T4; _lastPlcT5 = plc_T5; _lastPlcT6 = plc_T6;
                _lastPh1V = showPh1V; _lastPh1O = showPh1O;
                _lastPh2V = showPh2V; _lastPh2O = showPh2O;

                if (YhteisCanvas != null)
                {
                    if (jakosahaOn)
                    {
                        if (yhdistetty)
                            PiirraYhteisCanvasYhdistetty(YhteisCanvas, paksuudetYhd, leveydetYhd,
                                plc_T1, plc_T2, plc_T3, plc_T4, plc_T5, plc_T6);
                        else
                            PiirraYhteisCanvas(YhteisCanvas, paksuudetVasen, paksuudetOikea,
                                leveydetVasen, leveydetOikea, vasenOn, oikeaOn,
                                plc_T3, plc_T1, plc_T5, plc_T4, plc_T2, plc_T6);
                    }
                    else
                        PiirraJakosahaLepopaikka(YhteisCanvas, plc_T1, plc_T2, plc_T3, plc_T4, plc_T5, plc_T6);
                }

                PiirraPhCanvasit();

                // Store for profile canvas
                if (yhdistetty)
                {
                    _currentPaksuudet = new List<double>(paksuudetYhd);
                    _currentLeveydet = new List<double>(leveydetYhd);
                }
                else if (vasenOn)
                {
                    _currentPaksuudet = new List<double>(paksuudetVasen);
                    _currentLeveydet = new List<double>(leveydetVasen);
                }
                else
                {
                    _currentPaksuudet = new List<double>(paksuudetOikea);
                    _currentLeveydet = new List<double>(leveydetOikea);
                }
                PiirraProfilointiCanvas();

                bool profKaytossa = _profilointiOn;
                LaskeProfilointiArvot(_currentPaksuudet, _currentLeveydet,
                    out double profT1, out double profT2,
                    out double profT3, out double profT4,
                    out double profT5, out double profT6,
                    out double profT7, out double profT8);

                if (_currentPaksuudet.Count == 0 || !profKaytossa)
                {
                    profT1 = profilointiRajat.TryGetValue("ProfT1", out var p1) ? p1.Lepopaikka : 230.0;
                    profT2 = profilointiRajat.TryGetValue("ProfT2", out var p2) ? p2.Lepopaikka : 230.0;
                    profT3 = profilointiRajat.TryGetValue("ProfT3", out var p3) ? p3.Lepopaikka : 260.0;
                    profT4 = profilointiRajat.TryGetValue("ProfT4", out var p4) ? p4.Lepopaikka : -10.0;
                    profT5 = profilointiRajat.TryGetValue("ProfT5", out var p5) ? p5.Lepopaikka : 260.0;
                    profT6 = profilointiRajat.TryGetValue("ProfT6", out var p6) ? p6.Lepopaikka : -10.0;
                    profT7 = profilointiRajat.TryGetValue("ProfT7", out var p7) ? p7.Lepopaikka : 0.0;
                    profT8 = profilointiRajat.TryGetValue("ProfT8", out var p8) ? p8.Lepopaikka : 0.0;
                }

                // Tallenna profilointiarvot ja kerää kaikki WinCC:lle
                _lastProfT1 = profKaytossa ? profT1 + _profOffsetT1 : profT1;
                _lastProfT2 = profKaytossa ? profT2 + _profOffsetT2 : profT2;
                _lastProfT3 = profKaytossa ? profT3 + _profOffsetT3 : profT3;
                _lastProfT4 = profKaytossa ? profT4 + _profOffsetT4 : profT4;
                _lastProfT5 = profKaytossa ? profT5 + _profOffsetT5 : profT5;
                _lastProfT6 = profKaytossa ? profT6 + _profOffsetT6 : profT6;
                _lastProfT7 = profKaytossa ? profT7 + _profOffsetT7 : profT7;
                _lastProfT8 = profKaytossa ? profT8 + _profOffsetT8 : profT8;
                KeraaKaikkiPlcArvot();
            }
            finally { _isPiirraVisualRunning = false; }

        }

        // ── KeraaKaikkiPlcArvot — lähettää kaikki 18 viimeisintä PLC-arvoa WinCC-ikkunaan ──
        private void KeraaKaikkiPlcArvot()
        {
            _winCCWindow?.PaivitaSetPoints(
                _lastPlcT1, _lastPlcT2, _lastPlcT3, _lastPlcT4, _lastPlcT5, _lastPlcT6,
                _lastPh1V, _lastPh1O, _lastPh2V, _lastPh2O,
                _lastProfT1, _lastProfT2, _lastProfT3, _lastProfT4,
                _lastProfT5, _lastProfT6, _lastProfT7, _lastProfT8);
        }

        // ── Yhdistetty laskenta ──────────────────────────────────────────────
 private void LaskeYhdistetty(List<double> paksuudet,
            ref double plc_T1, ref double plc_T2, ref double plc_T3,
            ref double plc_T4, ref double plc_T5, ref double plc_T6,
            int t1RefKappale = 0, int t2RefKappale = 0)
        {
            int n = paksuudet.Count;
            double RakoT(int num) => teraParametrit.TryGetValue(num, out var t) ? t.Rako : 4.0;
            double VaistoT(int num) => teraRajat.TryGetValue(num, out var r) ? r.Vaisto : -140.0;
            double OffsetT(int num)
            {
                if (!teraParametrit.TryGetValue(num, out var t)) return 1.5;
                return t.OnkoVasenKatinen ? t.Laippa - t.Runko / 2.0 : t.Runko / 2.0;
            }
            double r1 = RakoT(1), r2 = RakoT(2), r3 = RakoT(3);
            double r4 = RakoT(4), r5 = RakoT(5), r6 = RakoT(6);

            if (n == 3) { double kl = (paksuudet[0] + r1 + paksuudet[1] + r2 + paksuudet[2]) / 2.0; plc_T1 = VaistoT(1); plc_T2 = VaistoT(2); plc_T3 = (kl - paksuudet[0] - r1 / 2.0) + OffsetT(1) + Math.Abs(VaistoT(1)); plc_T4 = (paksuudet[0] + r1 + paksuudet[1] + r2 / 2.0 - kl) + OffsetT(2) + Math.Abs(VaistoT(2)); plc_T5 = VaistoT(5); plc_T6 = VaistoT(6); return; }
            if (n == 4) { double kl = (paksuudet[0] + r1 + paksuudet[1] + r2 + paksuudet[2] + r4 + paksuudet[3]) / 2.0; double rakoT4 = (paksuudet[0] + r1 + paksuudet[1] + r2 + paksuudet[2] + r4 / 2.0) - kl; plc_T1 = VaistoT(1); plc_T2 = LaskeSivuTeraPlc(paksuudet[2], r2, r4, OffsetT(2), OffsetT(4)); plc_T3 = (kl - paksuudet[0] - r1 / 2.0) + OffsetT(1) + Math.Abs(VaistoT(1)); plc_T4 = rakoT4 + OffsetT(4); plc_T5 = VaistoT(5); plc_T6 = VaistoT(6); return; }
            if (n == 5)
            {
                double kl = (paksuudet[0] + r3 + paksuudet[1] + r1 + paksuudet[2] + r2 + paksuudet[3] + r4 + paksuudet[4]) / 2.0;
                plc_T3 = kl - (paksuudet[0] + r3 / 2.0) + OffsetT(3);
                plc_T4 = (paksuudet[0] + r3 + paksuudet[1] + r1 + paksuudet[2] + r2 + paksuudet[3] + r4 / 2.0) - kl + OffsetT(4);
                int t1Idx = (t1RefKappale >= 1 && t1RefKappale <= n) ? t1RefKappale - 1 : 1;
                int t2Idx = (t2RefKappale >= 1 && t2RefKappale <= n) ? t2RefKappale - 1 : 3;
                plc_T1 = LaskeSivuTeraPlc(paksuudet[t1Idx], r1, r3, OffsetT(1), OffsetT(3));
                plc_T2 = LaskeSivuTeraPlc(paksuudet[t2Idx], r2, r4, OffsetT(2), OffsetT(4));
                plc_T5 = VaistoT(5); plc_T6 = VaistoT(6); return;
            }
            if (n == 6)
            {
                double kl = (paksuudet[0] + r3 + paksuudet[1] + r1 + paksuudet[2] + r2 + paksuudet[3] + r4 + paksuudet[4] + r6 + paksuudet[5]) / 2.0;
                plc_T3 = kl - (paksuudet[0] + r3 / 2.0) + OffsetT(3);
                plc_T4 = (paksuudet[0] + r3 + paksuudet[1] + r1 + paksuudet[2] + r2 + paksuudet[3] + r4 / 2.0) - kl + OffsetT(4);
                int t1Idx = (t1RefKappale >= 1 && t1RefKappale <= n) ? t1RefKappale - 1 : 1;
                int t2Idx = (t2RefKappale >= 1 && t2RefKappale <= n) ? t2RefKappale - 1 : 3;
                plc_T1 = LaskeSivuTeraPlc(paksuudet[t1Idx], r1, r3, OffsetT(1), OffsetT(3));
                plc_T2 = LaskeSivuTeraPlc(paksuudet[t2Idx], r2, r4, OffsetT(2), OffsetT(4));
                plc_T5 = VaistoT(5); plc_T6 = LaskeUlkoTeraPlc(paksuudet[4], r6, r4, OffsetT(6), OffsetT(4)); return;
            }
            if (n == 7)
            {
                double kl = (paksuudet[0] + r5 + paksuudet[1] + r3 + paksuudet[2] + r1 + paksuudet[3] + r2 + paksuudet[4] + r4 + paksuudet[5] + r6 + paksuudet[6]) / 2.0;
                plc_T3 = kl - (paksuudet[0] + r5 + paksuudet[1] + r3 / 2.0) + OffsetT(3);
                plc_T4 = (paksuudet[0] + r5 + paksuudet[1] + r3 + paksuudet[2] + r1 + paksuudet[3] + r2 + paksuudet[4] + r4 / 2.0) - kl + OffsetT(4);
                int t1Idx = (t1RefKappale >= 1 && t1RefKappale <= n) ? t1RefKappale - 1 : 2;
                int t2Idx = (t2RefKappale >= 1 && t2RefKappale <= n) ? t2RefKappale - 1 : 4;
                plc_T1 = LaskeSivuTeraPlc(paksuudet[t1Idx], r1, r3, OffsetT(1), OffsetT(3));
                plc_T2 = LaskeSivuTeraPlc(paksuudet[t2Idx], r2, r4, OffsetT(2), OffsetT(4));
                plc_T5 = LaskeUlkoTeraPlc(paksuudet[1], r5, r3, OffsetT(5), OffsetT(3));
                plc_T6 = LaskeUlkoTeraPlc(paksuudet[5], r6, r4, OffsetT(6), OffsetT(4)); return;
            }

            int puolikas = n / 2; bool parillinenN = n % 2 == 0;
            var pVasen = paksuudet.Take(puolikas).ToList();
            var pOikea = paksuudet.Skip(parillinenN ? puolikas : puolikas + 1).ToList();
            if (pVasen.Count > 0) LaskeVasenPuoli(pVasen, 3, ref plc_T3, ref plc_T1, ref plc_T5);
            if (pOikea.Count > 0) LaskeOikeaPuoli(pOikea, 4, ref plc_T4, ref plc_T2, ref plc_T6);
            if (!parillinenN) { double kp = paksuudet[puolikas]; double rr1 = RakoT(1), rr2 = RakoT(2); plc_T1 -= kp / 2.0 + rr1 / 2.0; plc_T2 -= kp / 2.0 + rr2 / 2.0; }
        }

        // ── Laskenta erillinen ───────────────────────────────────────────────

        private static double LaskeKeskilinja2(List<double> p, double r1) => (p.Sum() + r1) / 2.0;
        private static double LaskeKeskilinja3(List<double> p, double r1, double r2) => (p.Sum() + r1 + r2) / 2.0;
        private static double LaskeKeskilinja4(List<double> p, double r1, double r2, double r3) => (p.Sum() + r1 + r2 + r3) / 2.0;
        private static double LaskeOffset(bool vasenKatinen, double laippa, double runko) =>
            vasenKatinen ? laippa - runko / 2.0 : runko / 2.0;
        private static double LaskeSivuTeraPlc(double kappale, double rakoSivu, double rakoPaa, double offsetSivu, double offsetPaa) =>
            -((rakoSivu / 2.0 + kappale + rakoPaa / 2.0) - offsetSivu + offsetPaa);
        private static double LaskeUlkoTeraPlc(double kappale, double rakoUlko, double rakoPaa, double offsetUlko, double offsetPaa) =>
            (rakoUlko / 2.0 + kappale + rakoPaa / 2.0) - offsetPaa + offsetUlko;

        private void LaskeOikeaPuoli(List<double> paksuudet, int halkaisuTera,
            ref double plc_T4, ref double plc_T2, ref double plc_T6)
        {
            if (paksuudet.Count == 0) return;
            double vaistoSisa = teraRajat.TryGetValue(2, out var rv2) ? rv2.Vaisto : Vaisto_Sisaterä;
            double vaistoUlko = teraRajat.TryGetValue(6, out var rv6) ? rv6.Vaisto : Vaisto_Ulkoterä;
            if (paksuudet.Count == 2)
            {
                if (halkaisuTera == 4 && teraParametrit.TryGetValue(4, out var t4))
                { double kl = LaskeKeskilinja2(paksuudet, t4.Rako); plc_T4 = paksuudet[0] + t4.Rako / 2.0 + LaskeOffset(t4.OnkoVasenKatinen, t4.Laippa, t4.Runko) - kl; plc_T2 = vaistoSisa; plc_T6 = vaistoUlko; }
                else if (halkaisuTera == 2 && teraParametrit.TryGetValue(2, out var t2))
                { double kl = LaskeKeskilinja2(paksuudet, t2.Rako); plc_T2 = vaistoSisa; plc_T4 = paksuudet[0] + t2.Rako / 2.0 + LaskeOffset(t2.OnkoVasenKatinen, t2.Laippa, t2.Runko) - kl - vaistoSisa; plc_T6 = vaistoUlko; }
            }
            else if (paksuudet.Count == 3)
            {
                teraParametrit.TryGetValue(4, out var tera4); teraParametrit.TryGetValue(2, out var tera2);
                double rako1 = tera2?.Rako ?? 4.0, rako2 = tera4?.Rako ?? 4.0;
                double kl = LaskeKeskilinja3(paksuudet, rako1, rako2);
                if (tera4 != null) { double off4 = LaskeOffset(tera4.OnkoVasenKatinen, tera4.Laippa, tera4.Runko); plc_T4 = paksuudet[0] + rako1 + paksuudet[1] + rako2 / 2.0 + off4 - kl; if (tera2 != null) plc_T2 = LaskeSivuTeraPlc(paksuudet[1], rako1, rako2, LaskeOffset(tera2.OnkoVasenKatinen, tera2.Laippa, tera2.Runko), off4); }
                plc_T6 = vaistoUlko;
            }
            else if (paksuudet.Count == 4)
            {
                teraParametrit.TryGetValue(4, out var tera4); teraParametrit.TryGetValue(2, out var tera2); teraParametrit.TryGetValue(6, out var tera6);
                if (tera4 == null) return;
                double rako1 = tera2?.Rako ?? 4.0, rako2 = tera4.Rako, rako3 = tera6?.Rako ?? 4.0;
                double kl = LaskeKeskilinja4(paksuudet, rako1, rako2, rako3);
                double off4 = LaskeOffset(tera4.OnkoVasenKatinen, tera4.Laippa, tera4.Runko);
                plc_T4 = paksuudet[0] + rako1 + paksuudet[1] + rako2 / 2.0 + off4 - kl;
                if (tera2 != null) plc_T2 = LaskeSivuTeraPlc(paksuudet[1], rako1, rako2, LaskeOffset(tera2.OnkoVasenKatinen, tera2.Laippa, tera2.Runko), off4);
                if (tera6 != null) plc_T6 = LaskeUlkoTeraPlc(paksuudet[2], rako3, rako2, LaskeOffset(tera6.OnkoVasenKatinen, tera6.Laippa, tera6.Runko), off4);
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
                if (halkaisuTera == 3 && teraParametrit.TryGetValue(3, out var t3))
                { double kl = LaskeKeskilinja2(paksuudet, t3.Rako); plc_T3 = paksuudet[0] + t3.Rako / 2.0 + LaskeOffset(t3.OnkoVasenKatinen, t3.Laippa, t3.Runko) - kl; plc_T1 = vaistoSisa; plc_T5 = vaistoUlko; }
                else if (halkaisuTera == 1 && teraParametrit.TryGetValue(1, out var t1))
                { double kl = LaskeKeskilinja2(paksuudet, t1.Rako); plc_T1 = vaistoSisa; plc_T3 = paksuudet[0] + t1.Rako / 2.0 + LaskeOffset(t1.OnkoVasenKatinen, t1.Laippa, t1.Runko) - kl - vaistoSisa; plc_T5 = vaistoUlko; }
            }
            else if (paksuudet.Count == 3)
            {
                teraParametrit.TryGetValue(3, out var tera3); teraParametrit.TryGetValue(1, out var tera1);
                double rako1 = tera1?.Rako ?? 4.0, rako2 = tera3?.Rako ?? 4.0;
                double kl = LaskeKeskilinja3(paksuudet, rako1, rako2);
                if (tera3 != null) { double off3 = LaskeOffset(tera3.OnkoVasenKatinen, tera3.Laippa, tera3.Runko); plc_T3 = paksuudet[0] + rako1 + paksuudet[1] + rako2 / 2.0 + off3 - kl; if (tera1 != null) plc_T1 = LaskeSivuTeraPlc(paksuudet[1], rako1, rako2, LaskeOffset(tera1.OnkoVasenKatinen, tera1.Laippa, tera1.Runko), off3); }
                plc_T5 = vaistoUlko;
            }
            else if (paksuudet.Count == 4)
            {
                teraParametrit.TryGetValue(3, out var tera3); teraParametrit.TryGetValue(1, out var tera1); teraParametrit.TryGetValue(5, out var tera5);
                if (tera3 == null) return;
                double rako1 = tera1?.Rako ?? 4.0, rako2 = tera3.Rako, rako3 = tera5?.Rako ?? 4.0;
                double kl = LaskeKeskilinja4(paksuudet, rako1, rako2, rako3);
                double off3 = LaskeOffset(tera3.OnkoVasenKatinen, tera3.Laippa, tera3.Runko);
                plc_T3 = paksuudet[0] + rako1 + paksuudet[1] + rako2 / 2.0 + off3 - kl;
                if (tera1 != null) plc_T1 = LaskeSivuTeraPlc(paksuudet[1], rako1, rako2, LaskeOffset(tera1.OnkoVasenKatinen, tera1.Laippa, tera1.Runko), off3);
                if (tera5 != null) plc_T5 = LaskeUlkoTeraPlc(paksuudet[2], rako3, rako2, LaskeOffset(tera5.OnkoVasenKatinen, tera5.Laippa, tera5.Runko), off3);
            }
        }

        // ── Canvas jakosaha ──────────────────────────────────────────────────

        private Brush TeraViivaBrush(double arvo, int teraNumero, Color normaali)
        {
            if (teraRajat.TryGetValue(teraNumero, out var raja))
                if (arvo < raja.Min || arvo > raja.Max)
                    return new SolidColorBrush(Colors.OrangeRed);
            return new SolidColorBrush(normaali);
        }

        private static void PiirraTeraViiva(Canvas canvas, double bladeX,
            double rectY, double rectHeight, double canvasWidth,
            Brush brush, string label, bool sahaa, bool labelRight)
        {
            if (bladeX < -50 || bladeX > canvasWidth + 50) return;
            var line = new Line { X1 = bladeX, Y1 = rectY - 20, X2 = bladeX, Y2 = rectY + rectHeight + 20, Stroke = brush, StrokeThickness = sahaa ? 3 : 1.5 };
            if (!sahaa) line.StrokeDashArray = new DoubleCollection { 4, 3 };
            canvas.Children.Add(line);
            double labelX = labelRight ? Math.Min(bladeX + 4, canvasWidth - 50) : Math.Max(bladeX - 44, 4);
            var parts = label.Split('\n');
            var tb = new TextBlock { Foreground = brush, FontSize = 10, FontWeight = sahaa ? FontWeights.Bold : FontWeights.Normal, TextAlignment = labelRight ? TextAlignment.Left : TextAlignment.Right };
            tb.Inlines.Add(new Run(parts[0] + "\n"));
            if (parts.Length > 1) tb.Inlines.Add(new Run(parts[1]));
            Canvas.SetLeft(tb, labelX); Canvas.SetTop(tb, rectY - 38);
            canvas.Children.Add(tb);
        }

        private static readonly Color[] PuuVarit =
        {
            Color.FromRgb(210,175,130), Color.FromRgb(185,145,100), Color.FromRgb(160,115,72),
            Color.FromRgb(220,190,148), Color.FromRgb(175,132,88),  Color.FromRgb(145,100,60),
            Color.FromRgb(230,205,165)
        };
        private static readonly Color PuuReuna = Color.FromRgb(100, 65, 30);

        private static double GetKappaleKorkeus(int i, List<double> leveydet, double rectHeight, double pixelsPerMm)
        {
            if (leveydet != null && i < leveydet.Count && leveydet[i] > 0)
                return Math.Min(leveydet[i] * pixelsPerMm, rectHeight);
            return rectHeight;
        }

        private double LaskeVahimmaisTeraHalkaisija()
        {
            double levinKappale = 0;

            if (OnJakosahaKaytossa())
            {
                double.TryParse(KuivausTextBox?.Text.Replace(",", ".") ?? "0",
                    NumberStyles.Float, CultureInfo.InvariantCulture, out double kuivaus);
                double kerroin = 1.0 + kuivaus / 100.0;
                var leveydet = GetLeveysValues();
                levinKappale = leveydet.Count > 0 ? leveydet.Max() * kerroin : 0;
            }
            else
            {
                double.TryParse(PhLevinKappaleBox?.Text.Replace(",", ".") ?? "0",
                    NumberStyles.Float, CultureInfo.InvariantCulture, out levinKappale);
            }

            return (levinKappale + 12.0) * 2.0 + 208.0;
        }

        private static Border LuoCanvasInfoLaatikko(string otsikko, string arvo, Color korostus)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(210, 25, 25, 25)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(10, 6, 10, 6),
                BorderBrush = new SolidColorBrush(korostus),
                BorderThickness = new Thickness(1)
            };

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                Text = otsikko,
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(190, 190, 190)),
                TextAlignment = TextAlignment.Center
            });
            stack.Children.Add(new TextBlock
            {
                Text = arvo,
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(korostus),
                TextAlignment = TextAlignment.Center
            });
            border.Child = stack;
            return border;
        }

        private static double LisaaCanvasInfoLaatikko(Canvas canvas, Border border, double canvasWidth, double top)
        {
            border.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetTop(border, top);
            Canvas.SetLeft(border, canvasWidth - border.DesiredSize.Width - 8);
            canvas.Children.Add(border);
            return top + border.DesiredSize.Height + 8;
        }

        private void LisaaJakosahaInfoLaatikot(Canvas canvas, double canvasWidth)
        {
            double top = 8;

            var tukkiBorder = LuoCanvasInfoLaatikko("Tukki \u2300", $"{_tukkiHalkaisija:F0} mm", Color.FromRgb(210, 170, 100));
            top = LisaaCanvasInfoLaatikko(canvas, tukkiBorder, canvasWidth, top);

            var vahimmaisBorder = LuoCanvasInfoLaatikko("Vähimmäis \u2300", $"{LaskeVahimmaisTeraHalkaisija():F0} mm", Color.FromRgb(79, 195, 247));
            LisaaCanvasInfoLaatikko(canvas, vahimmaisBorder, canvasWidth, top);
        }

        private void PiirraYhteisCanvasYhdistetty(Canvas canvas, List<double> paksuudet,
            List<double> leveydet, double plc_T1, double plc_T2, double plc_T3,
            double plc_T4, double plc_T5, double plc_T6)
        {
            canvas.Children.Clear();
            int n = paksuudet.Count;
            double canvasWidth = canvas.ActualWidth > 20 ? canvas.ActualWidth : 900;
            double canvasHeight = canvas.ActualHeight > 20 ? canvas.ActualHeight : 400;
            double centerY = canvasHeight / 2.0, centerX = canvasWidth / 2.0;
            double pixelsPerMm = (canvasWidth / 2.0 - 20) / 350.0;
            double rectHeight = canvasHeight * 0.65, rectY = centerY - rectHeight / 2.0;

            canvas.Children.Add(new Rectangle { Width = canvasWidth, Height = canvasHeight, Fill = new SolidColorBrush(Color.FromRgb(18, 15, 12)) });
            PiirraAsteikko(canvas, canvasWidth, canvasHeight, centerX, pixelsPerMm);
            canvas.Children.Add(new Line { X1 = centerX, Y1 = 20, X2 = centerX, Y2 = canvasHeight - 30, Stroke = new SolidColorBrush(Color.FromRgb(79, 195, 247)), StrokeThickness = 1.5, StrokeDashArray = new DoubleCollection { 4, 3 } });
            var cLbl = new TextBlock { Text = "0", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(79, 195, 247)) };
            Canvas.SetLeft(cLbl, centerX + 3); Canvas.SetTop(cLbl, 4); canvas.Children.Add(cLbl);

            int[] teraJarjestys;
            if (OnYhdistettyTila())
            {
                teraJarjestys = n switch
                {
                    3 => new[] { 1, 2 },
                    4 => new[] { 1, 2, 4 },
                    5 => new[] { 1, 3, 2, 4 },
                    6 => new[] { 3, 1, 2, 4, 6 },
                    7 => new[] { 5, 3, 1, 2, 4, 6 },
                    _ => Array.Empty<int>()
                };
            }
            else if (OikeaSahaCheck?.IsChecked == true)
            {
                teraJarjestys = Enumerable.Range(0, n - 1).Select(i => (i + 1) * 2).ToArray();
            }
            else
            {
                teraJarjestys = Enumerable.Range(0, n - 1).Select(i => i * 2 + 1).ToArray();
            }
            double RakoT(int num) => teraParametrit.TryGetValue(num, out var t) ? t.Rako : 4.0;
            double kokonaisLeveys = paksuudet.Sum() + teraJarjestys.Select(t => RakoT(t)).Sum();
            double curMm = -kokonaisLeveys / 2.0;

            for (int i = 0; i < n; i++)
            {
                double paksuus = paksuudet[i];
                double pieceW = paksuus * pixelsPerMm;
                double pieceH = GetKappaleKorkeus(i, leveydet, rectHeight, pixelsPerMm);
                double pieceX = centerX + curMm * pixelsPerMm;
                double pieceY = centerY - pieceH / 2.0;
                var c = PuuVarit[i % PuuVarit.Length];
                canvas.Children.Add(new Rectangle { Width = pieceW, Height = pieceH, Fill = new SolidColorBrush(Color.FromArgb(200, c.R, c.G, c.B)), Stroke = new SolidColorBrush(PuuReuna), StrokeThickness = 1, RadiusX = 3, RadiusY = 3 }.Also(r => { Canvas.SetLeft(r, pieceX); Canvas.SetTop(r, pieceY); }));
                double lev = (leveydet != null && i < leveydet.Count && leveydet[i] > 0) ? leveydet[i] : 0;
                string levTeksti = lev > 0 ? $"\n×\n{lev:F0}" : "";
                var tl = new TextBlock { Text = $"K{i + 1}\n{paksuus:F1}{levTeksti}", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(40, 25, 10)), TextAlignment = TextAlignment.Center, Width = Math.Max(20, pieceW) };
                Canvas.SetLeft(tl, pieceX + pieceW / 2.0 - tl.Width / 2.0); Canvas.SetTop(tl, centerY - 14); canvas.Children.Add(tl);
                curMm += paksuus;
                if (i < teraJarjestys.Length)
                {
                    double rako = RakoT(teraJarjestys[i]);
                    curMm += rako;
                }
            }

            if (_tukkiHalkaisija > 0)
            {
                double tukkiR = _tukkiHalkaisija / 2.0 * pixelsPerMm;
                canvas.Children.Add(new Ellipse
                {
                    Width = tukkiR * 2,
                    Height = tukkiR * 2,
                    Fill = Brushes.Transparent,
                    Stroke = new SolidColorBrush(Color.FromRgb(180, 140, 60)),
                    StrokeThickness = 1.5,
                    StrokeDashArray = new DoubleCollection { 6, 3 }
                }.Also(e => { Canvas.SetLeft(e, centerX - tukkiR); Canvas.SetTop(e, centerY - tukkiR); }));
            }

            double totalStartX = centerX - kokonaisLeveys / 2.0 * pixelsPerMm;
            double totalEndX = centerX + kokonaisLeveys / 2.0 * pixelsPerMm;
            double arrowY = rectY + rectHeight + 10;
            var arrowBrush = new SolidColorBrush(Color.FromRgb(255, 217, 61));
            canvas.Children.Add(new Line { X1 = totalStartX, Y1 = arrowY, X2 = totalEndX, Y2 = arrowY, Stroke = arrowBrush, StrokeThickness = 1 });
            canvas.Children.Add(new Line { X1 = totalStartX, Y1 = arrowY - 4, X2 = totalStartX, Y2 = arrowY + 4, Stroke = arrowBrush, StrokeThickness = 1 });
            canvas.Children.Add(new Line { X1 = totalEndX, Y1 = arrowY - 4, X2 = totalEndX, Y2 = arrowY + 4, Stroke = arrowBrush, StrokeThickness = 1 });
            var kokoLbl = new TextBlock { Text = $"\u27f5 {kokonaisLeveys:F1} mm \u27f6", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = arrowBrush, TextAlignment = TextAlignment.Center, Width = Math.Max(60, totalEndX - totalStartX) };
            Canvas.SetLeft(kokoLbl, totalStartX + (totalEndX - totalStartX) / 2.0 - kokoLbl.Width / 2.0); Canvas.SetTop(kokoLbl, arrowY + 5); canvas.Children.Add(kokoLbl);

            double t3X = centerX - plc_T3 * pixelsPerMm, t1X = centerX - (plc_T3 + plc_T1) * pixelsPerMm;
            double t4X = centerX + plc_T4 * pixelsPerMm, t2X = centerX + (plc_T4 + plc_T2) * pixelsPerMm;
            double t5X = centerX - (plc_T3 + plc_T5) * pixelsPerMm, t6X = centerX + (plc_T4 + plc_T6) * pixelsPerMm;
            bool t3Sahaa = n != 3, t4Sahaa = n != 3, t5Sahaa = n == 7, t6Sahaa = n >= 6;
            Color normO = Color.FromRgb(107, 203, 119), normV = Color.FromRgb(255, 107, 107), lepo = Color.FromRgb(100, 100, 100);

            PiirraTeraViiva(canvas, t5X, rectY, rectHeight, canvasWidth, t5Sahaa ? TeraViivaBrush(plc_T5, 5, normV) : new SolidColorBrush(lepo), $"T5\n{plc_T5:F1}", t5Sahaa, true);
            PiirraTeraViiva(canvas, t3X, rectY, rectHeight, canvasWidth, t3Sahaa ? TeraViivaBrush(plc_T3, 3, normV) : new SolidColorBrush(lepo), $"T3\n{plc_T3:F1}", t3Sahaa, false);
            PiirraTeraViiva(canvas, t6X, rectY, rectHeight, canvasWidth, t6Sahaa ? TeraViivaBrush(plc_T6, 6, normO) : new SolidColorBrush(lepo), $"T6\n{plc_T6:F1}", t6Sahaa, true);
            PiirraTeraViiva(canvas, t4X, rectY, rectHeight, canvasWidth, t4Sahaa ? TeraViivaBrush(plc_T4, 4, normO) : new SolidColorBrush(lepo), $"T4\n{plc_T4:F1}", t4Sahaa, true);
            PiirraTeraViiva(canvas, t1X, rectY, rectHeight, canvasWidth, TeraViivaBrush(plc_T1, 1, normV), $"T1\n{plc_T1:F1}", true, false);
            PiirraTeraViiva(canvas, t2X, rectY, rectHeight, canvasWidth, TeraViivaBrush(plc_T2, 2, normO), $"T2\n{plc_T2:F1}", true, true);

            double valiMm = Math.Abs((t2X - centerX) / pixelsPerMm - (t1X - centerX) / pixelsPerMm);
            double tvStartX = Math.Min(t1X, t2X);
            canvas.Children.Add(new Rectangle { Width = Math.Max(1, Math.Abs(t2X - t1X)), Height = 6, Fill = new SolidColorBrush(valiMm < turvaEtaisyys ? Color.FromArgb(180, 255, 50, 50) : Color.FromArgb(100, 50, 255, 50)) }.Also(r => { Canvas.SetLeft(r, tvStartX); Canvas.SetTop(r, rectY - 10); }));
            var turvaLbl = new TextBlock { Text = $"{valiMm:F1} mm", FontSize = 9, Foreground = new SolidColorBrush(valiMm < turvaEtaisyys ? Colors.OrangeRed : Colors.LightGreen) };
            Canvas.SetLeft(turvaLbl, tvStartX); Canvas.SetTop(turvaLbl, rectY - 22); canvas.Children.Add(turvaLbl);

            var otsikko = new TextBlock { Text = $"\U0001f500 Yhdistetty sahaus \u2014 {n} kpl", FontSize = 12, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(255, 217, 61)) };
            Canvas.SetLeft(otsikko, centerX - 100); Canvas.SetTop(otsikko, 6); canvas.Children.Add(otsikko);
            LisaaJakosahaInfoLaatikot(canvas, canvasWidth);
        }

        private void PiirraYhteisCanvas(Canvas canvas,
            List<double> paksuudetVasen, List<double> paksuudetOikea,
            List<double> leveydetVasen, List<double> leveydetOikea,
            bool vasenOn, bool oikeaOn,
            double plc_T3, double plc_T1, double plc_T5,
            double plc_T4, double plc_T2, double plc_T6)
        {
            canvas.Children.Clear();
            double canvasWidth = canvas.ActualWidth > 20 ? canvas.ActualWidth : 900;
            double canvasHeight = canvas.ActualHeight > 20 ? canvas.ActualHeight : 400;
            double centerY = canvasHeight / 2.0, centerX = canvasWidth / 2.0;
            double pixelsPerMm = (canvasWidth / 2.0 - 20) / 350.0;
            double rectHeight = canvasHeight * 0.65, rectY = centerY - rectHeight / 2.0;

            canvas.Children.Add(new Rectangle { Width = canvasWidth, Height = canvasHeight, Fill = new SolidColorBrush(Color.FromRgb(18, 15, 12)) });
            PiirraAsteikko(canvas, canvasWidth, canvasHeight, centerX, pixelsPerMm);
            canvas.Children.Add(new Line { X1 = centerX, Y1 = 20, X2 = centerX, Y2 = canvasHeight - 30, Stroke = new SolidColorBrush(Color.FromRgb(79, 195, 247)), StrokeThickness = 1.5, StrokeDashArray = new DoubleCollection { 4, 3 } });

            double RakoT(int num) => teraParametrit.TryGetValue(num, out var t) ? t.Rako : 4.0;
            void PiirraKappale(double xMm, double paksuus, double leveys, Color color, Color border, string label)
            {
                double x = centerX + xMm * pixelsPerMm, w = paksuus * pixelsPerMm;
                double h = leveys > 0 ? Math.Min(leveys * pixelsPerMm, rectHeight) : rectHeight;
                double y = centerY - h / 2.0;
                canvas.Children.Add(new Rectangle { Width = Math.Max(1, w), Height = h, Fill = new SolidColorBrush(Color.FromArgb(200, color.R, color.G, color.B)), Stroke = new SolidColorBrush(border), StrokeThickness = 1.2 }.Also(r => { Canvas.SetLeft(r, x); Canvas.SetTop(r, y); }));
                string levTeksti = leveys > 0 ? $"\n×\n{leveys:F0}" : "";
                var tl = new TextBlock { Text = $"{label}\n{paksuus:F1}{levTeksti}", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(40, 25, 10)), TextAlignment = TextAlignment.Center, Width = Math.Max(1, w) };
                Canvas.SetLeft(tl, x + w / 2.0 - tl.Width / 2.0); Canvas.SetTop(tl, centerY - 14); canvas.Children.Add(tl);


            }

            if (vasenOn && paksuudetVasen.Count > 0)
            {
                Color borderV = Color.FromRgb(255, 107, 107);
                double r1 = RakoT(1), r3 = RakoT(3), r5 = RakoT(5);
                double k1 = paksuudetVasen.Count >= 1 ? paksuudetVasen[0] : 0, k2 = paksuudetVasen.Count >= 2 ? paksuudetVasen[1] : 0;
                double k3 = paksuudetVasen.Count >= 3 ? paksuudetVasen[2] : 0, k4 = paksuudetVasen.Count >= 4 ? paksuudetVasen[3] : 0;
                double l1 = leveydetVasen.Count >= 1 ? leveydetVasen[0] : 0, l2 = leveydetVasen.Count >= 2 ? leveydetVasen[1] : 0;
                double l3 = leveydetVasen.Count >= 3 ? leveydetVasen[2] : 0, l4 = leveydetVasen.Count >= 4 ? leveydetVasen[3] : 0;
                double totalV = k1 + (paksuudetVasen.Count >= 2 ? r1 + k2 : 0) + (paksuudetVasen.Count >= 3 ? r3 + k3 : 0) + (paksuudetVasen.Count >= 4 ? r5 + k4 : 0);
                double klV = totalV / 2.0, k4X = -klV;
                double k3X = paksuudetVasen.Count >= 4 ? k4X + k4 + r5 : -klV;
                double k2X = paksuudetVasen.Count >= 3 ? k3X + k3 + r3 : paksuudetVasen.Count >= 2 ? -klV : -klV;
                double k1X = paksuudetVasen.Count >= 2 ? k2X + k2 + r1 : -klV;
                if (paksuudetVasen.Count >= 4) PiirraKappale(k4X, k4, l4, PuuVarit[3], borderV, "K4");
                if (paksuudetVasen.Count >= 3) PiirraKappale(k3X, k3, l3, PuuVarit[2], borderV, "K3");
                if (paksuudetVasen.Count >= 2) PiirraKappale(k2X, k2, l2, PuuVarit[1], borderV, "K2");
                if (paksuudetVasen.Count >= 1) PiirraKappale(k1X, k1, l1, PuuVarit[0], borderV, "K1");
                double leftEdge = paksuudetVasen.Count >= 4 ? k4X : paksuudetVasen.Count >= 3 ? k3X : paksuudetVasen.Count >= 2 ? k2X : k1X;
                double aY = rectY + rectHeight + 10, lX = centerX + leftEdge * pixelsPerMm, rX = centerX + (k1X + k1) * pixelsPerMm;
                var bV = new SolidColorBrush(Color.FromRgb(255, 107, 107));
                canvas.Children.Add(new Line { X1 = lX, Y1 = aY, X2 = rX, Y2 = aY, Stroke = bV, StrokeThickness = 1 }); canvas.Children.Add(new Line { X1 = lX, Y1 = aY - 4, X2 = lX, Y2 = aY + 4, Stroke = bV, StrokeThickness = 1 }); canvas.Children.Add(new Line { X1 = rX, Y1 = aY - 4, X2 = rX, Y2 = aY + 4, Stroke = bV, StrokeThickness = 1 });
                var lv = new TextBlock { Text = $"\u27f5 {totalV:F1} mm \u27f6", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = bV, TextAlignment = TextAlignment.Center, Width = Math.Max(60, Math.Abs(rX - lX)) };
                Canvas.SetLeft(lv, lX + (rX - lX) / 2.0 - lv.Width / 2.0); Canvas.SetTop(lv, aY + 5); canvas.Children.Add(lv);
            }

            if (oikeaOn && paksuudetOikea.Count > 0)
            {
                Color borderO = Color.FromRgb(107, 203, 119);
                double r2 = RakoT(2), r4 = RakoT(4), r6 = RakoT(6);
                double k5 = paksuudetOikea.Count >= 1 ? paksuudetOikea[0] : 0, k6 = paksuudetOikea.Count >= 2 ? paksuudetOikea[1] : 0;
                double k7 = paksuudetOikea.Count >= 3 ? paksuudetOikea[2] : 0, k8 = paksuudetOikea.Count >= 4 ? paksuudetOikea[3] : 0;
                double l5 = leveydetOikea.Count >= 1 ? leveydetOikea[0] : 0, l6 = leveydetOikea.Count >= 2 ? leveydetOikea[1] : 0;
                double l7 = leveydetOikea.Count >= 3 ? leveydetOikea[2] : 0, l8 = leveydetOikea.Count >= 4 ? leveydetOikea[3] : 0;
                double totalO = k5 + (paksuudetOikea.Count >= 2 ? r2 + k6 : 0) + (paksuudetOikea.Count >= 3 ? r4 + k7 : 0) + (paksuudetOikea.Count >= 4 ? r6 + k8 : 0);
                double klO = totalO / 2.0, k5X = -klO, k6X = k5X + k5 + r2, k7X = k6X + k6 + r4, k8X = k7X + k7 + r6;
                if (paksuudetOikea.Count >= 1) PiirraKappale(k5X, k5, l5, PuuVarit[0], borderO, "K5");
                if (paksuudetOikea.Count >= 2) PiirraKappale(k6X, k6, l6, PuuVarit[1], borderO, "K6");
                if (paksuudetOikea.Count >= 3) PiirraKappale(k7X, k7, l7, PuuVarit[2], borderO, "K7");
                if (paksuudetOikea.Count >= 4) PiirraKappale(k8X, k8, l8, PuuVarit[3], borderO, "K8");
                double rightEdge = paksuudetOikea.Count >= 4 ? k8X + k8 : paksuudetOikea.Count >= 3 ? k7X + k7 : paksuudetOikea.Count >= 2 ? k6X + k6 : k5X + k5;
                double aY = rectY + rectHeight + 10, lX = centerX + k5X * pixelsPerMm, rX = centerX + rightEdge * pixelsPerMm;
                var bO = new SolidColorBrush(Color.FromRgb(107, 203, 119));
                canvas.Children.Add(new Line { X1 = lX, Y1 = aY, X2 = rX, Y2 = aY, Stroke = bO, StrokeThickness = 1 }); canvas.Children.Add(new Line { X1 = lX, Y1 = aY - 4, X2 = lX, Y2 = aY + 4, Stroke = bO, StrokeThickness = 1 }); canvas.Children.Add(new Line { X1 = rX, Y1 = aY - 4, X2 = rX, Y2 = aY + 4, Stroke = bO, StrokeThickness = 1 });
                var lo = new TextBlock { Text = $"\u27f5 {totalO:F1} mm \u27f6", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = bO, TextAlignment = TextAlignment.Center, Width = Math.Max(60, Math.Abs(rX - lX)) };
                Canvas.SetLeft(lo, lX + (rX - lX) / 2.0 - lo.Width / 2.0); Canvas.SetTop(lo, aY + 5); canvas.Children.Add(lo);
            }

            {
                Color normO = oikeaOn ? Color.FromRgb(107, 203, 119) : Color.FromRgb(50, 90, 50); Color lepo = Color.FromRgb(100, 100, 100);
                bool t2S = oikeaOn && paksuudetOikea.Count >= 1, t4S = oikeaOn && paksuudetOikea.Count >= 2, t6S = oikeaOn && paksuudetOikea.Count >= 3;
                double t6X = centerX + (plc_T4 + plc_T6) * pixelsPerMm, t4X = centerX + plc_T4 * pixelsPerMm, t2X = centerX + (plc_T4 + plc_T2) * pixelsPerMm;
                PiirraTeraViiva(canvas, t6X, rectY, rectHeight, canvasWidth, t6S ? TeraViivaBrush(plc_T6, 6, normO) : new SolidColorBrush(lepo), $"T6\n{plc_T6:F1}", t6S, true);
                PiirraTeraViiva(canvas, t4X, rectY, rectHeight, canvasWidth, t4S ? TeraViivaBrush(plc_T4, 4, normO) : new SolidColorBrush(lepo), $"T4\n{plc_T4:F1}", t4S, true);
                PiirraTeraViiva(canvas, t2X, rectY, rectHeight, canvasWidth, t2S ? TeraViivaBrush(plc_T2, 2, normO) : new SolidColorBrush(lepo), $"T2\n{plc_T2:F1}", t2S, false);
                var ol = new TextBlock { Text = "Oikea saha \u25b6", Foreground = new SolidColorBrush(normO), FontSize = 11, FontWeight = FontWeights.Bold };
                Canvas.SetLeft(ol, canvasWidth - 140); Canvas.SetTop(ol, 6); canvas.Children.Add(ol);
            }

            {
                Color normV = vasenOn ? Color.FromRgb(255, 107, 107) : Color.FromRgb(90, 50, 50); Color lepo = Color.FromRgb(100, 100, 100);
                bool t1S = vasenOn && paksuudetVasen.Count >= 1, t3S = vasenOn && paksuudetVasen.Count >= 2, t5S = vasenOn && paksuudetVasen.Count >= 3;
                double t5X = centerX - (plc_T3 + plc_T5) * pixelsPerMm, t3X = centerX - plc_T3 * pixelsPerMm, t1X = centerX - (plc_T3 + plc_T1) * pixelsPerMm;
                PiirraTeraViiva(canvas, t5X, rectY, rectHeight, canvasWidth, t5S ? TeraViivaBrush(plc_T5, 5, normV) : new SolidColorBrush(lepo), $"T5\n{plc_T5:F1}", t5S, false);
                PiirraTeraViiva(canvas, t3X, rectY, rectHeight, canvasWidth, t3S ? TeraViivaBrush(plc_T3, 3, normV) : new SolidColorBrush(lepo), $"T3\n{plc_T3:F1}", t3S, false);
                PiirraTeraViiva(canvas, t1X, rectY, rectHeight, canvasWidth, t1S ? TeraViivaBrush(plc_T1, 1, normV) : new SolidColorBrush(lepo), $"T1\n{plc_T1:F1}", t1S, false);
                var vl = new TextBlock { Text = "\u25c4 Vasen saha", Foreground = new SolidColorBrush(normV), FontSize = 11, FontWeight = FontWeights.Bold };
                Canvas.SetLeft(vl, 6); Canvas.SetTop(vl, 6); canvas.Children.Add(vl);
            }
            if (_tukkiHalkaisija > 0)
            {
                double tukkiR = _tukkiHalkaisija / 2.0 * pixelsPerMm;
                canvas.Children.Add(new Ellipse
                {
                    Width = tukkiR * 2,
                    Height = tukkiR * 2,
                    Fill = Brushes.Transparent,
                    Stroke = new SolidColorBrush(Color.FromRgb(180, 140, 60)),
                    StrokeThickness = 1.5,
                    StrokeDashArray = new DoubleCollection { 6, 3 }
                }.Also(e => { Canvas.SetLeft(e, centerX - tukkiR); Canvas.SetTop(e, centerY - tukkiR); }));
            }
            LisaaJakosahaInfoLaatikot(canvas, canvasWidth);
        }

        // ── Profilointi laskenta ───────────────────────────────────────────────

        private void LaskeProfilointiArvot(
            List<double> paksuudet, List<double> leveydet,
            out double profT1, out double profT2,
            out double profT3, out double profT4,
            out double profT5, out double profT6,
            out double profT7, out double profT8)
        {
            if (paksuudet == null || paksuudet.Count == 0)
            {
                profT1 = profT2 = profT3 = profT4 = profT5 = profT6 = profT7 = profT8 = 0;
                return;
            }

            int n = paksuudet.Count;
            int[] teraJarjestys;
            if (OnYhdistettyTila())
            {
                teraJarjestys = n switch
                {
                    3 => new[] { 1, 2 },
                    4 => new[] { 1, 2, 4 },
                    5 => new[] { 1, 3, 2, 4 },
                    6 => new[] { 3, 1, 2, 4, 6 },
                    7 => new[] { 5, 3, 1, 2, 4, 6 },
                    _ => Array.Empty<int>()
                };
            }
            else if (OikeaSahaCheck?.IsChecked == true)
            {
                // Oikea: T2, T4, T6... järjestyksessä
                teraJarjestys = Enumerable.Range(0, n - 1).Select(i => (i + 1) * 2).ToArray();
            }
            else
            {
                // Vasen: T5, T3, T1 — käänteinen järjestys 4kpl = [5,3,1]
                teraJarjestys = n switch { 2 => new[] { 1 }, 3 => new[] { 3, 1 }, 4 => new[] { 5, 3, 1 }, _ => Array.Empty<int>() };
            }
            double[] raot = teraJarjestys.Select(t => GetProfilointiRako(t)).ToArray();
            double kokonaisLeveys = paksuudet.Sum() + raot.Sum();

            double kl = kokonaisLeveys / 2.0;

            double[] leftEdge = new double[n];
            double[] rightEdge = new double[n];
            double cur = -kl;

            for (int i = 0; i < n; i++)
            {
                leftEdge[i] = cur;
                rightEdge[i] = cur + paksuudet[i];
                if (i < n - 1)
                    cur += paksuudet[i] + (i < raot.Length ? raot[i] : DefaultProfilointiRako);
            }

            int t1RefIdx, t2RefIdx;
            if (OnYhdistettyTila())
            {
                t1RefIdx = (_yhdistettyT1RefKappale >= 1 && _yhdistettyT1RefKappale <= n)
                    ? _yhdistettyT1RefKappale - 1 : (n >= 3 ? n - 2 : n - 1);
                t2RefIdx = (_yhdistettyT2RefKappale >= 1 && _yhdistettyT2RefKappale <= n)
                    ? _yhdistettyT2RefKappale - 1 : (n >= 3 ? 1 : 0);
            }
            else
            {
                t1RefIdx = n >= 2 ? n - 2 : 0;
                t2RefIdx = 1;
            }
            profT1 = rightEdge[t1RefIdx];
            profT2 = Math.Abs(leftEdge[t2RefIdx]);  // leftEdge molemmissa tiloissa

            double maxLev = leveydet.Count > 0
                ? leveydet.Where(v => v > 0).DefaultIfEmpty(DefaultProfilointiLeveys).Max()
                : DefaultProfilointiLeveys;
            int oikeaNaapuri = Math.Min(t1RefIdx + 1, n - 1);
            int vasenNaapuri = Math.Max(t2RefIdx - 1, 0);
            double uloinLevOikea = (leveydet.Count > n - 1 && leveydet[n - 1] > 0) ? leveydet[n - 1] : maxLev;
            double uloinLevVasen = (leveydet.Count > 0 && leveydet[0] > 0) ? leveydet[0] : maxLev;

            double offset3 = (maxLev - uloinLevOikea) / 2.0;
            profT3 = offset3 + uloinLevOikea;
            profT4 = offset3;

            double offset5 = (maxLev - uloinLevVasen) / 2.0;
            profT5 = offset5 + uloinLevVasen;
            profT6 = offset5;



            profT7 = rightEdge[n - 1] - profT1;
            profT8 = Math.Abs(leftEdge[0]) - profT2;
        }

        // ── Profilointi canvas (edestäpäin) ───────────────────────────────────

        private void PiirraProfilointiCanvas()
        {
            if (ProfilointiCanvas == null) return;
            var canvas = ProfilointiCanvas;
            canvas.Children.Clear();

            double W = canvas.ActualWidth > 20 ? canvas.ActualWidth : 600;
            double H = canvas.ActualHeight > 20 ? canvas.ActualHeight : 400;
            double cx = W / 2.0, cy = H / 2.0;

            // Uniform scale: fit ±300 mm into available space
            double scale = Math.Min((W / 2.0 - 50) / 240.0, (H - 55) / 360.0);
            if (scale <= 0) scale = 0.5;

            // Black background
            canvas.Children.Add(new Rectangle { Width = W, Height = H, Fill = Brushes.Black });
            bool profKaytossa = _profilointiOn;

            // ── Compute profile positions ─────────────────────────────────────
            LaskeProfilointiArvot(_currentPaksuudet, _currentLeveydet,
                out double profT1, out double profT2,
                out double profT3, out double profT4,
                out double profT5, out double profT6,
                out double profT7, out double profT8);

            if (_currentPaksuudet.Count == 0 || !profKaytossa)
            {
                profT1 = profilointiRajat.TryGetValue("ProfT1", out var p1) ? p1.Lepopaikka : 230.0;
                profT2 = profilointiRajat.TryGetValue("ProfT2", out var p2) ? p2.Lepopaikka : 230.0;
                profT3 = profilointiRajat.TryGetValue("ProfT3", out var p3) ? p3.Lepopaikka : 260.0;
                profT4 = profilointiRajat.TryGetValue("ProfT4", out var p4) ? p4.Lepopaikka : -10.0;
                profT5 = profilointiRajat.TryGetValue("ProfT5", out var p5) ? p5.Lepopaikka : 260.0;
                profT6 = profilointiRajat.TryGetValue("ProfT6", out var p6) ? p6.Lepopaikka : -10.0;
                profT7 = profilointiRajat.TryGetValue("ProfT7", out var p7) ? p7.Lepopaikka : 0.0;
                profT8 = profilointiRajat.TryGetValue("ProfT8", out var p8) ? p8.Lepopaikka : 0.0;
            }

            double t1Pos = profKaytossa ? profT1 + _profOffsetT1 : profT1;
            double t2Pos = profKaytossa ? profT2 + _profOffsetT2 : profT2;
            double t7Pos = profKaytossa ? t1Pos + profT7 + _profOffsetT7 : profT7;
            double t7PlcArvo = profT7 + _profOffsetT7;  // PLC saa suhteellisen arvon
            double t8PlcArvo = profT8 + _profOffsetT8;   // ← tämä rivi puuttuu
            double t8Pos = profKaytossa ? t2Pos + profT8 + _profOffsetT8 : profT8;
            double profT3Y = profKaytossa ? profT3 + _profOffsetT3 : profT3;
            double profT4Y = profKaytossa ? profT4 + _profOffsetT4 : profT4;
            double profT5Y = profKaytossa ? profT5 + _profOffsetT5 : profT5;
            double profT6Y = profKaytossa ? profT6 + _profOffsetT6 : profT6;

            double korkeus = _currentLeveydet.Count > 0
                ? _currentLeveydet.Where(v => v > 0).DefaultIfEmpty(100.0).Max()
                : 100.0;
            if (korkeus <= 0) korkeus = 100.0;
            double zeroY = cy + (korkeus / 2.0) * scale;

            bool RajaYlitetty(string avain, double arvo)
            {
                if (!profilointiRajat.TryGetValue(avain, out var raja)) return false;
                return arvo < raja.Min || arvo > raja.Max;
            }

            bool t1RajaYli = profKaytossa && RajaYlitetty("ProfT1", t1Pos);
            bool t2RajaYli = profKaytossa && RajaYlitetty("ProfT2", t2Pos);
            bool t3RajaYli = profKaytossa && RajaYlitetty("ProfT3", profT3Y);
            bool t4RajaYli = profKaytossa && RajaYlitetty("ProfT4", profT4Y);
            bool t5RajaYli = profKaytossa && RajaYlitetty("ProfT5", profT5Y);
            bool t6RajaYli = profKaytossa && RajaYlitetty("ProfT6", profT6Y);
            bool t7RajaYli = profKaytossa && RajaYlitetty("ProfT7", t7Pos);
            bool t8RajaYli = profKaytossa && RajaYlitetty("ProfT8", t8Pos);

            // ── Horizontal scale (X axis) ─────────────────────────────────────
            PiirraProfilointiHorizAsteikko(canvas, W, H, cx, scale);

            // ── Vertical scale (Y axis) ───────────────────────────────────────
            PiirraProfilointiVertAsteikko(canvas, W, H, cy, scale, zeroY);

            // ── Zero level (green dashed horizontal) ─────────────────────────
            canvas.Children.Add(new Line
            {
                X1 = 30,
                Y1 = zeroY,
                X2 = W - 10,
                Y2 = zeroY,
                Stroke = new SolidColorBrush(Color.FromRgb(0, 180, 0)),
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 4, 3 }
            });

            if (!profKaytossa)
            {
                var eiKaytossa = new TextBlock
                {
                    Text = "Ei käytössä",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(170, 170, 170))
                };
                Canvas.SetLeft(eiKaytossa, 36);
                Canvas.SetTop(eiKaytossa, 10);
                canvas.Children.Add(eiKaytossa);
            }

            // ── Centerline (blue dashed vertical) ────────────────────────────
            canvas.Children.Add(new Line
            {
                X1 = cx,
                Y1 = 20,
                X2 = cx,
                Y2 = H - 30,
                Stroke = new SolidColorBrush(Color.FromRgb(79, 195, 247)),
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 4, 3 }
            });

            // ── Draw pieces (front view: width=paksuus, height=leveys) ────────
            if (_currentPaksuudet.Count > 0)
            {
                int n = _currentPaksuudet.Count;
                int[] teraJarjestys;
                if (OnYhdistettyTila())
                {
                    teraJarjestys = n switch
                    {
                        3 => new[] { 1, 2 },
                        4 => new[] { 1, 2, 4 },
                        5 => new[] { 1, 3, 2, 4 },
                        6 => new[] { 3, 1, 2, 4, 6 },
                        7 => new[] { 5, 3, 1, 2, 4, 6 },
                        _ => Array.Empty<int>()
                    };
                }
                else if (OikeaSahaCheck?.IsChecked == true)
                {
                    teraJarjestys = Enumerable.Range(0, n - 1).Select(i => (i + 1) * 2).ToArray();
                }
                else
                {
                    teraJarjestys = n switch { 2 => new[] { 1 }, 3 => new[] { 3, 1 }, 4 => new[] { 5, 3, 1 }, _ => Array.Empty<int>() };
                }
                double[] raot = teraJarjestys.Select(t => GetProfilointiRako(t)).ToArray();
                double totalPaksuus = _currentPaksuudet.Sum() + raot.Take(Math.Max(0, n - 1)).Sum();
                double startMm = -totalPaksuus / 2.0;
                for (int i = 0; i < _currentPaksuudet.Count; i++)
                {
                    double p = _currentPaksuudet[i];
                    double l = (i < _currentLeveydet.Count && _currentLeveydet[i] > 0) ? _currentLeveydet[i] : korkeus;
                    Color c = PuuVarit[i % PuuVarit.Length];
                    double pieceX = cx + startMm * scale;
                    double pieceW = p * scale;
                    double pieceH = l * scale;
                    canvas.Children.Add(new Rectangle
                    {
                        Width = Math.Max(1, pieceW),
                        Height = Math.Max(1, pieceH),
                        Fill = new SolidColorBrush(Color.FromArgb(210, c.R, c.G, c.B)),
                        Stroke = new SolidColorBrush(PuuReuna),
                        StrokeThickness = 1
                    }.Also(r => { Canvas.SetLeft(r, pieceX); Canvas.SetTop(r, cy - pieceH / 2.0); }));
                    int kNro = OnYhdistettyTila() ? i + 1
     : OikeaSahaCheck?.IsChecked == true ? i + 5
     : n - i;  // vasen saha — käänteinen: K4, K3, K2, K1
                    var kLbl = new TextBlock
                    {
                        Text = $"K{kNro}",
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush(Color.FromRgb(40, 25, 10)),
                        TextAlignment = TextAlignment.Center,
                        Width = Math.Max(1, pieceW)
                    };
                    Canvas.SetLeft(kLbl, pieceX);
                    Canvas.SetTop(kLbl, cy - pieceH / 2.0 + 4);
                    canvas.Children.Add(kLbl);
                    double palaRako = 0.0;
                    if (i < _currentPaksuudet.Count - 1)
                        palaRako = i < raot.Length ? raot[i] : DefaultProfilointiRako;
                    startMm += p + palaRako;
                }
            }

            // ── Log circle (gold dashed) ──────────────────────────────────────
            if (_tukkiHalkaisija > 0)
            {
                double tukkiR = _tukkiHalkaisija / 2.0 * scale;
                canvas.Children.Add(new Ellipse
                {
                    Width = tukkiR * 2,
                    Height = tukkiR * 2,
                    Fill = Brushes.Transparent,
                    Stroke = new SolidColorBrush(Color.FromRgb(212, 168, 67)),
                    StrokeThickness = 1.5,
                    StrokeDashArray = new DoubleCollection { 6, 3 }
                }.Also(e => { Canvas.SetLeft(e, cx - tukkiR); Canvas.SetTop(e, cy - tukkiR); }));
            }

            // ── T7 guide (right, gray rectangle 25mm wide × 50mm tall) ───────
            double t7X = cx + t7Pos * scale;
            double guideW = 20.0 * scale;
            double guideH = 40.0 * scale;
            if (t7X > 0 && t7X < W)
            {
                if (profKaytossa)
                {
                    canvas.Children.Add(new Rectangle
                    {
                        Width = Math.Max(2, guideW),
                        Height = Math.Max(2, guideH),
                        Fill = new SolidColorBrush(Color.FromArgb(160, 100, 100, 100)),
                        Stroke = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                        StrokeThickness = 1
                    }.Also(r => { Canvas.SetLeft(r, t7X); Canvas.SetTop(r, cy - guideH / 2.0); }));
                }
                else
                {
                    canvas.Children.Add(new Line
                    {
                        X1 = t7X,
                        Y1 = 20,
                        X2 = t7X,
                        Y2 = H - 30,
                        Stroke = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                        StrokeThickness = 2,
                        StrokeDashArray = new DoubleCollection { 4, 3 }
                    });
                }
                var t7Lbl = new TextBlock
                {
                    Text = $"T7\n{t7PlcArvo:F1}",
                    FontSize = 9,
                    Foreground = profKaytossa
                        ? t7RajaYli ? Brushes.OrangeRed : new SolidColorBrush(Color.FromRgb(180, 180, 180))
                        : new SolidColorBrush(Color.FromRgb(100, 100, 100))
                };
                Canvas.SetLeft(t7Lbl, t7X + 2);
                Canvas.SetTop(t7Lbl, cy - guideH / 2.0 - 22);
                canvas.Children.Add(t7Lbl);
            }

            // ── T8 guide (left, gray rectangle 25mm wide × 50mm tall) ────────
            double t8X = cx - t8Pos * scale;
            if (t8X > 0 && t8X < W)
            {
                if (profKaytossa)
                {
                    canvas.Children.Add(new Rectangle
                    {
                        Width = Math.Max(2, guideW),
                        Height = Math.Max(2, guideH),
                        Fill = new SolidColorBrush(Color.FromArgb(160, 100, 100, 100)),
                        Stroke = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                        StrokeThickness = 1
                    }.Also(r => { Canvas.SetLeft(r, t8X - guideW); Canvas.SetTop(r, cy - guideH / 2.0); }));
                }
                else
                {
                    canvas.Children.Add(new Line
                    {
                        X1 = t8X,
                        Y1 = 20,
                        X2 = t8X,
                        Y2 = H - 30,
                        Stroke = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                        StrokeThickness = 2,
                        StrokeDashArray = new DoubleCollection { 4, 3 }
                    });
                }
                var t8Lbl = new TextBlock
                {
                    Text = $"T8\n{t8PlcArvo:F1}",
                    FontSize = 9,
                    Foreground = profKaytossa
                        ? t8RajaYli ? Brushes.OrangeRed : new SolidColorBrush(Color.FromRgb(180, 180, 180))
                        : new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                    TextAlignment = TextAlignment.Right,
                    Width = 44
                };
                Canvas.SetLeft(t8Lbl, t8X - guideW - 44);
                Canvas.SetTop(t8Lbl, cy - guideH / 2.0 - 22);
                canvas.Children.Add(t8Lbl);
            }

            // ── T3 (right top, blue horizontal line from T1 to right) ─────────
            double t1X = cx + t1Pos * scale;
            double t2X = cx - t2Pos * scale;

            double t3Y = zeroY - profT3Y * scale;
            double t4Y = zeroY - profT4Y * scale;
            double t5Y = zeroY - profT5Y * scale;
            double t6Y = zeroY - profT6Y * scale;
            if (t3Y > 10 && t3Y < H - 10)
            {
                canvas.Children.Add(new Line
                {
                    X1 = t1X,
                    Y1 = t3Y,
                    X2 = t1X + 120,
                    Y2 = t3Y,
                    Stroke = !profKaytossa
                        ? new SolidColorBrush(Color.FromRgb(100, 100, 100))
                        : t3RajaYli
                            ? Brushes.OrangeRed
                            : new SolidColorBrush(Color.FromRgb(33, 150, 243)),
                    StrokeThickness = 2.5,
                    StrokeDashArray = !profKaytossa ? new DoubleCollection { 4, 3 } : null
                });
                var lbl = new TextBlock
                {
                    Text = $"T3  {profT3Y:F1}",
                    FontSize = 10,
                    FontWeight = FontWeights.Bold,
                    Foreground = !profKaytossa
                        ? new SolidColorBrush(Color.FromRgb(100, 100, 100))
                        : t3RajaYli ? Brushes.OrangeRed : new SolidColorBrush(Color.FromRgb(33, 150, 243))
                };
                Canvas.SetLeft(lbl, t1X + 120); Canvas.SetTop(lbl, t3Y - 16);
                canvas.Children.Add(lbl);
            }

            // ── T4 (right bottom, cyan horizontal line from T1 to right) ──────
            if (t4Y > 10 && t4Y < H - 10)
            {
                canvas.Children.Add(new Line
                {
                    X1 = t1X,
                    Y1 = t4Y,
                    X2 = t1X + 120,
                    Y2 = t4Y,
                    Stroke = !profKaytossa
                        ? new SolidColorBrush(Color.FromRgb(100, 100, 100))
                        : t4RajaYli ? Brushes.OrangeRed : new SolidColorBrush(Color.FromRgb(0, 188, 212)),
                    StrokeThickness = 2.5,
                    StrokeDashArray = !profKaytossa ? new DoubleCollection { 4, 3 } : null
                });
                var lbl = new TextBlock
                {
                    Text = $"T4  {profT4Y:F1}",
                    FontSize = 10,
                    FontWeight = FontWeights.Bold,
                    Foreground = !profKaytossa
                        ? new SolidColorBrush(Color.FromRgb(100, 100, 100))
                        : t4RajaYli ? Brushes.OrangeRed : new SolidColorBrush(Color.FromRgb(0, 188, 212))
                };
                Canvas.SetLeft(lbl, t1X + 120); Canvas.SetTop(lbl, t4Y + 4);
                canvas.Children.Add(lbl);
            }

            // ── T5 (left top, yellow horizontal line from left to T2) ─────────
            if (t5Y > 10 && t5Y < H - 10)
            {
                canvas.Children.Add(new Line
                {
                    X1 = t2X - 120,
                    Y1 = t5Y,
                    X2 = t2X,
                    Y2 = t5Y,
                    Stroke = !profKaytossa
                        ? new SolidColorBrush(Color.FromRgb(100, 100, 100))
                        : t5RajaYli ? Brushes.OrangeRed : new SolidColorBrush(Color.FromRgb(255, 235, 59)),
                    StrokeThickness = 2.5,
                    StrokeDashArray = !profKaytossa ? new DoubleCollection { 4, 3 } : null
                });
                var lbl = new TextBlock
                {
                    Text = $"T5  {profT5Y:F1}",
                    FontSize = 10,
                    FontWeight = FontWeights.Bold,
                    Foreground = !profKaytossa
                        ? new SolidColorBrush(Color.FromRgb(100, 100, 100))
                        : t5RajaYli ? Brushes.OrangeRed : new SolidColorBrush(Color.FromRgb(255, 235, 59))
                };
                Canvas.SetLeft(lbl, t2X - 160); Canvas.SetTop(lbl, t5Y - 16);
                canvas.Children.Add(lbl);
            }

            // ── T6 (left bottom, light gray horizontal line from left to T2) ──
            if (t6Y > 10 && t6Y < H - 10)
            {
                canvas.Children.Add(new Line
                {
                    X1 = t2X - 120,
                    Y1 = t6Y,
                    X2 = t2X,
                    Y2 = t6Y,
                    Stroke = !profKaytossa
                        ? new SolidColorBrush(Color.FromRgb(100, 100, 100))
                        : t6RajaYli ? Brushes.OrangeRed : new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                    StrokeThickness = 2.5,
                    StrokeDashArray = !profKaytossa ? new DoubleCollection { 4, 3 } : null
                });
                var lbl = new TextBlock
                {
                    Text = $"T6  {profT6Y:F1}",
                    FontSize = 10,
                    FontWeight = FontWeights.Bold,
                    Foreground = !profKaytossa
                        ? new SolidColorBrush(Color.FromRgb(100, 100, 100))
                        : t6RajaYli ? Brushes.OrangeRed : new SolidColorBrush(Color.FromRgb(220, 220, 220))
                };
                Canvas.SetLeft(lbl, t2X - 160); Canvas.SetTop(lbl, t6Y + 4);
                canvas.Children.Add(lbl);
            }

            // ── T1 (right, green vertical line) ──────────────────────────────
            if (t1X > 10 && t1X < W - 10)
            {
                var t1Brush = !profKaytossa
    ? new SolidColorBrush(Color.FromRgb(100, 100, 100))
    : t1RajaYli ? Brushes.OrangeRed : new SolidColorBrush(Color.FromRgb(76, 175, 80));
                canvas.Children.Add(new Line { X1 = t1X, Y1 = t3Y - 120, X2 = t1X, Y2 = t3Y, Stroke = t1Brush, StrokeThickness = 2.5, StrokeDashArray = !profKaytossa ? new DoubleCollection { 4, 3 } : null });
                canvas.Children.Add(new Line { X1 = t1X, Y1 = t4Y, X2 = t1X, Y2 = t4Y + 120, Stroke = t1Brush, StrokeThickness = 2.5, StrokeDashArray = !profKaytossa ? new DoubleCollection { 4, 3 } : null });
                var lbl = new TextBlock
                {
                    Text = $"T1\n{t1Pos:F1}",
                    FontSize = 10,
                    FontWeight = FontWeights.Bold,
                    Foreground = !profKaytossa
                        ? new SolidColorBrush(Color.FromRgb(100, 100, 100))
                        : t1RajaYli ? Brushes.OrangeRed : new SolidColorBrush(Color.FromRgb(76, 175, 80))
                };
                Canvas.SetLeft(lbl, t1X + 4); Canvas.SetTop(lbl, t3Y - 160);
                canvas.Children.Add(lbl);
            }

            // ── T2 (left, red vertical line) ──────────────────────────────────
            if (t2X > 10 && t2X < W - 10)
            {
                var t2Brush = !profKaytossa
    ? new SolidColorBrush(Color.FromRgb(100, 100, 100))
    : t2RajaYli ? Brushes.OrangeRed : new SolidColorBrush(Color.FromRgb(244, 67, 54));
                canvas.Children.Add(new Line { X1 = t2X, Y1 = t5Y - 120, X2 = t2X, Y2 = t5Y, Stroke = t2Brush, StrokeThickness = 2.5, StrokeDashArray = !profKaytossa ? new DoubleCollection { 4, 3 } : null });
                canvas.Children.Add(new Line { X1 = t2X, Y1 = t6Y, X2 = t2X, Y2 = t6Y + 120, Stroke = t2Brush, StrokeThickness = 2.5, StrokeDashArray = !profKaytossa ? new DoubleCollection { 4, 3 } : null });
                var lbl = new TextBlock
                {
                    Text = $"T2\n{t2Pos:F1}",
                    FontSize = 10,
                    FontWeight = FontWeights.Bold,
                    Foreground = !profKaytossa
                        ? new SolidColorBrush(Color.FromRgb(100, 100, 100))
                        : t2RajaYli ? Brushes.OrangeRed : new SolidColorBrush(Color.FromRgb(244, 67, 54)),
                    TextAlignment = TextAlignment.Right,
                    Width = 44
                };
                Canvas.SetLeft(lbl, t2X - 44); Canvas.SetTop(lbl, t5Y - 160);
                canvas.Children.Add(lbl);
            }

            if (profKaytossa)
            {
                var profVaroitukset = new List<string>();
                void TarkistaProfRaja(string nimi, double arvo, string avain)
                {
                    if (!profilointiRajat.TryGetValue(avain, out var raja)) return;
                    if (arvo < raja.Min || arvo > raja.Max)
                        profVaroitukset.Add($"{nimi}: {arvo:F1} (raja {raja.Min:F1}…{raja.Max:F1})");
                }
                TarkistaProfRaja("ProfT1", t1Pos, "ProfT1");
                TarkistaProfRaja("ProfT2", t2Pos, "ProfT2");
                TarkistaProfRaja("ProfT3", profT3Y, "ProfT3");
                TarkistaProfRaja("ProfT4", profT4Y, "ProfT4");
                TarkistaProfRaja("ProfT5", profT5Y, "ProfT5");
                TarkistaProfRaja("ProfT6", profT6Y, "ProfT6");
                TarkistaProfRaja("ProfT7", t7Pos, "ProfT7");
                TarkistaProfRaja("ProfT8", t8Pos, "ProfT8");
                if (profVaroitukset.Count > 0)
                    SetStatus($"⚠  Prof raja ylitetty: {string.Join("  |  ", profVaroitukset)}", Colors.OrangeRed);
            }

            if (Prof_Value != null)
                Prof_Value.Text = profKaytossa
                    ?  $"Profilointi T1:{t1Pos:F1}  T2:{t2Pos:F1}  T3:{profT3Y:F1}  T4:{profT4Y:F1}  T5:{profT5Y:F1}  T6:{profT6Y:F1}  T7:{profT7 + _profOffsetT7:F1}  T8:{profT8 + _profOffsetT8:F1}"
                    : $"Prof (lepo): T1:{(profilointiRajat.TryGetValue("ProfT1", out var lp1) ? lp1.Lepopaikka : 0):F1}  T2:{(profilointiRajat.TryGetValue("ProfT2", out var lp2) ? lp2.Lepopaikka : 0):F1}  T3:{(profilointiRajat.TryGetValue("ProfT3", out var lp3) ? lp3.Lepopaikka : 0):F1}  T4:{(profilointiRajat.TryGetValue("ProfT4", out var lp4) ? lp4.Lepopaikka : 0):F1}  T5:{(profilointiRajat.TryGetValue("ProfT5", out var lp5) ? lp5.Lepopaikka : 0):F1}  T6:{(profilointiRajat.TryGetValue("ProfT6", out var lp6) ? lp6.Lepopaikka : 0):F1}  T7:{(profilointiRajat.TryGetValue("ProfT7", out var lp7) ? lp7.Lepopaikka : 0):F1}  T8:{(profilointiRajat.TryGetValue("ProfT8", out var lp8) ? lp8.Lepopaikka : 0):F1}";

            // ── Title ─────────────────────────────────────────────────────────
            var otsikko = new TextBlock
            {
                Text = "\U0001F4D0 Profilointi \u2014 edest\u00e4",
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(179, 157, 219))
            };
            Canvas.SetLeft(otsikko, cx - 80); Canvas.SetTop(otsikko, 4);
            canvas.Children.Add(otsikko);

            // Tallenna profilointiarvot ja päivitä WinCC-ikkuna
            _lastProfT1 = t1Pos; _lastProfT2 = t2Pos;
            _lastProfT3 = profT3Y; _lastProfT4 = profT4Y;
            _lastProfT5 = profT5Y; _lastProfT6 = profT6Y;
            _lastProfT7 = t7PlcArvo; _lastProfT8 = t8PlcArvo;
            KeraaKaikkiPlcArvot();
        }

        private void PiirraProfilointiHorizAsteikko(Canvas canvas, double W, double H,
    double cx, double scale)
        {
            double axisY = H - 25;
            canvas.Children.Add(new Line
            {
                X1 = 30,
                Y1 = axisY,
                X2 = W - 10,
                Y2 = axisY,
                Stroke = new SolidColorBrush(Color.FromRgb(70, 70, 70)),
                StrokeThickness = 1
            });
            for (int mm = 0; mm <= 240; mm += 10)
            {
                bool isMajor = mm % 100 == 0, isMedium = mm % 50 == 0;
                double tickH = isMajor ? 12 : isMedium ? 7 : 3;
                foreach (int sign in (mm == 0 ? new[] { 1 } : new[] { 1, -1 }))
                {
                    double x = cx + sign * mm * scale;
                    if (x < 10 || x > W - 5) continue;
                    canvas.Children.Add(new Line
                    {
                        X1 = x,
                        Y1 = axisY,
                        X2 = x,
                        Y2 = axisY - tickH,
                        Stroke = new SolidColorBrush(isMajor ? Color.FromRgb(150, 150, 150) : Color.FromRgb(70, 70, 70)),
                        StrokeThickness = 1
                    });
                    if (isMajor)
                    {
                        var lbl = new TextBlock
                        {
                            Text = mm.ToString(),
                            FontSize = 9,
                            Foreground = new SolidColorBrush(Color.FromRgb(130, 130, 130)),
                            TextAlignment = TextAlignment.Center,
                            Width = 40
                        };
                        Canvas.SetLeft(lbl, x - 20); Canvas.SetTop(lbl, axisY - 24);
                        canvas.Children.Add(lbl);
                    }
                }
            }
        }

        private void PiirraProfilointiVertAsteikko(Canvas canvas, double W, double H,
    double cy, double scale, double zeroY)
        {
            double axisX = 28;
            canvas.Children.Add(new Line
            {
                X1 = axisX,
                Y1 = 15,
                X2 = axisX,
                Y2 = H - 25,
                Stroke = new SolidColorBrush(Color.FromRgb(70, 70, 70)),
                StrokeThickness = 1
            });
            // Ylöspäin 0 → +280
            for (int mm = 0; mm <= 280; mm += 10)
            {
                bool isMajor = mm % 100 == 0, isMedium = mm % 50 == 0;
                double tickW = isMajor ? 10 : isMedium ? 6 : 3;
                double y = zeroY - mm * scale;
                if (y < 10 || y > H - 28) continue;
                canvas.Children.Add(new Line
                {
                    X1 = axisX,
                    Y1 = y,
                    X2 = axisX + tickW,
                    Y2 = y,
                    Stroke = new SolidColorBrush(isMajor ? Color.FromRgb(150, 150, 150) : Color.FromRgb(70, 70, 70)),
                    StrokeThickness = 1
                });
                if (isMajor && mm > 0)
                {
                    var lbl = new TextBlock
                    {
                        Text = mm.ToString(),
                        FontSize = 9,
                        Foreground = new SolidColorBrush(Color.FromRgb(130, 130, 130)),
                        TextAlignment = TextAlignment.Right,
                        Width = 26
                    };
                    Canvas.SetLeft(lbl, 0); Canvas.SetTop(lbl, y - 7);
                    canvas.Children.Add(lbl);
                }
            }
            // Alaspäin 0 → -80
            for (int mm = 10; mm <= 80; mm += 10)
            {
                bool isMajor = mm % 100 == 0, isMedium = mm % 50 == 0;
                double tickW = isMajor ? 10 : isMedium ? 6 : 3;
                double y = zeroY + mm * scale;
                if (y < 10 || y > H - 28) continue;
                canvas.Children.Add(new Line
                {
                    X1 = axisX,
                    Y1 = y,
                    X2 = axisX + tickW,
                    Y2 = y,
                    Stroke = new SolidColorBrush(isMajor ? Color.FromRgb(150, 150, 150) : Color.FromRgb(70, 70, 70)),
                    StrokeThickness = 1
                });
                if (isMajor)
                {
                    var lbl = new TextBlock
                    {
                        Text = $"-{mm}",
                        FontSize = 9,
                        Foreground = new SolidColorBrush(Color.FromRgb(130, 130, 130)),
                        TextAlignment = TextAlignment.Right,
                        Width = 26
                    };
                    Canvas.SetLeft(lbl, 0); Canvas.SetTop(lbl, y - 7);
                    canvas.Children.Add(lbl);
                }
            }
        }
        private void PiirraPhCanvas(Canvas canvas,
      double ph1V, double ph1O, double ph2V, double ph2O)
        {
            canvas.Children.Clear();
            double canvasWidth = canvas.ActualWidth > 20 ? canvas.ActualWidth : 900;
            double canvasHeight = canvas.ActualHeight > 20 ? canvas.ActualHeight : 400;
            double centerX = canvasWidth / 2.0, centerY = canvasHeight / 2.0;
            double pixelsPerMm = (canvasWidth / 2.0 - 20) / 350.0;
            double rectHeight = canvasHeight * 0.40, rectY = centerY - rectHeight / 2.0;

            canvas.Children.Add(new Rectangle
            {
                Width = canvasWidth,
                Height = canvasHeight,
                Fill = new SolidColorBrush(Color.FromRgb(18, 15, 12))
            });

            PiirraAsteikko(canvas, canvasWidth, canvasHeight, centerX, pixelsPerMm); // ← LISÄÄ TÄMÄ

            canvas.Children.Add(new Line
            {
                X1 = centerX,
                Y1 = 20,
                X2 = centerX,
                Y2 = canvasHeight - 30,
                Stroke = new SolidColorBrush(Color.FromRgb(79, 195, 247)),
                StrokeThickness = 1.5,
                StrokeDashArray = new DoubleCollection { 6, 3 }
            });

            // PH1 terälinjat
            var ph1Brush = new SolidColorBrush(Color.FromRgb(255, 200, 80));
            double ph1VX = centerX - ph1V * pixelsPerMm, ph1OX = centerX + ph1O * pixelsPerMm;
            canvas.Children.Add(new Line
            {
                X1 = ph1VX,
                Y1 = rectY - 20,
                X2 = ph1VX,
                Y2 = rectY + rectHeight + 20,
                Stroke = ph1Brush,
                StrokeThickness = 3
            });
            canvas.Children.Add(new Line
            {
                X1 = ph1OX,
                Y1 = rectY - 20,
                X2 = ph1OX,
                Y2 = rectY + rectHeight + 20,
                Stroke = ph1Brush,
                StrokeThickness = 3
            });
            var ph1VLbl = new TextBlock
            {
                Text = $"PH1V\n{ph1V:F1}",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = ph1Brush,
                TextAlignment = TextAlignment.Right
            };
            Canvas.SetLeft(ph1VLbl, Math.Max(ph1VX - 44, 4)); Canvas.SetTop(ph1VLbl, rectY - 38);
            canvas.Children.Add(ph1VLbl);
            var ph1OLbl = new TextBlock
            {
                Text = $"PH1O\n{ph1O:F1}",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = ph1Brush
            };
            Canvas.SetLeft(ph1OLbl, ph1OX + 4); Canvas.SetTop(ph1OLbl, rectY - 38);
            canvas.Children.Add(ph1OLbl);

            // PH2 terälinjat
            var ph2Brush = new SolidColorBrush(Color.FromRgb(80, 200, 255));
            double ph2VX = centerX - ph2V * pixelsPerMm, ph2OX = centerX + ph2O * pixelsPerMm;
            canvas.Children.Add(new Line
            {
                X1 = ph2VX,
                Y1 = rectY - 20,
                X2 = ph2VX,
                Y2 = rectY + rectHeight + 20,
                Stroke = ph2Brush,
                StrokeThickness = 3
            });
            canvas.Children.Add(new Line
            {
                X1 = ph2OX,
                Y1 = rectY - 20,
                X2 = ph2OX,
                Y2 = rectY + rectHeight + 20,
                Stroke = ph2Brush,
                StrokeThickness = 3
            });
            var ph2VLbl = new TextBlock
            {
                Text = $"PH2V\n{ph2V:F1}",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = ph2Brush,
                TextAlignment = TextAlignment.Right
            };
            Canvas.SetLeft(ph2VLbl, Math.Max(ph2VX - 44, 4)); Canvas.SetTop(ph2VLbl, rectY - 38);
            canvas.Children.Add(ph2VLbl);
            var ph2OLbl = new TextBlock
            {
                Text = $"PH2O\n{ph2O:F1}",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = ph2Brush
            };
            Canvas.SetLeft(ph2OLbl, ph2OX + 4); Canvas.SetTop(ph2OLbl, rectY - 38);
            canvas.Children.Add(ph2OLbl);

            var otsikko = new TextBlock
            {
                Text = "\U0001fab5 Pelkkaparrua \u2014 PH1 & PH2",
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(255, 200, 80))
            };
            Canvas.SetLeft(otsikko, centerX - 120); Canvas.SetTop(otsikko, 6);
            canvas.Children.Add(otsikko);
        }
        private void PiirraJakosahaLepopaikka(Canvas canvas,
    double plc_T1, double plc_T2, double plc_T3,
    double plc_T4, double plc_T5, double plc_T6)
        {
            canvas.Children.Clear();
            double canvasWidth = canvas.ActualWidth > 20 ? canvas.ActualWidth : 900;
            double canvasHeight = canvas.ActualHeight > 20 ? canvas.ActualHeight : 400;
            double centerX = canvasWidth / 2.0, centerY = canvasHeight / 2.0;
            double pixelsPerMm = (canvasWidth / 2.0 - 20) / 350.0;
            double rectHeight = canvasHeight * 0.45, rectY = centerY - rectHeight / 2.0;

            canvas.Children.Add(new Rectangle
            {
                Width = canvasWidth,
                Height = canvasHeight,
                Fill = new SolidColorBrush(Color.FromRgb(18, 15, 12))
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
                StrokeDashArray = new DoubleCollection { 6, 3 }
            });

            var lepo = new SolidColorBrush(Color.FromRgb(100, 100, 100));

            double t3X = centerX - plc_T3 * pixelsPerMm;
            double t1X = centerX - (plc_T3 + plc_T1) * pixelsPerMm;
            double t5X = centerX - (plc_T3 + plc_T5) * pixelsPerMm;
            double t4X = centerX + plc_T4 * pixelsPerMm;
            double t2X = centerX + (plc_T4 + plc_T2) * pixelsPerMm;
            double t6X = centerX + (plc_T4 + plc_T6) * pixelsPerMm;

            foreach (var (x, label) in new[] {
        (t5X, $"T5\n{plc_T5:F1}"),
        (t3X, $"T3\n{plc_T3:F1}"),
        (t1X, $"T1\n{plc_T1:F1}"),
        (t2X, $"T2\n{plc_T2:F1}"),
        (t4X, $"T4\n{plc_T4:F1}"),
        (t6X, $"T6\n{plc_T6:F1}") })
            {
                if (x < 10 || x > canvasWidth - 10) continue;
                canvas.Children.Add(new Line
                {
                    X1 = x,
                    Y1 = rectY - 20,
                    X2 = x,
                    Y2 = rectY + rectHeight + 20,
                    Stroke = lepo,
                    StrokeThickness = 1.5,
                    StrokeDashArray = new DoubleCollection { 4, 3 }
                });
                bool right = x >= centerX;
                var tb = new TextBlock
                {
                    Text = label,
                    FontSize = 10,
                    Foreground = lepo,
                    TextAlignment = right ? TextAlignment.Left : TextAlignment.Right
                };
                Canvas.SetLeft(tb, right ? x + 4 : x - 44);
                Canvas.SetTop(tb, rectY - 38);
                canvas.Children.Add(tb);
            }

            var lepoTitle = new TextBlock
            {
                Text = "Lepopaikat",
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100))
            };
            Canvas.SetLeft(lepoTitle, centerX - 40); Canvas.SetTop(lepoTitle, 6);
            canvas.Children.Add(lepoTitle);
            LisaaJakosahaInfoLaatikot(canvas, canvasWidth);
        }
        private void PiirraPhLepopaikkaCanvas(Canvas canvas,
    double lepoV, double lepoO, bool isPh1)
        {
            canvas.Children.Clear();
            double W = canvas.ActualWidth > 20 ? canvas.ActualWidth : 900;
            double H = canvas.ActualHeight > 20 ? canvas.ActualHeight : 400;
            double cx = W / 2.0;
            double pixelsPerMm = (W / 2.0 - 20) / 350.0;
            double rectHeight = H * 0.60, rectY = H / 2.0 - rectHeight / 2.0;

            canvas.Children.Add(new Rectangle
            {
                Width = W,
                Height = H,
                Fill = new SolidColorBrush(Color.FromRgb(18, 15, 12))
            });

            PiirraAsteikko(canvas, W, H, cx, pixelsPerMm);

            canvas.Children.Add(new Line
            {
                X1 = cx,
                Y1 = 20,
                X2 = cx,
                Y2 = H - 30,
                Stroke = new SolidColorBrush(Color.FromRgb(79, 195, 247)),
                StrokeThickness = 1.5,
                StrokeDashArray = new DoubleCollection { 6, 3 }
            });

            // Vain yksi terälinjapari — harmaa katkoviiva
            var brush = new SolidColorBrush(Color.FromRgb(100, 100, 100));
            double vX = cx - lepoV * pixelsPerMm;
            double oX = cx + lepoO * pixelsPerMm;

            canvas.Children.Add(new Line
            {
                X1 = vX,
                Y1 = rectY,
                X2 = vX,
                Y2 = rectY + rectHeight,
                Stroke = brush,
                StrokeThickness = 1.5,
                StrokeDashArray = new DoubleCollection { 4, 3 }
            });
            canvas.Children.Add(new Line
            {
                X1 = oX,
                Y1 = rectY,
                X2 = oX,
                Y2 = rectY + rectHeight,
                Stroke = brush,
                StrokeThickness = 1.5,
                StrokeDashArray = new DoubleCollection { 4, 3 }
            });

            string etuliite = isPh1 ? "PH1" : "PH2";
            var vLbl = new TextBlock
            {
                Text = $"{etuliite}V\n{lepoV:F1}",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = brush,
                TextAlignment = TextAlignment.Right
            };
            Canvas.SetLeft(vLbl, Math.Max(vX - 44, 4)); Canvas.SetTop(vLbl, rectY - 38);
            canvas.Children.Add(vLbl);
            var oLbl = new TextBlock
            {
                Text = $"{etuliite}O\n{lepoO:F1}",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = brush
            };
            Canvas.SetLeft(oLbl, oX + 4); Canvas.SetTop(oLbl, rectY - 38);
            canvas.Children.Add(oLbl);
        }
        private static void PiirraAsteikko(Canvas canvas, double canvasWidth,
            double canvasHeight, double centerX, double pixelsPerMm)
        {
            canvas.Children.Add(new Line { X1 = 10, Y1 = canvasHeight - 30, X2 = canvasWidth - 10, Y2 = canvasHeight - 30, Stroke = new SolidColorBrush(Color.FromRgb(80, 80, 80)), StrokeThickness = 1 });
            for (int mm = 0; mm <= 350; mm += 10)
            {
                bool isMajor = mm % 100 == 0, isMedium = mm % 50 == 0;
                double tickH = isMajor ? 14 : isMedium ? 8 : 3;
                foreach (int sign in mm == 0 ? new[] { 1 } : new[] { 1, -1 })
                {
                    double x = centerX + sign * mm * pixelsPerMm;
                    canvas.Children.Add(new Line { X1 = x, Y1 = canvasHeight - 30, X2 = x, Y2 = canvasHeight - 30 - tickH, Stroke = new SolidColorBrush(isMajor ? Color.FromRgb(160, 160, 160) : Color.FromRgb(80, 80, 80)), StrokeThickness = 1 });
                    if (isMajor) { var lbl = new TextBlock { Text = mm.ToString(), FontSize = 9, Foreground = new SolidColorBrush(Color.FromRgb(140, 140, 140)), TextAlignment = TextAlignment.Center, Width = 40 }; Canvas.SetLeft(lbl, x - 20); Canvas.SetTop(lbl, canvasHeight - 28); canvas.Children.Add(lbl); }
                }
            }
            var vasenLbl = new TextBlock { Text = "\u2190 Vasen", FontSize = 9, Foreground = new SolidColorBrush(Color.FromRgb(255, 107, 107)) };
            Canvas.SetLeft(vasenLbl, 4); Canvas.SetTop(vasenLbl, canvasHeight - 28); canvas.Children.Add(vasenLbl);
            var oikeaLbl = new TextBlock { Text = "Oikea \u2192", FontSize = 9, Foreground = new SolidColorBrush(Color.FromRgb(107, 203, 119)) };
            Canvas.SetLeft(oikeaLbl, canvasWidth - 55); Canvas.SetTop(oikeaLbl, canvasHeight - 28); canvas.Children.Add(oikeaLbl);
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
            int count = GetSelectedPieceCount(); var result = new List<double>();
            for (int i = 1; i <= count; i++)
            { if (paksuusTextBoxesVasen.TryGetValue(i, out var tb) && double.TryParse(tb.Text.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture, out double v) && v > 0) result.Add(v); }
            return result;
        }
        private List<double> GetThicknessValuesOikea()
        {
            int count = GetSelectedPieceCount(); var result = new List<double>();
            for (int i = 5; i < 5 + count; i++)
            { if (paksuusTextBoxesOikea.TryGetValue(i, out var tb) && double.TryParse(tb.Text.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture, out double v) && v > 0) result.Add(v); }
            return result;
        }
        private List<double> GetThicknessValuesYhdistetty()
        {
            int count = GetSelectedYhdistettyCount(); var result = new List<double>();
            for (int i = 1; i <= count; i++)
            { if (paksuusTextBoxesYhdistetty.TryGetValue(i, out var tb) && double.TryParse(tb.Text.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture, out double v) && v > 0) result.Add(v); }
            return result;
        }
        private List<double> GetLeveysValuesVasen()
        {
            int count = GetSelectedPieceCount(); var result = new List<double>();
            for (int i = 1; i <= count; i++)
            { if (leveysTextBoxesVasen.TryGetValue(i, out var tb) && double.TryParse(tb.Text.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture, out double v) && v > 0) result.Add(v); else result.Add(0.0); }
            return result;
        }
        private List<double> GetLeveysValuesOikea()
        {
            int count = GetSelectedPieceCount(); var result = new List<double>();
            for (int i = 5; i < 5 + count; i++)
            { if (leveysTextBoxesOikea.TryGetValue(i, out var tb) && double.TryParse(tb.Text.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture, out double v) && v > 0) result.Add(v); else result.Add(0.0); }
            return result;
        }
        private List<double> GetLeveysValuesYhdistetty()
        {
            int count = GetSelectedYhdistettyCount(); var result = new List<double>();
            for (int i = 1; i <= count; i++)
            { if (leveysTextBoxesYhdistetty.TryGetValue(i, out var tb) && double.TryParse(tb.Text.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture, out double v) && v > 0) result.Add(v); else result.Add(0.0); }
            return result;
        }
        private void Ph1OffsetPlus_Click(object sender, RoutedEventArgs e)
        {
            _ph1Offset = Math.Round(_ph1Offset + 0.1, 1);
            if (Ph1OffsetLabel != null) Ph1OffsetLabel.Text = $"{_ph1Offset:F1} mm";
            PiirraVisual();
        }
        private void Ph1OffsetMinus_Click(object sender, RoutedEventArgs e)
        {
            _ph1Offset = Math.Round(_ph1Offset - 0.1, 1);
            if (Ph1OffsetLabel != null) Ph1OffsetLabel.Text = $"{_ph1Offset:F1} mm";
            PiirraVisual();
        }
        private void Ph2OffsetPlus_Click(object sender, RoutedEventArgs e)
        {
            _ph2Offset = Math.Round(_ph2Offset + 0.1, 1);
            if (Ph2OffsetLabel != null) Ph2OffsetLabel.Text = $"{_ph2Offset:F1} mm";
            PiirraVisual();
        }
        private void Ph2OffsetMinus_Click(object sender, RoutedEventArgs e)
        {
            _ph2Offset = Math.Round(_ph2Offset - 0.1, 1);
            if (Ph2OffsetLabel != null) Ph2OffsetLabel.Text = $"{_ph2Offset:F1} mm";
            PiirraVisual();
        }
        private void T1OffsetPlus_Click(object sender, RoutedEventArgs e)
        { _offsetT1 = Math.Round(_offsetT1 + 0.1, 1); if (T1OffsetLabel != null) T1OffsetLabel.Text = $"{_offsetT1:F1}"; PiirraVisual(); }
        private void T1OffsetMinus_Click(object sender, RoutedEventArgs e)
        { _offsetT1 = Math.Round(_offsetT1 - 0.1, 1); if (T1OffsetLabel != null) T1OffsetLabel.Text = $"{_offsetT1:F1}"; PiirraVisual(); }
        private void T2OffsetPlus_Click(object sender, RoutedEventArgs e)
        { _offsetT2 = Math.Round(_offsetT2 + 0.1, 1); if (T2OffsetLabel != null) T2OffsetLabel.Text = $"{_offsetT2:F1}"; PiirraVisual(); }
        private void T2OffsetMinus_Click(object sender, RoutedEventArgs e)
        { _offsetT2 = Math.Round(_offsetT2 - 0.1, 1); if (T2OffsetLabel != null) T2OffsetLabel.Text = $"{_offsetT2:F1}"; PiirraVisual(); }
        private void T3OffsetPlus_Click(object sender, RoutedEventArgs e)
        { _offsetT3 = Math.Round(_offsetT3 + 0.1, 1); if (T3OffsetLabel != null) T3OffsetLabel.Text = $"{_offsetT3:F1}"; PiirraVisual(); }
        private void T3OffsetMinus_Click(object sender, RoutedEventArgs e)
        { _offsetT3 = Math.Round(_offsetT3 - 0.1, 1); if (T3OffsetLabel != null) T3OffsetLabel.Text = $"{_offsetT3:F1}"; PiirraVisual(); }
        private void T4OffsetPlus_Click(object sender, RoutedEventArgs e)
        { _offsetT4 = Math.Round(_offsetT4 + 0.1, 1); if (T4OffsetLabel != null) T4OffsetLabel.Text = $"{_offsetT4:F1}"; PiirraVisual(); }
        private void T4OffsetMinus_Click(object sender, RoutedEventArgs e)
        { _offsetT4 = Math.Round(_offsetT4 - 0.1, 1); if (T4OffsetLabel != null) T4OffsetLabel.Text = $"{_offsetT4:F1}"; PiirraVisual(); }
        private void T5OffsetPlus_Click(object sender, RoutedEventArgs e)
        { _offsetT5 = Math.Round(_offsetT5 + 0.1, 1); if (T5OffsetLabel != null) T5OffsetLabel.Text = $"{_offsetT5:F1}"; PiirraVisual(); }
        private void T5OffsetMinus_Click(object sender, RoutedEventArgs e)
        { _offsetT5 = Math.Round(_offsetT5 - 0.1, 1); if (T5OffsetLabel != null) T5OffsetLabel.Text = $"{_offsetT5:F1}"; PiirraVisual(); }
        private void T6OffsetPlus_Click(object sender, RoutedEventArgs e)
        { _offsetT6 = Math.Round(_offsetT6 + 0.1, 1); if (T6OffsetLabel != null) T6OffsetLabel.Text = $"{_offsetT6:F1}"; PiirraVisual(); }
        private void T6OffsetMinus_Click(object sender, RoutedEventArgs e)
        { _offsetT6 = Math.Round(_offsetT6 - 0.1, 1); if (T6OffsetLabel != null) T6OffsetLabel.Text = $"{_offsetT6:F1}"; PiirraVisual(); }

        // ── Profilointi-offsetit ──────────────────────────────────────────────

        private void ProfOffsetChange(ref double field, double delta, double min, double max,
            TextBlock? label, string varoitusNimi)
        {
            double newVal = Math.Round(field + delta, 1);
            if (newVal < min || newVal > max)
            {
                SetStatus($"\u26a0  {varoitusNimi}: raja [{min:F0}\u2026{max:F0}] mm", Colors.OrangeRed);
                return;
            }
            field = newVal;
            if (label != null) label.Text = $"{field:F1}";
            PiirraProfilointiCanvas();
        }

        private void ProfT1OffsetPlus_Click(object sender, RoutedEventArgs e)
        { ProfOffsetChange(ref _profOffsetT1, 0.1, -250, 250, ProfT1OffsetLabel, "T1 prof"); }
        private void ProfT1OffsetMinus_Click(object sender, RoutedEventArgs e)
        { ProfOffsetChange(ref _profOffsetT1, -0.1, -250, 250, ProfT1OffsetLabel, "T1 prof"); }

        private void ProfT2OffsetPlus_Click(object sender, RoutedEventArgs e)
        { ProfOffsetChange(ref _profOffsetT2, 0.1, -250, 250, ProfT2OffsetLabel, "T2 prof"); }
        private void ProfT2OffsetMinus_Click(object sender, RoutedEventArgs e)
        { ProfOffsetChange(ref _profOffsetT2, -0.1, -250, 250, ProfT2OffsetLabel, "T2 prof"); }

        private void ProfT3OffsetPlus_Click(object sender, RoutedEventArgs e)
        { ProfOffsetChange(ref _profOffsetT3, 0.1, -280, 280, ProfT3OffsetLabel, "T3 prof"); }
        private void ProfT3OffsetMinus_Click(object sender, RoutedEventArgs e)
        { ProfOffsetChange(ref _profOffsetT3, -0.1, -280, 280, ProfT3OffsetLabel, "T3 prof"); }

        private void ProfT4OffsetPlus_Click(object sender, RoutedEventArgs e)
        { ProfOffsetChange(ref _profOffsetT4, 0.1, -280, 280, ProfT4OffsetLabel, "T4 prof"); }
        private void ProfT4OffsetMinus_Click(object sender, RoutedEventArgs e)
        { ProfOffsetChange(ref _profOffsetT4, -0.1, -280, 280, ProfT4OffsetLabel, "T4 prof"); }

        private void ProfT5OffsetPlus_Click(object sender, RoutedEventArgs e)
        { ProfOffsetChange(ref _profOffsetT5, 0.1, -280, 280, ProfT5OffsetLabel, "T5 prof"); }
        private void ProfT5OffsetMinus_Click(object sender, RoutedEventArgs e)
        { ProfOffsetChange(ref _profOffsetT5, -0.1, -280, 280, ProfT5OffsetLabel, "T5 prof"); }

        private void ProfT6OffsetPlus_Click(object sender, RoutedEventArgs e)
        { ProfOffsetChange(ref _profOffsetT6, 0.1, -280, 280, ProfT6OffsetLabel, "T6 prof"); }
        private void ProfT6OffsetMinus_Click(object sender, RoutedEventArgs e)
        { ProfOffsetChange(ref _profOffsetT6, -0.1, -280, 280, ProfT6OffsetLabel, "T6 prof"); }

        private void ProfT7OffsetPlus_Click(object sender, RoutedEventArgs e)
        { ProfOffsetChange(ref _profOffsetT7, 0.1, -280, 275, ProfT7OffsetLabel, "T7 prof"); }
        private void ProfT7OffsetMinus_Click(object sender, RoutedEventArgs e)
        { ProfOffsetChange(ref _profOffsetT7, -0.1, -280, 275, ProfT7OffsetLabel, "T7 prof"); }

        private void ProfT8OffsetPlus_Click(object sender, RoutedEventArgs e)
        { ProfOffsetChange(ref _profOffsetT8, 0.1, -275, 280, ProfT8OffsetLabel, "T8 prof"); }
        private void ProfT8OffsetMinus_Click(object sender, RoutedEventArgs e)
        { ProfOffsetChange(ref _profOffsetT8, -0.1, -275, 280, ProfT8OffsetLabel, "T8 prof"); }
        private void TukkiPlus_Click(object sender, RoutedEventArgs e)
        {
            _tukkiHalkaisija = Math.Round(_tukkiHalkaisija + 1.0, 0);
            if (TukkiHalkaisijaaLabel != null) TukkiHalkaisijaaLabel.Text = $"{_tukkiHalkaisija:F0}";
            PiirraVisual();
        }
        private void TukkiMinus_Click(object sender, RoutedEventArgs e)
        {
            _tukkiHalkaisija = Math.Round(_tukkiHalkaisija - 1.0, 0);
            if (TukkiHalkaisijaaLabel != null) TukkiHalkaisijaaLabel.Text = $"{_tukkiHalkaisija:F0}";
            PiirraVisual();
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

    internal static class UiExtensions
    {
        public static T Also<T>(this T self, Action<T> action) { action(self); return self; }
    }
}