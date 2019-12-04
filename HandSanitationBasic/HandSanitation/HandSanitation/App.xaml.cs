using HandSanitation.Views;
using Prism.Ioc;
using Prism.Modularity;
using System.Windows;
using HandSanitation.ArduinoAccess;
using HandSanitation.ArduinoAccess.Configurations;
using HandSanitation.ArduinoAccess.Model;
using HandSanitation.Core;
using HandSanitation.Signals;
using Prism.Logging;

namespace HandSanitation
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            Container.Resolve<ILogService>().SetupLog();
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IDataAccess, DataAccess>();
            containerRegistry.RegisterSingleton<IEmployeeManager, EmployeeManager>();
            containerRegistry.RegisterSingleton<ILedControl, LedControl>();
            containerRegistry.RegisterSingleton<ILogService, Logger>();
            containerRegistry.RegisterSingleton<IParamConfig, ParamConfig>();
        }
    }
}
