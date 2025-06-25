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
            var total = cards.Sum(card => card.GetValue(context));
            var aceCount = cards.Count(card => card is StandardCard sc && sc.Rank == CardRank.Ace);

            // Adjust for aces
            while (total > 21 && aceCount > 0)
            {
                total -= 10; // Convert an ace from 11 to 1
                aceCount--;
            }

            var description = (cards.Count, total) switch
            {
                (5, <= 21) => "5-Card Charlie!",
                (2, 21) => "Blackjack!",
                (_, > 21) => "Bust",
                _ => $"Total: {total}"
            };

            // For 5-Card Charlie, we'll return a special value higher than 21 but not considered a bust
            var value = (cards.Count, total) switch
            {
                (5, <= 21) => 22, // Special value for 5-Card Charlie to ensure it beats everything else
                (_, _) => total
            };

            return Task.FromResult(new HandRank(value, description));
        }

        public override async Task<bool> IsValidHand(IReadOnlyList<ICard> cards, IGameContext context)
        {
            if (!await base.IsValidHand(cards, context))
                return false;

            var handRank = await EvaluateHand(cards, context);
            // Consider 5-Card Charlie as a valid hand even though its value is 22
            return handRank.Value <= 21 || (cards.Count == 5 && handRank.Value == 22);
        }

        public override Task<IReadOnlyList<ICard>> GetPlayableCards(IReadOnlyList<ICard> hand, IGameContext context)
        {
            // In Blackjack, all cards are playable when it's your turn
            return Task.FromResult<IReadOnlyList<ICard>>(hand);
        }
    }
} 