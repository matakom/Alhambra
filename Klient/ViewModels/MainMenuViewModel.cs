using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using System.Reactive;
using Avalonia.Controls;
using Klient.Models;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Threading;

namespace Klient.ViewModels
{
    public class MainMenuViewModel : ViewModelBase
    {
        public ReactiveCommand<Unit, Unit> CreateLobbyCommand { get; }
        public ReactiveCommand<Unit, Unit> JoinLobbyCommand { get; }
        string? errorText;
        public string ErrorText
        {
            get => errorText;
            set => this.RaiseAndSetIfChanged(ref errorText, value);
        }
        public string? gameCode { get; set; }
        private Action<string> changeContentAction;
        public MainMenuViewModel(Action<string> changeContentAction)
        {
            Global.Status = "mainMenu";
            this.changeContentAction = changeContentAction;
            ProcessUser();
            CreateLobbyCommand = ReactiveCommand.Create(() =>
            {
                Global.SendAsync(new { action = "createLobby", username = Global.Username });
            });
            JoinLobbyCommand = ReactiveCommand.Create(() =>
            {
                Global.SendAsync(new { action = "joinLobby", gameCode = gameCode, username = Global.Username });
            });
        }

        public async void ProcessUser()
        {
            while (Global.WebSocketConnection.State == WebSocketState.Open && Global.Status == "mainMenu")
            {

                dynamic response = await Global.WaitingForMessage();
                
                if(response.success.ToString() == "0")
                {
                    Debug.WriteLine((object)response.message.ToString());
                    switch (response.message.ToString())
                    {
                        case "gameCodeIsNotSixDigit":
                            Debug.WriteLine("The game code does not have 6 digits!");
                            ErrorText = "The game code does not have 6 digits!";
                            break;
                        case "gameCodeIsNotNumber":
                            Debug.WriteLine("The game code must be a number!");
                            ErrorText = "The game code must be a number!";
                            break;
                        case "fullLobby":
                            Debug.WriteLine("The lobby is full!");
                            ErrorText = "The lobby is full!";
                            break;
                        case "lobbyDoesNotExists":
                            Debug.WriteLine("Lobby does not exists!");
                            ErrorText = "Lobby does not exists!";
                            break;
                    }
                    continue;
                }
                
                switch (((dynamic)response).action.ToString())
                {
                    case "lobbyCreated":
                        Global.GameCode = response.gameCode;
                        Global.ID = 1;
                        Lobby.Users[0] = Global.Username;
                        Debug.WriteLine("Lobby joined, server accepted");
                        Global.SendAsync(new { action = "getID", userID = Global.ID });
                        changeContentAction("lobby");
                        break;
                    case "lobbyJoined":
                        Global.GameCode = gameCode;
                        Global.ID = response.userID;
                        Debug.WriteLine("Lobby joined, server accepted");
                        Global.SendAsync(new { action = "getID", userID = Global.ID });
                        changeContentAction("lobby");
                        break;
                }
            }
        }
    }
}
