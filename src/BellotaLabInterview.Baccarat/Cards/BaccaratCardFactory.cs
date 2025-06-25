using System;
using System.Collections.Generic;
using System.Linq;
using BellotaLabInterview.Core.Domain.Cards;

namespace BellotaLabInterview.Baccarat.Cards;

public class BaccaratCardFactory : ICardFactory
{
    public IEnumerable<ICard> CreateDeck()
    {
        return from suit in Enum.GetValues<BaccaratSuit>()
               from rank in Enum.GetValues<BaccaratRank>()
               select new BaccaratCard(suit, rank);
    }
} 