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

        public bool CanRage { get; private set; }

        public bool HasRaged { get; private set; }

        public int HopeScrolls { get; private set; }

        public int GuardianHorns { get; private set; }

        public bool IsGuardianArmed { get; private set; }

        public bool HasFlameCloak { get; private set; }

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

        public bool IsHealthAtOrBelow(int percent)
        {
            return (long)CurrentHealth * 100 <= (long)Stats.MaximumHealth * percent;
        }

        public int HealthAtPercent(int percent)
        {
            return (int)((long)Stats.MaximumHealth * percent / 100);
        }

        internal void TakeDamage(int amount)
        {
            CurrentHealth = Math.Max(0, CurrentHealth - Math.Max(0, amount));
        }

        internal int Heal(int amount)
        {
            var healed = Math.Min(Math.Max(0, amount), Stats.MaximumHealth - CurrentHealth);
            CurrentHealth += healed;
            return healed;
        }

        internal void ApplyModifier(ItemModifier modifier)
        {
            switch (modifier.Kind)
            {
                case ItemModifierKind.MaximumHealth:
                    var maximumHealth = Bounded((long)Stats.MaximumHealth + modifier.Amount, 1);
                    Stats = new CombatantStats(maximumHealth, Stats.Attack, Stats.Defense);
                    CurrentHealth = Bounded((long)CurrentHealth + Math.Max(0, modifier.Amount), 0);
                    CurrentHealth = Math.Min(CurrentHealth, maximumHealth);
                    break;
                case ItemModifierKind.Attack:
                    Stats = new CombatantStats(
                        Stats.MaximumHealth,
                        Bounded((long)Stats.Attack + modifier.Amount, 1),
                        Stats.Defense);
                    break;
                case ItemModifierKind.Defense:
                    Stats = new CombatantStats(
                        Stats.MaximumHealth,
                        Stats.Attack,
                        Bounded((long)Stats.Defense + modifier.Amount, 0));
                    break;
            }
        }

        internal void RaiseGuard(int bonus)
        {
            GuardBonus = Math.Max(0, bonus);
        }

        internal void LowerGuard()
        {
            GuardBonus = 0;
        }

        internal void PrepareForEncounter()
        {
            GuardBonus = 0;
            HasRaged = false;
        }

        internal void EnableRage()
        {
            CanRage = true;
        }

        internal void MarkRaged()
        {
            HasRaged = true;
        }

        internal void AddHopeScroll()
        {
            HopeScrolls++;
        }

        internal void ConsumeHopeScroll()
        {
            HopeScrolls = Math.Max(0, HopeScrolls - 1);
        }

        internal void AddGuardianHorn()
        {
            GuardianHorns++;
        }

        internal void ArmGuardian()
        {
            GuardianHorns = Math.Max(0, GuardianHorns - 1);
            IsGuardianArmed = true;
        }

        internal void DisarmGuardian()
        {
            IsGuardianArmed = false;
        }

        internal void EquipFlameCloak()
        {
            HasFlameCloak = true;
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
            CanRage = source.CanRage;
            HasRaged = source.HasRaged;
            HopeScrolls = source.HopeScrolls;
            GuardianHorns = source.GuardianHorns;
            IsGuardianArmed = source.IsGuardianArmed;
            HasFlameCloak = source.HasFlameCloak;
        }

        private static int Bounded(long value, int minimum)
        {
            return (int)Math.Min(int.MaxValue, Math.Max(minimum, value));
        }
    }
}
