using Godot;

public partial class HealthUi : Control
{
    private ProgressBar _healthBar;

    public override void _Ready()
    {
        base._Ready();
        _healthBar = GetNode<ProgressBar>("HealthBar");
    }

    public void Update()
    {
        var player = World.Instance.Entities.Player;
        _healthBar.MaxValue = player.MaxHealth;
        _healthBar.Value = player.Health;
    }
}