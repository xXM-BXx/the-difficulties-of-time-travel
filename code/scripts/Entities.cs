using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class Entities : Node2D
{
    [Signal]
    public delegate void AllEnemiesDefeatedEventHandler();

    //Signals for Story Handler
    [Signal]
    public delegate void LayerChangedEventHandler();

    [Signal]
    public delegate void PlayerMovedEventHandler();

    [Signal]
    public delegate void ProgressTurnEventHandler();

    [Signal]
    public delegate void QueueChangedEventHandler();

    [Signal]
    public delegate void UpdateTimeLineEventHandler();
    
    
    private int _score;

    public int Score
    {
        get => _score;
        private set
        {
            _score = value;
            _timeSinceLastSpawn++;
            World.Instance.UpdateScore(_score);
        }
    }

    private int _highScore;

    private HexCoords _currentGridMousePosition = new(0, 0);

    private int _currentLayerChange;
    private List<HexCoords> _eligibleTiles = new();

    private TileMapLayer _entityStorage;
    private bool _isGameOver;
    private bool _isPlayerTurn;
    private TileMapLayer _mouseSnapping;
    private TileMapLayer _movementOptions;
    [Export] private Vector2I _targetScreenSize;
    public List<HexCoords> Queue { get; private set; }


    public Player Player { get; private set; } = new();

    private int CurrentLayerChange
    {
        get => _currentLayerChange;
        set
        {
            if (value == 0)
            {
                World.Instance.PeekedRoom = null;
                _currentLayerChange = 0;
            }

            if (value == -1 && World.Instance.GetCurrentRoomStack()
                    .HasNeighboringLayer(World.Instance.CurrentRoom, value))
            {
                _currentLayerChange = value;
                World.Instance.PeekedRoom = World.Instance.GetCurrentRoomStack()
                    .FindFirstByLayer(World.Instance.CurrentRoom.Layer - 1);
            }
            else if (value == +1 && World.Instance.GetCurrentRoomStack()
                         .HasNeighboringLayer(World.Instance.CurrentRoom, value))
            {
                _currentLayerChange = value;
                World.Instance.PeekedRoom = World.Instance.GetCurrentRoomStack()
                    .FindFirstByLayer(World.Instance.CurrentRoom.Layer + 1);
            }

            if (value == 2 || value == -2)
            {
                SetNewRoomByLayer(value / 2);
                _currentLayerChange = 0;
                EmitSignal("LayerChanged");
            }
        }
    }

    private Camera2D _camera;
    public override void _Ready()
    {
        _entityStorage = GetNode<TileMapLayer>("EntityStorage");
        _movementOptions = GetNode<TileMapLayer>("MovementOptions");
        _mouseSnapping = GetNode<TileMapLayer>("MouseSnapping");
        _camera = GetNode<Camera2D>("Camera");
    }

    private void LoadScore()
    {
        _highScore = Game.LoadScore();
    }
    
    public void SaveScore()
    {
        if(Score > _highScore) Game.SaveScore(Score);
    }

    public void SetupCamera()
    {
        _camera.Position = new Vector2(16, 8);
        _camera.Zoom = (GetViewportRect().Size / _targetScreenSize) * World.Instance.CurrentRoom.TargetScale;
    }

    public async void StartGameLoop()
    {
        Score = 0;
        LoadScore();
        EmitSignal("UpdateTimeLine");
        InitializeQueue();
        while (!_isGameOver) await TurnHandle();
    }

    public bool DoorsEnabled = false;
    
    private async Task TurnHandle()
    {
        if (World.Instance.IsEndlessWorld) await EndlessModeChanges();
        //Start by creating the Movement Patterns
        //First Generate the Movement Pattern in HexCoords
        _eligibleTiles.Clear();
        var currentCharacterId = _entityStorage.GetCellSourceId(OddQHexCoords.FromCubic(Queue.First()).ToVector2I());
        if (currentCharacterId == TileLookUp.PlayerId)
        {
            Score++;
            _isPlayerTurn = true;
            _eligibleTiles = HexUtil
                .FloodFill(Queue.First(), 4,
                    hex => hex.NeighborCoords()
                        .Where(n => World.Instance.CurrentRoom.IsTileWalkable(n) && IsTileEmpty(n)))
                .SelectMany(i => i).ToList();
            _eligibleTiles.RemoveAll(coords => coords == Queue.First());
            DrawMovementOptions();
            await ToSignal(this, "ProgressTurn");
            _isPlayerTurn = false;
        }
        else
        {
            var enemy = EnemyBehavior.GetEnemy(Queue.First());
            await enemy.Decide();
        }
        CheckForCollisions();
        if (IsRoomCleared())
        {
            EmitSignal("AllEnemiesDefeated");
            if(DoorsEnabled) World.Instance.MakeDoors();
        }
        else
        {
            World.Instance.DestroyDoors();
        }
        if(DoorsEnabled) CheckForDoors();
        var currentCharacter = Queue.First();
        Queue.RemoveAt(0);
        Queue.Add(currentCharacter);
        EmitSignal("QueueChanged");
    }


    private int _timeSinceLastSpawn = 0;
    private async Task EndlessModeChanges()
    {
        //First Update other rooms
        if (Score % 5 == 0 || Score == 1)
        {
            var rooms = World.Instance.GetRoomsExcept(World.Instance.CurrentRoom);
            var area = HexUtil.FloodFill(new(0, 0), 4, (c) => c.NeighborCoords()).SelectMany(subList => subList);
            foreach (var room in rooms)
            {
                room.PlaceRandomObstacles(8, area);
            }
        }
        
        if (Score % 10 == 0)
        {
            Player.Heal(1);
        }
        
        //Spawn enemies
        if (Queue.Count == 1 || (_timeSinceLastSpawn >= 3 && Queue.Count <= 5))
        {
            var freeTiles = HexUtil.FloodFill(new(0, 0), 5,
                hex => hex.NeighborCoords()).SelectMany(subList => subList).Where(n => World.Instance.CurrentRoom.IsTileWalkable(n) && IsTileEmpty(n)).Shuffled();
            var spawnBudget = int.Min((Score + 3) / 2, 5) - (Queue.Count - 1);
            foreach (var tile in freeTiles)
            {
                if (spawnBudget > 0)
                {
                    if (spawnBudget >= 1)
                    {
                        spawnBudget -= 1;
                        await SpawnEnemy(tile, TileLookUp.GenericEnemyId);
                    }
                }
                else
                {
                    _timeSinceLastSpawn = 0;
                    return;
                }
            }
        }
    }
    
    private void CheckForDoors()
    {
        var newPos = World.Instance.CheckForDoorsUsage(Queue.First());
    }
    public void DrawMovementOptions(List<HexCoords> movementOptions)
    {
        _movementOptions.Clear();
        foreach (var tile in movementOptions)
            _movementOptions.SetCell(OddQHexCoords.FromCubic(tile).ToVector2I(), TileLookUp.MovementOptionId,
                Vector2I.Zero);
    }

    public void DrawMovementOptions()
    {
        //Now Display it
        _movementOptions.Clear();
        foreach (var tile in _eligibleTiles)
            _movementOptions.SetCell(OddQHexCoords.FromCubic(tile).ToVector2I(), TileLookUp.MovementOptionId,
                Vector2I.Zero);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        
        if(Game.Instance.Paused) return;
        
        if (@event is InputEventMouseMotion)
        {
            var newPos = new OddQHexCoords(_mouseSnapping.LocalToMap(GetGlobalMousePosition() - Position)).ToCubic();
            if (newPos != _currentGridMousePosition) SetMouseMarkerPosition(newPos);
        }

        if (@event.IsActionPressed("left_click")) OnMouseClick();

        if (@event.IsActionPressed("layer_up") && _isPlayerTurn && Player.CanTimeTravel) CurrentLayerChange += 1;
        if (@event.IsActionPressed("layer_down") && _isPlayerTurn && Player.CanTimeTravel) CurrentLayerChange -= 1;
    }

    private async void OnMouseClick()
    {
        if (IsPlayerTurn() && _eligibleTiles.Contains(_currentGridMousePosition) && Player.CanMove)
        {
            _isPlayerTurn = false;
            await AnimatedMove(Queue.First(), _currentGridMousePosition, _eligibleTiles);
            EmitSignal("ProgressTurn");
            EmitSignal("PlayerMoved");
        }
    }

    public void Move(HexCoords from, HexCoords to)
    {
        var movedCharacter = _entityStorage.GetCellSourceId(OddQHexCoords.FromCubic(from).ToVector2I());
        _entityStorage.SetCell(OddQHexCoords.FromCubic(from).ToVector2I());
        _entityStorage.SetCell(OddQHexCoords.FromCubic(to).ToVector2I(), movedCharacter, Vector2I.Zero);
        //Update Queue
        Queue[Queue.FindIndex(coords => coords == from)] = to;
    }

    private bool IsPlayerTurn()
    {
        return _entityStorage.GetCellSourceId(OddQHexCoords.FromCubic(Queue.First()).ToVector2I()) ==
               TileLookUp.PlayerId;
    }

    private void InitializeQueue()
    {
        var playerPos = _entityStorage.GetUsedCellsById(TileLookUp.PlayerId).First();
        var entityList = _entityStorage.GetUsedCells();
        entityList.Remove(playerPos);
        entityList.Insert(0, playerPos);
        Queue = new List<HexCoords>(entityList.Select(v => new OddQHexCoords(v.X, v.Y).ToCubic()));
        EmitSignal("QueueChanged");
    }

    public int GetCharacterIdAtPosition(HexCoords position)
    {
        return _entityStorage.GetCellSourceId(OddQHexCoords.FromCubic(position).ToVector2I());
    }

    public IEnumerable<HexCoords> GetEligibleTiles()
    {
        return _eligibleTiles;
    }

    public HexCoords GetPlayerPosition()
    {
        return new OddQHexCoords(_entityStorage.GetUsedCellsById(TileLookUp.PlayerId).First()).ToCubic();
    }

    public bool IsTileEmpty(HexCoords coords)
    {
        var convertedCoords = OddQHexCoords.FromCubic(coords).ToVector2I();
        if (_entityStorage.GetCellSourceId(convertedCoords) == -1) return true;

        return false;
    }

    public void MarkTile(HexCoords coords)
    {
        _movementOptions.SetCell(OddQHexCoords.FromCubic(coords).ToVector2I(), TileLookUp.MouseMarkerId, Vector2I.Zero);
    }

    private void SetNewRoomByLayer(int layerChange)
    {
        World.Instance.CurrentRoom = World.Instance.GetCurrentRoomStack()
            .FindFirstByLayer(World.Instance.CurrentRoom.Layer + layerChange);
        if(layerChange > 0) AudioManager.Instance.PlayLayerUpSound();
        else if(layerChange < 0) AudioManager.Instance.PlayLayerDownSound();
        World.Instance.GetCurrentRoomStack().SetActiveByLayer(World.Instance.CurrentRoom.Layer);
        EmitSignal("ProgressTurn");
    }

    private void CheckForCollisions()
    {
        for (var i = Queue.Count - 1; i >= 0; i--)
        {
            var character = Queue[i];
            if (!World.Instance.CurrentRoom.IsTileWalkable(character))
            {
                var charId = _entityStorage.GetCellSourceId(OddQHexCoords.FromCubic(character).ToVector2I());
                if (charId == TileLookUp.PlayerId)
                {
                    Player.TakeDamage(3);
                }
                else
                {
                    var enemy = EnemyBehavior.GetEnemy(character);
                    enemy.TakeDamage(1);
                }
            }
        }
    }

    public void RemoveFromQueue(HexCoords coords)
    {
        Queue.Remove(coords);
        EmitSignal("QueueChanged");
    }

    public void ClearEntity(HexCoords coords)
    {
        _entityStorage.SetCell(OddQHexCoords.FromCubic(coords).ToVector2I());
    }

    public async Task AnimatedDamage(HexCoords position)
    {
        var id = GetCharacterIdAtPosition(position);
        var animation = TileLookUp.DamageAnim;
        for (var i = 0; i < animation.AnimationLength; i++)
        {
            var currentAtlasPosition =
                animation.AtlasStartPosition + new Vector2I(animation.AnimationSize.X * i, 0);
            _entityStorage.SetCell(OddQHexCoords.FromCubic(position).ToVector2I(), id, currentAtlasPosition);
            await ToSignal(GetTree().CreateTimer(animation.FrameDuration), "timeout");
        }
        _entityStorage.SetCell(OddQHexCoords.FromCubic(position).ToVector2I(), id, new(0, 0));
    }
    
    public async Task AnimatedDeath(HexCoords position)
    {
        var animation = TileLookUp.DeathAnim;
        for (var i = 0; i < animation.AnimationLength; i++)
        {
            var currentAtlasPosition =
                animation.AtlasStartPosition + new Vector2I(animation.AnimationSize.X * i, 0);
            _entityStorage.SetCell(OddQHexCoords.FromCubic(position).ToVector2I(), TileLookUp.DeathEffectID, currentAtlasPosition);
            await ToSignal(GetTree().CreateTimer(animation.FrameDuration), "timeout");
        }
        _entityStorage.SetCell(OddQHexCoords.FromCubic(position).ToVector2I());
        Score++;
    }

    public async Task AnimatedAttack(HexCoords position, HexCoords direction)
    {
        var id = GetCharacterIdAtPosition(position);
        if (direction.Length() != 1) return;
        if (id == TileLookUp.GenericEnemyId) AudioManager.Instance.PlayNormalAttackSound();
        if (direction == new HexCoords(0, -1) ||
            direction == new HexCoords(-1, 0) ||
            direction == new HexCoords(-1, 1))
        {
            var animation = TileLookUp.AttackAnims["left"];
            for (var i = 0; i < animation.AnimationLength; i++)
            {
                var currentAtlasPosition =
                    animation.AtlasStartPosition + new Vector2I(animation.AnimationSize.X * i, 0);
                _entityStorage.SetCell(OddQHexCoords.FromCubic(position).ToVector2I(), id, currentAtlasPosition);
                await ToSignal(GetTree().CreateTimer(animation.FrameDuration), "timeout");
            }
        }
        else
        {
            var animation = TileLookUp.AttackAnims["right"];
            for (var i = 0; i < animation.AnimationLength; i++)
            {
                var currentAtlasPosition =
                    animation.AtlasStartPosition + new Vector2I(animation.AnimationSize.X * i, 0);
                _entityStorage.SetCell(OddQHexCoords.FromCubic(position).ToVector2I(), id, currentAtlasPosition);
                await ToSignal(GetTree().CreateTimer(animation.FrameDuration), "timeout");
            }
        }

        _entityStorage.SetCell(OddQHexCoords.FromCubic(position).ToVector2I(), id, Vector2I.Zero);
    }

    public async Task AnimatedMove(HexCoords from, HexCoords to, IEnumerable<HexCoords> eligibleTiles)
    {
        CurrentLayerChange = 0;
        var path = HexUtil.FindPath(from, to, eligibleTiles);
        for (var i = 1; i < path.Count; i++) await AnimateMovement(path[i - 1], path[i]);
        //Update Queue
        Queue[Queue.FindIndex(coords => coords == from)] = to;
        EmitSignal("QueueChanged");
        CurrentLayerChange = 0;
    }

    private async Task AnimateMovement(HexCoords from, HexCoords to)
    {
        var movedCharacter = _entityStorage.GetCellSourceId(OddQHexCoords.FromCubic(from).ToVector2I());
        AudioManager.Instance.PlayWalkSound();
        var animation = TileLookUp.MovementAnims[to - from];
        for (var i = 0; i < animation.AnimationLength; i++)
        {
            var currentAtlasPosition = animation.AtlasStartPosition + new Vector2I(animation.AnimationSize.X * i, 0);
            _entityStorage.SetCell(OddQHexCoords.FromCubic(from).ToVector2I(), movedCharacter, currentAtlasPosition);
            if (i != animation.AnimationLength - 1)
                await ToSignal(GetTree().CreateTimer(animation.FrameDuration), "timeout");
        }

        _entityStorage.SetCell(OddQHexCoords.FromCubic(from).ToVector2I());
        _entityStorage.SetCell(OddQHexCoords.FromCubic(to).ToVector2I(), movedCharacter, Vector2I.Zero);
    }

    public bool IsRoomCleared()
    {
        return Queue.Count == 1;
    }

    public void OnPlayerDeath()
    {
        World.Instance.Ui.OnGameOver();
    }

    public void SetMouseMarkerPosition(HexCoords newPos)
    {
        _currentGridMousePosition = newPos;
        _mouseSnapping.Clear();
        _mouseSnapping.SetCell(OddQHexCoords.FromCubic(_currentGridMousePosition).ToVector2I(),
            TileLookUp.MouseMarkerId, Vector2I.Zero);
    }

    
    
    public async Task SpawnEnemy(HexCoords position, int id)
    {
        AudioManager.Instance.PlaySpawnSound();
        var animation = TileLookUp.SpawnAnim;
        for (var i = 0; i < animation.AnimationLength; i++)
        {
            var currentAtlasPosition = animation.AtlasStartPosition + new Vector2I(animation.AnimationSize.X * i, 0);
            _entityStorage.SetCell(OddQHexCoords.FromCubic(position).ToVector2I(), id, currentAtlasPosition);
            if (i != animation.AnimationLength - 1)
                await ToSignal(GetTree().CreateTimer(animation.FrameDuration), "timeout");
        }

        _entityStorage.SetCell(OddQHexCoords.FromCubic(position).ToVector2I(), id, new Vector2I(0, 0));
        Queue.Add(position);
        EmitSignal("QueueChanged");
    }

}