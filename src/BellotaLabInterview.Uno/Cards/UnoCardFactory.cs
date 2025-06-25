using System;
using System.Collections.Generic;
using BellotaLabInterview.Core.Domain.Cards;

namespace BellotaLabInterview.Uno.Cards;

public class UnoCardFactory : ICardFactory
{
    public IEnumerable<ICard> CreateDeck()
    {
        // Regular number cards (0-9)
        foreach (UnoColor color in Enum.GetValues<UnoColor>())
        {
            if (color == UnoColor.Wild) continue;
            
            // One 0 card per color
            yield return new UnoCard 
            { 
                Color = color, 
                Value = UnoValue.Zero,
                Action = UnoAction.None
            };
            
            // Two of each 1-9 per color
            for (int i = 1; i <= 9; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    yield return new UnoCard
                    {
                        Color = color,
                        Value = (UnoValue)i,
                        Action = UnoAction.None
                    };
                }
            }
            
            // Action cards (two of each per color)
            for (int i = 0; i < 2; i++)
            {
                yield return new UnoCard
                {
                    Color = color,
                    Action = UnoAction.Skip
                };
                yield return new UnoCard
                {
                    Color = color,
                    Action = UnoAction.Reverse
                };
                yield return new UnoCard
                {
                    Color = color,
                    Action = UnoAction.DrawTwo
                };
            }
        }
        
        // Wild cards (4 of each)
        for (int i = 0; i < 4; i++)
        {
            yield return new UnoCard
            {
                Color = UnoColor.Wild,
                Action = UnoAction.Wild
            };
            yield return new UnoCard
            {
                Color = UnoColor.Wild,
                Action = UnoAction.WildDrawFour
            };
        }
    }
} 