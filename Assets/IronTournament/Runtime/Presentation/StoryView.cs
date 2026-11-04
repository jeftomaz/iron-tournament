using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace IronTournament.Presentation
{
    public sealed class StoryView : MonoBehaviour
    {
        private static readonly Color Ink = new Color32(17, 23, 27, 255);
        private static readonly Color Paper = new Color32(233, 222, 197, 255);
        private static readonly Color Gold = new Color32(200, 163, 98, 255);
        private Canvas canvas;
        private CanvasScaler scaler;
        private Font font;
        private Font bodyFont;
        private Font titleFont;
        private RectTransform surface, scene, information, actions;
        private Text body;
        private Coroutine typing;
        private string fullText;
        private Vector2 size;
        public event Action Resized;
        public bool IsTyping => typing != null;
        public bool Portrait => Screen.height > Screen.width;
        public RectTransform SceneArea => scene;

        public void Initialize()
        {
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
            size = new Vector2(Screen.width, Screen.height);
            SetScale();
        }

        private void SetScale() => scaler.referenceResolution = Portrait ? new Vector2(360, 640) : new Vector2(1280, 720);
        private void LateUpdate()
        {
            var current = new Vector2(Screen.width, Screen.height);
            if (current == size || scaler == null) return;
            size = current; SetScale(); Resized?.Invoke();
        }

        public void Draw(Sprite background, string title, string subtitle, string inventory = "")
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
                Label(journal, "CAPÍTULO I\nA VILA", .08f, .80f, .84f, .14f, 24, Gold);
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
            actions = Rect("Actions", information, .05f, .045f, .9f, Portrait ? .24f : .40f);
        }

        public void Text(string speaker, string message, bool animate = false)
        {
            Label(information, speaker.ToUpperInvariant(), .05f, .83f, .90f, .12f, Portrait ? 11 : 14, Gold);
            body = Label(information, message, .05f, Portrait ? .33f : .47f, .90f, Portrait ? .49f : .34f,
                Portrait ? 16 : 20, Paper, bodyFont);
            body.resizeTextForBestFit = true;
            body.resizeTextMinSize = Portrait ? 12 : 14;
            body.resizeTextMaxSize = Portrait ? 16 : 20;
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

        public Button Choice(string caption, Action action, int index = 0, int count = 1)
        {
            if (Portrait)
            {
                if (count > 2)
                {
                    actions.anchorMax = new Vector2(.95f, .53f);
                    if (body != null)
                    {
                        body.rectTransform.anchorMin = new Vector2(.05f, .55f);
                        body.rectTransform.anchorMax = new Vector2(.95f, .82f);
                    }
                }
                // Two columns preserve a touch target on a phone.
                var columns = count > 1 ? 2 : 1;
                var rows = Mathf.CeilToInt(count / (float)columns);
                var width = .98f / columns;
                return Button(actions, caption, index % columns * width, 1 - (index / columns + 1f) / rows,
                    width - .02f, 1f / rows - .02f, action);
            }
            var height = Mathf.Min(.22f, .94f / count);
            return Button(actions, caption, 0, 1 - (index + 1) * height, 1, height - .025f, action);
        }

        public Button Button(Transform parent, string caption, float x, float y, float w, float h, Action action, Color? accent = null)
        {
            var rect = Rect(caption, parent, x, y, w, h);
            var image = rect.gameObject.AddComponent<Image>(); image.color = new Color32(53, 65, 57, 255);
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var colors = button.colors; colors.highlightedColor = new Color(.92f, .83f, .65f); button.colors = colors;
            var label = Label(rect, caption, .03f, .03f, .94f, .94f, Portrait ? 14 : 18, accent ?? Paper);
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = Portrait ? 11 : 14;
            label.resizeTextMaxSize = Portrait ? 14 : 18;
            label.alignment = TextAnchor.MiddleCenter;
            button.onClick.AddListener(() => action());
            return button;
        }

        public void Hotspot(string caption, float x, float y, Action action, bool visited = false, bool reward = false, bool danger = false)
        {
            var marks = visited ? "✓ " : "";
            var button = Button(scene, marks + caption, x, y, .23f, .12f, action,
                Gold);
            button.name = caption;
            if (reward) Label(button.transform, "+", .03f, .65f, .17f, .32f, 15, new Color32(131,208,244,255));
            if (danger) Label(button.transform, "!", .80f, .65f, .17f, .32f, 15, new Color32(244,148,124,255));
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
        public void SceneTitle(string message)
        {
            var title = Label(scene, message, .08f, .38f, .84f, .25f, Portrait ? 26 : 44, Gold);
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
