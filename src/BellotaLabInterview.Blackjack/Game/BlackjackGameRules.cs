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
            var dealerHandRank = await _handEvaluator.EvaluateHand(dealer.Hand, context);
            var dealerHasBlackjack = dealer.Hand.Count == 2 && dealerHandRank.Value == 21;
            var dealerHas5CardCharlie = dealer.Hand.Count == 5 && dealerHandRank.Value <= 21;

            // Check each player's hand
            foreach (var player in context.State.Players.SkipLast(1)) // Skip dealer
            {
                var handRank = await _handEvaluator.EvaluateHand(player.Hand, context);
                // Skip busted players
                if(handRank.Value > 21)
                {
                    continue;
                }

                // 5-Card Charlie wins if under 21
                if (player.Hand.Count == 5 && handRank.Value <= 21)
                {
                    winners.Add(player);
                    continue; // Skip normal win condition checks for this player
                }

                // Dealer busts
                if (dealerHandRank.Value > 21)
                {
                    winners.Add(player);
                    continue;
                }

                var playerHasBlackjack = player.Hand.Count == 2 && handRank.Value == 21;
                // For non-5-Card Charlie hands, check normal winning conditions
                if (playerHasBlackjack && !dealerHasBlackjack)
                {
                    winners.Add(player);
                }
                else if (handRank.Value > dealerHandRank.Value) // Player score beats dealer
                {
                    winners.Add(player);
                }
                else if (handRank.Value == dealerHandRank.Value) // Push (tie)
                {
                    // In case of tie:
                    // - If both have blackjack, it's a push
                    // - If neither has blackjack, it's a push
                    // - If one has blackjack, they win (handled above)
                    if (playerHasBlackjack == dealerHasBlackjack)
                    {
                        continue; // Push - neither wins
                    }
                }
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