using System.Linq;
using Godot;

public partial class TimeLine : Control
{
    private Panel _displayNameBox;
    private Label _displayNameLabel;

    private RoomStack _lastRoomStack;
    private int _offsetBottom = 237;

    private int _offsetLeft = 11;
    private int _offsetTop = 52;


    public override void _Ready()
    {
        _displayNameBox = GetNode<Panel>("DisplayNameBox");
        _displayNameLabel = _displayNameBox.GetNode<Label>("MarginContainer/VBoxContainer/DisplayName");
    }

    public void Update()
    {
        if (_lastRoomStack == null || World.Instance.GetCurrentRoomStack() != _lastRoomStack)
        {
            ClearItems();
            _lastRoomStack = World.Instance.GetCurrentRoomStack();
            var roomsInStack = _lastRoomStack.GetRooms();
            var newItemScene = GD.Load<PackedScene>("scenes/ui/TimeLineItem.tscn");
            var availableHeight = _offsetBottom - _offsetTop;
            foreach (var room in roomsInStack)
                if (newItemScene.Instantiate() is TimeLineItem newItemInstance)
                {
                    newItemInstance.Initialize(room);
                    var layerRange = _lastRoomStack.GetLayerRange();
                    var height = availableHeight * ((float)room.Layer / (layerRange.Y + 1 - layerRange.X + 1));
                    newItemInstance.Position = new Vector2(_offsetLeft, _offsetBottom - height);
                    AddChild(newItemInstance);
                }
        }

        UpdateDisplayName();
        foreach (var item in GetChildren().OfType<TimeLineItem>()) item.Update();
    }

    private void UpdateDisplayName()
    {
        _displayNameLabel.Text = World.Instance.CurrentRoom.DisplayName;
    }

    private void ClearItems()
    {
        if (GetChildCount() > 0)
        {
            var children = GetChildren().OfType<TimeLineItem>();
            foreach (var child in children)
            {
                RemoveChild(child);
                child.QueueFree();
            }
        }
    }

    private void OnCurrentNameChange()
    {
        _displayNameBox.Size =
            new Vector2(_displayNameBox.GetNode<MarginContainer>("MarginContainer").GetMinimumSize().X,
                _displayNameBox.Size.Y);
    }
}