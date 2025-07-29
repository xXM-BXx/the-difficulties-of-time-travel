using Godot;

public partial class ButtonClickManager : Node
{
    private AudioStreamPlayer _buttonClickSoundPlayer;

    [Export] public AudioStream ButtonClickSound;

    public override void _Ready()
    {
        base._Ready();
        _buttonClickSoundPlayer = new AudioStreamPlayer();
        AddChild(_buttonClickSoundPlayer);
        _buttonClickSoundPlayer.Bus = "UI";
        _buttonClickSoundPlayer.VolumeDb = -6;

        ButtonClickSound = GD.Load<AudioStream>("assets/audio/ButtonPress.wav");
        _buttonClickSoundPlayer.Stream = ButtonClickSound;

        GetTree().NodeAdded += OnNodeAdded;
    }


    private void OnNodeAdded(Node newNode)
    {
        if (newNode is Button button)
            button.Pressed += OnButtonPressed;
    }

    private void OnButtonPressed()
    {
        if (_buttonClickSoundPlayer.Playing)
            _buttonClickSoundPlayer.Stop();
        _buttonClickSoundPlayer.Play();
    }
}