using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using CeKeymap.App.Infrastructure;

namespace CeKeymap.App
{
    internal static class Program
    {
        private const string MutexName = "Global\\CeKeymapForRemotedesk_SingleInstance";

        [STAThread]
        private static void Main()
        {
            using (var mutex = new Mutex(initiallyOwned: true, MutexName, out var createdNew))
            {
                if (!createdNew)
                {
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var logger = new FileLogger(Path.Combine(baseDirectory, "log.txt"));

                SetUpGlobalExceptionHandling(logger);

                var settingsRepository = new SettingsFileRepository(Path.Combine(baseDirectory, "settings.json"), logger);
                var settings = settingsRepository.Load();
                var autoStartService = new AutoStartService(Application.ExecutablePath);
                var hookService = new KeyboardHookService(() => settings, logger);

                hookService.Start();

                var icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;
                var trayContext = new TrayApplicationContext(settings, settingsRepository, autoStartService, hookService, logger, icon);

                try
                {
                    Application.Run(trayContext);
                }
                finally
                {
                    hookService.Stop();
                }
            }
        }

        private static void SetUpGlobalExceptionHandling(FileLogger logger)
        {
            Application.ThreadException += (s, e) =>
            {
                logger.LogError("Unhandled UI thread exception.", e.Exception);
                MessageBox.Show(
                    "予期しないエラーが発生しました。詳細は log.txt を確認してください。",
                    "CeKeymap",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            };

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                logger.LogError("Unhandled exception.", e.ExceptionObject as Exception);
            };

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        }
    }
}
