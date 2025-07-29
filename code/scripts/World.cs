using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class World : Node2D
{
    private Room _currentRoom;

    private Room _peekedRoom;

    [Export] public bool IsEndlessWorld = false;

    //Code to track current Room etc.
    [Export] private NodePath _startRoom;
    public UI Ui {get; private set;}
    public static World Instance { get; private set; }

    public Entities Entities { get; private set; }

    public Room CurrentRoom
    {
        get => _currentRoom;
        set
        {
            _currentRoom = value;
            OnUpdateTimeLine();
            Entities.Position = GetCurrentRoomStack().Position;
            GetCurrentRoomStack().SetActiveByLayer(_currentRoom.Layer);
            Entities.SetupCamera();
        }
    }

    public Room PeekedRoom
    {
        get => _peekedRoom;
        set
        {
            _peekedRoom = value;
            if (_peekedRoom != null)
                GetCurrentRoomStack().SetPreviewByLayer(_peekedRoom.Layer);
            else
                GetCurrentRoomStack().SetActiveByLayer(_currentRoom.Layer);
            OnUpdateTimeLine();
        }
    }

    public override void _Ready()
    {
        Instance = this;
        Entities = GetNode<Entities>("Entities");
        Ui = GetNode<UI>("UI");
        CurrentRoom = GetNode<Room>(_startRoom);
        Entities.StartGameLoop();
        Entities.SetupCamera();
        Ui.Initialize();
    }

    private void OnUpdateTimeLine()
    {
        Ui.UpdateTimeLine();
    }

    public RoomStack GetCurrentRoomStack()
    {
        return CurrentRoom.GetRoomStack();
    }

    private void OnQueueChanged()
    {
        Ui.UpdateQueue();
    }

    public void OnHealthChanged()
    {
        Ui.UpdateHealthBar();
    }

    public void MakeDoors()
    {
        if(CurrentRoom != null) CurrentRoom.PlaceDoors();
    }

    public void DestroyDoors()
    {
        if(CurrentRoom != null) CurrentRoom.DestroyDoors();
    }

    public HexCoords CheckForDoorsUsage(HexCoords playerPos)
    {
        var newPos = CurrentRoom.CheckForDoorUsage(playerPos);
        if (newPos != playerPos)
        {
            Ui.OnTutorialEnd();
        }
        return newPos;
    }

    public void UpdateScore(int score)
    {
        if (IsEndlessWorld)
        {
            Ui.UpdateScore(score);
        }
    }

    public IEnumerable<Room> GetRoomsExcept(Room room)
    {
        return GetCurrentRoomStack().GetChildren().OfType<Room>().Where(r => r != room);
    }
    
    [Signal]
    public delegate void DoorEnteredEventHandler();
    
}