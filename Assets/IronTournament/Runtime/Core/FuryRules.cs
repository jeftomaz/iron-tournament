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
                && IsAtOrBelowThreshold(combatant);
        }

        public static bool IsAtOrBelowThreshold(CombatantState combatant)
        {
            return (long)combatant.CurrentHealth * 100 <= (long)combatant.Stats.MaximumHealth * ThresholdPercent;
        }

        public static int Threshold(CombatantState target)
        {
            return (int)((long)target.Stats.MaximumHealth * ThresholdPercent / 100);
        }
    }
}
