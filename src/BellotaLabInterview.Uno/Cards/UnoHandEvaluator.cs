using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BellotaLabInterview.Core.Domain.Cards;
using BellotaLabInterview.Core.Domain.Game;

namespace BellotaLabInterview.Uno.Cards;

public class UnoHandEvaluator : HandEvaluatorBase
{
    private readonly IGameRules _gameRules;

    public UnoHandEvaluator(IGameRules gameRules)
    {
        _gameRules = gameRules;
    }

    public override Task<HandRank> EvaluateHand(IReadOnlyList<ICard> cards, IGameContext context)
    {
        // In Uno, hand value is the sum of card values (used for scoring when someone wins)
        int totalValue = cards.Sum(card => card.GetValue(context));
        return Task.FromResult(new HandRank(totalValue, $"Total Value: {totalValue}"));
    }

    public override async Task<bool> IsValidHand(IReadOnlyList<ICard> cards, IGameContext context)
    {
        if (!await base.IsValidHand(cards, context))
            return false;

        // In Uno, any hand is valid as long as it contains valid Uno cards
        return cards.All(card => card is UnoCard);
    }

    public override async Task<IReadOnlyList<ICard>> GetPlayableCards(IReadOnlyList<ICard> hand, IGameContext context)
    {
        var topCard = await GetTopCard(context);
        if (topCard == null)
            return hand; // First card of the game, any card is playable

        return await Task.WhenAll(
            hand.Select(async card => new { Card = card, IsValid = await _gameRules.IsValidMove(context.State.CurrentPlayer, card, context) })
        ).ContinueWith(t => t.Result.Where(r => r.IsValid).Select(r => r.Card).ToList());
    }

    private Task<ICard?> GetTopCard(IGameContext context)
    {
        // This would need to be implemented based on your actual game state management
        // For now, we'll assume it's accessible through game-specific data
        if (context.State is IUnoGameState unoState)
        {
            return Task.FromResult<ICard?>(unoState.TopCard);
        }
        return Task.FromResult<ICard?>(null);
    }
}

public interface IUnoGameState : IGameState
{
    UnoCard? TopCard { get; }
    UnoColor CurrentColor { get; }
} 