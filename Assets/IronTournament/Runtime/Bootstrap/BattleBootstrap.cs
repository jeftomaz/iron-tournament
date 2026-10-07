using System;
using IronTournament.Content;
using IronTournament.Core;
using IronTournament.Presentation;
using UnityEngine;

namespace IronTournament.Bootstrap
{
    [DisallowMultipleComponent]
    public sealed class BattleBootstrap : MonoBehaviour
    {
        [SerializeField] private BattleView view;
        [SerializeField] private BattlePresenter presenter;
        [SerializeField] private ActionMenu menu;
        [SerializeField] private CampaignDefinition campaign;

        private CampaignRun campaignRun;

        private void OnEnable()
        {
            if (view != null) view.CombatStarted += InitializeCampaign;
            if (presenter != null) presenter.EncounterConcluded += ConcludeEncounter;
            if (menu != null) menu.DropSelected += ChooseDrop;
        }

        private void OnDisable()
        {
            if (view != null) view.CombatStarted -= InitializeCampaign;
            if (presenter != null) presenter.EncounterConcluded -= ConcludeEncounter;
            if (menu != null) menu.DropSelected -= ChooseDrop;
        }

        private void InitializeCampaign()
        {
            if (view == null || presenter == null || menu == null || campaign == null ||
                view.SelectedPlayer == null || view.SelectedEncounter == null)
            {
                Fail("Battle bootstrap requires a player, encounter, campaign, menu, view, and presenter.");
                return;
            }

            try
            {
                var hero = ContentMapper.BuildCombatant(view.SelectedPlayer);
                var configuration = ContentMapper.BuildCampaign(campaign);
                campaignRun = new CampaignRun(hero, configuration, new SeededRandomSource(Environment.TickCount));
                presenter.Initialize(campaignRun.CurrentBattle);
            }
            catch (Exception exception)
            {
                Fail(exception);
            }
        }

        private void ConcludeEncounter(BattlePhase result)
        {
            if (campaignRun == null || !campaignRun.ConcludeEncounter())
            {
                Fail("The campaign could not conclude the presented encounter.");
                return;
            }

            try
            {
                switch (campaignRun.Phase)
                {
                    case CampaignPhase.Battle:
                        BeginNextEncounter();
                        break;
                    case CampaignPhase.DropChoice:
                        if (result != BattlePhase.Victory) throw new InvalidOperationException("Only a victory can offer drops.");
                        view.ShowDropChoice(campaignRun.DropOffer);
                        menu.ConfigureDrops(campaignRun.DropOffer);
                        break;
                    case CampaignPhase.Completed:
                        view.ShowCampaignComplete();
                        break;
                    case CampaignPhase.Failed:
                        break;
                    default:
                        throw new InvalidOperationException("The campaign entered an unsupported phase.");
                }
            }
            catch (Exception exception)
            {
                Fail(exception);
            }
        }

        private void ChooseDrop(ItemId item)
        {
            if (campaignRun == null || !campaignRun.ChooseDrop(item))
            {
                Fail("The selected drop is not available.");
                return;
            }

            try
            {
                BeginNextEncounter();
            }
            catch (Exception exception)
            {
                Fail(exception);
            }
        }

        private void BeginNextEncounter()
        {
            if (campaignRun == null || campaignRun.Phase != CampaignPhase.Battle)
                throw new InvalidOperationException("An active campaign encounter is required.");

            view.BeginNextEncounter(campaignRun.CurrentBattle.State.Opponent.Id);
            presenter.BeginNextEncounter(campaignRun.CurrentBattle);
        }

        private void Fail(string message)
        {
            Debug.LogError(message, this);
            enabled = false;
        }

        private void Fail(Exception exception)
        {
            Debug.LogException(exception, this);
            enabled = false;
        }
    }
}
