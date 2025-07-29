using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// Coordinates of a hex in a cubic coordinate system
/// (See https://www.redblobgames.com/grids/hexagons/)
[GlobalClass]
public partial class HexCoords : Resource
{
    [Export] public int Q, R, S;

    public HexCoords()
    {
    }

    public HexCoords(int q, int r, int s)
    {
        Q = q;
        R = r;
        S = s;
    }

    /// Construct from axial coords
    public HexCoords(int q, int r)
    {
        Q = q;
        R = r;
        S = -q - r;
    }

    public void Init(int q, int r)
    {
        Q = q;
        R = r;
        S = -q - r;
    }

    public static HexCoords operator +(HexCoords a, HexCoords b)
    {
        return new HexCoords(a.Q + b.Q, a.R + b.R, a.S + b.S);
    }

    public static HexCoords operator -(HexCoords a, HexCoords b)
    {
        return new HexCoords(a.Q - b.Q, a.R - b.R, a.S - b.S);
    }

    public static HexCoords operator *(HexCoords a, int b)
    {
        return new HexCoords(a.Q * b, a.R * b, a.S * b);
    }

    public static bool operator ==(HexCoords a, HexCoords b)
    {
        return a.Q == b.Q && a.R == b.R && a.S == b.S;
    }

    public static bool operator !=(HexCoords a, HexCoords b)
    {
        return a.Q != b.Q || a.R != b.R || a.S != b.S;
    }

    public override bool Equals(object obj)
    {
        if (obj is not HexCoords other)
            return false;

        return this == other;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Q, R);
    }
}

/// <summary>
///     Odd Q is what Hexagon Tilemaps with Stacked vertical offset use
/// </summary>
/// <param name="Col"></param>
/// <param name="Row"></param>
public record struct OddQHexCoords(int Col, int Row)
{
    public OddQHexCoords(Vector2I vector2I) : this(vector2I.X, vector2I.Y)
    {
    }

    public static OddQHexCoords FromCubic(HexCoords coords)
    {
        return new OddQHexCoords(coords.Q, coords.R + (coords.Q - (coords.Q & 1)) / 2);
    }

    public HexCoords ToCubic()
    {
        return new HexCoords(Col, Row - (Col - (Col & 1)) / 2);
    }

    public Vector2I ToVector2I()
    {
        return new Vector2I(Col, Row);
    }
}


public static class HexUtil
{
    public static readonly HexCoords[] EdgeDirections =
    [
        new(+1, 0, -1), new(+1, -1, 0), new(0, -1, +1),
        new(-1, 0, +1), new(-1, +1, 0), new(0, +1, -1)
    ];

    public static readonly HexCoords[] TilesWithRangeTwoToPosition =
    [
        new(+2, -1, -1), new(+1, -2, +1), new(-1, -1, +2),
        new(-2, +1, +1), new(-1, +2, -1), new(+1, +1, -2),
        new(+2, -2, -0), new(+2, 0, -2), new(0, -2, +2),
        new(0, +2, -2), new(+2, 0, -2), new(-2, 0, +2)
    ];

    public static IEnumerable<HexCoords> NeighborCoords(this HexCoords center)
    {
        return EdgeDirections.Select(dir => center + dir);
    }

    public static IEnumerable<HexCoords> RangeTwoCoords(this HexCoords center)
    {
        return TilesWithRangeTwoToPosition.Select(dir => center + dir);
    }

    public static int DistanceTo(this HexCoords a, HexCoords b)
    {
        return Length(a - b);
    }

    public static int Length(this HexCoords hex)
    {
        return (Mathf.Abs(hex.Q) + Mathf.Abs(hex.R) + Mathf.Abs(hex.S)) / 2;
    }

    /// <summary>
    ///     Returns the new position of a point after rotating it around the origin.
    /// </summary>
    /// <param name="point">The point to rotate.</param>
    /// <param name="amount">
    ///     The amount of rotation. Negative numbers mean counterclockwise, positive numbers clockwise
    ///     rotation. Each step of rotation is 60 degrees.
    /// </param>
    /// <returns></returns>
    public static HexCoords RotatedBy(this HexCoords point, int amount)
    {
        switch (amount)
        {
            case > 0:
            {
                for (var i = 1; i <= amount; i++) point = new HexCoords(-point.R, -point.S, -point.Q);
                break;
            }
            case < 0:
            {
                for (var i = -1; i >= amount; i--) point = new HexCoords(-point.S, -point.Q, -point.R);
                break;
            }
        }

        return point;
    }

    /// <summary>
    ///     Returns the new position of a point after rotating it around the origin.
    /// </summary>
    /// <param name="point">The point to rotate.</param>
    /// <param name="amount">
    ///     The amount of rotation. Negative numbers mean counterclockwise, positive numbers clockwise
    ///     rotation. Each step of rotation is 60 degrees.
    /// </param>
    /// <param name="aroundPivot">The pivot point to rotate around.</param>
    /// <returns></returns>
    public static HexCoords RotatedBy(this HexCoords point, int amount, HexCoords aroundPivot)
    {
        var translated = point - aroundPivot;
        var rotated = RotatedBy(translated, amount);
        var translatedBack = rotated + aroundPivot;
        return translatedBack;
    }

    public static List<List<HexCoords>> FloodFill(HexCoords startTile, int depth,
        Func<HexCoords, IEnumerable<HexCoords>> reachableTiles)
    {
        var visited = new HashSet<HexCoords>();
        var tilesAtDepth = new List<List<HexCoords>> { new() { startTile } };
        for (var currentDepth = 1; currentDepth < depth; currentDepth++)
        {
            tilesAtDepth.Add([]);
            foreach (var tile in tilesAtDepth[currentDepth - 1])
            foreach (var neighbor in reachableTiles(tile))
            {
                if (!visited.Add(neighbor)) continue;
                tilesAtDepth[currentDepth].Add(neighbor);
            }
        }

        return tilesAtDepth;
    }

    public static List<HexCoords> FindPath(HexCoords startTile, HexCoords endTile,
        IEnumerable<HexCoords> eligibleTiles)
    {
        var frontier = new Queue<HexCoords>();
        frontier.Enqueue(startTile);
        var cameFrom = new Dictionary<HexCoords, HexCoords>();
        cameFrom[startTile] = null;

        while (frontier.Count != 0)
        {
            var current = frontier.Dequeue();
            foreach (var next in current.NeighborCoords())
                if (eligibleTiles.Contains(next) && !cameFrom.ContainsKey(next))
                {
                    frontier.Enqueue(next);
                    cameFrom[next] = current;
                }
        }

        var currentPosition = endTile;
        var path = new List<HexCoords>();
        while (currentPosition != startTile)
        {
            path.Add(currentPosition);
            currentPosition = cameFrom[currentPosition];
        }

        path.Add(startTile);
        path.Reverse();
        return path;
    }
}