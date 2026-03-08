namespace MessagePublisher.Models.Settings;

public class FlexSpotSettings
{
    public string RadioId { get; set; } = string.Empty;

    public string Host { get; set; } = string.Empty;

    public string SpotterCall { get; set; } = string.Empty;

    public bool Enabled { get; set; }

    public override string ToString()
    {
        return $"Flex Spot Settings RadioId:{RadioId} - Host:{Host} - SpotterCall:{SpotterCall} - Enabled:{Enabled}";
    }
}