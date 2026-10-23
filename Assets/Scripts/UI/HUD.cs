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
        { dimensionLabel = dimension; prompt = interaction; message = feedback; flash = overlay; flashGroup = fade; completionPanel = complete; playPanel = play; promptBacking = promptBackground; }
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
        { if (prompt && prompt.text != value) prompt.text = value; if (promptBacking) promptBacking.enabled = value.Length > 0; }
        public void Message(string value) { if (message) message.text = value; messageUntil = Time.unscaledTime + 2.5f; }
        public void Flash(Color tint) { if (flash) flash.color = tint; flashRemaining = flashDuration; }
        public void ShowCompletion() { if (completionPanel) completionPanel.SetActive(true); if (playPanel) playPanel.SetActive(false); }
    }
}
