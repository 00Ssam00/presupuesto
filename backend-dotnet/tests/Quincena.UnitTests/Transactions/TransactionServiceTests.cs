using Moq;
using Quincena.Application.Cards;
using Quincena.Application.Categories;
using Quincena.Application.PaymentMethods;
using Quincena.Application.Periods;
using Quincena.Application.Rollover;
using Quincena.Application.Transactions;
using Quincena.Domain.Entities;
using Xunit;

namespace Quincena.UnitTests.Transactions;

// Port de la parte CRUD de backend/src/__tests__/transactions.test.ts (la parte de
// importTransactions se prueba en ImportServiceTests). "propaga errores de BD como
// 500" se cubre a nivel de integración vía ExceptionHandlingMiddleware.
public class TransactionServiceTests
{
    private static TransactionService BuildService(
        Mock<ITransactionRepository> repo,
        Mock<IRolloverService>? rollover = null) =>
        new(
            repo.Object,
            (rollover ?? new Mock<IRolloverService>()).Object,
            new Mock<IPeriodRepository>().Object,
            new Mock<ICategoryRepository>().Object,
            new Mock<IPaymentMethodRepository>().Object,
            new Mock<ICardRepository>().Object);

    [Fact]
    public async Task Create_usa_null_para_card_id_ausente_y_recalcula()
    {
        var repo = new Mock<ITransactionRepository>();
        Transaction? added = null;
        repo.Setup(r => r.AddAsync(It.IsAny<Transaction>()))
            .Callback<Transaction>(t => added = t)
            .ReturnsAsync((Transaction t) => t);
        var rollover = new Mock<IRolloverService>();
        var service = BuildService(repo, rollover);

        await service.CreateAsync(new CreateTransactionRequest
        {
            PeriodId = Guid.NewGuid(), CategoryId = Guid.NewGuid(), PaymentMethodId = Guid.NewGuid(),
            Type = "expense", Amount = 5000m, Description = "Mercado", Date = new DateOnly(2026, 6, 20),
        });

        Assert.Null(added!.CardId);
        rollover.Verify(r => r.RecalculateAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Update_retorna_null_sin_recalcular_si_no_existe()
    {
        var repo = new Mock<ITransactionRepository>();
        repo.Setup(r => r.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Action<Transaction>>())).ReturnsAsync((Transaction?)null);
        var rollover = new Mock<IRolloverService>();
        var service = BuildService(repo, rollover);

        var result = await service.UpdateAsync(Guid.NewGuid(), new UpdateTransactionRequest
        {
            CategoryId = Guid.NewGuid(), PaymentMethodId = Guid.NewGuid(),
            Type = "expense", Amount = 3000m, Description = "x", Date = new DateOnly(2026, 6, 21),
        });

        Assert.Null(result);
        rollover.Verify(r => r.RecalculateAllAsync(), Times.Never);
    }

    [Fact]
    public async Task Update_recalcula_si_existe_y_no_permite_mover_de_quincena()
    {
        var id = Guid.NewGuid();
        var existing = new Transaction { Id = id, PeriodId = Guid.NewGuid() };
        var originalPeriodId = existing.PeriodId;
        var repo = new Mock<ITransactionRepository>();
        repo.Setup(r => r.UpdateAsync(id, It.IsAny<Action<Transaction>>()))
            .Returns<Guid, Action<Transaction>>((_, apply) => { apply(existing); return Task.FromResult<Transaction?>(existing); });
        var rollover = new Mock<IRolloverService>();
        var service = BuildService(repo, rollover);

        var result = await service.UpdateAsync(id, new UpdateTransactionRequest
        {
            CategoryId = Guid.NewGuid(), PaymentMethodId = Guid.NewGuid(),
            Type = "expense", Amount = 3000m, Description = "Ajuste", Date = new DateOnly(2026, 6, 21),
        });

        Assert.Same(existing, result);
        Assert.Equal(originalPeriodId, result!.PeriodId); // UpdateTransactionRequest no trae period_id
        rollover.Verify(r => r.RecalculateAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Delete_siempre_recalcula_sin_validar_existencia_previa()
    {
        var repo = new Mock<ITransactionRepository>();
        var rollover = new Mock<IRolloverService>();
        var service = BuildService(repo, rollover);

        await service.DeleteAsync(Guid.NewGuid());

        rollover.Verify(r => r.RecalculateAllAsync(), Times.Once);
    }
}
