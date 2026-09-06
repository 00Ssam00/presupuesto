using Moq;
using Quincena.Application.Cards;
using Quincena.Domain.Entities;
using Xunit;

namespace Quincena.UnitTests.Cards;

// Port de backend/src/__tests__/cards.test.ts.
public class CardServiceTests
{
    [Fact]
    public async Task Create_convierte_last_four_y_bank_vacios_a_null()
    {
        var repo = new Mock<ICardRepository>();
        Card? added = null;
        repo.Setup(r => r.AddAsync(It.IsAny<Card>()))
            .Callback<Card>(c => added = c)
            .ReturnsAsync((Card c) => c);
        var service = new CardService(repo.Object);

        await service.CreateAsync(new CreateCardRequest { Name = "Visa", LastFour = "", Bank = "" });

        Assert.Null(added!.LastFour);
        Assert.Null(added.Bank);
    }

    [Fact]
    public async Task Update_no_borra_is_active_cuando_no_viene_en_el_body()
    {
        // Regresión: is_active pasaba a NULL y la tarjeta desaparecía de los
        // listados que filtran is_active = TRUE (ver cards.test.ts en Node).
        var id = Guid.NewGuid();
        var existing = new Card { Id = id, Name = "Visa", IsActive = true };
        var repo = new Mock<ICardRepository>();
        repo.Setup(r => r.UpdateAsync(id, It.IsAny<Action<Card>>()))
            .Returns<Guid, Action<Card>>((_, apply) => { apply(existing); return Task.FromResult<Card?>(existing); });
        var service = new CardService(repo.Object);

        var result = await service.UpdateAsync(id, new UpdateCardRequest
        {
            Name = "Visa", LastFour = "1234", Bank = "Bancolombia",
        });

        Assert.True(result!.IsActive);
        Assert.Equal("1234", result.LastFour);
        Assert.Equal("Bancolombia", result.Bank);
    }

    [Fact]
    public async Task Update_sin_last_four_ni_bank_los_deja_en_null_igual_que_Node()
    {
        // A diferencia de icon en categories, last_four/bank en Node NO usan
        // COALESCE — se sobrescriben siempre, incluso a null si no vienen.
        var id = Guid.NewGuid();
        var existing = new Card { Id = id, Name = "Visa", LastFour = "9999", Bank = "Nu", IsActive = true };
        var repo = new Mock<ICardRepository>();
        repo.Setup(r => r.UpdateAsync(id, It.IsAny<Action<Card>>()))
            .Returns<Guid, Action<Card>>((_, apply) => { apply(existing); return Task.FromResult<Card?>(existing); });
        var service = new CardService(repo.Object);

        var result = await service.UpdateAsync(id, new UpdateCardRequest { Name = "Visa" });

        Assert.Null(result!.LastFour);
        Assert.Null(result.Bank);
    }

    [Fact]
    public async Task Update_permite_desactivar_explicitamente()
    {
        var id = Guid.NewGuid();
        var existing = new Card { Id = id, Name = "Visa", IsActive = true };
        var repo = new Mock<ICardRepository>();
        repo.Setup(r => r.UpdateAsync(id, It.IsAny<Action<Card>>()))
            .Returns<Guid, Action<Card>>((_, apply) => { apply(existing); return Task.FromResult<Card?>(existing); });
        var service = new CardService(repo.Object);

        var result = await service.UpdateAsync(id, new UpdateCardRequest { Name = "Visa", IsActive = false });

        Assert.False(result!.IsActive);
    }

    [Fact]
    public async Task Update_retorna_null_si_no_existe()
    {
        var repo = new Mock<ICardRepository>();
        repo.Setup(r => r.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Action<Card>>()))
            .ReturnsAsync((Card?)null);
        var service = new CardService(repo.Object);

        var result = await service.UpdateAsync(Guid.NewGuid(), new UpdateCardRequest { Name = "X" });

        Assert.Null(result);
    }

    [Fact]
    public async Task Delete_hace_soft_delete()
    {
        var id = Guid.NewGuid();
        var repo = new Mock<ICardRepository>();
        var service = new CardService(repo.Object);

        await service.DeleteAsync(id);

        repo.Verify(r => r.DeactivateAsync(id), Times.Once);
    }
}
