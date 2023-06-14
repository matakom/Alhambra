using System;
using System.Collections.Generic;
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
                TakenBuildings = new int[] {0, 0, 0, 0, 0, 0 };
            }
        }
        public void NextPlayer(bool next)
        {
            if (!next)
            {
                return;
            }
            if(PlayingUser == Users.Count - 1)
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
            foreach(User user in Users)
            {
                if(user.Username == userName)
                {
                    Users.Remove(user);
                }
            }
        }
        public Money DrawMoneyCard()
        {
            var card = DeckOfMoney[0];
            DeckOfMoney.RemoveAt(0);
            return card;
        }
        public Buildings DrawBuildingCard()
        {
            var card = DeckOfBuildings[0];
            DeckOfBuildings.RemoveAt(0);
            return card;
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
