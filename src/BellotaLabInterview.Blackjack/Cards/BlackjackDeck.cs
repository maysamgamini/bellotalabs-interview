using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BellotaLabInterview.Core.Domain.Cards;
using BellotaLabInterview.Core.Domain.Game;

namespace BellotaLabInterview.Blackjack.Cards;

public class BlackjackDeck : DeckBase
{
    public BlackjackDeck(ICardFactory cardFactory) : base(cardFactory)
    {
        if (cardFactory is not BlackjackCardFactory)
            throw new ArgumentException("BlackjackDeck requires a BlackjackCardFactory", nameof(cardFactory));
    }

    public override IReadOnlyList<ICard> Cards => _cards;

    public override Task<ICard> DrawCard()
    {
        if (_cards.Count == 0)
        {
            throw new InvalidOperationException("No cards remaining in the deck.");
        }

        var card = _cards[0];
        _cards.RemoveAt(0);
        return Task.FromResult(card);
    }

    public override Task<IReadOnlyList<ICard>> DrawCards(int count)
    {
        var cards = new List<ICard>();
        for (int i = 0; i < count && _cards.Count > 0; i++)
        {
            var card = _cards[0];
            _cards.RemoveAt(0);
            cards.Add(card);
        }
        return Task.FromResult<IReadOnlyList<ICard>>(cards);
    }

    public override async Task Reset()
    {
        _cards.Clear();
        _cards.AddRange(_cardFactory.CreateDeck());
        await Shuffle();
    }
} 