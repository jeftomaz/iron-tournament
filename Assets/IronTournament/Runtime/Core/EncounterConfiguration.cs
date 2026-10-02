using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace IronTournament.Core
{
    public sealed class EncounterConfiguration
    {
        public const int RequiredEnemyStatVariancePercent = 15;

        private readonly ReadOnlyCollection<ItemConfiguration> dropPool;

        public EncounterConfiguration(
            CombatantConfiguration opponent,
            int enemyStatVariancePercent,
            IList<ItemConfiguration> dropPool)
        {
            if (opponent == null)
            {
                throw new ArgumentNullException(nameof(opponent));
            }

            if (opponent.Side != CombatantSide.Enemy)
            {
                throw new ArgumentException("The opponent must be an enemy.", nameof(opponent));
            }

            if (enemyStatVariancePercent != RequiredEnemyStatVariancePercent)
            {
                throw new ArgumentOutOfRangeException(nameof(enemyStatVariancePercent));
            }

            if (dropPool == null)
            {
                throw new ArgumentNullException(nameof(dropPool));
            }

            var dropIds = new HashSet<ItemId>();
            for (var index = 0; index < dropPool.Count; index++)
            {
                var drop = dropPool[index];
                if (drop == null)
                {
                    throw new ArgumentException("A drop is required.", nameof(dropPool));
                }

                if (!dropIds.Add(drop.Id))
                {
                    throw new ArgumentException("Drops must be unique.", nameof(dropPool));
                }
            }

            Opponent = opponent;
            EnemyStatVariancePercent = enemyStatVariancePercent;
            this.dropPool = new ReadOnlyCollection<ItemConfiguration>(
                new List<ItemConfiguration>(dropPool));
        }

        public CombatantConfiguration Opponent { get; }

        public int EnemyStatVariancePercent { get; }

        public IReadOnlyList<ItemConfiguration> DropPool => dropPool;
    }
}
