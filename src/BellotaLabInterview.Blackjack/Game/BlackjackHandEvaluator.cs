using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BellotaLabInterview.Core.Domain.Cards;
using BellotaLabInterview.Core.Domain.Game;

namespace BellotaLabInterview.Blackjack.Game
{
    public class BlackjackHandEvaluator : HandEvaluatorBase
    {
        public override Task<HandRank> EvaluateHand(IReadOnlyList<ICard> cards, IGameContext context)
        {
            // During the game, only count face-up cards for dealer
            var isDealer = context?.State?.Players?.LastOrDefault() == context?.State?.CurrentPlayer;
            var isGameOver = context?.State?.CurrentState == GameState.GameOver;

            // Only filter face-down cards during dealer's turn
            var visibleCards = (isDealer && !isGameOver)
                ? cards.Where(c => !(c is StandardCard sc) || sc.IsFaceUp).ToList()
                : cards.ToList();
            
            var total = visibleCards.Sum(card => card.GetValue(context));
            var aceCount = visibleCards.Count(card => card is StandardCard sc && sc.Rank == CardRank.Ace);

            // Adjust for aces
            while (total > 21 && aceCount > 0)
            {
                total -= 10; // Convert an ace from 11 to 1
                aceCount--;
            }

            var description = (visibleCards.Count, total) switch
            {
                (5, <= 21) => "5-Card Charlie!",
                (2, 21) => "Blackjack!",
                (_, > 21) => "Bust",
                _ => $"Total: {total}"
            };

            return Task.FromResult(new HandRank(total, description));
        }

        public override async Task<bool> IsValidHand(IReadOnlyList<ICard> cards, IGameContext context)
        {
            if (!await base.IsValidHand(cards, context))
                return false;

            var handRank = await EvaluateHand(cards, context);
            return handRank.Value <= 21;
        }

        public override Task<IReadOnlyList<ICard>> GetPlayableCards(IReadOnlyList<ICard> hand, IGameContext context)
        {
            // In Blackjack, all cards are playable when it's your turn
            return Task.FromResult<IReadOnlyList<ICard>>(hand);
        }
    }
} 