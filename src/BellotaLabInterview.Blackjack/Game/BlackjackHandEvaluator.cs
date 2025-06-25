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

            var description = total switch
            {
                21 when cards.Count == 2 => "Blackjack!",
                _ when total > 21 => "Bust",
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