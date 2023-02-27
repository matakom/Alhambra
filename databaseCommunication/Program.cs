using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
            string port = "3306;";
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
            var command = new MySqlCommand("insert into user(username, mail) values('filipek', 'filipek2010@gmail.com');", databaseConnection);
            var reader = command.ExecuteReader();
            /*
            while (reader.Read())
                Console.WriteLine(reader.GetInt32(0));
            */
            Console.WriteLine("hotovo");
            Console.ReadKey();



        }
    }
}
