using Godot;

public partial class DialogueHandler : PanelContainer
{
    [Signal]
    public delegate void OnClickEventHandler();

    private Label _name;

    private TileMapLayer _portrait;
    private RichTextLabel _text;

    public override void _Ready()
    {
        base._Ready();
        _portrait = GetNode<TileMapLayer>("Horizontal/DialoguePortrait/TileMapLayer");
        _name = GetNode<Label>("Horizontal/Text/Name");
        _text = GetNode<RichTextLabel>("Horizontal/Text/Text");
    }

    public void DisplayDialogue(DialogueCard dialogue)
    {
        Show();
        _portrait.SetCell(new Vector2I(0, 0), dialogue.SpeakerImageId, Vector2I.Zero);
        _name.Text = dialogue.SpeakerName;
        _text.Text = dialogue.DialogueText;
    }

    public void OnSizeChanged()
    {
        Size = new Vector2(Size.X, GetMinimumSize().Y);
    }

    private void OnClickCallback()
    {
        EmitSignal("OnClick");
    }
}