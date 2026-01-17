namespace Orders.Infrastructure.Caching;

public static class CacheKeys
{
    public const string ProductsPrefix = "products:";
    public const string OrdersPrefix = "orders:";
    public const string CustomersPrefix = "customers:";
    public const string IdempotencyPrefix = "idempotency:";

    public static string Product(Guid id) => $"{ProductsPrefix}{id}";
    public static string ProductsBySku(string sku) => $"{ProductsPrefix}sku:{sku}";
    public static string ProductsAll(int page, int size, bool? isActive) => $"{ProductsPrefix}all:{page}:{size}:{isActive}";

    public static string Order(Guid id) => $"{OrdersPrefix}{id}";
    public static string OrderByNumber(string orderNumber) => $"{OrdersPrefix}number:{orderNumber}";
    public static string OrdersByCustomer(Guid customerId) => $"{OrdersPrefix}customer:{customerId}";

    public static string Customer(Guid id) => $"{CustomersPrefix}{id}";
    public static string CustomerByEmail(string email) => $"{CustomersPrefix}email:{email}";

    public static string Idempotency(string key) => $"{IdempotencyPrefix}{key}";
}
