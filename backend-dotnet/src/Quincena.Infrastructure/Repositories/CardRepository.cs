using Microsoft.EntityFrameworkCore;
using Quincena.Application.Cards;
using Quincena.Domain.Entities;

namespace Quincena.Infrastructure.Repositories;

public class CardRepository : ICardRepository
{
    private readonly QuincenaDbContext _db;

    public CardRepository(QuincenaDbContext db)
    {
        _db = db;
    }

    public Task<List<Card>> GetActiveOrderedAsync() =>
        _db.Cards.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();

    public async Task<Card> AddAsync(Card card)
    {
        _db.Cards.Add(card);
        await _db.SaveChangesAsync();
        return card;
    }

    public async Task<Card?> UpdateAsync(Guid id, Action<Card> apply)
    {
        var card = await _db.Cards.FirstOrDefaultAsync(c => c.Id == id);
        if (card == null) return null;
        apply(card);
        await _db.SaveChangesAsync();
        return card;
    }

    public async Task DeactivateAsync(Guid id)
    {
        var card = await _db.Cards.FirstOrDefaultAsync(c => c.Id == id);
        if (card == null) return;
        card.IsActive = false;
        await _db.SaveChangesAsync();
    }
}
