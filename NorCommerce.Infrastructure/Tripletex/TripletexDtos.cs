namespace NorCommerce.Infrastructure.Tripletex
{
    // Wrappers (once for all)
    public record TxSingle<T>(T Value);
    public record TxList<T>(List<T> Values);

    // Real data
    public record SessionToken(string Token, DateTime ExpirationDate);

    // Customer
    public record TxCustomer(
        int? Id,
        string Name,
        string? Email,
        string? InvoiceEmail,
        bool IsPrivateIndividual
    );

    // VAT
    public record TxVatType(int Id, string Number, string Name, decimal Percentage);

    // Shared
    public record TxRef(int Id);

    // Order
    public record TxOrder(
        int? Id,
        TxRef Customer,
        DateOnly OrderDate,
        DateOnly DeliveryDate,
        bool IsPrioritizeAmountsIncludingVat,
        List<TxOrderLine> OrderLines);

    // Order Line
    public record TxOrderLine(
        string Description,
        decimal Count,
        decimal UnitPriceIncludingVatCurrency,
        TxRef VatType
    );

    // Invoice
    public record TxInvoice(int Id, int InvoiceNumber);
}
