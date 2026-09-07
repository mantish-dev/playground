using Stockroom.Business.Validation;

namespace Stockroom.Application.Orders;

internal static class PlaceOrderRequestValidator
{
    private const int MaxLinesPerOrder = 50;

    public static void Validate(PlaceOrderRequest request)
    {
        var errors = new ValidationErrors()
            .RequireNotBlank(nameof(request.CustomerEmail), request.CustomerEmail);

        if (request.CustomerEmail is not null && !request.CustomerEmail.Contains('@', StringComparison.Ordinal))
        {
            errors.Add(nameof(request.CustomerEmail), "CustomerEmail must be a valid e-mail address.");
        }

        if (request.Lines is null || request.Lines.Count == 0)
        {
            errors.Add(nameof(request.Lines), "At least one order line is required.");
        }
        else
        {
            if (request.Lines.Count > MaxLinesPerOrder)
            {
                errors.Add(nameof(request.Lines), $"An order may contain at most {MaxLinesPerOrder} lines.");
            }

            for (var i = 0; i < request.Lines.Count; i++)
            {
                var line = request.Lines[i];
                if (line.ProductId == Guid.Empty)
                {
                    errors.Add($"Lines[{i}].ProductId", "ProductId is required.");
                }

                errors.RequirePositive($"Lines[{i}].Quantity", line.Quantity);
            }
        }

        errors.ThrowIfAny();
    }
}
