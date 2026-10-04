using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace IronTournament.Core
{
    public sealed class Battle : IBattle
    {
        public const int GuardDamage = 4;

        private readonly IRandomSource random;

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
            PlayRound(action, events);
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

        private static bool IsAvailable(AbilityId ability)
        {
            switch (ability)
            {
                case AbilityId.BasicAttack:
                case AbilityId.Guard:
                    return true;
                default:
                    return false;
            }
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
            events.Add(new AbilityUsedEvent(State.Opponent.Id, AbilityId.BasicAttack));
            Attack(State.Opponent, State.Hero, true, events);
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
