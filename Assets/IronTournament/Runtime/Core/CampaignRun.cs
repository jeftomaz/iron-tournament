using System;
using System.Collections.Generic;

namespace IronTournament.Core
{
    public enum CampaignPhase
    {
        None,
        Battle,
        Completed,
        Failed
    }

    public sealed class CampaignRun
    {
        private readonly IReadOnlyList<EncounterConfiguration> encounters;
        private readonly IRandomSource random;
        private Battle battle;

        public CampaignRun(CombatantConfiguration hero, CampaignConfiguration campaign, IRandomSource random)
        {
            if (hero == null)
            {
                throw new ArgumentNullException(nameof(hero));
            }

            if (campaign == null)
            {
                throw new ArgumentNullException(nameof(campaign));
            }

            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            if (hero.Side != CombatantSide.Player)
            {
                throw new ArgumentException("The hero must be a player.", nameof(hero));
            }

            Hero = new CombatantState(hero, hero.BaseStats);
            encounters = campaign.Encounters;
            this.random = random;
            StartEncounter(0);
        }

        public CombatantState Hero { get; }

        public CampaignPhase Phase { get; private set; }

        public int EncounterIndex { get; private set; }

        public int EncounterCount => encounters.Count;

        public IBattle CurrentBattle => battle;

        public bool ConcludeEncounter()
        {
            if (Phase != CampaignPhase.Battle || !battle.State.IsOver)
            {
                return false;
            }

            if (battle.State.Phase == BattlePhase.Defeat)
            {
                Phase = CampaignPhase.Failed;
                return true;
            }

            if (EncounterIndex == encounters.Count - 1)
            {
                Phase = CampaignPhase.Completed;
                return true;
            }

            StartEncounter(EncounterIndex + 1);
            return true;
        }

        private void StartEncounter(int index)
        {
            var encounter = encounters[index];
            var opponent = EncounterSetup.CreateOpponent(
                encounter.Opponent,
                encounter.EnemyStatVariancePercent,
                FuryRules.CanRageInCampaign(encounter.Opponent.Id),
                random);
            EncounterIndex = index;
            battle = new Battle(new BattleState(Hero, opponent), random);
            Phase = CampaignPhase.Battle;
        }
    }
}
