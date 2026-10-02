using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace IronTournament.Core
{
    public sealed class CampaignConfiguration
    {
        private readonly ReadOnlyCollection<EncounterConfiguration> encounters;

        public CampaignConfiguration(IList<EncounterConfiguration> encounters)
        {
            if (encounters == null)
            {
                throw new ArgumentNullException(nameof(encounters));
            }

            if (encounters.Count == 0)
            {
                throw new ArgumentException("At least one encounter is required.", nameof(encounters));
            }

            var opponents = new HashSet<CombatantId>();
            for (var index = 0; index < encounters.Count; index++)
            {
                var encounter = encounters[index];
                if (encounter == null)
                {
                    throw new ArgumentException("An encounter is required.", nameof(encounters));
                }

                if (!opponents.Add(encounter.Opponent.Id))
                {
                    throw new ArgumentException("Encounter opponents must be unique.", nameof(encounters));
                }
            }

            this.encounters = new ReadOnlyCollection<EncounterConfiguration>(
                new List<EncounterConfiguration>(encounters));
        }

        public IReadOnlyList<EncounterConfiguration> Encounters => encounters;
    }
}
