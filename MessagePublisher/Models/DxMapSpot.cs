using System.Text.RegularExpressions;

namespace MessagePublisher.Models;

public class DxMapSpot
{
    public string SpotterCallsign { get; set; } = String.Empty;
    public string SpotterGrid { get; set; } = String.Empty;
    public string DxCallsign { get; set; } = String.Empty;
    public string DxGrid { get; set; } = String.Empty;
    
    /// <summary>
    /// Frequency in Hertz
    /// </summary>
    public ulong Freq { get; set; }

    /// <summary>
    /// Mode i.e. FT8 FT4
    /// </summary>
    public string Mode { get; set; } = String.Empty;

    public double Snr { get; set; }

    public int Df { get; set; }

    public bool IsCq { get; set; }

    public bool IsSpotValid
    {
        get
        {
            if (Freq <= 0) return false;
            if (DxCallsign.Contains('<') || DxCallsign.Contains('>')) return false;
            if (DxCallsign.All(char.IsDigit)) return false;
            if (!DxCallsign.Any(char.IsDigit)) return false;
            if(!Regex.IsMatch(DxGrid, "^[A-Ra-r]{2}[0-9]{2}([A-Xa-x]{2}|$)\\z"))
            {
                return false;
            }
            return true;
        }
    }
    
    public string CreateSpotMessage()
    {
        var spotType = IsCq ? "CQ" : "HRD";
        return $"WSJT|{SpotterCallsign}|{SpotterGrid}|{Freq}|{DxCallsign}|{DxGrid}|{Mode}|{Snr}|{Df}|{spotType}|2.13";
    }
}