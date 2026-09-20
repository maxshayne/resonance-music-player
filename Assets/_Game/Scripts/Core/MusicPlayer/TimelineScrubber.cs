using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Core.MusicPlayer
{
    public class TimelineScrubber : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public event Action Released;

        public bool Dragging { get; private set; }

        public void OnPointerDown(PointerEventData eventData) => Dragging = true;

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!Dragging)
                return;

            Dragging = false;
            Released?.Invoke();
        }
        
        private void OnDisable() => Dragging = false;
    }
}
