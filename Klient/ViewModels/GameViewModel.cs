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
using Avalonia.Threading;
using System.Collections.Specialized;
using Klient.Views;

namespace Klient.ViewModels
{
    public class GameViewModel : ViewModelBase
    {
        public static Dictionary<string, int> usersNames = new Dictionary<string, int>();
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
        public ObservableCollection<Image> TopCards
        {
            get => topCards;
            set => this.RaiseAndSetIfChanged(ref topCards, value);
        }
        /*
        public ObservableCollection<Avalonia.Thickness> TopCardsMargin
        {
            get => topCards;
            set => this.RaiseAndSetIfChanged(ref topCardsMargin, value);
        }
        */
        public GameViewModel(Action<string> changeContentAction)
        {
            //PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(TopCards)));
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

            //Setting users ID to their place on screen
            usersNames.Add("bottom", Global.ID);
            Cards.AddUserPosition(Global.ID, "bottom");
            List<string> positions = new List<string>();
            if (response.usersInGame.Count == 4)
            {
                positions.Add("right");
            }
            if(response.usersInGame.Count >= 2)
            {
                positions.Add("top");
            }
            if (response.usersInGame.Count >= 3)
            {
                positions.Add("left");
            }


            if(response.usersInGame.Count == 4)
            {
                for(int i = 1; i < 4; i++)
                {
                    if(Global.ID + i > 3)
                    {
                        Cards.AddUserPosition(Global.ID + i - 4, positions.First());
                        usersNames.Add(positions.First(), Global.ID + i - 4);
                    }
                    else
                    {
                        Cards.AddUserPosition(Global.ID + i, positions.First());
                        usersNames.Add(positions.First(), Global.ID + i);
                    }
                    positions.RemoveAt(0);
                }
            }
            else if (response.usersInGame.Count == 3)
            {
                for (int i = 1; i < 3; i++)
                {
                    if (Global.ID + i > 2)
                    {
                        Cards.AddUserPosition(Global.ID + i - 3, positions.First());
                        usersNames.Add(positions.First(), Global.ID + i - 3);
                    }
                    else
                    {
                        Cards.AddUserPosition(Global.ID + i, positions.First());
                        usersNames.Add(positions.First(), Global.ID + i);
                    }
                    positions.RemoveAt(0);
                }
            }
            else if (response.usersInGame.Count == 2)
            {
                for (int i = 1; i < 2; i++)
                {
                    if (Global.ID + i > 1)
                    {
                        Cards.AddUserPosition(Global.ID + i - 2, positions.First());
                        usersNames.Add(positions.First(), Global.ID + i - 2);
                    }
                    else
                    {
                        Cards.AddUserPosition(Global.ID + i, positions.First());
                        usersNames.Add(positions.First(), Global.ID + i);
                    }
                    positions.RemoveAt(0);
                }
            }
            /*
            for (int i = 0; i < response.usersInGame.Count; i++)
            {
                if (i + 1 == Global.ID)
                {
                    continue;
                }
                usersNames.Add(positions.First(), i + 1);
                positions.RemoveAt(0);
            }
            */

            //Setting usernames in game
            Bottom = Cards.Users[usersNames["bottom"]].Username + " - " + usersNames["bottom"];
            Top = Cards.Users[usersNames["top"]].Username + " - " + usersNames["top"];
            if (usersNames.ContainsKey("left"))
            {
                Left = Cards.Users[usersNames["left"]].Username + " - " + usersNames["left"];
            }
            if (usersNames.ContainsKey("right"))
            {
                Right = Cards.Users[usersNames["right"]].Username + " - " + usersNames["right"];
            }

            GameView.Refresh(response);

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
            


            /*
            //Adding position to Cards.Users
            for (int i = 0; i < response.usersInGame.Count; i++)
            {

            }
            */

            //Adding money and building on table to screen
            /*
            for (int i = 0; i < 4; i++)
            {
                MoneyOnTable.Add(new Avalonia.Media.Imaging.Bitmap("../../.." + response.moneyCards[i].path.ToString()));

                BuildingsOnTable.Add(new Avalonia.Media.Imaging.Bitmap("../../.." + response.buildingCards[i].path.ToString()));
            }
            DrawCardsToUser("bottom", true, 0);
            DrawCardsToUser("top", true, 0);
            */

        }
        public async Task DrawCardsToUser(string side, bool trueForMoney, int slot)
        {
            int numOfCards = Cards.Users[usersNames[side] - 1].Money.Count;
            List<Vector2> coordinates = await CalculatePosition(numOfCards, side);

            for (int i = 0; i < numOfCards; i++)
            {
                string path = "";

                path = "";
                
                if (side == "bottom")
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
                //image.Source = 
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

                int index = 0;

                if (side == "bottom")
                {
                    index = BottomCards.Count;
                    BottomCards.Add(image);
                }
                else if(side == "top")
                {
                    index = TopCards.Count;
                    TopCards.Add(image);
                }

                Move(new Vector2(fromX, fromY), new Vector2(Xshift, Yshift), 5, side, i);

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
        public async Task Move(Vector2 from, Vector2 to, double seconds, string side, int cardNumber)
        {
            double fps = 30;

            double numberOfFrames = Math.Round(fps * seconds);

            double shiftX = (to.X - from.X) / numberOfFrames;
            double shiftY = (to.Y - from.Y) / numberOfFrames;

            List<int> XCoords = new List<int>();
            List<int> YCoords = new List<int>();

            for (int j = 0; j < numberOfFrames; j++)
            {
                XCoords.Add(Convert.ToInt16(from.X + j * shiftX));
                YCoords.Add(Convert.ToInt16(from.Y + j * shiftY));
            }

            int delay = Convert.ToInt32(Math.Round((1000 / fps) / 11 * 6));

            for (int i = 0; i < numberOfFrames; i++)
            {
                if(side == "bottom")
                {
                    BottomCards[cardNumber].Margin = Avalonia.Thickness.Parse($"{XCoords[i]},{YCoords[i]},0,0");
                }
                else if(side == "top")
                {
                    TopCards[cardNumber].Margin = Avalonia.Thickness.Parse($"{XCoords[i]},{YCoords[i]},0,0");
                }
                
                //TopCardsMargin[cardNumber] = Avalonia.Thickness.Parse($"{XCoords[i]},{YCoords[i]},0,0");
                await Task.Delay(delay);
            }

            /*
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000 / fps);
            int i = 0;
            timer.Tick += (sender, e) =>
            {
                Debug.WriteLine(i);
                Debug.WriteLine(DateTime.Now.Millisecond);
                TopCardsMargin[cardNumber] = Avalonia.Thickness.Parse($"{XCoords[i]},{YCoords[i]},0,0");
                //image.Margin = Avalonia.Thickness.Parse($"{XCoords[i]},{YCoords[i]},0,0");
                i++;

                if(!(i < numberOfFrames))
                {
                    timer.Stop();
                }
            };
            timer.Start();
            */
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
