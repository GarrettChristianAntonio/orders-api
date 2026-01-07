namespace Orders.Domain.ValueObjects;

public sealed record OrderStatus
{
    public static readonly OrderStatus Pending = new("Pending");
    public static readonly OrderStatus Confirmed = new("Confirmed");
    public static readonly OrderStatus Processing = new("Processing");
    public static readonly OrderStatus Shipped = new("Shipped");
    public static readonly OrderStatus Delivered = new("Delivered");
    public static readonly OrderStatus Cancelled = new("Cancelled");

    public string Value { get; }

    private OrderStatus(string value)
    {
        Value = value;
    }

    public static OrderStatus FromString(string status)
    {
        return status.ToLowerInvariant() switch
        {
            "pending" => Pending,
            "confirmed" => Confirmed,
            "processing" => Processing,
            "shipped" => Shipped,
            "delivered" => Delivered,
            "cancelled" => Cancelled,
            _ => throw new ArgumentException($"Invalid order status: {status}", nameof(status))
        };
    }

    public bool CanTransitionTo(OrderStatus newStatus)
    {
        if (this == Cancelled)
        {
            return false;
        }

        if (this == Delivered)
        {
            return false;
        }

        if (newStatus == Cancelled)
        {
            return this == Pending || this == Confirmed;
        }

        return (this, newStatus) switch
        {
            (_, _) when this == Pending && newStatus == Confirmed => true,
            (_, _) when this == Confirmed && newStatus == Processing => true,
            (_, _) when this == Processing && newStatus == Shipped => true,
            (_, _) when this == Shipped && newStatus == Delivered => true,
            _ => false
        };
    }

    public override string ToString() => Value;

    public static implicit operator string(OrderStatus status) => status.Value;
}
