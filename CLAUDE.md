# Resonance Test 

Unity 6000.3.19f1, Windows standalone (resizable window), Built-in RP, uGUI + TextMeshPro.
DI — Extenject, async — UniTask, reactive — R3, tweens — DOTween Pro.

## Code rules

- Only necessary comments.
- Use access modifiers to methods (except internal)
- No XML documentation (`/// <summary>`).
- Do not create assembly definitions without an explicit request.
- Keep scripts lean: no layers, interfaces, wrappers or parameters "for the future".
  An abstraction appears when there is a second consumer, not in advance.
