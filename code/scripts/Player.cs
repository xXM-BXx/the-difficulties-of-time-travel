using Godot;

public partial class Player : Node
{
    public bool CanMove = false;
    public bool CanTimeTravel = true;
    public int MaxHealth { get; private set; } = 3;
    public int Health { get; private set; } = 3;

    public async void TakeDamage(int damage)
    {
        Health -= damage;
        if (damage > 0) AudioManager.Instance.PlayPlayerHurtSound();
        World.Instance.OnHealthChanged();
        await World.Instance.Entities.AnimatedDamage(World.Instance.Entities.GetPlayerPosition());
        if (Health <= 0) OnDeath();
    }

    public void HealFull()
    {
        Health = MaxHealth;
        World.Instance.OnHealthChanged();
    }
    public void Heal(int heal)
    {
        if(Health < MaxHealth) Health += heal;
    }
    private void OnDeath()
    {
        GD.Print("Player Died");
        World.Instance.Entities.OnPlayerDeath();
    }
}