using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using System.Diagnostics;
using System.Text.Json;
using Newtonsoft.Json;
using System.Reflection.Metadata;
using Avalonia.Controls;
using Klient.Models;
using System.Reactive;

namespace Klient.ViewModels
{
    public class LobbyViewModel : ViewModelBase
    {
        public ReactiveCommand<Unit, Unit> LeaveLobbyCommand { get; }
        private Action<string> changeContentAction;
        string gameCode = "Generating lobby";
        public string GameCode
        {
            get => gameCode;
            set => this.RaiseAndSetIfChanged(ref gameCode, value);
        }
        public LobbyViewModel(Action<string> changeContentAction)
        {
            this.changeContentAction = changeContentAction;
            Global.Status = "inLobby";
            gameCode = Global.GameCode;
            ProcessUser();
            LeaveLobbyCommand = ReactiveCommand.Create(() =>
            {
                Global.SendAsync(new { action = "leaveLobby", gameCode = Global.GameCode, username = Global.Username, userID = Global.ID });
            });
        }
       
        public async void ProcessUser()
        {
            while (Global.WebSocketConnection.State == WebSocketState.Open && Global.Status == "inLobby")
            {

                dynamic response = await Global.WaitingForMessage();

                switch (((dynamic)response).action.ToString())
                {
                    case "lobbyLeft":
                        Global.Status = "mainMenu";
                        changeContentAction("mainMenu");
                        break;
                }
            }
        }
    }
}
