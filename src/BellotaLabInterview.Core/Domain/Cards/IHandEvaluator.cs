using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BellotaLabInterview.Core.Domain.Game;

namespace BellotaLabInterview.Core.Domain.Cards;

public readonly record struct HandRank
{
    public int Value { get; init; }
    public string Description { get; init; }
    public IDictionary<string, object> AdditionalData { get; init; }

    public HandRank(int value, string description, IDictionary<string, object>? additionalData = null)
    {
        Value = value;
        Description = description;
        AdditionalData = additionalData ?? new Dictionary<string, object>();
    }

    public static bool operator >(HandRank left, HandRank right) => left.Value > right.Value;
    public static bool operator <(HandRank left, HandRank right) => left.Value < right.Value;
    public static bool operator >=(HandRank left, HandRank right) => left.Value >= right.Value;
    public static bool operator <=(HandRank left, HandRank right) => left.Value <= right.Value;
}

public interface IHandEvaluator
{
    Task<HandRank> EvaluateHand(IReadOnlyList<ICard> cards, IGameContext context);
    Task<bool> IsValidHand(IReadOnlyList<ICard> cards, IGameContext context);
    Task<IReadOnlyList<ICard>> GetPlayableCards(IReadOnlyList<ICard> hand, IGameContext context);
}

public abstract class HandEvaluatorBase : IHandEvaluator
{
    public abstract Task<HandRank> EvaluateHand(IReadOnlyList<ICard> cards, IGameContext context);
    
    public virtual Task<bool> IsValidHand(IReadOnlyList<ICard> cards, IGameContext context)
    {
        if (cards == null || cards.Count == 0)
            return Task.FromResult(false);

        // Basic validation - can be overridden by specific games
        return Task.FromResult(cards.All(card => card != null));
    }

    public virtual Task<IReadOnlyList<ICard>> GetPlayableCards(IReadOnlyList<ICard> hand, IGameContext context)
    {
        // By default, all cards are playable
        // Specific games should override this to implement their rules
        return Task.FromResult<IReadOnlyList<ICard>>(hand);
    }
} 