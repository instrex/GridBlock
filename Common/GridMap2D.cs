using Microsoft.Xna.Framework;
using System;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ModLoader;

namespace GridBlock.Common;

public class GridMap2D<T>(int chunkSize, int worldBoundsX, int worldBoundsY) {
    readonly T[] _values = new T[worldBoundsX / chunkSize * worldBoundsY / chunkSize];

    /// <summary>
    /// Size of each chunk on this map.
    /// </summary>
    public int CellSize { get; } = chunkSize;

    /// <summary>
    /// Gets total number of values.
    /// </summary>
    public int Length => _values.Length;

    /// <summary>
    /// Size of the whole map (in chunk space).
    /// </summary>
    public Point Bounds { get; } = new(worldBoundsX / chunkSize, worldBoundsY / chunkSize);

    /// <summary>
    /// Transform a 2d index into 1d Id.
    /// </summary>
    public int ToChunkId(Point chunkCoord) => chunkCoord.X + chunkCoord.Y * Bounds.X;

    public Point ToChunkCoord(int id) => new(id % Bounds.X, id / Bounds.X);

    /// <summary>
    /// Attempts to get a chunk by tile coordinates.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetByTileCoord(Point tileCoord) => GetById(ToChunkId(new(tileCoord.X / CellSize, tileCoord.Y / CellSize)));

    /// <summary>
    /// Attempts to get a chunk by tile coordinates.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetByChunkCoord(Point chunkCoord) => GetById(ToChunkId(chunkCoord));

    /// <summary>
    /// Attempts to get a chunk by world coordinates.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetByWorldPos(Vector2 worldPos) => GetByTileCoord(worldPos.ToTileCoordinates());

    /// <summary>
    /// Attempts to get a chunk by internal id.
    /// </summary>
    public T GetById(int id) {
        if (!_values.IndexInRange(id)) {
            ModContent.GetInstance<GridBlock>().Logger.Warn($"Attempted to access GridBlock chunk out of bounds. (id: {id})");
            return default;
        }

        return _values[id];
    }

    public void Fill(Func<GridMap2D<T>, int, T> constructor) {
        for (var i = 0; i < _values.Length; i++) {
            _values[i] = constructor(this, i);
        }
    }
}
