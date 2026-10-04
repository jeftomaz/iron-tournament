using System;
using IronTournament.Core;
using UnityEngine;
using UnityEngine.UI;

namespace IronTournament.Presentation
{
    [DisallowMultipleComponent]
    public sealed class BattleHud : MonoBehaviour
    {
        [Serializable]
        private sealed class HealthDisplay
        {
            [SerializeField] private Text value;
            [SerializeField] private RectTransform fill;

            public void Render(int current, int maximum)
            {
                value.text = maximum == 0 ? "HP — / —" : $"HP {current} / {maximum}";
                fill.anchorMax = new Vector2(maximum == 0 ? 0 : (float)current / maximum, 1);
            }
        }

        [SerializeField] private HealthDisplay player;
        [SerializeField] private HealthDisplay opponent;
        [SerializeField] private Text status;

        public string StatusMessage => status.text;

        private void Awake() => Clear();

        public void Clear()
        {
            player.Render(0, 0);
            opponent.Render(0, 0);
            SetStatus("Preparando combate…");
        }

        public void ShowHealth(CombatantSide side, int current, int maximum)
        {
            if (maximum <= 0) throw new ArgumentOutOfRangeException(nameof(maximum));
            if (current < 0 || current > maximum) throw new ArgumentOutOfRangeException(nameof(current));
            switch (side)
            {
                case CombatantSide.Player: player.Render(current, maximum); break;
                case CombatantSide.Enemy: opponent.Render(current, maximum); break;
                default: throw new ArgumentOutOfRangeException(nameof(side));
            }
        }

        public void SetStatus(string message)
        {
            if (string.IsNullOrWhiteSpace(message) || message.Length > 80)
                throw new ArgumentException("A status of 1 to 80 characters is required.", nameof(message));
            foreach (char character in message)
                if (char.IsControl(character)) throw new ArgumentException("Control characters are not allowed.", nameof(message));
            status.supportRichText = false;
            status.text = message;
        }
    }
}
