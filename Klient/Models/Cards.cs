using Avalonia.Controls;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Klient.Models
{
    public static class Cards
    {
        public static Money[] MoneyOnTable = new Money[4];
        public static Buildings[] BuildingsOnTable = new Buildings[4];
        public static int NumberOfPlayers;
        public static List<User> Users = new List<User>();
        public struct User
        {
            public List<Buildings> Buildings = new List<Buildings>();
            public List<Money> Money = new List<Money>();
            public List<Vector2> MoneyCoordinates = new List<Vector2>();
            public int ID;
            public string Username;
            public int Points;
            public string Position;
            public User(int id, string username)
            {
                ID = id;
                Username = username;
                Position = "";
            }
        }
        public static void AddUserPosition(int id, string side)
        {
            var temp = Users[id];
            temp.Position = side;
            Users[id] = temp;
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
    public class Card
    {
        public double x;
        public double y;
        public double opacity;
        public string path;
        public Card(double x, double y, string path)
        {
            this.x = x;
            this.y = y;
            this.path = path;
        }
    }
}
