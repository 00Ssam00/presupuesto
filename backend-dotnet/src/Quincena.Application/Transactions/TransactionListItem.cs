namespace Quincena.Application.Transactions;

// Port del SELECT_WITH_JOINS de transactionsController.ts (t.* + category_name/
// category_color/payment_method_name/card_name). El orden de propiedades importa:
// define el orden de las claves en el JSON de salida, igual que Postgres devuelve
// t.* primero y los alias de los JOIN después.
public class TransactionListItem
{
    public Guid Id { get; set; }
    public Guid PeriodId { get; set; }
    public Guid CategoryId { get; set; }
    public Guid PaymentMethodId { get; set; }
    public Guid? CardId { get; set; }
    public string Type { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Description { get; set; } = null!;
    public string? Note { get; set; }
    public DateOnly Date { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CategoryName { get; set; } = null!;
    public string CategoryColor { get; set; } = null!;
    public string PaymentMethodName { get; set; } = null!;
    public string? CardName { get; set; }
}
