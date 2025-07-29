using Godot;

public partial class TimeLineItem : Control
{
    private TextureRect _eye;
    private TextureRect _marker;
    private Room _room;

    public override void _Ready()
    {
        _eye = GetNode<TextureRect>("Eye");
        _marker = GetNode<TextureRect>("Marker");
    }

    public void Initialize(Room room)
    {
        _room = room;
    }

    public void Update()
    {
        if (_room == World.Instance.CurrentRoom)
            SetStatus(TimeLineItemStatus.Active);
        else if (_room == World.Instance.PeekedRoom)
            SetStatus(TimeLineItemStatus.Peeked);
        else
            SetStatus(TimeLineItemStatus.None);
    }

    private void SetStatus(TimeLineItemStatus status)
    {
        switch (status)
        {
            case TimeLineItemStatus.None:
                _eye.Hide();
                _marker.Hide();
                break;
            case TimeLineItemStatus.Active:
                _eye.Hide();
                _marker.Show();
                break;
            case TimeLineItemStatus.Peeked:
                _eye.Show();
                _marker.Hide();
                break;
        }
    }

    private enum TimeLineItemStatus
    {
        None,
        Peeked,
        Active
    }
}