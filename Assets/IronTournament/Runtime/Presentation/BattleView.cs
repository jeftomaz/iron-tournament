using System;
using IronTournament.Core;
using IronTournament.Content;
using UnityEngine;
using UnityEngine.UI;

namespace IronTournament.Presentation
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class BattleView : MonoBehaviour
    {
        [Serializable]
        private sealed class EncounterVisual
        {
            public EncounterDefinition definition;
            public Sprite introduction;
            public Sprite combat;
            public Sprite background;
            public string caption;
        }

        [Serializable]
        private sealed class HeroVisual
        {
            public CombatantDefinition definition;
            public Sprite introduction;
            public Sprite combat;
        }

        [SerializeField] private HeroVisual[] heroes;
        [SerializeField] private CombatantId selectedHero = CombatantId.Warrior;
        [SerializeField] private Text heroName;
        [SerializeField] private EncounterVisual[] encounters;
        [SerializeField] private CombatantId selectedOpponent = CombatantId.Goblin;
        [SerializeField] private Text opponentName;
        [SerializeField] private RectTransform composition;
        [SerializeField] private Text title;
        [SerializeField] private Text encounter;
        [SerializeField] private RectTransform arena;
        [SerializeField] private Image background;
        [SerializeField] private Image player;
        [SerializeField] private Image opponent;
        [SerializeField] private RectTransform playerLabel;
        [SerializeField] private RectTransform opponentLabel;
        [SerializeField] private Button startButton;
        [SerializeField] private Sprite combatPlayerSprite;
        [SerializeField] private Sprite combatOpponentSprite;
        [SerializeField] private BattleHud hud;
        [SerializeField] private RectTransform battleStatus;
        [SerializeField] private ActionMenu actionMenu;

        private Vector2 previousViewport = new Vector2(-1, -1);
        private bool combatPresentationStarted;
        private bool refreshSelection;
        private Vector2 playerFeedbackOffset;
        private Vector2 opponentFeedbackOffset;
        private BattlePhase outcome;

        public bool HasOutcome => outcome == BattlePhase.Victory || outcome == BattlePhase.Defeat;
        public bool IsCombatStarted => combatPresentationStarted;
        public event Action CombatStarted;

        public EncounterDefinition SelectedEncounter { get; private set; }
        public CombatantDefinition SelectedHero { get; private set; }

        public void SelectHero(CombatantId id)
        {
            if (combatPresentationStarted) throw new InvalidOperationException("The hero cannot change during combat.");
            HeroVisual visual = null;
            if (heroes != null)
                foreach (var candidate in heroes)
                    if (candidate?.definition != null && candidate.definition.Id == id) { visual = candidate; break; }
            if (visual == null) throw new ArgumentOutOfRangeException(nameof(id));
            if (!ContentValidator.Validate(visual.definition).IsValid || visual.definition.Side != CombatantSide.Player ||
                visual.introduction == null || visual.combat == null)
                throw new ArgumentException("A valid hero and complete visual references are required.", nameof(id));
            actionMenu.ConfigureHero(id);
            selectedHero = id;
            SelectedHero = visual.definition;
            player.sprite = visual.introduction;
            combatPlayerSprite = visual.combat;
            heroName.supportRichText = false;
            heroName.text = visual.definition.DisplayName;
            previousViewport = new Vector2(-1, -1);
            LateUpdate();
        }

        public void SelectEncounter(CombatantId id)
        {
            if (combatPresentationStarted) throw new InvalidOperationException("An encounter cannot change during combat.");
            EncounterVisual visual = null;
            if (encounters != null)
                foreach (var candidate in encounters)
                    if (candidate?.definition?.Opponent != null && candidate.definition.Opponent.Id == id)
                    {
                        visual = candidate;
                        break;
                    }
            if (visual == null) throw new ArgumentOutOfRangeException(nameof(id));
            if (!ContentValidator.Validate(visual.definition).IsValid || visual.introduction == null ||
                visual.combat == null || visual.background == null || string.IsNullOrWhiteSpace(visual.caption) ||
                visual.caption.Length > 80)
                throw new ArgumentException("A valid encounter and complete visual references are required.", nameof(id));
            foreach (char character in visual.caption)
                if (char.IsControl(character)) throw new ArgumentException("Invalid encounter caption.", nameof(id));
            selectedOpponent = id;
            SelectedEncounter = visual.definition;
            opponent.sprite = visual.introduction;
            combatOpponentSprite = visual.combat;
            background.sprite = visual.background;
            opponentName.supportRichText = encounter.supportRichText = false;
            opponentName.text = visual.definition.Opponent.DisplayName;
            encounter.text = visual.caption;
            previousViewport = new Vector2(-1, -1);
            LateUpdate();
        }

        internal void ShowOutcome(BattlePhase result)
        {
            if (result != BattlePhase.Victory && result != BattlePhase.Defeat)
                throw new ArgumentOutOfRangeException(nameof(result));
            outcome = result;
            startButton.gameObject.SetActive(false);
            actionMenu.SetAvailableActions(Array.Empty<AbilityId>(), true);
            actionMenu.gameObject.SetActive(false);
            battleStatus.gameObject.SetActive(true);
            hud.SetStatus(result == BattlePhase.Victory ? "Vitória!" : "Derrota");
            ResetCombatantFeedback();
            previousViewport = new Vector2(-1, -1);
            LateUpdate();
        }

        public RectTransform GetCombatantRect(CombatantSide side) => CombatantImage(side).rectTransform;

        public void SetCombatantFeedback(CombatantSide side, Vector2 offset, Color tint)
        {
            if (!IsFinite(offset.x) || !IsFinite(offset.y))
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (!IsFinite(tint.r) || !IsFinite(tint.g) || !IsFinite(tint.b) || !IsFinite(tint.a))
                throw new ArgumentOutOfRangeException(nameof(tint));
            var image = CombatantImage(side);
            if (side == CombatantSide.Player) playerFeedbackOffset = offset;
            else opponentFeedbackOffset = offset;
            image.color = tint;
            previousViewport = new Vector2(-1, -1);
            LateUpdate();
        }

        public void ResetCombatantFeedback()
        {
            var defeatedTint = new Color(0.45f, 0.45f, 0.45f, 0.65f);
            if (player != null) SetCombatantFeedback(CombatantSide.Player, Vector2.zero,
                outcome == BattlePhase.Defeat ? defeatedTint : Color.white);
            if (opponent != null) SetCombatantFeedback(CombatantSide.Enemy, Vector2.zero,
                outcome == BattlePhase.Victory ? defeatedTint : Color.white);
        }

        private Image CombatantImage(CombatantSide side)
        {
            switch (side)
            {
                case CombatantSide.Player: return player;
                case CombatantSide.Enemy: return opponent;
                default: throw new ArgumentOutOfRangeException(nameof(side));
            }
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        private void OnEnable()
        {
            if (heroes != null && heroes.Length > 0 && !combatPresentationStarted) SelectHero(selectedHero);
            if (encounters != null && encounters.Length > 0 && !combatPresentationStarted)
                SelectEncounter(selectedOpponent);
            previousViewport = new Vector2(-1, -1);
            if (Application.isPlaying && startButton != null)
                startButton.onClick.AddListener(StartCombatPresentation);
        }

        private void OnDisable()
        {
            if (startButton != null) startButton.onClick.RemoveListener(StartCombatPresentation);
        }

        private void StartCombatPresentation()
        {
            if (combatPresentationStarted || player == null || opponent == null ||
                combatPlayerSprite == null || combatOpponentSprite == null) return;
            combatPresentationStarted = true;
            player.sprite = combatPlayerSprite;
            opponent.sprite = combatOpponentSprite;
            startButton.gameObject.SetActive(false);
            hud.Clear();
            battleStatus.gameObject.SetActive(true);
            actionMenu.gameObject.SetActive(true);
            actionMenu.SetAvailableActions(System.Array.Empty<IronTournament.Core.AbilityId>(), true);
            previousViewport = new Vector2(-1, -1);
            LateUpdate();
            CombatStarted?.Invoke();
        }

        private void OnValidate()
        {
            previousViewport = new Vector2(-1, -1);
            refreshSelection = !Application.isPlaying;
        }

        private void LateUpdate()
        {
            if (refreshSelection)
            {
                refreshSelection = false;
                if (heroes != null && heroes.Length > 0) SelectHero(selectedHero);
                if (encounters != null && encounters.Length > 0) SelectEncounter(selectedOpponent);
            }
            if (composition == null || !(composition.parent is RectTransform viewport)) return;
            var size = viewport.rect.size;
            if (size.x <= 0 || size.y <= 0 || size == previousViewport) return;
            if (title == null || encounter == null || arena == null || background == null ||
                player == null || opponent == null || playerLabel == null || opponentLabel == null || startButton == null ||
                hud == null || battleStatus == null || actionMenu == null) return;

            previousViewport = size;
            bool portrait = size.y > size.x;
            bool mage = combatPresentationStarted && SelectedHero?.Id == CombatantId.Mage;
            var reference = portrait ? new Vector2(360, 640) : new Vector2(1280, 720);
            float scale = Mathf.Min(size.x / reference.x, size.y / reference.y);
            if (scale >= 1) scale = Mathf.Floor(scale);
            composition.anchorMin = composition.anchorMax = composition.pivot = new Vector2(0.5f, 0.5f);
            composition.anchoredPosition = Vector2.zero;
            composition.sizeDelta = reference;
            composition.localScale = Vector3.one * scale;

            if (portrait)
            {
                Place(title.rectTransform, 16, 24, 328, 32);
                Place(encounter.rectTransform, 16, 66, 328, 20);
                Place(arena, 16, 106, 328, mage ? 286 : 310);
                Place(playerLabel, 16, mage ? 408 : 432, 156, 76);
                Place(opponentLabel, 188, mage ? 408 : 432, 156, 76);
                Place((RectTransform)startButton.transform, 16, 536, 328, 48);
                Place(battleStatus, 16, mage ? 496 : 520, 328, HasOutcome ? 80 : 20);
                Place((RectTransform)actionMenu.transform, 16, mage ? 528 : 552, 328, mage ? 90 : 48);
            }
            else
            {
                Place(title.rectTransform, 40, 24, 1200, 42);
                Place(encounter.rectTransform, 40, 76, 1200, 24);
                Place(arena, 40, 116, 1200, combatPresentationStarted ? (mage ? 370 : 384) : 454);
                Place(playerLabel, 40, combatPresentationStarted ? (mage ? 502 : 516) : 590, 580, 76);
                Place(opponentLabel, 660, combatPresentationStarted ? (mage ? 502 : 516) : 590, 580, 76);
                Place((RectTransform)startButton.transform, 440, 678, 400, 32);
                Place(battleStatus, 40, mage ? 592 : 606, 1200, HasOutcome ? 86 : 24);
                Place((RectTransform)actionMenu.transform, 340, mage ? 624 : 644, 600, mage ? 90 : 48);
            }

            title.fontSize = portrait ? 26 : 36;
            encounter.fontSize = portrait ? 12 : 14;
            var status = battleStatus.GetComponent<Text>();
            status.fontSize = HasOutcome ? (portrait ? 30 : 36) : 14;
            status.color = outcome == BattlePhase.Victory ? new Color32(245, 198, 82, 255)
                : outcome == BattlePhase.Defeat ? new Color32(242, 130, 120, 255) : Color.white;
            FitBackground();
            PlaceCombatant(player, portrait ? 0.27f : 0.32f, portrait ? 3 : 5, scale,
                combatPresentationStarted ? 0.09f : 0.12f, playerFeedbackOffset);
            PlaceCombatant(opponent, portrait ? 0.73f : 0.68f, portrait ? 3 : 5, scale,
                combatPresentationStarted ? 0.34f : 0.12f, opponentFeedbackOffset);
        }

        private void FitBackground()
        {
            if (background.sprite == null) return;
            var source = background.sprite.rect.size;
            float cover = Mathf.Max(arena.rect.width / source.x, arena.rect.height / source.y);
            var rect = background.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = source * cover;
        }

        private void PlaceCombatant(Image image, float horizontalPosition, int referenceScale, float scale, float groundPosition, Vector2 feedbackOffset)
        {
            if (image.sprite == null) return;
            // Round in screen pixels even when a small Game View must scale the composition down.
            float pixelScale = Mathf.Max(1, Mathf.Floor(referenceScale * scale));
            float availableHeight = arena.rect.height * (1 - groundPosition) - 8;
            pixelScale = Mathf.Min(pixelScale, Mathf.Max(1, Mathf.Floor(availableHeight * scale / image.sprite.rect.height)));
            var rect = image.rectTransform;
            rect.anchorMin = rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0);
            rect.anchoredPosition = new Vector2(
                Mathf.Round((arena.rect.width * horizontalPosition + feedbackOffset.x) * scale) / scale,
                Mathf.Round((arena.rect.height * groundPosition + feedbackOffset.y) * scale) / scale);
            rect.sizeDelta = image.sprite.rect.size * pixelScale / scale;
        }

        private static void Place(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }
    }
}
