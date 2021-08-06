using System;
using System.Windows;
using BsfEditor.View;
using BsfEditor.ViewModel;
using ToxicRagers.Helpers;

namespace BsfEditor
{
    public static class Program
    {
        #region Public members
        [STAThread]
        public static void Main()
        {
            Logger.Level = 0;
            var application = new Application();
            application.DispatcherUnhandledException += (sender, args) =>
            {
                Logger.LogToFile(Logger.LogLevel.Error, "Unhandled exception caught on UI thread", args.Exception);
                args.Handled = true;
            };
            var mainViewModel = new MainViewModel();
            var window = new MainWindow();
            window.Closed += (sender, e) => { application.Shutdown(); };
            window.DataContext = mainViewModel;
            application.Run(window);
        }
        #endregion
    }
}
