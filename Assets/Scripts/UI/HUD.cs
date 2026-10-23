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
        }
        private void Update()
        {
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
