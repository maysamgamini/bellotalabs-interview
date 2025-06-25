using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BellotaLabInterview.Blackjack.Cards;
using BellotaLabInterview.Blackjack.Game;
using BellotaLabInterview.Core.Domain.Cards;
using BellotaLabInterview.Core.Domain.Game;
using BellotaLabInterview.Core.Domain.Players;
using Moq;
using Xunit;

namespace BellotaLabInterview.Tests
{
    public class BlackjackTests
    {
        [Fact]
        public void BlackjackCard_CalculatesCorrectPoints()
        {
            // Arrange & Act
            var aceCard = new BlackjackCard(CardSuit.Spades, CardRank.Ace);
            var kingCard = new BlackjackCard(CardSuit.Hearts, CardRank.King);
            var tenCard = new BlackjackCard(CardSuit.Diamonds, CardRank.Ten);
            var twoCard = new BlackjackCard(CardSuit.Clubs, CardRank.Two);

            var contextMock = new Mock<IGameContext>();

            // Assert
            Assert.Equal(11, aceCard.GetValue(contextMock.Object));
            Assert.Equal(10, kingCard.GetValue(contextMock.Object));
            Assert.Equal(10, tenCard.GetValue(contextMock.Object));
            Assert.Equal(2, twoCard.GetValue(contextMock.Object));
        }

        [Fact]
        public async Task BlackjackHandEvaluator_HandlesAcesCorrectly()
        {
            // Arrange
            var evaluator = new BlackjackHandEvaluator();
            var contextMock = new Mock<IGameContext>();
            var hand = new List<ICard>
            {
                new BlackjackCard(CardSuit.Spades, CardRank.Ace),
                new BlackjackCard(CardSuit.Hearts, CardRank.Ace),
                new BlackjackCard(CardSuit.Diamonds, CardRank.Nine)
            };

            // Act
            var handRank = await evaluator.EvaluateHand(hand, contextMock.Object);

            // Assert
            Assert.Equal(21, handRank.Value); // 11 + 1 + 9 = 21
        }

        [Fact]
        public async Task BlackjackHandEvaluator_DetectsBust()
        {
            // Arrange
            var evaluator = new BlackjackHandEvaluator();
            var contextMock = new Mock<IGameContext>();
            var hand = new List<ICard>
            {
                new BlackjackCard(CardSuit.Spades, CardRank.King),
                new BlackjackCard(CardSuit.Hearts, CardRank.Queen),
                new BlackjackCard(CardSuit.Diamonds, CardRank.Two)
            };

            // Act
            var isValid = await evaluator.IsValidHand(hand, contextMock.Object);

            // Assert
            Assert.False(isValid); // 10 + 10 + 2 = 22 (bust)
        }

        [Fact]
        public async Task BlackjackGameRules_DeterminesWinnersCorrectly()
        {
            // Arrange
            var evaluator = new BlackjackHandEvaluator();
            var rules = new BlackjackGameRules(evaluator);
            
            var dealer = new Mock<IPlayer>();
            dealer.Setup(d => d.Hand).Returns(new List<ICard> 
            { 
                new BlackjackCard(CardSuit.Spades, CardRank.King),
                new BlackjackCard(CardSuit.Hearts, CardRank.Six)
            });

            var player1 = new Mock<IPlayer>();
            player1.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Diamonds, CardRank.Ten),
                new BlackjackCard(CardSuit.Clubs, CardRank.Nine)
            });

            var player2 = new Mock<IPlayer>();
            player2.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Spades, CardRank.Eight),
                new BlackjackCard(CardSuit.Hearts, CardRank.Seven)
            });

            var stateMock = new Mock<IGameState>();
            stateMock.Setup(s => s.CurrentPlayer).Returns(dealer.Object);
            stateMock.Setup(s => s.Players).Returns(new[] { player1.Object, player2.Object, dealer.Object });

            var contextMock = new Mock<IGameContext>();
            contextMock.Setup(c => c.State).Returns(stateMock.Object);

            // Act
            var winners = await rules.DetermineWinners(contextMock.Object);

            // Assert
            Assert.Single(winners);
            Assert.Contains(player1.Object, winners); // Player 1 wins with 19 vs dealer's 16
        }

        [Fact]
        public async Task BlackjackGame_DealerStandsOnSeventeen()
        {
            // Arrange
            var contextMock = new Mock<IGameContext>();
            var cardFactoryMock = new Mock<ICardFactory>();
            var handEvaluator = new BlackjackHandEvaluator();
            var deckMock = new Mock<IDeck>();
            var gameRules = new BlackjackGameRules(handEvaluator);

            var dealer = new Mock<IPlayer>();
            var dealerHand = new List<ICard>
            {
                new BlackjackCard(CardSuit.Spades, CardRank.Ten),
                new BlackjackCard(CardSuit.Hearts, CardRank.Seven)
            };
            dealer.Setup(d => d.Hand).Returns(dealerHand);

            var stateMock = new Mock<IGameState>();
            stateMock.Setup(s => s.CurrentPlayer).Returns(dealer.Object);
            contextMock.Setup(c => c.State).Returns(stateMock.Object);

            var game = new BlackjackGame(contextMock.Object, cardFactoryMock.Object, handEvaluator, deckMock.Object, gameRules);

            // Act
            var isGameOver = await game.IsGameOver();

            // Assert
            Assert.True(isGameOver); // Dealer should stand on 17
            deckMock.Verify(d => d.DrawCards(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task BlackjackHandEvaluator_Recognizes5CardCharlie()
        {
            // Arrange
            var evaluator = new BlackjackHandEvaluator();
            var contextMock = new Mock<IGameContext>();
            var hand = new List<ICard>
            {
                new BlackjackCard(CardSuit.Hearts, CardRank.Two),     // 2
                new BlackjackCard(CardSuit.Diamonds, CardRank.Three), // 3
                new BlackjackCard(CardSuit.Clubs, CardRank.Four),     // 4
                new BlackjackCard(CardSuit.Spades, CardRank.Five),    // 5
                new BlackjackCard(CardSuit.Hearts, CardRank.Three)    // 3
            };                                                        // Total: 17

            // Act
            var handRank = await evaluator.EvaluateHand(hand, contextMock.Object);

            // Assert
            Assert.Equal(22, handRank.Value); // Special value for 5-Card Charlie
            Assert.Equal("5-Card Charlie!", handRank.Description);
            Assert.True(await evaluator.IsValidHand(hand, contextMock.Object)); // Should be valid despite value > 21
        }

        [Fact]
        public async Task BlackjackGameRules_5CardCharlieBeatsDealerBlackjack()
        {
            // Arrange
            var evaluator = new BlackjackHandEvaluator();
            var rules = new BlackjackGameRules(evaluator);
            
            // Setup dealer with blackjack
            var dealer = new Mock<IPlayer>();
            dealer.Setup(d => d.Hand).Returns(new List<ICard> 
            { 
                new BlackjackCard(CardSuit.Spades, CardRank.Ace),
                new BlackjackCard(CardSuit.Hearts, CardRank.King)
            });

            // Setup player with 5-Card Charlie
            var player = new Mock<IPlayer>();
            var playerHand = new List<ICard>
            {
                new BlackjackCard(CardSuit.Hearts, CardRank.Two),     // 2
                new BlackjackCard(CardSuit.Diamonds, CardRank.Three), // 3
                new BlackjackCard(CardSuit.Clubs, CardRank.Four),     // 4
                new BlackjackCard(CardSuit.Spades, CardRank.Five),    // 5
                new BlackjackCard(CardSuit.Hearts, CardRank.Three)    // 3
            };                                                        // Total: 17
            player.Setup(p => p.Hand).Returns(playerHand);

            var stateMock = new Mock<IGameState>();
            stateMock.Setup(s => s.Players).Returns(new[] { player.Object, dealer.Object });

            var contextMock = new Mock<IGameContext>();
            contextMock.Setup(c => c.State).Returns(stateMock.Object);

            // Act
            var winners = await rules.DetermineWinners(contextMock.Object);
            var isGameOver = await rules.IsGameOver(contextMock.Object);

            // Assert
            Assert.True(isGameOver); // Game should be over when 5-Card Charlie is achieved
            Assert.Single(winners); // Should only have one winner
            Assert.Contains(player.Object, winners); // Player with 5-Card Charlie should win
            Assert.DoesNotContain(dealer.Object, winners); // Dealer should lose despite having blackjack
        }
    }
} 