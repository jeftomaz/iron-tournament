using System;
using NUnit.Framework;

namespace IronTournament.Core.Tests
{
    public sealed class ProgressTests
    {
        [Test]
        public void CompletedCampaignRecordsTheFinalSnapshotWithUpgrades()
        {
            var run = CompleteWithAttackUpgrade(10);

            Assert.That(run.Phase, Is.EqualTo(CampaignPhase.Completed));
            Assert.That(run.FinalSnapshot.HeroClass, Is.EqualTo(CombatantId.Mage));
            Assert.That(run.FinalSnapshot.DisplayName, Is.EqualTo("Mage"));
            Assert.That(run.FinalSnapshot.Stats, Is.EqualTo(new CombatantStats(110, 40, 5)));
        }

        [Test]
        public void FinalSnapshotKeepsStatsWithinTheSaveLimit()
        {
            var run = CompleteWithAttackUpgrade(20000);

            Assert.That(run.Hero.Stats.Attack, Is.EqualTo(20030));
            Assert.That(run.FinalSnapshot.Stats.Attack, Is.EqualTo(HeroSnapshot.MaximumStatValue));
        }

        [Test]
        public void EnemyModeUnlocksOnlyAfterBothClassesClearTheCampaign()
        {
            var progress = new ProgressData();
            var afterMage = progress.WithCompletedCampaign(CompletedRun(TestContent.Mage()));
            var afterBoth = afterMage.WithCompletedCampaign(CompletedRun(TestContent.Warrior(), 50, 50));

            Assert.That(progress.IsCleared(CombatantId.Mage), Is.False);
            Assert.That(progress.IsEnemyModeUnlocked, Is.False);
            Assert.That(afterMage.IsCleared(CombatantId.Mage), Is.True);
            Assert.That(afterMage.IsEnemyModeUnlocked, Is.False);
            Assert.That(afterBoth.IsEnemyModeUnlocked, Is.True);
            Assert.That(afterBoth.ClearedHeroes.Count, Is.EqualTo(2));
        }

        [Test]
        public void NewerCompletionReplacesTheClassSnapshot()
        {
            var progress = new ProgressData(new[]
            {
                new HeroSnapshot(CombatantId.Mage, "Mago", new CombatantStats(100, 10, 1))
            });

            var updated = progress.WithCompletedCampaign(CompletedRun(TestContent.Mage()));

            Assert.That(updated.ClearedHeroes.Count, Is.EqualTo(1));
            Assert.That(updated.FinalSnapshot(CombatantId.Mage).Stats, Is.EqualTo(new CombatantStats(110, 30, 5)));
        }

        [Test]
        public void OnlyCompletedCampaignsAreRecorded()
        {
            var progress = new ProgressData();
            var running = new CampaignRun(
                TestContent.Mage(),
                TestContent.Campaign(TestContent.Goblin()),
                new ScriptedRandomSource(0, 0, 0));
            var weakMage = TestContent.Hero(CombatantId.Mage, new CombatantStats(10, 30, 5), AbilityId.BasicAttack);
            var failed = new CampaignRun(
                weakMage,
                TestContent.Campaign(TestContent.Goblin()),
                new ScriptedRandomSource(0, 0, 0, 15));
            failed.CurrentBattle.Submit(AbilityId.BasicAttack);
            failed.ConcludeEncounter();

            Assert.That(failed.Phase, Is.EqualTo(CampaignPhase.Failed));
            Assert.Throws<ArgumentException>(() => progress.WithCompletedCampaign(running));
            Assert.Throws<ArgumentException>(() => progress.WithCompletedCampaign(failed));
            Assert.Throws<ArgumentNullException>(() => progress.WithCompletedCampaign(null));
        }

        [Test]
        public void ProgressRejectsInvalidSnapshots()
        {
            var stats = new CombatantStats(110, 30, 5);
            var mage = new HeroSnapshot(CombatantId.Mage, "Mago", stats);

            Assert.Throws<ArgumentOutOfRangeException>(() => new HeroSnapshot(CombatantId.Goblin, "Goblin", stats));
            Assert.Throws<ArgumentOutOfRangeException>(() => new HeroSnapshot(CombatantId.Mage, "Mago", default));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new HeroSnapshot(CombatantId.Mage, "Mago", new CombatantStats(HeroSnapshot.MaximumStatValue + 1, 30, 5)));
            Assert.Throws<ArgumentException>(() => new HeroSnapshot(CombatantId.Mage, "<b>Mago</b>", stats));
            Assert.Throws<ArgumentException>(() => new ProgressData(new[] { mage, mage }));
            Assert.Throws<ArgumentException>(() => new ProgressData(new HeroSnapshot[] { null }));
        }

        [Test]
        public void CampaignHeroMustBeAHeroClass()
        {
            var goblin = TestContent.Hero(CombatantId.Goblin, new CombatantStats(45, 15, 3), AbilityId.BasicAttack);
            var campaign = TestContent.Campaign(TestContent.Enemy(CombatantId.Skeleton, 62, 20, 5));

            Assert.Throws<ArgumentException>(() => new CampaignRun(goblin, campaign, new ScriptedRandomSource(0, 0, 0)));
        }

        private static CampaignRun CompleteWithAttackUpgrade(int amount)
        {
            var campaign = TestContent.CampaignWithDrops(
                new[] { TestContent.Item(ItemId.AttackGem, 3, new ItemModifier(ItemModifierKind.Attack, amount)) },
                TestContent.Enemy(CombatantId.Goblin, 1, 15, 3),
                TestContent.Enemy(CombatantId.Skeleton, 1, 20, 0));
            var run = new CampaignRun(TestContent.Mage(), campaign, new ScriptedRandomSource(0, 0, 0, 0, 0, 0, 0));
            run.CurrentBattle.Submit(AbilityId.BasicAttack);
            run.ConcludeEncounter();
            run.ChooseDrop(ItemId.AttackGem);
            Assert.That(run.FinalSnapshot, Is.Null);

            run.CurrentBattle.Submit(AbilityId.BasicAttack);
            run.ConcludeEncounter();
            return run;
        }

        private static CampaignRun CompletedRun(CombatantConfiguration hero, params int[] heroRolls)
        {
            var rolls = new int[3 + heroRolls.Length];
            heroRolls.CopyTo(rolls, 3);
            var run = new CampaignRun(
                hero,
                TestContent.Campaign(TestContent.Enemy(CombatantId.Goblin, 1, 15, 3)),
                new ScriptedRandomSource(rolls));
            run.CurrentBattle.Submit(AbilityId.BasicAttack);
            run.ConcludeEncounter();
            return run;
        }
    }
}
