using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BellotaLabInterview.Core.Domain.Cards;
using BellotaLabInterview.Core.Domain.Game;
using BellotaLabInterview.Core.Domain.Players;
using BellotaLabInterview.Uno.Cards;

namespace BellotaLabInterview.Uno.Game;

public class UnoGame : GameBase
{
    private readonly GameOptions _options;
    
    public UnoGame(
        IGameContext context,
        ICardFactory cardFactory,
        IHandEvaluator handEvaluator,
        IDeck deck,
        GameOptions options)
        : base(GameType.Uno, context, cardFactory, handEvaluator, deck)
    {
        _options = options;
    }

    protected override Task<bool> ValidatePlayers(IReadOnlyList<IPlayer> players)
    {
        var isValid = players.Count >= _options.MinPlayers && 
                     players.Count <= _options.MaxPlayers;
        return Task.FromResult(isValid);
    }

    protected override async Task InitializeGameState(IReadOnlyList<IPlayer> players)
    {
        // Initialize players with starting points
        foreach (var player in players)
        {
            await player.UpdatePoints(_options.InitialPoints);
        }

        // Additional Uno-specific initialization can be done here
    }

    protected override async Task DealInitialCards()
    {
        // In Uno, each player starts with 7 cards
        foreach (var player in Context.State.Players)
        {
            var cards = await Deck.DrawCards(7);
            foreach (var card in cards)
            {
                await player.AddCard(card);
            }
        }

        // Draw first card for the discard pile
        var firstCard = await Deck.DrawCard();
        // Update game state with first card (implementation depends on your state management)
    }

    protected override Task StartFirstTurn()
    {
        // First player starts
        // Additional Uno-specific turn initialization can be done here
        return Task.CompletedTask;
    }

    protected override Task CleanupGame()
    {
        // Cleanup any Uno-specific resources or state
        return Task.CompletedTask;
    }

    public override Task<bool> IsGameOver()
    {
        // Game is over if any player has no cards left
        var isOver = Context.State.Players.Any(player => player.Hand.Count == 0);
        return Task.FromResult(isOver);
    }

    public override async Task<GameResult> GetGameResult()
    {
        var winners = new List<IPlayer>();
        var losers = new List<IPlayer>();
        var results = new Dictionary<string, object>();

        foreach (var player in Context.State.Players)
        {
            if (player.Hand.Count == 0)
            {
                winners.Add(player);
                // Calculate points from other players' hands
                int points = 0;
                foreach (var otherPlayer in Context.State.Players)
                {
                    if (otherPlayer != player)
                    {
                        var handRank = await HandEvaluator.EvaluateHand(otherPlayer.Hand, Context);
                        points += handRank.Value;
                    }
                }
                results[$"Winner_{player.Id}_Points"] = points;
            }
            else
            {
                losers.Add(player);
            }
        }

        return new GameResult(winners, losers, results);
    }
} 