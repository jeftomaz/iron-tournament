using System;
using System.Collections.Generic;
using IronTournament.Core;
using UnityEngine;

namespace IronTournament.Content
{
    [CreateAssetMenu(menuName = "Iron Tournament/Encounter", fileName = "Encounter")]
    public sealed class EncounterDefinition : ScriptableObject
    {
        [SerializeField] private CombatantDefinition opponent;
        [Range(0, 100)]
        [SerializeField] private int enemyStatVariancePercent = EncounterConfiguration.RequiredEnemyStatVariancePercent;
        [SerializeField] private ItemDefinition[] dropPool;

        public CombatantDefinition Opponent => opponent;

        public int EnemyStatVariancePercent => enemyStatVariancePercent;

        public IReadOnlyList<ItemDefinition> DropPool => dropPool ?? Array.Empty<ItemDefinition>();
    }
}
