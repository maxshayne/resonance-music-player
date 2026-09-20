using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core.MusicPlayer
{
    [CreateAssetMenu(fileName = "MusicPlaylist", menuName = "Game/Music Playlist")]
    public class MusicPlaylistSO : ScriptableObject
    {
        [Serializable]
        public class Track
        {
            public AudioClip Clip;
            public string Title;
            public string Artist;
        }

        [SerializeField] private List<Track> _tracks = new();

        public IReadOnlyList<Track> Tracks => _tracks;
    }
}
