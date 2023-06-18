using Avalonia.Controls;
using Klient.Models;
using System.Collections.Generic;
using System;
using Klient.ViewModels;
using Avalonia.Media.Imaging;
using static Klient.Models.Cards;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using System.Net;
using System.IO;
using Avalonia.Media;
using DynamicData;
using Avalonia.Controls.Shapes;

namespace Klient.Views
{
    public partial class GameView : UserControl
    {
        static private double lengthOfAnimation = 2;
        static public List<MyImage> BottomCardsImages = new List<MyImage>();
        static public List<MyImage> LeftCardsImages = new List<MyImage>();
        static public List<MyImage> RightCardsImages = new List<MyImage>();
        static public List<MyImage> TopCardsImages = new List<MyImage>();
        static public List<MyImage> MoneyCards = new List<MyImage>();
        static public List<MyImage> BuildingCards = new List<MyImage>();
        static public Canvas canvas;
        static public MyImage UsedCard = new MyImage(new Vector2(0, 0), false);
        static public UserControl userControl = new UserControl();
        static Avalonia.Controls.Shapes.Rectangle rectangleLeft;
        static Avalonia.Controls.Shapes.Rectangle rectangleRight;
        static Avalonia.Controls.Shapes.Rectangle playingUser;

        public GameView()
        {
            InitializeComponent();
            canvas = this.FindControl<Canvas>("Canvas");
            userControl = this.FindControl<UserControl>("UserControl");

            // setting rectangle on top of non playing users
            rectangleLeft = new Avalonia.Controls.Shapes.Rectangle();
            rectangleRight = new Avalonia.Controls.Shapes.Rectangle();
            playingUser = new Avalonia.Controls.Shapes.Rectangle();
            playingUser.Height = 150;
            playingUser.Width = 40;
            playingUser.Fill = new SolidColorBrush(Colors.Red);
            Canvas.Children.Add(playingUser);
        }
        public static void SetPlayersRoundRectangle(string side)
        {
            while (true)
            {
                if (playingUser == null)
                {
                    Task.Delay(20);
                }
                else
                {
                    break;
                }
            }
            if (side == "bottom")
            {
                Canvas.SetBottom(playingUser, 40);
                Canvas.SetLeft(playingUser, 1535);
            }
            else if (side == "top")
            {
                Canvas.SetBottom(playingUser, 840);
                Canvas.SetLeft(playingUser, 350);
            }
            else if (side == "left")
            {
                Canvas.SetBottom(playingUser, 40);
                Canvas.SetLeft(playingUser, 350);
            }
            else if (side == "right")
            {
                Canvas.SetBottom(playingUser, 840);
                Canvas.SetLeft(playingUser, 1535);
            }
        }
        public static void SetRectangle()
        {
            if (Cards.Users.Count < 4)
            {
                GameView.canvas.Children.Add(rectangleRight);
                Canvas.SetBottom(rectangleRight, 830);
                Canvas.SetLeft(rectangleRight, 1520);
                rectangleRight.Width = 400;
                rectangleRight.Height = 250;
                rectangleRight.Fill = new SolidColorBrush(Colors.White);
            }
            else
            {
                GameView.canvas.Children.Remove(rectangleRight);
            }
            if (Cards.Users.Count < 3)
            {
                GameView.canvas.Children.Add(rectangleLeft);
                Canvas.SetBottom(rectangleLeft, 0);
                Canvas.SetLeft(rectangleLeft, 0);
                rectangleLeft.Width = 400;
                rectangleLeft.Height = 250;
                rectangleLeft.Fill = new SolidColorBrush(Colors.White);
            }
            else
            {
                GameView.canvas.Children.Remove(rectangleLeft);
            }
        }
        public static async void SetTableCard(string cardPath, bool money, int slot)
        {
            if (money)
            {
                int index = 0;
                if (MoneyCards.Count == 4)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (MoneyCards[i] == null)
                        {
                            index = i;
                            break;
                        }
                        if (i == 3)
                        {
                            return;
                        }
                    }
                }

                MyImage image = new MyImage(new Vector2(550, 620), true);
                Debug.WriteLine(image.vector);
                image.Height = 160;
                Debug.WriteLine($@"../../..{cardPath}.png");
                image.Source = new Bitmap($@"../../..{cardPath}");
                canvas.Children.Add(image);
                if (MoneyCards.Count == 4)
                {
                    MoneyCards[index] = image;
                }
                else
                {
                    MoneyCards.Add(image);
                }
                Vector2 to = new Vector2(710 + 135 * slot, 620);
                image.Move(image.vector, to, lengthOfAnimation, image, false);
                image.vector = to;
            }
            else
            {
                int index = 0;
                if (BuildingCards.Count == 4)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (BuildingCards[i] == null || slot == i)
                        {
                            index = i;
                            break;
                        }
                        if (i == 3)
                        {
                            return;
                        }
                    }
                }

                MyImage image = new MyImage(new Vector2(550, 410), true);
                Debug.WriteLine(image.vector);
                image.Height = 160;
                Debug.WriteLine($@"../../..{cardPath}.png");
                image.Source = new Bitmap($@"../../..{cardPath}");
                canvas.Children.Add(image);
                if (BuildingCards.Count == 4)
                {
                    BuildingCards[index] = image;
                }
                else
                {
                    BuildingCards.Add(image);
                }
                Vector2 to = new Vector2(710 + 135 * slot, 410);
                image.Move(image.vector, to, lengthOfAnimation, image, false);
                image.vector = to;
            }
        }
        public static async void SetCardToPlayer(int slot, string side, dynamic response, bool draw)
        {
            if (side == "bottom")
            {
                List<Vector2> coords = CalculatePosition(BottomCardsImages.Count + 1, "bottom");
                int numberOfBottomCards = BottomCardsImages.Count;
                for (int i = 0; i < numberOfBottomCards + 1; i++)
                {
                    if (i == numberOfBottomCards)
                    {
                        BottomCardsImages.Add(MoneyCards[slot]);
                        if (draw)
                        {
                            BottomCardsImages[i].Move(MoneyCards[slot].vector, coords[i], lengthOfAnimation, BottomCardsImages[i], false);
                        }
                        MoneyCards[slot].chosen = false;
                        MoneyCards[slot] = null;
                        BottomCardsImages[BottomCardsImages.Count - 1].UpdateVisualState();
                        BottomCardsImages.Last().vector = coords[i];
                    }
                    else
                    {
                        if (draw)
                        {
                            BottomCardsImages[i].Move(BottomCardsImages[i].vector, coords[i], lengthOfAnimation, BottomCardsImages[i], false);
                        }
                    }
                }
            }
            else if (side == "top")
            {
                List<Vector2> coords = CalculatePosition(TopCardsImages.Count + 1, "top");
                for (int i = 0; i < TopCardsImages.Count + 1; i++)
                {
                    if (i == TopCardsImages.Count)
                    {
                        MyImage image = new MyImage(new Vector2(550, 620), true);
                        Debug.WriteLine(image.vector);
                        image.Height = 160;
                        Debug.WriteLine($@"../../../Assets/money.png");
                        image.Source = new Bitmap($@"../../../Assets/money.png");
                        canvas.Children.Add(image);
                        image.vector = MoneyCards[slot].vector;
                        TopCardsImages.Add(image);
                        canvas.Children.Remove(MoneyCards[slot]);
                        if (draw)
                        {
                            image.Move(image.vector, coords[i], lengthOfAnimation, image, false);
                        }
                        MoneyCards[slot].chosen = false;
                        MoneyCards[slot].UpdateVisualState();
                        MoneyCards[slot] = null;
                        TopCardsImages.Last().vector = coords[i];
                        break;
                    }
                    else
                    {
                        if (draw)
                        {
                            TopCardsImages[i].Move(TopCardsImages[i].vector, coords[i], lengthOfAnimation, TopCardsImages[i], false);
                        }
                    }
                }
            }
            else if (side == "right")
            {
                List<Vector2> coords = CalculatePosition(RightCardsImages.Count + 1, "right");
                for (int i = 0; i < RightCardsImages.Count + 1; i++)
                {
                    if (i == RightCardsImages.Count)
                    {
                        MyImage image = new MyImage(new Vector2(550, 620), true);
                        Debug.WriteLine(image.vector);
                        image.Width = 160;
                        Debug.WriteLine($@"../../../Assets/moneyr.png");
                        image.Source = new Bitmap($@"../../../Assets/moneyr.png");
                        canvas.Children.Add(image);
                        image.vector = MoneyCards[slot].vector;
                        RightCardsImages.Add(image);
                        canvas.Children.Remove(MoneyCards[slot]);
                        if (draw)
                        {
                            image.Move(image.vector, coords[i], lengthOfAnimation, image, false);
                        }
                        MoneyCards[slot].chosen = false;
                        MoneyCards[slot].UpdateVisualState();
                        MoneyCards[slot] = null;
                        RightCardsImages.Last().vector = coords[i];
                        break;
                    }
                    else
                    {
                        if (draw)
                        {
                            RightCardsImages[i].Move(RightCardsImages[i].vector, coords[i], lengthOfAnimation, RightCardsImages[i], false);
                        }
                    }
                }
            }
            else if (side == "left")
            {
                List<Vector2> coords = CalculatePosition(LeftCardsImages.Count + 1, "left");
                for (int i = 0; i < LeftCardsImages.Count + 1; i++)
                {
                    if (i == LeftCardsImages.Count)
                    {
                        MyImage image = new MyImage(new Vector2(550, 620), true);
                        Debug.WriteLine(image.vector);
                        image.Width = 160;
                        Debug.WriteLine($@"../../../Assets/moneyr.png");
                        image.Source = new Bitmap($@"../../../Assets/moneyr.png");
                        canvas.Children.Add(image);
                        image.vector = MoneyCards[slot].vector;
                        LeftCardsImages.Add(image);
                        canvas.Children.Remove(MoneyCards[slot]);
                        if (draw)
                        {
                            image.Move(image.vector, coords[i], lengthOfAnimation, image, false);
                        }
                        MoneyCards[slot].chosen = false;
                        MoneyCards[slot].UpdateVisualState();
                        MoneyCards[slot] = null;
                        LeftCardsImages.Last().vector = coords[i];
                        break;
                    }
                    else
                    {
                        if (draw)
                        {
                            LeftCardsImages[i].Move(LeftCardsImages[i].vector, coords[i], lengthOfAnimation, LeftCardsImages[i], false);
                        }
                    }
                }
            }
        }
        public static async void Refresh(dynamic response)
        {
            // každý uživatel
            for (int i = 0; i < Cards.Users.Count; i++)
            {
                // je rozdíl penìz pøed a po?
                List<Money> moneyResponse = ParseToMoney(response, i);

                if (moneyResponse.Count == Cards.Users[i].Money.Count)
                {
                    continue;
                }
                List<Money> moneyUser = Cards.Users[i].Money;

                // každá penìžní karta
                for (int j = 0; j < response.Game.Users[i].Money.Count; j++)
                {
                    for (int g = 0; g < moneyUser.Count; g++)
                    {
                        if (moneyUser[g] == moneyResponse[j])
                        {
                            moneyUser.RemoveAt(g);
                            moneyResponse.RemoveAt(j);
                            g--;
                        }
                    }
                }
                // uživatel dostal peníze
                if (moneyResponse.Count > 0)
                {
                    for (int j = 0; j < moneyResponse.Count; j++)
                    {
                        // pokud je uživatel bottom
                        if (Cards.Users[i].Position == "bottom")
                        {
                            MyImage image = new MyImage(new Vector2(550, 620), true);
                            Debug.WriteLine(image.vector);
                            image.Height = 160;
                            Debug.WriteLine($@"../../..{moneyResponse[j].path}.png");
                            image.Source = new Bitmap($@"../../..{moneyResponse[j].path}");
                            canvas.Children.Add(image);
                            BottomCardsImages.Add(image);
                            if (j + 1 == moneyResponse.Count)
                            {
                                MoveCards("bottom");
                            }
                        }
                        else
                        {
                            if (Cards.Users[i].Position == "left")
                            {
                                MyImage image = new MyImage(new Vector2(550, 620), false);
                                image.Source = new Bitmap(@"../../../Assets/moneyr.png");
                                image.Width = 160;
                                canvas.Children.Add(image);
                                LeftCardsImages.Add(image);
                                if (j + 1 == moneyResponse.Count)
                                {
                                    MoveCards("left");
                                }
                            }
                            else if (Cards.Users[i].Position == "right")
                            {
                                MyImage image = new MyImage(new Vector2(550, 620), false);
                                image.Source = new Bitmap(@"../../../Assets/moneyr.png");
                                image.Width = 160;
                                canvas.Children.Add(image);
                                RightCardsImages.Add(image);
                                if (j + 1 == moneyResponse.Count)
                                {
                                    MoveCards("right");
                                }
                            }
                            else if (Cards.Users[i].Position == "top")
                            {
                                MyImage image = new MyImage(new Vector2(550, 620), false);
                                image.Source = new Bitmap(@"../../../Assets/money.png");
                                image.Height = 160;
                                canvas.Children.Add(image);
                                TopCardsImages.Add(image);
                                if (j + 1 == moneyResponse.Count)
                                {
                                    MoveCards("top");
                                }
                            }
                        }
                    }
                }
            }
        }
        public static async void MoveCards(string side)
        {
            int countOfIteration = 0;
            switch (side)
            {
                case "bottom":
                    countOfIteration = BottomCardsImages.Count;
                    break;
                case "left":
                    countOfIteration = LeftCardsImages.Count;
                    break;
                case "right":
                    countOfIteration = RightCardsImages.Count;
                    break;
                case "top":
                    countOfIteration = TopCardsImages.Count;
                    break;
                default:
                    break;
            }
            List<Vector2> to = CalculatePosition(countOfIteration, side);
            for (int i = 0; i < countOfIteration; i++)
            {
                if (side == "bottom")
                {
                    BottomCardsImages[i].Move(BottomCardsImages[i].vector, to[i], lengthOfAnimation, BottomCardsImages[i], false);
                    BottomCardsImages[i].vector = to[i];
                }
                else if (side == "left")
                {
                    LeftCardsImages[i].Move(LeftCardsImages[i].vector, to[i], lengthOfAnimation, LeftCardsImages[i], false);
                    LeftCardsImages[i].vector = to[i];
                }
                else if (side == "right")
                {
                    RightCardsImages[i].Move(RightCardsImages[i].vector, to[i], lengthOfAnimation, RightCardsImages[i], false);
                    RightCardsImages[i].vector = to[i];
                }
                else if (side == "top")
                {
                    TopCardsImages[i].Move(TopCardsImages[i].vector, to[i], lengthOfAnimation, TopCardsImages[i], false);
                    TopCardsImages[i].vector = to[i];
                }
            }
        }
        public static List<Vector2> CalculatePosition(int numberOfCards, string player)
        {
            List<Vector2> coordinates = new List<Vector2>();
            int y = 0;
            int x = 0;
            if (player == "bottom")
            {
                y = 80;
            }
            else if (player == "top")
            {
                y = 850;
            }
            else
            {
                if (player == "left")
                {
                    x = 40;
                }
                else if (player == "right")
                {
                    x = 1700;
                }

                float bottom = 325f;
                float top = 845f;
                float midPoint = (bottom + top) / 2f;

                float heightOfCards = numberOfCards * 114 + ((numberOfCards - 1) * 10);

                if (top - bottom > heightOfCards)
                {
                    float startOfCards = midPoint - (heightOfCards / 2);

                    for (int i = 0; i < numberOfCards; i++)
                    {
                        coordinates.Add(new Vector2(x, startOfCards));
                        startOfCards += 114;
                    }
                }
                else
                {
                    float difference = top - bottom - heightOfCards;
                    difference *= -1;

                    float startOfCards = bottom - 15;

                    for (int i = 0; i < numberOfCards; i++)
                    {
                        coordinates.Add(new Vector2(x, startOfCards));
                        startOfCards += (114 - (difference / numberOfCards));
                    }
                }

                return coordinates;
            }

            float start = 420f;
            float end = 1600f;
            float mid = (start + end) / 2f;

            float widthOfCards = numberOfCards * 114 + ((numberOfCards - 1) * 10);

            if (end - start > widthOfCards)
            {
                float startOfCards = mid - (widthOfCards / 2);

                for (int i = 0; i < numberOfCards; i++)
                {
                    coordinates.Add(new Vector2(startOfCards, y));
                    startOfCards += 114;
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
                    startOfCards += (114 - (difference / numberOfCards));
                }
            }

            return coordinates;
        }
        public static List<Money> ParseToMoney(dynamic response, int id)
        {
            List<Money> moneyOut = new List<Money>();
            for (int i = 0; i < response.Game.Users[id].Money.Count; i++)
            {
                var money = new Money(response.Game.Users[id].Money[i].name.ToString(),
                                          response.Game.Users[id].Money[i].path.ToString(),
                                          Convert.ToInt32(response.Game.Users[id].Money[i].value),
                                          response.Game.Users[id].Money[i].color.ToString(),
                                          Convert.ToBoolean(response.Game.Users[id].Money[i].special));
                moneyOut.Add(money);
            }
            return moneyOut;
        }
        public static async void MoneyCardTaken(dynamic response)
        {
            string side = GameViewModel.usersNames.FirstOrDefault(x => x.Value == Convert.ToInt16(response.ID)).Key;

            for (int i = 0; i < response.TakenCards.Count; i++)
            {
                if (response.TakenCards.Count - 1 != i)
                {
                    SetCardToPlayer(Convert.ToInt16(response.TakenCards[i]), side, response, false);
                }
                else
                {
                    SetCardToPlayer(Convert.ToInt16(response.TakenCards[i]), side, response, true);
                }
                Cards.Users[Convert.ToInt16(response.ID)].Money.Add(Cards.MoneyOnTable[Convert.ToInt16(response.TakenCards[i])]);
            }

            // z balíèku se vytahuje nová karta
            for (int i = 0; i < response.TakenCards.Count; i++)
            {
                SetTableCard(response.MoneyOnTable[Convert.ToInt16(response.TakenCards[i])].path.ToString(), true, Convert.ToInt16(response.TakenCards[i]));
                Cards.MoneyOnTable[response.TakenCards[i]] = new Money(response.MoneyOnTable[Convert.ToInt16(response.TakenCards[i])].name.ToString(),
                                                                      response.MoneyOnTable[Convert.ToInt16(response.TakenCards[i])].path.ToString(),
                                                                      Convert.ToInt32(response.MoneyOnTable[Convert.ToInt16(response.TakenCards[i])].value),
                                                                      response.MoneyOnTable[Convert.ToInt16(response.TakenCards[i])].color.ToString(),
                                                                      Convert.ToBoolean(response.MoneyOnTable[Convert.ToInt16(response.TakenCards[i])].special));
            }

            for (int i = 0; i < 4; i++)
            {
                if (Cards.BuildingsOnTable[i].value == -1)
                {
                    SetTableCard(response.Game.BuildingsOnTable[i].path.ToString(), false, i);
                    Cards.BuildingsOnTable[i] = new Buildings(response.BuildingsOnTable[i].name.ToString(),
                                                              response.BuildingsOnTable[i].path.ToString(),
                                                              Convert.ToInt32(response.BuildingsOnTable[i].value),
                                                              Convert.ToInt32(response.BuildingsOnTable[i].rarity));
                }
            }


        }
        public static async Task BuildingTaken(dynamic response)
        {
            string side = GameViewModel.usersNames.FirstOrDefault(x => x.Value == Convert.ToInt16(response.ID)).Key;

            // setting a shift for building card
            Vector2 to = BuildingCards[Convert.ToInt32(response.slot)].vector;
            if (side == "bottom")
            {
                to.Y += -570;
            }
            else if (side == "top")
            {
                to.Y += 670;
            }
            else if (side == "right")
            {
                to.X += 1345 - 135 * Convert.ToInt32(response.slot + 1);
            }
            else if (side == "left")
            {
                to.X += -870 - 135 * Convert.ToInt32(response.slot + 1);
            }
            else
            {
                Debug.WriteLine("WE HAVE GOT A PROBLEM");
            }

            // odhozeni vybrane budovy grafika
            BuildingCards[Convert.ToInt32(response.slot)].Move(BuildingCards[Convert.ToInt32(response.slot)].vector, to, lengthOfAnimation, BuildingCards[Convert.ToInt32(response.slot)], true);
            BuildingCards[Convert.ToInt32(response.slot)].chosen = false;

            Cards.BuildingsOnTable[Convert.ToInt32(response.slot)] = new Buildings(response.Game.BuildingsOnTable[Convert.ToInt32(response.slot)].name.ToString(),
                                                                                  response.Game.BuildingsOnTable[Convert.ToInt32(response.slot)].path.ToString(),
                                                                                  Convert.ToInt16(response.Game.BuildingsOnTable[Convert.ToInt32(response.slot)].value),
                                                                                  Convert.ToInt16(response.Game.BuildingsOnTable[Convert.ToInt32(response.slot)].rarity));

            // nastaveni nove karty do vyberu
            if (Convert.ToBoolean(response.moveFinished))
            {
                for (int i = 0; i < 4; i++)
                {
                    if (Cards.BuildingsOnTable[i].value == -1 || i == Convert.ToInt32(response.slot))
                    {
                        SetTableCard(response.BuildingsOnTable[i].path.ToString(), false, i);
                        Cards.BuildingsOnTable[i] = new Buildings(response.BuildingsOnTable[i].name.ToString(),
                                                                  response.BuildingsOnTable[i].path.ToString(),
                                                                  Convert.ToInt32(response.BuildingsOnTable[i].value),
                                                                  Convert.ToInt32(response.BuildingsOnTable[i].rarity));
                    }
                }
            }

            List<int> toRemove = new List<int>();

            for (int i = 0; i < Convert.ToInt16(response.numberOfMoneyCards); i++)
            {
                for (int j = 0; j < Cards.Users[Convert.ToInt16(response.ID)].Money.Count; j++)
                {
                    if (response.moneyUsed[i].ToString() == Cards.Users[Convert.ToInt16(response.ID)].Money[j].name)
                    {
                        Vector2 from = new Vector2(0, 0);
                        if (side == "bottom")
                        {
                            to = BottomCardsImages[j + i].vector;
                            to.Y += -300;
                            from = BottomCardsImages[j + i].vector;
                            BottomCardsImages[j + i].Move(from, to, lengthOfAnimation, BottomCardsImages[j + i], true);
                            toRemove.Add(j + i);
                            Cards.Users[Convert.ToInt32(response.ID)].Money.RemoveAt(j);
                        }
                        else if (side == "top")
                        {
                            to = TopCardsImages[j + i].vector;
                            to.Y += 300;
                            from = TopCardsImages[j + i].vector;
                            TopCardsImages[j + i].Move(from, to, lengthOfAnimation, TopCardsImages[j + i], true);
                            toRemove.Add(j + i);
                            Cards.Users[Convert.ToInt32(response.ID)].Money.RemoveAt(j);
                        }
                        else if (side == "right")
                        {
                            to = RightCardsImages[j + i].vector;
                            to.X += 300;
                            from = RightCardsImages[j + i].vector;
                            RightCardsImages[j + i].Move(from, to, lengthOfAnimation, RightCardsImages[j + i], true);
                            toRemove.Add(j + i);
                            Cards.Users[Convert.ToInt32(response.ID)].Money.RemoveAt(j);
                        }
                        else if (side == "left")
                        {
                            to = LeftCardsImages[j + i].vector;
                            to.X += -300;
                            from = LeftCardsImages[j + i].vector;
                            LeftCardsImages[j + i].Move(from, to, lengthOfAnimation, LeftCardsImages[j + i], true);
                            toRemove.Add(j + i);
                            Cards.Users[Convert.ToInt32(response.ID)].Money.RemoveAt(j);
                        }
                        else
                        {
                            Debug.WriteLine("WE HAVE GOT A PROBLEM 2---------");
                        }

                        break;
                    }
                }
            }
            for (int i = 0; i < toRemove.Count; i++)
            {
                if (side == "bottom")
                {
                    BottomCardsImages.RemoveAt(toRemove[i] - i);
                }
                else if (side == "top")
                {
                    TopCardsImages.RemoveAt(toRemove[i] - i);
                }
                else if (side == "right")
                {
                    RightCardsImages.RemoveAt(toRemove[i] - i);
                }
                else if (side == "left")
                {
                    LeftCardsImages.RemoveAt(toRemove[i] - i);
                }
            }

            MoveCards(side);

            for (int i = 0; i < 6; i++)
            {
                Cards.Users[Convert.ToInt32(response.ID)].TakenBuildings[i] = Convert.ToInt32(response.playersBuildings[i]);
            }

        }
        public static void Reset()
        {
            lengthOfAnimation = 2;
            BottomCardsImages.Clear();
            LeftCardsImages.Clear();
            RightCardsImages.Clear();
            TopCardsImages.Clear();
            MoneyCards.Clear();
            BuildingCards.Clear();
            canvas = null;
            UsedCard = null;
            userControl = null;
        }
    }
}
