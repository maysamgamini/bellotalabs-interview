using System;
using BellotaLabInterview.Core.Domain.Cards;
using BellotaLabInterview.Core.Domain.Game;

namespace BellotaLabInterview.Poker.Cards;

public class PokerDeck : DeckBase
{
    public PokerDeck(ICardFactory cardFactory) : base(cardFactory)
    {
        if (cardFactory is not PokerCardFactory)
            throw new ArgumentException("PokerDeck requires a PokerCardFactory", nameof(cardFactory));
    }
} 