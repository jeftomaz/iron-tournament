using NUnit.Framework;

namespace IronTournament.Core.Tests
{
    public sealed class WarriorGoblinTests
    {
        [Test]
        public void AttackSubtractsDefenseAndTheGoblinResponds()
        {
            var random = new ScriptedRandomSource(50, 50);
            var hero = TestContent.Hero(CombatantId.Warrior, new CombatantStats(120, 30, 5), AbilityId.BasicAttack);
            var battle = TestContent.StartBattle(hero, TestContent.Goblin(), random);

            var result = battle.Submit(AbilityId.BasicAttack);

            Assert.That(result.IsAccepted, Is.True);
            Assert.That(result.Events.Count, Is.EqualTo(4));
            AssertAbility(result.Events[0], CombatantId.Warrior, AbilityId.BasicAttack);
            AssertDamage(result.Events[1], CombatantId.Goblin, DamageKind.Attack, 27, 18);
            AssertAbility(result.Events[2], CombatantId.Goblin, AbilityId.BasicAttack);
            AssertDamage(result.Events[3], CombatantId.Warrior, DamageKind.Attack, 10, 110);
            Assert.That(battle.State.Phase, Is.EqualTo(BattlePhase.PlayerTurn));
            Assert.That(battle.State.Round, Is.EqualTo(2));
            Assert.That(random.Remaining, Is.Zero);
        }

        [TestCase(9, 50, 30, false, true)]
        [TestCase(10, 50, 27, false, false)]
        [TestCase(50, 6, 54, true, false)]
        [TestCase(50, 7, 27, false, false)]
        [TestCase(0, 0, 60, true, true)]
        public void WarriorStrikeRollsPiercingThenCritical(
            int piercingRoll,
            int criticalRoll,
            int expectedDamage,
            bool isCritical,
            bool isPiercing)
        {
            var opponent = TestContent.Enemy(CombatantId.Goblin, 200, 15, 3);
            var battle = TestContent.StartBattle(
                TestContent.Warrior(),
                opponent,
                new ScriptedRandomSource(piercingRoll, criticalRoll));

            var result = battle.Submit(AbilityId.BasicAttack);

            var hit = (DamageDealtEvent)result.Events[1];
            Assert.That(hit.Amount, Is.EqualTo(expectedDamage));
            Assert.That(hit.IsCritical, Is.EqualTo(isCritical));
            Assert.That(hit.IsPiercing, Is.EqualTo(isPiercing));
            Assert.That(battle.State.Opponent.CurrentHealth, Is.EqualTo(200 - expectedDamage));
        }

        [Test]
        public void FinishingBlowEndsTheBattleBeforeTheGoblinActs()
        {
            var battle = TestContent.StartBattle(
                TestContent.Warrior(),
                TestContent.Goblin(),
                new ScriptedRandomSource(50, 3));

            var result = battle.Submit(AbilityId.BasicAttack);

            Assert.That(result.Events.Count, Is.EqualTo(3));
            AssertDamage(result.Events[1], CombatantId.Goblin, DamageKind.Attack, 54, 0);
            Assert.That(((BattleEndedEvent)result.Events[2]).Result, Is.EqualTo(BattlePhase.Victory));
            Assert.That(battle.State.Phase, Is.EqualTo(BattlePhase.Victory));
            Assert.That(battle.State.Hero.CurrentHealth, Is.EqualTo(120));
            Assert.That(battle.AvailableActions, Is.Empty);
            Assert.That(battle.Submit(AbilityId.BasicAttack).Rejection, Is.EqualTo(ActionRejection.BattleOver));
        }

        [Test]
        public void GuardDoublesDefenseForTheRoundAndReflectsDamage()
        {
            var random = new ScriptedRandomSource();
            var opponent = TestContent.Enemy(CombatantId.Goblin, 45, 50, 3);
            var battle = TestContent.StartBattle(TestContent.Warrior(), opponent, random);

            var result = battle.Submit(AbilityId.Guard);

            AssertAbility(result.Events[0], CombatantId.Warrior, AbilityId.Guard);
            AssertDamage(result.Events[1], CombatantId.Goblin, DamageKind.Reflection, 7, 38);
            AssertDamage(result.Events[3], CombatantId.Warrior, DamageKind.Attack, 4, 116);
            Assert.That(battle.State.Hero.GuardBonus, Is.Zero);
            Assert.That(battle.State.Hero.Defense, Is.EqualTo(23));
        }

        [Test]
        public void ReflectionDealsAtLeastOneDamage()
        {
            var hero = TestContent.Hero(CombatantId.Warrior, new CombatantStats(120, 30, 0), AbilityId.Guard);
            var battle = TestContent.StartBattle(hero, TestContent.Goblin(), new ScriptedRandomSource());

            var result = battle.Submit(AbilityId.Guard);

            AssertDamage(result.Events[1], CombatantId.Goblin, DamageKind.Reflection, 1, 44);
        }

        [Test]
        public void ReflectionCanDefeatTheGoblin()
        {
            var opponent = TestContent.Enemy(CombatantId.Goblin, 5, 15, 3);
            var battle = TestContent.StartBattle(TestContent.Warrior(), opponent, new ScriptedRandomSource());

            battle.Submit(AbilityId.Guard);

            Assert.That(battle.State.Phase, Is.EqualTo(BattlePhase.Victory));
            Assert.That(battle.State.Hero.GuardBonus, Is.Zero);
        }

        [Test]
        public void GoblinCanDefeatTheHero()
        {
            var hero = TestContent.Hero(CombatantId.Warrior, new CombatantStats(10, 1, 0), AbilityId.BasicAttack);
            var battle = TestContent.StartBattle(hero, TestContent.Goblin(), new ScriptedRandomSource(50, 50));

            var result = battle.Submit(AbilityId.BasicAttack);

            AssertDamage(result.Events[1], CombatantId.Goblin, DamageKind.Attack, 0, 45);
            AssertDamage(result.Events[3], CombatantId.Warrior, DamageKind.Attack, 15, 0);
            Assert.That(((BattleEndedEvent)result.Events[4]).Result, Is.EqualTo(BattlePhase.Defeat));
            Assert.That(battle.State.Phase, Is.EqualTo(BattlePhase.Defeat));
            Assert.That(battle.Submit(AbilityId.BasicAttack).Rejection, Is.EqualTo(ActionRejection.BattleOver));
        }

        [Test]
        public void RejectsActionsTheHeroCannotUse()
        {
            var random = new ScriptedRandomSource();
            var battle = TestContent.StartBattle(TestContent.Warrior(), TestContent.Goblin(), random);

            Assert.That(battle.AvailableActions, Is.EqualTo(new[] { AbilityId.BasicAttack, AbilityId.Guard }));
            Assert.That(battle.Submit(AbilityId.RevertTurn).Rejection, Is.EqualTo(ActionRejection.UnavailableAction));
            Assert.That(battle.Submit(AbilityId.None).Rejection, Is.EqualTo(ActionRejection.UnavailableAction));
            Assert.That(battle.Submit((AbilityId)99).Rejection, Is.EqualTo(ActionRejection.UnavailableAction));
            Assert.That(battle.State.Round, Is.EqualTo(1));
            Assert.That(battle.State.Opponent.CurrentHealth, Is.EqualTo(45));
        }

        internal static void AssertAbility(BattleEvent battleEvent, CombatantId actor, AbilityId ability)
        {
            var used = (AbilityUsedEvent)battleEvent;
            Assert.That(used.Actor, Is.EqualTo(actor));
            Assert.That(used.Ability, Is.EqualTo(ability));
        }

        internal static void AssertDamage(
            BattleEvent battleEvent,
            CombatantId target,
            DamageKind kind,
            int amount,
            int remainingHealth)
        {
            var damage = (DamageDealtEvent)battleEvent;
            Assert.That(damage.Target, Is.EqualTo(target));
            Assert.That(damage.Kind, Is.EqualTo(kind));
            Assert.That(damage.Amount, Is.EqualTo(amount));
            Assert.That(damage.RemainingHealth, Is.EqualTo(remainingHealth));
        }
    }
}
