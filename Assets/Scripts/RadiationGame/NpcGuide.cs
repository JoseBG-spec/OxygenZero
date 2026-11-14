namespace MarsOutpost.Radiation
{
    /// <summary>
    /// The calm Safety Officer. The dialogue is data-driven and explains radiation,
    /// monitoring, and shielding without alarmist wording or real dose units.
    /// </summary>
    public sealed class NpcGuide : Interactable
    {
        public RadiationConfig config;
        public DialoguePanel dialogue;

        public override string Prompt =>
            config != null && !string.IsNullOrWhiteSpace(config.npcPrompt)
                ? config.npcPrompt
                : "Talk to the Safety Officer";

        public override bool CanInteract => dialogue != null && !dialogue.IsOpen;

        public override void Interact()
        {
            if (dialogue == null)
            {
                return;
            }

            string speaker = config != null ? config.npcName : "Safety Officer";
            string[] pages = config != null ? config.dialoguePages : null;
            dialogue.Open(speaker, pages);
        }
    }
}
