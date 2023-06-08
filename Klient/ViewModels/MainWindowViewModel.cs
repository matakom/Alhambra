using Klient.Models;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Klient.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        ViewModelBase content;
        public MainWindowViewModel()
        {
            InitializeAsync();
        }
        private async Task InitializeAsync()
        {
            await RandomUsername();
            await ConnectToServer();
            Content = new MainMenuViewModel(ChangeContent);
        }
        public ViewModelBase Content
        {
            get => content;
            set => this.RaiseAndSetIfChanged(ref content, value);
        }
        private void ChangeContent(string viewModelName)
        {
            try
            {

                if (viewModelName == "lobby")
                {
                    Content = new LobbyViewModel(ChangeContent);
                }
                else if (viewModelName == "mainMenu")
                {
                    Content = new MainMenuViewModel(ChangeContent);
                }
                else if (viewModelName == "game")
                {
                    Content = new GameViewModel(ChangeContent);
                    Debug.WriteLine(Content);
                }
            }
            catch (Exception ex)
            {
                // Získání informací o chybě
                string errorMessage = ex.Message;
                string stackTrace = ex.StackTrace;

                // Zápis chyby do souboru
                string logFilePath = "";
                if(Global.ID != null)
                {
                    logFilePath = $"../../../.logs/errorlog{Global.ID}.txt"; // Cesta k souboru logu
                }
                else
                {
                    logFilePath = "errorlog.txt";
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
        static async Task RandomUsername()
        {
            var httpClient = new HttpClient();
            var apiResponse = await httpClient.GetAsync("https://random-word-api.herokuapp.com/word?lang=en");
            var word = await apiResponse.Content.ReadAsStringAsync();
            var startIndex = word.IndexOf("[\"") + 2;
            var endIndex = word.IndexOf("\"]");
            word = word.Substring(startIndex, endIndex - startIndex);
            Global.Username = word;
        }
        static async Task ConnectToServer()
        {
            Uri serverUri = new Uri("ws://localhost:5000");
            //Uri serverUri = new Uri("ws://192.168.1.111:5000");

            Global.WebSocketConnection = new ClientWebSocket();
            try
            {
                await Global.WebSocketConnection.ConnectAsync(serverUri, CancellationToken.None);
                await Global.SendAsync(new { action = "GetUsername", username = Global.Username });
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception:" + e);
            }
        }
    }
}