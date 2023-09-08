namespace MessagePublisher.Models;

public class FlexSpot
{
    public double RxFrequency { get; set; }

    public double TxFrequency { get; set; }

    public string Mode { get; set; } = string.Empty;

    public string Callsign { get; set; } = string.Empty;

    public string Color { get; set; } = "ffff00";

    public string BackgroundColor { get; set; } = String.Empty;

    public string Source { get; set; } = string.Empty;

    public string SpotterCallsign { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.Now;

    public int LifetimeSeconds { get; set; } = 30;

    public string Comment { get; set; } = string.Empty;

    public int Priority { get; set; } = 5;

    public string TriggerAction { get; set; } = "tune";
}