using BellotaLabInterview.Core.Domain.Cards;
using BellotaLabInterview.Core.Domain.Game;

namespace BellotaLabInterview.Uno.Cards;

public enum UnoColor
{
    Red,
    Blue,
    Green,
    Yellow,
    Wild
}

public enum UnoValue
{
    Zero = 0,
    One = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    Six = 6,
    Seven = 7,
    Eight = 8,
    Nine = 9
}

public enum UnoAction
{
    None,
    Skip,
    Reverse,
    DrawTwo,
    Wild,
    WildDrawFour
}

public record UnoCard : CardBase
{
    public UnoColor Color { get; init; }
    public UnoValue? Value { get; init; }
    public UnoAction Action { get; init; }

    public override string DisplayName => 
        Action != UnoAction.None 
            ? $"{Color} {Action}" 
            : $"{Color} {Value}";

    public override int GetValue(IGameContext context) => 
        Action switch
        {
            UnoAction.Wild or UnoAction.WildDrawFour => 50,
            UnoAction.Skip or UnoAction.Reverse or UnoAction.DrawTwo => 20,
            _ => (int)(Value ?? 0)
        };
} 