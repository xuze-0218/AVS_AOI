using AVS_Modules_Settings.ViewModels;
using AVS_Modules_Settings.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace AVS_Modules_Settings
{
    public class SettingsModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ParameterConfigView>();
            containerRegistry.RegisterForNavigation<ProtocolConfigView>();
            containerRegistry.RegisterForNavigation<CameraDebugView>();
            containerRegistry.RegisterForNavigation<PlcDebugView>();
        }
    }
}