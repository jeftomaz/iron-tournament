using System;
using System.Collections.Generic;
using IronTournament.Core;
using UnityEngine;

namespace IronTournament.Content
{
    [Serializable]
    public struct ItemModifierDefinition
    {
        [SerializeField] private ItemModifierKind kind;
        [SerializeField] private int amount;

        public ItemModifierKind Kind => kind;

        public int Amount => amount;
    }

    [CreateAssetMenu(menuName = "Iron Tournament/Item", fileName = "Item")]
    public sealed class ItemDefinition : ScriptableObject
    {
        [SerializeField] private ItemId id;
        [SerializeField] private string displayName;
        [Min(1)] [SerializeField] private int dropWeight = 1;
        [SerializeField] private ItemModifierDefinition[] modifiers;

        public ItemId Id => id;

        public string DisplayName => displayName;

        public int DropWeight => dropWeight;

        public IReadOnlyList<ItemModifierDefinition> Modifiers => modifiers ?? Array.Empty<ItemModifierDefinition>();
    }
}
