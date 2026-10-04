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
            var hero = State.Hero;
            switch (ability)
            {
                case AbilityId.BasicAttack:
                case AbilityId.Guard:
                    return true;
                case AbilityId.RevertTurn:
                case AbilityId.RevertBattle:
                    return State.RevertCharges > 0 && turnStart != null;
                case AbilityId.UseHopeScroll:
                    return hero.HopeScrolls > 0 && hero.IsHealthAtOrBelow(ItemRules.HopeScrollThresholdPercent);
                case AbilityId.ArmGuardian:
                    return hero.GuardianHorns > 0 && !hero.IsGuardianArmed;
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
            var opponent = State.Opponent;
            events.Add(new AbilityUsedEvent(hero.Id, action));
            ResolveHeroAction(action, events);
            ApplyBurn(events);
            if (opponent.IsDefeated)
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

            ApplyBurn(events);
            if (opponent.IsDefeated)
            {
                Finish(BattlePhase.Victory, events);
                return;
            }

            hero.LowerGuard();
            State.AdvanceRound();
        }

        private void ResolveHeroAction(AbilityId action, List<BattleEvent> events)
        {
            var hero = State.Hero;
            switch (action)
            {
                case AbilityId.BasicAttack:
                    var outcome = AttackRules.Resolve(hero, State.Opponent, false, random);
                    Damage(hero, State.Opponent, DamageKind.Attack, outcome.Damage, outcome.IsCritical, outcome.IsPiercing, events);
                    break;
                case AbilityId.Guard:
                    hero.RaiseGuard(GuardBonus(hero.Stats.Defense));
                    Damage(hero, State.Opponent, DamageKind.Reflection, GuardDamage, false, false, events);
                    break;
                case AbilityId.UseHopeScroll:
                    hero.ConsumeHopeScroll();
                    Heal(hero, hero.HealthAtPercent(ItemRules.HopeScrollHealPercent), events);
                    break;
                case AbilityId.ArmGuardian:
                    hero.ArmGuardian();
                    break;
            }
        }

        private void ResolveOpponentTurn(List<BattleEvent> events)
        {
            var opponent = State.Opponent;
            var hero = State.Hero;
            if (FuryRules.IsTriggered(opponent))
            {
                events.Add(new AbilityUsedEvent(opponent.Id, AbilityId.Fury));
                opponent.MarkRaged();
                StrikeHero(DamageKind.Fury, FuryRules.Excess(hero), false, false, events);
                return;
            }

            events.Add(new AbilityUsedEvent(opponent.Id, AbilityId.BasicAttack));
            var outcome = AttackRules.Resolve(opponent, hero, true, random);
            StrikeHero(DamageKind.Attack, outcome.Damage, outcome.IsCritical, outcome.IsPiercing, events);
        }

        private void StrikeHero(DamageKind kind, int amount, bool isCritical, bool isPiercing, List<BattleEvent> events)
        {
            var hero = State.Hero;
            if (hero.IsGuardianArmed && amount >= hero.CurrentHealth)
            {
                InterceptWithGuardian(amount, events);
                return;
            }

            Damage(State.Opponent, hero, kind, amount, isCritical, isPiercing, events);
        }

        private void InterceptWithGuardian(int preventedDamage, List<BattleEvent> events)
        {
            var hero = State.Hero;
            var opponent = State.Opponent;
            hero.DisarmGuardian();
            events.Add(new GuardianInterceptedEvent(hero.Id, preventedDamage));

            var strike = ItemRules.GuardianWarriorAttack + hero.Stats.Attack;
            Damage(hero, opponent, DamageKind.GuardianStrike, strike, false, false, events);
            if (opponent.IsDefeated)
            {
                var recovery = hero.HealthAtPercent(ItemRules.GuardianRecoveryPercent) - hero.CurrentHealth;
                if (recovery > 0)
                {
                    Heal(hero, recovery, events);
                }

                return;
            }

            var recoil = hero.CurrentHealth - 1;
            if (recoil > 0)
            {
                Damage(opponent, hero, DamageKind.GuardianRecoil, recoil, false, false, events);
            }
        }

        private void ApplyBurn(List<BattleEvent> events)
        {
            if (State.Hero.HasFlameCloak && !State.Opponent.IsDefeated)
            {
                Damage(State.Hero, State.Opponent, DamageKind.Burn, ItemRules.FlameCloakBurn, false, false, events);
            }
        }

        private static void Damage(
            CombatantState source,
            CombatantState target,
            DamageKind kind,
            int amount,
            bool isCritical,
            bool isPiercing,
            List<BattleEvent> events)
        {
            target.TakeDamage(amount);
            events.Add(new DamageDealtEvent(
                source.Id,
                target.Id,
                kind,
                amount,
                target.CurrentHealth,
                isCritical,
                isPiercing));
        }

        private static void Heal(CombatantState target, int amount, List<BattleEvent> events)
        {
            var healed = target.Heal(amount);
            events.Add(new HealthRestoredEvent(target.Id, healed, target.CurrentHealth));
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
