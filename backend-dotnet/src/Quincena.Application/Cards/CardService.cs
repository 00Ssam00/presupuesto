using Quincena.Domain.Entities;

namespace Quincena.Application.Cards;

// Port de backend/src/controllers/cardsController.ts.
public class CardService : ICardService
{
    private readonly ICardRepository _repository;

    public CardService(ICardRepository repository)
    {
        _repository = repository;
    }

    public Task<List<Card>> ListAsync() => _repository.GetActiveOrderedAsync();

    public Task<Card> CreateAsync(CreateCardRequest request)
    {
        var card = new Card
        {
            Name = request.Name,
            LastFour = string.IsNullOrEmpty(request.LastFour) ? null : request.LastFour,
            Bank = string.IsNullOrEmpty(request.Bank) ? null : request.Bank,
        };
        return _repository.AddAsync(card);
    }

    public Task<Card?> UpdateAsync(Guid id, UpdateCardRequest request) =>
        _repository.UpdateAsync(id, card =>
        {
            card.Name = request.Name;
            card.LastFour = string.IsNullOrEmpty(request.LastFour) ? null : request.LastFour;
            card.Bank = string.IsNullOrEmpty(request.Bank) ? null : request.Bank;
            // COALESCE: is_active solo se pisa si vino en el body.
            if (request.IsActive.HasValue) card.IsActive = request.IsActive.Value;
        });

    public Task DeleteAsync(Guid id) => _repository.DeactivateAsync(id);
}
