using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BellotaLabInterview.Core.Domain.Cards;
using BellotaLabInterview.Core.Domain.Game;
using BellotaLabInterview.Core.Domain.Players;

namespace BellotaLabInterview.Blackjack.Game
{
    public class BlackjackGameRules : IGameRules
    {
        private readonly IHandEvaluator _handEvaluator;
        private const int DEALER_STAND_VALUE = 17;

        public int MinPlayers => 1;  // One player vs dealer
        public int MaxPlayers => 7;  // Standard casino table size
        public int InitialHandSize => 2;  // Initial deal is 2 cards

        public BlackjackGameRules(IHandEvaluator handEvaluator)
        {
            _handEvaluator = handEvaluator;
        }

        public async Task<bool> CanPlayerAct(IPlayer player, IGameContext context)
        {
            // Player can act if:
            // 1. It's their turn
            // 2. They haven't busted
            // 3. They're not the dealer (dealer acts last)
            if (context.State.CurrentPlayer != player || player == context.State.Players.Last())
                return false;

            var handRank = await _handEvaluator.EvaluateHand(player.Hand, context);
            return handRank.Value <= 21;
        }

        public Task<bool> IsValidTurn(IPlayer player, IGameContext context)
        {
            // Valid turn if it's the player's turn and they haven't busted
            return Task.FromResult(
                context.State.CurrentPlayer == player &&
                player.Hand.Any()
            );
        }

        public Task<bool> IsValidMove(IPlayer player, ICard card, IGameContext context)
        {
            // In Blackjack, players don't play cards, they only receive them
            return Task.FromResult(false);
        }

        public Task<bool> IsValidPlay(IPlayer player, IReadOnlyList<ICard> cards, IGameContext context)
        {
            // In Blackjack, players don't play cards, they only receive them
            return Task.FromResult(false);
        }

        public Task<bool> CanTransitionState(GameState currentState, GameState nextState, IGameContext context)
        {
            // Allow all state transitions for now
            // Could be enhanced to enforce specific game flow
            return Task.FromResult(true);
        }

        public async Task<bool> IsGameOver(IGameContext context)
        {
            if (context?.State?.Players == null || !context.State.Players.Any())
                return true;

            // Check if all players have busted
            var allPlayersBusted = true;
            foreach (var player in context.State.Players.SkipLast(1)) // Skip dealer
            {
                var handRank = await _handEvaluator.EvaluateHand(player.Hand, context);
                if (handRank.Value <= 21)
                {
                    allPlayersBusted = false;
                    break;
                }
            }

            // If all players busted, game is over - dealer wins automatically
            if (allPlayersBusted)
                return true;

            // Otherwise, check dealer's hand
            var dealer = context.State.Players.Last();
            var dealerHandRank = await _handEvaluator.EvaluateHand(dealer.Hand, context);

            // Game is over if dealer busts or has 17 or more
            return dealerHandRank.Value >= DEALER_STAND_VALUE || dealerHandRank.Value > 21;
        }

        public async Task<IReadOnlyList<IPlayer>> DetermineWinners(IGameContext context)
        {
            var winners = new List<IPlayer>();
            var dealer = context.State.Players.Last();

            // Check if all players have busted
            var allPlayersBusted = true;
            foreach (var player in context.State.Players.SkipLast(1)) // Skip dealer
            {
                var handRank = await _handEvaluator.EvaluateHand(player.Hand, context);
                if (handRank.Value <= 21)
                {
                    allPlayersBusted = false;
                    break;
                }
            }

            // If all players busted, dealer wins automatically
            if (allPlayersBusted)
            {
                winners.Add(dealer);
                return winners;
            }

            var dealerHandRank = await _handEvaluator.EvaluateHand(dealer.Hand, context);
            var dealerScore = dealerHandRank.Value;

            // If dealer busts, all non-busted players win
            if (dealerScore > 21)
            {
                foreach (var player in context.State.Players.SkipLast(1)) // Skip dealer
                {
                    var playerHandRank = await _handEvaluator.EvaluateHand(player.Hand, context);
                    if (playerHandRank.Value <= 21)
                    {
                        winners.Add(player);
                    }
                }
                return winners;
            }

            // If dealer doesn't bust, compare each player's hand with dealer
            foreach (var player in context.State.Players.SkipLast(1)) // Skip dealer
            {
                var playerHandRank = await _handEvaluator.EvaluateHand(player.Hand, context);
                var playerScore = playerHandRank.Value;

                // Player wins if:
                // 1. They didn't bust (score <= 21)
                // 2. Their score is higher than dealer's
                if (playerScore <= 21 && playerScore > dealerScore)
                {
                    winners.Add(player);
                }
            }

            // If no players won and dealer didn't bust, dealer wins
            if (winners.Count == 0 && dealerScore <= 21)
            {
                winners.Add(dealer);
            }

            return winners;
        }

        public Task<int> CalculateScore(IPlayer player, IGameContext context)
        {
            return _handEvaluator.EvaluateHand(player.Hand, context)
                .ContinueWith(t => t.Result.Value);
        }
    }
} 