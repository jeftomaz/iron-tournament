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
        private enum MenuMode
        {
            Actions,
            Drops
        }

        [SerializeField] private Button attackButton;
        [SerializeField] private Button guardButton;

        private readonly List<Button> buttons = new List<Button>();
        private readonly List<AbilityConfiguration> actions = new List<AbilityConfiguration>();
        private readonly List<ItemConfiguration> drops = new List<ItemConfiguration>();
        private readonly Dictionary<Button, UnityAction> listeners = new Dictionary<Button, UnityAction>();
        private readonly HashSet<AbilityId> available = new HashSet<AbilityId>();
        private bool inputBlocked = true;
        private bool presentationBlocked;
        private MenuMode mode;

        public event Action<AbilityId> ActionSelected;
        public event Action<ItemId> DropSelected;

        public int ActionRows => Math.Max(1, (OptionCount + 1) / 2);

        private void Awake()
        {
            EnsureButtonPool(2);
            if (actions.Count == 0)
            {
                actions.Add(new AbilityConfiguration(AbilityId.BasicAttack, "Atacar", AbilityTarget.Opponent, true));
                actions.Add(new AbilityConfiguration(AbilityId.Guard, "Defender", AbilityTarget.Self, true));
            }
        }

        private void OnEnable()
        {
            AttachListeners();
            Render();
        }

        private void OnDisable() => DetachListeners();

        public void ConfigureActions(IReadOnlyList<AbilityConfiguration> configurations)
        {
            if (configurations == null) throw new ArgumentNullException(nameof(configurations));
            var ids = new HashSet<AbilityId>();
            foreach (var configuration in configurations)
            {
                if (configuration == null || !ids.Add(configuration.Id))
                    throw new ArgumentException("Distinct action configurations are required.", nameof(configurations));
            }
            var listening = isActiveAndEnabled;
            if (listening) DetachListeners();
            mode = MenuMode.Actions;
            actions.Clear();
            drops.Clear();
            foreach (var configuration in configurations) actions.Add(configuration);
            EnsureButtonPool(actions.Count);
            available.Clear();
            if (listening) AttachListeners();
            Render();
        }

        public void ConfigureDrops(IReadOnlyList<ItemConfiguration> offer)
        {
            if (offer == null || offer.Count == 0)
                throw new ArgumentException("At least one drop is required.", nameof(offer));

            var ids = new HashSet<ItemId>();
            foreach (var item in offer)
            {
                if (item == null || !ids.Add(item.Id))
                    throw new ArgumentException("Distinct drop configurations are required.", nameof(offer));
            }

            var listening = isActiveAndEnabled;
            if (listening) DetachListeners();
            mode = MenuMode.Drops;
            actions.Clear();
            drops.Clear();
            foreach (var item in offer) drops.Add(item);
            EnsureButtonPool(drops.Count);
            available.Clear();
            inputBlocked = false;
            if (listening) AttachListeners();
            Render();
        }

        public void SetAvailableActions(IReadOnlyList<AbilityId> availableActions, bool inputBlocked)
        {
            if (availableActions == null) throw new ArgumentNullException(nameof(availableActions));
            foreach (var action in availableActions)
                if (action == AbilityId.None || !Enum.IsDefined(typeof(AbilityId), action) || !HasAction(action))
                    throw new ArgumentOutOfRangeException(nameof(availableActions));
            available.Clear();
            foreach (var action in availableActions) available.Add(action);
            this.inputBlocked = inputBlocked;
            Render();
        }

        public void SetPresentationBlocked(bool blocked)
        {
            presentationBlocked = blocked;
            Render();
        }

        private void EnsureButtonPool(int count)
        {
            if (attackButton == null || guardButton == null)
                throw new InvalidOperationException("Action buttons are required.");
            if (buttons.Count == 0)
            {
                buttons.Add(attackButton);
                buttons.Add(guardButton);
            }
            while (buttons.Count < count)
            {
                var button = Instantiate(attackButton, transform);
                button.onClick.RemoveAllListeners();
                buttons.Add(button);
            }
        }

        private void AttachListeners()
        {
            DetachListeners();
            for (var index = 0; index < OptionCount; index++)
            {
                var button = buttons[index];
                UnityAction listener;
                if (mode == MenuMode.Actions)
                {
                    var action = actions[index].Id;
                    listener = () => Select(action);
                }
                else
                {
                    var item = drops[index].Id;
                    listener = () => SelectDrop(item);
                }
                listeners.Add(button, listener);
                button.onClick.AddListener(listener);
            }
        }

        private void DetachListeners()
        {
            foreach (var listener in listeners) listener.Key.onClick.RemoveListener(listener.Value);
            listeners.Clear();
        }

        private void Render()
        {
            var blocked = inputBlocked || presentationBlocked;
            var rows = ActionRows;
            var firstRowSingle = OptionCount % 2 != 0;
            for (var index = 0; index < buttons.Count; index++)
            {
                var visible = index < OptionCount;
                var button = buttons[index];
                button.gameObject.SetActive(visible);
                if (!visible) continue;
                if (mode == MenuMode.Actions)
                {
                    var action = actions[index];
                    button.name = ButtonName(action.Id);
                    button.interactable = available.Contains(action.Id) && !blocked;
                    var actionLabel = button.GetComponentInChildren<Text>();
                    actionLabel.supportRichText = false;
                    actionLabel.text = action.DisplayName;
                    actionLabel.fontSize = rows > 1 ? 14 : 16;
                }
                else
                {
                    var item = drops[index];
                    button.name = $"Drop{item.Id}";
                    button.interactable = !blocked;
                    var dropLabel = button.GetComponentInChildren<Text>();
                    dropLabel.supportRichText = false;
                    dropLabel.text = item.DisplayName;
                    dropLabel.fontSize = rows > 1 ? 14 : 16;
                }
                Place((RectTransform)button.transform, index, rows, firstRowSingle);
            }
        }

        private int OptionCount => mode == MenuMode.Actions ? actions.Count : drops.Count;

        private bool HasAction(AbilityId action)
        {
            foreach (var configured in actions)
                if (configured.Id == action) return true;
            return false;
        }

        private void Select(AbilityId action)
        {
            if (mode != MenuMode.Actions || !isActiveAndEnabled || !available.Contains(action) || inputBlocked || presentationBlocked) return;
            inputBlocked = true;
            Render();
            ActionSelected?.Invoke(action);
        }

        private void SelectDrop(ItemId item)
        {
            if (mode != MenuMode.Drops || !isActiveAndEnabled || inputBlocked || presentationBlocked) return;
            inputBlocked = true;
            Render();
            DropSelected?.Invoke(item);
        }

        private static string ButtonName(AbilityId ability)
        {
            switch (ability)
            {
                case AbilityId.BasicAttack: return "Attack";
                case AbilityId.Guard: return "Guard";
                case AbilityId.RevertTurn: return "RevertTurn";
                case AbilityId.RevertBattle: return "RevertBattle";
                default: return ability.ToString();
            }
        }

        private static void Place(RectTransform rect, int index, int rows, bool firstRowSingle)
        {
            var row = firstRowSingle && index == 0 ? 0 : (firstRowSingle ? index + 1 : index) / 2;
            var single = firstRowSingle && index == 0;
            var column = single ? 0 : (firstRowSingle ? index - 1 : index) % 2;
            var top = 1f - (float)row / rows;
            var bottom = 1f - (float)(row + 1) / rows;
            rect.anchorMin = new Vector2(single ? 0 : column * .5f, bottom);
            rect.anchorMax = new Vector2(single ? 1 : (column + 1) * .5f, top);
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(-6, -6);
        }
    }
}
