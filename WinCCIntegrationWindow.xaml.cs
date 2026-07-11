using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using IO = System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace SahanOhjausGUI
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Asetukset (tallennetaan omaan JSON-tiedostoon — ei muuteta TallennusData)
    // ═══════════════════════════════════════════════════════════════════════════

    public enum WinCCVersion { V7V8, Unified }

    public class AlarmBitDefinition
    {
        public int BitIndex { get; set; }
        public string Text { get; set; } = "";
    }

    public class WinCCSettings
    {
        public string ServerName { get; set; } = "localhost";
        public string TagPrefix { get; set; } = "Saha1_";
        public WinCCVersion Version { get; set; } = WinCCVersion.V7V8;
        public bool UseSimulator { get; set; } = true;
        public List<AlarmBitDefinition> AlarmBits { get; set; } = BuildDefaultAlarmBits();

        public static List<AlarmBitDefinition> BuildDefaultAlarmBits() =>
        [
            new() { BitIndex =  0, Text = "Bit 0:  Hätäseis" },
            new() { BitIndex =  1, Text = "Bit 1:  Ylikuormitus" },
            new() { BitIndex =  2, Text = "Bit 2:  Taajuusmuuttajavirhe" },
            new() { BitIndex =  3, Text = "Bit 3:  Akselin positiovirhe" },
            new() { BitIndex =  4, Text = "Bit 4:  Turvapiirivirhe" },
            new() { BitIndex =  5, Text = "Bit 5:  Paineilmavirhe" },
            new() { BitIndex =  6, Text = "Bit 6:  Lämpötilavirhe" },
            new() { BitIndex =  7, Text = "Bit 7:  Kommunikaatiokatko" },
            new() { BitIndex =  8, Text = "Bit 8:  Servovika (T1)" },
            new() { BitIndex =  9, Text = "Bit 9:  Servovika (T2)" },
            new() { BitIndex = 10, Text = "Bit 10: Servovika (T3)" },
            new() { BitIndex = 11, Text = "Bit 11: Servovika (T4)" },
            new() { BitIndex = 12, Text = "Bit 12: Servovika (T5)" },
            new() { BitIndex = 13, Text = "Bit 13: Servovika (T6)" },
            new() { BitIndex = 14, Text = "Bit 14: PH-vika" },
            new() { BitIndex = 15, Text = "Bit 15: Yleinen varoitus" },
        ];
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // IWinCCConnector — yhteinen rajapinta kaikille toteutuksille
    // ═══════════════════════════════════════════════════════════════════════════

    public interface IWinCCConnector
    {
        bool Connect();
        void Disconnect();
        bool WriteTag(string tagName, double value);
        double? ReadTag(string tagName);
        bool IsConnected { get; }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Vaihtoehto A — WinCC V7/V8 COM/OLE -rajapinta
    // ═══════════════════════════════════════════════════════════════════════════

    public class WinCCComConnector : IWinCCConnector
    {
        // Lisää projektiviite: HMIRuntime.dll  (C:\Siemens\WinCC\bin\HMIRuntime.dll)
        // tai: CCOmASOHMIRT.dll
        // Tässä toteutuksessa WinCC Runtime ei ole asennettuna — graceful fallback.

        private readonly string _serverName;
        private bool _connected;

        // private dynamic? _hmiRuntime;  // HMIRuntime.Application -COM-objekti

        public WinCCComConnector(string serverName) => _serverName = serverName;

        public bool IsConnected => _connected;

        public bool Connect()
        {
            try
            {
                // ── COM-aktivointi (paikallinen tai DCOM-verkko) ──────────────
                // var progId = "HMIRuntime.Application";
                // var type   = Type.GetTypeFromProgID(progId, _serverName, throwOnError: true);
                // _hmiRuntime = Activator.CreateInstance(type!);
                // _connected  = _hmiRuntime != null;
                // return _connected;

                // WinCC ei ole asennettuna tässä ympäristössä.
                _connected = false;
                return false;
            }
            catch
            {
                _connected = false;
                return false;
            }
        }

        public void Disconnect()
        {
            // if (_hmiRuntime != null)
            // {
            //     System.Runtime.InteropServices.Marshal.ReleaseComObject(_hmiRuntime);
            //     _hmiRuntime = null;
            // }
            _connected = false;
        }

        public bool WriteTag(string tagName, double value)
        {
            if (!_connected) return false;
            try
            {
                // WinCC V7/V8 kirjoitus:
                // var tag = _hmiRuntime!.Tags[tagName];
                // tag.Value = (float)value;
                // tag.Write(0);   // 0 = synkroninen kirjoitus
                return true;
            }
            catch { return false; }
        }

        public double? ReadTag(string tagName)
        {
            if (!_connected) return null;
            try
            {
                // WinCC V7/V8 luku:
                // var tag = _hmiRuntime!.Tags[tagName];
                // tag.Read(0);    // 0 = synkroninen luku
                // return (double)(float)tag.Value;
                return null;
            }
            catch { return null; }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Vaihtoehto B — WinCC Unified OPC UA / Named Pipe JSON
    // ═══════════════════════════════════════════════════════════════════════════

    public class WinCCUnifiedConnector : IWinCCConnector
    {
        // OPC UA: lisää NuGet-paketti  OPCFoundation.NetStandard.Opc.Ua
        // Named Pipe: System.IO.Pipes (sisäänrakennettu)
        // Tässä toteutuksessa WinCC Unified ei ole asennettuna — graceful fallback.

        private readonly string _serverName;
        private bool _connected;

        // private Opc.Ua.Client.Session? _opcSession;
        // private System.IO.Pipes.NamedPipeClientStream? _pipe;
        // private System.IO.StreamWriter?  _pipeWriter;
        // private System.IO.StreamReader?  _pipeReader;

        public WinCCUnifiedConnector(string serverName) => _serverName = serverName;

        public bool IsConnected => _connected;

        public bool Connect()
        {
            try
            {
                // ── OPC UA ───────────────────────────────────────────────────
                // var endpointUrl = $"opc.tcp://{_serverName}:4840";
                // var config = await ApplicationConfiguration.Load(...);
                // var endpoint = CoreClientUtils.SelectEndpoint(endpointUrl, false, 15000);
                // _opcSession = await Session.Create(config, new ConfiguredEndpoint(null, endpoint), false, "", 60000, null, null);
                // _connected = _opcSession.Connected;

                // ── TAI Named Pipe JSON ──────────────────────────────────────
                // var pipeName = "WinCCUnifiedRT";
                // _pipe = new System.IO.Pipes.NamedPipeClientStream(".", pipeName, System.IO.Pipes.PipeDirection.InOut);
                // _pipe.Connect(5000);
                // _pipeWriter = new System.IO.StreamWriter(_pipe) { AutoFlush = true };
                // _pipeReader = new System.IO.StreamReader(_pipe);
                // _connected = _pipe.IsConnected;

                _connected = false;
                return false;
            }
            catch
            {
                _connected = false;
                return false;
            }
        }

        public void Disconnect()
        {
            // _opcSession?.Close();
            // _pipeWriter?.Close();
            // _pipe?.Close();
            _connected = false;
        }

        public bool WriteTag(string tagName, double value)
        {
            if (!_connected) return false;
            try
            {
                // OPC UA kirjoitus:
                // var nodeId = new Opc.Ua.NodeId($"ns=3;s={tagName}");
                // var writeValue = new Opc.Ua.WriteValue { NodeId = nodeId, AttributeId = Opc.Ua.Attributes.Value, Value = new Opc.Ua.DataValue((float)value) };
                // _opcSession!.Write(null, new Opc.Ua.WriteValueCollection { writeValue }, out var results, out _);
                // return Opc.Ua.StatusCode.IsGood(results[0]);

                // TAI Named Pipe JSON:
                // var req = JsonSerializer.Serialize(new { op = "write", tag = tagName, value = (float)value });
                // _pipeWriter!.WriteLine(req);
                return true;
            }
            catch { return false; }
        }

        public double? ReadTag(string tagName)
        {
            if (!_connected) return null;
            try
            {
                // OPC UA luku:
                // var nodeId = new Opc.Ua.NodeId($"ns=3;s={tagName}");
                // var result = _opcSession!.ReadValue(nodeId);
                // return Convert.ToDouble(result.Value);

                // TAI Named Pipe JSON:
                // var req = JsonSerializer.Serialize(new { op = "read", tag = tagName });
                // _pipeWriter!.WriteLine(req);
                // var line = _pipeReader!.ReadLine() ?? "{}";
                // using var doc = JsonDocument.Parse(line);
                // return doc.RootElement.GetProperty("value").GetDouble();
                return null;
            }
            catch { return null; }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // WinCCSimulator — täysin toimiva simulaattori (ei vaadi WinCC:tä)
    // ═══════════════════════════════════════════════════════════════════════════

    public class WinCCSimulator : IWinCCConnector
    {
        // Actual-arvot liukuvat kohti SetPoint-arvoja nopeudella 10 mm/s.
        // Päivityssykli SimIntervalMs → liike max SimStepMm per sykli.

        private const int    SimIntervalMs = 100;           // 100 ms per sykli
        private const double SimSpeedMmS   = 10.0;          // 10 mm/s
        private const double SimStepMm     = SimSpeedMmS * (SimIntervalMs / 1000.0); // 1 mm

        private readonly Dictionary<string, double> _tags = new(StringComparer.OrdinalIgnoreCase);
        private readonly object _lock = new();
        private Timer? _moveTimer;

        public bool IsConnected => true;

        public bool Connect()
        {
            _moveTimer = new Timer(SimulateMovement, null, 0, SimIntervalMs);
            return true;
        }

        public void Disconnect()
        {
            _moveTimer?.Dispose();
            _moveTimer = null;
        }

        public bool WriteTag(string tagName, double value)
        {
            lock (_lock)
                _tags[tagName] = value;
            return true;
        }

        public double? ReadTag(string tagName)
        {
            lock (_lock)
                return _tags.TryGetValue(tagName, out double v) ? v : 0.0;
        }

        private void SimulateMovement(object? state)
        {
            lock (_lock)
            {
                foreach (var key in _tags.Keys.Where(k => k.EndsWith("_SetPoint", StringComparison.OrdinalIgnoreCase)).ToList())
                {
                    var actualKey = key[..^"_SetPoint".Length] + "_Actual";
                    double sp = _tags[key];
                    double current = _tags.TryGetValue(actualKey, out double a) ? a : sp;
                    double diff = sp - current;
                    _tags[actualKey] = Math.Abs(diff) <= SimStepMm ? sp : current + Math.Sign(diff) * SimStepMm;
                }
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // AxisStatus — tietorakenne DataGridille (INotifyPropertyChanged)
    // ═══════════════════════════════════════════════════════════════════════════

    public class AxisStatus : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private void Notify(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public string Group { get; init; } = "";
        public string Name  { get; init; } = "";

        public string SetPointTag { get; init; } = "";
        public string ActualTag   { get; init; } = "";

        private double _setPoint;
        public double SetPoint
        {
            get => _setPoint;
            set
            {
                if (Math.Abs(_setPoint - value) > 1e-9)
                {
                    _setPoint = value;
                    Notify(nameof(SetPoint));
                    Notify(nameof(Difference));
                    Notify(nameof(IsOk));
                    Notify(nameof(StatusText));
                }
            }
        }

        private double _actual;
        public double Actual
        {
            get => _actual;
            set
            {
                if (Math.Abs(_actual - value) > 1e-9)
                {
                    _actual = value;
                    Notify(nameof(Actual));
                    Notify(nameof(Difference));
                    Notify(nameof(IsOk));
                    Notify(nameof(StatusText));
                }
            }
        }

        public double Difference => Math.Abs(SetPoint - Actual);
        public bool   IsOk       => Difference < 0.2;
        public string StatusText => IsOk ? "OK" : "VIRHE";
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // WinCCIntegrationWindow — pääikkuna
    // ═══════════════════════════════════════════════════════════════════════════

    public partial class WinCCIntegrationWindow : Window
    {
        // ── Vakiot ──────────────────────────────────────────────────────────────
        private static readonly string SettingsPath =
            IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SahanOhjaus", "wincc_settings.json");

        private static readonly JsonSerializerOptions JsonOpts =
            new() { WriteIndented = true, Converters = { new JsonStringEnumConverter() } };

        // ── Tila ────────────────────────────────────────────────────────────────
        private WinCCSettings _settings = new();
        private IWinCCConnector? _connector;

        private readonly ObservableCollection<AxisStatus> _axes = [];
        private readonly double[] _setPoints = new double[AxisDefs.Length];

        private Thread? _pollingThread;
        private volatile bool _polling;
        private Timer?  _heartbeatTimer;
        private int     _heartbeatValue;

        // Akseliryhmien nimet ja taginimien osat (täytetään BuildAxes():ssa)
        private static readonly (string Group, string Name, string TagBase)[] AxisDefs =
        [
            // Jakosaha
            ("Jakosaha", "T1", "Jakosaha_T1"),
            ("Jakosaha", "T2", "Jakosaha_T2"),
            ("Jakosaha", "T3", "Jakosaha_T3"),
            ("Jakosaha", "T4", "Jakosaha_T4"),
            ("Jakosaha", "T5", "Jakosaha_T5"),
            ("Jakosaha", "T6", "Jakosaha_T6"),
            // Pintahakkurit
            ("Pintahakkurit", "PH1 Vasen",  "PH1_Vasen"),
            ("Pintahakkurit", "PH1 Oikea",  "PH1_Oikea"),
            ("Pintahakkurit", "PH2 Vasen",  "PH2_Vasen"),
            ("Pintahakkurit", "PH2 Oikea",  "PH2_Oikea"),
            // Profilointi
            ("Profilointi", "Prof T1", "Prof_T1"),
            ("Profilointi", "Prof T2", "Prof_T2"),
            ("Profilointi", "Prof T3", "Prof_T3"),
            ("Profilointi", "Prof T4", "Prof_T4"),
            ("Profilointi", "Prof T5", "Prof_T5"),
            ("Profilointi", "Prof T6", "Prof_T6"),
            ("Profilointi", "Prof T7", "Prof_T7"),
            ("Profilointi", "Prof T8", "Prof_T8"),
        ];

        // ── Konstruktori ────────────────────────────────────────────────────────
        public WinCCIntegrationWindow()
        {
            InitializeComponent();
            BuildAxes();
            LataaAsetuksetSisaisesti();
            PaivitaAsetusUI();
            AxisGrid.ItemsSource = _axes;
        }

        // ── Akseli-tietorakenteiden luonti ──────────────────────────────────────
        private void BuildAxes()
        {
            _axes.Clear();
            foreach (var (group, name, tagBase) in AxisDefs)
            {
                _axes.Add(new AxisStatus
                {
                    Group       = group,
                    Name        = name,
                    SetPointTag = tagBase + "_SetPoint",
                    ActualTag   = tagBase + "_Actual",
                    SetPoint    = 0.0,
                    Actual      = 0.0,
                });
            }
        }

        public void PaivitaSetPoints(
            double t1, double t2, double t3, double t4, double t5, double t6,
            double ph1V, double ph1O, double ph2V, double ph2O,
            double profT1, double profT2, double profT3, double profT4,
            double profT5, double profT6, double profT7, double profT8)
        {
            var values = new[]
            {
                t1, t2, t3, t4, t5, t6,
                ph1V, ph1O, ph2V, ph2O,
                profT1, profT2, profT3, profT4, profT5, profT6, profT7, profT8
            };

            Dispatcher.InvokeIfRequired(() =>
            {
                for (int i = 0; i < values.Length && i < _setPoints.Length; i++)
                    _setPoints[i] = values[i];

                for (int i = 0; i < values.Length && i < _axes.Count; i++)
                    _axes[i].SetPoint = values[i];
            });
        }

        // Taginnimet muodostetaan dynaamisesti yhteys-/pollausoperaatioissa
        // käyttämällä FullTag()-metodia: prefix + tagBase + "_SetPoint" / "_Actual".
        // Koska SetPointTag- ja ActualTag-kentät ovat init-only (ei sisällä prefixiä),
        // uusi BuildAxes()-kutsu ei ole tarpeen pelkän prefixin vaihtuessa.

        // Palauttaa täyden tagnimen: Prefix + tagBase + "_SetPoint" / "_Actual"
        private string FullTag(string tagBase, bool isSetPoint) =>
            _settings.TagPrefix + tagBase + (isSetPoint ? "_SetPoint" : "_Actual");

        // ── Asetukset: tallennus ja lataus ──────────────────────────────────────
        private void LataaAsetuksetSisaisesti()
        {
            try
            {
                if (IO.File.Exists(SettingsPath))
                {
                    var json = IO.File.ReadAllText(SettingsPath);
                    var loaded = JsonSerializer.Deserialize<WinCCSettings>(json, JsonOpts);
                    if (loaded != null) _settings = loaded;
                }
            }
            catch { /* käytetään oletuksia */ }
        }

        private void TallennaAsetuksetSisaisesti()
        {
            try
            {
                IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(SettingsPath)!);
                IO.File.WriteAllText(SettingsPath, JsonSerializer.Serialize(_settings, JsonOpts));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Asetusten tallennus epäonnistui:\n{ex.Message}",
                    "Virhe", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void PaivitaAsetusUI()
        {
            ServerNameBox.Text = _settings.ServerName;
            TagPrefixBox.Text  = _settings.TagPrefix;
            VersionV7Radio.IsChecked      = _settings.Version == WinCCVersion.V7V8;
            VersionUnifiedRadio.IsChecked = _settings.Version == WinCCVersion.Unified;
            SimulatorCheck.IsChecked      = _settings.UseSimulator;
            AlarmBitList.ItemsSource      = _settings.AlarmBits;
            PaivitaSimulaatioBadge();
        }

        private void LueAsetuksistUI()
        {
            _settings.ServerName    = ServerNameBox.Text.Trim();
            _settings.TagPrefix     = TagPrefixBox.Text.Trim();
            _settings.Version       = VersionV7Radio.IsChecked == true ? WinCCVersion.V7V8 : WinCCVersion.Unified;
            _settings.UseSimulator  = SimulatorCheck.IsChecked == true;
        }

        // ── Yhteyden hallinta ────────────────────────────────────────────────────
        private IWinCCConnector LuoConnector()
        {
            if (_settings.UseSimulator) return new WinCCSimulator();
            return _settings.Version == WinCCVersion.V7V8
                ? new WinCCComConnector(_settings.ServerName)
                : new WinCCUnifiedConnector(_settings.ServerName);
        }

        private void ConnectBtn_Click(object sender, RoutedEventArgs e)
        {
            LueAsetuksistUI();
            PaivitaSimulaatioBadge();

            _connector?.Disconnect();
            _connector = LuoConnector();

            bool ok = _connector.Connect();

            if (ok || _settings.UseSimulator)
            {
                PaivitaYhteysUI(true);
                KaynnistaPollaus();
                KaynnistaSydamen();
            }
            else
            {
                PaivitaYhteysUI(false);
                ConnectionDetailText.Text =
                    $"Yhteys epäonnistui palvelimeen '{_settings.ServerName}'.\n" +
                    "Varmista että WinCC Runtime on käynnissä ja asetukset ovat oikein.";
            }
        }

        private void DisconnectBtn_Click(object sender, RoutedEventArgs e)
        {
            PysaytaPollaus();
            PysaytaSydamen();
            _connector?.Disconnect();
            _connector = null;
            PaivitaYhteysUI(false);
        }

        private void PaivitaYhteysUI(bool connected)
        {
            Dispatcher.InvokeIfRequired(() =>
            {
                bool sim = connected && _settings.UseSimulator;
                ConnectionDot.Fill = connected
                    ? new SolidColorBrush(sim ? Color.FromRgb(255, 193, 7) : Color.FromRgb(76, 175, 80))
                    : new SolidColorBrush(Color.FromRgb(158, 158, 158));
                ConnectionStatusText.Text = connected
                    ? (sim ? "Simulaatiotila" : "Yhdistetty")
                    : "Yhdistämätön";
                ConnectionStatusText.Foreground = new SolidColorBrush(
                    connected
                        ? (sim ? Color.FromRgb(255, 193, 7) : Color.FromRgb(76, 175, 80))
                        : Color.FromRgb(158, 158, 158));
                ConnectBtn.IsEnabled    = !connected;
                DisconnectBtn.IsEnabled = connected;
                LahetaKuvioBtn.IsEnabled = connected;
                ConnectionDetailText.Text = connected
                    ? (sim
                        ? "Simulaattori käynnissä. Actual-arvot liukuvat kohti SetPoint-arvoja (10 mm/s)."
                        : $"Yhdistetty WinCC-palvelimeen '{_settings.ServerName}'. Prefix: '{_settings.TagPrefix}'")
                    : "Ei yhteyttä WinCC-järjestelmään. Tarkista asetukset ja paina Yhdistä.";
            });
        }

        private void PaivitaSimulaatioBadge()
        {
            SimulationBadge.Visibility = _settings.UseSimulator ? Visibility.Visible : Visibility.Collapsed;
        }

        // ── "Lähetä kuvio" ───────────────────────────────────────────────────────
        private void LahetaKuvio_Click(object sender, RoutedEventArgs e)
        {
            LahetaKuvioWinCC();
        }

        public void LahetaKuvioWinCC()
        {
            if (_connector == null) return;

            LahetaKuvioBtn.IsEnabled = false;
            LahetaStatusText.Text = "Lähetetään...";

            var prefix = _settings.TagPrefix;
            var axisSnapshot = _axes.Select(a => (a.SetPointTag, a.SetPoint)).ToList();

            System.Threading.Tasks.Task.Run(() =>
            {
                int ok = 0, fail = 0;
                foreach (var (tag, sp) in axisSnapshot)
                {
                    if (_connector.WriteTag(prefix + tag, sp))
                        ok++;
                    else
                        fail++;
                }

                // Nosta New_Data_Ready -bitti hetkellisesti
                string readyTag = prefix + "New_Data_Ready";
                _connector.WriteTag(readyTag, 1);
                Thread.Sleep(500);
                _connector.WriteTag(readyTag, 0);

                Dispatcher.InvokeIfRequired(() =>
                {
                    LahetaKuvioBtn.IsEnabled = true;
                    LahetaStatusText.Text = fail == 0
                        ? $"✓ Kaikki {ok} SetPoint-tagia lähetetty ({DateTime.Now:HH:mm:ss})"
                        : $"⚠ Lähetetty {ok}/{ok + fail} tagista. {fail} epäonnistui.";
                });
            });
        }

        // ── Reaaliaikainen pollaus ───────────────────────────────────────────────
        private void KaynnistaPollaus()
        {
            _polling = true;
            _pollingThread = new Thread(PollausLoop) { IsBackground = true, Name = "WinCC-pollaus" };
            _pollingThread.Start();
        }

        private void PysaytaPollaus()
        {
            _polling = false;
            _pollingThread?.Join(1000);
            _pollingThread = null;
        }

        private void PollausLoop()
        {
            while (_polling && _connector != null)
            {
                try
                {
                    PollaaAkselit();
                    PollaaHalytykset();
                }
                catch { /* ei kaadu pollauskierroksen virheeseen */ }

                Thread.Sleep(150);
            }
        }

        private void PollaaAkselit()
        {
            var prefix = _settings.TagPrefix;
            var updates = new List<(int Index, double Actual)>();

            for (int i = 0; i < _axes.Count; i++)
            {
                var axis = _axes[i];
                double? val = _connector?.ReadTag(prefix + axis.ActualTag);
                if (val.HasValue) updates.Add((i, val.Value));
            }

            bool allOk = true;
            int failCount = 0;

            Dispatcher.InvokeIfRequired(() =>
            {
                foreach (var (idx, actual) in updates)
                    _axes[idx].Actual = actual;

                foreach (var a in _axes)
                {
                    if (!a.IsOk)
                    {
                        allOk = false;
                        failCount++;
                    }
                }

                PaivitaValmisIndicator(allOk, failCount);
                LastUpdateText.Text = $"Viimeisin päivitys: {DateTime.Now:HH:mm:ss.fff}";
            });
        }

        private void PaivitaValmisIndicator(bool allOk, int failCount)
        {
            if (allOk && _axes.Count > 0)
            {
                ValmisBackground.Color = Color.FromRgb(27, 94, 32);
                ValmisText.Text = "✅  VALMIS SYÖTTEESEEN";
                ValmisText.Foreground = new SolidColorBrush(Color.FromRgb(200, 255, 200));
                ValmisDetailText.Text = "Kaikki 18 akselia ovat tavoiteasemissa (erotus < 0.2 mm).";
            }
            else
            {
                ValmisBackground.Color = Color.FromRgb(62, 30, 10);
                ValmisText.Text = $"⏳  EI VALMIS  ({failCount} akselia poissa paikalta)";
                ValmisText.Foreground = new SolidColorBrush(Color.FromRgb(255, 200, 120));
                ValmisDetailText.Text = "Odotellaan akseleiden asemoitumista (toleranssi < 0.2 mm)...";
            }
        }

        // ── Hälytykset ───────────────────────────────────────────────────────────
        private void PollaaHalytykset()
        {
            var prefix = _settings.TagPrefix;
            double? raw = _connector?.ReadTag(prefix + "Alarm_Word");
            if (raw == null) return;

            int word = (int)raw.Value;
            Dispatcher.InvokeIfRequired(() => PaivitaHalytysnakuma(word));
        }

        private void PaivitaHalytysnakuma(int alarmWord)
        {
            AlarmWordText.Text     = $"0x{alarmWord:X4}";
            AlarmWordDecText.Text  = $"  ({alarmWord})";
            AlarmWordText.Foreground = alarmWord != 0
                ? new SolidColorBrush(Color.FromRgb(244, 67, 54))
                : new SolidColorBrush(Color.FromRgb(76, 175, 80));

            AlarmListPanel.Children.Clear();
            bool anyActive = false;

            for (int bit = 0; bit < 16; bit++)
            {
                if ((alarmWord & (1 << bit)) == 0) continue;
                anyActive = true;

                var def = _settings.AlarmBits.FirstOrDefault(b => b.BitIndex == bit);
                string text = def?.Text ?? $"Bit {bit}: Tuntematon hälytys";

                var tb = new System.Windows.Controls.TextBlock
                {
                    Text       = $"⛔  {text}",
                    Foreground = new SolidColorBrush(Color.FromRgb(244, 67, 54)),
                    FontSize   = 13,
                    FontWeight = FontWeights.SemiBold,
                    Margin     = new Thickness(0, 3, 0, 3),
                };
                AlarmListPanel.Children.Add(tb);
            }

            if (!anyActive)
            {
                AlarmListPanel.Children.Add(new System.Windows.Controls.TextBlock
                {
                    Text       = "Ei aktiivisia hälytyksiä",
                    Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80)),
                    FontSize   = 13,
                    Margin     = new Thickness(0, 4, 0, 4),
                });
            }
        }

        // ── Heartbeat ────────────────────────────────────────────────────────────
        private void KaynnistaSydamen()
        {
            _heartbeatValue = 0;
            _heartbeatTimer = new Timer(HeartbeatTick, null, 0, 1000);
        }

        private void PysaytaSydamen()
        {
            _heartbeatTimer?.Dispose();
            _heartbeatTimer = null;
        }

        private void HeartbeatTick(object? state)
        {
            if (_connector == null) return;
            _connector.WriteTag(_settings.TagPrefix + "HB_Counter", _heartbeatValue++);
        }

        // ── Asetuspainikkeet ─────────────────────────────────────────────────────
        private void TallennaAsetukset_Click(object sender, RoutedEventArgs e)
        {
            LueAsetuksistUI();
            TallennaAsetuksetSisaisesti();
            LahetaStatusText.Text = $"Asetukset tallennettu ({SettingsPath})";
        }

        private void LataaAsetukset_Click(object sender, RoutedEventArgs e)
        {
            LataaAsetuksetSisaisesti();
            PaivitaAsetusUI();
            LahetaStatusText.Text = "Asetukset ladattu tiedostosta.";
        }

        // ── Ikkunan sulkeminen ───────────────────────────────────────────────────
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            PysaytaPollaus();
            PysaytaSydamen();
            _connector?.Disconnect();
        }
    }

    // ─── Dispatcher-helperi ────────────────────────────────────────────────────
    internal static class DispatcherExtensions
    {
        internal static void InvokeIfRequired(this Dispatcher d, Action action)
        {
            if (d.CheckAccess()) action();
            else d.Invoke(action);
        }
    }
}
