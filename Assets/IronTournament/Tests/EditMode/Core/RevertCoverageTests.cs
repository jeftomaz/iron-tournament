using NUnit.Framework;

namespace IronTournament.Core.Tests
{
    public sealed class RevertCoverageTests
    {
        [Test]
        public void RevertBattleRestoresEveryTrackedValueExceptTheCharge()
        {
            var opponent = TestContent.Enemy(CombatantId.Goblin, 200, 15, 3);
            var battle = TestContent.StartBattle(TestContent.Mage(), opponent, new ScriptedRandomSource());
            var heroStats = battle.State.Hero.Stats;
            var opponentStats = battle.State.Opponent.Stats;
            battle.Submit(AbilityId.BasicAttack);
            battle.Submit(AbilityId.BasicAttack);
            battle.Submit(AbilityId.BasicAttack);

            battle.Submit(AbilityId.RevertBattle);

            var state = battle.State;
            Assert.That(state.Hero.CurrentHealth, Is.EqualTo(110));
            Assert.That(state.Opponent.CurrentHealth, Is.EqualTo(200));
            Assert.That(state.Hero.Stats, Is.EqualTo(heroStats));
            Assert.That(state.Opponent.Stats, Is.EqualTo(opponentStats));
            Assert.That(state.Hero.GuardBonus, Is.Zero);
            Assert.That(state.Round, Is.EqualTo(1));
            Assert.That(state.Phase, Is.EqualTo(BattlePhase.PlayerTurn));
            Assert.That(state.RevertCharges, Is.Zero);
        }

        [Test]
        public void RevertTurnUndoesTheEffectsOfTheLastRoundOnly()
        {
            var hero = TestContent.Hero(
                CombatantId.Warrior,
                new CombatantStats(120, 30, 23),
                AbilityId.BasicAttack,
                AbilityId.Guard,
                AbilityId.RevertTurn);
            var opponent = TestContent.Enemy(CombatantId.Goblin, 200, 40, 3);
            var battle = TestContent.StartBattle(hero, opponent, new ScriptedRandomSource(50, 50));
            battle.Submit(AbilityId.BasicAttack);
            battle.Submit(AbilityId.Guard);

            battle.Submit(AbilityId.RevertTurn);

            Assert.That(battle.State.Opponent.CurrentHealth, Is.EqualTo(173));
            Assert.That(battle.State.Hero.CurrentHealth, Is.EqualTo(103));
            Assert.That(battle.State.Hero.GuardBonus, Is.Zero);
            Assert.That(battle.State.Round, Is.EqualTo(2));
        }

        [Test]
        public void ReversionIsForbiddenAfterTheMageDies()
        {
            var mage = TestContent.Hero(
                CombatantId.Mage,
                new CombatantStats(10, 30, 5),
                AbilityId.BasicAttack,
                AbilityId.RevertTurn,
                AbilityId.RevertBattle);
            var opponent = TestContent.Enemy(CombatantId.Goblin, 200, 15, 3);
            var battle = TestContent.StartBattle(mage, opponent, new ScriptedRandomSource());

            battle.Submit(AbilityId.BasicAttack);

            Assert.That(battle.State.Phase, Is.EqualTo(BattlePhase.Defeat));
            Assert.That(battle.AvailableActions, Is.Empty);
            Assert.That(battle.Submit(AbilityId.RevertTurn).Rejection, Is.EqualTo(ActionRejection.BattleOver));
            Assert.That(battle.Submit(AbilityId.RevertBattle).Rejection, Is.EqualTo(ActionRejection.BattleOver));
            Assert.That(battle.State.Hero.CurrentHealth, Is.Zero);
        }

        [Test]
        public void ReversionIsForbiddenAfterVictory()
        {
            var opponent = TestContent.Enemy(CombatantId.Goblin, 30, 15, 3);
            var battle = TestContent.StartBattle(TestContent.Mage(), opponent, new ScriptedRandomSource());

            battle.Submit(AbilityId.BasicAttack);

            Assert.That(battle.State.Phase, Is.EqualTo(BattlePhase.Victory));
            Assert.That(battle.Submit(AbilityId.RevertBattle).Rejection, Is.EqualTo(ActionRejection.BattleOver));
        }
    }
}
