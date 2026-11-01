using Microsoft.AspNetCore.Mvc;
using Stockroom.Application.Alerts;

namespace Stockroom.Api.Controllers;

[ApiController]
[Route("api/stock-alerts")]
[Produces("application/json")]
public sealed class StockAlertsController(IStockAlertService stockAlerts) : ControllerBase
{
    /// <summary>Active products whose available stock is at or below their reorder threshold.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<StockAlert>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<StockAlert>>> List(CancellationToken cancellationToken)
    {
        var alerts = await stockAlerts.ListAsync(cancellationToken);
        return Ok(alerts);
    }
}
