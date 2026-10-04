using NUnit.Framework;

namespace IronTournament.Core.Tests
{
    public sealed class MageRevertTests
    {
        [Test]
        public void MageAttackIgnoresDefenseWithoutRolling()
        {
            var random = new ScriptedRandomSource();
            var battle = TestContent.StartBattle(TestContent.Mage(), TestContent.Goblin(), random);

            var result = battle.Submit(AbilityId.BasicAttack);

            WarriorGoblinTests.AssertDamage(result.Events[1], CombatantId.Goblin, DamageKind.Attack, 30, 15);
            WarriorGoblinTests.AssertDamage(result.Events[3], CombatantId.Mage, DamageKind.Attack, 10, 100);
            Assert.That(((DamageDealtEvent)result.Events[1]).IsPiercing, Is.False);
            Assert.That(random.Remaining, Is.Zero);
        }

        [Test]
        public void RevertTurnUndoesTheAttackAndTheResponseAndReturnsControlToTheMage()
        {
            var battle = TestContent.StartBattle(TestContent.Mage(), TestContent.Goblin(), new ScriptedRandomSource());
            var state = battle.State;
            battle.Submit(AbilityId.BasicAttack);

            var result = battle.Submit(AbilityId.RevertTurn);

            Assert.That(result.IsAccepted, Is.True);
            Assert.That(result.Events.Count, Is.EqualTo(1));
            WarriorGoblinTests.AssertAbility(result.Events[0], CombatantId.Mage, AbilityId.RevertTurn);
            Assert.That(battle.State, Is.SameAs(state));
            Assert.That(state.Hero.CurrentHealth, Is.EqualTo(110));
            Assert.That(state.Opponent.CurrentHealth, Is.EqualTo(45));
            Assert.That(state.Round, Is.EqualTo(1));
            Assert.That(state.Phase, Is.EqualTo(BattlePhase.PlayerTurn));
            Assert.That(state.RevertCharges, Is.Zero);
            Assert.That(battle.AvailableActions, Is.EqualTo(new[] { AbilityId.BasicAttack }));

            var newChoice = battle.Submit(AbilityId.BasicAttack);

            Assert.That(newChoice.IsAccepted, Is.True);
            Assert.That(state.Opponent.CurrentHealth, Is.EqualTo(15));
        }

        [Test]
        public void RevertBattleRestoresTheEncounterEntry()
        {
            var opponent = TestContent.Enemy(CombatantId.Goblin, 200, 15, 3);
            var battle = TestContent.StartBattle(TestContent.Mage(), opponent, new ScriptedRandomSource());
            battle.Submit(AbilityId.BasicAttack);
            battle.Submit(AbilityId.BasicAttack);

            battle.Submit(AbilityId.RevertBattle);

            Assert.That(battle.State.Hero.CurrentHealth, Is.EqualTo(110));
            Assert.That(battle.State.Opponent.CurrentHealth, Is.EqualTo(200));
            Assert.That(battle.State.Round, Is.EqualTo(1));
            Assert.That(battle.State.RevertCharges, Is.Zero);
        }

        [Test]
        public void ReversionsNeedAPreviousTurnAndShareOneChargePerEncounter()
        {
            var opponent = TestContent.Enemy(CombatantId.Goblin, 200, 15, 3);
            var battle = TestContent.StartBattle(TestContent.Mage(), opponent, new ScriptedRandomSource());

            Assert.That(battle.State.RevertCharges, Is.EqualTo(BattleState.RevertChargesPerEncounter));
            Assert.That(battle.AvailableActions, Is.EqualTo(new[] { AbilityId.BasicAttack }));
            Assert.That(battle.Submit(AbilityId.RevertBattle).Rejection, Is.EqualTo(ActionRejection.UnavailableAction));

            battle.Submit(AbilityId.BasicAttack);
            Assert.That(
                battle.AvailableActions,
                Is.EqualTo(new[] { AbilityId.BasicAttack, AbilityId.RevertTurn, AbilityId.RevertBattle }));

            battle.Submit(AbilityId.RevertTurn);
            battle.Submit(AbilityId.BasicAttack);

            Assert.That(battle.AvailableActions, Is.EqualTo(new[] { AbilityId.BasicAttack }));
            Assert.That(battle.Submit(AbilityId.RevertBattle).Rejection, Is.EqualTo(ActionRejection.UnavailableAction));
        }

        [Test]
        public void ReversionDoesNotRewindTheRandomSequence()
        {
            var random = new ScriptedRandomSource(50, 50, 9, 50);
            var hero = TestContent.Hero(
                CombatantId.Warrior,
                new CombatantStats(120, 30, 23),
                AbilityId.BasicAttack,
                AbilityId.RevertTurn);
            var opponent = TestContent.Enemy(CombatantId.Goblin, 200, 15, 3);
            var battle = TestContent.StartBattle(hero, opponent, random);

            var first = battle.Submit(AbilityId.BasicAttack);
            battle.Submit(AbilityId.RevertTurn);
            var second = battle.Submit(AbilityId.BasicAttack);

            Assert.That(((DamageDealtEvent)first.Events[1]).Amount, Is.EqualTo(27));
            Assert.That(((DamageDealtEvent)second.Events[1]).Amount, Is.EqualTo(30));
            Assert.That(((DamageDealtEvent)second.Events[1]).IsPiercing, Is.True);
            Assert.That(battle.State.Opponent.CurrentHealth, Is.EqualTo(170));
            Assert.That(random.Remaining, Is.Zero);
        }
    }
}
