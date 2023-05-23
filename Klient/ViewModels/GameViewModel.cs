using Klient.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using System.IO;
using System.Numerics;
using Avalonia.Controls;
using Avalonia;
using System.Runtime.Intrinsics;
using Avalonia.Animation.Animators;
using Microsoft.CodeAnalysis.Operations;
using DynamicData;
using Avalonia.Controls.Templates;
using System.Threading;

namespace Klient.ViewModels
{
    public class GameViewModel : ViewModelBase
    {
        public Dictionary<string, int> usersNames = new Dictionary<string, int>();
        private Action<string> changeContentAction;
        private ObservableCollection<Avalonia.Media.Imaging.Bitmap> moneyOnTable = new ObservableCollection<Avalonia.Media.Imaging.Bitmap>();
        public ObservableCollection<Avalonia.Media.Imaging.Bitmap> MoneyOnTable
        {
            get => moneyOnTable;
            set => this.RaiseAndSetIfChanged(ref moneyOnTable, value);
        }
        private ObservableCollection<Avalonia.Media.Imaging.Bitmap> buildingsOnTable = new ObservableCollection<Avalonia.Media.Imaging.Bitmap>();
        public ObservableCollection<Avalonia.Media.Imaging.Bitmap> BuildingsOnTable
        {
            get => buildingsOnTable;
            set => this.RaiseAndSetIfChanged(ref buildingsOnTable, value);
        }
        private string bottom;
        public string Bottom
        {
            get => bottom;
            set => this.RaiseAndSetIfChanged(ref bottom, value);
        }
        private string top;
        public string Top
        {
            get => top;
            set => this.RaiseAndSetIfChanged(ref top, value);
        }
        private string left;
        public string Left
        {
            get => left;
            set => this.RaiseAndSetIfChanged(ref left, value);
        }
        private string right;
        public string Right
        {
            get => right;
            set => this.RaiseAndSetIfChanged(ref right, value);
        }

        private ObservableCollection<Image> bottomCards = new ObservableCollection<Image>();
        public ObservableCollection<Image> BottomCards
        {
            get => bottomCards;
            set => this.RaiseAndSetIfChanged(ref bottomCards, value);
        }
        private ObservableCollection<Image> topCards = new ObservableCollection<Image>();
        private ObservableCollection<Image> TopCards
        {
            get => topCards;
            set => this.RaiseAndSetIfChanged(ref topCards, value);
        }


        public GameViewModel(Action<string> changeContentAction)
        {
            Global.Status = "game";
            this.changeContentAction = changeContentAction;
            Global.SendAsync(new { action = "getUsernameAndIDForStartingGame", username = Global.Username, userID = Global.ID, gameCode = Global.GameCode });

            ProcessUser();
        }
        public async void PrepareGame(dynamic response)
        {
            //Adding users to cards class
            for (int i = 0; i < response.usersInGame.Count; i++)
            {
                var user = new Cards.User(i + 1, response.usersInGame[i].Username.ToString());
                Cards.Users.Add(user);
            }

            //Add money cards to all users
            for (int i = 0; i < Cards.Users.Count; i++)
            {
                int sum = 0;
                int numOfCard = 0;
                while (sum < 20)
                {
                    var money = new Money(response.moneyOfPlayers[i].Money[numOfCard].name.ToString(),
                                          response.moneyOfPlayers[i].Money[numOfCard].path.ToString(),
                                          Convert.ToInt32(response.moneyOfPlayers[i].Money[numOfCard].value),
                                          response.moneyOfPlayers[i].Money[numOfCard].color.ToString(),
                                          Convert.ToBoolean(response.moneyOfPlayers[i].Money[numOfCard].special));
                    Cards.Users[i].Money.Add(money);
                    numOfCard++;
                    sum += money.value;
                }
            }

            //Setting users ID to their place on screen
            usersNames.Add("bottom", Global.ID);
            List<string> positions = new List<string>();
            if(response.usersInGame.Count >= 2)
            {
                positions.Add("top");
            }
            if(response.usersInGame.Count >= 3)
            {
                positions.Add("left");
            }
            if(response.usersInGame.Count == 4)
            {
                positions.Add("right");
            }
            for(int i = 0; i < response.usersInGame.Count; i++)
            {
                if (i + 1 == Global.ID)
                {
                    continue;
                }
                usersNames.Add(positions.First(), i + 1);
                positions.RemoveAt(0);
            }

            //Setting usernames in game
            Bottom = (Cards.Users[usersNames["bottom"] - 1].Username == null ? "null" : Cards.Users[usersNames["bottom"] - 1].Username) + " - " + (usersNames["bottom"] == null ? "000" : usersNames["bottom"]);
            Top = (Cards.Users[usersNames["top"] - 1].Username == null ? "null" : Cards.Users[usersNames["top"] - 1].Username) + " - " + (usersNames["top"] == null ? "000" : usersNames["top"]);
            if (usersNames.ContainsKey("left"))
            {
                Left = (Cards.Users[usersNames["left"] - 1].Username == null ? "null" : Cards.Users[usersNames["left"] - 1].Username) + " - " + (usersNames["left"] == null ? "000" : usersNames["left"]);
            }
            if (usersNames.ContainsKey("right"))
            {
                Right = (Cards.Users[usersNames["right"] - 1].Username == null ? "null" : Cards.Users[usersNames["right"] - 1].Username) + " - " + (usersNames["right"] == null ? "000" : usersNames["right"]);
            }

            //Adding position to Cards.Users
            for (int i = 0; i < response.usersInGame.Count; i++)
            {

            }

            //Adding money and building on table to screen
            for (int i = 0; i < 4; i++)
            {
                MoneyOnTable.Add(new Avalonia.Media.Imaging.Bitmap("../../.." + response.moneyCards[i].path.ToString()));

                BuildingsOnTable.Add(new Avalonia.Media.Imaging.Bitmap("../../.." + response.buildingCards[i].path.ToString()));
            }
            
            DrawCardsToUser("bottom", true, 0);
            DrawCardsToUser("top", true, 0);

        }
        public async void DrawCardsToUser(string side, bool trueForMoney, int slot)
        {
            int numOfCards = Cards.Users[usersNames[side] - 1].Money.Count;
            List<Vector2> coordinates = await CalculatePosition(numOfCards, side);

            for (int i = 0; i < numOfCards; i++)
            {
                string path = "";
                if(side == "bottom")
                {
                    path = @$"../../../Assets/{Cards.Users[usersNames[side] - 1].Money[i].name}.png";
                }
                else
                {
                    path = @"../../../Assets/money.png";
                }
                Bitmap bitmap = new Bitmap(path);

                int marginForBottom;

                if (i == 0)
                {
                    marginForBottom = 920;
                }
                else
                {
                    marginForBottom = -160;
                }

                Image image = new Image();
                image.Source = bitmap;
                image.Height = 160;

                int Yshift = 0;
                if (i == 0)
                {
                    if(side == "bottom")
                    {
                        Yshift = marginForBottom - 70;
                    }
                    else if(side == "top")
                    {
                        Yshift = marginForBottom - 850;
                    }
                }
                else
                {
                    Yshift = -160;
                }
                int Xshift = (Convert.ToInt16(coordinates[i].X) - (numOfCards - 1 - i) * 85) - 50;
                image.Margin = Avalonia.Thickness.Parse($"{Xshift},{Yshift},0,0");

                int fromX = 550;
                if(slot != 0)
                {
                    fromX += 160 + slot * 135;
                }
                int fromY = 0;
                if (trueForMoney)
                {
                    fromY = marginForBottom - 410;
                }


                /*
                Thread myThread = new Thread(new ParameterizedThreadStart((obj) =>
                {
                    Move(new Vector2(fromX, fromY), new Vector2(Xshift, Yshift), 10, image);
                }));
                myThread.Start();
                */
                Move(new Vector2(fromX, fromY), new Vector2(Xshift, Yshift), 3, image);

                if (side == "bottom")
                {
                    BottomCards.Add(image);
                }
                else if(side == "top")
                {
                    TopCards.Add(image);
                }
            }
        }
        public async void ProcessUser()
        {
            while (Global.WebSocketConnection.State == WebSocketState.Open && Global.Status == "game")
            {

                dynamic response = await Global.WaitingForMessage();

                if (response.success.ToString() == "0")
                {
                    Debug.WriteLine((object)response.message.ToString());
                    switch (response.message.ToString())
                    {

                    }
                    continue;
                }

                switch (((dynamic)response).action.ToString())
                {
                    case "prepareGame":
                        PrepareGame(response);
                        break;
                }
            }
        }
        public async Task Move(Vector2 from, Vector2 to, double seconds, Image image)
        {
            Debug.WriteLine(DateTime.Now);
            double fps = 60;

            double numberOfFrames = Math.Round((1000 * seconds) / (1000 / fps));

            Vector2 difference = new Vector2(0, 0);
            Vector2 newPosition = from;
            double restOfFrames = 0;

            double shiftX = (to.X - from.X) / numberOfFrames;
            double shiftY = (to.Y - from.Y) / numberOfFrames;

            for (int i = 0; i < numberOfFrames; i++)
            {
                int XCoords = Convert.ToInt16(from.X + i * shiftX);
                int YCoords = Convert.ToInt16(from.Y + i * shiftY);
                image.Margin = Avalonia.Thickness.Parse($"{XCoords},{YCoords},0,0");
                await Task.Delay(Convert.ToInt32(Math.Round((1000 / fps) / 11 * 6)));
            }
        }
        public static async Task<List<Vector2>> CalculatePosition(int numberOfCards, string player)
        {
            List<Vector2> coordinates = new List<Vector2>();
            int y = 0;
            int x = 0;
            if (player == "bottom")
            {
                y = 20;
            }
            else if (player == "top")
            {
                y = 900;
            }

            float start = 420f;
            float end = 1500f;
            float mid = (start + end) / 2f;

            float widthOfCards = numberOfCards * 114 + ((numberOfCards - 1) * 10);

            if (end - start > widthOfCards)
            {
                float startOfCards = mid - (widthOfCards / 2);

                for (int i = 0; i < numberOfCards; i++)
                {
                    coordinates.Add(new Vector2(startOfCards, y));
                    startOfCards += 124;
                }
            }
            else
            {
                float difference = end - start - widthOfCards;
                difference *= -1;

                float startOfCards = 405;

                for (int i = 0; i < numberOfCards; i++)
                {
                    coordinates.Add(new Vector2(startOfCards, y));
                    startOfCards += (124 - difference);
                }
            }

            return coordinates;
        }
    }
}
