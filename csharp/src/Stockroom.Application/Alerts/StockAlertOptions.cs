namespace Stockroom.Application.Alerts;

public sealed class StockAlertOptions
{
    public const string SectionName = "StockAlerts";

    /// <summary>The reorder threshold a new product gets when its request does not name one.</summary>
    public int DefaultReorderThreshold { get; set; } = 5;
}
