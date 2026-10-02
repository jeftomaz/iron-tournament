using System;
using System.Collections.Generic;
using IronTournament.Core;
using UnityEngine;

namespace IronTournament.Content
{
    [Serializable]
    public struct CombatantStatsDefinition
    {
        [SerializeField] private int maximumHealth;
        [SerializeField] private int attack;
        [SerializeField] private int defense;

        public int MaximumHealth => maximumHealth;

        public int Attack => attack;

        public int Defense => defense;
    }

    [CreateAssetMenu(menuName = "Iron Tournament/Combatant", fileName = "Combatant")]
    public sealed class CombatantDefinition : ScriptableObject
    {
        [SerializeField] private CombatantId id;
        [SerializeField] private CombatantSide side;
        [SerializeField] private string displayName;
        [SerializeField] private CombatantStatsDefinition baseStats;
        [SerializeField] private AbilityDefinition[] abilities;
        [SerializeField] private Sprite portrait;

        public CombatantId Id => id;

        public CombatantSide Side => side;

        public string DisplayName => displayName;

        public CombatantStatsDefinition BaseStats => baseStats;

        public IReadOnlyList<AbilityDefinition> Abilities => abilities ?? Array.Empty<AbilityDefinition>();

        public Sprite Portrait => portrait;
    }
}
