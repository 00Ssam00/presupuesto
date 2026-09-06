using Quincena.Domain.Entities;

namespace Quincena.Application.Cards;

public interface ICardRepository
{
    Task<List<Card>> GetActiveOrderedAsync();
    Task<Card> AddAsync(Card card);

    // Retorna null si no existe, igual al 404 sin side-effects de Node.
    Task<Card?> UpdateAsync(Guid id, Action<Card> apply);

    // No valida existencia previa, igual que el soft-delete de Node.
    Task DeactivateAsync(Guid id);
}
