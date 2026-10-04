using System;

namespace IronTournament.Core
{
    internal readonly struct AttackOutcome
    {
        public AttackOutcome(int damage, bool isCritical, bool isPiercing)
        {
            Damage = damage;
            IsCritical = isCritical;
            IsPiercing = isPiercing;
        }

        public int Damage { get; }

        public bool IsCritical { get; }

        public bool IsPiercing { get; }
    }

    internal static class AttackRules
    {
        public const int CriticalChancePercent = 7;
        public const int CriticalMultiplier = 2;
        public const int PiercingChancePercent = 10;

        public static AttackOutcome Resolve(
            CombatantState attacker,
            CombatantState target,
            bool isOpponent,
            IRandomSource random)
        {
            if (isOpponent)
            {
                return new AttackOutcome(Mitigate(attacker.Stats.Attack, target), false, false);
            }

            if (IsArcane(attacker.Id))
            {
                return new AttackOutcome(attacker.Stats.Attack, false, false);
            }

            return Physical(attacker, target, random);
        }

        public static bool Roll(IRandomSource random, int chancePercent)
        {
            return random.Next(0, 100) < chancePercent;
        }

        private static bool IsArcane(CombatantId id)
        {
            return id == CombatantId.Mage;
        }

        private static AttackOutcome Physical(CombatantState attacker, CombatantState target, IRandomSource random)
        {
            var isPiercing = Roll(random, PiercingChancePercent);
            var damage = isPiercing ? attacker.Stats.Attack : Mitigate(attacker.Stats.Attack, target);
            var isCritical = Roll(random, CriticalChancePercent);
            return new AttackOutcome(isCritical ? damage * CriticalMultiplier : damage, isCritical, isPiercing);
        }

        private static int Mitigate(int attack, CombatantState target)
        {
            return Math.Max(0, attack - target.Defense);
        }
    }
}
