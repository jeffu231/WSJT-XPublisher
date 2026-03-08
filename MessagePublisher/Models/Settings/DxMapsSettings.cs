namespace MessagePublisher.Models.Settings;

public class DxMapsSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public bool Enabled { get; set; }
    public bool SendSpot { get; set; }
    
    public override string ToString()
    {
        return $"DxMaps Settings Host:{Host} - Port:{Port} - Enabled:{Enabled} - SendSpot:{SendSpot}";
    }
}