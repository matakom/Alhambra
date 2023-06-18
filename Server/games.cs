using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class games
    {
        public List<User> Users = new List<User>();
        public int NumberOfPlayers;
        public List<Money> DeckOfMoney = new List<Money>();
        public List<Buildings> DeckOfBuildings = new List<Buildings>();
        public Money[] MoneyOnTable = new Money[4];
        public Buildings[] BuildingsOnTable = new Buildings[4];
        public int PlayingUser;
        public games()
        {
            PlayingUser = 0;
        }
        public struct User
        {
            public List<Buildings> Buildings;
            public List<Money> Money;
            public int ID;
            public string Username;
            public int Points;
            public string Position;
            public int[] TakenBuildings;
            public User(int id, string username)
            {
                ID = id;
                Username = username;
                Buildings = new List<Buildings>();
                Money = new List<Money>();
                Points = 0;
                Position = "";
                TakenBuildings = new int[] { 0, 0, 0, 0, 0, 0 };
            }
        }
        public void NextPlayer(bool next)
        {
            if (!next)
            {
                return;
            }
            if (PlayingUser == Users.Count - 1)
            {
                PlayingUser = 0;
                return;
            }
            PlayingUser++;
        }
        public void AddUser(int userID, string userName)
        {
            Users.Add(new User(userID, userName));
        }
        public void RemoveUser(string userName)
        {
            foreach (User user in Users)
            {
                if (user.Username == userName)
                {
                    Users.Remove(user);
                }
            }
        }
        public Money DrawMoneyCard()
        {
            Money card = new Money("", "", -1, "", true);
            int phase = -1;
            if (DeckOfMoney.Count == 0)
            {
                phase = 3;
            }
            else
            {
                card = DeckOfMoney[0];
                if (card.special)
                {
                    switch (card.name)
                    {
                        case "a":
                            phase = 1;
                            break;
                        case "b":
                            phase = 2;
                            break;
                    }
                }
            }
            if (card.special)
            {
                card.value = phase;
            }
            DeckOfMoney.RemoveAt(0);
            return card;
        }
        public Buildings DrawBuildingCard()
        {
            int phase = -1;
            if (DeckOfBuildings.Count == 0)
            {
                phase = 3;
            }
            else
            {
                var card = DeckOfBuildings[0];
                DeckOfBuildings.RemoveAt(0);
                return card;
            }
            return new Buildings("", "", -1, 7);
        }
        public List<int> CalculatePoints(int phase)
        {
            List<int> points = new List<int>
            {
                0, 0, 0, 0
            };

            Dictionary<int, List<int>> rarityPoints = new Dictionary<int, List<int>>();

            if (phase == 1)
            {
                rarityPoints = new Dictionary<int, List<int>>
                {
                    { 1, new List<int> { 1 } },
                    { 2, new List<int> { 2 } },
                    { 3, new List<int> { 3 } },
                    { 4, new List<int> { 4 } },
                    { 5, new List<int> { 5 } },
                    { 6, new List<int> { 6 } }
                };
            }
            else if (phase == 2)
            {
                rarityPoints = new Dictionary<int, List<int>>
                {
                    { 1, new List<int> { 1, 8 } },
                    { 2, new List<int> { 2, 9 } },
                    { 3, new List<int> { 3, 10 } },
                    { 4, new List<int> { 4, 11 } },
                    { 5, new List<int> { 5, 12 } },
                    { 6, new List<int> { 6, 13 } }
                };
            }
            else if (phase == 3)
            {
                rarityPoints = new Dictionary<int, List<int>>
                {
                    { 1, new List<int> { 1, 8, 16 } },
                    { 2, new List<int> { 2, 9, 17 } },
                    { 3, new List<int> { 3, 10, 18 } },
                    { 4, new List<int> { 4, 11, 19 } },
                    { 5, new List<int> { 5, 12, 20 } },
                    { 6, new List<int> { 6, 13, 21 } }
                };
            }

            // každá rarita
            for (int i = 0; i < 6; i++)
            {
                Dictionary<int, int> numberOfBuildings = new Dictionary<int, int>();

                // add building to dictionary
                for (int j = 0; j < Users.Count; j++)
                {
                    numberOfBuildings.Add(j, Users[j].TakenBuildings[i]);
                }

                // sort the dictionary
                numberOfBuildings = numberOfBuildings.OrderByDescending(entry => entry.Value).ToDictionary(entry => entry.Key, entry => entry.Value);

                

                for (int j = 0; j < phase; j++)
                {
                    if(numberOfBuildings.Count <= j)
                    {
                        break;
                    }
                    if (numberOfBuildings.Skip(j).First().Value == 0)
                    {
                        break;
                    }
                    if(numberOfBuildings.Count == j + 1)
                    {
                        points[numberOfBuildings.Skip(j).First().Key] += rarityPoints[i + 1][phase - j - 1];
                        Debug.WriteLine("3. Poslední - Hráč " + Convert.ToString(numberOfBuildings.First().Key) + " dostává " + Convert.ToString(rarityPoints[i + 1][phase - j - 1]) + " bodů");
                    }
                    else if (numberOfBuildings.Skip(j).First().Value != numberOfBuildings.Skip(j + 1).First().Value)
                    {
                        points[numberOfBuildings.First().Key] += rarityPoints[i + 1][phase - j - 1];
                        Debug.WriteLine("1. Nerovná se - Hráč " + Convert.ToString(numberOfBuildings.First().Key) + " dostává " + Convert.ToString(rarityPoints[i + 1][phase - j - 1]) + " bodů");
                    }
                    else if (numberOfBuildings.Skip(j).First().Value == numberOfBuildings.Skip(j + 1).First().Value)
                    {
                        List<int> samePoints = (List<int>)numberOfBuildings.Where(pair => pair.Value == numberOfBuildings.Skip(j).First().Value).Select(pair => pair.Key).ToList();
                        int countOfSamePointsPlayers = samePoints.Count;
                        int totalPoints = 0;
                        for (int k = 0; k < countOfSamePointsPlayers; k++)
                        {
                            if (!(k >= phase))
                            {
                                totalPoints += rarityPoints[i + 1][phase - k - 1];
                            }
                            if(k > 0)
                            {
                                j++;
                            }
                        }
                        int pointsForOnePlayer = (int)Math.Floor((double)(totalPoints / countOfSamePointsPlayers));
                        for (int k = 0; k < countOfSamePointsPlayers; k++)
                        {
                            points[numberOfBuildings.Skip(k).First().Key] += pointsForOnePlayer;
                            Debug.WriteLine("2. Rovná se - Hráč " + Convert.ToString(numberOfBuildings.Skip(k).First().Key) + " dostává " + Convert.ToString(pointsForOnePlayer) + " bodů");
                        }
                    }
                    else
                    {
                        Debug.WriteLine("HOW DID WE GET HERE???");
                    }
                }
            }

            for (int i = 0; i < Users.Count; i++)
            {
                var user = Users[i];
                user.Points = points[i];
                Users[i] = user;
            }
            
            return points;
        }

    }
    public class Money
    {
        public string name;
        public string path;
        public int value;
        public string color;
        public bool special;
        public Money(string name, string path, int value, string color, bool special)
        {
            this.name = name;
            this.path = path;
            this.value = value;
            this.color = color;
            this.special = special;
        }
    }
    public class Buildings
    {
        public string name;
        public string path;
        public int value;
        public int rarity;
        public string color;
        public Buildings(string name, string path, int value, int rarity)
        {
            this.name = name;
            this.path = path;
            this.value = value;
            this.rarity = rarity;
        }
    }
}
