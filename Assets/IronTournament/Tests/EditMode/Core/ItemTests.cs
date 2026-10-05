using NUnit.Framework;

namespace IronTournament.Core.Tests
{
    public sealed class ItemTests
    {
        [Test]
        public void DropOfferIsWeightedDistinctAndFilteredByClass()
        {
            var random = new ScriptedRandomSource(0, 0, 0, 50, 50, 0, 11);

            var run = ReachDropChoice(
                TestContent.Warrior(),
                WeakGoblin(),
                Skeleton(62, 20),
                random,
                TestContent.Item(ItemId.HealingPotion, 3),
                TestContent.Item(ItemId.AttackGem, 3),
                TestContent.Item(ItemId.DefenseRune, 3),
                TestContent.Item(ItemId.PowerCrystal, 2),
                TestContent.Item(ItemId.LifeElixir, 1),
                TestContent.Item(ItemId.DivineDefense, 1),
                TestContent.Item(ItemId.HopeScroll, 2),
                TestContent.Item(ItemId.FlameCloak, 2),
                TestContent.Item(ItemId.BrotherhoodHorn, 2));

            Assert.That(run.Phase, Is.EqualTo(CampaignPhase.DropChoice));
            Assert.That(run.DropOffer.Count, Is.EqualTo(CampaignRun.DropOfferSize));
            Assert.That(run.DropOffer[0].Id, Is.EqualTo(ItemId.HealingPotion));
            Assert.That(run.DropOffer[1].Id, Is.EqualTo(ItemId.HopeScroll));
            Assert.That(random.Remaining, Is.Zero);
        }

        [Test]
        public void ChooseDropAcceptsOnlyOfferedItems()
        {
            var run = ReachDropChoice(
                TestContent.Mage(),
                WeakGoblin(),
                Skeleton(62, 20),
                new ScriptedRandomSource(0, 0, 0, 1, 0, 0, 0),
                TestContent.Item(ItemId.HopeScroll, 2),
                TestContent.Item(ItemId.FlameCloak, 2));

            Assert.That(run.DropOffer.Count, Is.EqualTo(1));
            Assert.That(run.ConcludeEncounter(), Is.False);
            Assert.That(run.ChooseDrop(ItemId.HopeScroll), Is.False);
            Assert.That(run.ChooseDrop(ItemId.None), Is.False);
            Assert.That(run.ChooseDrop((ItemId)99), Is.False);
            Assert.That(run.Phase, Is.EqualTo(CampaignPhase.DropChoice));

            Assert.That(run.ChooseDrop(ItemId.FlameCloak), Is.True);
            Assert.That(run.Phase, Is.EqualTo(CampaignPhase.Battle));
            Assert.That(run.EncounterIndex, Is.EqualTo(1));
            Assert.That(run.ObtainedItems, Is.EqualTo(new[] { ItemId.FlameCloak }));
            Assert.That(run.DropOffer, Is.Empty);
            Assert.That(run.Hero.HasFlameCloak, Is.True);
            Assert.That(run.ChooseDrop(ItemId.FlameCloak), Is.False);
        }

        [Test]
        public void FlameCloakIsNotOfferedAgainOnceOwned()
        {
            var random = new ScriptedRandomSource(0, 0, 0, 0, 0, 0, 0, 0, 0);
            var campaign = TestContent.CampaignWithDrops(
                new[] { TestContent.Item(ItemId.FlameCloak, 2), TestContent.Item(ItemId.HealingPotion, 3) },
                WeakGoblin(),
                TestContent.Enemy(CombatantId.Skeleton, 1, 20, 0),
                TestContent.Enemy(CombatantId.Knight, 1, 26, 0));
            var run = new CampaignRun(TestContent.Mage(), campaign, random);
            run.CurrentBattle.Submit(AbilityId.BasicAttack);
            run.ConcludeEncounter();
            run.ChooseDrop(ItemId.FlameCloak);
            run.CurrentBattle.Submit(AbilityId.BasicAttack);

            run.ConcludeEncounter();

            Assert.That(run.DropOffer.Count, Is.EqualTo(1));
            Assert.That(run.DropOffer[0].Id, Is.EqualTo(ItemId.HealingPotion));
            Assert.That(random.Remaining, Is.Zero);
        }

        [Test]
        public void PermanentUpgradesApplyTheirModifiers()
        {
            var run = ReachDropChoice(
                TestContent.Mage(),
                WeakGoblin(),
                Skeleton(62, 20),
                new ScriptedRandomSource(0, 0, 0, 0, 0, 0, 0, 0),
                TestContent.Item(ItemId.AttackGem, 3, new ItemModifier(ItemModifierKind.Attack, 10)),
                TestContent.Item(ItemId.DivineDefense, 1, new ItemModifier(ItemModifierKind.Defense, 15)));

            run.ChooseDrop(ItemId.AttackGem);

            Assert.That(run.Hero.Stats.Attack, Is.EqualTo(40));
            Assert.That(run.Hero.Stats.Defense, Is.EqualTo(5));
            Assert.That(run.CurrentBattle.State.Hero.Stats.Attack, Is.EqualTo(40));
        }

        [TestCase(ItemId.HealingPotion, 95)]
        [TestCase(ItemId.LifeElixir, 110)]
        public void HealingItemsRestoreHealthUpToTheMaximum(ItemId item, int expectedHealth)
        {
            var random = new ScriptedRandomSource(0, 0, 0, 60, 0, 0, 0, 0);
            var campaign = TestContent.CampaignWithDrops(
                new[] { TestContent.Item(item, 1) },
                TestContent.Enemy(CombatantId.Goblin, 31, 60, 3),
                Skeleton(62, 20));
            var run = new CampaignRun(TestContent.Mage(), campaign, random);
            run.CurrentBattle.Submit(AbilityId.BasicAttack);
            run.CurrentBattle.Submit(AbilityId.BasicAttack);
            run.ConcludeEncounter();

            run.ChooseDrop(item);

            Assert.That(run.Hero.CurrentHealth, Is.EqualTo(expectedHealth));
            Assert.That(random.Remaining, Is.Zero);
        }

        [Test]
        public void HopeScrollHealsHalfOnlyAtThirtyPercentAndSpendsTheTurn()
        {
            var random = new ScriptedRandomSource(0, 0, 0, 50, 50, 0, 0, 0, 0, 50, 50, 85, 80);
            var warrior = TestContent.Hero(
                CombatantId.Warrior,
                new CombatantStats(120, 30, 0),
                AbilityId.BasicAttack,
                AbilityId.Guard,
                AbilityId.UseHopeScroll);
            var run = ReachDropChoice(warrior, WeakGoblin(), Skeleton(500, 100), random, TestContent.Item(ItemId.HopeScroll, 2));
            run.ChooseDrop(ItemId.HopeScroll);
            var battle = run.CurrentBattle;

            Assert.That(battle.AvailableActions, Is.EqualTo(new[] { AbilityId.BasicAttack, AbilityId.Guard }));

            battle.Submit(AbilityId.BasicAttack);

            Assert.That(battle.State.Hero.CurrentHealth, Is.EqualTo(35));
            Assert.That(
                battle.AvailableActions,
                Is.EqualTo(new[] { AbilityId.BasicAttack, AbilityId.Guard, AbilityId.UseHopeScroll }));

            var result = battle.Submit(AbilityId.UseHopeScroll);

            WarriorGoblinTests.AssertAbility(result.Events[0], CombatantId.Warrior, AbilityId.UseHopeScroll);
            var healed = (HealthRestoredEvent)result.Events[1];
            Assert.That(healed.Amount, Is.EqualTo(60));
            Assert.That(healed.CurrentHealth, Is.EqualTo(95));
            WarriorGoblinTests.AssertDamage(result.Events[3], CombatantId.Warrior, DamageKind.Attack, 80, 15);
            Assert.That(battle.State.Hero.HopeScrolls, Is.Zero);
            Assert.That(random.Remaining, Is.Zero);
        }

        [Test]
        public void FlameCloakBurnsAfterTheHeroActionAndAfterTheOpponentTurn()
        {
            var run = ReachDropChoice(
                TestContent.Mage(),
                WeakGoblin(),
                Skeleton(200, 20),
                new ScriptedRandomSource(0, 0, 0, 0, 0, 0, 0, 20),
                TestContent.Item(ItemId.FlameCloak, 2));
            run.ChooseDrop(ItemId.FlameCloak);

            var result = run.CurrentBattle.Submit(AbilityId.BasicAttack);

            Assert.That(result.Events.Count, Is.EqualTo(6));
            WarriorGoblinTests.AssertDamage(result.Events[1], CombatantId.Skeleton, DamageKind.Attack, 30, 170);
            WarriorGoblinTests.AssertDamage(result.Events[2], CombatantId.Skeleton, DamageKind.Burn, 5, 165);
            WarriorGoblinTests.AssertDamage(result.Events[4], CombatantId.Mage, DamageKind.Attack, 15, 95);
            WarriorGoblinTests.AssertDamage(result.Events[5], CombatantId.Skeleton, DamageKind.Burn, 5, 160);
        }

        [Test]
        public void BurnCanWinAfterTheOpponentActs()
        {
            var run = ReachDropChoice(
                TestContent.Mage(),
                WeakGoblin(),
                Skeleton(40, 20),
                new ScriptedRandomSource(0, 0, 0, 0, 0, 0, 0, 20),
                TestContent.Item(ItemId.FlameCloak, 2));
            run.ChooseDrop(ItemId.FlameCloak);

            var result = run.CurrentBattle.Submit(AbilityId.BasicAttack);

            WarriorGoblinTests.AssertDamage(result.Events[5], CombatantId.Skeleton, DamageKind.Burn, 5, 0);
            Assert.That(((BattleEndedEvent)result.Events[6]).Result, Is.EqualTo(BattlePhase.Victory));
        }

        [Test]
        public void GuardianInterceptsALethalHitAndLeavesTheMageAtOneHealth()
        {
            var random = new ScriptedRandomSource(0, 0, 0, 0, 0, 0, 0, 80, 80);
            var battle = StartGuardianBattle(Skeleton(500, 100), random);

            Assert.That(battle.AvailableActions, Is.EqualTo(new[] { AbilityId.BasicAttack, AbilityId.ArmGuardian }));

            battle.Submit(AbilityId.ArmGuardian);

            Assert.That(battle.State.Hero.IsGuardianArmed, Is.True);
            Assert.That(battle.State.Hero.GuardianHorns, Is.Zero);
            Assert.That(battle.State.Hero.CurrentHealth, Is.EqualTo(35));

            var result = battle.Submit(AbilityId.BasicAttack);

            var intercepted = (GuardianInterceptedEvent)result.Events[3];
            Assert.That(intercepted.ProtectedCombatant, Is.EqualTo(CombatantId.Mage));
            Assert.That(intercepted.PreventedDamage, Is.EqualTo(75));
            WarriorGoblinTests.AssertDamage(result.Events[4], CombatantId.Skeleton, DamageKind.GuardianStrike, 60, 410);
            WarriorGoblinTests.AssertDamage(result.Events[5], CombatantId.Mage, DamageKind.GuardianRecoil, 34, 1);
            Assert.That(battle.State.Hero.IsGuardianArmed, Is.False);
            Assert.That(battle.State.Phase, Is.EqualTo(BattlePhase.PlayerTurn));
            Assert.That(random.Remaining, Is.Zero);
        }

        [Test]
        public void GuardianStrikeThatWinsRestoresThirtyPercentHealth()
        {
            var battle = StartGuardianBattle(Skeleton(80, 100), new ScriptedRandomSource(0, 0, 0, 0, 0, 0, 0, 100, 80));
            battle.Submit(AbilityId.ArmGuardian);

            var result = battle.Submit(AbilityId.BasicAttack);

            WarriorGoblinTests.AssertDamage(result.Events[4], CombatantId.Skeleton, DamageKind.GuardianStrike, 60, 0);
            var healed = (HealthRestoredEvent)result.Events[5];
            Assert.That(healed.Amount, Is.EqualTo(18));
            Assert.That(healed.CurrentHealth, Is.EqualTo(33));
            Assert.That(((BattleEndedEvent)result.Events[6]).Result, Is.EqualTo(BattlePhase.Victory));
        }

        [Test]
        public void RevertTurnRestoresConsumedItemsAndArmedEffects()
        {
            var battle = StartGuardianBattle(Skeleton(500, 100), new ScriptedRandomSource(0, 0, 0, 0, 0, 0, 0, 80, 80));
            battle.Submit(AbilityId.ArmGuardian);
            battle.Submit(AbilityId.BasicAttack);

            battle.Submit(AbilityId.RevertTurn);

            Assert.That(battle.State.Hero.IsGuardianArmed, Is.True);
            Assert.That(battle.State.Hero.CurrentHealth, Is.EqualTo(35));
            Assert.That(battle.State.Opponent.CurrentHealth, Is.EqualTo(500));
        }

        [Test]
        public void RevertBattleReturnsTheUnusedHorn()
        {
            var battle = StartGuardianBattle(Skeleton(500, 100), new ScriptedRandomSource(0, 0, 0, 0, 0, 0, 0, 80));
            battle.Submit(AbilityId.ArmGuardian);

            battle.Submit(AbilityId.RevertBattle);

            Assert.That(battle.State.Hero.GuardianHorns, Is.EqualTo(1));
            Assert.That(battle.State.Hero.IsGuardianArmed, Is.False);
            Assert.That(battle.State.Hero.CurrentHealth, Is.EqualTo(110));
        }

        private static IBattle StartGuardianBattle(CombatantConfiguration opponent, IRandomSource random)
        {
            var run = ReachDropChoice(
                TestContent.Mage(),
                WeakGoblin(),
                opponent,
                random,
                TestContent.Item(ItemId.BrotherhoodHorn, 2));
            run.ChooseDrop(ItemId.BrotherhoodHorn);
            return run.CurrentBattle;
        }

        private static CampaignRun ReachDropChoice(
            CombatantConfiguration hero,
            CombatantConfiguration firstOpponent,
            CombatantConfiguration nextOpponent,
            IRandomSource random,
            params ItemConfiguration[] dropPool)
        {
            var campaign = TestContent.CampaignWithDrops(dropPool, firstOpponent, nextOpponent);
            var run = new CampaignRun(hero, campaign, random);
            run.CurrentBattle.Submit(AbilityId.BasicAttack);
            run.ConcludeEncounter();
            return run;
        }

        private static CombatantConfiguration WeakGoblin()
        {
            return TestContent.Enemy(CombatantId.Goblin, 1, 15, 3);
        }

        private static CombatantConfiguration Skeleton(int health, int attack)
        {
            return TestContent.Enemy(CombatantId.Skeleton, health, attack, 0);
        }
    }
}
