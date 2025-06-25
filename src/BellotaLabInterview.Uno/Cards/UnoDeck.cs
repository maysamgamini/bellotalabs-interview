using System;
using System.Threading.Tasks;
using BellotaLabInterview.Core.Domain.Cards;
using BellotaLabInterview.Core.Domain.Game;

namespace BellotaLabInterview.Uno.Cards;

public class UnoDeck : DeckBase
{
    public UnoDeck(ICardFactory cardFactory) : base(cardFactory)
    {
    }

    public override async Task Reset()
    {
        // Ensure we're using a UnoCardFactory
        if (_cardFactory is not UnoCardFactory)
            throw new InvalidOperationException("UnoDeck requires a UnoCardFactory");

        await base.Reset();
    }
} 