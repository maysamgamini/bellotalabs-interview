using System;
using BellotaLabInterview.Core.Domain.Cards;
using BellotaLabInterview.Core.Domain.Game;

namespace BellotaLabInterview.Baccarat.Cards;

public class BaccaratDeck : DeckBase
{
    public BaccaratDeck(ICardFactory cardFactory) : base(cardFactory)
    {
        if (cardFactory is not BaccaratCardFactory)
            throw new ArgumentException("BaccaratDeck requires a BaccaratCardFactory", nameof(cardFactory));
    }
} 