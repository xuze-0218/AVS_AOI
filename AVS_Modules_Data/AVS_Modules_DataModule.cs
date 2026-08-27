using AVS_Modules_Data.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace AVS_Modules_Data
{
    public class AVS_Modules_DataModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<HistoryRecordView>();
        }
    }
}