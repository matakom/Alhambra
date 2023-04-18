using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Klient.Models
{
    public class Cards
    {
        public static string[] Buildings = new string[4];
        public static string[] Money = new string[4];
        public static Users AllUsers = new Users();
        public struct Users
        {
            public user Me;
            public user Top;
            public user Left;
            public user Right;

            public Users()
            {
                Me = new user();
                Top = new user();
                Left = new user();
                Right = new user();
            }
        }
        public struct user
        {
            public List<string> Cards;
            public int ID;
            public string Username;

            public user(List<string> cards, int id, string username)
            {
                Cards = cards;
                ID = id;
                Username = username;
            }
        }
    }
}
