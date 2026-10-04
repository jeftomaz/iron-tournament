using System;
using System.Collections;
using System.Collections.Generic;
using IronTournament.Core;
using UnityEngine;
using UnityEngine.UI;

namespace IronTournament.Presentation
{
    [DisallowMultipleComponent]
    public sealed class BattleEventPlayer : MonoBehaviour
    {
        [SerializeField] private BattleView view;
        [SerializeField] private BattleHud hud;
        [SerializeField] private ActionMenu menu;
        [SerializeField] private Text feedback;

        private Coroutine playback;
        private string previousStatus;
        private CombatantSide feedbackTarget;

        public bool IsPlaying { get; private set; }

        public void PlayEvents(IReadOnlyList<BattleEvent> events, BattleState state)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (state.Hero.Id == state.Opponent.Id)
                throw new ArgumentException("Combatant IDs must be distinct.", nameof(state));
            var effects = new List<IEnumerator>(events.Count);
            for (int index = 0; index < events.Count; index++)
            {
                switch (events[index])
                {
                    case AbilityUsedEvent ability:
                        var actor = Participant(state, ability.Actor).Side;
                        switch (ability.Ability)
                        {
                            case AbilityId.BasicAttack: effects.Add(Attack(actor)); break;
                            case AbilityId.Guard: effects.Add(Guard(actor)); break;
                            default: throw new NotSupportedException($"Unsupported visual ability: {ability.Ability}.");
                        }
                        break;
                    case DamageDealtEvent damage:
                        Participant(state, damage.Source);
                        var target = Participant(state, damage.Target);
                        if (damage.RemainingHealth > target.Stats.MaximumHealth)
                            throw new ArgumentOutOfRangeException(nameof(events), "Remaining health exceeds maximum health.");
                        effects.Add(PresentDamage(damage, target.Side, target.Stats.MaximumHealth));
                        break;
                    case BattleEndedEvent ended:
                        if (index != events.Count - 1)
                            throw new ArgumentException("The outcome must be the final event.", nameof(events));
                        effects.Add(End(ended));
                        break;
                    default: throw new ArgumentException("Every event must have a supported type.", nameof(events));
                }
            }
            Play(effects);
        }

        private static CombatantState Participant(BattleState state, CombatantId id)
        {
            if (id == state.Hero.Id) return state.Hero;
            if (id == state.Opponent.Id) return state.Opponent;
            throw new ArgumentException("The event combatant does not belong to this battle.", nameof(id));
        }

        private IEnumerator PresentDamage(DamageDealtEvent damage, CombatantSide target, int maximumHealth)
        {
            hud.ShowHealth(target, damage.RemainingHealth, maximumHealth);
            yield return AnimateDamage(target, damage.Amount, damage.IsCritical, damage.IsPiercing,
                damage.Kind == DamageKind.Reflection);
        }

        public void Play(IReadOnlyList<IEnumerator> effects)
        {
            if (effects == null) throw new ArgumentNullException(nameof(effects));
            if (IsPlaying) throw new InvalidOperationException("A visual sequence is already playing.");
            if (!isActiveAndEnabled) throw new InvalidOperationException("The event player must be active.");
            if (view.HasOutcome) throw new InvalidOperationException("The battle presentation has ended.");
            var sequence = new List<IEnumerator>(effects.Count);
            foreach (var effect in effects)
            {
                if (effect == null) throw new ArgumentException("Every visual effect is required.", nameof(effects));
                sequence.Add(effect);
            }
            if (sequence.Count == 0) return;
            previousStatus = hud.StatusMessage;
            IsPlaying = true;
            menu.SetPresentationBlocked(true);
            playback = StartCoroutine(PlaySequence(sequence));
        }

        public IEnumerator Attack(CombatantSide actor)
        {
            view.GetCombatantRect(actor);
            return AnimateAttack(actor);
        }

        public IEnumerator Guard(CombatantSide actor)
        {
            view.GetCombatantRect(actor);
            return AnimateGuard(actor);
        }

        public IEnumerator Damage(CombatantSide target, int amount, bool critical = false, bool piercing = false)
        {
            view.GetCombatantRect(target);
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            return AnimateDamage(target, amount, critical, piercing);
        }

        public void Cancel()
        {
            if (!IsPlaying) return;
            if (playback != null) StopCoroutine(playback);
            Finish();
        }

        public IEnumerator End(BattleEndedEvent result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            return PresentOutcome(result.Result);
        }

        private IEnumerator PresentOutcome(BattlePhase result)
        {
            view.ShowOutcome(result);
            yield break;
        }

        private void OnDisable() => Cancel();

        private void LateUpdate()
        {
            if (feedback != null && feedback.gameObject.activeSelf && feedbackTarget != CombatantSide.None)
                PositionFeedback();
        }

        private IEnumerator PlaySequence(List<IEnumerator> sequence)
        {
            try
            {
                foreach (var effect in sequence)
                {
                    yield return effect;
                    if (view.HasOutcome) break;
                }
            }
            finally { Finish(); }
        }

        private void Finish()
        {
            if (!IsPlaying) return;
            if (view != null) view.ResetCombatantFeedback();
            if (feedback != null) feedback.gameObject.SetActive(false);
            feedbackTarget = CombatantSide.None;
            if (hud != null && !view.HasOutcome) hud.SetStatus(previousStatus);
            if (menu != null) menu.SetPresentationBlocked(false);
            playback = null;
            IsPlaying = false;
        }

        private IEnumerator AnimateAttack(CombatantSide actor)
        {
            hud.SetStatus(actor == CombatantSide.Player ? "Ataque do herói" : "Ataque do inimigo");
            var direction = actor == CombatantSide.Player ? new Vector2(12, 6) : new Vector2(-12, -6);
            view.SetCombatantFeedback(actor, direction / 2, Color.white);
            yield return new WaitForSecondsRealtime(0.09f);
            view.SetCombatantFeedback(actor, direction, Color.white);
            yield return new WaitForSecondsRealtime(0.09f);
            view.SetCombatantFeedback(actor, Vector2.zero, Color.white);
            yield return new WaitForSecondsRealtime(0.09f);
        }

        private IEnumerator AnimateGuard(CombatantSide actor)
        {
            hud.SetStatus(actor == CombatantSide.Player ? "Herói defende" : "Inimigo defende");
            ShowFeedback(actor, "Defesa", new Color32(175, 214, 240, 255));
            view.SetCombatantFeedback(actor, Vector2.zero, new Color(0.6f, 0.8f, 1));
            yield return new WaitForSecondsRealtime(0.35f);
            view.SetCombatantFeedback(actor, Vector2.zero, Color.white);
            feedback.gameObject.SetActive(false);
        }

        private IEnumerator AnimateDamage(CombatantSide target, int amount, bool critical, bool piercing, bool reflection = false)
        {
            string qualifiers = (critical ? " · Crítico" : "") + (piercing ? " · Penetração" : "");
            hud.SetStatus($"{(reflection ? "Reflexão · " : "")}{amount} de dano{qualifiers}");
            ShowFeedback(target, amount == 0 ? "0" : $"−{amount}", critical ? new Color32(245, 198, 82, 255) : Color.white);
            var tint = amount == 0 ? Color.white : new Color(1, 0.45f, 0.4f);
            view.SetCombatantFeedback(target, new Vector2(amount == 0 ? 0 : -3, 0), tint);
            yield return new WaitForSecondsRealtime(0.12f);
            view.SetCombatantFeedback(target, new Vector2(amount == 0 ? 0 : 3, 0), tint);
            yield return new WaitForSecondsRealtime(0.12f);
            view.SetCombatantFeedback(target, Vector2.zero, Color.white);
            yield return new WaitForSecondsRealtime(0.16f);
            feedback.gameObject.SetActive(false);
        }

        private void ShowFeedback(CombatantSide side, string message, Color color)
        {
            feedbackTarget = side;
            feedback.supportRichText = false;
            feedback.text = message;
            feedback.color = color;
            feedback.gameObject.SetActive(true);
            PositionFeedback();
        }

        private void PositionFeedback()
        {
            var combatant = view.GetCombatantRect(feedbackTarget);
            var arena = (RectTransform)combatant.parent;
            var rect = feedback.rectTransform;
            rect.anchoredPosition = new Vector2(
                Mathf.Clamp(combatant.anchoredPosition.x, rect.rect.width / 2, arena.rect.width - rect.rect.width / 2),
                Mathf.Min(combatant.anchoredPosition.y + combatant.rect.height + 8, arena.rect.height - rect.rect.height));
        }
    }
}
