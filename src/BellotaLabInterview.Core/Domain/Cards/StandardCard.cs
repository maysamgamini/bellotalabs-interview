namespace BellotaLabInterview.Core.Domain.Cards
{
    public abstract record StandardCard : CardBase
    {
        public CardSuit Suit { get; init; }
        public CardRank Rank { get; init; }
        public bool IsFaceUp { get; private set; }
        private static bool _useUnicode = true;

        protected StandardCard(CardSuit suit, CardRank rank)
        {
            Suit = suit;
            Rank = rank;
            IsFaceUp = true;  // Cards are face up by default
        }

        private string GetCardFormat() => IsFaceUp 
            ? $"{GetRankSymbol(Rank)}{GetSuitSymbol(Suit)}"
            : "XX";
        
        public override string DisplayName => GetCardFormat();

        protected static string GetSuitSymbol(CardSuit suit)
        {
            if (_useUnicode)
            {
                try
                {
                    return suit switch
                    {
                        CardSuit.Hearts => "♥",
                        CardSuit.Diamonds => "♦",
                        CardSuit.Clubs => "♣",
                        CardSuit.Spades => "♠",
                        _ => suit.ToString()
                    };
                }
                catch
                {
                    _useUnicode = false;
                }
            }
            
            // Fallback to ASCII symbols
            return suit switch
            {
                CardSuit.Hearts => "<3",    // Heart
                CardSuit.Diamonds => "<>",   // Diamond
                CardSuit.Clubs => "()",     // Club
                CardSuit.Spades => "^",     // Spade
                _ => suit.ToString()
            };
        }

        protected static string GetRankSymbol(CardRank rank) => rank switch
        {
            CardRank.Ace => "A",
            CardRank.Jack => "J",
            CardRank.Queen => "Q",
            CardRank.King => "K",
            _ => ((int)rank).ToString()
        };

        public override string ToString() => GetCardFormat();

        public void FlipFaceUp() => IsFaceUp = true;
        public void FlipFaceDown() => IsFaceUp = false;
    }
} 