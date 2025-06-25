using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BellotaLabInterview.Core.Domain.Cards;

namespace BellotaLabInterview.Core.Domain.Game;

public interface IDeck
{
    IReadOnlyList<ICard> Cards { get; }
    Task<ICard> DrawCard();
    Task<IReadOnlyList<ICard>> DrawCards(int count);
    Task Shuffle();
    int RemainingCards { get; }
    Task Reset();
}

public abstract class DeckBase : IDeck
{
    protected readonly List<ICard> _cards;
    protected readonly ICardFactory _cardFactory;
    protected readonly Random _random;

    protected DeckBase(ICardFactory cardFactory)
    {
        _cardFactory = cardFactory;
        _cards = new List<ICard>();
        _random = new Random();
    }

    public virtual IReadOnlyList<ICard> Cards => _cards.AsReadOnly();
    public int RemainingCards => _cards.Count;

    public virtual Task<ICard> DrawCard()
    {
        if (_cards.Count == 0)
            throw new InvalidOperationException("No cards remaining in the deck.");

        var card = _cards[0];
        _cards.RemoveAt(0);
        return Task.FromResult(card);
    }

    public virtual Task<IReadOnlyList<ICard>> DrawCards(int count)
    {
        if (count > _cards.Count)
            throw new InvalidOperationException($"Not enough cards remaining. Requested: {count}, Available: {_cards.Count}");

        var cards = _cards.Take(count).ToList();
        _cards.RemoveRange(0, count);
        return Task.FromResult<IReadOnlyList<ICard>>(cards);
    }

    public virtual Task Shuffle()
    {
        int n = _cards.Count;
        while (n > 1)
        {
            n--;
            int k = _random.Next(n + 1);
            (_cards[k], _cards[n]) = (_cards[n], _cards[k]);
        }
        return Task.CompletedTask;
    }

    public virtual async Task Reset()
    {
        _cards.Clear();
        _cards.AddRange(_cardFactory.CreateDeck());
        await Shuffle();
    }
} 