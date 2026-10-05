using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace IronTournament.Core.Tests
{
    public sealed class EnemyModeTests
    {
        [Test]
        public void EnemyModeRequiresBothClassesCleared()
        {
            var onlyMage = new ProgressData(new[]
            {
                new HeroSnapshot(CombatantId.Mage, "Mago", new CombatantStats(130, 40, 5))
            });

            Assert.Throws<ArgumentException>(
                () => CampaignRun.StartEnemyMode(CombatantId.Goblin, Roster(), new ProgressData(), new ScriptedRandomSource()));
            Assert.Throws<ArgumentException>(
                () => CampaignRun.StartEnemyMode(CombatantId.Goblin, Roster(), onlyMage, new ScriptedRandomSource()));
        }

        [TestCase(CombatantId.Warrior)]
        [TestCase(CombatantId.None)]
        [TestCase((CombatantId)99)]
        public void EnemyModeRejectsCharactersOutsideTheRoster(CombatantId character)
        {
            Assert.Throws<ArgumentException>(
                () => CampaignRun.StartEnemyMode(character, Roster(), Unlocked(), new ScriptedRandomSource()));
        }

        [Test]
        public void EnemyModeBuildsTiersWithoutTheChosenAndEndsWithTheHeroBoss()
        {
            var random = new ScriptedRandomSource(0, 1, 0, 1, 0, 0, 0);

            var run = CampaignRun.StartEnemyMode(CombatantId.Skeleton, Roster(), Unlocked(), random);

            Assert.That(run.Mode, Is.EqualTo(CampaignMode.EnemyMode));
            Assert.That(
                run.Opponents,
                Is.EqualTo(new[] { CombatantId.Goblin, CombatantId.Werewolf, CombatantId.Vampire, CombatantId.Mage }));
            Assert.That(run.Hero.Id, Is.EqualTo(CombatantId.Skeleton));
            Assert.That(run.Hero.Side, Is.EqualTo(CombatantSide.Player));
            Assert.That(run.Hero.Stats, Is.EqualTo(new CombatantStats(62, 20, 5)));
            Assert.That(run.Hero.HasAbility(AbilityId.Fury), Is.True);
            Assert.That(run.CurrentBattle.State.Opponent.Id, Is.EqualTo(CombatantId.Goblin));
            Assert.That(run.CurrentBattle.State.Opponent.CanRage, Is.False);
            Assert.That(run.CurrentBattle.AvailableActions, Is.EqualTo(new[] { AbilityId.BasicAttack }));
            Assert.That(random.Remaining, Is.Zero);
        }

        [Test]
        public void EnemyModeOffersThreeBaseItemsOnlyBeforeTheBoss()
        {
            var random = new ScriptedRandomSource(
                0, 0, 0, 0,
                0, 0, 0, 50, 50,
                0, 0, 0, 50, 50,
                0, 0, 0, 50, 50,
                0, 0, 0);
            var roster = Roster(
                TestContent.Enemy(CombatantId.Goblin, 100, 10, 0),
                TestContent.Enemy(CombatantId.Skeleton, 1, 20, 0),
                TestContent.Enemy(CombatantId.Knight, 1, 26, 0),
                TestContent.Enemy(CombatantId.Vampire, 1, 34, 0));
            var run = CampaignRun.StartEnemyMode(CombatantId.Goblin, roster, Unlocked(), random);

            for (var fight = 0; fight < 2; fight++)
            {
                run.CurrentBattle.Submit(AbilityId.BasicAttack);
                run.ConcludeEncounter();
                Assert.That(run.Phase, Is.EqualTo(CampaignPhase.Battle));
            }

            run.CurrentBattle.Submit(AbilityId.BasicAttack);
            run.ConcludeEncounter();

            Assert.That(run.Phase, Is.EqualTo(CampaignPhase.DropChoice));
            Assert.That(run.DropOffer.Count, Is.EqualTo(3));
            Assert.That(run.DropOffer[0].Id, Is.EqualTo(ItemId.HealingPotion));
            Assert.That(run.DropOffer[1].Id, Is.EqualTo(ItemId.AttackGem));
            Assert.That(run.DropOffer[2].Id, Is.EqualTo(ItemId.DefenseRune));

            run.ChooseDrop(ItemId.AttackGem);

            var boss = run.CurrentBattle.State.Opponent;
            Assert.That(run.Hero.Stats.Attack, Is.EqualTo(20));
            Assert.That(boss.Id, Is.EqualTo(CombatantId.Warrior));
            Assert.That(boss.Configuration.DisplayName, Is.EqualTo("Guerreiro"));
            Assert.That(boss.Stats, Is.EqualTo(new CombatantStats(150, 45, 30)));
            Assert.That(boss.CanRage, Is.False);
            Assert.That(random.Remaining, Is.Zero);
        }

        [Test]
        public void HeroFuryIsTheOnlyActionAtThirtyPercentAndHappensOncePerEncounter()
        {
            var random = new ScriptedRandomSource(0, 0, 0, 0, 0, 0, 0, 50, 50, 60, 50, 50, 60, 50, 50, 60, 40);
            var roster = Roster(
                TestContent.Enemy(CombatantId.Goblin, 100, 15, 35),
                TestContent.Enemy(CombatantId.Skeleton, 500, 50, 0));
            var battle = CampaignRun.StartEnemyMode(CombatantId.Goblin, roster, Unlocked(), random).CurrentBattle;
            for (var round = 0; round < 3; round++)
            {
                battle.Submit(AbilityId.BasicAttack);
            }

            Assert.That(battle.State.Hero.CurrentHealth, Is.EqualTo(25));
            Assert.That(battle.AvailableActions, Is.EqualTo(new[] { AbilityId.Fury }));
            Assert.That(battle.Submit(AbilityId.BasicAttack).Rejection, Is.EqualTo(ActionRejection.UnavailableAction));

            var result = battle.Submit(AbilityId.Fury);

            WarriorGoblinTests.AssertAbility(result.Events[0], CombatantId.Goblin, AbilityId.Fury);
            WarriorGoblinTests.AssertDamage(result.Events[1], CombatantId.Skeleton, DamageKind.Fury, 305, 150);
            WarriorGoblinTests.AssertDamage(result.Events[3], CombatantId.Goblin, DamageKind.Attack, 5, 20);
            Assert.That(battle.State.Hero.HasRaged, Is.True);
            Assert.That(battle.AvailableActions, Is.EqualTo(new[] { AbilityId.BasicAttack }));
            Assert.That(random.Remaining, Is.Zero);
        }

        [Test]
        public void VampireAndNecromancerAttacksIgnoreDefense()
        {
            var random = new ScriptedRandomSource(0, 0, 0, 0, 0, 0, 0, 15);
            var battle = CampaignRun.StartEnemyMode(CombatantId.Vampire, Roster(), Unlocked(), random).CurrentBattle;

            var result = battle.Submit(AbilityId.BasicAttack);

            WarriorGoblinTests.AssertDamage(result.Events[1], CombatantId.Goblin, DamageKind.Attack, 34, 11);
            WarriorGoblinTests.AssertDamage(result.Events[3], CombatantId.Vampire, DamageKind.Attack, 3, 107);
            Assert.That(random.Remaining, Is.Zero);
        }

        private static ProgressData Unlocked()
        {
            return new ProgressData(new[]
            {
                new HeroSnapshot(CombatantId.Warrior, "Guerreiro", new CombatantStats(150, 45, 30)),
                new HeroSnapshot(CombatantId.Mage, "Mago", new CombatantStats(130, 40, 5))
            });
        }

        private static CampaignConfiguration Roster(params CombatantConfiguration[] overrides)
        {
            var enemies = new List<CombatantConfiguration>
            {
                TestContent.Goblin(),
                TestContent.Enemy(CombatantId.Skeleton, 62, 20, 5),
                TestContent.Enemy(CombatantId.Knight, 80, 26, 8),
                TestContent.Enemy(CombatantId.Werewolf, 95, 30, 10),
                TestContent.Enemy(CombatantId.Vampire, 110, 34, 12),
                TestContent.Enemy(CombatantId.Necromancer, 125, 38, 14),
                TestContent.Enemy(CombatantId.DemonKing, 145, 44, 16)
            };
            foreach (var replacement in overrides)
            {
                enemies[enemies.FindIndex(enemy => enemy.Id == replacement.Id)] = replacement;
            }

            var dropPool = new[]
            {
                TestContent.Item(ItemId.HealingPotion, 3),
                TestContent.Item(ItemId.AttackGem, 3, new ItemModifier(ItemModifierKind.Attack, 10)),
                TestContent.Item(ItemId.DefenseRune, 3, new ItemModifier(ItemModifierKind.Defense, 5)),
                TestContent.Item(ItemId.HopeScroll, 2),
                TestContent.Item(ItemId.FlameCloak, 2)
            };
            return TestContent.CampaignWithDrops(dropPool, enemies.ToArray());
        }
    }
}
