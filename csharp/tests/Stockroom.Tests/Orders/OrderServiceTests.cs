using Microsoft.Extensions.Logging.Abstractions;
using Stockroom.Application.Common;
using Stockroom.Application.Orders;
using Stockroom.Domain.Entities;
using Stockroom.Domain.Exceptions;
using Stockroom.Tests.Fakes;

namespace Stockroom.Tests.Orders;

public sealed class OrderServiceTests
{
    private readonly InMemoryOrderRepository _orders = new();
    private readonly InMemoryProductRepository _products = new();
    private readonly InMemoryUnitOfWork _unitOfWork = new();
    private readonly FixedClock _clock = new(FixedClock.Default);
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _sut = new OrderService(_orders, _products, _unitOfWork, _clock, NullLogger<OrderService>.Instance);
    }

    [Fact]
    public async Task Place_reserves_stock_and_snapshots_prices()
    {
        var widget = AddProduct("WIDGET", 10m, stock: 10);
        var gadget = AddProduct("GADGET", 2.5m, stock: 4);

        var dto = await _sut.PlaceAsync(Request(widget, 3, gadget, 4), TestContext.Current.CancellationToken);

        Assert.Equal(OrderStatus.Pending, dto.Status);
        Assert.Equal(40m, dto.Total);
        Assert.Equal(7, widget.QuantityAvailable);
        Assert.Equal(0, gadget.QuantityAvailable);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Place_with_insufficient_stock_reserves_nothing()
    {
        var widget = AddProduct("WIDGET", 10m, stock: 2);

        await Assert.ThrowsAsync<InsufficientStockException>(() =>
            _sut.PlaceAsync(Request(widget, 3), TestContext.Current.CancellationToken));

        Assert.Equal(2, widget.QuantityAvailable);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Place_unknown_product_throws_not_found()
    {
        var request = new PlaceOrderRequest("a@b.c", [new PlaceOrderLineRequest(Guid.NewGuid(), 1)]);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.PlaceAsync(request, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Place_validates_email_and_lines()
    {
        var request = new PlaceOrderRequest("not-an-email", [new PlaceOrderLineRequest(Guid.Empty, 0)]);

        var ex = await Assert.ThrowsAsync<ValidationException>(() => _sut.PlaceAsync(request, TestContext.Current.CancellationToken));

        Assert.Contains("CustomerEmail", ex.Errors.Keys);
        Assert.Contains("Lines[0].ProductId", ex.Errors.Keys);
        Assert.Contains("Lines[0].Quantity", ex.Errors.Keys);
    }

    [Fact]
    public async Task Ship_consumes_reserved_stock()
    {
        var widget = AddProduct("WIDGET", 10m, stock: 10);
        var placed = await _sut.PlaceAsync(Request(widget, 4), TestContext.Current.CancellationToken);
        _clock.Advance(TimeSpan.FromDays(1));

        var shipped = await _sut.ShipAsync(placed.Id, TestContext.Current.CancellationToken);

        Assert.Equal(OrderStatus.Shipped, shipped.Status);
        Assert.Equal(FixedClock.Default.AddDays(1), shipped.ShippedAt);
        Assert.Equal(6, widget.QuantityOnHand);
        Assert.Equal(0, widget.QuantityReserved);
    }

    [Fact]
    public async Task Cancel_releases_reserved_stock()
    {
        var widget = AddProduct("WIDGET", 10m, stock: 10);
        var placed = await _sut.PlaceAsync(Request(widget, 4), TestContext.Current.CancellationToken);

        var cancelled = await _sut.CancelAsync(placed.Id, TestContext.Current.CancellationToken);

        Assert.Equal(OrderStatus.Cancelled, cancelled.Status);
        Assert.Equal(10, widget.QuantityAvailable);
        Assert.Equal(0, widget.QuantityReserved);
    }

    [Fact]
    public async Task Shipped_order_cannot_be_cancelled()
    {
        var widget = AddProduct("WIDGET", 10m, stock: 10);
        var placed = await _sut.PlaceAsync(Request(widget, 1), TestContext.Current.CancellationToken);
        await _sut.ShipAsync(placed.Id, TestContext.Current.CancellationToken);

        var ex = await Assert.ThrowsAsync<InvalidOrderStateException>(() =>
            _sut.CancelAsync(placed.Id, TestContext.Current.CancellationToken));

        Assert.Equal(OrderStatus.Shipped, ex.Status);
    }

    [Fact]
    public async Task List_filters_by_status()
    {
        var widget = AddProduct("WIDGET", 10m, stock: 10);
        var first = await _sut.PlaceAsync(Request(widget, 1), TestContext.Current.CancellationToken);
        var second = await _sut.PlaceAsync(Request(widget, 1), TestContext.Current.CancellationToken);
        await _sut.ShipAsync(first.Id, TestContext.Current.CancellationToken);

        var pending = await _sut.ListAsync(OrderStatus.Pending, TestContext.Current.CancellationToken);

        Assert.Equal([second.Id], pending.Select(o => o.Id));
    }

    private Product AddProduct(string sku, decimal price, int stock)
    {
        var product = Product.Create(sku, sku, price, stock, _clock.UtcNow);
        _products.Add(product);
        return product;
    }

    private static PlaceOrderRequest Request(Product product, int quantity)
    {
        return new PlaceOrderRequest("buyer@example.com", [new PlaceOrderLineRequest(product.Id, quantity)]);
    }

    private static PlaceOrderRequest Request(Product first, int firstQty, Product second, int secondQty)
    {
        return new PlaceOrderRequest("buyer@example.com",
        [
            new PlaceOrderLineRequest(first.Id, firstQty),
            new PlaceOrderLineRequest(second.Id, secondQty),
        ]);
    }
}
