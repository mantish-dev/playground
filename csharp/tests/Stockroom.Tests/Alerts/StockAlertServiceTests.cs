using Stockroom.Application.Alerts;
using Stockroom.Domain.Entities;
using Stockroom.Tests.Fakes;

namespace Stockroom.Tests.Alerts;

public sealed class StockAlertServiceTests
{
    private readonly InMemoryProductRepository _products = new();
    private readonly RecordingStockAlertNotifier _notifier = new();
    private readonly FixedClock _clock = new(FixedClock.Default);
    private readonly StockAlertService _sut;

    public StockAlertServiceTests()
    {
        _sut = new StockAlertService(_products, _notifier);
    }

    [Fact]
    public async Task List_returns_active_products_at_or_below_their_threshold()
    {
        var low = AddProduct("LOW", stock: 3, threshold: 5);
        var atThreshold = AddProduct("EDGE", stock: 5, threshold: 5);
        AddProduct("PLENTY", stock: 50, threshold: 5);
        var discontinued = AddProduct("GONE", stock: 0, threshold: 5);
        discontinued.Discontinue(_clock.UtcNow);

        var alerts = await _sut.ListAsync(TestContext.Current.CancellationToken);

        Assert.Equal([atThreshold.Id, low.Id], alerts.Select(a => a.ProductId));
    }

    [Fact]
    public async Task Check_notifies_once_with_every_product_that_needs_reordering()
    {
        var low = AddProduct("LOW", stock: 1, threshold: 2);
        var plenty = AddProduct("PLENTY", stock: 10, threshold: 2);

        await _sut.CheckAsync([low, plenty], TestContext.Current.CancellationToken);

        var batch = Assert.Single(_notifier.Batches);
        Assert.Equal(["LOW"], batch.Select(a => a.Sku));
    }

    [Fact]
    public async Task Check_stays_quiet_when_nothing_needs_reordering()
    {
        var plenty = AddProduct("PLENTY", stock: 10, threshold: 2);

        await _sut.CheckAsync([plenty], TestContext.Current.CancellationToken);

        Assert.Empty(_notifier.Batches);
    }

    private Product AddProduct(string sku, int stock, int threshold)
    {
        var product = Product.Create(sku, sku, 1m, stock, _clock.UtcNow);
        product.ChangeReorderThreshold(threshold, _clock.UtcNow);
        _products.Add(product);
        return product;
    }
}
