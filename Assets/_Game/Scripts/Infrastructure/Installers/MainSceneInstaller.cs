using Game.Core.MusicPlayer;
using Game.UI;
using UnityEngine;
using Zenject;

namespace Game.Infrastructure.Installers
{
    public class MainSceneInstaller : MonoInstaller
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private MusicPlaylistSO _playlist;
        [SerializeField] private float _rewindThreshold = 3f;
        [SerializeField] private SpectrumAnalyzer _analyzer = new();

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<MusicPlayerService>().AsSingle()
                .WithArguments(_audioSource, _playlist, _analyzer, _rewindThreshold);
            Container.Bind<CanvasStateController>().AsSingle();
        }
    }
}
