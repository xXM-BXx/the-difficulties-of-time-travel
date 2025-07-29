using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class EnemyBehavior : Node
{
    public static EnemyType GetEnemy(HexCoords position)
    {
        var id = World.Instance.Entities.GetCharacterIdAtPosition(position);
        if (id == TileLookUp.GenericEnemyId)
            return new GenericEnemy(position);
        return new GenericEnemy(position);
    }


    public abstract partial class EnemyType : Node
    {
        protected HexCoords Position;

        public EnemyType(HexCoords position)
        {
            Position = position;
        }

        public abstract Task Decide();
        public abstract Task TakeDamage(int damage);
    }

    private partial class GenericEnemy : EnemyType
    {
        public GenericEnemy(HexCoords position) : base(position)
        {
        }

        public List<HexCoords> GetEligibleTiles()
        {
            var eligibleTiles = HexUtil
                .FloodFill(Position, 2,
                    hex => hex.NeighborCoords().Where(n =>
                        World.Instance.CurrentRoom.IsTileWalkable(n) && World.Instance.Entities.IsTileEmpty(n)))
                .SelectMany(i => i).ToList();
            eligibleTiles.RemoveAll(coords => coords == Position);
            return eligibleTiles;
        }

        public override async Task Decide()
        {
            var options = GetEligibleTiles();
            World.Instance.Entities.DrawMovementOptions(options);

            var target = World.Instance.Entities.GetPlayerPosition();
            var bestPos = Position;
            var distance = int.MaxValue;
            if (target.DistanceTo(Position) == 1)
            {
                await World.Instance.Entities.AnimatedAttack(Position, target - Position);
                World.Instance.Entities.Player.TakeDamage(1);
                return;
            }

            foreach (var tile in options)
                if (target.DistanceTo(tile) < distance)
                {
                    distance = target.DistanceTo(tile);
                    bestPos = tile;
                }

            await World.Instance.Entities.AnimatedMove(Position, bestPos, options);
        }

        public override Task TakeDamage(int damage)
        {
            AudioManager.Instance.PlayPlayerHurtSound();
            World.Instance.Entities.AnimatedDeath(Position);
            World.Instance.Entities.RemoveFromQueue(Position);
            World.Instance.Entities.ClearEntity(Position);
            return Task.CompletedTask;
        }
    }
}