using UnityEngine;
using Zenject;

namespace Game.Infrastructure.App
{
    public class ApplicationSettings : IInitializable
    {
        public void Initialize()
        {
            QualitySettings.vSyncCount = 1;
        }
    }
}
