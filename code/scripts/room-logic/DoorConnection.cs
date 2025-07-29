using Godot;

[GlobalClass]
public partial class DoorConnection : Resource
{
    [Export] public HexCoords endPosition;

    [Export] public NodePath neighborRoom;

    [Export] public HexCoords startPosition;
}