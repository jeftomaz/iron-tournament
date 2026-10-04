using System;
using IronTournament.Core;
using UnityEngine;

namespace IronTournament.Presentation
{
    [DisallowMultipleComponent]
    public sealed class BattlePresenter : MonoBehaviour
    {
        [SerializeField] private BattleView view;
        [SerializeField] private BattleHud hud;
        [SerializeField] private ActionMenu menu;
        [SerializeField] private BattleEventPlayer events;

        private IBattle battle;
        private bool awaitingPlayback;

        public void Initialize(IBattle encounter)
        {
            if (encounter == null) throw new ArgumentNullException(nameof(encounter));
            if (battle != null) throw new InvalidOperationException("The presenter is already bound to a battle.");
            if (encounter.State == null || encounter.State.IsOver)
                throw new ArgumentException("An active battle is required.", nameof(encounter));
            battle = encounter;
            if (isActiveAndEnabled && view.IsCombatStarted) RenderState();
        }

        private void OnEnable()
        {
            view.CombatStarted += RenderState;
            menu.ActionSelected += Submit;
            if (battle != null && view.IsCombatStarted) RenderState();
        }

        private void OnDisable()
        {
            view.CombatStarted -= RenderState;
            menu.ActionSelected -= Submit;
            events.Cancel();
            awaitingPlayback = false;
            menu.SetAvailableActions(Array.Empty<AbilityId>(), true);
        }

        private void LateUpdate()
        {
            if (!awaitingPlayback || events.IsPlaying) return;
            awaitingPlayback = false;
            RenderState();
        }

        private void Submit(AbilityId action)
        {
            if (battle == null || awaitingPlayback || events.IsPlaying || !view.IsCombatStarted) return;
            var result = battle.Submit(action);
            if (!result.IsAccepted)
            {
                RenderState();
                if (!battle.State.IsOver) hud.SetStatus("Ação indisponível");
                return;
            }
            awaitingPlayback = true;
            menu.SetAvailableActions(battle.AvailableActions, true);
            events.PlayEvents(result.Events, battle.State);
        }

        private void RenderState()
        {
            if (battle == null) return;
            var state = battle.State;
            hud.ShowHealth(CombatantSide.Player, state.Hero.CurrentHealth, state.Hero.Stats.MaximumHealth);
            hud.ShowHealth(CombatantSide.Enemy, state.Opponent.CurrentHealth, state.Opponent.Stats.MaximumHealth);
            if (state.IsOver) view.ShowOutcome(state.Phase);
            else
            {
                hud.SetStatus("Vez do Guerreiro");
                menu.SetAvailableActions(battle.AvailableActions, false);
            }
        }
    }
}
