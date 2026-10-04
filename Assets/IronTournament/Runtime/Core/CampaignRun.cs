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

    public enum CampaignMode
    {
        None,
        Campaign,
        EnemyMode
    }

    public sealed class CampaignRun
    {
        public const int DropOfferSize = 2;

        private static readonly ReadOnlyCollection<ItemConfiguration> NoOffer =
            new ReadOnlyCollection<ItemConfiguration>(new List<ItemConfiguration>());

        private readonly List<EncounterStage> stages;
        private readonly IRandomSource random;
        private readonly List<ItemId> obtainedItems = new List<ItemId>();
        private Battle battle;

        public CampaignRun(CombatantConfiguration hero, CampaignConfiguration campaign, IRandomSource random)
            : this(CampaignMode.Campaign, CampaignHero(hero), CampaignStages(hero, campaign), random)
        {
        }

        private CampaignRun(
            CampaignMode mode,
            CombatantConfiguration hero,
            List<EncounterStage> stages,
            IRandomSource random)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            Mode = mode;
            Hero = new CombatantState(hero, hero.BaseStats);
            this.stages = stages;
            this.random = random;
            DropOffer = NoOffer;
            ObtainedItems = new ReadOnlyCollection<ItemId>(obtainedItems);

            var opponents = new List<CombatantId>();
            foreach (var stage in stages)
            {
                opponents.Add(stage.Opponent.Id);
            }

            Opponents = new ReadOnlyCollection<CombatantId>(opponents);
            StartEncounter(0);
        }

        public CampaignMode Mode { get; }

        public CombatantState Hero { get; }

        public CampaignPhase Phase { get; private set; }

        public int EncounterIndex { get; private set; }

        public int EncounterCount => stages.Count;

        public IReadOnlyList<CombatantId> Opponents { get; }

        public IBattle CurrentBattle => battle;

        public IReadOnlyList<ItemConfiguration> DropOffer { get; private set; }

        public IReadOnlyList<ItemId> ObtainedItems { get; }

        public HeroSnapshot FinalSnapshot { get; private set; }

        public static CampaignRun StartEnemyMode(
            CombatantId character,
            CampaignConfiguration campaign,
            ProgressData progress,
            IRandomSource random)
        {
            if (campaign == null)
            {
                throw new ArgumentNullException(nameof(campaign));
            }

            if (progress == null)
            {
                throw new ArgumentNullException(nameof(progress));
            }

            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            if (!progress.IsEnemyModeUnlocked)
            {
                throw new ArgumentException("Enemy mode is locked.", nameof(progress));
            }

            var hero = EnemyModeSetup.Character(character, campaign);
            var stages = EnemyModeSetup.Stages(character, campaign, progress, random);
            return new CampaignRun(CampaignMode.EnemyMode, hero, stages, random);
        }

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

            if (EncounterIndex == stages.Count - 1)
            {
                Complete();
                return true;
            }

            var stage = stages[EncounterIndex];
            var offer = DropTable.Roll(stage.DropPool, stage.DropOfferSize, random);
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

        private static CombatantConfiguration CampaignHero(CombatantConfiguration hero)
        {
            if (hero == null)
            {
                throw new ArgumentNullException(nameof(hero));
            }

            if (hero.Side != CombatantSide.Player || !HeroSnapshot.IsHeroClass(hero.Id))
            {
                throw new ArgumentException("The hero must be the Warrior or the Mage.", nameof(hero));
            }

            return hero;
        }

        private static List<EncounterStage> CampaignStages(CombatantConfiguration hero, CampaignConfiguration campaign)
        {
            if (campaign == null)
            {
                throw new ArgumentNullException(nameof(campaign));
            }

            var stages = new List<EncounterStage>();
            foreach (var encounter in campaign.Encounters)
            {
                stages.Add(new EncounterStage(
                    encounter.Opponent,
                    encounter.EnemyStatVariancePercent,
                    FuryRules.CanRageInCampaign(encounter.Opponent.Id),
                    ItemRules.Eligible(encounter.DropPool, hero.Id),
                    DropOfferSize));
            }

            return stages;
        }

        private void Complete()
        {
            Phase = CampaignPhase.Completed;
            if (Mode == CampaignMode.Campaign)
            {
                FinalSnapshot = new HeroSnapshot(Hero.Id, Hero.Configuration.DisplayName, Hero.Stats);
            }
        }

        private void StartEncounter(int index)
        {
            var stage = stages[index];
            var opponent = EncounterSetup.CreateOpponent(stage.Opponent, stage.VariancePercent, stage.CanRage, random);
            EncounterIndex = index;
            DropOffer = NoOffer;
            Hero.PrepareForEncounter();
            battle = new Battle(new BattleState(Hero, opponent), random);
            Phase = CampaignPhase.Battle;
        }
    }
}
