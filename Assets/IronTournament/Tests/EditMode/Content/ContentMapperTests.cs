using System;
using System.Collections.Generic;
using System.Reflection;
using IronTournament.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;

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
        public void BattleAssetsMapToPlayableWarriorGoblinContent()
        {
            var warrior = AssetDatabase.LoadAssetAtPath<CombatantDefinition>("Assets/IronTournament/Content/Warrior.asset");
            var encounter = AssetDatabase.LoadAssetAtPath<EncounterDefinition>("Assets/IronTournament/Content/GoblinEncounter.asset");
            Assert.That(warrior, Is.Not.Null);
            Assert.That(encounter, Is.Not.Null);
            var hero = ContentMapper.BuildCombatant(warrior);
            var configuration = ContentMapper.BuildEncounter(encounter);
            Assert.That(hero.Id, Is.EqualTo(CombatantId.Warrior));
            Assert.That(hero.Side, Is.EqualTo(CombatantSide.Player));
            Assert.That(hero.BaseStats.MaximumHealth, Is.EqualTo(120));
            Assert.That(hero.BaseStats.Attack, Is.EqualTo(30));
            Assert.That(hero.BaseStats.Defense, Is.EqualTo(23));
            Assert.That(hero.Abilities.Count, Is.EqualTo(2));
            Assert.That(hero.Abilities[0].Id, Is.EqualTo(AbilityId.BasicAttack));
            Assert.That(hero.Abilities[1].Id, Is.EqualTo(AbilityId.Guard));
            Assert.That(configuration.Opponent.Id, Is.EqualTo(CombatantId.Goblin));
            Assert.That(configuration.Opponent.Side, Is.EqualTo(CombatantSide.Enemy));
            Assert.That(configuration.Opponent.BaseStats.MaximumHealth, Is.EqualTo(45));
            Assert.That(configuration.Opponent.BaseStats.Attack, Is.EqualTo(15));
            Assert.That(configuration.Opponent.BaseStats.Defense, Is.EqualTo(3));
            Assert.That(configuration.EnemyStatVariancePercent, Is.EqualTo(15));
            Assert.That(warrior.Portrait, Is.Not.Null);
            Assert.That(encounter.Opponent.Portrait, Is.Not.Null);
            var battle = new Battle(new BattleState(new CombatantState(hero, hero.BaseStats),
                new CombatantState(configuration.Opponent, configuration.Opponent.BaseStats)), new SeededRandomSource(17));
            Assert.That(battle.Submit(AbilityId.Guard).IsAccepted, Is.True);
            Assert.That(battle.Submit(AbilityId.BasicAttack).IsAccepted, Is.True);
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
