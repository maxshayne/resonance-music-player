using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Core.MusicPlayer
{
    public class VisualizerGraphic : MaskableGraphic
    {
        private const int CornerSegments = 3;
        private const float BaseDim = 0.45f;

        [Header("Bars")]
        [SerializeField, Range(8, 96)] private int _idleBarCount = 48;
        [SerializeField, Range(0.1f, 1f)] private float _barWidthRatio = 0.6f;
        [SerializeField, Range(0.1f, 1f)] private float _heightRatio = 0.9f;
        [SerializeField] private float _cornerRadius = 6f;
        [SerializeField] private float _minBarHeight = 6f;

        [Header("Color")]
        [SerializeField]
        private Color _lowColor = new Color(0.66f, 0.33f, 0.97f);
        [SerializeField] private Color _highColor = new Color(0.49f, 0.83f, 0.99f);
        [SerializeField] private Color _capColor = new Color(0.77f, 0.71f, 0.99f);

        private IReadOnlyList<float> _bands;

        public void Render(IReadOnlyList<float> bands)
        {
            _bands = bands;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            Rect rect = rectTransform.rect;
            int bars = _bands?.Count ?? _idleBarCount;
            float cell = rect.width / bars;
            float width = cell * _barWidthRatio;

            for (int i = 0; i < bars; i++)
            {
                float level = _bands == null ? 0f : _bands[i];
                float height = Mathf.Max(_minBarHeight, level * rect.height * _heightRatio);
                float center = rect.xMin + (i + 0.5f) * cell;

                Color bar = Color.Lerp(_lowColor, _highColor, i / (bars - 1f)) * color;
                Color bottom = new Color(bar.r * BaseDim, bar.g * BaseDim, bar.b * BaseDim, bar.a);
                Color top = Color.Lerp(bar, _capColor * color, 0.3f + 0.5f * level);

                AddBar(vh, center - width * 0.5f, center + width * 0.5f, rect.yMin, rect.yMin + height, bottom, top);
            }
        }

        private void AddBar(VertexHelper vh, float x0, float x1, float y0, float y1, Color bottom, Color top)
        {
            float height = y1 - y0;
            float radius = Mathf.Min(_cornerRadius, (x1 - x0) * 0.5f, height * 0.5f);

            float insetX0 = x0 + radius;
            float insetX1 = x1 - radius;
            float insetY0 = y0 + radius;
            float insetY1 = y1 - radius;

            int origin = vh.currentVertCount;
            vh.AddVert(new Vector3((x0 + x1) * 0.5f, (y0 + y1) * 0.5f), Color.Lerp(bottom, top, 0.5f), Vector2.zero);

            for (int corner = 0; corner < 4; corner++)
            {
                float pivotX = corner == 0 || corner == 3 ? insetX0 : insetX1;
                float pivotY = corner < 2 ? insetY0 : insetY1;

                for (int segment = 0; segment <= CornerSegments; segment++)
                {
                    float angle = (180f + corner * 90f + 90f * segment / CornerSegments) * Mathf.Deg2Rad;
                    float x = pivotX + Mathf.Cos(angle) * radius;
                    float y = pivotY + Mathf.Sin(angle) * radius;

                    vh.AddVert(new Vector3(x, y), Color.Lerp(bottom, top, (y - y0) / height), Vector2.zero);
                }
            }

            int rim = vh.currentVertCount - origin - 1;
            for (int i = 0; i < rim; i++)
                vh.AddTriangle(origin, origin + 1 + i, origin + 1 + (i + 1) % rim);
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            raycastTarget = false;
        }
#endif
    }
}
