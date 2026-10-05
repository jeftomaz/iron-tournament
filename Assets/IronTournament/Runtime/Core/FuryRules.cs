using System;

namespace IronTournament.Core
{
    internal static class FuryRules
    {
        public const int ThresholdPercent = 30;

        public static bool CanRageInCampaign(CombatantId id)
        {
            return id == CombatantId.Vampire || id == CombatantId.Necromancer || id == CombatantId.DemonKing;
        }

        public static bool IsTriggered(CombatantState combatant)
        {
            return combatant.CanRage
                && !combatant.HasRaged
                && !combatant.IsDefeated
                && combatant.IsHealthAtOrBelow(ThresholdPercent);
        }

        public static int Excess(CombatantState target)
        {
            return Math.Max(0, target.CurrentHealth - target.HealthAtPercent(ThresholdPercent));
        }
    }
}
