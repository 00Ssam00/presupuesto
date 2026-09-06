using System.Globalization;
using System.Text.RegularExpressions;
using Quincena.Application.Cards;
using Quincena.Application.Categories;
using Quincena.Application.PaymentMethods;
using Quincena.Application.Periods;
using Quincena.Application.Rollover;
using Quincena.Domain.Entities;

namespace Quincena.Application.Transactions;

// Port de backend/src/controllers/transactionsController.ts (incluye importTransactions).
public partial class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _repository;
    private readonly IRolloverService _rolloverService;
    private readonly IPeriodRepository _periodRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly ICardRepository _cardRepository;

    public TransactionService(
        ITransactionRepository repository,
        IRolloverService rolloverService,
        IPeriodRepository periodRepository,
        ICategoryRepository categoryRepository,
        IPaymentMethodRepository paymentMethodRepository,
        ICardRepository cardRepository)
    {
        _repository = repository;
        _rolloverService = rolloverService;
        _periodRepository = periodRepository;
        _categoryRepository = categoryRepository;
        _paymentMethodRepository = paymentMethodRepository;
        _cardRepository = cardRepository;
    }

    public Task<List<TransactionListItem>> ListAsync(Guid? periodId, string? type, Guid? categoryId) =>
        _repository.ListAsync(periodId, type, categoryId);

    public Task<TransactionListItem?> GetByIdAsync(Guid id) => _repository.GetByIdWithJoinsAsync(id);

    public async Task<Transaction> CreateAsync(CreateTransactionRequest request)
    {
        var transaction = new Transaction
        {
            PeriodId = request.PeriodId,
            CategoryId = request.CategoryId,
            PaymentMethodId = request.PaymentMethodId,
            CardId = request.CardId,
            Type = request.Type,
            Amount = request.Amount,
            Description = request.Description,
            Note = request.Note,
            Date = request.Date,
        };
        var created = await _repository.AddAsync(transaction);
        await _rolloverService.RecalculateAllAsync();
        return created;
    }

    public async Task<Transaction?> UpdateAsync(Guid id, UpdateTransactionRequest request)
    {
        var updated = await _repository.UpdateAsync(id, t =>
        {
            t.CategoryId = request.CategoryId;
            t.PaymentMethodId = request.PaymentMethodId;
            t.CardId = request.CardId;
            t.Type = request.Type;
            t.Amount = request.Amount;
            t.Description = request.Description;
            t.Note = request.Note;
            t.Date = request.Date;
            t.UpdatedAt = DateTime.UtcNow;
        });
        if (updated == null) return null;

        await _rolloverService.RecalculateAllAsync();
        return updated;
    }

    public async Task DeleteAsync(Guid id)
    {
        // Igual que Node: no valida existencia previa, siempre recalcula.
        await _repository.DeleteAsync(id);
        await _rolloverService.RecalculateAllAsync();
    }

    [GeneratedRegex(@"^\d{4}-\d{2}-\d{2}$")]
    private static partial Regex DateFormatRegex();

    private static T? FindByName<T>(List<T> list, Func<T, string> nameSelector, string? name) where T : class
    {
        var needle = (name ?? "").Trim().ToLowerInvariant();
        return list.FirstOrDefault(x => nameSelector(x).ToLowerInvariant() == needle);
    }

    // La quincena de cada fila se resuelve por su fecha (obligatoria en el CSV) contra
    // el rango de las quincenas existentes, no por una única quincena elegida por el
    // usuario — así un archivo que abarca varias quincenas se importa en una sola
    // pasada. Importación parcial: cada fila se valida y reporta por separado.
    //
    // A diferencia de Node (que inserta todas las filas válidas en paralelo con
    // Promise.all), acá se insertan secuencialmente porque un solo DbContext de EF
    // Core no es seguro para operaciones concurrentes — mismo criterio ya aplicado
    // en RolloverDataSource. El orden final de `results` por fila es idéntico.
    public async Task<ImportResult> ImportAsync(List<ImportRow> rows)
    {
        var periods = await _periodRepository.GetAllOrderByStartDateDescAsync();
        var categories = await _categoryRepository.GetActiveOrderedAsync();
        var methods = await _paymentMethodRepository.GetActiveOrderedAsync();
        var cards = await _cardRepository.GetActiveOrderedAsync();

        var results = new ImportRowResult?[rows.Count];
        var toInsert = new List<(int RowIndex, Transaction Transaction)>();

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var rowNum = i + 1;

            var montoRaw = row.Monto ?? "";
            if (string.IsNullOrEmpty(row.Monto) ||
                !decimal.TryParse(montoRaw.Trim(), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var amount) ||
                amount <= 0)
            {
                results[i] = new ImportRowResult { Row = rowNum, Status = "error", Error = $"Monto inválido: \"{montoRaw}\"" };
                continue;
            }

            var type = (row.Tipo ?? "").Trim().ToLowerInvariant();
            if (type != "income" && type != "expense")
            {
                results[i] = new ImportRowResult { Row = rowNum, Status = "error", Error = $"Tipo debe ser \"income\" o \"expense\", no \"{row.Tipo ?? ""}\"" };
                continue;
            }

            var dateStr = (row.Fecha ?? "").Trim();
            // Node solo valida el formato con regex (no la validez calendárica real);
            // acá además exigimos que parseé a una fecha real — divergencia menor y
            // deliberada, sin caso de prueba que dependa de fechas con formato válido
            // pero calendario inválido (ej. mes 13).
            if (!DateFormatRegex().IsMatch(dateStr) || !DateOnly.TryParseExact(dateStr, "yyyy-MM-dd", out var date))
            {
                results[i] = new ImportRowResult { Row = rowNum, Status = "error", Error = $"Fecha requerida e inválida: \"{dateStr}\" (usar YYYY-MM-DD)" };
                continue;
            }

            var period = periods.FirstOrDefault(p => date >= p.StartDate && date <= p.EndDate);
            if (period == null)
            {
                results[i] = new ImportRowResult { Row = rowNum, Status = "error", Error = $"No hay ninguna quincena que incluya la fecha {dateStr}" };
                continue;
            }

            var category = FindByName(categories, c => c.Name, row.Categoria);
            if (category == null)
            {
                results[i] = new ImportRowResult { Row = rowNum, Status = "error", Error = $"Categoría no encontrada: \"{row.Categoria ?? ""}\"" };
                continue;
            }

            var method = FindByName(methods, m => m.Name, row.Metodo);
            if (method == null)
            {
                results[i] = new ImportRowResult { Row = rowNum, Status = "error", Error = $"Método de pago no encontrado: \"{row.Metodo ?? ""}\"" };
                continue;
            }

            Guid? cardId = null;
            if (method.RequiresCard)
            {
                var card = FindByName(cards, c => c.Name, row.Tarjeta);
                if (card == null)
                {
                    results[i] = new ImportRowResult { Row = rowNum, Status = "error", Error = $"El método \"{method.Name}\" requiere tarjeta: \"{row.Tarjeta ?? ""}\" no encontrada" };
                    continue;
                }
                cardId = card.Id;
            }

            var description = (row.Descripcion ?? "").Trim();
            if (description.Length == 0) description = "Importado";

            toInsert.Add((i, new Transaction
            {
                PeriodId = period.Id,
                CategoryId = category.Id,
                PaymentMethodId = method.Id,
                CardId = cardId,
                Type = type,
                Amount = amount,
                Description = description,
                Date = date,
            }));
        }

        foreach (var (rowIndex, transaction) in toInsert)
        {
            var created = await _repository.AddAsync(transaction);
            results[rowIndex] = new ImportRowResult { Row = rowIndex + 1, Status = "created", Id = created.Id };
        }

        if (toInsert.Count > 0) await _rolloverService.RecalculateAllAsync();

        return new ImportResult
        {
            Created = toInsert.Count,
            Failed = rows.Count - toInsert.Count,
            Results = results.Select(r => r!).ToList(),
        };
    }
}
