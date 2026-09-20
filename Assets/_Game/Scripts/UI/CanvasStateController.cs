using R3;

namespace Game.UI
{
    public class CanvasStateController
    {
        public readonly ReactiveProperty<bool> DialogueVisible = new ReactiveProperty<bool>(true);
        public readonly ReactiveProperty<bool> PlayerVisible = new ReactiveProperty<bool>(true);

        public void ToggleDialogue() => DialogueVisible.Value = !DialogueVisible.Value;

        public void TogglePlayer() => PlayerVisible.Value = !PlayerVisible.Value;
    }
}
