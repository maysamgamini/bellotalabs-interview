using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using BellotaLabInterview.Infrastructure.DependencyInjection;
using BellotaLabInterview.Core.Domain.Game;
using BellotaLabInterview.Core.Domain.Cards;
using BellotaLabInterview.Core.Domain.Players;
using BellotaLabInterview.Core.Domain.Snapshots;
using System.Linq;

namespace BellotaLabInterview.UI.Console.DependencyInjection
{
    public static class ServiceConfiguration
    {
        public static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            
            // Add infrastructure services
            services.AddBellotaLabServices();
            
            // Add UI.Console specific services
            services.AddScoped<IGameContext, GameContext>();

            return services.BuildServiceProvider();
        }
    }

    public class DemoGameState : IGameState
    {
        private readonly List<IPlayer> _players = new();
        private IPlayer? _currentPlayer;

        public Guid GameId { get; set; } = Guid.NewGuid();
        public GameType GameType { get; set; } = GameType.Blackjack;
        public GameState CurrentState { get; set; } = GameState.Setup;
        public IPlayer CurrentPlayer 
        { 
            get => _currentPlayer ?? _players.FirstOrDefault() ?? throw new InvalidOperationException("No players in game");
            set => _currentPlayer = value;
        }
        public IReadOnlyList<IPlayer> Players => _players;
        public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;
        public bool IsPlayDirectionClockwise { get; set; } = true;

        public void SetPlayers(IEnumerable<IPlayer> players)
        {
            _players.Clear();
            _players.AddRange(players);
            _currentPlayer = _players.FirstOrDefault();
            foreach (var player in _players)
            {
                if (player is DemoPlayer demoPlayer)
                {
                    demoPlayer.UpdatePoints(100);
                }
            }
        }
    }

    public class GameContext : IGameContext
    {
        private readonly DemoGameState _state = new();
        public IGameState State => _state;
        public ICardPlayValidator CardPlayValidator { get; } = new DefaultCardPlayValidator();
        public ICardEffectHandler EffectHandler { get; } = new DefaultCardEffectHandler();

        public Task SetState(GameState state)
        {
            _state.CurrentState = state;
            _state.LastUpdateTime = DateTime.UtcNow;
            return Task.CompletedTask;
        }

        public Task SetPlayers(IEnumerable<IPlayer> players)
        {
            _state.SetPlayers(players);
            return Task.CompletedTask;
        }

        public Task SetCurrentPlayer(IPlayer player)
        {
            _state.CurrentPlayer = player;
            _state.LastUpdateTime = DateTime.UtcNow;
            return Task.CompletedTask;
        }

        public Task<bool> IsValidMove(ICard card)
        {
            return Task.FromResult(true); // In Blackjack, all moves are valid
        }

        public Task<bool> CanPlayerAct(IPlayer player)
        {
            return Task.FromResult(State.CurrentPlayer == player);
        }

        public Task AdvanceNextPlayer()
        {
            var players = (_state.Players as List<IPlayer>) ?? new List<IPlayer>();
            if (players.Count == 0) return Task.CompletedTask;

            var currentIndex = players.IndexOf(_state.CurrentPlayer);
            var nextIndex = (currentIndex + 1) % players.Count;
            _state.CurrentPlayer = players[nextIndex];
            _state.LastUpdateTime = DateTime.UtcNow;
            return Task.CompletedTask;
        }

        public Task DealCards(int count)
        {
            return Task.CompletedTask; // Handled by the game implementation
        }

        public Task PlayCard(ICard card)
        {
            return Task.CompletedTask; // Not used in Blackjack
        }

        public Task<GameSnapshot> CreateSnapshot()
        {
            return Task.FromResult(new GameSnapshot
            {
                GameType = _state.GameType,
                State = _state.CurrentState,
                CurrentPlayerIndex = (_state.Players as List<IPlayer>)?.IndexOf(_state.CurrentPlayer) ?? -1,
                IsPlayDirectionClockwise = _state.IsPlayDirectionClockwise,
                Players = _state.Players.Select(p => new PlayerSnapshot
                {
                    Name = p.Name,
                    State = p.State,
                    Points = p.Points,
                    Hand = p.Hand.ToList(),
                    PlayedCards = new List<ICard>()
                }).ToList(),
                Deck = new DeckSnapshot
                {
                    Cards = new List<ICard>(),
                    CurrentPosition = 0
                },
                DiscardPile = new List<ICard>(),
                GameSpecificData = new Dictionary<string, object>()
            });
        }

        public Task RestoreFromSnapshot(GameSnapshot snapshot)
        {
            if (snapshot == null) return Task.CompletedTask;

            _state.GameType = snapshot.GameType;
            _state.CurrentState = snapshot.State;
            _state.IsPlayDirectionClockwise = snapshot.IsPlayDirectionClockwise;
            _state.LastUpdateTime = DateTime.UtcNow;

            var players = (_state.Players as List<IPlayer>);
            if (players != null && snapshot.CurrentPlayerIndex >= 0 && snapshot.CurrentPlayerIndex < players.Count)
            {
                _state.CurrentPlayer = players[snapshot.CurrentPlayerIndex];
            }

            // Restore player states
            foreach (var playerSnapshot in snapshot.Players)
            {
                var player = _state.Players.FirstOrDefault(p => p.Name == playerSnapshot.Name);
                if (player != null)
                {
                    player.UpdatePoints(Convert.ToInt32(playerSnapshot.Points));
                }
            }

            return Task.CompletedTask;
        }
    }

    public class DefaultCardPlayValidator : ICardPlayValidator
    {
        public Task<bool> ValidatePlay(ICard card, IGameContext context)
        {
            return Task.FromResult(true);
        }

        public Task<bool> CanPlay(ICard topCard, ICard playedCard, IGameContext context)
        {
            return Task.FromResult(true);
        }
    }

    public class DefaultCardEffectHandler : ICardEffectHandler
    {
        public Task HandleEffect(ICard card, IGameContext context)
        {
            return Task.CompletedTask;
        }

        public Task HandleCardPlayed(ICard card, IGameContext context)
        {
            return Task.CompletedTask;
        }
    }
} 