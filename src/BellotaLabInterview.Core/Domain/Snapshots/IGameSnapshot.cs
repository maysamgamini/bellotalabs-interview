using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BellotaLabInterview.Core.Domain.Cards;
using BellotaLabInterview.Core.Domain.Game;
using BellotaLabInterview.Core.Domain.Players;

namespace BellotaLabInterview.Core.Domain.Snapshots;

public record GameSnapshot
{
    public required GameType GameType { get; init; }
    public required GameState State { get; init; }
    public required int CurrentPlayerIndex { get; init; }
    public required bool IsPlayDirectionClockwise { get; init; }
    public required List<PlayerSnapshot> Players { get; init; }
    public required DeckSnapshot Deck { get; init; }
    public required List<ICard> DiscardPile { get; init; }
    public required Dictionary<string, object> GameSpecificData { get; init; }
}

public record PlayerSnapshot
{
    public required string Name { get; init; }
    public required PlayerState State { get; init; }
    public required double Points { get; init; }
    public required List<ICard> Hand { get; init; }
    public required List<ICard> PlayedCards { get; init; }
}

public record DeckSnapshot
{
    public required List<ICard> Cards { get; init; }
    public required int CurrentPosition { get; init; }
}

public interface IGameSnapshotManager
{
    GameSnapshot CreateSnapshot(IGameContext context);
    void RestoreSnapshot(GameSnapshot snapshot, IGameContext context);
    void SaveSnapshot(GameSnapshot snapshot);
    GameSnapshot LoadSnapshot(Guid snapshotId);
}

public interface IGameSnapshotStorage
{
    Task SaveSnapshot(GameSnapshot snapshot);
    Task<GameSnapshot> LoadSnapshot(Guid snapshotId);
    Task<IEnumerable<GameSnapshot>> ListSnapshots(GameType gameType);
    Task DeleteSnapshot(Guid snapshotId);
} 