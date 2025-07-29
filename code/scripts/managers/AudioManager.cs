using Godot;

public partial class AudioManager : Node
{
    public static AudioManager Instance;
    private AudioStreamPlayer _normalAttackPlayer;
    private AudioStreamPlayer _playerHurtPlayer;

    private AudioStreamPlayer _walkingPlayer;
    
    
    private AudioStreamPlayer _layerDownPlayer;
    private AudioStreamPlayer _layerUpPlayer;
    private AudioStreamPlayer _spawnPlayer;

    
    
    private AudioStreamPlayer _bgmPlayer;

    public override void _Ready()
    {
        base._Ready();
        Instance = this;


        _walkingPlayer = new AudioStreamPlayer();
        AddChild(_walkingPlayer);
        _walkingPlayer.Bus = "Sounds";
        _walkingPlayer.VolumeDb = -6;
        _walkingPlayer.Stream = GD.Load<AudioStream>("assets/audio/Step.wav");

        _playerHurtPlayer = new AudioStreamPlayer();
        AddChild(_playerHurtPlayer);
        _playerHurtPlayer.Bus = "Sounds";
        _playerHurtPlayer.VolumeDb = -6;
        _playerHurtPlayer.Stream = GD.Load<AudioStream>("assets/audio/PlayerHurt.wav");

        _normalAttackPlayer = new AudioStreamPlayer();
        AddChild(_normalAttackPlayer);
        _normalAttackPlayer.Bus = "Sounds";
        _normalAttackPlayer.VolumeDb = -6;
        _normalAttackPlayer.Stream = GD.Load<AudioStream>("assets/audio/BasicAttack.wav");
        
        _layerDownPlayer = new AudioStreamPlayer();
        AddChild(_layerDownPlayer);
        _layerDownPlayer.Bus = "Sounds";
        _layerDownPlayer.VolumeDb = -6;
        _layerDownPlayer.Stream = GD.Load<AudioStream>("assets/audio/LayerDown.wav");
        
        _layerUpPlayer = new AudioStreamPlayer();
        AddChild(_layerUpPlayer);
        _layerUpPlayer.Bus = "Sounds";
        _layerUpPlayer.VolumeDb = -6;
        _layerUpPlayer.Stream = GD.Load<AudioStream>("assets/audio/LayerUp.wav");
        
        _spawnPlayer = new AudioStreamPlayer();
        AddChild(_spawnPlayer);
        _spawnPlayer.Bus = "Sounds";
        _spawnPlayer.VolumeDb = -6;
        _spawnPlayer.Stream = GD.Load<AudioStream>("assets/audio/EnemySpawn.wav");
        
        _bgmPlayer = new AudioStreamPlayer();
        _bgmPlayer.Bus = "Music";
        _bgmPlayer.VolumeDb = -6;
        _bgmPlayer.Stream = GD.Load<AudioStream>("assets/audio/BGM.wav");
        _bgmPlayer.Autoplay = true;
        AddChild(_bgmPlayer);
    }

    public void PlayWalkSound()
    {
        if (_walkingPlayer.Playing)
            _walkingPlayer.Stop();
        _walkingPlayer.Play();
    }

    public void PlayPlayerHurtSound()
    {
        if (_playerHurtPlayer.Playing)
            _playerHurtPlayer.Stop();
        _playerHurtPlayer.Play();
    }

    public void PlayNormalAttackSound()
    {
        if (_normalAttackPlayer.Playing)
            _normalAttackPlayer.Stop();
        _normalAttackPlayer.Play();
    }
    
    public void PlayLayerUpSound()
    {
        if (_layerUpPlayer.Playing)
            _layerUpPlayer.Stop();
        _layerUpPlayer.Play();
    }
    public void PlayLayerDownSound()
    {
        if (_layerDownPlayer.Playing)
            _layerDownPlayer.Stop();
        _layerDownPlayer.Play();
    }
    public void PlaySpawnSound()
    {
        if (_spawnPlayer.Playing)
            _spawnPlayer.Stop();
        _spawnPlayer.Play();
    }
    
}