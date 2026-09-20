using Game.Infrastructure.App;
using Zenject;

namespace Game.Infrastructure.Installers
{
    public class ProjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<ApplicationSettings>().AsSingle();
        }
    }
}
