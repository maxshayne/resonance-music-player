using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core.MusicPlayer
{
    [Serializable]
    public class SpectrumAnalyzer
    {
        private const int SpectrumSamples = 2048;
        private const float SilenceMagnitude = 1e-7f;
        private const float PeakWeight = 0.68f;
        private const float TiltDb = 28f;

        [SerializeField, Range(8, 96)] private int _bandCount = 48;
        [SerializeField] private float _minFrequency = 32f;
        [SerializeField] private float _maxFrequency = 16000f;
        [SerializeField] private float _floorDb = -58f;
        [SerializeField] private float _ceilingDb = -12f;
        
        [SerializeField] private float _attackSpeed = 46f;
        [SerializeField] private float _releaseSpeed = 8.5f;

        private float[] _spectrum;
        private float[] _levels;
        private int[] _edges;

        public IReadOnlyList<float> Bands => _levels;
        
        public bool Sample(AudioSource source, float delta)
        {
            if (_levels == null || _levels.Length != _bandCount)
                Build();
            
            if (source.isPlaying)
                source.GetSpectrumData(_spectrum, 0, FFTWindow.Blackman);
            else
                Array.Clear(_spectrum, 0, _spectrum.Length);

            float moved = 0f;

            for (int i = 0; i < _bandCount; i++)
            {
                int from = _edges[i];
                int to = _edges[i + 1];

                float peak = 0f;
                float sum = 0f;

                for (int bin = from; bin < to; bin++)
                {
                    float magnitude = _spectrum[bin];
                    if (magnitude > peak)
                        peak = magnitude;
                    sum += magnitude;
                }

                float band = peak * PeakWeight + sum / (to - from) * (1f - PeakWeight);
                float db = 20f * Mathf.Log10(band + SilenceMagnitude) + TiltDb * i / (_bandCount - 1f);
                float target = Mathf.Clamp01(Mathf.InverseLerp(_floorDb, _ceilingDb, db));

                float speed = target > _levels[i] ? _attackSpeed : _releaseSpeed;
                float level = Mathf.Lerp(_levels[i], target, 1f - Mathf.Exp(-delta * speed));

                moved = Mathf.Max(moved, Mathf.Abs(level - _levels[i]));
                _levels[i] = level;
            }

            return moved > 0.0005f;
        }

        private void Build()
        {
            _spectrum = new float[SpectrumSamples];
            _levels = new float[_bandCount];
            _edges = new int[_bandCount + 1];

            float nyquist = AudioSettings.outputSampleRate * 0.5f;
            float low = Mathf.Min(_minFrequency, nyquist);
            float high = Mathf.Min(_maxFrequency, nyquist);

            for (int i = 0; i <= _bandCount; i++)
            {
                float frequency = low * Mathf.Pow(high / low, (float)i / _bandCount);
                int bin = Mathf.RoundToInt(frequency / nyquist * SpectrumSamples);
                _edges[i] = Mathf.Clamp(bin, i == 0 ? 0 : _edges[i - 1] + 1, SpectrumSamples);
            }
        }

#if UNITY_EDITOR
        public void LogSnapshot(AudioSource source)
        {
            if (_spectrum == null)
                return;

            int peakBin = 0;
            for (int bin = 1; bin < _spectrum.Length; bin++)
                if (_spectrum[bin] > _spectrum[peakBin])
                    peakBin = bin;

            int loudest = 0;
            float loudestDb = float.MinValue;
            float mean = 0f;
            float tallest = 0f;

            for (int i = 0; i < _bandCount; i++)
            {
                float peak = 0f;
                for (int bin = _edges[i]; bin < _edges[i + 1]; bin++)
                    peak = Mathf.Max(peak, _spectrum[bin]);

                float db = 20f * Mathf.Log10(peak + SilenceMagnitude);
                if (db > loudestDb)
                {
                    loudestDb = db;
                    loudest = i;
                }

                mean += _levels[i] / _bandCount;
                tallest = Mathf.Max(tallest, _levels[i]);
            }

            float nyquist = AudioSettings.outputSampleRate * 0.5f;
            Debug.Log($"bars: mean {mean:0.00} max {tallest:0.00} | loudest band {loudest} " +
                      $"(~{_minFrequency * Mathf.Pow(_maxFrequency / _minFrequency, (float)loudest / _bandCount):0} Hz) raw {loudestDb:0.0} dB | " +
                      $"peak bin {peakBin} = {peakBin * nyquist / SpectrumSamples:0} Hz");
        }

#endif
    }
}
