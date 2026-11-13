using System;
using System.Collections.Generic;
using UnityEngine;

namespace IronTournament.Content
{
    [CreateAssetMenu(menuName = "Iron Tournament/Campaign", fileName = "Campaign")]
    public sealed class CampaignDefinition : ScriptableObject
    {
        [SerializeField] private EncounterDefinition[] orderedEncounters;
        [SerializeField] private CombatantDefinition finalPhase;

        public IReadOnlyList<EncounterDefinition> OrderedEncounters => orderedEncounters ?? Array.Empty<EncounterDefinition>();
        public CombatantDefinition FinalPhase => finalPhase;
    }
}
