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

        private void OnEnable()
        {
            if (view != null) view.CombatStarted += InitializeSelectedBattle;
        }

        private void OnDisable()
        {
            if (view != null) view.CombatStarted -= InitializeSelectedBattle;
        }

        private void InitializeSelectedBattle()
        {
            if (view == null || presenter == null || view.SelectedPlayer == null || view.SelectedEncounter == null)
            {
                Debug.LogError("Battle bootstrap requires a selected player, encounter, view, and presenter.", this);
                enabled = false;
                return;
            }

            try
            {
                var hero = ContentMapper.BuildCombatant(view.SelectedPlayer);
                var encounter = ContentMapper.BuildEncounter(view.SelectedEncounter);
                presenter.Initialize(EncounterBattleFactory.Create(
                    hero,
                    encounter,
                    new SeededRandomSource(Environment.TickCount)));
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                enabled = false;
            }
        }
    }
}
