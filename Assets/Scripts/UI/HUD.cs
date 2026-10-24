using UnityEngine;
using UnityEngine.UI;
namespace Paradox
{
    // Put on the Canvas; the builder assigns its small set of UI elements.
    public sealed class HUD : MonoBehaviour
    {
        [SerializeField] private Text dimensionLabel;
        [SerializeField] private Text prompt;
        [SerializeField] private Text message;
        [SerializeField] private Image promptBacking;
        [SerializeField] private Image flash;
        [SerializeField] private CanvasGroup flashGroup;
        [SerializeField] private GameObject completionPanel;
        [SerializeField] private GameObject playPanel;
        [SerializeField, Range(0.2f, 0.4f)] private float flashDuration = 0.3f;
        private float messageUntil;
        private float flashRemaining;
        private CanvasGroup intro;
        private float introStarted;
        public void Configure(Text dimension, Text interaction, Text feedback, Image overlay, CanvasGroup fade, GameObject complete, GameObject play, Image promptBackground)
        { dimensionLabel = dimension; prompt = interaction; message = feedback; flash = overlay; flashGroup = fade; completionPanel = complete; playPanel = play; promptBacking = promptBackground; HideInstructionBars(); }
        private void Awake() { HideInstructionBars(); }
        private void HideInstructionBars()
        {
            // Keep serialized references intact, but retire both instruction strips.
            if (prompt) prompt.gameObject.SetActive(false);
            if (promptBacking) promptBacking.gameObject.SetActive(false);
            if (!playPanel) return;
            foreach (string name in new[] { "Controls", "Footer Backing" })
            {
                Transform strip = playPanel.transform.Find(name);
                if (strip) strip.gameObject.SetActive(false);
            }
        }
        private void Start()
        {
            if (!dimensionLabel || !prompt || !message || !flash || !flashGroup || !completionPanel || !playPanel) Debug.LogWarning("HUD references are incomplete.", this);
            if (completionPanel) completionPanel.SetActive(false);
            if (flashGroup) flashGroup.alpha = 0;
            BeginLevelIntro();
        }
        private void BeginLevelIntro()
        {
            if (!playPanel) return;
            foreach (string name in new[] { "Header Backing", "Tagline" })
            {
                var item = playPanel.transform.Find(name);
                if (item) item.gameObject.SetActive(false);
            }
            // Keep the original title and font; the other HUD elements remain independent.
            var title = playPanel.transform.Find("Title")?.GetComponent<Text>();
            if (!title) return;
            int index = System.Array.IndexOf(MainMenu.LevelScenes, gameObject.scene.name);
            if (index < 0) return;
            title.text = "Level " + (index + 1) + " — " + MainMenu.LevelNames[index];
            title.fontSize = 26;
            title.resizeTextForBestFit = true;
            title.resizeTextMinSize = 18;
            title.resizeTextMaxSize = 26;
            title.alignment = TextAnchor.MiddleCenter;
            title.color = new Color(.96f, .94f, .88f);
            title.raycastTarget = false;
            var rect = title.rectTransform;
            rect.anchorMin = new Vector2(.1f, 1);
            rect.anchorMax = new Vector2(.9f, 1);
            rect.pivot = new Vector2(.5f, 1);
            rect.anchoredPosition = new Vector2(0, -75);
            rect.sizeDelta = new Vector2(0, 48);
            var shadow = title.GetComponent<Shadow>();
            if (!shadow) shadow = title.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(.04f, .03f, .08f, .85f);
            shadow.effectDistance = new Vector2(1, -1);
            intro = title.GetComponent<CanvasGroup>();
            if (!intro) intro = title.gameObject.AddComponent<CanvasGroup>();
            intro.alpha = 1; intro.blocksRaycasts = false; intro.interactable = false;
            introStarted = Time.unscaledTime;
            // Expand keeps the existing menu/tablet usable on narrow and wide screens.
            var scaler = GetComponent<CanvasScaler>();
            if (scaler) scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        }
        private void Update()
        {
            if (intro && intro.gameObject.activeSelf)
            {
                float fade = Mathf.Clamp01((Time.unscaledTime - introStarted - 3f) / .6f);
                intro.alpha = 1 - Mathf.SmoothStep(0, 1, fade);
                if (fade >= 1) intro.gameObject.SetActive(false);
            }
            if (message && Time.unscaledTime > messageUntil && message.text.Length != 0) message.text = "";
            flashRemaining = Mathf.Max(0, flashRemaining - Time.unscaledDeltaTime);
            if (flashGroup) flashGroup.alpha = 0.45f * flashRemaining / flashDuration;
        }
        public void SetDimension(Dimension dimension) { if (dimensionLabel) dimensionLabel.text = dimension == Dimension.White ? "◦  WHITE DIMENSION" : "◦  BLACK DIMENSION"; }
        public void Prompt(string value)
        { /* Instruction prompts are retired; interaction callers retain their existing behaviour. */ }
        public void Message(string value) { if (message) message.text = value; messageUntil = Time.unscaledTime + 2.5f; }
        public void Flash(Color tint) { if (flash) flash.color = tint; flashRemaining = flashDuration; }
        public void ShowCompletion() { if (LevelMenus.Active) { if(playPanel) playPanel.SetActive(false); LevelMenus.Active.ShowCompletion(); return; } if (completionPanel) completionPanel.SetActive(true); if (playPanel) playPanel.SetActive(false); }
    }
}
