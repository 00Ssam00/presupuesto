namespace Quincena.Application.Transactions;

public class CreateTransactionRequest
{
    public Guid PeriodId { get; set; }
    public Guid CategoryId { get; set; }
    public Guid PaymentMethodId { get; set; }
    public Guid? CardId { get; set; }
    public string Type { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Description { get; set; } = null!;
    public string? Note { get; set; }
    public DateOnly Date { get; set; }
}

// Sin period_id: igual que Node, el PUT no permite mover una transacción de
// quincena (solo POST la fija).
public class UpdateTransactionRequest
{
    public Guid CategoryId { get; set; }
    public Guid PaymentMethodId { get; set; }
    public Guid? CardId { get; set; }
    public string Type { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Description { get; set; } = null!;
    public string? Note { get; set; }
    public DateOnly Date { get; set; }
}
