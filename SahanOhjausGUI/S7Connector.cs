using S7.Net;
using System;
using System.Collections.Generic;

namespace SahanOhjausGUI
{
    public class S7Connector : IWinCCConnector
    {
        private Plc? _plc;
        private readonly string _ip;
        private readonly short _rack;
        private readonly short _slot;

        // DB-numero — muuta tarvittaessa
        private const int DbNumber = 856;
        private const int NewDataReadyOffset = 144;

        private static readonly Dictionary<string, int> TagOffsets = new()
        {
            // SetPointit
            { "Saha1_Jakosaha_T1_SetPoint", 0   },
            { "Saha1_Jakosaha_T2_SetPoint", 4   },
            { "Saha1_Jakosaha_T3_SetPoint", 8   },
            { "Saha1_Jakosaha_T4_SetPoint", 12  },
            { "Saha1_Jakosaha_T5_SetPoint", 16  },
            { "Saha1_Jakosaha_T6_SetPoint", 20  },
            { "Saha1_PH1_Vasen_SetPoint",   24  },
            { "Saha1_PH1_Oikea_SetPoint",   28  },
            { "Saha1_PH2_Vasen_SetPoint",   32  },
            { "Saha1_PH2_Oikea_SetPoint",   36  },
            { "Saha1_Prof_T1_SetPoint",     40  },
            { "Saha1_Prof_T2_SetPoint",     44  },
            { "Saha1_Prof_T3_SetPoint",     48  },
            { "Saha1_Prof_T4_SetPoint",     52  },
            { "Saha1_Prof_T5_SetPoint",     56  },
            { "Saha1_Prof_T6_SetPoint",     60  },
            { "Saha1_Prof_T7_SetPoint",     64  },
            { "Saha1_Prof_T8_SetPoint",     68  },
            // Actual-arvot
            { "Saha1_Jakosaha_T1_Actual",   72  },
            { "Saha1_Jakosaha_T2_Actual",   76  },
            { "Saha1_Jakosaha_T3_Actual",   80  },
            { "Saha1_Jakosaha_T4_Actual",   84  },
            { "Saha1_Jakosaha_T5_Actual",   88  },
            { "Saha1_Jakosaha_T6_Actual",   92  },
            { "Saha1_PH1_Vasen_Actual",     96  },
            { "Saha1_PH1_Oikea_Actual",     100 },
            { "Saha1_PH2_Vasen_Actual",     104 },
            { "Saha1_PH2_Oikea_Actual",     108 },
            { "Saha1_Prof_T1_Actual",       112 },
            { "Saha1_Prof_T2_Actual",       116 },
            { "Saha1_Prof_T3_Actual",       120 },
            { "Saha1_Prof_T4_Actual",       124 },
            { "Saha1_Prof_T5_Actual",       128 },
            { "Saha1_Prof_T6_Actual",       132 },
            { "Saha1_Prof_T7_Actual",       136 },
            { "Saha1_Prof_T8_Actual",       140 },
            // Kontrolli
            { "Saha1_HB_Counter",           146 },
            { "Saha1_Alarm_Word",           148 },
        };

        public S7Connector(string ip, short rack = 0, short slot = 1)
        {
            _ip = ip;
            _rack = rack;
            _slot = slot;
        }

        public bool IsConnected => _plc?.IsConnected == true;

        public bool Connect()
        {
            try
            {
                _plc = new Plc(CpuType.S71500, _ip, _rack, _slot);
                _plc.Open();
                return _plc.IsConnected;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"S7 yhteys epäonnistui:\n\nIP: {_ip}\nRack: {_rack}  Slot: {_slot}\n\n{ex.Message}",
                    "Yhteysvirhe",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
                return false;
            }
        }

        public void Disconnect()
        {
            _plc?.Close();
            _plc = null;
        }

        public bool WriteTag(string tagName, double value)
        {
            if (_plc?.IsConnected != true) return false;
            try
            {
                // New_Data_Ready on bitti
                if (tagName.EndsWith("New_Data_Ready"))
                {
                    _plc.Write($"DB{DbNumber}.DBX{NewDataReadyOffset}.0", value != 0);
                    return true;
                }

                if (!TagOffsets.TryGetValue(tagName, out int offset)) return false;

                if (tagName.EndsWith("HB_Counter") || tagName.EndsWith("Alarm_Word"))
                    _plc.Write($"DB{DbNumber}.DBW{offset}", (short)(int)value);
                else
                    _plc.Write($"DB{DbNumber}.DBD{offset}", (float)value);

                return true;
            }
            catch { return false; }
        }

        public double? ReadTag(string tagName)
        {
            if (_plc?.IsConnected != true) return null;
            try
            {
                if (!TagOffsets.TryGetValue(tagName, out int offset)) return null;

                if (tagName.EndsWith("Alarm_Word") || tagName.EndsWith("HB_Counter"))
                    return Convert.ToDouble(_plc.Read($"DB{DbNumber}.DBW{offset}"));
                else
                {
                    // DBD palauttaa uint — täytyy muuntaa float-bittien kautta
                    var raw = _plc.Read($"DB{DbNumber}.DBD{offset}");
                    uint bits = Convert.ToUInt32(raw);
                    float f = BitConverter.ToSingle(BitConverter.GetBytes(bits), 0);
                    return (double)f;
                }
            }
            catch { return null; }
        }
    }
}