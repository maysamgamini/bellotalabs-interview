using System;
using System.Collections.Generic;
using System.Linq;
using BellotaLabInterview.Core.Domain.Cards;

namespace BellotaLabInterview.Blackjack.Cards;

public class BlackjackCardFactory : ICardFactory
{
    public IEnumerable<ICard> CreateDeck()
    {
        var cards = from suit in Enum.GetValues<CardSuit>()
                   from rank in Enum.GetValues<CardRank>()
                   select new BlackjackCard(suit, rank);
        
        return cards.ToList(); // Create all cards at once
    }
} 