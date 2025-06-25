using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BellotaLabInterview.Core.Domain.Cards;

namespace BellotaLabInterview.Core.Domain.Players;

public interface IPlayer
{
    Guid Id { get; }
    string Name { get; }
    PlayerState State { get; }
    IReadOnlyList<ICard> Hand { get; }
    int Points { get; }
    
    Task AddCard(ICard card);
    Task RemoveCard(ICard card);
    Task UpdatePoints(int delta);
}

public enum PlayerState
{
    Waiting,
    Playing,
    Folded,
    Won,
    Lost
} 