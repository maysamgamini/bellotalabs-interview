using System;
using System.Collections.Generic;
using System.Linq;
using BellotaLabInterview.Core.Domain.Cards;

namespace BellotaLabInterview.Poker.Cards;

public class PokerCardFactory : ICardFactory
{
    public IEnumerable<ICard> CreateDeck()
    {
        return from suit in Enum.GetValues<PokerSuit>()
               from rank in Enum.GetValues<PokerRank>()
               select new PokerCard(suit, rank);
    }
} 