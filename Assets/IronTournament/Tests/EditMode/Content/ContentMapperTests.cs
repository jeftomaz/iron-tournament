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
            Assert.That(configuration.Encounters.Count, Is.EqualTo(CampaignConfiguration.CanonicalOrder.Count));
            Assert.That(configuration.Encounters[0].Opponent.Id, Is.EqualTo(CombatantId.Goblin));
            Assert.That(configuration.Encounters[0].Opponent.BaseStats.MaximumHealth, Is.EqualTo(45));
        }

        [Test]
        public void TryBuildCampaignRejectsTruncatedOrReorderedCampaigns()
        {
            var encounters = CreateCanonicalEncounters();
            var truncated = Create<CampaignDefinition>();
            Set(truncated, "orderedEncounters", new[] { encounters[0], encounters[1] });
            var reordered = Create<CampaignDefinition>();
            var swapped = (EncounterDefinition[])encounters.Clone();
            swapped[0] = encounters[1];
            swapped[1] = encounters[0];
            Set(reordered, "orderedEncounters", swapped);

            Assert.That(ContentMapper.TryBuildCampaign(truncated, out _, out var truncatedReport), Is.False);
            Assert.That(truncatedReport.IsValid, Is.False);
            Assert.That(ContentMapper.TryBuildCampaign(reordered, out _, out var reorderedReport), Is.False);
            Assert.That(reorderedReport.IsValid, Is.False);
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
            Assert.That(hero.Abilities.Count, Is.EqualTo(3));
            Assert.That(hero.Abilities[0].Id, Is.EqualTo(AbilityId.BasicAttack));
            Assert.That(hero.Abilities[1].Id, Is.EqualTo(AbilityId.Guard));
            Assert.That(hero.Abilities[2].Id, Is.EqualTo(AbilityId.UseHopeScroll));
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
        public void CanonicalCampaignAssetMapsAllEncountersAndDrops()
        {
            var campaign = AssetDatabase.LoadAssetAtPath<CampaignDefinition>(
                "Assets/IronTournament/Content/Campaign.asset");
            var warrior = AssetDatabase.LoadAssetAtPath<CombatantDefinition>(
                "Assets/IronTournament/Content/Warrior.asset");
            var mage = AssetDatabase.LoadAssetAtPath<CombatantDefinition>(
                "Assets/IronTournament/Content/Mage.asset");
            var knight = AssetDatabase.LoadAssetAtPath<CombatantDefinition>(
                "Assets/IronTournament/Content/Knight.asset");

            Assert.That(campaign, Is.Not.Null);
            Assert.That(warrior, Is.Not.Null);
            Assert.That(mage, Is.Not.Null);
            Assert.That(knight, Is.Not.Null);

            var configuration = ContentMapper.BuildCampaign(campaign);
            Assert.That(configuration.Encounters.Count, Is.EqualTo(CampaignConfiguration.CanonicalOrder.Count));

            for (var index = 0; index < configuration.Encounters.Count; index++)
            {
                var encounter = configuration.Encounters[index];
                Assert.That(encounter.Opponent.Id, Is.EqualTo(CampaignConfiguration.CanonicalOrder[index]));
                Assert.That(encounter.DropPool.Count, Is.EqualTo(9));
            }

            var warriorConfiguration = ContentMapper.BuildCombatant(warrior);
            var mageConfiguration = ContentMapper.BuildCombatant(mage);
            var knightConfiguration = ContentMapper.BuildCombatant(knight);
            Assert.That(warriorConfiguration.Abilities[2].Id, Is.EqualTo(AbilityId.UseHopeScroll));
            Assert.That(mageConfiguration.Abilities[3].Id, Is.EqualTo(AbilityId.ArmGuardian));
            Assert.That(knightConfiguration.BaseStats, Is.EqualTo(new CombatantStats(80, 26, 8)));
            Assert.That(knight.Portrait, Is.Not.Null);
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
            var encounters = CreateCanonicalEncounters();
            encounter = encounters[0];

            var campaign = Create<CampaignDefinition>();
            Set(campaign, "orderedEncounters", encounters);
            return campaign;
        }

        private EncounterDefinition[] CreateCanonicalEncounters()
        {
            var ability = Create<AbilityDefinition>();
            Set(ability, "id", AbilityId.BasicAttack);
            Set(ability, "displayName", "Atacar");
            Set(ability, "target", AbilityTarget.Opponent);

            var encounters = new EncounterDefinition[CampaignConfiguration.CanonicalOrder.Count];
            for (var index = 0; index < encounters.Length; index++)
            {
                object statsBox = new CombatantStatsDefinition();
                Set(statsBox, "maximumHealth", 45);
                Set(statsBox, "attack", 15);
                Set(statsBox, "defense", 3);

                var opponentId = CampaignConfiguration.CanonicalOrder[index];
                var opponent = Create<CombatantDefinition>();
                Set(opponent, "id", opponentId);
                Set(opponent, "side", CombatantSide.Enemy);
                Set(opponent, "displayName", opponentId.ToString());
                Set(opponent, "baseStats", (CombatantStatsDefinition)statsBox);
                Set(opponent, "abilities", new[] { ability });

                encounters[index] = Create<EncounterDefinition>();
                Set(encounters[index], "opponent", opponent);
                Set(encounters[index], "dropPool", Array.Empty<ItemDefinition>());
            }

            return encounters;
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
