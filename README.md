# Resonance Music Player

Test assignment: a music player with an audio-reactive visualizer next to a dialogue box on the UI canvas.

## Running

1. Open the project in Unity 6000.3.19f1.
2. Play from the `Main` scene.

## Controls

- Minimize button on the player hides it; the tab at the right edge brings it back.
- `M` toggles the music player, `D` toggles the dialogue box.

## Stack

- Extenject (DI), UniTask (async), R3 (reactive), DOTween Pro (tweens)
- uGUI + TextMeshPro, Built-in RP
- Windows standalone, resizable window

## Structure

```
Assets/_Game/
├── Art/                   provided art (BG, dialogue stub, music player sprites)
├── Audio/Music/           provided tracks
├── Configs/               MusicPlaylist asset
├── Fonts/                 Anton (titles), NotoSans (paragraphs)
├── Scripts/
│   ├── Core/MusicPlayer/  MusicPlayerService (playback state, R3), MusicPlayerView (controls,
│   │                      track info, timeline), SpectrumAnalyzer, VisualizerGraphic
│   ├── Infrastructure/    app settings, project and scene installers
│   └── UI/                panel layout and visibility state
├── Prefabs/               BottomPanel (dialogue box + music player)
├── Resources/             ProjectContext
└── Scenes/                Main
```
