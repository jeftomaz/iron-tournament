using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace IronTournament.Core
{
    public readonly struct ItemModifier
    {
        public ItemModifier(ItemModifierKind kind, int amount)
        {
            if (!Enum.IsDefined(typeof(ItemModifierKind), kind) || kind == ItemModifierKind.None)
            {
                throw new ArgumentOutOfRangeException(nameof(kind));
            }

            if (amount == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            Kind = kind;
            Amount = amount;
        }

        public ItemModifierKind Kind { get; }

        public int Amount { get; }
    }

    public sealed class ItemConfiguration
    {
        private readonly ReadOnlyCollection<ItemModifier> modifiers;

        public ItemConfiguration(
            ItemId id,
            string displayName,
            int dropWeight,
            IList<ItemModifier> modifiers)
        {
            if (!Enum.IsDefined(typeof(ItemId), id) || id == ItemId.None)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            if (dropWeight <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dropWeight));
            }

            if (modifiers == null)
            {
                throw new ArgumentNullException(nameof(modifiers));
            }

            var modifierKinds = new HashSet<ItemModifierKind>();
            for (var index = 0; index < modifiers.Count; index++)
            {
                if (!modifierKinds.Add(modifiers[index].Kind))
                {
                    throw new ArgumentException("Modifiers must be unique.", nameof(modifiers));
                }
            }

            Id = id;
            DisplayName = ConfigurationGuard.DisplayName(displayName, nameof(displayName));
            DropWeight = dropWeight;
            this.modifiers = new ReadOnlyCollection<ItemModifier>(new List<ItemModifier>(modifiers));
        }

        public ItemId Id { get; }

        public string DisplayName { get; }

        public int DropWeight { get; }

        public IReadOnlyList<ItemModifier> Modifiers => modifiers;
    }
}
