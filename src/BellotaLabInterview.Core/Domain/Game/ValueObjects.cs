using System;
using System.Collections.Generic;

namespace BellotaLabInterview.Core.Domain.Game;

public readonly record struct Points
{
    public int Value { get; init; }

    public Points(int value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Points cannot be negative");
        Value = value;
    }

    public static Points operator +(Points left, Points right) => new(left.Value + right.Value);
    public static Points operator -(Points left, Points right) => new(Math.Max(0, left.Value - right.Value));
    public static bool operator >(Points left, Points right) => left.Value > right.Value;
    public static bool operator <(Points left, Points right) => left.Value < right.Value;
    public static bool operator >=(Points left, Points right) => left.Value >= right.Value;
    public static bool operator <=(Points left, Points right) => left.Value <= right.Value;

    public static implicit operator int(Points points) => points.Value;
    public static explicit operator Points(int value) => new(value);
}

public readonly record struct BetAmount
{
    public int Value { get; init; }

    public BetAmount(int value)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Bet amount must be positive");
        Value = value;
    }

    public static BetAmount operator +(BetAmount left, BetAmount right) => new(left.Value + right.Value);
    public static bool operator >(BetAmount left, BetAmount right) => left.Value > right.Value;
    public static bool operator <(BetAmount left, BetAmount right) => left.Value < right.Value;
    public static bool operator >=(BetAmount left, BetAmount right) => left.Value >= right.Value;
    public static bool operator <=(BetAmount left, BetAmount right) => left.Value <= right.Value;

    public static implicit operator int(BetAmount bet) => bet.Value;
    public static explicit operator BetAmount(int value) => new(value);
}

public readonly record struct GameOptions
{
    public int MinPlayers { get; init; }
    public int MaxPlayers { get; init; }
    public Points InitialPoints { get; init; }
    public BetAmount MinBet { get; init; }
    public BetAmount MaxBet { get; init; }
    public IDictionary<string, object> GameSpecificOptions { get; init; }

    public GameOptions(
        int minPlayers,
        int maxPlayers,
        Points initialPoints,
        BetAmount minBet,
        BetAmount maxBet,
        IDictionary<string, object>? gameSpecificOptions = null)
    {
        if (minPlayers <= 0)
            throw new ArgumentOutOfRangeException(nameof(minPlayers), "Minimum players must be positive");
        if (maxPlayers < minPlayers)
            throw new ArgumentOutOfRangeException(nameof(maxPlayers), "Maximum players must be greater than or equal to minimum players");
        if (maxBet < minBet)
            throw new ArgumentOutOfRangeException(nameof(maxBet), "Maximum bet must be greater than or equal to minimum bet");

        MinPlayers = minPlayers;
        MaxPlayers = maxPlayers;
        InitialPoints = initialPoints;
        MinBet = minBet;
        MaxBet = maxBet;
        GameSpecificOptions = gameSpecificOptions ?? new Dictionary<string, object>();
    }
} 