using System.Windows;
using FinanzasApp.Data;

namespace FinanzasApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DatabaseInitializer.Initialize();
        }
    }
}
