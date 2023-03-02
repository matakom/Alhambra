using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using DotNetEnv;
using MySqlConnector;

namespace databaseCommunication
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //connection string
            DotNetEnv.Env.Load(@"../../../.env");
            string server = "Server=127.0.0.1;";
            string user = "User ID=spojeni;";
            string password = "Password=" + Environment.GetEnvironmentVariable("mySQLPassword") + ";";
            string database = "Database=alhambra";
            string connectionString = server + user + password + database;


            //connection to database
            var databaseConnection = new MySqlConnection(connectionString);
            try
            {
                databaseConnection.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
                throw;
            }

            //creating command
            //string sCommand = "insert into user(username, mail, passwordHash) values('ErikZUruguay', 'erik_z@urugay.com', '123erik');";
            //string sCommand = "select username from user where id = 1";

            Console.WriteLine(getText("mail", "where id = 9", databaseConnection));
            

            
            Console.WriteLine("hotovo");
            Console.ReadKey();



        }
        static string getText(string attribute, string where, MySqlConnection databaseConnection)
        {
            string message = "";
            string sCommand = "select " + attribute + " from user " + where + ";";
            var command = new MySqlCommand(sCommand, databaseConnection);
            var reader = command.ExecuteReader();
            while (reader.Read())
                message = (reader.GetString(0));
            return message;
        }

    }
}
