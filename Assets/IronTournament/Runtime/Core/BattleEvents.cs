using System;

namespace IronTournament.Core
{
    public abstract class BattleEvent
    {
        internal BattleEvent()
        {
        }
    }

    public sealed class AbilityUsedEvent : BattleEvent
    {
        public AbilityUsedEvent(CombatantId actor, AbilityId ability)
        {
            Actor = ConfigurationGuard.Defined(actor, nameof(actor));
            Ability = ConfigurationGuard.Defined(ability, nameof(ability));
        }

        public CombatantId Actor { get; }

        public AbilityId Ability { get; }
    }

    public sealed class DamageDealtEvent : BattleEvent
    {
        public DamageDealtEvent(
            CombatantId source,
            CombatantId target,
            int amount,
            int remainingHealth,
            bool isCritical)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            if (remainingHealth < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(remainingHealth));
            }

            Source = ConfigurationGuard.Defined(source, nameof(source));
            Target = ConfigurationGuard.Defined(target, nameof(target));
            Amount = amount;
            RemainingHealth = remainingHealth;
            IsCritical = isCritical;
        }

        public CombatantId Source { get; }

        public CombatantId Target { get; }

        public int Amount { get; }

        public int RemainingHealth { get; }

        public bool IsCritical { get; }
    }

    public sealed class BattleEndedEvent : BattleEvent
    {
        public BattleEndedEvent(BattlePhase result)
        {
            if (result != BattlePhase.Victory && result != BattlePhase.Defeat)
            {
                throw new ArgumentOutOfRangeException(nameof(result));
            }

            Result = result;
        }

        public BattlePhase Result { get; }
    }
}
