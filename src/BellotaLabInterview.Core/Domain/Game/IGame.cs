using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BellotaLabInterview.Core.Domain.Cards;
using BellotaLabInterview.Core.Domain.Players;

namespace BellotaLabInterview.Core.Domain.Game;

public interface IGame
{
    GameType Type { get; }
    IGameContext Context { get; }
    Task Initialize(IEnumerable<IPlayer> players);
    Task StartGame();
    Task EndGame();
    Task<bool> IsGameOver();
    Task<GameResult> GetGameResult();
}

public record GameResult(
    IReadOnlyList<IPlayer> Winners,
    IReadOnlyList<IPlayer> Losers,
    IDictionary<string, object> GameSpecificResults
);

public abstract class GameBase : IGame
{
    protected readonly IGameContext Context;
    protected readonly ICardFactory CardFactory;
    protected readonly IHandEvaluator HandEvaluator;
    protected readonly IDeck Deck;
    
    public GameType Type { get; }
    IGameContext IGame.Context => Context;

    protected GameBase(
        GameType type,
        IGameContext context,
        ICardFactory cardFactory,
        IHandEvaluator handEvaluator,
        IDeck deck)
    {
        Type = type;
        Context = context;
        CardFactory = cardFactory;
        HandEvaluator = handEvaluator;
        Deck = deck;
    }

    public virtual async Task Initialize(IEnumerable<IPlayer> players)
    {
        // Validate players
        var playerList = players.ToList();
        if (!await ValidatePlayers(playerList))
            throw new InvalidOperationException("Invalid player configuration");

        // Reset deck
        await Deck.Reset();

        // Initialize game-specific state
        await InitializeGameState(playerList);
    }

    public virtual async Task StartGame()
    {
        if (Context.State.CurrentState != GameState.Setup)
            throw new InvalidOperationException("Game must be in Setup state to start");

        // Deal initial cards
        await DealInitialCards();

        // Start first turn
        await StartFirstTurn();
    }

    public virtual async Task EndGame()
    {
        if (Context.State.CurrentState == GameState.GameOver)
            throw new InvalidOperationException("Game is already over");

        // Perform game-specific cleanup
        await CleanupGame();
    }

    public abstract Task<bool> IsGameOver();
    public abstract Task<GameResult> GetGameResult();

    protected abstract Task<bool> ValidatePlayers(IReadOnlyList<IPlayer> players);
    protected abstract Task InitializeGameState(IReadOnlyList<IPlayer> players);
    protected abstract Task DealInitialCards();
    protected abstract Task StartFirstTurn();
    protected abstract Task CleanupGame();
} 