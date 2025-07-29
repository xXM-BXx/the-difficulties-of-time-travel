using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class RoomStack : Node2D
{
    public Room GetNeighboringLayer(Room currentRoom, int layerChange)
    {
        if (layerChange is -1 or 1)
        {
            var currentLayer = currentRoom.Layer;
            for (var i = 1; i <= GetChildCount(); i++)
                foreach (var room in GetChildren().OfType<Room>().ToList())
                    if (room.Layer == currentLayer + layerChange * i)
                        return room;
        }
        else
        {
            GD.PrintErr("layerChange must be 1 or -1");
        }
        return null;
    }

    public bool HasNeighboringLayer(Room currentRoom, int layerChange)
    {
        return GetNeighboringLayer(currentRoom, layerChange) != null;
    }

    public void SetPreviewByLayer(int layer)
    {
        var allRooms = GetChildren().OfType<Room>().ToList();

        var targetRoom = FindFirstByLayer(layer);

        if (targetRoom == null)
        {
            GD.PrintErr("Could not Find Room");
            return;
        }

        foreach (var room in allRooms) room.HideRoom();
        targetRoom.PreviewRoom();
    }

    public void SetActiveByLayer(int layer)
    {
        var allRooms = GetChildren().OfType<Room>().ToList();
        var targetRoom = FindFirstByLayer(layer);
        if (targetRoom == null)
        {
            GD.PrintErr("Could not Find Room");
            return;
        }

        foreach (var room in allRooms) room.HideRoom();
        targetRoom.ShowRoom();
    }

    public Room FindFirstByLayer(int layer)
    {
        var children = GetChildren().OfType<Room>().ToList();
        return children.FirstOrDefault(r => r.Layer == layer);
    }

    public IEnumerable<Room> GetRooms()
    {
        return GetChildren().OfType<Room>();
    }

    public Vector2I GetLayerRange()
    {
        var result = new Vector2I(100, -100);
        foreach (var room in GetRooms())
        {
            if (room.Layer < result.X) result.X = room.Layer;

            if (room.Layer > result.Y) result.Y = room.Layer;
        }

        return result;
    }
}