using System;
using System.Collections.Generic;
using IronTournament.Core;

namespace IronTournament.Content
{
    public static class ContentMapper
    {
        public static CombatantConfiguration BuildCombatant(CombatantDefinition definition)
        {
            EnsureValid(ContentValidator.Validate(definition));
            return MapCombatant(definition);
        }

        public static EncounterConfiguration BuildEncounter(EncounterDefinition definition)
        {
            EnsureValid(ContentValidator.Validate(definition));
            return MapEncounter(definition);
        }

        public static CampaignConfiguration BuildCampaign(CampaignDefinition definition)
        {
            EnsureValid(ContentValidator.Validate(definition));
            return MapCampaign(definition);
        }

        public static bool TryBuildCampaign(
            CampaignDefinition definition,
            out CampaignConfiguration configuration,
            out ContentValidationReport report)
        {
            report = ContentValidator.Validate(definition);
            if (!report.IsValid)
            {
                configuration = null;
                return false;
            }

            configuration = MapCampaign(definition);
            return true;
        }

        private static CampaignConfiguration MapCampaign(CampaignDefinition definition)
        {
            var encounters = new List<EncounterConfiguration>(definition.OrderedEncounters.Count);
            foreach (var encounter in definition.OrderedEncounters)
            {
                encounters.Add(MapEncounter(encounter));
            }

            return new CampaignConfiguration(encounters);
        }

        private static EncounterConfiguration MapEncounter(EncounterDefinition definition)
        {
            var drops = new List<ItemConfiguration>(definition.DropPool.Count);
            foreach (var drop in definition.DropPool)
            {
                drops.Add(MapItem(drop));
            }

            return new EncounterConfiguration(
                MapCombatant(definition.Opponent),
                definition.EnemyStatVariancePercent,
                drops);
        }

        private static CombatantConfiguration MapCombatant(CombatantDefinition definition)
        {
            var abilities = new List<AbilityConfiguration>(definition.Abilities.Count);
            foreach (var ability in definition.Abilities)
            {
                abilities.Add(MapAbility(ability));
            }

            var stats = definition.BaseStats;
            return new CombatantConfiguration(
                definition.Id,
                definition.Side,
                definition.DisplayName.Trim(),
                new CombatantStats(stats.MaximumHealth, stats.Attack, stats.Defense),
                abilities);
        }

        private static AbilityConfiguration MapAbility(AbilityDefinition definition)
        {
            return new AbilityConfiguration(
                definition.Id,
                definition.DisplayName.Trim(),
                definition.Target,
                definition.ConsumesTurn);
        }

        private static ItemConfiguration MapItem(ItemDefinition definition)
        {
            var modifiers = new List<ItemModifier>(definition.Modifiers.Count);
            foreach (var modifier in definition.Modifiers)
            {
                modifiers.Add(new ItemModifier(modifier.Kind, modifier.Amount));
            }

            return new ItemConfiguration(
                definition.Id,
                definition.DisplayName.Trim(),
                definition.DropWeight,
                modifiers);
        }

        private static void EnsureValid(ContentValidationReport report)
        {
            if (!report.IsValid)
            {
                throw new ContentValidationException(report);
            }
        }
    }
}
