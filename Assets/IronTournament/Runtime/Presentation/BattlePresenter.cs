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
        private bool completionReported;

        public event Action<BattlePhase> EncounterConcluded;

        public void Initialize(IBattle encounter)
        {
            if (encounter == null) throw new ArgumentNullException(nameof(encounter));
            if (battle != null) throw new InvalidOperationException("The presenter is already bound to a battle.");
            Bind(encounter);
        }

        public void BeginNextEncounter(IBattle encounter)
        {
            if (encounter == null) throw new ArgumentNullException(nameof(encounter));
            if (battle == null || !battle.State.IsOver)
                throw new InvalidOperationException("The current encounter must be over before advancing.");
            if (awaitingPlayback || events.IsPlaying)
                throw new InvalidOperationException("The previous encounter is still being presented.");
            Bind(encounter);
        }

        private void Bind(IBattle encounter)
        {
            if (encounter.State == null || encounter.State.IsOver)
                throw new ArgumentException("An active battle is required.", nameof(encounter));
            if (view.SelectedPlayer != null && encounter.State.Hero.Id != view.SelectedPlayer.Id)
                throw new ArgumentException("The battle player must match the selected player.", nameof(encounter));
            if (view.SelectedEncounter != null &&
                encounter.State.Opponent.Id != view.SelectedEncounter.Opponent.Id)
                throw new ArgumentException("The battle opponent must match the selected encounter.", nameof(encounter));
            battle = encounter;
            completionReported = false;
            menu.ConfigureActions(encounter.State.Hero.Configuration.Abilities);
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
            if (battle.State.IsOver && !completionReported)
            {
                completionReported = true;
                EncounterConcluded?.Invoke(battle.State.Phase);
            }
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
            if (action == AbilityId.RevertTurn || action == AbilityId.RevertBattle)
            {
                awaitingPlayback = false;
                RenderState();
                hud.SetStatus($"{(action == AbilityId.RevertTurn ? "Turno revertido" : "Batalha revertida")} · Reversão: {battle.State.RevertCharges}");
            }
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
                hud.SetStatus($"Vez do {state.Hero.Configuration.DisplayName}" +
                    (state.Hero.Id == CombatantId.Mage ? $" · Reversão: {state.RevertCharges}" : ""));
                menu.SetAvailableActions(battle.AvailableActions, false);
            }
        }
    }
}
