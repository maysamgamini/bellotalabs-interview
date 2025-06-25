using System.Collections.Generic;
using System.Threading.Tasks;
using BellotaLabInterview.Core.Domain.Game;

namespace BellotaLabInterview.Core.Domain.Cards;

public interface ICardFactory
{
    IEnumerable<ICard> CreateDeck();
}

public interface ICardPlayValidator
{
    Task<bool> CanPlay(ICard card, ICard targetCard, IGameContext context);
}

public interface ICardEffectHandler
{
    Task HandleCardPlayed(ICard card, IGameContext context);
} 