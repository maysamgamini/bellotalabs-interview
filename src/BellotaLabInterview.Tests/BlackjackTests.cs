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
            Assert.Equal(17, handRank.Value); // Actual total value
            Assert.Equal("5-Card Charlie!", handRank.Description);
            Assert.True(await evaluator.IsValidHand(hand, contextMock.Object));
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

        [Fact]
        public async Task BlackjackGameRules_Multiple5CardCharliesAllWin()
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

            // Setup player1 with 5-Card Charlie
            var player1 = new Mock<IPlayer>();
            var player1Hand = new List<ICard>
            {
                new BlackjackCard(CardSuit.Hearts, CardRank.Two),     // 2
                new BlackjackCard(CardSuit.Diamonds, CardRank.Three), // 3
                new BlackjackCard(CardSuit.Clubs, CardRank.Four),     // 4
                new BlackjackCard(CardSuit.Spades, CardRank.Five),    // 5
                new BlackjackCard(CardSuit.Hearts, CardRank.Three)    // 3
            };                                                        // Total: 17
            player1.Setup(p => p.Hand).Returns(player1Hand);

            // Setup player2 with 5-Card Charlie
            var player2 = new Mock<IPlayer>();
            var player2Hand = new List<ICard>
            {
                new BlackjackCard(CardSuit.Diamonds, CardRank.Two),    // 2
                new BlackjackCard(CardSuit.Hearts, CardRank.Four),     // 4
                new BlackjackCard(CardSuit.Spades, CardRank.Three),    // 3
                new BlackjackCard(CardSuit.Clubs, CardRank.Five),      // 5
                new BlackjackCard(CardSuit.Diamonds, CardRank.Three)   // 3
            };                                                         // Total: 17
            player2.Setup(p => p.Hand).Returns(player2Hand);

            var stateMock = new Mock<IGameState>();
            stateMock.Setup(s => s.Players).Returns(new[] { player1.Object, player2.Object, dealer.Object });

            var contextMock = new Mock<IGameContext>();
            contextMock.Setup(c => c.State).Returns(stateMock.Object);

            // Act
            var winners = await rules.DetermineWinners(contextMock.Object);
            var isGameOver = await rules.IsGameOver(contextMock.Object);

            // Assert
            Assert.Equal(2, winners.Count); // Should have two winners
            Assert.Contains(player1.Object, winners); // Player 1 should win with 5-Card Charlie
            Assert.Contains(player2.Object, winners); // Player 2 should win with 5-Card Charlie
            Assert.DoesNotContain(dealer.Object, winners); // Dealer should lose despite having blackjack
        }

        [Fact]
        public async Task DetermineWinners_No5CardCharlie_NormalWinningConditionsApply()
        {
            // Arrange
            var evaluator = new BlackjackHandEvaluator();
            var rules = new BlackjackGameRules(evaluator);
            
            var player1 = new Mock<IPlayer>();
            player1.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Hearts, CardRank.Ten),    // 10
                new BlackjackCard(CardSuit.Diamonds, CardRank.Nine)  // 9
            });                                                      // Total: 19

            var player2 = new Mock<IPlayer>();
            player2.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Spades, CardRank.King),   // 10
                new BlackjackCard(CardSuit.Hearts, CardRank.Queen),  // 10
                new BlackjackCard(CardSuit.Diamonds, CardRank.Four)  // 4
            });                                                      // Total: 24 (Bust)

            var player3 = new Mock<IPlayer>();
            player3.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Clubs, CardRank.Ten),     // 10
                new BlackjackCard(CardSuit.Hearts, CardRank.Eight)   // 8
            });                                                      // Total: 18

            // Dealer has 19
            var dealer = new Mock<IPlayer>();
            var dealerCards = new List<ICard>
            {
                new BlackjackCard(CardSuit.Spades, CardRank.Nine),    // 9
                new BlackjackCard(CardSuit.Hearts, CardRank.Ten)      // 10
            };
            foreach (var card in dealerCards.Cast<BlackjackCard>())
            {
                card.FlipFaceUp(); // Ensure dealer's cards are face up for end-game evaluation
            }
            dealer.Setup(d => d.Hand).Returns(dealerCards);

            var stateMock = new Mock<IGameState>();
            stateMock.Setup(s => s.Players).Returns(new[] { player1.Object, player2.Object, player3.Object, dealer.Object });

            var contextMock = new Mock<IGameContext>();
            contextMock.Setup(c => c.State).Returns(stateMock.Object);

            // Act
            var winners = await rules.DetermineWinners(contextMock.Object);

            // Assert
            Assert.Empty(winners); // No winners - player1 pushes, player2 busts, player3 loses
            Assert.DoesNotContain(player1.Object, winners); // Push with dealer at 19
            Assert.DoesNotContain(player2.Object, winners); // Busted with 24
            Assert.DoesNotContain(player3.Object, winners); // Loses with 18
            Assert.DoesNotContain(dealer.Object, winners); // Dealer pushes with player1
        }

        [Fact]
        public async Task DetermineWinners_5CardCharlieAndNormalHands_OnlyCharlieWins()
        {
            // Arrange
            var evaluator = new BlackjackHandEvaluator();
            var rules = new BlackjackGameRules(evaluator);
            
            // Player 1 has 5-Card Charlie
            var player1 = new Mock<IPlayer>();
            player1.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Hearts, CardRank.Two),     // 2
                new BlackjackCard(CardSuit.Diamonds, CardRank.Three), // 3
                new BlackjackCard(CardSuit.Clubs, CardRank.Four),     // 4
                new BlackjackCard(CardSuit.Spades, CardRank.Five),    // 5
                new BlackjackCard(CardSuit.Hearts, CardRank.Three)    // 3
            });                                                       // Total: 17 (5-Card Charlie)

            // Player 2 has a good hand but not 5-Card Charlie
            var player2 = new Mock<IPlayer>();
            player2.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Spades, CardRank.Ten),    // 10
                new BlackjackCard(CardSuit.Hearts, CardRank.Nine)    // 9
            });                                                      // Total: 19

            // Player 3 has 5-Card Charlie with lower total
            var player3 = new Mock<IPlayer>();
            player3.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Diamonds, CardRank.Two),   // 2
                new BlackjackCard(CardSuit.Hearts, CardRank.Three),   // 3
                new BlackjackCard(CardSuit.Spades, CardRank.Four),    // 4
                new BlackjackCard(CardSuit.Clubs, CardRank.Three),    // 3
                new BlackjackCard(CardSuit.Diamonds, CardRank.Four)   // 4
            });                                                       // Total: 16 (5-Card Charlie)

            // Dealer has 19
            var dealer = new Mock<IPlayer>();
            var dealerCards = new List<ICard>
            {
                new BlackjackCard(CardSuit.Spades, CardRank.Nine),    // 9
                new BlackjackCard(CardSuit.Hearts, CardRank.Ten)      // 10
            };
            foreach (var card in dealerCards.Cast<BlackjackCard>())
            {
                card.FlipFaceUp(); // Ensure dealer's cards are face up for end-game evaluation
            }
            dealer.Setup(d => d.Hand).Returns(dealerCards);

            var stateMock = new Mock<IGameState>();
            stateMock.Setup(s => s.Players).Returns(new[] { player1.Object, player2.Object, player3.Object, dealer.Object });

            var contextMock = new Mock<IGameContext>();
            contextMock.Setup(c => c.State).Returns(stateMock.Object);

            // Act
            var winners = await rules.DetermineWinners(contextMock.Object);

            // Assert
            Assert.Equal(2, winners.Count); // Both 5-Card Charlie players win
            Assert.Contains(player1.Object, winners); // Wins with 5-Card Charlie (17)
            Assert.DoesNotContain(player2.Object, winners); // Loses with 19 against dealer's 19
            Assert.Contains(player3.Object, winners); // Wins with 5-Card Charlie (16)
            Assert.DoesNotContain(dealer.Object, winners); // Dealer loses to 5-Card Charlies
        }

        [Fact]
        public async Task DetermineWinners_HandlesSpecialCases()
        {
            // Arrange
            var evaluator = new BlackjackHandEvaluator();
            var rules = new BlackjackGameRules(evaluator);
            
            // Player 1 has natural blackjack (Ace + King)
            var player1 = new Mock<IPlayer>();
            player1.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Hearts, CardRank.Ace),    // 11
                new BlackjackCard(CardSuit.Diamonds, CardRank.King)  // 10
            });                                                      // Total: 21 (Natural Blackjack)

            // Player 2 has regular 21 (Three cards)
            var player2 = new Mock<IPlayer>();
            player2.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Spades, CardRank.Seven),  // 7
                new BlackjackCard(CardSuit.Hearts, CardRank.Nine),   // 9
                new BlackjackCard(CardSuit.Diamonds, CardRank.Five)  // 5
            });                                                      // Total: 21 (Regular)

            // Player 3 has same score as dealer
            var player3 = new Mock<IPlayer>();
            player3.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Clubs, CardRank.Ten),     // 10
                new BlackjackCard(CardSuit.Hearts, CardRank.Seven)   // 7
            });                                                      // Total: 17 (Push with dealer)

            // Player 4 has natural blackjack (like player 1)
            var player4 = new Mock<IPlayer>();
            player4.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Clubs, CardRank.Ace),     // 11
                new BlackjackCard(CardSuit.Spades, CardRank.Queen)   // 10
            });                                                      // Total: 21 (Natural Blackjack)

            // Dealer has 19
            var dealer = new Mock<IPlayer>();
            var dealerCards = new List<ICard>
            {
                new BlackjackCard(CardSuit.Spades, CardRank.Nine),    // 9
                new BlackjackCard(CardSuit.Hearts, CardRank.Ten)      // 10
            };
            foreach (var card in dealerCards.Cast<BlackjackCard>())
            {
                card.FlipFaceUp(); // Ensure dealer's cards are face up for end-game evaluation
            }
            dealer.Setup(d => d.Hand).Returns(dealerCards);

            var stateMock = new Mock<IGameState>();
            stateMock.Setup(s => s.Players).Returns(new[] { player1.Object, player2.Object, player3.Object, player4.Object, dealer.Object });

            var contextMock = new Mock<IGameContext>();
            contextMock.Setup(c => c.State).Returns(stateMock.Object);

            // Act
            var winners = await rules.DetermineWinners(contextMock.Object);

            // Assert
            Assert.Equal(3, winners.Count); // Natural blackjacks and regular 21 win
            Assert.Contains(player1.Object, winners); // Wins with natural blackjack
            Assert.Contains(player2.Object, winners); // Wins with regular 21
            Assert.DoesNotContain(player3.Object, winners); // Push with dealer (17)
            Assert.Contains(player4.Object, winners); // Wins with natural blackjack
            Assert.DoesNotContain(dealer.Object, winners); // Dealer loses to higher scores
        }

        [Fact]
        public async Task DetermineWinners_HandlesMultipleNaturalBlackjacks()
        {
            // Arrange
            var evaluator = new BlackjackHandEvaluator();
            var rules = new BlackjackGameRules(evaluator);
            
            // Player 1 has natural blackjack
            var player1 = new Mock<IPlayer>();
            player1.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Hearts, CardRank.Ace),    // 11
                new BlackjackCard(CardSuit.Diamonds, CardRank.King)  // 10
            });                                                      // Total: 21 (Natural Blackjack)

            // Player 2 has natural blackjack
            var player2 = new Mock<IPlayer>();
            player2.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Spades, CardRank.Ace),    // 11
                new BlackjackCard(CardSuit.Hearts, CardRank.Jack)    // 10
            });                                                      // Total: 21 (Natural Blackjack)

            // Dealer has natural blackjack
            var dealer = new Mock<IPlayer>();
            var dealerCards = new List<ICard>
            {
                new BlackjackCard(CardSuit.Spades, CardRank.Ace),     // 11
                new BlackjackCard(CardSuit.Hearts, CardRank.King)     // 10
            };                                                        // Total: 21 (Natural)
            foreach (var card in dealerCards.Cast<BlackjackCard>())
            {
                card.FlipFaceUp(); // Ensure dealer's cards are face up for end-game evaluation
            }
            dealer.Setup(d => d.Hand).Returns(dealerCards);

            var stateMock = new Mock<IGameState>();
            stateMock.Setup(s => s.Players).Returns(new[] { player1.Object, player2.Object, dealer.Object });

            var contextMock = new Mock<IGameContext>();
            contextMock.Setup(c => c.State).Returns(stateMock.Object);

            // Act
            var winners = await rules.DetermineWinners(contextMock.Object);

            // Assert
            Assert.Empty(winners); // All natural blackjacks push with dealer's natural
            Assert.DoesNotContain(player1.Object, winners); // Natural pushes with dealer
            Assert.DoesNotContain(player2.Object, winners); // Natural pushes with dealer
        }

        [Fact]
        public async Task DetermineWinners_VerifyPrecedenceOrder()
        {
            // Arrange
            var evaluator = new BlackjackHandEvaluator();
            var rules = new BlackjackGameRules(evaluator);
            
            // Player 1 has 5-Card Charlie (lowest possible: 2+2+2+2+2=10)
            var player1 = new Mock<IPlayer>();
            player1.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Hearts, CardRank.Two),    // 2
                new BlackjackCard(CardSuit.Diamonds, CardRank.Two),  // 2
                new BlackjackCard(CardSuit.Clubs, CardRank.Two),     // 2
                new BlackjackCard(CardSuit.Spades, CardRank.Two),    // 2
                new BlackjackCard(CardSuit.Hearts, CardRank.Two)     // 2
            });                                                      // Total: 10 (5-Card Charlie)

            // Player 2 has natural blackjack
            var player2 = new Mock<IPlayer>();
            player2.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Spades, CardRank.Ace),    // 11
                new BlackjackCard(CardSuit.Hearts, CardRank.King)    // 10
            });                                                      // Total: 21 (Natural Blackjack)

            // Player 3 has regular 21
            var player3 = new Mock<IPlayer>();
            player3.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Clubs, CardRank.Seven),   // 7
                new BlackjackCard(CardSuit.Diamonds, CardRank.Nine), // 9
                new BlackjackCard(CardSuit.Hearts, CardRank.Five)    // 5
            });                                                      // Total: 21 (Regular)

            // Dealer has 19
            var dealer = new Mock<IPlayer>();
            var dealerCards = new List<ICard>
            {
                new BlackjackCard(CardSuit.Spades, CardRank.Nine),    // 9
                new BlackjackCard(CardSuit.Hearts, CardRank.Ten)      // 10
            };
            foreach (var card in dealerCards.Cast<BlackjackCard>())
            {
                card.FlipFaceUp(); // Ensure dealer's cards are face up for end-game evaluation
            }
            dealer.Setup(d => d.Hand).Returns(dealerCards);

            var stateMock = new Mock<IGameState>();
            stateMock.Setup(s => s.Players).Returns(new[] { player1.Object, player2.Object, player3.Object, dealer.Object });

            var contextMock = new Mock<IGameContext>();
            contextMock.Setup(c => c.State).Returns(stateMock.Object);

            // Act
            var winners = await rules.DetermineWinners(contextMock.Object);

            // Assert
            Assert.Equal(3, winners.Count); // All players beat dealer's 19
            Assert.Contains(player1.Object, winners); // Wins with 5-Card Charlie (10)
            Assert.Contains(player2.Object, winners); // Wins with natural blackjack
            Assert.Contains(player3.Object, winners); // Wins with regular 21
            Assert.DoesNotContain(dealer.Object, winners); // Dealer loses to all players
        }

        [Fact]
        public async Task DetermineWinners_Only5CardCharliePlayersWin()
        {
            // Arrange
            var evaluator = new BlackjackHandEvaluator();
            var rules = new BlackjackGameRules(evaluator);
            
            // Player 1 has 5-Card Charlie
            var player1 = new Mock<IPlayer>();
            player1.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Hearts, CardRank.Two),     // 2
                new BlackjackCard(CardSuit.Diamonds, CardRank.Three), // 3
                new BlackjackCard(CardSuit.Clubs, CardRank.Four),     // 4
                new BlackjackCard(CardSuit.Spades, CardRank.Five),    // 5
                new BlackjackCard(CardSuit.Hearts, CardRank.Three)    // 3
            });                                                       // Total: 17 (5-Card Charlie)

            // Player 2 has good hand (20) but not 5-Card Charlie
            var player2 = new Mock<IPlayer>();
            player2.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Spades, CardRank.King),    // 10
                new BlackjackCard(CardSuit.Hearts, CardRank.Queen)    // 10
            });                                                       // Total: 20

            // Player 3 has 5 cards but busted
            var player3 = new Mock<IPlayer>();
            player3.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Clubs, CardRank.Five),     // 5
                new BlackjackCard(CardSuit.Diamonds, CardRank.Six),   // 6
                new BlackjackCard(CardSuit.Hearts, CardRank.Four),    // 4
                new BlackjackCard(CardSuit.Spades, CardRank.Three),   // 3
                new BlackjackCard(CardSuit.Hearts, CardRank.Four)     // 4
            });                                                       // Total: 22 (Bust)

            // Player 4 has another 5-Card Charlie
            var player4 = new Mock<IPlayer>();
            player4.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Diamonds, CardRank.Ace),   // 1 (adjusted from 11)
                new BlackjackCard(CardSuit.Hearts, CardRank.Two),     // 2
                new BlackjackCard(CardSuit.Spades, CardRank.Three),   // 3
                new BlackjackCard(CardSuit.Clubs, CardRank.Four),     // 4
                new BlackjackCard(CardSuit.Diamonds, CardRank.Five)   // 5
            });                                                       // Total: 15 (5-Card Charlie)

            // Dealer has 19
            var dealer = new Mock<IPlayer>();
            var dealerCards = new List<ICard>
            {
                new BlackjackCard(CardSuit.Spades, CardRank.Nine),    // 9
                new BlackjackCard(CardSuit.Hearts, CardRank.Ten)      // 10
            };
            foreach (var card in dealerCards.Cast<BlackjackCard>())
            {
                card.FlipFaceUp(); // Ensure dealer's cards are face up for end-game evaluation
            }
            dealer.Setup(d => d.Hand).Returns(dealerCards);

            var stateMock = new Mock<IGameState>();
            stateMock.Setup(s => s.Players).Returns(new[] { player1.Object, player2.Object, player3.Object, player4.Object, dealer.Object });

            var contextMock = new Mock<IGameContext>();
            contextMock.Setup(c => c.State).Returns(stateMock.Object);

            // Act
            var winners = await rules.DetermineWinners(contextMock.Object);

            // Assert
            Assert.Equal(3, winners.Count); // Both 5-Card Charlie players and player2 with 20 win against dealer's 19
            Assert.Contains(player1.Object, winners); // Wins with 5-Card Charlie (17)
            Assert.Contains(player2.Object, winners); // Wins with 20 against dealer's 19
            Assert.DoesNotContain(player3.Object, winners); // Busted with 22 (not a valid 5-Card Charlie)
            Assert.Contains(player4.Object, winners); // Wins with 5-Card Charlie (15)
            Assert.DoesNotContain(dealer.Object, winners); // Dealer loses to all winning hands
        }

        [Fact]
        public async Task DetermineWinners_HouseWinsWhenAllPlayersBust()
        {
            // Arrange
            var evaluator = new BlackjackHandEvaluator();
            var rules = new BlackjackGameRules(evaluator);
            
            // Player 1 busts with 23
            var player1 = new Mock<IPlayer>();
            player1.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Hearts, CardRank.King),    // 10
                new BlackjackCard(CardSuit.Diamonds, CardRank.Queen), // 10
                new BlackjackCard(CardSuit.Clubs, CardRank.Three)     // 3
            });                                                       // Total: 23 (Bust)

            // Player 2 busts with 25
            var player2 = new Mock<IPlayer>();
            player2.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Spades, CardRank.Ten),     // 10
                new BlackjackCard(CardSuit.Hearts, CardRank.Nine),    // 9
                new BlackjackCard(CardSuit.Diamonds, CardRank.Six)    // 6
            });                                                       // Total: 25 (Bust)

            // Player 3 busts with 22
            var player3 = new Mock<IPlayer>();
            player3.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Clubs, CardRank.Eight),    // 8
                new BlackjackCard(CardSuit.Diamonds, CardRank.Seven), // 7
                new BlackjackCard(CardSuit.Hearts, CardRank.Seven)    // 7
            });                                                       // Total: 22 (Bust)

            // Dealer has 19
            var dealer = new Mock<IPlayer>();
            var dealerCards = new List<ICard>
            {
                new BlackjackCard(CardSuit.Spades, CardRank.Nine),    // 9
                new BlackjackCard(CardSuit.Hearts, CardRank.Ten)      // 10
            };
            foreach (var card in dealerCards.Cast<BlackjackCard>())
            {
                card.FlipFaceUp(); // Ensure dealer's cards are face up for end-game evaluation
            }
            dealer.Setup(d => d.Hand).Returns(dealerCards);

            var stateMock = new Mock<IGameState>();
            stateMock.Setup(s => s.Players).Returns(new[] { player1.Object, player2.Object, player3.Object, dealer.Object });

            var contextMock = new Mock<IGameContext>();
            contextMock.Setup(c => c.State).Returns(stateMock.Object);

            // Act
            var winners = await rules.DetermineWinners(contextMock.Object);

            // Assert
            Assert.Empty(winners); // House wins (no player winners)
            Assert.DoesNotContain(player1.Object, winners); // Busted with 23
            Assert.DoesNotContain(player2.Object, winners); // Busted with 25
            Assert.DoesNotContain(player3.Object, winners); // Busted with 22
            Assert.DoesNotContain(dealer.Object, winners); // Dealer not in winners list
        }

        [Fact]
        public async Task DetermineWinners_HouseWinsWhenNoPlayersBeatDealer()
        {
            // Arrange
            var evaluator = new BlackjackHandEvaluator();
            var rules = new BlackjackGameRules(evaluator);
            
            // Player 1 has 15
            var player1 = new Mock<IPlayer>();
            player1.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Hearts, CardRank.Eight),    // 8
                new BlackjackCard(CardSuit.Diamonds, CardRank.Seven)   // 7
            });                                                        // Total: 15

            // Player 2 has 16
            var player2 = new Mock<IPlayer>();
            player2.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Spades, CardRank.Ten),      // 10
                new BlackjackCard(CardSuit.Hearts, CardRank.Six)       // 6
            });                                                        // Total: 16

            // Dealer has 19
            var dealer = new Mock<IPlayer>();
            var dealerCards = new List<ICard>
            {
                new BlackjackCard(CardSuit.Spades, CardRank.Nine),    // 9
                new BlackjackCard(CardSuit.Hearts, CardRank.Ten)      // 10
            };
            foreach (var card in dealerCards.Cast<BlackjackCard>())
            {
                card.FlipFaceUp(); // Ensure dealer's cards are face up for end-game evaluation
            }
            dealer.Setup(d => d.Hand).Returns(dealerCards);

            var stateMock = new Mock<IGameState>();
            stateMock.Setup(s => s.Players).Returns(new[] { player1.Object, player2.Object, dealer.Object });

            var contextMock = new Mock<IGameContext>();
            contextMock.Setup(c => c.State).Returns(stateMock.Object);

            // Act
            var winners = await rules.DetermineWinners(contextMock.Object);

            // Assert
            Assert.Empty(winners); // House wins (no player winners)
            Assert.DoesNotContain(player1.Object, winners); // Lost with 15
            Assert.DoesNotContain(player2.Object, winners); // Lost with 16
            Assert.DoesNotContain(dealer.Object, winners); // Dealer not in winners list
        }

        [Fact]
        public async Task DetermineWinners_BasicScenarios()
        {
            // Arrange
            var evaluator = new BlackjackHandEvaluator();
            var rules = new BlackjackGameRules(evaluator);
            
            // Player 1 busts with 23
            var player1 = new Mock<IPlayer>();
            player1.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Hearts, CardRank.King),    // 10
                new BlackjackCard(CardSuit.Diamonds, CardRank.Queen), // 10
                new BlackjackCard(CardSuit.Clubs, CardRank.Three)     // 3
            });                                                       // Total: 23 (Bust)

            // Player 2 pushes with dealer at 19
            var player2 = new Mock<IPlayer>();
            player2.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Spades, CardRank.Ten),     // 10
                new BlackjackCard(CardSuit.Hearts, CardRank.Nine)     // 9
            });                                                       // Total: 19 (Push)

            // Player 3 ties with dealer at 17
            var player3 = new Mock<IPlayer>();
            player3.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Clubs, CardRank.Ten),      // 10
                new BlackjackCard(CardSuit.Diamonds, CardRank.Seven)  // 7
            });                                                       // Total: 17

            // Player 4 loses to dealer with 16
            var player4 = new Mock<IPlayer>();
            player4.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Spades, CardRank.Nine),    // 9
                new BlackjackCard(CardSuit.Hearts, CardRank.Seven)    // 7
            });                                                       // Total: 16

            // Dealer has 19
            var dealer = new Mock<IPlayer>();
            var dealerCards = new List<ICard>
            {
                new BlackjackCard(CardSuit.Spades, CardRank.Nine),    // 9
                new BlackjackCard(CardSuit.Hearts, CardRank.Ten)      // 10
            };
            foreach (var card in dealerCards.Cast<BlackjackCard>())
            {
                card.FlipFaceUp(); // Ensure dealer's cards are face up for end-game evaluation
            }
            dealer.Setup(d => d.Hand).Returns(dealerCards);

            var stateMock = new Mock<IGameState>();
            stateMock.Setup(s => s.Players).Returns(new[] { player1.Object, player2.Object, player3.Object, player4.Object, dealer.Object });

            var contextMock = new Mock<IGameContext>();
            contextMock.Setup(c => c.State).Returns(stateMock.Object);

            // Act
            var winners = await rules.DetermineWinners(contextMock.Object);

            // Assert
            Assert.Empty(winners); // No winners - player2 pushes with dealer
            Assert.DoesNotContain(player1.Object, winners); // Busted with 23
            Assert.DoesNotContain(player2.Object, winners); // Push with dealer at 19
            Assert.DoesNotContain(player3.Object, winners); // Lost with 17
            Assert.DoesNotContain(player4.Object, winners); // Lost with 16
        }

        [Fact]
        public async Task DetermineWinners_BlackjackScenarios()
        {
            // Arrange
            var evaluator = new BlackjackHandEvaluator();
            var rules = new BlackjackGameRules(evaluator);
            
            // Player 1 has natural blackjack
            var player1 = new Mock<IPlayer>();
            player1.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Hearts, CardRank.Ace),     // 11
                new BlackjackCard(CardSuit.Diamonds, CardRank.King)   // 10
            });                                                       // Total: 21 (Natural)

            // Player 2 has regular 21 (three cards)
            var player2 = new Mock<IPlayer>();
            player2.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Spades, CardRank.Seven),   // 7
                new BlackjackCard(CardSuit.Hearts, CardRank.Nine),    // 9
                new BlackjackCard(CardSuit.Clubs, CardRank.Five)      // 5
            });                                                       // Total: 21 (Regular)

            // Player 3 has natural blackjack (like player1)
            var player3 = new Mock<IPlayer>();
            player3.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Clubs, CardRank.Ace),      // 11
                new BlackjackCard(CardSuit.Diamonds, CardRank.Queen)  // 10
            });                                                       // Total: 21 (Natural)

            // Dealer has natural blackjack
            var dealer = new Mock<IPlayer>();
            var dealerCards = new List<ICard>
            {
                new BlackjackCard(CardSuit.Spades, CardRank.Ace),     // 11
                new BlackjackCard(CardSuit.Hearts, CardRank.King)     // 10
            };                                                        // Total: 21 (Natural)
            foreach (var card in dealerCards.Cast<BlackjackCard>())
            {
                card.FlipFaceUp(); // Ensure dealer's cards are face up for end-game evaluation
            }
            dealer.Setup(d => d.Hand).Returns(dealerCards);

            var stateMock = new Mock<IGameState>();
            stateMock.Setup(s => s.Players).Returns(new[] { player1.Object, player2.Object, player3.Object, dealer.Object });

            var contextMock = new Mock<IGameContext>();
            contextMock.Setup(c => c.State).Returns(stateMock.Object);

            // Act
            var winners = await rules.DetermineWinners(contextMock.Object);

            // Assert
            Assert.Empty(winners); // Natural blackjacks push with dealer's natural, regular 21 loses
            Assert.DoesNotContain(player1.Object, winners); // Natural pushes with dealer
            Assert.DoesNotContain(player2.Object, winners); // Regular 21 loses to dealer's natural
            Assert.DoesNotContain(player3.Object, winners); // Natural pushes with dealer
        }

        [Fact]
        public async Task DetermineWinners_5CardCharlieWinsAndRegularHandsPush()
        {
            // Arrange
            var evaluator = new BlackjackHandEvaluator();
            var rules = new BlackjackGameRules(evaluator);
            
            // Player 1 has 5-Card Charlie
            var player1 = new Mock<IPlayer>();
            player1.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Hearts, CardRank.Two),     // 2
                new BlackjackCard(CardSuit.Diamonds, CardRank.Three), // 3
                new BlackjackCard(CardSuit.Clubs, CardRank.Four),     // 4
                new BlackjackCard(CardSuit.Spades, CardRank.Five),    // 5
                new BlackjackCard(CardSuit.Hearts, CardRank.Three)    // 3
            });                                                       // Total: 17 (5-Card Charlie)

            // Player 2 has a good hand but not 5-Card Charlie
            var player2 = new Mock<IPlayer>();
            player2.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Spades, CardRank.Ten),    // 10
                new BlackjackCard(CardSuit.Hearts, CardRank.Nine)    // 9
            });                                                      // Total: 19

            // Player 3 has 5-Card Charlie with lower total
            var player3 = new Mock<IPlayer>();
            player3.Setup(p => p.Hand).Returns(new List<ICard>
            {
                new BlackjackCard(CardSuit.Diamonds, CardRank.Two),   // 2
                new BlackjackCard(CardSuit.Hearts, CardRank.Three),   // 3
                new BlackjackCard(CardSuit.Spades, CardRank.Four),    // 4
                new BlackjackCard(CardSuit.Clubs, CardRank.Three),    // 3
                new BlackjackCard(CardSuit.Diamonds, CardRank.Four)   // 4
            });                                                       // Total: 16 (5-Card Charlie)

            // Dealer has 19
            var dealer = new Mock<IPlayer>();
            var dealerCards = new List<ICard>
            {
                new BlackjackCard(CardSuit.Spades, CardRank.Nine),    // 9
                new BlackjackCard(CardSuit.Hearts, CardRank.Ten)      // 10
            };
            foreach (var card in dealerCards.Cast<BlackjackCard>())
            {
                card.FlipFaceUp(); // Ensure dealer's cards are face up for end-game evaluation
            }
            dealer.Setup(d => d.Hand).Returns(dealerCards);

            var stateMock = new Mock<IGameState>();
            stateMock.Setup(s => s.Players).Returns(new[] { player1.Object, player2.Object, player3.Object, dealer.Object });

            var contextMock = new Mock<IGameContext>();
            contextMock.Setup(c => c.State).Returns(stateMock.Object);

            // Act
            var winners = await rules.DetermineWinners(contextMock.Object);

            // Assert
            Assert.Equal(2, winners.Count); // Both 5-Card Charlie players win
            Assert.Contains(player1.Object, winners); // Wins with 5-Card Charlie (17)
            Assert.DoesNotContain(player2.Object, winners); // Loses with 19 against dealer's 19
            Assert.Contains(player3.Object, winners); // Wins with 5-Card Charlie (16)
            Assert.DoesNotContain(dealer.Object, winners); // Dealer loses to 5-Card Charlies
        }
    }
} 