using System.Collections.Generic;
using Godot;

public class TileLookUp
{
    
    public static readonly List<int> FloorIDs = new()
    {
        0,
        6,
        1,
        7,
        10
    };

    public static readonly List<int> WallIDs = new()
    {
        5,
        2,
        3,
        8,
        11
    };

    public static readonly List<int> PitIDs = new()
    {
        9,
        12,
        13
    };

    public static int GetWallTerrainByFloorId(int floorId)
    {
        if(floorId == 0) return 0;
        if (floorId == 1) return 2;
        if(floorId == 6) return 1;
        if(floorId == 7) return 3;
        if(floorId == 10) return 4;
        return -1;
    }
    
    public static int GetPitByFloorId(int floorId)
    {
        if(floorId == 0) return 9;
        if(floorId == 1) return 9;
        if(floorId == 6) return 9;
        if(floorId == 7) return 12;
        if(floorId == 10) return 13;
        return -1;
    }
    
    
    public static readonly int DoorID = 4;

    public static readonly int PlayerId = 0;
    public static readonly int GenericEnemyId = 1;
    public static readonly int MovementOptionId = 1;
    public static readonly int MouseMarkerId = 2;

    public static readonly Dictionary<HexCoords, CustomAnimation> MovementAnims = new()
    {
        { new HexCoords(0, -1), new CustomAnimation(new Vector2I(0, 3), 4, new Vector2I(3, 4)) },
        { new HexCoords(1, -1), new CustomAnimation(new Vector2I(0, 7), 4, new Vector2I(5, 4)) },
        { new HexCoords(1, 0), new CustomAnimation(new Vector2I(0, 11), 4, new Vector2I(5, 4)) },
        { new HexCoords(0, 1), new CustomAnimation(new Vector2I(12, 3), 4, new Vector2I(3, 4)) },
        { new HexCoords(-1, 1), new CustomAnimation(new Vector2I(0, 15), 4, new Vector2I(5, 4)) },
        { new HexCoords(-1, 0), new CustomAnimation(new Vector2I(0, 19), 4, new Vector2I(5, 4)) }
    };

    public static readonly Dictionary<string, CustomAnimation> AttackAnims = new()
    {
        { "left", new CustomAnimation(new Vector2I(0, 23), 4, new Vector2I(3, 3), 0.05f) },
        { "right", new CustomAnimation(new Vector2I(12, 23), 4, new Vector2I(3, 3), 0.05f) }
    };

    public static CustomAnimation SpawnAnim = new(new Vector2I(0, 26), 3, new Vector2I(3, 3), 0.2f);
    public static CustomAnimation DamageAnim = new(new(0, 29), 3, new Vector2I(3, 3), 0.075f);

    public static readonly int DeathEffectID = 5;
    public static CustomAnimation DeathAnim = new(new(0, 0), 3, new Vector2I(3, 3), 0.075f);

    public record struct CustomAnimation(
        Vector2I AtlasStartPosition,
        int AnimationLength,
        Vector2I AnimationSize,
        float FrameDuration = 0.1f)
    {
    }
}