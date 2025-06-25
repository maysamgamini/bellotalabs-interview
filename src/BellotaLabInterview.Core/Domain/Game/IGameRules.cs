using System.Collections.Generic;
using System.Threading.Tasks;
using BellotaLabInterview.Core.Domain.Cards;
using BellotaLabInterview.Core.Domain.Players;

namespace BellotaLabInterview.Core.Domain.Game;

public interface IGameRules
{
    // Player constraints
    int MinPlayers { get; }
    int MaxPlayers { get; }
    int InitialHandSize { get; }
    
    // Turn validation
    Task<bool> CanPlayerAct(IPlayer player, IGameContext context);
    Task<bool> IsValidTurn(IPlayer player, IGameContext context);
    
    // Move validation
    Task<bool> IsValidMove(IPlayer player, ICard card, IGameContext context);
    Task<bool> IsValidPlay(IPlayer player, IReadOnlyList<ICard> cards, IGameContext context);
    
    // Game state validation
    Task<bool> CanTransitionState(GameState currentState, GameState nextState, IGameContext context);
    Task<bool> IsGameOver(IGameContext context);
    
    // Scoring
    Task<int> CalculateScore(IPlayer player, IGameContext context);
    Task<IReadOnlyList<IPlayer>> DetermineWinners(IGameContext context);
}

public abstract class GameRulesBase : IGameRules
{
    public virtual int MinPlayers => 2;
    public virtual int MaxPlayers => 4;
    public virtual int InitialHandSize => 5;

    public virtual Task<bool> CanPlayerAct(IPlayer player, IGameContext context)
    {
        return Task.FromResult(context.State.CurrentPlayer.Id == player.Id &&
                             player.State == PlayerState.Playing);
    }

    public virtual Task<bool> IsValidTurn(IPlayer player, IGameContext context)
    {
        return Task.FromResult(context.State.CurrentPlayer.Id == player.Id);
    }

    public abstract Task<bool> IsValidMove(IPlayer player, ICard card, IGameContext context);
    
    public virtual Task<bool> IsValidPlay(IPlayer player, IReadOnlyList<ICard> cards, IGameContext context)
    {
        return Task.FromResult(cards.Count > 0 && player.Hand.Count >= cards.Count);
    }

    public virtual Task<bool> CanTransitionState(GameState currentState, GameState nextState, IGameContext context)
    {
        var isValid = (currentState, nextState) switch
        {
            (GameState.Setup, GameState.Dealing) => true,
            (GameState.Dealing, GameState.Playing) => true,
            (GameState.Playing, GameState.Scoring) => true,
            (GameState.Scoring, GameState.GameOver) => true,
            _ => false
        };
        return Task.FromResult(isValid);
    }

    public abstract Task<bool> IsGameOver(IGameContext context);
    
    public abstract Task<int> CalculateScore(IPlayer player, IGameContext context);
    
    public virtual async Task<IReadOnlyList<IPlayer>> DetermineWinners(IGameContext context)
    {
        var winners = new List<IPlayer>();
        var highestScore = int.MinValue;

        foreach (var player in context.State.Players)
        {
            var score = await CalculateScore(player, context);
            if (score > highestScore)
            {
                winners.Clear();
                winners.Add(player);
                highestScore = score;
            }
            else if (score == highestScore)
            {
                winners.Add(player);
            }
        }

        return winners;
    }
} 