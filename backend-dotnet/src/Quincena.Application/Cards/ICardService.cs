using Quincena.Domain.Entities;

namespace Quincena.Application.Cards;

public interface ICardService
{
    Task<List<Card>> ListAsync();
    Task<Card> CreateAsync(CreateCardRequest request);
    Task<Card?> UpdateAsync(Guid id, UpdateCardRequest request);
    Task DeleteAsync(Guid id);
}
