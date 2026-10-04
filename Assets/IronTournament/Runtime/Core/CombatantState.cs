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

        private CombatantState(CombatantState source)
        {
            Configuration = source.Configuration;
            CopyFrom(source);
        }

        public CombatantConfiguration Configuration { get; }

        public CombatantId Id => Configuration.Id;

        public CombatantSide Side => Configuration.Side;

        public CombatantStats Stats { get; private set; }

        public int CurrentHealth { get; private set; }

        public int GuardBonus { get; private set; }

        public int Defense => Stats.Defense + GuardBonus;

        public bool IsDefeated => CurrentHealth == 0;

        public bool HasAbility(AbilityId ability)
        {
            var abilities = Configuration.Abilities;
            for (var index = 0; index < abilities.Count; index++)
            {
                if (abilities[index].Id == ability)
                {
                    return true;
                }
            }

            return false;
        }

        internal void TakeDamage(int amount)
        {
            CurrentHealth = Math.Max(0, CurrentHealth - Math.Max(0, amount));
        }

        internal void RaiseGuard(int bonus)
        {
            GuardBonus = Math.Max(0, bonus);
        }

        internal void LowerGuard()
        {
            GuardBonus = 0;
        }

        internal CombatantState Clone()
        {
            return new CombatantState(this);
        }

        internal void CopyFrom(CombatantState source)
        {
            if (source.Configuration != Configuration)
            {
                throw new ArgumentException("A snapshot must belong to the same combatant.", nameof(source));
            }

            Stats = source.Stats;
            CurrentHealth = source.CurrentHealth;
            GuardBonus = source.GuardBonus;
        }
    }
}
