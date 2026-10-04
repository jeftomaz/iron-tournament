using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace IronTournament.Core
{
    public sealed class CampaignConfiguration
    {
        public static readonly IReadOnlyList<CombatantId> CanonicalOrder = new ReadOnlyCollection<CombatantId>(new[]
        {
            CombatantId.Goblin,
            CombatantId.Skeleton,
            CombatantId.Knight,
            CombatantId.Werewolf,
            CombatantId.Vampire,
            CombatantId.Necromancer,
            CombatantId.DemonKing
        });

        private readonly ReadOnlyCollection<EncounterConfiguration> encounters;

        public CampaignConfiguration(IList<EncounterConfiguration> encounters)
        {
            if (encounters == null)
            {
                throw new ArgumentNullException(nameof(encounters));
            }

            if (encounters.Count != CanonicalOrder.Count)
            {
                throw new ArgumentException("The campaign must contain the canonical encounters.", nameof(encounters));
            }

            for (var index = 0; index < encounters.Count; index++)
            {
                var encounter = encounters[index];
                if (encounter == null)
                {
                    throw new ArgumentException("An encounter is required.", nameof(encounters));
                }

                if (encounter.Opponent.Id != CanonicalOrder[index])
                {
                    throw new ArgumentException("Encounters must follow the canonical order.", nameof(encounters));
                }
            }

            this.encounters = new ReadOnlyCollection<EncounterConfiguration>(
                new List<EncounterConfiguration>(encounters));
        }

        public IReadOnlyList<EncounterConfiguration> Encounters => encounters;
    }
}
