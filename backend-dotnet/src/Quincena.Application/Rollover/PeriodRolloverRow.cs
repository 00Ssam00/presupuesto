namespace Quincena.Application.Rollover;

public record PeriodRolloverRow(
    Guid Id,
    DateOnly StartDate,
    decimal BaseIncome,
    decimal Rollover,
    decimal TotalExpenses,
    decimal TotalIncomeExtra);

public record RolloverUpdate(Guid PeriodId, decimal Rollover);
