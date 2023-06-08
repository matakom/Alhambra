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
using System.Collections.ObjectModel;
using Avalonia.Threading;

namespace Klient.ViewModels
{
    public class LobbyViewModel : ViewModelBase
    {
        public ReactiveCommand<Unit, Unit> LeaveLobbyCommand { get; }
        public ReactiveCommand<Unit, Unit> StartGameCommand { get; }
        private Action<string> changeContentAction;
        string gameCode = "Generating lobby";
        //string[] users = { "", "", "", "" };
        private ObservableCollection<string> users = new ObservableCollection<string>(new[] { "", "", "", "" });
        string? errorText;
        public string ErrorText
        {
            get => errorText;
            set => this.RaiseAndSetIfChanged(ref errorText, value);
        }
        public string GameCode
        {
            get => gameCode;
            set => this.RaiseAndSetIfChanged(ref gameCode, value);
        }
        public ObservableCollection<string> Users
        {
            get => users;
            set => this.RaiseAndSetIfChanged(ref users, value);
        }
        public LobbyViewModel(Action<string> changeContentAction)
        {
            this.changeContentAction = changeContentAction;
            Global.Status = "inLobby";
            gameCode = Global.GameCode;
            Users[(int)(Global.ID)] = Global.Username;
            ProcessUser();
            LeaveLobbyCommand = ReactiveCommand.Create(() =>
            {
                Global.SendAsync(new { action = "leaveLobby", gameCode = Global.GameCode, username = Global.Username, userID = Global.ID });
            });
            StartGameCommand = ReactiveCommand.Create(() =>
            {
                Global.SendAsync(new { action = "startGame", gameCode = Global.GameCode, username = Global.Username, userID = Global.ID });
            });

        }
        public async void UpdatePlayers(dynamic response)
        {
            for (int i = 0; i < response.users.Count; i++)
            {
                Lobby.Users[i] = response.users[i].ToString();
                Users[i] = response.users[i].ToString();
                if (Lobby.Users[i] == Global.Username)
                {
                    Global.ID = i;
                }
                Debug.WriteLine(Users[i] + " - " + i);
            }
            for(int i = 3; i > response.users.Count - 1; i--)
            {
                Lobby.Users[i] = "";
                Users[i] = "";
            }
        }
        public async void ProcessUser()
        {
            while (Global.WebSocketConnection.State == WebSocketState.Open && Global.Status == "inLobby")
            {
                dynamic response = await Global.WaitingForMessage();

                if(response.success.ToString() == "0")
                {
                    switch (((dynamic)response).message.ToString())
                    {
                        case "notALobbyLeader":
                            Debug.WriteLine("You are not a lobby leader!");
                            ErrorText = "You are not a lobby leader!";
                            break;
                        case "onlyOnePlayer":
                            Debug.WriteLine("You need two or more players!");
                            ErrorText = "You need two or more players!";
                            break;
                    }
                    continue;
                }
                switch (((dynamic)response).action.ToString())
                {
                    case "lobbyLeft":
                        Global.Status = "mainMenu";
                        changeContentAction("mainMenu");
                        break;
                    case "updatePlayers":
                        UpdatePlayers(response);
                        break;
                    case "gameStarted":
                        changeContentAction("game");
                        break;
                }
            }
        }
    }
}
