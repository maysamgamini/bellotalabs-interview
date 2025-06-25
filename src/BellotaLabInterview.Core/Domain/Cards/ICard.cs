using BellotaLabInterview.Core.Domain.Game;

namespace BellotaLabInterview.Core.Domain.Cards;

public interface ICard
{
    string DisplayName { get; }
    int GetValue(IGameContext context);
}

public abstract record CardBase : ICard
{
    public abstract string DisplayName { get; }
    public abstract int GetValue(IGameContext context);
} 