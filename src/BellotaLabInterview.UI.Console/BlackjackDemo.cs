using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BellotaLabInterview.Blackjack.Cards;
using BellotaLabInterview.Core.Domain.Cards;
using BellotaLabInterview.Core.Domain.Game;
using BellotaLabInterview.Core.Domain.Players;
using Microsoft.Extensions.DependencyInjection;

namespace BellotaLabInterview.UI.Console
{
    public class BlackjackDemo
    {
        private readonly IGame _game;
        private readonly IGameContext _context;
        private readonly List<IPlayer> _players;
        private readonly IHandEvaluator _handEvaluator;
        private readonly IDeck _deck;

        public BlackjackDemo(IServiceProvider serviceProvider)
        {
            _game = serviceProvider.GetRequiredService<IGame>();
            _context = serviceProvider.GetRequiredService<IGameContext>();
            _handEvaluator = serviceProvider.GetRequiredService<IHandEvaluator>();
            _deck = serviceProvider.GetRequiredService<IDeck>();
            _players = new List<IPlayer>();
        }

        public async Task RunDemo()
        {
            // Set console output encoding to UTF-8
            System.Console.OutputEncoding = System.Text.Encoding.UTF8;

            System.Console.WriteLine("Welcome to Blackjack Demo!");
            System.Console.WriteLine("---------------------------\n");

            // Setup players
            var player1 = new DemoPlayer("Player 1");
            var player2 = new DemoPlayer("Player 2");
            var dealer = new DemoPlayer("Dealer");
            _players.AddRange(new[] { player1, player2, dealer });

            // Initialize game
            await _game.Initialize(_players);
            System.Console.WriteLine("Game initialized with 2 players and a dealer.\n");

            // Deal initial cards
            await _game.StartGame();
            await ShowGameState();

            // Player turns
            foreach (var player in _players.SkipLast(1)) // Skip dealer
            {
                await HandlePlayerTurn(player);
            }

            // Dealer's turn
            await HandleDealerTurn();

            // Show results
            var gameResult = await _game.GetGameResult();
            System.Console.WriteLine("\nGame Over! Final hands:");
            await ShowGameState();

            // Get dealer's score
            var dealerScore = await _handEvaluator.EvaluateHand(_players.Last().Hand, _context);

            System.Console.WriteLine("\nResults:");
            System.Console.WriteLine("--------");

            // Check if all players busted first
            var allPlayersBusted = true;
            foreach (var player in _players.SkipLast(1))
            {
                var playerScore = await _handEvaluator.EvaluateHand(player.Hand, _context);
                if (playerScore.Value <= 21)
                {
                    allPlayersBusted = false;
                    break;
                }
            }

            if (allPlayersBusted)
            {
                System.Console.WriteLine("All players busted! Dealer wins automatically!");
                foreach (var player in _players.SkipLast(1))
                {
                    var playerScore = await _handEvaluator.EvaluateHand(player.Hand, _context);
                    System.Console.WriteLine($"{player.Name} ({playerScore.Value}) vs Dealer ({dealerScore.Value}) - BUST - Dealer wins");
                }
            }
            else
            {
                // Show each player's result compared to dealer
                foreach (var player in _players.SkipLast(1))
                {
                    var playerScore = await _handEvaluator.EvaluateHand(player.Hand, _context);
                    string outcomeText;
                    string winner;

                    if (playerScore.Value > 21)
                    {
                        outcomeText = "BUST";
                        winner = "Dealer wins";
                    }
                    else if (dealerScore.Value > 21)
                    {
                        outcomeText = "WIN (Dealer busted)";
                        winner = $"{player.Name} wins";
                    }
                    else if (playerScore.Value > dealerScore.Value)
                    {
                        outcomeText = "WIN";
                        winner = $"{player.Name} wins";
                    }
                    else if (playerScore.Value < dealerScore.Value)
                    {
                        outcomeText = "LOSE";
                        winner = "Dealer wins";
                    }
                    else
                    {
                        outcomeText = "PUSH (Tie)";
                        winner = "Push - No winner";
                    }

                    System.Console.WriteLine($"{player.Name} ({playerScore.Value}) vs Dealer ({dealerScore.Value}) - {outcomeText} - {winner}");
                }
            }

            System.Console.WriteLine("\nPress any key to exit...");
            System.Console.ReadKey();
        }

        private async Task HandlePlayerTurn(IPlayer player)
        {
            System.Console.WriteLine($"\n{player.Name}'s turn:");
            await ShowPlayerHand(player);

            while (await _game.Context.CanPlayerAct(player))
            {
                System.Console.Write("Hit or Stand? (H/S): ");
                var key = System.Console.ReadKey();
                System.Console.WriteLine();

                if (key.Key == ConsoleKey.H)
                {
                    var card = await _deck.DrawCard();
                    await player.AddCard(card);
                    await ShowPlayerHand(player);

                    var handRank = await _handEvaluator.EvaluateHand(player.Hand, _context);
                    if (handRank.Value > 21)
                    {
                        System.Console.WriteLine("Bust!");
                        break;
                    }
                }
                else if (key.Key == ConsoleKey.S)
                {
                    break;
                }
            }

            // Advance to next player
            await _context.AdvanceNextPlayer();
        }

        private async Task HandleDealerTurn()
        {
            var dealer = _players.Last();
            System.Console.WriteLine($"\n{dealer.Name}'s turn:");
            
            // Check if all players have busted
            var allPlayersBusted = true;
            foreach (var player in _players.SkipLast(1))
            {
                var handRank = await _handEvaluator.EvaluateHand(player.Hand, _context);
                if (handRank.Value <= 21)
                {
                    allPlayersBusted = false;
                    break;
                }
            }

            // Flip all dealer's cards face up at the start of their turn
            foreach (var card in dealer.Hand.Cast<BlackjackCard>())
            {
                card.FlipFaceUp();
            }
            await ShowPlayerHand(dealer);

            // If all players busted, dealer doesn't need to play
            if (allPlayersBusted)
            {
                System.Console.WriteLine("All players busted! Dealer wins automatically!");
                return;
            }

            while (!await _game.IsGameOver())
            {
                System.Console.WriteLine("Dealer hits...");
                var card = await _deck.DrawCard();
                if (card is BlackjackCard blackjackCard)
                {
                    blackjackCard.FlipFaceUp(); // Ensure new cards are face up
                }
                await dealer.AddCard(card);
                await ShowPlayerHand(dealer);
                await Task.Delay(1000); // Add a small delay for dramatic effect
            }
        }

        private async Task ShowGameState()
        {
            foreach (var player in _players)
            {
                await ShowPlayerHand(player);
            }
            System.Console.WriteLine();
        }

        private async Task ShowPlayerHand(IPlayer player)
        {
            // For dealer, only show value of face-up cards during game
            if (player.Name == "Dealer" && !await _game.IsGameOver() && player != _context.State.CurrentPlayer)
            {
                // First flip cards to their correct state
                if (player.Hand.Count > 0)
                {
                    var firstCard = player.Hand[0] as BlackjackCard;
                    if (firstCard != null)
                    {
                        firstCard.FlipFaceUp();
                    }
                    for (int i = 1; i < player.Hand.Count; i++)
                    {
                        var card = player.Hand[i] as BlackjackCard;
                        if (card != null)
                        {
                            card.FlipFaceDown();
                        }
                    }
                }
            }

            var handRank = await _handEvaluator.EvaluateHand(player.Hand, _context);
            
            // Only show hand value for non-dealer or when all cards are visible
            if (player.Name != "Dealer" || await _game.IsGameOver() || player == _context.State.CurrentPlayer)
            {
                System.Console.Write($"{player.Name}'s hand ({handRank.Value}): ");
            }
            else
            {
                System.Console.Write($"{player.Name}'s hand: ");
            }
            
            if (player.Name == "Dealer" && !await _game.IsGameOver() && player != _context.State.CurrentPlayer)
            {
                // Show first card, hide the rest for dealer during game (but not during dealer's turn)
                if (player.Hand.Count > 0)
                {
                    var firstCard = player.Hand[0] as BlackjackCard;
                    if (firstCard != null)
                    {
                        System.Console.Write(firstCard.DisplayName);
                    }
                    for (int i = 1; i < player.Hand.Count; i++)
                    {
                        System.Console.Write(" XX");
                    }
                }
            }
            else
            {
                // Show all cards for players and dealer after game over or during dealer's turn
                var cards = player.Hand.Cast<BlackjackCard>();
                foreach (var card in cards)
                {
                    card.FlipFaceUp();
                }
                System.Console.Write(string.Join(" ", cards.Select(c => c.DisplayName)));
            }
            System.Console.WriteLine();
        }
    }

    public class DemoPlayer : IPlayer
    {
        private readonly List<ICard> _hand = new();
        private PlayerState _state = PlayerState.Waiting;
        private int _points = 0;

        public DemoPlayer(string name)
        {
            Name = name;
            Id = Guid.NewGuid();
        }

        public Guid Id { get; }
        public string Name { get; }
        public IReadOnlyList<ICard> Hand => _hand;
        public PlayerState State => _state;
        public int Points => _points;

        public Task AddCard(ICard card)
        {
            _hand.Add(card);
            return Task.CompletedTask;
        }

        public Task RemoveCard(ICard card)
        {
            _hand.Remove(card);
            return Task.CompletedTask;
        }

        public Task UpdatePoints(int points)
        {
            _points = points;
            return Task.CompletedTask;
        }
    }
} 