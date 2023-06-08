using Avalonia;
using Avalonia.ReactiveUI;
using Klient.Models;
using System;
using System.Diagnostics;
using System.IO;

namespace Klient
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]

        public static void Main(string[] args)
        {
            try
            {
                // prepare and run your App here
                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);
            }
            catch (Exception ex)
            {
                // Získání informací o chybě
                string errorMessage = ex.Message;
                string stackTrace = ex.StackTrace;

                // Zápis chyby do souboru
                string logFilePath = "";
                if (Global.ID != null)
                {
                    logFilePath = $"../../../.logs/errorlog{Global.ID}.txt"; // Cesta k souboru logu
                }
                else
                {
                    logFilePath = "errorlog.txt";
                }
                Debug.WriteLine(logFilePath);

                if (File.Exists(logFilePath))
                {
                    File.Delete(logFilePath);
                }

                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    if (Global.ID != null)
                    {
                        writer.WriteLine("ID: " + Global.ID);
                    }
                    writer.WriteLine("Datum a čas: " + DateTime.Now);
                    writer.WriteLine("Chybová zpráva: " + errorMessage);
                    writer.WriteLine("Stack trace: " + stackTrace);
                    writer.WriteLine("--------");
                }
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
    }
}