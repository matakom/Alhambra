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
using System.Reactive;
using System.Net.Http.Headers;
using System.Collections;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace Klient.ViewModels
{
    public class GameViewModel : ViewModelBase
    {
        private string playingUser;
        public string PlayingUser
        {
            get => playingUser;
            set
            {
                this.RaiseAndSetIfChanged(ref playingUser, value);
                showPlayerOnRound();
            }
        }
        public static Dictionary<string, int> usersNames = new Dictionary<string, int>();
        private Action<string> changeContentAction;
        public ReactiveCommand<Unit, Unit> SendCards { get; }
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
        private string countOfMoney;
        public string CountOfMoney
        {
            get => countOfMoney;
            set => this.RaiseAndSetIfChanged(ref countOfMoney, value);
        }
        private string countOfBuildings;
        public string CountOfBuildings
        {
            get => countOfBuildings;
            set => this.RaiseAndSetIfChanged(ref countOfBuildings, value);
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
        private ObservableCollection<ObservableCollection<string>> numberOfTakenBuildings = new ObservableCollection<ObservableCollection<string>>()
        {
            new ObservableCollection<string> { "", "", "", "", "", "" },
            new ObservableCollection<string> { "", "", "", "", "", "" },
            new ObservableCollection<string> { "", "", "", "", "", "" },
            new ObservableCollection<string> { "", "", "", "", "", "" }
        };
        public ObservableCollection<ObservableCollection<string>> NumberOfTakenBuildings
        {
            get => numberOfTakenBuildings;
            set => this.RaiseAndSetIfChanged(ref numberOfTakenBuildings, value);
        }
        public GameViewModel(Action<string> changeContentAction)
        {
            //PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(TopCards)));
            Global.Status = "game";
            this.changeContentAction = changeContentAction;
            Global.SendAsync(new { action = "getUsernameAndIDForStartingGame", username = Global.Username, userID = Global.ID, gameCode = Global.GameCode });

            SendCards = ReactiveCommand.Create(() =>
            {
                Dictionary<int, string> money = AllChosenMoney(true);
                if (!money.ContainsKey(-1))
                {
                    Global.SendAsync(new { action = "pickCards", gameCode = Global.GameCode, username = Global.Username, userID = Global.ID, cardName = money.Values, slot = money.Keys });
                    return;
                }
                Dictionary<int, string> test = AllChosenMoney(false);
                if (test.ContainsKey(-1))
                {
                    //HANDLE WHEN PLAYER CHOSES ILLEGAL CARDS
                }

            });

            ProcessUser();
        }
        public static Dictionary<int, string> AllChosenMoney(bool forMoney)
        {
            if (forMoney)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (GameView.BuildingCards[i].chosen)
                    {
                        return new Dictionary<int, string>() { { -1, "chosenBuilding" } };
                    }
                }

                Dictionary<int, string> money = new Dictionary<int, string>();
                for (int i = 0; i < GameView.BottomCardsImages.Count; i++)
                {
                    if (GameView.BottomCardsImages[i].chosen)
                    {
                        return new Dictionary<int, string>() { { -1, "chosenPlayersMoney" } };
                    }
                }
                for (int i = 0; i < 4; i++)
                {
                    if (GameView.MoneyCards[i].chosen)
                    {
                        money.Add(i, Cards.MoneyOnTable[i].name);
                    }
                }
                return money;
            }

            Dictionary<int, string> building = new Dictionary<int, string>();
            List<string> moneyToBuy = new List<string>();
            int price = 0;
            for (int i = 0; i < 4; i++)
            {
                if (GameView.BuildingCards[i].chosen)
                {
                    building.Add(i, Cards.BuildingsOnTable[i].name);
                    price = Convert.ToInt16(Cards.BuildingsOnTable[i].name.Substring(1, 2));
                }
            }
            List<int> value = new List<int>();
            for (int i = 0; i < GameView.BottomCardsImages.Count; i++)
            {
                if (GameView.BottomCardsImages[i].chosen)
                {
                    moneyToBuy.Add(Cards.Users[Global.ID].Money[i].name);
                    value.Add(Cards.Users[Global.ID].Money[i].value);
                }
            }
            for (int i = 0; i < 4; i++)
            {
                if (GameView.MoneyCards[i].chosen)
                {
                    return new Dictionary<int, string>() { { -1, "chosenPlayersMoney" } };
                }
            }
            List<string> types = new List<string>();
            for (int i = 0; i < moneyToBuy.Count; i++)
            {
                types.Add(moneyToBuy[i]);
                types[i] = types[i].Substring(0, 2);

                string type = "";

                switch (types[i])
                {
                    case "br":
                        types[i] = "brown";
                        break;
                    case "ye":
                        types[i] = "yellow";
                        break;
                    case "bl":
                        types[i] = "blue";
                        break;
                    case "gr":
                        types[i] = "green";
                        break;
                }
                for (int j = i; j >= 0; j--)
                {
                    if (types[i] != types[j])
                    {
                        return new Dictionary<int, string>() { { 1, "bad" } };
                    }
                }
            }
            if (types.Count < 1)
            {
                return new Dictionary<int, string>() { { -1, "chosenPlayersMoney" } };
            }
            if(value.Count == 0 || price == 0)
            {
                return new Dictionary<int, string>() { { 1, "notOk" } };
            }
            Global.SendAsync(new { action = "pickBuilding", gameCode = Global.GameCode, username = Global.Username, userID = Global.ID, cardName = building.Values, slot = building.Keys, price = price, moneyCardsName = moneyToBuy, moneyCardsType = types, moneyCardsValue = value });
            return new Dictionary<int, string>() { { 1, "ok" } };
        }
        public async void showPlayerOnRound()
        {
            int id = Cards.Users.FirstOrDefault(t => t.Username == PlayingUser).ID - 1;
            string side = usersNames.FirstOrDefault(t => t.Value == id).Key;

            GameView.SetPlayersRoundRectangle(side);
        }
        public async Task PrepareGame(dynamic response)
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
            if (response.usersInGame.Count >= 2)
            {
                positions.Add("top");
            }
            if (response.usersInGame.Count >= 3)
            {
                positions.Add("left");
            }


            if (response.usersInGame.Count == 4)
            {
                for (int i = 1; i < 4; i++)
                {
                    if (Global.ID + i > 3)
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

            //Setting usernames in game
            Bottom = Cards.Users[usersNames["bottom"]].Username + " - " + Cards.Users[usersNames["bottom"]].Points + " (" + usersNames["bottom"] + ")";
            Top = Cards.Users[usersNames["top"]].Username + " - " + Cards.Users[usersNames["top"]].Points + " (" + usersNames["top"] + ")";
            if (usersNames.ContainsKey("left"))
            {
                Left = Cards.Users[usersNames["left"]].Username + " - " + Cards.Users[usersNames["left"]].Points + " (" + usersNames["left"] + ")";
            }
            if (usersNames.ContainsKey("right"))
            {
                Right = Cards.Users[usersNames["right"]].Username + " - " + Cards.Users[usersNames["right"]].Points + " (" + usersNames["right"] + ")";
            }

            int count = 0;
            while (GameView.canvas == null)
            {
                count++;
                await Task.Delay(500);
                if (count == 20)
                {
                    throw new Exception("canvas are set to null :/");
                }
            }

            GameView.Refresh(response);

            // money cards
            for (int i = 0; i < 4; i++)
            {
                Cards.MoneyOnTable[i] = new Money(response.moneyCards[i].name.ToString(),
                                          response.moneyCards[i].path.ToString(),
                                          Convert.ToInt32(response.moneyCards[i].value),
                                          response.moneyCards[i].color.ToString(),
                                          Convert.ToBoolean(response.moneyCards[i].special));
                GameView.SetTableCard(response.moneyCards[i].path.ToString(), true, i);
            }

            // building cards
            for (int i = 0; i < 4; i++)
            {
                Cards.BuildingsOnTable[i] = new Buildings(response.buildingCards[i].name.ToString(),
                                                          response.buildingCards[i].path.ToString(),
                                                          Convert.ToInt16(response.buildingCards[i].value),
                                                          Convert.ToInt16(response.buildingCards[i].rarity));
                GameView.SetTableCard(response.buildingCards[i].path.ToString(), false, i);
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
            RefreshNumberOfTakenBuildings();
            GameView.SetRectangle();
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
                    if (side == "bottom")
                    {
                        Yshift = marginForBottom - 70;
                    }
                    else if (side == "top")
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
                if (slot != 0)
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
                else if (side == "top")
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

                if (response?.Game != null)
                {
                    CountOfMoney = Convert.ToString(response.Game.DeckOfMoney.Count.ToString());
                    CountOfBuildings = Convert.ToString(response.Game.DeckOfBuildings.Count.ToString());
                }

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
                        await PrepareGame(response);
                        PlayingUser = Cards.Users[Convert.ToInt16(response.Game.PlayingUser)].Username;
                        break;
                    case "moneyCardTaken":
                        GameView.MoneyCardTaken(response);
                        PlayingUser = Cards.Users[Convert.ToInt16(response.Game.PlayingUser)].Username;
                        break;
                    case "buildingTaken":
                        await GameView.BuildingTaken(response);
                        PlayingUser = Cards.Users[Convert.ToInt16(response.Game.PlayingUser)].Username;
                        RefreshNumberOfTakenBuildings();
                        break;
                    case "pointsRound":
                        for (int i = 0; i < Cards.Users.Count; i++)
                        {
                            var user = Cards.Users[i];
                            user.Points = Convert.ToInt32(response.points[i]);
                            Cards.Users[i] = user;
                        }
                        Bottom = Cards.Users[usersNames["bottom"]].Username + " - " + Cards.Users[usersNames["bottom"]].Points + " (" + usersNames["bottom"] + ")";
                        Top = Cards.Users[usersNames["top"]].Username + " - " + Cards.Users[usersNames["top"]].Points + " (" + usersNames["top"] + ")";
                        if (usersNames.ContainsKey("left"))
                        {
                            Left = Cards.Users[usersNames["left"]].Username + " - " + Cards.Users[usersNames["left"]].Points + " (" + usersNames["left"] + ")";
                        }
                        if (usersNames.ContainsKey("right"))
                        {
                            Right = Cards.Users[usersNames["right"]].Username + " - " + Cards.Users[usersNames["right"]].Points + " (" + usersNames["right"] + ")";
                        }
                        break;
                    case "gameEnded":
                        for (int i = 0; i < Cards.Users.Count; i++)
                        {
                            var user = Cards.Users[i];
                            user.Points = Convert.ToInt32(response.points[i]);
                            Cards.Users[i] = user;
                        }
                        Reset();
                        changeContentAction("gameEnd");
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
                if (side == "bottom")
                {
                    BottomCards[cardNumber].Margin = Avalonia.Thickness.Parse($"{XCoords[i]},{YCoords[i]},0,0");
                }
                else if (side == "top")
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
            *
            **/
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
        public async void RefreshNumberOfTakenBuildings()
        {
            for (int i = 0; i < Cards.Users.Count; i++)
            {
                string side = usersNames.FirstOrDefault(x => x.Value == (Cards.Users[i].ID - 1)).Key;
                int index = -1;
                switch (side)
                {
                    case "bottom":
                        index = 0;
                        break;
                    case "top":
                        index = 1;
                        break;
                    case "left":
                        index = 2;
                        break;
                    case "right":
                        index = 3;
                        break;
                }
                for (int j = 0; j < 6; j++)
                {
                    NumberOfTakenBuildings[index][j] = Convert.ToString(Cards.Users[i].TakenBuildings[j]);
                }
            }
        }
        public async void Reset()
        {
            PlayingUser = "";

            Bottom = "";
            Top = "";
            Left = "";
            Right = "";

            BottomCards.Clear();
            TopCards.Clear();
            NumberOfTakenBuildings.Clear();

            usersNames.Clear();

            // Reset any other necessary properties or collections
        }

        
    }
}
