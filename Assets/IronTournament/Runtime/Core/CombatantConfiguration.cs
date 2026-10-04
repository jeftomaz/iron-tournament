using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace IronTournament.Core
{
    public readonly struct CombatantStats
    {
        public CombatantStats(int maximumHealth, int attack, int defense)
        {
            if (maximumHealth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumHealth));
            }

            if (attack <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(attack));
            }

            if (defense < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(defense));
            }

            MaximumHealth = maximumHealth;
            Attack = attack;
            Defense = defense;
        }

        public int MaximumHealth { get; }

        public int Attack { get; }

        public int Defense { get; }
    }

    public sealed class CombatantConfiguration
    {
        private readonly ReadOnlyCollection<AbilityConfiguration> abilities;

        public CombatantConfiguration(
            CombatantId id,
            CombatantSide side,
            string displayName,
            CombatantStats baseStats,
            IList<AbilityConfiguration> abilities)
        {
            if (!Enum.IsDefined(typeof(CombatantId), id) || id == CombatantId.None)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            if (!Enum.IsDefined(typeof(CombatantSide), side) || side == CombatantSide.None)
            {
                throw new ArgumentOutOfRangeException(nameof(side));
            }

            if (abilities == null)
            {
                throw new ArgumentNullException(nameof(abilities));
            }

            if (abilities.Count == 0)
            {
                throw new ArgumentException("At least one ability is required.", nameof(abilities));
            }

            var abilityIds = new HashSet<AbilityId>();
            for (var index = 0; index < abilities.Count; index++)
            {
                var ability = abilities[index];
                if (ability == null)
                {
                    throw new ArgumentException("An ability is required.", nameof(abilities));
                }

                if (!abilityIds.Add(ability.Id))
                {
                    throw new ArgumentException("Abilities must be unique.", nameof(abilities));
                }
            }

            Id = id;
            Side = side;
            DisplayName = ConfigurationGuard.DisplayName(displayName, nameof(displayName));
            BaseStats = baseStats;
            this.abilities = new ReadOnlyCollection<AbilityConfiguration>(
                new List<AbilityConfiguration>(abilities));
        }

        public CombatantId Id { get; }

        public CombatantSide Side { get; }

        public string DisplayName { get; }

        public CombatantStats BaseStats { get; }

        public IReadOnlyList<AbilityConfiguration> Abilities => abilities;
    }
}
