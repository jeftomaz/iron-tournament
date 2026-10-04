using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace IronTournament.Core
{
    public enum CampaignPhase
    {
        None,
        Battle,
        DropChoice,
        Completed,
        Failed
    }

    public sealed class CampaignRun
    {
        public const int DropOfferSize = 2;

        private static readonly ReadOnlyCollection<ItemConfiguration> NoOffer =
            new ReadOnlyCollection<ItemConfiguration>(new List<ItemConfiguration>());

        private readonly IReadOnlyList<EncounterConfiguration> encounters;
        private readonly IRandomSource random;
        private readonly List<ItemId> obtainedItems = new List<ItemId>();
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
            DropOffer = NoOffer;
            ObtainedItems = new ReadOnlyCollection<ItemId>(obtainedItems);
            StartEncounter(0);
        }

        public CombatantState Hero { get; }

        public CampaignPhase Phase { get; private set; }

        public int EncounterIndex { get; private set; }

        public int EncounterCount => encounters.Count;

        public IBattle CurrentBattle => battle;

        public IReadOnlyList<ItemConfiguration> DropOffer { get; private set; }

        public IReadOnlyList<ItemId> ObtainedItems { get; }

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

            var offer = DropTable.Roll(EligibleDrops(encounters[EncounterIndex].DropPool), DropOfferSize, random);
            if (offer.Count == 0)
            {
                StartEncounter(EncounterIndex + 1);
                return true;
            }

            DropOffer = new ReadOnlyCollection<ItemConfiguration>(offer);
            Phase = CampaignPhase.DropChoice;
            return true;
        }

        public bool ChooseDrop(ItemId item)
        {
            if (Phase != CampaignPhase.DropChoice)
            {
                return false;
            }

            for (var index = 0; index < DropOffer.Count; index++)
            {
                if (DropOffer[index].Id == item)
                {
                    ItemRules.Apply(DropOffer[index], Hero);
                    obtainedItems.Add(item);
                    StartEncounter(EncounterIndex + 1);
                    return true;
                }
            }

            return false;
        }

        private List<ItemConfiguration> EligibleDrops(IReadOnlyList<ItemConfiguration> pool)
        {
            var eligible = new List<ItemConfiguration>();
            for (var index = 0; index < pool.Count; index++)
            {
                if (ItemRules.IsAvailableTo(pool[index].Id, Hero.Id))
                {
                    eligible.Add(pool[index]);
                }
            }

            return eligible;
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
            DropOffer = NoOffer;
            battle = new Battle(new BattleState(Hero, opponent), random);
            Phase = CampaignPhase.Battle;
        }
    }
}
