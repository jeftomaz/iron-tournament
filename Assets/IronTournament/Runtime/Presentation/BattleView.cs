using System;
using System.Collections.Generic;
using System.Text;
using IronTournament.Content;
using IronTournament.Core;
using UnityEngine;
using UnityEngine.UI;

namespace IronTournament.Presentation
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class BattleView : MonoBehaviour
    {
        [Serializable]
        private sealed class CombatantVisual
        {
            public CombatantDefinition definition;
            public EncounterDefinition encounter;
            public Sprite east;
            public Sprite northEast;
            public Sprite west;
            public Sprite southWest;
            public Sprite background;
            public string caption;
        }

        [SerializeField] private CombatantVisual[] combatants;
        [SerializeField] private CombatantId selectedPlayer = CombatantId.Warrior;
        [SerializeField] private CombatantId selectedOpponent = CombatantId.Goblin;
        [SerializeField] private Text heroName;
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
        [SerializeField] private BattleHud hud;
        [SerializeField] private RectTransform battleStatus;
        [SerializeField] private Text dropDetails;
        [SerializeField] private ActionMenu actionMenu;

        private Vector2 previousViewport = new Vector2(-1, -1);
        private bool combatPresentationStarted;
        private bool refreshSelection;
        private Vector2 playerFeedbackOffset;
        private Vector2 opponentFeedbackOffset;
        private BattlePhase outcome;
        private int actionRows = -1;

        public bool HasOutcome => outcome == BattlePhase.Victory || outcome == BattlePhase.Defeat;
        public bool IsCombatStarted => combatPresentationStarted;
        public event Action CombatStarted;

        public CombatantDefinition SelectedPlayer { get; private set; }
        public EncounterDefinition SelectedEncounter { get; private set; }

        public void SelectPlayer(CombatantId id)
        {
            if (combatPresentationStarted) throw new InvalidOperationException("The player cannot change during combat.");
            var visual = FindVisual(id);
            ValidateVisual(visual, id);
            selectedPlayer = id;
            SelectedPlayer = visual.definition;
            player.sprite = visual.east;
            heroName.supportRichText = false;
            heroName.text = visual.definition.DisplayName;
            RequestLayout();
        }

        public void SelectEncounter(CombatantId id)
        {
            if (combatPresentationStarted) throw new InvalidOperationException("An encounter cannot change during combat.");
            ApplyEncounterSelection(id, false);
        }

        public void ShowDropChoice(IReadOnlyList<ItemConfiguration> offer)
        {
            if (!combatPresentationStarted || outcome != BattlePhase.Victory)
                throw new InvalidOperationException("A victorious encounter is required for a drop choice.");
            if (offer == null || offer.Count == 0)
                throw new ArgumentException("At least one drop is required.", nameof(offer));

            actionMenu.gameObject.SetActive(true);
            battleStatus.gameObject.SetActive(true);
            dropDetails.supportRichText = false;
            dropDetails.text = DescribeOffer(offer);
            dropDetails.gameObject.SetActive(true);
            hud.SetStatus("Escolha um saque");
            RequestLayout();
        }

        public void BeginNextEncounter(CombatantId id)
        {
            if (!combatPresentationStarted)
                throw new InvalidOperationException("Combat must be started before advancing an encounter.");

            outcome = BattlePhase.None;
            ApplyEncounterSelection(id, true);
            var playerVisual = FindVisual(SelectedPlayer.Id);
            player.sprite = playerVisual.northEast;
            startButton.gameObject.SetActive(false);
            hud.Clear();
            battleStatus.gameObject.SetActive(true);
            dropDetails.gameObject.SetActive(false);
            actionMenu.gameObject.SetActive(true);
            actionMenu.SetAvailableActions(Array.Empty<AbilityId>(), true);
            ResetCombatantFeedback();
            RequestLayout();
        }

        public void ShowCampaignComplete()
        {
            if (outcome != BattlePhase.Victory)
                throw new InvalidOperationException("A victorious encounter is required to complete the campaign.");

            hud.SetStatus("Campanha concluída!");
            dropDetails.gameObject.SetActive(false);
            RequestLayout();
        }

        private void ApplyEncounterSelection(CombatantId id, bool combatPose)
        {
            var visual = FindVisual(id);
            ValidateVisual(visual, id);
            if (visual.encounter == null || !ContentValidator.Validate(visual.encounter).IsValid ||
                visual.encounter.Opponent.Id != id)
                throw new ArgumentException("A valid encounter matching the opponent is required.", nameof(id));
            selectedOpponent = id;
            SelectedEncounter = visual.encounter;
            opponent.sprite = combatPose ? visual.southWest : visual.west;
            background.sprite = visual.background;
            opponentName.supportRichText = encounter.supportRichText = false;
            opponentName.text = visual.definition.DisplayName;
            encounter.text = visual.caption;
            RequestLayout();
        }

        internal void ShowOutcome(BattlePhase result)
        {
            if (result != BattlePhase.Victory && result != BattlePhase.Defeat)
                throw new ArgumentOutOfRangeException(nameof(result));
            outcome = result;
            startButton.gameObject.SetActive(false);
            actionMenu.SetAvailableActions(Array.Empty<AbilityId>(), true);
            actionMenu.gameObject.SetActive(false);
            dropDetails.gameObject.SetActive(false);
            battleStatus.gameObject.SetActive(true);
            hud.SetStatus(result == BattlePhase.Victory ? "Vitória!" : "Derrota");
            ResetCombatantFeedback();
            RequestLayout();
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
            RequestLayout();
            LateUpdate();
        }

        public void ResetCombatantFeedback()
        {
            var defeatedTint = new Color(.45f, .45f, .45f, .65f);
            if (player != null) SetCombatantFeedback(CombatantSide.Player, Vector2.zero,
                outcome == BattlePhase.Defeat ? defeatedTint : Color.white);
            if (opponent != null) SetCombatantFeedback(CombatantSide.Enemy, Vector2.zero,
                outcome == BattlePhase.Victory ? defeatedTint : Color.white);
        }

        private void OnEnable()
        {
            if (combatants != null && combatants.Length > 0 && !combatPresentationStarted)
            {
                SelectPlayer(selectedPlayer);
                SelectEncounter(selectedOpponent);
            }
            RequestLayout();
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
                SelectedPlayer == null || SelectedEncounter == null) return;
            var playerVisual = FindVisual(SelectedPlayer.Id);
            var opponentVisual = FindVisual(SelectedEncounter.Opponent.Id);
            if (playerVisual.northEast == null || opponentVisual.southWest == null) return;
            combatPresentationStarted = true;
            player.sprite = playerVisual.northEast;
            opponent.sprite = opponentVisual.southWest;
            startButton.gameObject.SetActive(false);
            hud.Clear();
            battleStatus.gameObject.SetActive(true);
            dropDetails.gameObject.SetActive(false);
            actionMenu.gameObject.SetActive(true);
            actionMenu.SetAvailableActions(Array.Empty<AbilityId>(), true);
            RequestLayout();
            CombatStarted?.Invoke();
        }

        private void OnValidate()
        {
            RequestLayout();
            refreshSelection = !Application.isPlaying;
        }

        private void LateUpdate()
        {
            if (refreshSelection)
            {
                refreshSelection = false;
                if (combatants != null && combatants.Length > 0)
                {
                    SelectPlayer(selectedPlayer);
                    SelectEncounter(selectedOpponent);
                }
            }
            if (actionMenu != null && actionRows != actionMenu.ActionRows) RequestLayout();
            if (composition == null || !(composition.parent is RectTransform viewport)) return;
            var size = viewport.rect.size;
            if (size.x <= 0 || size.y <= 0 || size == previousViewport) return;
            if (title == null || encounter == null || arena == null || background == null || player == null ||
                opponent == null || playerLabel == null || opponentLabel == null || startButton == null || hud == null ||
                battleStatus == null || dropDetails == null || actionMenu == null) return;

            previousViewport = size;
            actionRows = actionMenu.ActionRows;
            var extraRows = Mathf.Max(0, actionRows - 1);
            var showingDrops = HasOutcome && actionMenu.gameObject.activeSelf && dropDetails.gameObject.activeSelf;
            var portrait = size.y > size.x;
            var reference = portrait ? new Vector2(360, 640) : new Vector2(1280, 720);
            var scale = Mathf.Min(size.x / reference.x, size.y / reference.y);
            if (scale >= 1) scale = Mathf.Floor(scale);
            composition.anchorMin = composition.anchorMax = composition.pivot = new Vector2(.5f, .5f);
            composition.anchoredPosition = Vector2.zero;
            composition.sizeDelta = reference;
            composition.localScale = Vector3.one * scale;

            if (portrait)
            {
                Place(title.rectTransform, 16, 24, 328, 32);
                Place(encounter.rectTransform, 16, 66, 328, 20);
                Place(arena, 16, 106, 328, 310 - extraRows * 24);
                Place(playerLabel, 16, 432 - extraRows * 24, 156, 76);
                Place(opponentLabel, 188, 432 - extraRows * 24, 156, 76);
                Place((RectTransform)startButton.transform, 16, 536, 328, 48);
                if (showingDrops)
                {
                    Place(battleStatus, 16, 516 - extraRows * 24, 328, 20);
                    Place(dropDetails.rectTransform, 16, 540 - extraRows * 24, 328, 28);
                    Place((RectTransform)actionMenu.transform, 16, 574 - extraRows * 28, 328, 48 + extraRows * 42);
                }
                else
                {
                    Place(battleStatus, 16, 520 - extraRows * 24, 328, HasOutcome ? 80 : 20);
                    Place((RectTransform)actionMenu.transform, 16, 552 - extraRows * 24, 328, 48 + extraRows * 42);
                }
            }
            else
            {
                Place(title.rectTransform, 40, 24, 1200, 42);
                Place(encounter.rectTransform, 40, 76, 1200, 24);
                var dropOffset = showingDrops ? extraRows * 42 : extraRows * 14;
                Place(arena, 40, 116, 1200, (combatPresentationStarted ? 384 : 454) - dropOffset);
                var labelOffset = showingDrops ? extraRows * 48 : extraRows * 14;
                Place(playerLabel, 40, (combatPresentationStarted ? 516 : 590) - labelOffset, 580, 76);
                Place(opponentLabel, 660, (combatPresentationStarted ? 516 : 590) - labelOffset, 580, 76);
                Place((RectTransform)startButton.transform, 440, 678, 400, 32);
                if (showingDrops)
                {
                    Place(battleStatus, 40, 602 - extraRows * 42, 1200, 20);
                    Place(dropDetails.rectTransform, 40, 626 - extraRows * 42, 1200, 24);
                    Place((RectTransform)actionMenu.transform, 340, 656 - extraRows * 42, 600, 48 + extraRows * 42);
                }
                else
                {
                    Place(battleStatus, 40, 606 - extraRows * 14, 1200, HasOutcome ? 86 : 24);
                    Place((RectTransform)actionMenu.transform, 340, 644 - extraRows * 20, 600, 48 + extraRows * 42);
                }
            }

            title.fontSize = portrait ? 26 : 36;
            encounter.fontSize = portrait ? 12 : 14;
            var status = battleStatus.GetComponent<Text>();
            status.fontSize = HasOutcome ? (portrait ? 30 : 36) : 14;
            status.color = outcome == BattlePhase.Victory ? new Color32(245, 198, 82, 255)
                : outcome == BattlePhase.Defeat ? new Color32(242, 130, 120, 255) : Color.white;
            FitBackground();
            PlaceCombatant(player, portrait ? .27f : .32f, portrait ? 3 : 5, scale,
                combatPresentationStarted ? .09f : .12f, playerFeedbackOffset);
            PlaceCombatant(opponent, portrait ? .73f : .68f, portrait ? 3 : 5, scale,
                combatPresentationStarted ? .34f : .12f, opponentFeedbackOffset);
        }

        private CombatantVisual FindVisual(CombatantId id)
        {
            if (combatants != null)
                foreach (var visual in combatants)
                    if (visual?.definition != null && visual.definition.Id == id) return visual;
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        private static void ValidateVisual(CombatantVisual visual, CombatantId id)
        {
            if (visual == null)
                throw new ArgumentException("Complete combatant visual references are required.", nameof(id));

            var content = ContentValidator.Validate(visual.definition);
            if (!content.IsValid)
                throw new ArgumentException(string.Join(" | ", content.Errors), nameof(id));

            if (visual.east == null) throw new ArgumentException("East sprite is required.", nameof(id));
            if (visual.northEast == null) throw new ArgumentException("North-east sprite is required.", nameof(id));
            if (visual.west == null) throw new ArgumentException("West sprite is required.", nameof(id));
            if (visual.southWest == null) throw new ArgumentException("South-west sprite is required.", nameof(id));
            if (visual.background == null) throw new ArgumentException("Background sprite is required.", nameof(id));
            if (string.IsNullOrWhiteSpace(visual.caption) || visual.caption.Length > 80)
                throw new ArgumentException("A valid combatant caption is required.", nameof(id));
            foreach (var character in visual.caption)
                if (char.IsControl(character)) throw new ArgumentException("Invalid combatant caption.", nameof(id));
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

        private void RequestLayout() => previousViewport = new Vector2(-1, -1);

        private void FitBackground()
        {
            if (background.sprite == null) return;
            var source = background.sprite.rect.size;
            var cover = Mathf.Max(arena.rect.width / source.x, arena.rect.height / source.y);
            var rect = background.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = source * cover;
        }

        private void PlaceCombatant(Image image, float horizontalPosition, int referenceScale, float scale,
            float groundPosition, Vector2 feedbackOffset)
        {
            if (image.sprite == null) return;
            var pixelScale = Mathf.Max(1, Mathf.Floor(referenceScale * scale));
            var availableHeight = arena.rect.height * (1 - groundPosition) - 8;
            pixelScale = Mathf.Min(pixelScale, Mathf.Max(1, Mathf.Floor(availableHeight * scale / image.sprite.rect.height)));
            var rect = image.rectTransform;
            rect.anchorMin = rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(.5f, 0);
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

        private static string DescribeOffer(IReadOnlyList<ItemConfiguration> offer)
        {
            var result = new StringBuilder();
            for (var index = 0; index < offer.Count; index++)
            {
                var item = offer[index] ?? throw new ArgumentException("Drop entries are required.", nameof(offer));
                if (index > 0) result.Append('\n');
                result.Append(item.DisplayName).Append(" · ").Append(DescribeItem(item));
            }
            return result.ToString();
        }

        private static string DescribeItem(ItemConfiguration item)
        {
            switch (item.Id)
            {
                case ItemId.HealingPotion: return "recupera 40 HP";
                case ItemId.LifeElixir: return "recupera todo o HP";
                case ItemId.HopeScroll: return "cura 50% com HP baixo";
                case ItemId.FlameCloak: return "causa 5 de dano por turno";
                case ItemId.BrotherhoodHorn: return "bloqueia golpe letal";
            }

            var result = new StringBuilder();
            for (var index = 0; index < item.Modifiers.Count; index++)
            {
                if (index > 0) result.Append(" · ");
                var modifier = item.Modifiers[index];
                switch (modifier.Kind)
                {
                    case ItemModifierKind.MaximumHealth: result.Append("HP máx. "); break;
                    case ItemModifierKind.Attack: result.Append("ATQ "); break;
                    case ItemModifierKind.Defense: result.Append("DEF "); break;
                    default: throw new ArgumentOutOfRangeException();
                }
                result.Append(modifier.Amount >= 0 ? "+" : string.Empty).Append(modifier.Amount);
            }
            return result.Length > 0 ? result.ToString() : "efeito especial";
        }
    }
}
