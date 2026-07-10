using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SahanOhjausGUI
{
    // ─────────────────────────────────────────────────────────────────────────
    // Asetusluokka (tallennetaan omaan JSON-tiedostoon, erillään TallennusData)
    // ─────────────────────────────────────────────────────────────────────────
    public class WinCCSettings
    {
        public string ServerName { get; set; } = "localhost";
        public string TagPrefix { get; set; } = "Saha1_";
        public string Version { get; set; } = "A";   // "A" tai "B"
        public bool UseSimulator { get; set; } = true;
        public Dictionary<int, string> AlarmTexts { get; set; } = new();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Rajapinta WinCC-yhteydelle
    // ─────────────────────────────────────────────────────────────────────────
    public interface IWinCCConnector
    {
        bool Connect();
        void Disconnect();
        bool WriteTag(string tagName, double value);
        double? ReadTag(string tagName);
        bool IsConnected { get; }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Vaihtoehto A: WinCC V7/V8 COM/OLE-rajapinta (rakenne kommenteilla)
    // ─────────────────────────────────────────────────────────────────────────
    public class WinCCComConnector : IWinCCConnector
    {
        private readonly string _serverName;
        private bool _connected;

        // Tähän lisätään COM-viittaukset, kun WinCC-kirjastot ovat asennettuna:
        // private HMIRuntime _hmiRuntime;

        public WinCCComConnector(string serverName)
        {
            _serverName = serverName;
        }

        public bool IsConnected => _connected;

        public bool Connect()
        {
            try
            {
                // COM-yhteyden muodostus:
                // _hmiRuntime = new HMIRuntime();
                // if (_serverName != "localhost")
                //     _hmiRuntime.ActiveProject.Open(_serverName);
                // _connected = true;

                // Graceful fallback ilman asennettua WinCC:tä:
                _connected = false;
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WinCC COM connect failed: {ex.Message}");
                _connected = false;
                return false;
            }
        }

        public void Disconnect()
        {
            try
            {
                // _hmiRuntime?.Close();
                _connected = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WinCC COM disconnect error: {ex.Message}");
            }
        }

        public bool WriteTag(string tagName, double value)
        {
            if (!_connected) return false;
            try
            {
                // HMIRuntime.Tags[tagName].Write(value);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WinCC COM WriteTag '{tagName}' failed: {ex.Message}");
                return false;
            }
        }

        public double? ReadTag(string tagName)
        {
            if (!_connected) return null;
            try
            {
                // var tag = HMIRuntime.Tags[tagName];
                // tag.Read();
                // return Convert.ToDouble(tag.Value);
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WinCC COM ReadTag '{tagName}' failed: {ex.Message}");
                return null;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Vaihtoehto B: WinCC Unified – OPC UA / Named Pipe JSON
    // ─────────────────────────────────────────────────────────────────────────
    public class WinCCUnifiedConnector : IWinCCConnector
    {
        private readonly string _serverName;
        private bool _connected;

        // OPC UA tai Named Pipe -asiakasolio lisätään tähän:
        // private OpcUaClient _opcClient;
        // private NamedPipeClientStream _pipe;

        public WinCCUnifiedConnector(string serverName)
        {
            _serverName = serverName;
        }

        public bool IsConnected => _connected;

        public bool Connect()
        {
            try
            {
                // Vaihtoehto 1 – OPC UA:
                // var endpointUrl = $"opc.tcp://{_serverName}:4840";
                // _opcClient = new OpcUaClient(endpointUrl);
                // _opcClient.Connect();
                // _connected = _opcClient.IsConnected;

                // Vaihtoehto 2 – Named Pipe JSON:
                // _pipe = new NamedPipeClientStream(".", "WinCCUnifiedPipe", PipeDirection.InOut);
                // _pipe.Connect(timeout: 3000);
                // _connected = _pipe.IsConnected;

                _connected = false;
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WinCC Unified connect failed: {ex.Message}");
                _connected = false;
                return false;
            }
        }

        public void Disconnect()
        {
            try
            {
                // _opcClient?.Disconnect();
                // _pipe?.Close();
                _connected = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WinCC Unified disconnect error: {ex.Message}");
            }
        }

        public bool WriteTag(string tagName, double value)
        {
            if (!_connected) return false;
            try
            {
                // OPC UA:
                // var nodeId = $"ns=2;s={tagName}";
                // _opcClient.WriteNode(nodeId, value);

                // Named Pipe JSON:
                // var req = JsonSerializer.Serialize(new { cmd = "write", tag = tagName, val = value });
                // var bytes = Encoding.UTF8.GetBytes(req + "\n");
                // _pipe.Write(bytes, 0, bytes.Length);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WinCC Unified WriteTag '{tagName}' failed: {ex.Message}");
                return false;
            }
        }

        public double? ReadTag(string tagName)
        {
            if (!_connected) return null;
            try
            {
                // OPC UA:
                // var nodeId = $"ns=2;s={tagName}";
                // return Convert.ToDouble(_opcClient.ReadNode(nodeId).Value);

                // Named Pipe JSON:
                // var req = JsonSerializer.Serialize(new { cmd = "read", tag = tagName });
                // ... lähetä & lue vastaus ...
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WinCC Unified ReadTag '{tagName}' failed: {ex.Message}");
                return null;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Simulaattori – täysin toimiva, ei vaadi WinCC:tä
    // ─────────────────────────────────────────────────────────────────────────
    public class WinCCSimulator : IWinCCConnector
    {
        private readonly Dictionary<string, double> _tags = new();
        private readonly object _lock = new();
        public bool IsConnected => true;

        public bool Connect() => true;
        public void Disconnect() { }

        public bool WriteTag(string tagName, double value)
        {
            lock (_lock)
            {
                _tags[tagName] = value;
            }
            return true;
        }

        public double? ReadTag(string tagName)
        {
            lock (_lock)
            {
                return _tags.TryGetValue(tagName, out var v) ? v : 0.0;
            }
        }

        /// <summary>
        /// Liukuttaa Actual-arvoja SetPoint-arvoja kohti (simulaatio, 10 mm/s).
        /// Kutsutaan taustasäikeestä säännöllisesti.
        /// </summary>
        public void StepSimulation(double deltaSeconds)
        {
            const double speed = 10.0; // mm/s
            lock (_lock)
            {
                var tagKeys = new List<string>(_tags.Keys);
                foreach (var key in tagKeys)
                {
                    if (!key.EndsWith("_Actual", StringComparison.OrdinalIgnoreCase)) continue;
                    var spKey = key[..^"_Actual".Length] + "_SetPoint";
                    if (!_tags.TryGetValue(spKey, out var sp)) continue;
                    var act = _tags[key];
                    var diff = sp - act;
                    var maxStep = speed * deltaSeconds;
                    if (Math.Abs(diff) <= maxStep)
                        _tags[key] = sp;
                    else
                        _tags[key] = act + Math.Sign(diff) * maxStep;
                }
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Näkymämalli DataGridille
    // ─────────────────────────────────────────────────────────────────────────
    public class AkseliRivi
    {
        public string AkseliNimi { get; set; } = "";
        public string SetPoint { get; set; } = "—";
        public string Actual { get; set; } = "—";
        public string Erotus { get; set; } = "—";
        public string Tila { get; set; } = "—";
        public SolidColorBrush TilaVari { get; set; } = new(Colors.Gray);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Näkymämalli SetPoint-listalle
    // ─────────────────────────────────────────────────────────────────────────
    public class SetPointItem
    {
        public string Name { get; set; } = "";
        public string ValueStr { get; set; } = "—";
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Hälytysnäkymämalli
    // ─────────────────────────────────────────────────────────────────────────
    public class HalytysRivi
    {
        public string Bitti { get; set; } = "";
        public string Teksti { get; set; } = "";
        public SolidColorBrush Vari { get; set; } = new(Colors.OrangeRed);
        public SolidColorBrush Tausta { get; set; } = new(Color.FromRgb(50, 20, 20));
        public SolidColorBrush Reuna { get; set; } = new(Color.FromRgb(120, 40, 40));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Ikkuna
    // ─────────────────────────────────────────────────────────────────────────
    public partial class WinCCIntegrationWindow : Window
    {
        private static readonly string SettingsFile =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wincc_settings.json");

        private WinCCSettings _settings = new();
        private IWinCCConnector? _connector;

        // Akselin nimet (18 kpl) ja niiden taginimet (ilman etuliitettä)
        private static readonly (string Display, string TagBase)[] Axes =
        {
            ("Jakosaha T1",    "Jakosaha_T1"),
            ("Jakosaha T2",    "Jakosaha_T2"),
            ("Jakosaha T3",    "Jakosaha_T3"),
            ("Jakosaha T4",    "Jakosaha_T4"),
            ("Jakosaha T5",    "Jakosaha_T5"),
            ("Jakosaha T6",    "Jakosaha_T6"),
            ("PH1 Vasen",      "PH1_Vasen"),
            ("PH1 Oikea",      "PH1_Oikea"),
            ("PH2 Vasen",      "PH2_Vasen"),
            ("PH2 Oikea",      "PH2_Oikea"),
            ("Prof T1",        "Prof_T1"),
            ("Prof T2",        "Prof_T2"),
            ("Prof T3",        "Prof_T3"),
            ("Prof T4",        "Prof_T4"),
            ("Prof T5",        "Prof_T5"),
            ("Prof T6",        "Prof_T6"),
            ("Prof T7",        "Prof_T7"),
            ("Prof T8",        "Prof_T8"),
        };

        private static readonly string[] DefaultAlarmTexts =
        {
            "Hätäseis",
            "Ylikuormitus",
            "Asemointivirhe",
            "Taajuusmuuttaja hälytys",
            "Servo virhe",
            "Raja-anturi lauennut",
            "Kommunikaatiovirhe",
            "Lämpötilahälytys",
            "Painehälytys",
            "Virtahälytys",
            "Turvapiiri auki",
            "Luvaton liike",
            "Referenssin menetys",
            "Ohjausvirta puuttuu",
            "Reservi (bitti 14)",
            "Yleinen virhe",
        };

        // Nykyiset SetPoint-arvot (indeksi vastaa Axes[]-taulukkoa)
        private readonly double[] _setPoints = new double[18];
        // Nykyiset Actual-arvot
        private readonly double[] _actuals = new double[18];

        // Taustasäie
        private Thread? _pollingThread;
        private volatile bool _pollingActive;
        private readonly object _connectorLock = new();

        // Heartbeat-laskuri
        private int _heartbeatCounter;

        // Hälytyssana
        private int _alarmWord;

        // Hälytysten TextBoxit asetussivulla
        private readonly System.Windows.Controls.TextBox[] _alarmTextBoxes = new System.Windows.Controls.TextBox[16];

        // Simulaattori (aina käytettävissä)
        private WinCCSimulator? _simulator;

        // DataGrid-rivit
        private readonly ObservableCollection<AkseliRivi> _akseliRivit = new();

        // ─────────────────────────────────────────────────────────────────────
        public WinCCIntegrationWindow()
        {
            InitializeComponent();
            LuoHalytysTekstitKontrollit();
            AkseliTaulukko.ItemsSource = _akseliRivit;
            for (int i = 0; i < 18; i++)
                _akseliRivit.Add(new AkseliRivi { AkseliNimi = Axes[i].Display });
            LataaAsetukset();
            PaivitaAsetusUI();
            PaivitaSetPointListat();
            KaynnistaTaustasaie();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Asetussivun hälytysten TextBox-kontrollit
        // ─────────────────────────────────────────────────────────────────────
        private void LuoHalytysTekstitKontrollit()
        {
            for (int i = 0; i < 16; i++)
            {
                var row = new System.Windows.Controls.StackPanel
                {
                    Orientation = System.Windows.Controls.Orientation.Horizontal,
                    Margin = new Thickness(0, 2, 0, 2)
                };
                var lbl = new System.Windows.Controls.TextBlock
                {
                    Text = $"Bitti {i}:",
                    Width = 56,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                    FontSize = 11
                };
                var tb = new System.Windows.Controls.TextBox
                {
                    Width = 160,
                    Height = 24,
                    FontSize = 11,
                    Margin = new Thickness(4, 0, 8, 0)
                };
                _alarmTextBoxes[i] = tb;
                row.Children.Add(lbl);
                row.Children.Add(tb);

                if (i < 8)
                    HalytysTekstitVasen.Children.Add(row);
                else
                    HalytysTekstitOikea.Children.Add(row);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Asetustiedoston lataus
        // ─────────────────────────────────────────────────────────────────────
        private void LataaAsetukset()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    var json = File.ReadAllText(SettingsFile, Encoding.UTF8);
                    _settings = JsonSerializer.Deserialize<WinCCSettings>(json) ?? new WinCCSettings();
                }
            }
            catch
            {
                _settings = new WinCCSettings();
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Asetusten tallennus
        // ─────────────────────────────────────────────────────────────────────
        private void TallennaAsetukset()
        {
            _settings.ServerName = ServerNimiTextBox.Text.Trim();
            _settings.TagPrefix = TagPrefixTextBox.Text.Trim();
            _settings.Version = VersionARadio.IsChecked == true ? "A" : "B";
            _settings.UseSimulator = SimulaattoriCheckBox.IsChecked == true;

            for (int i = 0; i < 16; i++)
            {
                var txt = _alarmTextBoxes[i].Text.Trim();
                if (!string.IsNullOrEmpty(txt))
                    _settings.AlarmTexts[i] = txt;
                else
                    _settings.AlarmTexts.Remove(i);
            }

            try
            {
                var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFile, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Asetustiedoston tallennus epäonnistui:\n{ex.Message}",
                    "Virhe", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Päivitä UI asetusten mukaan
        // ─────────────────────────────────────────────────────────────────────
        private void PaivitaAsetusUI()
        {
            ServerNimiTextBox.Text = _settings.ServerName;
            TagPrefixTextBox.Text = _settings.TagPrefix;
            VersionARadio.IsChecked = _settings.Version != "B";
            VersionBRadio.IsChecked = _settings.Version == "B";
            SimulaattoriCheckBox.IsChecked = _settings.UseSimulator;

            for (int i = 0; i < 16; i++)
            {
                var def = i < DefaultAlarmTexts.Length ? DefaultAlarmTexts[i] : $"Bitti {i}";
                _alarmTextBoxes[i].Text = _settings.AlarmTexts.TryGetValue(i, out var t) ? t : def;
            }

            // Otsikkorivin tiedot
            YhteysServerNimi.Text = _settings.ServerName;
            YhteysPrefixNimi.Text = _settings.TagPrefix;
            YhteysVersionTeksti.Text = _settings.Version == "A" ? "WinCC V7/V8 (COM/OLE)" : "WinCC Unified (OPC UA / Named Pipe)";
            SimulaatioMerkki.Visibility = _settings.UseSimulator ? Visibility.Visible : Visibility.Collapsed;
        }

        // ─────────────────────────────────────────────────────────────────────
        // SetPoint-listat yhteys-välilehdellä
        // ─────────────────────────────────────────────────────────────────────
        private void PaivitaSetPointListat()
        {
            var jakosaha = new System.Collections.ObjectModel.ObservableCollection<SetPointItem>();
            var ph = new System.Collections.ObjectModel.ObservableCollection<SetPointItem>();
            var prof = new System.Collections.ObjectModel.ObservableCollection<SetPointItem>();

            for (int i = 0; i < 18; i++)
            {
                var item = new SetPointItem
                {
                    Name = Axes[i].Display + ":",
                    ValueStr = _setPoints[i].ToString("F2", CultureInfo.InvariantCulture)
                };
                if (i < 6) jakosaha.Add(item);
                else if (i < 10) ph.Add(item);
                else prof.Add(item);
            }

            JakosahaSPList.ItemsSource = jakosaha;
            PintahakkuriSPList.ItemsSource = ph;
            ProfilointiSPList.ItemsSource = prof;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Tagi-nimen muodostus
        // ─────────────────────────────────────────────────────────────────────
        private string TagName(string baseName) => _settings.TagPrefix + baseName;

        // ─────────────────────────────────────────────────────────────────────
        // Yhdistä / katkaise
        // ─────────────────────────────────────────────────────────────────────
        private void YhdistaNappi_Click(object sender, RoutedEventArgs e)
        {
            TallennaAsetukset();
            PaivitaAsetusUI();

            lock (_connectorLock)
            {
                _connector?.Disconnect();
                _connector = null;
                _simulator = null;

                if (_settings.UseSimulator)
                {
                    _simulator = new WinCCSimulator();
                    _connector = _simulator;
                }
                else if (_settings.Version == "A")
                {
                    _connector = new WinCCComConnector(_settings.ServerName);
                }
                else
                {
                    _connector = new WinCCUnifiedConnector(_settings.ServerName);
                }

                var ok = _connector.Connect();
                PaivitaYhteysStatus(ok || _settings.UseSimulator);
            }
        }

        private void KatkaisNappi_Click(object sender, RoutedEventArgs e)
        {
            lock (_connectorLock)
            {
                _connector?.Disconnect();
                _connector = null;
                _simulator = null;
            }
            PaivitaYhteysStatus(false);
        }

        private void PaivitaYhteysStatus(bool connected)
        {
            Dispatcher.Invoke(() =>
            {
                YhteysLamppu.Fill = connected
                    ? new SolidColorBrush(Color.FromRgb(76, 175, 80))
                    : new SolidColorBrush(Color.FromRgb(220, 83, 83));
                YhteysStatusTeksti.Text = connected ? "Yhdistetty" : "Ei yhteyttä";
                YhdistaNappi.IsEnabled = !connected;
                KatkaisNappi.IsEnabled = connected;
            });
        }

        // ─────────────────────────────────────────────────────────────────────
        // Lähetä kuvio koneelle
        // ─────────────────────────────────────────────────────────────────────
        private void LahetaKuvioNappi_Click(object sender, RoutedEventArgs e)
        {
            IWinCCConnector? connector;
            lock (_connectorLock)
            {
                connector = _connector;
            }

            if (connector == null || !connector.IsConnected)
            {
                LahetysStatusTeksti.Text = "⚠ Ei yhteyttä WinCC:hen. Yhdistä ensin.";
                LahetysStatusTeksti.Foreground = new SolidColorBrush(Colors.Orange);
                return;
            }

            LahetaKuvioNappi.IsEnabled = false;
            LahetysStatusTeksti.Text = "⏳ Lähetetään...";
            LahetysStatusTeksti.Foreground = new SolidColorBrush(Colors.White);

            Task.Run(() =>
            {
                var errors = new List<string>();

                // Kirjoita 18 SetPoint-tagia
                for (int i = 0; i < 18; i++)
                {
                    var tagName = TagName(Axes[i].TagBase + "_SetPoint");
                    if (!connector.WriteTag(tagName, _setPoints[i]))
                        errors.Add(tagName);
                }

                // New_Data_Ready -bitin nostaminen
                var readyTag = TagName("New_Data_Ready");
                connector.WriteTag(readyTag, 1);
                Thread.Sleep(500);
                connector.WriteTag(readyTag, 0);

                Dispatcher.Invoke(() =>
                {
                    LahetaKuvioNappi.IsEnabled = true;
                    if (errors.Count == 0)
                    {
                        LahetysStatusTeksti.Text = $"✓ Kuvio lähetetty onnistuneesti ({DateTime.Now:HH:mm:ss})";
                        LahetysStatusTeksti.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80));
                    }
                    else
                    {
                        LahetysStatusTeksti.Text = $"⚠ Virhe {errors.Count} tagissa: {string.Join(", ", errors)}";
                        LahetysStatusTeksti.Foreground = new SolidColorBrush(Colors.Orange);
                    }
                    PaivitaSetPointListat();
                });
            });
        }

        // ─────────────────────────────────────────────────────────────────────
        // Asetukset-näppäimet
        // ─────────────────────────────────────────────────────────────────────
        private void TallennaAsetukset_Click(object sender, RoutedEventArgs e)
        {
            TallennaAsetukset();
            PaivitaAsetusUI();
            MessageBox.Show("Asetukset tallennettu.", "Tallennettu", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void PalautaOletukset_Click(object sender, RoutedEventArgs e)
        {
            _settings = new WinCCSettings();
            PaivitaAsetusUI();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Taustasäie – pollaus & simulointi
        // ─────────────────────────────────────────────────────────────────────
        private void KaynnistaTaustasaie()
        {
            _pollingActive = true;
            _pollingThread = new Thread(PollingLoop)
            {
                IsBackground = true,
                Name = "WinCCPolling"
            };
            _pollingThread.Start();
        }

        private void PollingLoop()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            long lastHeartbeatMs = 0;

            while (_pollingActive)
            {
                var loopStart = sw.ElapsedMilliseconds;

                IWinCCConnector? connector;
                WinCCSimulator? sim;
                lock (_connectorLock)
                {
                    connector = _connector;
                    sim = _simulator;
                }

                // Simulaation askel
                if (sim != null)
                {
                    sim.StepSimulation(0.150); // n. 150ms sykli
                }

                // Lue Actual-arvot
                if (connector != null && connector.IsConnected)
                {
                    for (int i = 0; i < 18; i++)
                    {
                        var v = connector.ReadTag(TagName(Axes[i].TagBase + "_Actual"));
                        _actuals[i] = v ?? _actuals[i];
                    }

                    // Lue hälytyssana
                    var alarmVal = connector.ReadTag(TagName("Alarm_Word"));
                    _alarmWord = alarmVal.HasValue ? (int)alarmVal.Value : 0;

                    // Heartbeat sekunnin välein
                    if (sw.ElapsedMilliseconds - lastHeartbeatMs >= 1000)
                    {
                        lastHeartbeatMs = sw.ElapsedMilliseconds;
                        _heartbeatCounter++;
                        connector.WriteTag(TagName("HB_Counter"), _heartbeatCounter);
                    }
                }

                // Päivitä UI
                Dispatcher.Invoke(PaivitaUI);

                // Odota loppuun n. 150ms sykli
                var elapsed = (int)(sw.ElapsedMilliseconds - loopStart);
                var wait = Math.Max(10, 150 - elapsed);
                Thread.Sleep(wait);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // UI-päivitys (ajetaan UI-säikeessä)
        // ─────────────────────────────────────────────────────────────────────
        private void PaivitaUI()
        {
            bool connected;
            lock (_connectorLock)
            {
                connected = _connector?.IsConnected == true;
            }

            // Päivitysaika
            PaivitysaikaTeksti.Text = DateTime.Now.ToString("HH:mm:ss.fff");
            HeartbeatTeksti.Text = _heartbeatCounter.ToString();

            // Rivit
            int valmiit = 0;
            for (int i = 0; i < 18; i++)
            {
                var sp = _setPoints[i];
                var act = _actuals[i];
                var diff = sp - act;
                var ok = Math.Abs(diff) < 0.2;
                if (ok) valmiit++;

                var rivi = _akseliRivit[i];
                rivi.AkseliNimi = Axes[i].Display;
                rivi.SetPoint = sp.ToString("F2", CultureInfo.InvariantCulture);
                rivi.Actual = connected ? act.ToString("F2", CultureInfo.InvariantCulture) : "—";
                rivi.Erotus = connected ? diff.ToString("F2", CultureInfo.InvariantCulture) : "—";
                rivi.Tila = connected ? (ok ? "OK" : "VIRHE") : "—";
                rivi.TilaVari = connected
                    ? (ok ? new SolidColorBrush(Color.FromRgb(76, 175, 80))
                          : new SolidColorBrush(Color.FromRgb(220, 83, 83)))
                    : new SolidColorBrush(Colors.Gray);
            }

            // Päivitä DataGrid
            AkseliTaulukko.Items.Refresh();

            // VALMIS SYÖTTEESEEN -indikaattori
            bool valmis = connected && valmiit == 18;
            if (valmis)
            {
                ValmisIsoTeksti.Text = "✓ VALMIS SYÖTTEESEEN — kaikki akselit paikassaan";
                ValmisIsoTeksti.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80));
                ValmisLamppu.Fill = new SolidColorBrush(Color.FromRgb(76, 175, 80));
                ValmisIsoIndikaattori.Background = new SolidColorBrush(Color.FromRgb(10, 30, 10));
                ValmisIsoIndikaattori.BorderBrush = new SolidColorBrush(Color.FromRgb(40, 100, 40));
                ValmistusTeksti.Text = "✓ VALMIS SYÖTTEESEEN";
                ValmistusTeksti.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80));
            }
            else
            {
                ValmisIsoTeksti.Text = connected
                    ? $"⏳ ODOTTAA — akselit paikassaan: {valmiit} / 18"
                    : "⏳ ODOTTAA — akselit eivät paikassaan";
                ValmisIsoTeksti.Foreground = new SolidColorBrush(Color.FromRgb(200, 160, 60));
                ValmisLamppu.Fill = new SolidColorBrush(Color.FromRgb(100, 100, 100));
                ValmisIsoIndikaattori.Background = new SolidColorBrush(Color.FromRgb(26, 26, 26));
                ValmisIsoIndikaattori.BorderBrush = new SolidColorBrush(Color.FromRgb(51, 51, 51));
                ValmistusTeksti.Text = "⏳ ODOTTAA";
                ValmistusTeksti.Foreground = new SolidColorBrush(Color.FromRgb(136, 136, 136));
            }

            ToleranceTeksti.Text = $"Toleranssi: ±0.2 mm | Akselit paikassaan: {valmiit} / 18";

            // Hälytykset
            PaivitaHalytykset();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Hälytyssanan dekoodaus
        // ─────────────────────────────────────────────────────────────────────
        private void PaivitaHalytykset()
        {
            AlarmWordHex.Text = $"0x{_alarmWord:X4}";
            AlarmWordDec.Text = _alarmWord.ToString();

            var rivit = new System.Collections.ObjectModel.ObservableCollection<HalytysRivi>();
            for (int bit = 0; bit < 16; bit++)
            {
                bool active = (_alarmWord & (1 << bit)) != 0;
                if (!active) continue;

                string text = _settings.AlarmTexts.TryGetValue(bit, out var t) ? t
                    : (bit < DefaultAlarmTexts.Length ? DefaultAlarmTexts[bit] : $"Bitti {bit}");

                rivit.Add(new HalytysRivi
                {
                    Bitti = $"Bitti {bit}:",
                    Teksti = text,
                    Vari = new SolidColorBrush(Colors.OrangeRed),
                    Tausta = new SolidColorBrush(Color.FromRgb(50, 20, 20)),
                    Reuna = new SolidColorBrush(Color.FromRgb(120, 40, 40))
                });
            }

            if (rivit.Count == 0)
            {
                rivit.Add(new HalytysRivi
                {
                    Bitti = "—",
                    Teksti = "Ei aktiivisia hälytyksiä",
                    Vari = new SolidColorBrush(Color.FromRgb(76, 175, 80)),
                    Tausta = new SolidColorBrush(Color.FromRgb(10, 30, 10)),
                    Reuna = new SolidColorBrush(Color.FromRgb(40, 100, 40))
                });
            }

            HalytysLista.ItemsSource = rivit;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Julkinen metodi: aseta uudet SetPoint-arvot ohjelmasta
        // ─────────────────────────────────────────────────────────────────────
        public void SetSetPoints(
            IReadOnlyList<double> jakosaha,      // T1–T6 (6 kpl)
            IReadOnlyList<double> pintahakkurit, // PH1V, PH1O, PH2V, PH2O (4 kpl)
            IReadOnlyList<double> profilointi)   // Prof_T1–Prof_T8 (8 kpl)
        {
            for (int i = 0; i < Math.Min(6, jakosaha.Count); i++)
                _setPoints[i] = jakosaha[i];
            for (int i = 0; i < Math.Min(4, pintahakkurit.Count); i++)
                _setPoints[6 + i] = pintahakkurit[i];
            for (int i = 0; i < Math.Min(8, profilointi.Count); i++)
                _setPoints[10 + i] = profilointi[i];

            Dispatcher.InvokeAsync(PaivitaSetPointListat);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Ikkunan sulkeminen
        // ─────────────────────────────────────────────────────────────────────
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _pollingActive = false;
            lock (_connectorLock)
            {
                _connector?.Disconnect();
                _connector = null;
                _simulator = null;
            }
        }
    }
}
