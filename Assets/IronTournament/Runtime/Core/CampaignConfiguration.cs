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

        public CampaignConfiguration(IList<EncounterConfiguration> encounters, CombatantConfiguration finalPhase = null)
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

            if (finalPhase != null && (finalPhase.Id != CombatantId.DemonKing || finalPhase.Side != CombatantSide.Enemy))
                throw new ArgumentException("The final phase must be the Demon King.", nameof(finalPhase));
            FinalPhase = finalPhase;
            this.encounters = new ReadOnlyCollection<EncounterConfiguration>(
                new List<EncounterConfiguration>(encounters));
        }

        public IReadOnlyList<EncounterConfiguration> Encounters => encounters;
        public CombatantConfiguration FinalPhase { get; }
    }
}
