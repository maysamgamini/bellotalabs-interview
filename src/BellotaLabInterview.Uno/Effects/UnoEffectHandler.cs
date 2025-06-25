using System.Threading.Tasks;
using BellotaLabInterview.Core.Domain.Cards;
using BellotaLabInterview.Core.Domain.Game;
using BellotaLabInterview.Uno.Cards;

namespace BellotaLabInterview.Uno.Effects;

public class UnoEffectHandler : ICardEffectHandler
{
    public async Task HandleCardPlayed(ICard card, IGameContext context)
    {
        if (card is not UnoCard unoCard) return;
        
        switch (unoCard.Action)
        {
            case UnoAction.Skip:
                await HandleSkip(context);
                break;
            case UnoAction.Reverse:
                await HandleReverse(context);
                break;
            case UnoAction.DrawTwo:
                await HandleDrawTwo(context);
                break;
            case UnoAction.WildDrawFour:
                await HandleWildDrawFour(context);
                break;
        }
    }
    
    private async Task HandleSkip(IGameContext context)
    {
        await context.AdvanceNextPlayer(); // Skip next player
    }
    
    private async Task HandleReverse(IGameContext context)
    {
        // Game context should handle direction change internally
        await Task.CompletedTask;
    }
    
    private async Task HandleDrawTwo(IGameContext context)
    {
        await context.DealCards(2);
        await context.AdvanceNextPlayer(); // Skip their turn
    }
    
    private async Task HandleWildDrawFour(IGameContext context)
    {
        await context.DealCards(4);
        await context.AdvanceNextPlayer();
    }
} 