using System;
using BellotaLabInterview.Core.Domain.Cards;
using BellotaLabInterview.Core.Domain.Game;

namespace BellotaLabInterview.Baccarat.Cards;

public enum BaccaratSuit
{
    Hearts,
    Diamonds,
    Clubs,
    Spades
}

public enum BaccaratRank
{
    Ace = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    Six = 6,
    Seven = 7,
    Eight = 8,
    Nine = 9,
    Ten = 10,
    Jack = 11,
    Queen = 12,
    King = 13
}

public record BaccaratCard : CardBase
{
    public BaccaratSuit Suit { get; init; }
    public BaccaratRank Rank { get; init; }

    public BaccaratCard(BaccaratSuit suit, BaccaratRank rank)
    {
        Suit = suit;
        Rank = rank;
    }

    public override string DisplayName => $"{Rank} of {Suit}";

    public override int GetValue(IGameContext context) =>
        Rank switch
        {
            BaccaratRank.Ten or BaccaratRank.Jack or BaccaratRank.Queen or BaccaratRank.King => 0,
            _ => (int)Rank
        };
} 