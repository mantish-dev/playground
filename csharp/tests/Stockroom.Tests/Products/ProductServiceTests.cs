using Microsoft.Extensions.Logging.Abstractions;
using Stockroom.Business.Exceptions;
using Stockroom.Business.Products;
using Stockroom.Application.Products;
using Stockroom.Domain.Exceptions;
using Stockroom.Tests.Fakes;

namespace Stockroom.Tests.Products;

public sealed class ProductServiceTests
{
    private readonly InMemoryProductRepository _products = new();
    private readonly InMemoryUnitOfWork _unitOfWork = new();
    private readonly FixedClock _clock = new(FixedClock.Default);
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _sut = new ProductService(_products, new ProductCatalog(_products, _clock), _unitOfWork, _clock, NullLogger<ProductService>.Instance);
    }

    [Fact]
    public async Task Create_normalises_sku_and_persists()
    {
        var dto = await _sut.CreateAsync(new CreateProductRequest(" widget-001 ", "Widget", 12.5m, 10), TestContext.Current.CancellationToken);

        Assert.Equal("WIDGET-001", dto.Sku);
        Assert.Equal(10, dto.QuantityAvailable);
        Assert.Equal(FixedClock.Default, dto.CreatedAt);
        Assert.Equal(1, _unitOfWork.SaveCount);
        Assert.Single(_products.All);
    }

    [Fact]
    public async Task Create_rejects_duplicate_sku()
    {
        await _sut.CreateAsync(new CreateProductRequest("WIDGET-001", "Widget", 1m, 0), TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.CreateAsync(new CreateProductRequest("widget-001", "Other", 1m, 0), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Create_collects_all_validation_errors()
    {
        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            _sut.CreateAsync(new CreateProductRequest("", "", -1m, -5), TestContext.Current.CancellationToken));

        Assert.Equal(["InitialStock", "Name", "Sku", "UnitPrice"], ex.Errors.Keys.Order(StringComparer.Ordinal));
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Restock_increases_on_hand_and_stamps_update_time()
    {
        var created = await _sut.CreateAsync(new CreateProductRequest("A", "A", 1m, 5), TestContext.Current.CancellationToken);
        _clock.Advance(TimeSpan.FromHours(1));

        var dto = await _sut.RestockAsync(created.Id, new RestockRequest(7), TestContext.Current.CancellationToken);

        Assert.Equal(12, dto.QuantityOnHand);
        Assert.Equal(FixedClock.Default.AddHours(1), dto.UpdatedAt);
    }

    [Fact]
    public async Task Restock_rejects_discontinued_product()
    {
        var created = await _sut.CreateAsync(new CreateProductRequest("A", "A", 1m, 5), TestContext.Current.CancellationToken);
        await _sut.DiscontinueAsync(created.Id, TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<DomainException>(() =>
            _sut.RestockAsync(created.Id, new RestockRequest(1), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Get_unknown_id_throws_not_found()
    {
        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.GetAsync(Guid.NewGuid(), TestContext.Current.CancellationToken));

        Assert.Equal("Product", ex.EntityName);
    }

    [Fact]
    public async Task ListLowStock_returns_active_products_at_or_below_threshold()
    {
        var low = await _sut.CreateAsync(new CreateProductRequest("LOW", "Low", 1m, 2), TestContext.Current.CancellationToken);
        var edge = await _sut.CreateAsync(new CreateProductRequest("EDGE", "Edge", 1m, 5), TestContext.Current.CancellationToken);
        await _sut.CreateAsync(new CreateProductRequest("FULL", "Full", 1m, 50), TestContext.Current.CancellationToken);
        var gone = await _sut.CreateAsync(new CreateProductRequest("GONE", "Gone", 1m, 0), TestContext.Current.CancellationToken);
        await _sut.DiscontinueAsync(gone.Id, TestContext.Current.CancellationToken);

        var result = await _sut.ListLowStockAsync(5, TestContext.Current.CancellationToken);

        Assert.Equal([low.Id, edge.Id], result.Select(p => p.Id));
    }

    [Fact]
    public async Task ListLowStock_rejects_negative_threshold()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _sut.ListLowStockAsync(-1, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task List_active_only_hides_discontinued()
    {
        var keep = await _sut.CreateAsync(new CreateProductRequest("KEEP", "Keep", 1m, 0), TestContext.Current.CancellationToken);
        var drop = await _sut.CreateAsync(new CreateProductRequest("DROP", "Drop", 1m, 0), TestContext.Current.CancellationToken);
        await _sut.DiscontinueAsync(drop.Id, TestContext.Current.CancellationToken);

        var active = await _sut.ListAsync(activeOnly: true, TestContext.Current.CancellationToken);
        var all = await _sut.ListAsync(activeOnly: false, TestContext.Current.CancellationToken);

        Assert.Equal([keep.Id], active.Select(p => p.Id));
        Assert.Equal(2, all.Count);
    }
}
