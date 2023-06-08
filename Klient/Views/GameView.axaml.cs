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

namespace Klient.Views
{
    public partial class GameView : UserControl
    {
        static private double lengthOfAnimation = 3;
        static List<MyImage> BottomCardsImages = new List<MyImage>();
        static List<MyImage> LeftCardsImages = new List<MyImage>();
        static List<MyImage> RightCardsImages = new List<MyImage>();
        static List<MyImage> TopCardsImages = new List<MyImage>();
        static List<string> BottomCards = new List<string>();
        static List<MyImage> MoneyCards = new List<MyImage>();
        static List<MyImage> BuildingCards = new List<MyImage>();
        static private Canvas canvas;
        public GameView()
        {
            InitializeComponent();
            canvas = this.FindControl<Canvas>("Canvas");
        }
        public async void SetTableCards(dynamic response)
        {

        }
        public static async void Refresh(dynamic response)
        {
            // každý uživatel
            for (int i = 0; i < Cards.Users.Count; i++)
            {
                // je rozdíl penìz pøed a po?

                List<Money> moneyResponse = ParseToMoney(response, i);
                // parsnout array na money list -----------------------------------------------------------------------

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
                // když uživatel má ménì penìz
                if (moneyUser.Count > 0)
                {
                    // pro každý zbylý peníz
                    for (int j = 0; j < moneyUser.Count; j++)
                    {
                        // pokud je uživatel bottom
                        if (Cards.Users[i].Username == Global.Username)
                        {
                            int index = BottomCards.IndexOf(moneyUser[j].name);
                            BottomCards.RemoveAt(index);
                            BottomCardsImages.RemoveAt(index);
                            canvas.Children.Remove(BottomCardsImages[index]);
                            if (j + 1 == moneyUser.Count)
                            {
                                MoveCards("bottom");
                            }
                        }
                        else
                        {
                            if (Cards.Users[i].Position == "left")
                            {
                                LeftCardsImages.RemoveAt(0);
                                canvas.Children.Remove(LeftCardsImages[0]);
                                if (j + 1 == moneyUser.Count)
                                {
                                    MoveCards("left");
                                }
                            }
                            else if (Cards.Users[i].Position == "right")
                            {
                                RightCardsImages.RemoveAt(0);
                                canvas.Children.Remove(LeftCardsImages[0]);
                                if (j + 1 == moneyUser.Count)
                                {
                                    MoveCards("right");
                                }
                            }
                            else if (Cards.Users[i].Position == "top")
                            {
                                TopCardsImages.RemoveAt(0);
                                canvas.Children.Remove(LeftCardsImages[0]);
                                if (j + 1 == moneyUser.Count)
                                {
                                    MoveCards("top");
                                }
                            }
                        }
                    }
                }
                // uživatel dostal peníze
                else if (moneyResponse.Count > 0)
                {
                    for (int j = 0; j < moneyResponse.Count; j++)
                    {
                        // pokud je uživatel bottom
                        if (Cards.Users[i].Position == "bottom")
                        {
                            MyImage image = new MyImage(new Vector2(550, 620));
                            Debug.WriteLine(image.vector);
                            image.Height = 160;
                            Debug.WriteLine($@"../../..{moneyResponse[j].path}.png");
                            image.Source = new Bitmap($@"../../..{moneyResponse[j].path}");
                            canvas.Children.Add(image);
                            BottomCards.Add(moneyResponse[j].name);
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
                                MyImage image = new MyImage(new Vector2(550, 620));
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
                                MyImage image = new MyImage(new Vector2(550, 620));
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
                                MyImage image = new MyImage(new Vector2(550, 620));
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
            List<Vector2> to = await CalculatePosition(countOfIteration, side);
            for (int i = 0; i < countOfIteration; i++)
            {
                if (side == "bottom")
                {
                    Move(BottomCardsImages[i].vector, to[i], lengthOfAnimation, side, i);
                }
                else if (side == "left")
                {
                    Move(LeftCardsImages[i].vector, to[i], lengthOfAnimation, side, i);
                }
                else if (side == "right")
                {
                    Move(RightCardsImages[i].vector, to[i], lengthOfAnimation, side, i);
                }
                else if (side == "top")
                {
                    Move(TopCardsImages[i].vector, to[i], lengthOfAnimation, side, i);
                }
            }
        }
        public static async Task Move(Vector2 from, Vector2 to, double seconds, string side, int cardNumber)
        {
            double fps = 60;

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
                    Canvas.SetBottom(BottomCardsImages[cardNumber], YCoords[i]);
                    Canvas.SetLeft(BottomCardsImages[cardNumber], XCoords[i]);
                }
                else if (side == "top")
                {
                    Canvas.SetBottom(TopCardsImages[cardNumber], YCoords[i]);
                    Canvas.SetLeft(TopCardsImages[cardNumber], XCoords[i]);
                }
                else if (side == "left")
                {
                    Canvas.SetBottom(LeftCardsImages[cardNumber], YCoords[i]);
                    Canvas.SetLeft(LeftCardsImages[cardNumber], XCoords[i]);
                }
                else if (side == "right")
                {
                    Canvas.SetBottom(RightCardsImages[cardNumber], YCoords[i]);
                    Canvas.SetLeft(RightCardsImages[cardNumber], XCoords[i]);
                }
                await Task.Delay(delay);
            }
        }
        public static async Task<List<Vector2>> CalculatePosition(int numberOfCards, string player)
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
                y = 900;
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

                float bottom = 300f;
                float top = 780f;
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
            float end = 1500f;
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
    }
}
