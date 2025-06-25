using System;
using BellotaLabInterview.Core.Domain.Cards;
using BellotaLabInterview.Core.Domain.Game;

namespace BellotaLabInterview.Blackjack.Cards;

public record BlackjackCard : StandardCard
{
    public BlackjackCard(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        FlipFaceUp(); // Ensure card is face up by default
    }

    public override int GetValue(IGameContext context) =>
        Rank switch
        {
            CardRank.Ace => 11, // Ace can be 1 or 11, handled by game logic
            CardRank.Jack or CardRank.Queen or CardRank.King => 10,
            _ => (int)Rank
        };

    public override string ToString() => base.ToString();
} 