using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IronTournament.Presentation
{
    public enum StoryButtonStyle { Primary, Secondary, Inventory }

    public sealed class StoryView : MonoBehaviour
    {
        private static readonly Color Ink = new Color32(20, 19, 27, 255);
        private static readonly Color Paper = new Color32(243, 233, 200, 255);
        private static readonly Color Gold = new Color32(211, 173, 96, 255);
        private Canvas canvas;
        private CanvasScaler scaler;
        private Font font;
        private Font bodyFont;
        private Font titleFont;
        private Sprite buttonSkin, panelSkin;
        private AudioSource sounds;
        private readonly Dictionary<string, AudioClip> soundClips = new Dictionary<string, AudioClip>();
        private RectTransform surface, scene, information, actions;
        private Text body;
        private Coroutine typing;
        private string fullText;
        private Vector2 size;
        public event Action Resized;
        public bool IsTyping => typing != null;
        private Vector2 ViewSize => canvas != null ? ((RectTransform)canvas.transform).rect.size : Vector2.zero;
        public bool Portrait => ViewSize.y > ViewSize.x;
        public RectTransform SceneArea => scene;

        public void Initialize()
        {
            sounds = gameObject.AddComponent<AudioSource>();
            sounds.playOnAwake = false;
            foreach (var name in new[] { "Confirm", "Slash", "Magic", "Hit", "Block", "Defeat", "Heal" })
                soundClips[name] = Resources.Load<AudioClip>("StoryAudio/" + name);
            buttonSkin = LoadSkin("StoryUi/Button");
            panelSkin = LoadSkin("StoryUi/Panel");
            var fallback = Resources.Load<Font>("StoryFonts/Inter-Regular");
            font = Resources.Load<Font>("StoryFonts/Saltblackcombat-Regular") ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            bodyFont = Resources.Load<Font>("StoryFonts/Pix32") ?? font;
            titleFont = Resources.Load<Font>("StoryFonts/Deutsch") ?? font;
            if (fallback != null && font.fontNames.Length > 0 && fallback.fontNames.Length > 0)
            {
                font.fontNames = new[] { font.fontNames[0], fallback.fontNames[0] };
                if (bodyFont.fontNames.Length > 0)
                    bodyFont.fontNames = new[] { bodyFont.fontNames[0], fallback.fontNames[0] };
            }
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 100;
            canvas.pixelPerfect = true;
            scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            gameObject.AddComponent<GraphicRaycaster>();
            size = ViewSize;
            SetScale();
        }

        private static Sprite LoadSkin(string path)
        {
            var texture = Resources.Load<Texture2D>(path);
            if (texture == null) return null;
            texture.filterMode = FilterMode.Point;
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f),
                100, 0, SpriteMeshType.FullRect, new Vector4(8, 8, 8, 8));
        }

        private void SetScale() => scaler.referenceResolution = Portrait ? new Vector2(360, 640) : new Vector2(1280, 720);
        private void LateUpdate()
        {
            var current = ViewSize;
            if (current == size || current.x <= 0 || current.y <= 0 || scaler == null) return;
            size = current; SetScale(); Resized?.Invoke();
        }

        public void Draw(Sprite background, string title, string subtitle, string inventory = "", bool forest = false, bool showChapter = true)
        {
            CompleteText();
            if (surface != null) { surface.gameObject.SetActive(false); Destroy(surface.gameObject); }
            surface = Rect("StorySurface", transform, 0, 0, 1, 1);
            Panel(surface, 0, 0, 1, 1, Ink);
            var gameName = Label(surface, "IRON TOURNAMENT", .04f, .947f, .7f, .032f, Portrait ? 14 : 17, Gold);
            gameName.font = titleFont;
            var heading = Label(surface, title, .04f, Portrait ? .891f : .87f, .92f, Portrait ? .05f : .075f, Portrait ? 23 : 29, Paper);
            heading.verticalOverflow = VerticalWrapMode.Overflow;
            if (title == "Iron Tournament") heading.font = titleFont;
            var subtitleFont = subtitle.Length > 45 ? bodyFont : font;
            if (Portrait)
            {
                scene = Rect("Scene", surface, 10f/360, 206f/640, 340f/360, 340f/640);
                information = Rect("Information", surface, .028f, .018f, .944f, .29f);
                Label(surface, subtitle, .04f, .855f, .92f, .03f, 11, Gold, subtitleFont);
            }
            else
            {
                scene = Rect("Scene", surface, 340f/1280, 22f/720, 600f/1280, 600f/720);
                information = Rect("Information", surface, .755f, .03f, .23f, .82f);
                var journal = Rect("Journal", surface, .02f, .03f, .225f, .82f);
                Panel(journal, 0, 0, 1, 1, new Color32(25, 34, 35, 255));
                if (showChapter) Label(journal, forest ? "CAPÍTULO I\nA FLORESTA" : "CAPÍTULO I\nA VILA", .08f, .80f, .84f, .14f, 24, Gold);
                Label(journal, subtitle, .08f, .61f, .84f, .17f, 17, Paper, subtitleFont);
                Label(journal, inventory, .08f, .14f, .84f, .42f, 17, Paper, bodyFont);
            }
            var sceneBounds = scene;
            sceneBounds.name = "SceneBounds";
            scene = Rect("Scene", sceneBounds, 0, 0, 1, 1);
            var aspect = scene.gameObject.AddComponent<AspectRatioFitter>();
            aspect.aspectMode = AspectRatioFitter.AspectMode.FitInParent; aspect.aspectRatio = 1;
            var image = scene.gameObject.AddComponent<Image>();
            image.sprite = background; image.preserveAspect = true;
            image.color = background == null ? new Color32(23, 30, 32, 255) : Color.white;
            image.raycastTarget = false;
            Panel(information, 0, 0, 1, 1, new Color32(29, 39, 38, 255));
            Panel(information, 0, .985f, 1, .015f, Gold);
            actions = Rect("Actions", information, .08f, .12f, .84f, Portrait ? .70f : .72f);
        }

        public void Text(string speaker, string message, bool animate = false, Image speakingActor = null, bool cinematic = false, float? heightOverride = null)
        {
            if (string.IsNullOrEmpty(message)) return;
            var narration = speaker == "Narrador";
            var height = heightOverride ?? (message.Length > 110 ? .34f : message.Length > 65 ? .28f : .23f);
            var width = speakingActor == null ? .92f : .72f;
            if (speakingActor != null)
            {
                Canvas.ForceUpdateCanvases();
                var fontSize = Portrait ? 14 : 18;
                var settings = new TextGenerationSettings
                {
                    font = bodyFont, fontSize = fontSize, scaleFactor = 1, lineSpacing = 1,
                    color = Paper, textAnchor = TextAnchor.UpperLeft, richText = false,
                    horizontalOverflow = HorizontalWrapMode.Wrap, verticalOverflow = VerticalWrapMode.Overflow,
                    generationExtents = new Vector2(scene.rect.width * .68f, 1000),
                };
                var generator = new TextGenerator();
                var textWidth = generator.GetPreferredWidth(message, settings);
                width = Mathf.Clamp((textWidth + 45) / scene.rect.width, .32f, .72f);
                settings.generationExtents = new Vector2(scene.rect.width * width * .90f, 1000);
                var textHeight = generator.GetPreferredHeight(message, settings);
                height = Mathf.Clamp((textHeight + 42) / scene.rect.height, .16f, .38f);
            }
            var headX = speakingActor == null ? .5f : (speakingActor.rectTransform.anchorMin.x + speakingActor.rectTransform.anchorMax.x) / 2;
            var headY = speakingActor == null ? 1f : speakingActor.rectTransform.anchorMax.y;
            var x = Mathf.Clamp(headX - width / 2, .02f, .98f - width);
            var y = speakingActor == null ? .98f - height : Mathf.Min(.98f - height, headY + .045f);
            if (cinematic)
            {
                scene.GetComponent<Image>().color = Color.black;
                width = .88f; height = .36f; x = .06f; y = .32f;
            }
            var box = Rect(speakingActor == null ? "Narration" : "Dialogue", scene, x, y, width, height);
            if (speakingActor != null && panelSkin != null)
            {
                var backdrop = box.gameObject.AddComponent<Image>(); backdrop.sprite = panelSkin;
                backdrop.type = Image.Type.Sliced; backdrop.pixelsPerUnitMultiplier = 2f;
                backdrop.color = new Color32(48, 52, 64, 250); backdrop.raycastTarget = false;
            }
            else Panel(box, 0, 0, 1, 1, narration ? Color.black : new Color32(20, 19, 27, 242));
            if (speakingActor != null)
            {
                var tail = Rect("DialogueTail", scene, Mathf.Clamp(headX - .012f, x + .025f, x + width - .05f), headY + .012f, .024f, Mathf.Max(.01f, y - headY - .012f));
                var tailImage = tail.gameObject.AddComponent<Image>(); tailImage.color = new Color32(20, 19, 27, 242); tailImage.raycastTarget = false;
            }
            if (!cinematic)
            {
                var heading = Label(box, speaker.ToUpperInvariant(), .06f, .70f, .88f, .27f, Portrait ? 14 : 18, Gold);
                heading.resizeTextForBestFit = true; heading.resizeTextMinSize = Portrait ? 11 : 14; heading.resizeTextMaxSize = Portrait ? 14 : 18;
                if (narration) heading.alignment = TextAnchor.MiddleCenter;
            }
            body = Label(box, message, .06f, .08f, .88f, cinematic ? .84f : .57f,
                cinematic ? (Portrait ? 17 : 22) : (Portrait ? 14 : 18), Paper, bodyFont);
            if (narration) body.alignment = TextAnchor.MiddleCenter;
            body.resizeTextForBestFit = true;
            body.resizeTextMinSize = Portrait ? 11 : 14;
            body.resizeTextMaxSize = cinematic ? (Portrait ? 17 : 22) : (Portrait ? 14 : 18);
            if (animate && message.Length > 0) typing = StartCoroutine(Type(message));
        }

        private IEnumerator Type(string text)
        {
            fullText = text; body.text = "";
            for (var i = 1; i <= text.Length; i++)
            { body.text = text.Substring(0, i); yield return new WaitForSecondsRealtime(.015f); }
            typing = null;
        }

        public bool CompleteText()
        {
            if (typing == null) return false;
            StopCoroutine(typing); typing = null;
            if (body != null) body.text = fullText;
            return true;
        }

        public Button Choice(string caption, Action action, int index = 0, int count = 1, StoryButtonStyle style = StoryButtonStyle.Primary)
        {
            if (Portrait)
            {
                // Two columns preserve a touch target on a phone.
                var columns = count > 1 ? 2 : 1;
                var rows = Mathf.CeilToInt(count / (float)columns);
                var width = 1f / columns;
                var rowHeight = 1f / rows;
                var buttonHeight = Mathf.Min(.38f, rowHeight - .08f);
                var bottom = rows == 1 ? (1 - buttonHeight) / 2 : 1 - index / columns * rowHeight - buttonHeight - .04f;
                return Button(actions, caption, index % columns * width + .04f, bottom,
                    width - .08f, buttonHeight, action, style: style);
            }
            var desktopRowHeight = Mathf.Min(.18f, .94f / count);
            return Button(actions, caption, .06f, 1 - (index + 1) * desktopRowHeight + .02f, .88f, desktopRowHeight - .04f, action, style: style);
        }

        public Button Button(Transform parent, string caption, float x, float y, float w, float h, Action action, Color? accent = null, StoryButtonStyle style = StoryButtonStyle.Primary)
        {
            var rect = Rect(caption, parent, x, y, w, h);
            var image = rect.gameObject.AddComponent<Image>(); image.sprite = buttonSkin; image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 2f;
            image.color = style == StoryButtonStyle.Secondary ? new Color32(80, 85, 94, 255) :
                style == StoryButtonStyle.Inventory ? new Color32(162, 123, 64, 255) : new Color32(101, 111, 61, 255);
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var colors = button.colors; colors.highlightedColor = new Color(.92f, .83f, .65f); button.colors = colors;
            var label = Label(rect, caption, .03f, .03f, .94f, .94f, Portrait ? 14 : 18, accent ?? Paper);
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = Portrait ? 11 : 14;
            label.resizeTextMaxSize = Portrait ? 14 : 18;
            label.alignment = TextAnchor.MiddleCenter;
            button.onClick.AddListener(() => { PlaySound("Confirm"); action(); });
            return button;
        }

        public void PlaySound(string name)
        {
            if (soundClips.TryGetValue(name, out var clip) && clip != null) sounds.PlayOneShot(clip, .35f);
        }

        public void Hotspot(string caption, float x, float y, Action action, bool visited = false, bool reward = false, bool danger = false, string display = null)
        {
            var marks = visited ? "✓ " : "";
            var width = caption == "Buraco no chão" ? .24f : display == null ? .16f : .11f;
            var button = Button(scene, marks + (display ?? caption), x, y, width, display == null ? .07f : .06f, action, Gold,
                visited ? StoryButtonStyle.Secondary : StoryButtonStyle.Primary);
            button.name = caption;
            if (reward || danger)
                button.image.color = danger ? new Color32(155, 55, 48, 255) : new Color32(50, 110, 155, 255);
        }

        public Button RewardChoice(string caption, string description, Action action, int index, int count)
        {
            var button = Choice(caption, action, index, count);
            var title = button.GetComponentInChildren<Text>();
            title.rectTransform.anchorMin = new Vector2(.04f, .55f);
            title.rectTransform.anchorMax = new Vector2(.96f, .95f);
            var detail = Label(button.transform, description, .05f, .08f, .90f, .44f, Portrait ? 11 : 14, Paper, bodyFont);
            detail.alignment = TextAnchor.MiddleCenter; detail.resizeTextForBestFit = true;
            detail.resizeTextMinSize = Portrait ? 9 : 11; detail.resizeTextMaxSize = Portrait ? 11 : 14;
            return button;
        }

        public Image Actor(Sprite sprite, float x, float y, float height)
        {
            if (sprite == null) throw new ArgumentNullException(nameof(sprite));
            sprite.texture.filterMode = FilterMode.Point;
            var width = height * sprite.rect.width / sprite.rect.height;
            var rect = Rect("Actor", scene, x - width / 2, y, width, height);
            var image = rect.gameObject.AddComponent<Image>(); image.sprite = sprite;
            image.preserveAspect = true; image.raycastTarget = false;
            return image;
        }

        public void Annotation(string message, float x, float y) => Label(scene, message, x, y, .9f-x, .1f, Portrait ? 12 : 20, Gold);
        public Text Health(Image actor, string name, int current, int maximum)
        {
            var headX = (actor.rectTransform.anchorMin.x + actor.rectTransform.anchorMax.x) / 2;
            var y = Mathf.Min(.88f, actor.rectTransform.anchorMax.y + .02f);
            var box = Rect("Health_" + name, scene, Mathf.Clamp(headX - .16f, .02f, .66f), y, .32f, .105f);
            var frame = box.gameObject.AddComponent<Image>();
            frame.sprite = panelSkin; frame.type = Image.Type.Sliced; frame.pixelsPerUnitMultiplier = 2f;
            frame.color = new Color32(48, 52, 64, 250); frame.raycastTarget = false;
            var track = Rect("HealthTrack", box, .09f, .16f, .82f, .22f);
            var background = track.gameObject.AddComponent<Image>();
            background.color = new Color32(20, 19, 27, 255); background.raycastTarget = false;
            var fill = Rect("HealthFill", track, 0, 0, 1, 1).gameObject.AddComponent<Image>();
            fill.color = new Color32(122, 174, 95, 255); fill.raycastTarget = false;
            var label = Label(box, "", .04f, .42f, .92f, .46f, Portrait ? 11 : 15, Paper, bodyFont);
            label.alignment = TextAnchor.MiddleCenter;
            label.resizeTextForBestFit = true; label.resizeTextMinSize = Portrait ? 9 : 12; label.resizeTextMaxSize = Portrait ? 11 : 15;
            SetHealth(label, current, maximum);
            return label;
        }

        public void SetHealth(Text label, int current, int maximum)
        {
            label.text = $"HP {current}/{maximum}";
            var fill = (RectTransform)label.transform.parent.Find("HealthTrack/HealthFill");
            fill.anchorMax = new Vector2(maximum > 0 ? Mathf.Clamp01((float)current / maximum) : 0, 1);
        }
        public void SceneTitle(string message, float y = .38f)
        {
            var title = Label(scene, message, .08f, y, .84f, .25f, Portrait ? 26 : 44, Gold);
            title.font = titleFont; title.alignment = TextAnchor.MiddleCenter;
        }

        private Text Label(Transform parent, string message, float x, float y, float w, float h, int size, Color color, Font selectedFont = null)
        {
            var rect = Rect("Text", parent, x, y, w, h);
            var text = rect.gameObject.AddComponent<Text>(); text.font = selectedFont ?? font; text.text = message;
            text.fontSize = size; text.color = color; text.supportRichText = false; text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private void Panel(Transform parent, float x, float y, float w, float h, Color color)
        {
            var rect = Rect("Panel", parent, x, y, w, h);
            var image = rect.gameObject.AddComponent<Image>(); image.color = color; image.raycastTarget = false;
            if (panelSkin != null && x == 0 && y == 0 && w == 1 && h == 1 && (parent.name == "Information" || parent.name == "Journal"))
            {
                image.sprite = panelSkin; image.type = Image.Type.Sliced;
                image.color = new Color32(72, 48, 44, 255);
                for (var i = 0; i < 13; i++)
                {
                    var grain = Rect("WoodGrain", rect, .07f + (i * 17 % 40) / 100f, .08f + i * .065f,
                        .15f + (i * 13 % 29) / 100f, .003f);
                    var stroke = grain.gameObject.AddComponent<Image>();
                    stroke.color = new Color32(199, 149, 103, 45); stroke.raycastTarget = false;
                }
            }
        }

        private static RectTransform Rect(string name, Transform parent, float x, float y, float w, float h)
        {
            var obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false);
            var rect = (RectTransform)obj.transform;
            rect.anchorMin = new Vector2(x, y); rect.anchorMax = new Vector2(x + w, y + h);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }
    }
}
