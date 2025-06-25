using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BellotaLabInterview.Core.Domain.Cards;
using BellotaLabInterview.Core.Domain.Players;
using BellotaLabInterview.Core.Domain.Snapshots;

namespace BellotaLabInterview.Core.Domain.Game;

public interface IGameContext
{
    IGameState State { get; }
    ICardPlayValidator CardPlayValidator { get; }
    ICardEffectHandler EffectHandler { get; }
    
    Task<bool> IsValidMove(ICard card);
    Task<bool> CanPlayerAct(IPlayer player);
    Task AdvanceNextPlayer();
    Task DealCards(int count);
    Task PlayCard(ICard card);
    
    // State management
    Task SetState(GameState state);
    Task SetPlayers(IEnumerable<IPlayer> players);
    Task SetCurrentPlayer(IPlayer player);
    
    // Snapshot support
    Task<GameSnapshot> CreateSnapshot();
    Task RestoreFromSnapshot(GameSnapshot snapshot);
}

public interface IGameState
{
    Guid GameId { get; }
    GameType GameType { get; }
    GameState CurrentState { get; }
    IPlayer CurrentPlayer { get; }
    IReadOnlyList<IPlayer> Players { get; }
    DateTime LastUpdateTime { get; }
    bool IsPlayDirectionClockwise { get; }
}

public enum GameType
{
    HighStakes,
    Blackjack,
    Poker,
    Uno,
    Baccarat
}

public enum GameState
{
    Setup,
    Dealing,
    Betting,
    Playing,
    Scoring,
    GameOver
} 