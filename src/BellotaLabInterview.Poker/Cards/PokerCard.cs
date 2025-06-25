using System;
using BellotaLabInterview.Core.Domain.Cards;
using BellotaLabInterview.Core.Domain.Game;

namespace BellotaLabInterview.Poker.Cards;

public enum PokerSuit
{
    Hearts,
    Diamonds,
    Clubs,
    Spades
}

public enum PokerRank
{
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
    King = 13,
    Ace = 14
}

public record PokerCard : CardBase
{
    public PokerSuit Suit { get; init; }
    public PokerRank Rank { get; init; }

    public PokerCard(PokerSuit suit, PokerRank rank)
    {
        Suit = suit;
        Rank = rank;
    }

    public override string DisplayName => $"{Rank} of {Suit}";

    public override int GetValue(IGameContext context) => (int)Rank;
} 