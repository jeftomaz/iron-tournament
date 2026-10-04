using System;
using System.Collections.Generic;
using IronTournament.Core;

namespace IronTournament.Content
{
    public static class ContentValidator
    {
        public static ContentValidationReport Validate(CampaignDefinition definition)
        {
            var errors = new List<string>();
            if (definition == null)
            {
                errors.Add("Campaign definition is required.");
                return new ContentValidationReport(errors);
            }

            if (definition.OrderedEncounters.Count == 0)
            {
                errors.Add("Campaign must contain at least one encounter.");
            }

            var opponents = new HashSet<CombatantId>();
            for (var index = 0; index < definition.OrderedEncounters.Count; index++)
            {
                var encounter = definition.OrderedEncounters[index];
                ValidateEncounter(encounter, $"Campaign encounter {index + 1}", errors);

                if (encounter != null && encounter.Opponent != null && !opponents.Add(encounter.Opponent.Id))
                {
                    errors.Add($"Campaign repeats opponent '{encounter.Opponent.Id}'.");
                }
            }

            return new ContentValidationReport(errors);
        }

        public static ContentValidationReport Validate(EncounterDefinition definition)
        {
            var errors = new List<string>();
            ValidateEncounter(definition, "Encounter", errors);
            return new ContentValidationReport(errors);
        }

        public static ContentValidationReport Validate(CombatantDefinition definition)
        {
            var errors = new List<string>();
            ValidateCombatant(definition, "Combatant", errors);
            return new ContentValidationReport(errors);
        }

        public static ContentValidationReport Validate(AbilityDefinition definition)
        {
            var errors = new List<string>();
            ValidateAbility(definition, "Ability", errors);
            return new ContentValidationReport(errors);
        }

        public static ContentValidationReport Validate(ItemDefinition definition)
        {
            var errors = new List<string>();
            ValidateItem(definition, "Item", errors);
            return new ContentValidationReport(errors);
        }

        private static void ValidateEncounter(
            EncounterDefinition definition,
            string context,
            ICollection<string> errors)
        {
            if (definition == null)
            {
                errors.Add($"{context} definition is required.");
                return;
            }

            if (definition.Opponent == null)
            {
                errors.Add($"{context} requires an opponent.");
            }
            else
            {
                ValidateCombatant(definition.Opponent, $"{context} opponent", errors);
                if (definition.Opponent.Side != CombatantSide.Enemy)
                {
                    errors.Add($"{context} opponent must be an enemy.");
                }
            }

            if (definition.EnemyStatVariancePercent != EncounterConfiguration.RequiredEnemyStatVariancePercent)
            {
                errors.Add(
                    $"{context} must use ±{EncounterConfiguration.RequiredEnemyStatVariancePercent}% enemy stat variance.");
            }

            var drops = new HashSet<ItemId>();
            for (var index = 0; index < definition.DropPool.Count; index++)
            {
                var item = definition.DropPool[index];
                ValidateItem(item, $"{context} drop {index + 1}", errors);
                if (item != null && !drops.Add(item.Id))
                {
                    errors.Add($"{context} repeats drop '{item.Id}'.");
                }
            }
        }

        private static void ValidateCombatant(
            CombatantDefinition definition,
            string context,
            ICollection<string> errors)
        {
            if (definition == null)
            {
                errors.Add($"{context} definition is required.");
                return;
            }

            if (!IsValid(definition.Id, CombatantId.None))
            {
                errors.Add($"{context} has an invalid id.");
            }

            if (!IsValid(definition.Side, CombatantSide.None))
            {
                errors.Add($"{context} has an invalid side.");
            }

            ValidateDisplayName(definition.DisplayName, context, errors);
            if (definition.BaseStats.MaximumHealth <= 0)
            {
                errors.Add($"{context} maximum health must be positive.");
            }

            if (definition.BaseStats.Attack <= 0)
            {
                errors.Add($"{context} attack must be positive.");
            }

            if (definition.BaseStats.Defense < 0)
            {
                errors.Add($"{context} defense cannot be negative.");
            }

            if (definition.Abilities.Count == 0)
            {
                errors.Add($"{context} requires at least one ability.");
            }

            var abilities = new HashSet<AbilityId>();
            for (var index = 0; index < definition.Abilities.Count; index++)
            {
                var ability = definition.Abilities[index];
                ValidateAbility(ability, $"{context} ability {index + 1}", errors);
                if (ability != null && !abilities.Add(ability.Id))
                {
                    errors.Add($"{context} repeats ability '{ability.Id}'.");
                }
            }
        }

        private static void ValidateAbility(
            AbilityDefinition definition,
            string context,
            ICollection<string> errors)
        {
            if (definition == null)
            {
                errors.Add($"{context} definition is required.");
                return;
            }

            if (!IsValid(definition.Id, AbilityId.None))
            {
                errors.Add($"{context} has an invalid id.");
            }

            if (!IsValid(definition.Target, AbilityTarget.None))
            {
                errors.Add($"{context} has an invalid target.");
            }

            ValidateDisplayName(definition.DisplayName, context, errors);
        }

        private static void ValidateItem(
            ItemDefinition definition,
            string context,
            ICollection<string> errors)
        {
            if (definition == null)
            {
                errors.Add($"{context} definition is required.");
                return;
            }

            if (!IsValid(definition.Id, ItemId.None))
            {
                errors.Add($"{context} has an invalid id.");
            }

            ValidateDisplayName(definition.DisplayName, context, errors);
            if (definition.DropWeight <= 0)
            {
                errors.Add($"{context} drop weight must be positive.");
            }

            var modifiers = new HashSet<ItemModifierKind>();
            for (var index = 0; index < definition.Modifiers.Count; index++)
            {
                var modifier = definition.Modifiers[index];
                if (!IsValid(modifier.Kind, ItemModifierKind.None))
                {
                    errors.Add($"{context} modifier {index + 1} has an invalid kind.");
                }

                if (modifier.Amount == 0)
                {
                    errors.Add($"{context} modifier {index + 1} cannot be zero.");
                }

                if (!modifiers.Add(modifier.Kind))
                {
                    errors.Add($"{context} repeats modifier '{modifier.Kind}'.");
                }
            }
        }

        private static void ValidateDisplayName(
            string displayName,
            string context,
            ICollection<string> errors)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                errors.Add($"{context} requires a display name.");
                return;
            }

            if (displayName.Trim().Length > 48)
            {
                errors.Add($"{context} display name exceeds 48 characters.");
            }

            for (var index = 0; index < displayName.Length; index++)
            {
                var character = displayName[index];
                if (char.IsControl(character) || character == '<' || character == '>')
                {
                    errors.Add($"{context} display name contains unsafe characters.");
                    return;
                }
            }
        }

        private static bool IsValid(CombatantId value, CombatantId none)
        {
            return Enum.IsDefined(typeof(CombatantId), value) && value != none;
        }

        private static bool IsValid(CombatantSide value, CombatantSide none)
        {
            return Enum.IsDefined(typeof(CombatantSide), value) && value != none;
        }

        private static bool IsValid(AbilityId value, AbilityId none)
        {
            return Enum.IsDefined(typeof(AbilityId), value) && value != none;
        }

        private static bool IsValid(AbilityTarget value, AbilityTarget none)
        {
            return Enum.IsDefined(typeof(AbilityTarget), value) && value != none;
        }

        private static bool IsValid(ItemId value, ItemId none)
        {
            return Enum.IsDefined(typeof(ItemId), value) && value != none;
        }

        private static bool IsValid(ItemModifierKind value, ItemModifierKind none)
        {
            return Enum.IsDefined(typeof(ItemModifierKind), value) && value != none;
        }
    }
}
