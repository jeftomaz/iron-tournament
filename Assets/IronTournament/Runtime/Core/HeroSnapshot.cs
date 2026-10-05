using System;

namespace IronTournament.Core
{
    public sealed class HeroSnapshot
    {
        public const int MaximumStatValue = 9999;

        public HeroSnapshot(CombatantId heroClass, string displayName, CombatantStats stats)
        {
            if (!IsHeroClass(heroClass))
            {
                throw new ArgumentOutOfRangeException(nameof(heroClass));
            }

            if (stats.MaximumHealth <= 0
                || stats.Attack <= 0
                || stats.MaximumHealth > MaximumStatValue
                || stats.Attack > MaximumStatValue
                || stats.Defense > MaximumStatValue)
            {
                throw new ArgumentOutOfRangeException(nameof(stats));
            }

            HeroClass = heroClass;
            DisplayName = ConfigurationGuard.DisplayName(displayName, nameof(displayName));
            Stats = stats;
        }

        public CombatantId HeroClass { get; }

        public string DisplayName { get; }

        public CombatantStats Stats { get; }

        public static bool IsHeroClass(CombatantId id)
        {
            return id == CombatantId.Warrior || id == CombatantId.Mage;
        }
    }
}
