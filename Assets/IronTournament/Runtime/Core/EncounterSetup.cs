using System;

namespace IronTournament.Core
{
    internal static class EncounterSetup
    {
        public static CombatantState CreateOpponent(
            CombatantConfiguration opponent,
            int variancePercent,
            bool canRage,
            IRandomSource random)
        {
            var stats = opponent.BaseStats;
            var effective = variancePercent == 0
                ? stats
                : new CombatantStats(
                    Vary(stats.MaximumHealth, variancePercent, 1, random),
                    Vary(stats.Attack, variancePercent, 1, random),
                    Vary(stats.Defense, variancePercent, 0, random));
            var state = new CombatantState(opponent, effective);
            if (canRage)
            {
                state.EnableRage();
            }

            return state;
        }

        private static int Vary(int value, int variancePercent, int minimum, IRandomSource random)
        {
            var percent = 100 + random.Next(-variancePercent, variancePercent + 1);
            return (int)Math.Max(minimum, ((long)value * percent + 50) / 100);
        }
    }
}
