using Godot;

public partial class QueueItem : Control
{
    private HexCoords _coords;
    private bool _isFirst;

    private int _storedZIndex;


    public override void _Ready()
    {
        base._Ready();
        if (_isFirst) GetNode<NinePatchRect>("Highlight").Show();
    }

    public void Initialize(HexCoords coords, bool isFirst)
    {
        _storedZIndex = ZIndex;
        _coords = coords;
        var id = World.Instance.Entities.GetCharacterIdAtPosition(coords);
        GetNode<TileMapLayer>("TileMapLayer").SetCell(new Vector2I(0, 0), id, new Vector2I(0, 0));
        _isFirst = isFirst;
    }

    public void OnEnter()
    {
        World.Instance.Entities.SetMouseMarkerPosition(_coords);
    }

    public void OnExit()
    {
    }
}