using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace IronTournament.Core
{
    public sealed class Battle : IBattle
    {
        public const int GuardDamage = 4;

        private readonly IRandomSource random;
        private readonly BattleState encounterStart;
        private BattleState turnStart;

        public Battle(BattleState state, IRandomSource random)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            if (state.Phase != BattlePhase.PlayerTurn)
            {
                throw new ArgumentException("The battle must start on the player turn.", nameof(state));
            }

            State = state;
            this.random = random;
            encounterStart = state.Clone();
        }

        public BattleState State { get; }

        public IReadOnlyList<AbilityId> AvailableActions => ComputeAvailableActions();

        public ActionResult Submit(AbilityId action)
        {
            if (State.IsOver)
            {
                return ActionResult.Rejected(ActionRejection.BattleOver);
            }

            if (!ComputeAvailableActions().Contains(action))
            {
                return ActionResult.Rejected(ActionRejection.UnavailableAction);
            }

            var events = new List<BattleEvent>();
            if (action == AbilityId.RevertTurn || action == AbilityId.RevertBattle)
            {
                Revert(action, events);
            }
            else
            {
                turnStart = State.Clone();
                PlayRound(action, events);
            }

            return ActionResult.Accepted(events);
        }

        private ReadOnlyCollection<AbilityId> ComputeAvailableActions()
        {
            var actions = new List<AbilityId>();
            if (!State.IsOver)
            {
                var abilities = State.Hero.Configuration.Abilities;
                for (var index = 0; index < abilities.Count; index++)
                {
                    if (IsAvailable(abilities[index].Id))
                    {
                        actions.Add(abilities[index].Id);
                    }
                }
            }

            return new ReadOnlyCollection<AbilityId>(actions);
        }

        private bool IsAvailable(AbilityId ability)
        {
            if (!State.Hero.HasAbility(ability))
            {
                return false;
            }

            switch (ability)
            {
                case AbilityId.BasicAttack:
                case AbilityId.Guard:
                    return true;
                case AbilityId.RevertTurn:
                case AbilityId.RevertBattle:
                    return State.RevertCharges > 0 && turnStart != null;
                default:
                    return false;
            }
        }

        private void Revert(AbilityId action, List<BattleEvent> events)
        {
            events.Add(new AbilityUsedEvent(State.Hero.Id, action));
            State.RestoreFrom(action == AbilityId.RevertTurn ? turnStart : encounterStart);
            State.ConsumeRevertCharge();
            turnStart = null;
        }

        private void PlayRound(AbilityId action, List<BattleEvent> events)
        {
            var hero = State.Hero;
            events.Add(new AbilityUsedEvent(hero.Id, action));
            ResolveHeroAction(action, events);
            if (State.Opponent.IsDefeated)
            {
                Finish(BattlePhase.Victory, events);
                return;
            }

            ResolveOpponentTurn(events);
            if (hero.IsDefeated)
            {
                Finish(BattlePhase.Defeat, events);
                return;
            }

            hero.LowerGuard();
            State.AdvanceRound();
        }

        private void ResolveHeroAction(AbilityId action, List<BattleEvent> events)
        {
            switch (action)
            {
                case AbilityId.BasicAttack:
                    Attack(State.Hero, State.Opponent, false, events);
                    break;
                case AbilityId.Guard:
                    Guard(events);
                    break;
            }
        }

        private void ResolveOpponentTurn(List<BattleEvent> events)
        {
            var opponent = State.Opponent;
            if (FuryRules.IsTriggered(opponent))
            {
                events.Add(new AbilityUsedEvent(opponent.Id, AbilityId.Fury));
                Fury(opponent, State.Hero, events);
                return;
            }

            events.Add(new AbilityUsedEvent(opponent.Id, AbilityId.BasicAttack));
            Attack(opponent, State.Hero, true, events);
        }

        private static void Fury(CombatantState actor, CombatantState target, List<BattleEvent> events)
        {
            actor.MarkRaged();
            var excess = Math.Max(0, target.CurrentHealth - FuryRules.Threshold(target));
            target.TakeDamage(excess);
            events.Add(new DamageDealtEvent(
                actor.Id,
                target.Id,
                DamageKind.Fury,
                excess,
                target.CurrentHealth,
                false,
                false));
        }

        private void Attack(CombatantState attacker, CombatantState target, bool isOpponent, List<BattleEvent> events)
        {
            var outcome = AttackRules.Resolve(attacker, target, isOpponent, random);
            target.TakeDamage(outcome.Damage);
            events.Add(new DamageDealtEvent(
                attacker.Id,
                target.Id,
                DamageKind.Attack,
                outcome.Damage,
                target.CurrentHealth,
                outcome.IsCritical,
                outcome.IsPiercing));
        }

        private void Guard(List<BattleEvent> events)
        {
            var hero = State.Hero;
            var opponent = State.Opponent;
            hero.RaiseGuard(GuardBonus(hero.Stats.Defense));
            opponent.TakeDamage(GuardDamage);
            events.Add(new DamageDealtEvent(
                hero.Id,
                opponent.Id,
                DamageKind.Reflection,
                GuardDamage,
                opponent.CurrentHealth,
                false,
                false));
        }

        // A DEF efetiva durante a próxima ação inimiga é ceil(DEF × 1,5).
        private static int GuardBonus(int defense)
        {
            return defense / 2 + defense % 2;
        }

        private void Finish(BattlePhase result, List<BattleEvent> events)
        {
            State.Hero.LowerGuard();
            State.Finish(result);
            events.Add(new BattleEndedEvent(result));
        }
    }
}
