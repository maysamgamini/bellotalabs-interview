using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using BellotaLabInterview.Core.Domain.Cards;
using BellotaLabInterview.Core.Domain.Game;
using BellotaLabInterview.Core.Domain.Players;
using BellotaLabInterview.Uno.Cards;

namespace BellotaLabInterview.Uno.Game;

public class UnoGameRules : GameRulesBase
{
    public override int MaxPlayers => 10;
    public override int InitialHandSize => 7;

    public override Task<bool> IsValidMove(IPlayer player, ICard card, IGameContext context)
    {
        if (card is not UnoCard unoCard || 
            context.State is not IUnoGameState unoState ||
            unoState.TopCard is not UnoCard topCard)
            return Task.FromResult(false);

        // Wild cards can always be played
        if (unoCard.Color == UnoColor.Wild)
            return Task.FromResult(true);
        
        // Match color, value, or action
        return Task.FromResult(
            unoCard.Color == topCard.Color || 
            (unoCard.Value.HasValue && unoCard.Value == topCard.Value) ||
            (unoCard.Action != UnoAction.None && unoCard.Action == topCard.Action)
        );
    }

    public override Task<bool> IsGameOver(IGameContext context)
    {
        return Task.FromResult(context.State.Players.Any(p => p.Hand.Count == 0));
    }

    public override Task<int> CalculateScore(IPlayer player, IGameContext context)
    {
        return Task.FromResult(player.Hand.Sum(card => card.GetValue(context)));
    }
} 