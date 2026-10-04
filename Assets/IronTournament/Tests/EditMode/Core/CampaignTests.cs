using System;
using NUnit.Framework;

namespace IronTournament.Core.Tests
{
    public sealed class CampaignTests
    {
        [Test]
        public void EncounterVariesEachEnemyStatOnceWithinFifteenPercent()
        {
            var random = new ScriptedRandomSource(-15, 15, 0);

            var run = new CampaignRun(TestContent.Mage(), TestContent.Campaign(TestContent.Goblin()), random);

            var opponent = run.CurrentBattle.State.Opponent;
            Assert.That(opponent.Stats.MaximumHealth, Is.EqualTo(38));
            Assert.That(opponent.Stats.Attack, Is.EqualTo(17));
            Assert.That(opponent.Stats.Defense, Is.EqualTo(3));
            Assert.That(opponent.CurrentHealth, Is.EqualTo(38));
            Assert.That(random.Remaining, Is.Zero);
        }

        [Test]
        public void VariationRoundsAndKeepsStatsValid()
        {
            var opponent = TestContent.Enemy(CombatantId.Goblin, 45, 1, 0);

            var run = new CampaignRun(
                TestContent.Mage(),
                TestContent.Campaign(opponent),
                new ScriptedRandomSource(15, -15, -15));

            var stats = run.CurrentBattle.State.Opponent.Stats;
            Assert.That(stats.MaximumHealth, Is.EqualTo(52));
            Assert.That(stats.Attack, Is.EqualTo(1));
            Assert.That(stats.Defense, Is.Zero);
        }

        [Test]
        public void ReversionKeepsTheStatsAlreadyRolled()
        {
            var random = new ScriptedRandomSource(-15, 15, 0, 17);
            var run = new CampaignRun(TestContent.Mage(), TestContent.Campaign(TestContent.Goblin()), random);
            var battle = run.CurrentBattle;
            battle.Submit(AbilityId.BasicAttack);

            battle.Submit(AbilityId.RevertBattle);

            Assert.That(battle.State.Opponent.Stats.MaximumHealth, Is.EqualTo(38));
            Assert.That(battle.State.Opponent.Stats.Attack, Is.EqualTo(17));
            Assert.That(battle.State.Opponent.CurrentHealth, Is.EqualTo(38));
            Assert.That(random.Remaining, Is.Zero);
        }

        [TestCase(12)]
        [TestCase(18)]
        public void OpponentDamageVariesTwentyPercent(int roll)
        {
            var battle = TestContent.StartBattle(TestContent.Mage(), TestContent.Goblin(), new ScriptedRandomSource(roll));

            battle.Submit(AbilityId.BasicAttack);

            Assert.That(battle.State.Hero.CurrentHealth, Is.EqualTo(110 - (roll - 5)));
        }

        [TestCase(11)]
        [TestCase(19)]
        public void OpponentDamageNeverLeavesTheVarianceRange(int roll)
        {
            var battle = TestContent.StartBattle(TestContent.Mage(), TestContent.Goblin(), new ScriptedRandomSource(roll));

            Assert.Throws<InvalidOperationException>(() => battle.Submit(AbilityId.BasicAttack));
        }

        [TestCase(CombatantId.Vampire, true)]
        [TestCase(CombatantId.Necromancer, true)]
        [TestCase(CombatantId.DemonKing, true)]
        [TestCase(CombatantId.Goblin, false)]
        [TestCase(CombatantId.Skeleton, false)]
        [TestCase(CombatantId.Knight, false)]
        [TestCase(CombatantId.Werewolf, false)]
        public void OnlyTheLastThreeEnemiesCanRage(CombatantId id, bool canRage)
        {
            var opponent = TestContent.Enemy(id, 100, 34, 12);

            var run = new CampaignRun(TestContent.Mage(), TestContent.Campaign(opponent), new ScriptedRandomSource(0, 0, 0));

            Assert.That(run.CurrentBattle.State.Opponent.CanRage, Is.EqualTo(canRage));
        }

        [Test]
        public void FuryTriggersOnceAtThirtyPercentAndReplacesTheAttack()
        {
            var random = new ScriptedRandomSource(0, 0, 0, 30, 30, 30, 30, 30, 30, 30);
            var battle = StartFuryCampaign(random, AbilityId.BasicAttack);
            for (var round = 0; round < 6; round++)
            {
                var regular = battle.Submit(AbilityId.BasicAttack);
                WarriorGoblinTests.AssertAbility(regular.Events[2], CombatantId.Vampire, AbilityId.BasicAttack);
            }

            var fury = battle.Submit(AbilityId.BasicAttack);
            var afterFury = battle.Submit(AbilityId.BasicAttack);

            WarriorGoblinTests.AssertAbility(fury.Events[2], CombatantId.Vampire, AbilityId.Fury);
            WarriorGoblinTests.AssertDamage(fury.Events[3], CombatantId.Mage, DamageKind.Fury, 200, 150);
            WarriorGoblinTests.AssertAbility(afterFury.Events[2], CombatantId.Vampire, AbilityId.BasicAttack);
            Assert.That(battle.State.Hero.CurrentHealth, Is.EqualTo(125));
            Assert.That(battle.State.Opponent.HasRaged, Is.True);
            Assert.That(random.Remaining, Is.Zero);
        }

        [Test]
        public void RevertTurnRestoresTheFuryFlag()
        {
            var random = new ScriptedRandomSource(0, 0, 0, 30, 30, 30, 30, 30, 30);
            var battle = StartFuryCampaign(random, AbilityId.BasicAttack, AbilityId.RevertTurn);
            for (var round = 0; round < 7; round++)
            {
                battle.Submit(AbilityId.BasicAttack);
            }

            battle.Submit(AbilityId.RevertTurn);

            Assert.That(battle.State.Opponent.HasRaged, Is.False);
            Assert.That(battle.State.Hero.CurrentHealth, Is.EqualTo(350));

            var furyAgain = battle.Submit(AbilityId.BasicAttack);

            WarriorGoblinTests.AssertAbility(furyAgain.Events[2], CombatantId.Vampire, AbilityId.Fury);
            Assert.That(random.Remaining, Is.Zero);
        }

        [Test]
        public void CampaignFollowsTheFixedOrderAndCarriesTheHero()
        {
            var campaign = TestContent.Campaign(TestContent.Goblin(), TestContent.Enemy(CombatantId.Skeleton, 62, 20, 5));
            var run = new CampaignRun(TestContent.Mage(), campaign, new ScriptedRandomSource(0, 0, 0, 15, 0, 0, 0));
            run.CurrentBattle.Submit(AbilityId.BasicAttack);

            Assert.That(run.ConcludeEncounter(), Is.False);

            run.CurrentBattle.Submit(AbilityId.BasicAttack);

            Assert.That(run.ConcludeEncounter(), Is.True);
            Assert.That(run.Phase, Is.EqualTo(CampaignPhase.Battle));
            Assert.That(run.EncounterIndex, Is.EqualTo(1));
            Assert.That(run.EncounterCount, Is.EqualTo(2));
            Assert.That(run.CurrentBattle.State.Opponent.Id, Is.EqualTo(CombatantId.Skeleton));
            Assert.That(run.CurrentBattle.State.Hero, Is.SameAs(run.Hero));
            Assert.That(run.Hero.CurrentHealth, Is.EqualTo(100));
            Assert.That(run.CurrentBattle.State.RevertCharges, Is.EqualTo(BattleState.RevertChargesPerEncounter));
        }

        [Test]
        public void CampaignCompletesAfterTheLastVictory()
        {
            var run = new CampaignRun(
                TestContent.Mage(),
                TestContent.Campaign(TestContent.Goblin()),
                new ScriptedRandomSource(0, 0, 0, 15));
            run.CurrentBattle.Submit(AbilityId.BasicAttack);
            run.CurrentBattle.Submit(AbilityId.BasicAttack);

            Assert.That(run.ConcludeEncounter(), Is.True);
            Assert.That(run.Phase, Is.EqualTo(CampaignPhase.Completed));
            Assert.That(run.ConcludeEncounter(), Is.False);
        }

        [Test]
        public void CampaignFailsWhenTheHeroFalls()
        {
            var mage = TestContent.Hero(CombatantId.Mage, new CombatantStats(10, 30, 5), AbilityId.BasicAttack);
            var run = new CampaignRun(mage, TestContent.Campaign(TestContent.Goblin()), new ScriptedRandomSource(0, 0, 0, 15));
            run.CurrentBattle.Submit(AbilityId.BasicAttack);

            Assert.That(run.ConcludeEncounter(), Is.True);
            Assert.That(run.Phase, Is.EqualTo(CampaignPhase.Failed));
        }

        [Test]
        public void CampaignRequiresAPlayerHero()
        {
            var campaign = TestContent.Campaign(TestContent.Enemy(CombatantId.Skeleton, 62, 20, 5));

            Assert.Throws<ArgumentException>(
                () => new CampaignRun(TestContent.Goblin(), campaign, new ScriptedRandomSource(0, 0, 0)));
        }

        private static IBattle StartFuryCampaign(IRandomSource random, params AbilityId[] heroAbilities)
        {
            var hero = TestContent.Hero(CombatantId.Mage, new CombatantStats(500, 10, 5), heroAbilities);
            var vampire = TestContent.Enemy(CombatantId.Vampire, 100, 34, 12);
            return new CampaignRun(hero, TestContent.Campaign(vampire), random).CurrentBattle;
        }
    }
}
