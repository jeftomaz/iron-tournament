using System;
using System.Collections.Generic;
using IronTournament.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IronTournament.Presentation
{
    [DisallowMultipleComponent]
    public sealed class ActionMenu : MonoBehaviour
    {
        [Serializable]
        private sealed class ActionBinding
        {
            public AbilityId ability;
            public Button button;
            [NonSerialized] public UnityAction listener;
        }

        [SerializeField] private ActionBinding[] buttons;
        private readonly HashSet<AbilityId> available = new HashSet<AbilityId>();
        private CombatantId hero = CombatantId.Warrior;
        private bool inputBlocked = true;
        private bool presentationBlocked;
        public event Action<AbilityId> ActionSelected;

        private void OnEnable()
        {
            foreach (var binding in buttons)
            {
                binding.listener = () => Select(binding);
                binding.button.onClick.AddListener(binding.listener);
            }
            Render();
        }

        private void OnDisable()
        {
            foreach (var binding in buttons) binding.button.onClick.RemoveListener(binding.listener);
        }

        public void ConfigureHero(CombatantId id)
        {
            if (id != CombatantId.Warrior && id != CombatantId.Mage) throw new ArgumentOutOfRangeException(nameof(id));
            hero = id;
            Render();
        }

        public void SetAvailableActions(IReadOnlyList<AbilityId> actions, bool inputBlocked)
        {
            if (actions == null) throw new ArgumentNullException(nameof(actions));
            foreach (var action in actions)
                if (action == AbilityId.None || !Enum.IsDefined(typeof(AbilityId), action))
                    throw new ArgumentOutOfRangeException(nameof(actions));
            available.Clear();
            foreach (var action in actions) available.Add(action);
            this.inputBlocked = inputBlocked;
            Render();
        }

        public void SetPresentationBlocked(bool blocked)
        {
            presentationBlocked = blocked;
            Render();
        }

        private void Render()
        {
            if (buttons == null) return;
            int position = 0;
            foreach (var binding in buttons)
            {
                bool mage = hero == CombatantId.Mage;
                bool visible = binding.ability == AbilityId.BasicAttack || (mage
                    ? binding.ability == AbilityId.RevertTurn || binding.ability == AbilityId.RevertBattle
                    : binding.ability == AbilityId.Guard);
                binding.button.gameObject.SetActive(visible);
                binding.button.interactable = available.Contains(binding.ability) && !inputBlocked && !presentationBlocked;
                if (!visible) continue;
                var rect = (RectTransform)binding.button.transform;
                rect.anchorMin = mage && position == 0 ? new Vector2(0, .5f) : new Vector2((position - (mage ? 1 : 0)) * .5f, 0);
                rect.anchorMax = mage && position == 0 ? Vector2.one : new Vector2((position - (mage ? 1 : 0) + 1) * .5f, mage ? .5f : 1);
                rect.pivot = new Vector2(.5f, .5f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(-6, mage ? -6 : 0);
                var caption = binding.button.GetComponentInChildren<Text>();
                caption.fontSize = mage ? 14 : 16;
                caption.text = binding.ability == AbilityId.BasicAttack
                    ? (mage ? "Ataque mágico" : "Atacar") : binding.ability == AbilityId.Guard ? "Defender"
                    : binding.ability == AbilityId.RevertTurn ? "Reverter turno" : "Reverter batalha";
                position++;
            }
        }

        private void Select(ActionBinding binding)
        {
            if (!isActiveAndEnabled || !binding.button.IsInteractable() || !binding.button.gameObject.activeInHierarchy) return;
            inputBlocked = true;
            Render();
            ActionSelected?.Invoke(binding.ability);
        }
    }
}
