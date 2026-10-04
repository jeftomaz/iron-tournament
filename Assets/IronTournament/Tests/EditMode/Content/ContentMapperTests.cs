using System;
using System.Collections.Generic;
using System.Reflection;
using IronTournament.Core;
using NUnit.Framework;
using UnityEngine;

namespace IronTournament.Content.Tests
{
    public sealed class ContentMapperTests
    {
        private readonly List<UnityEngine.Object> assets = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var asset in assets)
            {
                UnityEngine.Object.DestroyImmediate(asset);
            }

            assets.Clear();
        }

        [Test]
        public void TryBuildCampaignMapsValidatedContent()
        {
            var campaign = CreateValidCampaign(out _);

            var success = ContentMapper.TryBuildCampaign(campaign, out var configuration, out var report);

            Assert.That(success, Is.True);
            Assert.That(report.IsValid, Is.True);
            Assert.That(configuration.Encounters.Count, Is.EqualTo(1));
            Assert.That(configuration.Encounters[0].Opponent.Id, Is.EqualTo(CombatantId.Goblin));
            Assert.That(configuration.Encounters[0].Opponent.BaseStats.MaximumHealth, Is.EqualTo(45));
        }

        [Test]
        public void BuildEncounterRejectsAnInvalidVariance()
        {
            CreateValidCampaign(out var encounter);
            Set(encounter, "enemyStatVariancePercent", 14);

            Assert.That(ContentValidator.Validate(encounter).IsValid, Is.False);
            Assert.Throws<ContentValidationException>(() => ContentMapper.BuildEncounter(encounter));
        }

        private CampaignDefinition CreateValidCampaign(out EncounterDefinition encounter)
        {
            var ability = Create<AbilityDefinition>();
            Set(ability, "id", AbilityId.BasicAttack);
            Set(ability, "displayName", "Atacar");
            Set(ability, "target", AbilityTarget.Opponent);

            object statsBox = new CombatantStatsDefinition();
            Set(statsBox, "maximumHealth", 45);
            Set(statsBox, "attack", 15);
            Set(statsBox, "defense", 3);

            var goblin = Create<CombatantDefinition>();
            Set(goblin, "id", CombatantId.Goblin);
            Set(goblin, "side", CombatantSide.Enemy);
            Set(goblin, "displayName", "Goblin");
            Set(goblin, "baseStats", (CombatantStatsDefinition)statsBox);
            Set(goblin, "abilities", new[] { ability });

            encounter = Create<EncounterDefinition>();
            Set(encounter, "opponent", goblin);
            Set(encounter, "dropPool", Array.Empty<ItemDefinition>());

            var campaign = Create<CampaignDefinition>();
            Set(campaign, "orderedEncounters", new[] { encounter });
            return campaign;
        }

        private T Create<T>() where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            assets.Add(asset);
            return asset;
        }

        private static void Set(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing serialized field '{fieldName}'.");
            field.SetValue(target, value);
        }
    }
}
