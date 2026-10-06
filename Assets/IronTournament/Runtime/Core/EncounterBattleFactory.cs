using System;

namespace IronTournament.Core
{
    public static class EncounterBattleFactory
    {
        public static IBattle Create(
            CombatantConfiguration hero,
            EncounterConfiguration encounter,
            IRandomSource random)
        {
            if (hero == null) throw new ArgumentNullException(nameof(hero));
            if (encounter == null) throw new ArgumentNullException(nameof(encounter));
            if (random == null) throw new ArgumentNullException(nameof(random));

            var opponent = EncounterSetup.CreateOpponent(
                encounter.Opponent,
                encounter.EnemyStatVariancePercent,
                FuryRules.CanRageInCampaign(encounter.Opponent.Id),
                random);
            return new Battle(new BattleState(new CombatantState(hero, hero.BaseStats), opponent), random);
        }
    }
}
