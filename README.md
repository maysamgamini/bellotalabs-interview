# Card Games Framework

A .NET framework for implementing various card games using Clean Architecture and Domain-Driven Design principles. The framework currently supports multiple card games including Blackjack, Poker, Uno, Baccarat, and High Stakes variants.

## Architecture Overview

The solution follows Clean Architecture with Domain-Driven Design (DDD) principles, organized into distinct layers:

### Core Domain Layer (BellotaLabInterview.Core)

The heart of the system, containing all domain models and business logic:

```
BellotaLabInterview.Core/
├── Domain/
│   ├── Cards/           # Card-related domain objects and interfaces
│   ├── Game/            # Core game abstractions and rules
│   ├── Players/         # Player management and state
│   └── Snapshots/      # Game state persistence
└── Services/           # Domain services
```

### Game Implementations

Each game is implemented in its own project, extending the core domain:

```
BellotaLabInterview.Blackjack/
BellotaLabInterview.Poker/
BellotaLabInterview.Uno/
BellotaLabInterview.Baccarat/
BellotaLabInterview.HighStakes/
```

### Infrastructure Layer

Handles cross-cutting concerns and dependency injection:

```
BellotaLabInterview.Infrastructure/
└── DependencyInjection/  # Service registration and configuration
```

### Presentation Layer

```
BellotaLabInterview.UI.Console/  # Console-based user interface
```

## Core Domain Model

### Game Components

```mermaid
classDiagram
    class IGame {
        +GameType Type
        +IGameContext Context
        +Initialize(players)
        +StartGame()
        +EndGame()
        +IsGameOver()
        +GetGameResult()
    }

    class GameBase {
        #IGameContext Context
        #ICardFactory CardFactory
        #IHandEvaluator HandEvaluator
        #IDeck Deck
        +Initialize(players)*
        #ValidatePlayers(players)*
        #InitializeGameState(players)*
        #DealInitialCards()*
    }

    class IGameRules {
        +int MinPlayers
        +int MaxPlayers
        +int InitialHandSize
        +CanPlayerAct(player, context)
        +IsValidMove(player, card, context)
        +IsGameOver(context)
        +CalculateScore(player, context)
    }

    IGame <|.. GameBase
    GameBase --> IGameRules
```

### Card System

```mermaid
classDiagram
    class ICard {
        +string DisplayName
        +GetValue(context)
    }

    class StandardCard {
        +CardSuit Suit
        +CardRank Rank
        +bool IsFaceUp
        +FlipFaceUp()
        +FlipFaceDown()
        #GetSuitSymbol(suit)
        #GetRankSymbol(rank)
    }

    class IDeck {
        +IReadOnlyList~ICard~ Cards
        +int RemainingCards
        +DrawCard()
        +DrawCards(count)
        +Shuffle()
        +Reset()
    }

    ICard <|.. StandardCard
    IDeck --> ICard
```

### State Management

```mermaid
classDiagram
    class IGameContext {
        +IGameState State
        +ICardPlayValidator CardPlayValidator
        +ICardEffectHandler EffectHandler
        +IsValidMove(card)
        +CanPlayerAct(player)
        +AdvanceNextPlayer()
        +DealCards(count)
        +PlayCard(card)
    }

    class GameSnapshot {
        +GameType GameType
        +GameState State
        +List~PlayerSnapshot~ Players
        +DeckSnapshot Deck
        +List~ICard~ DiscardPile
        +Dictionary~string,object~ GameSpecificData
    }

    class Points {
        +int Value
        +operator +(Points, Points)
        +operator -(Points, Points)
    }

    class BetAmount {
        +int Value
        +operator +(BetAmount, BetAmount)
    }

    IGameContext --> GameSnapshot
```

## Design Patterns Used

1. **Template Method Pattern**
   - `GameBase` provides the game flow skeleton with abstract methods:
     - `ValidatePlayers(players)`
     - `InitializeGameState(players)`
     - `DealInitialCards()`
     - `StartFirstTurn()`
     - `CleanupGame()`
   - Concrete games like `BlackjackGame` implement specific behaviors

2. **Strategy Pattern**
   - `IHandEvaluator` for different card evaluation rules
     - `BlackjackHandEvaluator` handles Ace values (1 or 11)
     - Each game implements its own hand evaluation logic
   - `IGameRules` for game-specific rules
     - Defines player constraints (min/max players)
     - Controls turn validation and move validation
     - Handles game state transitions
     - Implements scoring and winner determination

3. **Value Objects**
   - `Points` for immutable point values
     - Ensures non-negative values
     - Provides arithmetic operations
   - `BetAmount` for betting operations
     - Ensures positive bet values
     - Supports comparison operations
   - `GameSnapshot` for state persistence
     - Captures complete game state
     - Supports save/restore functionality

4. **Factory Pattern**
   - `ICardFactory` for creating game-specific cards
   - Each game implements its own card factory:
     - `BlackjackCardFactory` for standard 52-card deck
     - `UnoCardFactory` for Uno-specific cards

5. **Observer Pattern** (via Event Handlers)
   - `ICardEffectHandler` for handling card effects
   - `ICardPlayValidator` for validating card plays

## Game State Machine

The framework implements a state machine for game flow:

```mermaid
stateDiagram-v2
    Setup --> Dealing: Initialize
    Dealing --> Playing: StartGame
    Playing --> Scoring: IsGameOver
    Scoring --> GameOver: EndGame
```

## Card Representation

The framework provides a flexible card system:

- **Standard Cards**
  - Unicode symbols for suits (♥, ♦, ♣, ♠)
  - Fallback ASCII symbols (<3, <>, (), ^)
  - Face up/down state management
  - Rank values (Ace=1/11, Face cards=10)

## Dependency Injection

Services are registered in the Infrastructure layer:

```csharp
public static IServiceCollection AddBellotaLabServices(this IServiceCollection services)
{
    return services
        .AddCoreServices()
        .AddUnoServices()
        .AddBlackjackServices()
        .AddHighStakesServices();
}
```

Game-specific registrations:

```csharp
private static IServiceCollection AddBlackjackServices(this IServiceCollection services)
{
    services.AddScoped<ICardFactory, BlackjackCardFactory>();
    services.AddScoped<IHandEvaluator, BlackjackHandEvaluator>();
    services.AddScoped<IDeck, BlackjackDeck>();
    services.AddScoped<IGameRules, BlackjackGameRules>();
    services.AddScoped<IGame, BlackjackGame>();
    return services;
}
```

## Poker Game Structure

```mermaid
classDiagram
    class Game {
        -Deck deck
        -List~Player~ players
        -Dealer dealer
        -Table table
        -Pot pot
        -int currentPlayerIndex
        +Game(List~string~ playerNames)
        +void Start()
        +void DealHoleCards()
        +void BettingRound()
        +void RevealFlop()
        +void RevealTurn()
        +void RevealRiver()
        +void Showdown()
        +Player DetermineWinner()
    }

    class Player {
        -string Name
        -Hand hand
        -int Chips
        +Player(string name, int startingChips)
        +void PlaceBet(int amount)
        +void Fold()
        +void Call(int amount)
        +void Raise(int amount)
        +void ReceiveCard(Card card)
        +void ResetHand()
    }

    class Hand {
        -List~Card~ cards
        +void AddCard(Card card)
        +int Evaluate()
        +void Clear()
    }

    class Dealer {
        +void DealHoleCards(Game game)
        +void DealFlop(Game game)
        +void DealTurn(Game game)
        +void DealRiver(Game game)
    }

    class Table {
        -List~Card~ communityCards
        +void AddCommunityCard(Card card)
        +List~Card~ GetCommunityCards()
    }

    class Card {
        +Suit Suit
        +Rank Rank
        +string ToString()
    }

    class Pot {
        -int amount
        +void AddToPot(int amount)
        +int GetAmount()
        +void DistributeWinnings(Player winner)
    }

    Game --> Deck : has
    Game --> Player : has
    Game --> Dealer : has
    Game --> Table : uses
    Game --> Pot : uses
    Player --> Hand : has
    Hand --> Card : contains
    Deck --> Card : contains
    Table --> Card : contains
    Dealer ..> Deck : deals from
    Dealer ..> Player : deals to

    class Suit {
        <<enumeration>>
        Clubs
        Diamonds
        Hearts
        Spades
    }

    class Rank {
        <<enumeration>>
        Two
        Three
        Four
        Five
        Six
        Seven
        Eight
        Nine
        Ten
        Jack
        Queen
        King
        Ace
    }

    Card --> Suit : has
    Card --> Rank : has
```

## Blackjack Game Structure

```mermaid
classDiagram
    class Game {
        -Deck deck
        -List~Player~ players
        -Dealer dealer
        +void Start()
        +void DealInitial()
        +void PlayerTurns()
        +void DealerTurn()
        +void DetermineWinners()
    }

    class Player {
        -string Name
        -Hand hand
        +Player(string name)
        +void Hit(Deck deck)
        +void Stand()
    }

    class Hand {
        -List~Card~ cards
        +int CalculateValue()
        +void AddCard(Card card)
        +bool IsBusted()
        +bool IsBlackjack()
    }

    class Deck {
        -List~Card~ cards
        +Deck()
        +void Shuffle()
        +Card Draw()
        +int Count()
    }

    class Card {
        +Suit Suit
        +Rank Rank
        +int GetValue()
        +string ToString()
    }

    class Dealer {
        +void Play(Deck deck)
    }

    class Suit {
        <<enumeration>>
        Clubs
        Diamonds
        Hearts
        Spades
    }

    class Rank {
        <<enumeration>>
        Two
        Three
        Four
        Five
        Six
        Seven
        Eight
        Nine
        Ten
        Jack
        Queen
        King
        Ace
    }

    Game --> Deck : uses
    Game --> Player : has
    Game --> Dealer : has
    Player --> Hand : has
    Hand --> Card : contains
    Deck --> Card : contains
    Card --> Suit : has
    Card --> Rank : has
```

## Hand Evaluation System

```mermaid
classDiagram
    class IHandEvaluator {
        <<interface>>
        +EvaluateHand(List~Card~ cards, IGameContext context)* HandRank
        +IsValidHand(List~Card~ cards, IGameContext context)* bool
        +GetPlayableCards(List~Card~ cards, IGameContext context)* List~Card~
    }

    class HandEvaluatorBase {
        <<abstract>>
        +EvaluateHand(List~Card~ cards, IGameContext context)* HandRank
        +IsValidHand(List~Card~ cards, IGameContext context) bool
        +GetPlayableCards(List~Card~ cards, IGameContext context) List~Card~
    }

    class BlackjackHandEvaluator {
        +EvaluateHand(List~Card~ cards, IGameContext context) HandRank
        +IsValidHand(List~Card~ cards, IGameContext context) bool
        -AdjustForAces(int total, int aceCount) int
    }

    class PokerHandEvaluator {
        +EvaluateHand(List~Card~ cards, IGameContext context) HandRank
        +IsValidHand(List~Card~ cards, IGameContext context) bool
        -EvaluatePokerHand(List~Card~ cards) HandRank
    }

    class HandRank {
        +int Value
        +string Description
        +Dictionary~string,object~ AdditionalData
    }

    IHandEvaluator <|.. HandEvaluatorBase
    HandEvaluatorBase <|-- BlackjackHandEvaluator
    HandEvaluatorBase <|-- PokerHandEvaluator
    BlackjackHandEvaluator ..> HandRank : creates
    PokerHandEvaluator ..> HandRank : creates
```

## Game Base Structure

```mermaid
classDiagram
    class GameBase {
        <<abstract>>
        #IGameContext Context
        #ICardFactory CardFactory
        #IHandEvaluator HandEvaluator
        #IDeck Deck
        +Initialize(players)*
        #ValidatePlayers(players)*
        #InitializeGameState(players)*
        #DealInitialCards()*
    }

    class IHandEvaluator {
        <<interface>>
        +EvaluateHand(cards, context)* HandRank
        +IsValidHand(cards, context)* bool
        +GetPlayableCards(cards, context)* List~Card~
    }

    GameBase --> IHandEvaluator : uses
```

## Requirements

- .NET 9.0
- Visual Studio 2022 or later

## Getting Started

1. Clone the repository
2. Open the solution in Visual Studio
3. Build the solution
4. Run the Console UI project

## License

This project is licensed under the MIT License