using System;
using System.Collections.Generic;
using R3;
using UnityEngine;
using Zenject;

namespace Game.Core.MusicPlayer
{
    public class MusicPlayerService : ITickable, IDisposable
    {
        private const float SeekTailGuard = 0.05f;

        private readonly AudioSource _source;
        private readonly MusicPlaylistSO _playlist;
        private readonly SpectrumAnalyzer _analyzer;
        private readonly float _rewindThreshold;

        private readonly ReactiveProperty<MusicPlaylistSO.Track> _track = new();
        private readonly ReactiveProperty<bool> _playing = new();
        private readonly ReactiveProperty<float> _played = new();
        private readonly Subject<Unit> _spectrumUpdated = new();

        private int _index;

        public MusicPlayerService(AudioSource source, MusicPlaylistSO playlist, SpectrumAnalyzer analyzer,
            float rewindThreshold)
        {
            _source = source;
            _playlist = playlist;
            _analyzer = analyzer;
            _rewindThreshold = rewindThreshold;

            Load(0);
        }

        public ReadOnlyReactiveProperty<MusicPlaylistSO.Track> Track => _track;
        public ReadOnlyReactiveProperty<bool> Playing => _playing;
        public ReadOnlyReactiveProperty<float> Played => _played;
        public Observable<Unit> SpectrumUpdated => _spectrumUpdated;
        public IReadOnlyList<float> Bands => _analyzer.Bands;
        public float Length => _source.clip != null ? _source.clip.length : 0f;

        public void Tick()
        {
            if (_playing.Value && !_source.isPlaying)
                Step(1);

            float length = Length;
            _played.Value = length > 0f ? Mathf.Min(_source.time, length) : 0f;

            if (_analyzer.Sample(_source, Time.unscaledDeltaTime))
                _spectrumUpdated.OnNext(Unit.Default);
        }

        public void Toggle()
        {
            if (_playing.Value)
                Pause();
            else
                Resume();
        }

        public void PlayNext() => Step(1);

        public void PlayPrevious()
        {
            if (_played.Value > _rewindThreshold)
                Seek(0f);
            else
                Step(-1);
        }

        public void Seek(float seconds)
        {
            float length = Length;
            if (length <= 0f)
                return;

            _source.time = Mathf.Clamp(seconds, 0f, length - SeekTailGuard);
            _played.Value = _source.time;
        }

        public void Dispose()
        {
            _track.Dispose();
            _playing.Dispose();
            _played.Dispose();
            _spectrumUpdated.Dispose();
        }

        private void Step(int direction)
        {
            int count = _playlist.Tracks.Count;
            if (count == 0)
                return;

            Load((_index + direction + count) % count);

            if (_playing.Value)
                _source.Play();
        }

        private void Load(int index)
        {
            _index = index;

            MusicPlaylistSO.Track entry = _playlist.Tracks.Count > 0 ? _playlist.Tracks[index] : null;

            _source.clip = entry?.Clip;
            _track.Value = entry;
            _played.Value = 0f;
        }

        private void Resume()
        {
            if (_source.clip == null)
                return;

            _source.UnPause();

            if (!_source.isPlaying)
                _source.Play();

            _playing.Value = true;
        }

        private void Pause()
        {
            _source.Pause();
            _playing.Value = false;
        }

#if UNITY_EDITOR
        public void LogSpectrumSnapshot() => _analyzer.LogSnapshot(_source);
#endif
    }
}
