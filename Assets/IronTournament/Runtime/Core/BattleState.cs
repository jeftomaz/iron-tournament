using System;

namespace IronTournament.Core
{
    public enum BattlePhase
    {
        None,
        PlayerTurn,
        Victory,
        Defeat
    }

    public sealed class BattleState
    {
        public const int RevertChargesPerEncounter = 1;

        public BattleState(CombatantState hero, CombatantState opponent)
        {
            if (hero == null)
            {
                throw new ArgumentNullException(nameof(hero));
            }

            if (opponent == null)
            {
                throw new ArgumentNullException(nameof(opponent));
            }

            if (hero.Side != CombatantSide.Player)
            {
                throw new ArgumentException("The hero must be a player.", nameof(hero));
            }

            if (opponent.Side != CombatantSide.Enemy)
            {
                throw new ArgumentException("The opponent must be an enemy.", nameof(opponent));
            }

            if (hero.Id == opponent.Id)
            {
                throw new ArgumentException("Combatants must be distinct.", nameof(opponent));
            }

            Hero = hero;
            Opponent = opponent;
            Phase = BattlePhase.PlayerTurn;
            Round = 1;
            RevertCharges = hero.HasAbility(AbilityId.RevertTurn) || hero.HasAbility(AbilityId.RevertBattle)
                ? RevertChargesPerEncounter
                : 0;
        }

        private BattleState(BattleState source)
        {
            Hero = source.Hero.Clone();
            Opponent = source.Opponent.Clone();
            Phase = source.Phase;
            Round = source.Round;
            RevertCharges = source.RevertCharges;
        }

        public CombatantState Hero { get; }

        public CombatantState Opponent { get; }

        public BattlePhase Phase { get; private set; }

        public int Round { get; private set; }

        public int RevertCharges { get; private set; }

        public bool IsOver => Phase == BattlePhase.Victory || Phase == BattlePhase.Defeat;

        internal void AdvanceRound()
        {
            Round++;
        }

        internal void Finish(BattlePhase result)
        {
            Phase = result;
        }

        internal BattleState Clone()
        {
            return new BattleState(this);
        }

        internal void RestoreFrom(BattleState snapshot)
        {
            Hero.CopyFrom(snapshot.Hero);
            Opponent.CopyFrom(snapshot.Opponent);
            Phase = snapshot.Phase;
            Round = snapshot.Round;
        }

        internal void ConsumeRevertCharge()
        {
            RevertCharges = Math.Max(0, RevertCharges - 1);
        }
    }
}
