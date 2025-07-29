using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Room : Node2D
{
    [Export] private DoorConnection _doorConnection;

    [Export] public int Layer { get; private set; }
    [Export] public string DisplayName { get; private set; } = "Some Time";
    [Export] public float TargetScale { get; private set; } = 1.0f;


    private TileMapLayer _foreground;
    private TileMapLayer _midground;
    public override void _Ready()
    {
        base._Ready();
        _foreground = GetNode<TileMapLayer>("Fore");
        _midground = GetNode<TileMapLayer>("Mid");
    }

    public bool IsTileWalkable(HexCoords coords)
    {
        var convertedCoords = OddQHexCoords.FromCubic(coords).ToVector2I();
        if (!TileLookUp.FloorIDs.Contains(GetNode<TileMapLayer>("Mid").GetCellSourceId(convertedCoords))) return false;
        if (TileLookUp.PitIDs.Contains(GetNode<TileMapLayer>("Back").GetCellSourceId(convertedCoords))) return false;
        if (TileLookUp.WallIDs.Contains(GetNode<TileMapLayer>("Fore").GetCellSourceId(convertedCoords))) return false;
        return true;
    }

    public void HideRoom()
    {
        Visible = false;
    }

    public void PreviewRoom()
    {
        Material = GD.Load<Material>("assets/materials/hologram_material.tres");
        Visible = true;
    }

    public void ShowRoom()
    {
        Material = GD.Load<Material>("assets/materials/default_material.tres");
        Visible = true;
    }

    public RoomStack GetRoomStack()
    {
        return GetParent() as RoomStack;
    }

    public void PlaceDoors()
    {
        if (_doorConnection != null) _foreground?.SetCell(OddQHexCoords.FromCubic(_doorConnection.startPosition).ToVector2I(), TileLookUp.DoorID, new(0, 0));
    }
    
    public void DestroyDoors()
    {
        if (_doorConnection != null) _foreground?.SetCell(OddQHexCoords.FromCubic(_doorConnection.startPosition).ToVector2I());
    }

    public HexCoords CheckForDoorUsage(HexCoords playerPos)
    {
        if (_doorConnection != null && _doorConnection.startPosition == playerPos)
        {
            return _doorConnection.endPosition;
        }
        return playerPos;
    }
    
    public Room GetNeighborRoom()
    {
        if (_doorConnection != null) return GetNode<Room>(_doorConnection.neighborRoom);
        return null;
    }

    private void ClearObstacles(IEnumerable<HexCoords> area)
    {
        foreach (var coords in area)
        {
            _foreground.SetCell(OddQHexCoords.FromCubic(coords).ToVector2I());
        }
    }
    public void PlaceRandomObstacles(int amount, IEnumerable<HexCoords> area)
    {
        ClearObstacles(area);
        
        var targets = area.Shuffled().Take(amount);

        foreach (var target in targets)
        {
            var targetOddQ = OddQHexCoords.FromCubic(target).ToVector2I();
            _foreground.SetCellsTerrainConnect(new(){targetOddQ}, 0, TileLookUp.GetWallTerrainByFloorId(_midground.GetCellSourceId(targetOddQ)), true);
        }
    }
    
}