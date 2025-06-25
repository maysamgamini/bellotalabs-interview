using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BellotaLabInterview.Core.Domain.Cards;
using BellotaLabInterview.Core.Domain.Game;
using BellotaLabInterview.Core.Domain.Players;

namespace BellotaLabInterview.Blackjack.Game
{
    public class BlackjackGame : GameBase
    {
        private readonly IGameRules _gameRules;

        public BlackjackGame(
            IGameContext context,
            ICardFactory cardFactory,
            IHandEvaluator handEvaluator,
            IDeck deck,
            IGameRules gameRules)
            : base(GameType.Blackjack, context, cardFactory, handEvaluator, deck)
        {
            _gameRules = gameRules;
        }

        protected override Task<bool> ValidatePlayers(IReadOnlyList<IPlayer> players)
        {
            return Task.FromResult(
                players.Count >= _gameRules.MinPlayers && 
                players.Count <= _gameRules.MaxPlayers);
        }

        protected override async Task InitializeGameState(IReadOnlyList<IPlayer> players)
        {
            // Set up the game state with players
            var state = Context.State;
            
            // Set up the players
            await Context.SetPlayers(players);
            
            // Set initial state
            await Context.SetState(GameState.Setup);
            
            // Set first player
            if (players.Any())
            {
                await Context.SetCurrentPlayer(players.First());
            }
        }

        protected override async Task DealInitialCards()
        {
            // Deal two cards to each player
            foreach (var player in Context.State.Players)
            {
                var cards = (await Deck.DrawCards(_gameRules.InitialHandSize)).ToList();
                for (int i = 0; i < cards.Count; i++)
                {
                    var card = cards[i];
                    if (card is StandardCard standardCard)
                    {
                        // For dealer, only first card is face up
                        if (player == Context.State.Players.Last() && i > 0)
                        {
                            standardCard.FlipFaceDown();
                        }
                        else
                        {
                            standardCard.FlipFaceUp();
                        }
                    }
                    await player.AddCard(card);
                }
            }
        }

        protected override Task StartFirstTurn()
        {
            // In Blackjack, dealer acts last
            return Task.CompletedTask;
        }

        protected override async Task CleanupGame()
        {
            // Collect all cards back to deck
            foreach (var player in Context.State.Players)
            {
                foreach (var card in player.Hand)
                {
                    await player.RemoveCard(card);
                }
            }
        }

        public override Task<bool> IsGameOver()
        {
            return _gameRules.IsGameOver(Context);
        }

        public override async Task<GameResult> GetGameResult()
        {
            var winners = await _gameRules.DetermineWinners(Context);
            var losers = Context.State.Players.Except(winners).ToList();
            var results = new Dictionary<string, object>();

            // Add scores to results
            foreach (var player in Context.State.Players)
            {
                var score = await _gameRules.CalculateScore(player, Context);
                results[$"Player_{player.Id}_Score"] = score;
            }

            return new GameResult(winners, losers, results);
        }
    }
} 