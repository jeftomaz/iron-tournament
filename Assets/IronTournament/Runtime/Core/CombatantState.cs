using System;

namespace IronTournament.Core
{
    public sealed class CombatantState
    {
        public CombatantState(CombatantConfiguration configuration, CombatantStats stats)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (stats.MaximumHealth <= 0 || stats.Attack <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stats));
            }

            Configuration = configuration;
            Stats = stats;
            CurrentHealth = stats.MaximumHealth;
        }

        public CombatantConfiguration Configuration { get; }

        public CombatantId Id => Configuration.Id;

        public CombatantSide Side => Configuration.Side;

        public CombatantStats Stats { get; }

        public int CurrentHealth { get; }

        public bool IsDefeated => CurrentHealth == 0;
    }
}
