using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace IronTournament.Core.Tests
{
    public sealed class BattleContractTests
    {
        [Test]
        public void BattleStartsOnThePlayerTurnWithFullHealth()
        {
            var state = new BattleState(Combatant(CombatantId.Warrior, CombatantSide.Player), Goblin());

            Assert.That(state.Phase, Is.EqualTo(BattlePhase.PlayerTurn));
            Assert.That(state.IsOver, Is.False);
            Assert.That(state.Hero.CurrentHealth, Is.EqualTo(state.Hero.Stats.MaximumHealth));
            Assert.That(state.Opponent.CurrentHealth, Is.EqualTo(state.Opponent.Stats.MaximumHealth));
            Assert.That(state.Opponent.IsDefeated, Is.False);
        }

        [Test]
        public void BattleRejectsSwappedSides()
        {
            var hero = Combatant(CombatantId.Warrior, CombatantSide.Player);

            Assert.Throws<ArgumentException>(() => new BattleState(Goblin(), hero));
        }

        [Test]
        public void EncounterFactoryCreatesAnActiveBattleWithEncounterVariation()
        {
            var hero = Configuration(CombatantId.Warrior, CombatantSide.Player);
            var opponent = Configuration(CombatantId.DemonKing, CombatantSide.Enemy);
            var encounter = new EncounterConfiguration(opponent,
                EncounterConfiguration.RequiredEnemyStatVariancePercent,
                new List<ItemConfiguration>());

            var battle = EncounterBattleFactory.Create(hero, encounter, new SeededRandomSource(17));

            Assert.That(battle.State.Phase, Is.EqualTo(BattlePhase.PlayerTurn));
            Assert.That(battle.State.Hero.Stats, Is.EqualTo(hero.BaseStats));
            Assert.That(battle.State.Opponent.Stats.MaximumHealth, Is.InRange(38, 52));
            Assert.That(battle.State.Opponent.CanRage, Is.True);
        }

        [Test]
        public void CombatantRejectsUninitializedStats()
        {
            var configuration = Configuration(CombatantId.Goblin, CombatantSide.Enemy);

            Assert.Throws<ArgumentOutOfRangeException>(() => new CombatantState(configuration, default));
        }

        [Test]
        public void AcceptedResultKeepsAnIsolatedCopyOfTheEvents()
        {
            var events = new List<BattleEvent>
            {
                new AbilityUsedEvent(CombatantId.Warrior, AbilityId.BasicAttack)
            };

            var result = ActionResult.Accepted(events);
            events.Clear();

            Assert.That(result.IsAccepted, Is.True);
            Assert.That(result.Events.Count, Is.EqualTo(1));
        }

        [Test]
        public void AcceptedResultRequiresEvents()
        {
            Assert.Throws<ArgumentException>(() => ActionResult.Accepted(new List<BattleEvent>()));
            Assert.Throws<ArgumentException>(() => ActionResult.Accepted(new List<BattleEvent> { null }));
        }

        [Test]
        public void RejectedResultRequiresAReasonAndHasNoEvents()
        {
            var result = ActionResult.Rejected(ActionRejection.BattleOver);

            Assert.That(result.IsAccepted, Is.False);
            Assert.That(result.Events, Is.Empty);
            Assert.Throws<ArgumentOutOfRangeException>(() => ActionResult.Rejected(ActionRejection.None));
            Assert.Throws<ArgumentOutOfRangeException>(() => ActionResult.Rejected((ActionRejection)99));
        }

        [Test]
        public void EventsRejectInvalidValues()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new AbilityUsedEvent(CombatantId.None, AbilityId.BasicAttack));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new AbilityUsedEvent(CombatantId.Warrior, (AbilityId)99));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new DamageDealtEvent(CombatantId.Warrior, CombatantId.Goblin, DamageKind.Attack, -1, 10, false, false));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new DamageDealtEvent(CombatantId.Warrior, CombatantId.Goblin, DamageKind.Attack, 5, -1, false, false));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new DamageDealtEvent(CombatantId.Warrior, CombatantId.Goblin, DamageKind.None, 5, 10, false, false));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BattleEndedEvent(BattlePhase.PlayerTurn));
        }

        private static CombatantState Goblin()
        {
            return Combatant(CombatantId.Goblin, CombatantSide.Enemy);
        }

        private static CombatantState Combatant(CombatantId id, CombatantSide side)
        {
            return new CombatantState(Configuration(id, side), new CombatantStats(45, 15, 3));
        }

        private static CombatantConfiguration Configuration(CombatantId id, CombatantSide side)
        {
            var attack = new AbilityConfiguration(AbilityId.BasicAttack, "Atacar", AbilityTarget.Opponent, true);
            return new CombatantConfiguration(
                id,
                side,
                id.ToString(),
                new CombatantStats(45, 15, 3),
                new[] { attack });
        }
    }
}
