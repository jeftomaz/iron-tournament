using System;
using System.Collections.Generic;
using IronTournament.Core;
using UnityEngine;
using UnityEngine.UI;

namespace IronTournament.Presentation
{
    [DisallowMultipleComponent]
    public sealed class ActionMenu : MonoBehaviour
    {
        [SerializeField] private Button attackButton;
        [SerializeField] private Button guardButton;

        private bool attackAvailable;
        private bool guardAvailable;
        private bool inputBlocked = true;
        private bool presentationBlocked;

        public event Action<AbilityId> ActionSelected;

        private void OnEnable()
        {
            attackButton.onClick.AddListener(SelectAttack);
            guardButton.onClick.AddListener(SelectGuard);
        }

        private void OnDisable()
        {
            attackButton.onClick.RemoveListener(SelectAttack);
            guardButton.onClick.RemoveListener(SelectGuard);
        }

        public void SetAvailableActions(IReadOnlyList<AbilityId> actions, bool inputBlocked)
        {
            if (actions == null) throw new ArgumentNullException(nameof(actions));
            bool nextAttackAvailable = false;
            bool nextGuardAvailable = false;
            foreach (var action in actions)
            {
                if (action == AbilityId.None || !Enum.IsDefined(typeof(AbilityId), action))
                    throw new ArgumentOutOfRangeException(nameof(actions));
                nextAttackAvailable |= action == AbilityId.BasicAttack;
                nextGuardAvailable |= action == AbilityId.Guard;
            }
            attackAvailable = nextAttackAvailable;
            guardAvailable = nextGuardAvailable;
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
            bool blocked = inputBlocked || presentationBlocked;
            attackButton.interactable = attackAvailable && !blocked;
            guardButton.interactable = guardAvailable && !blocked;
        }

        private void SelectAttack() => Select(attackButton, AbilityId.BasicAttack);
        private void SelectGuard() => Select(guardButton, AbilityId.Guard);

        private void Select(Button button, AbilityId action)
        {
            if (!isActiveAndEnabled || !button.IsInteractable() || !button.gameObject.activeInHierarchy) return;
            // Block repeated clicks until the controller supplies the next set of actions.
            inputBlocked = true;
            Render();
            ActionSelected?.Invoke(action);
        }
    }
}
