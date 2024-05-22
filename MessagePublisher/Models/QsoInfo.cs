namespace MessagePublisher.Models;

public class QsoInfo
{
    public QsoInfo(string dxCall, string dxGrid, double bearing, string deCall, string deGrid)
    {
        DxCall = dxCall;
        DxGrid = dxGrid;
        Bearing = bearing;
        DeCall = deCall;
        DeGrid = deGrid;
    }
    public string DxCall { get; set; }

    public string DxGrid { get; set; }

    public double Bearing { get; set; }

    public string DeCall { get; set; }

    public string DeGrid { get; set; }
    
}