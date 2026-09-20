using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game.Core.MusicPlayer
{
    public class MusicPlayerView : MonoBehaviour
    {
        [Header("Controls")]
        [SerializeField] private Button _playToggle;
        [SerializeField] private Button _previousTrack;
        [SerializeField] private Button _nextTrack;
        [SerializeField] private Sprite _playIcon;
        [SerializeField] private Sprite _pauseIcon;

        [Header("Track")]
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _artist;
        [SerializeField] private Slider _progress;
        [SerializeField] private TimelineScrubber _scrubber;
        [SerializeField] private TMP_Text _time;
        [SerializeField] private Color _playedColor = new Color(0.46f, 0.58f, 0.87f);

        [Header("Visualizer")]
        [SerializeField] private VisualizerGraphic _visualizer;

        private MusicPlayerService _player;
        private readonly CompositeDisposable _disposables = new();
        private string _playedHex;
        private int _shownSecond;

        [Inject]
        public void Construct(MusicPlayerService player) => _player = player;

        private void Awake() => _playedHex = ColorUtility.ToHtmlStringRGB(_playedColor);

        private void Start()
        {
            _player.Track.Subscribe(ShowTrack).AddTo(_disposables);
            _player.Playing.Subscribe(ShowPlaying).AddTo(_disposables);
            _player.Played.Subscribe(ShowProgress).AddTo(_disposables);
            _player.SpectrumUpdated.Subscribe(_ => _visualizer.Render(_player.Bands)).AddTo(_disposables);
        }

        private void OnEnable()
        {
            _playToggle.onClick.AddListener(Toggle);
            _previousTrack.onClick.AddListener(PlayPrevious);
            _nextTrack.onClick.AddListener(PlayNext);
            _progress.onValueChanged.AddListener(Scrub);
            _scrubber.Released += Seek;
        }

        private void OnDisable()
        {
            _playToggle.onClick.RemoveListener(Toggle);
            _previousTrack.onClick.RemoveListener(PlayPrevious);
            _nextTrack.onClick.RemoveListener(PlayNext);
            _progress.onValueChanged.RemoveListener(Scrub);
            _scrubber.Released -= Seek;
        }

        private void OnDestroy() => _disposables.Dispose();

        private void Toggle() => _player.Toggle();

        private void PlayPrevious() => _player.PlayPrevious();

        private void PlayNext() => _player.PlayNext();

        private void Seek() => _player.Seek(_progress.value * _player.Length);

        private void Scrub(float value)
        {
            if (_scrubber.Dragging)
                ShowTime(value * _player.Length, _player.Length);
        }

        private void ShowTrack(MusicPlaylistSO.Track track)
        {
            _title.SetText(track?.Title);
            _artist.SetText(track?.Artist);
            _shownSecond = -1;
        }

        private void ShowPlaying(bool playing) => _playToggle.image.sprite = playing ? _pauseIcon : _playIcon;

        private void ShowProgress(float played)
        {
            if (_scrubber.Dragging)
                return;

            float length = _player.Length;
            _progress.SetValueWithoutNotify(length > 0f ? played / length : 0f);

            ShowTime(played, length);
        }

        private void ShowTime(float played, float length)
        {
            int second = (int)played;
            if (second == _shownSecond)
                return;

            _shownSecond = second;
            _time.text = $"<color=#{_playedHex}>{Clock(played)}</color> / {Clock(length)}";
        }

        private static string Clock(float seconds) => $"{(int)seconds / 60}:{(int)seconds % 60:00}";

#if UNITY_EDITOR
        [ContextMenu("Log spectrum snapshot")]
        private void LogSpectrumSnapshot() => _player.LogSpectrumSnapshot();
#endif
    }
}