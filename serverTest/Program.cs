using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using System.Security.Policy;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Data.SqlClient;
using DotNetEnv;
using MySqlConnector;
using System.Net.Http;

namespace serverTest
{

    class Program
    {
        public static Dictionary<string, WebSocket> AllConnection = new Dictionary<string, WebSocket>();
        static void Main(string[] args)
        {
            MySqlConnection databaseConnection = StartDatabase();

            string port = "5000";
            string url = "http://localhost:" + port + "/";
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            Console.WriteLine("Waiting for WS connection...");
            Console.WriteLine(url);

            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                if (context.Request.IsWebSocketRequest)
                {
                    UserProcess(context, databaseConnection);
                }
            }
        }
        static async Task UserProcess(HttpListenerContext context, MySqlConnection databaseConnection)
        {
            List<WebSocket> LobbyConnection = new List<WebSocket>();
            var webSocketContext = await context.AcceptWebSocketAsync(subProtocol: null);
            WebSocket serverWebSocket = webSocketContext.WebSocket;
            Console.Write("WS connected\n--------------------\n");
            string username = "";
            int ID = 0;
            try
            {
                while (serverWebSocket.State == WebSocketState.Open)
                {
                    ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
                    //var jsonObject = new { messageJson = "", id = 0 };

                    WebSocketReceiveResult result = await serverWebSocket.ReceiveAsync(buffer, CancellationToken.None);
                    string jsonString = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                    dynamic response = JsonConvert.DeserializeObject(jsonString);
                    Console.WriteLine("RECEIVED FROM CLIENT: " + response.id + "\nACTION: " + response.action);

                    switch (response.action.ToString())
                    {
                        case "createLobby":
                            await CreateLobby(serverWebSocket, databaseConnection, response.username.ToString());
                            ID = 1;
                            break;
                        case "joinLobby":
                            if (!CheckGameCode(response.gameCode.ToString(), serverWebSocket))
                            {
                                continue;
                            }
                            int numberOfPlayersInLobby = NumberOfPlayersInLobby(databaseConnection, response.gameCode.ToString());
                            ID = numberOfPlayersInLobby;
                            JoinLobby(serverWebSocket, response.gameCode.ToString(), databaseConnection, response.username.ToString(), numberOfPlayersInLobby);
                            //Add connections from lobby to lobbyConnection
                            string[] users = UsersInLobby(response.gameCode.ToString(), databaseConnection);
                            foreach (string user in users)
                            {
                                LobbyConnection.Add(AllConnection[user]);
                            }
                            break;
                        case "leaveLobby":
                            Console.WriteLine("WORKING THERE");
                            try
                            {
                                await LeaveLobby(serverWebSocket, response.gameCode.ToString(), response.userID.ToString(), databaseConnection);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("\n" + e);
                                Console.ReadKey();
                            }
                            break;
                        case "getUsername":
                            username = response.username.ToString();
                            AllConnection.Add(response.username.ToString(), serverWebSocket);
                            LobbyConnection.Add(AllConnection[response.username.ToString()]);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                //Násilné odpojení klienta
                Console.WriteLine("Klient " + username + " disconnected :(\n" + e);
                AllConnection.Remove(username);
                //Získání gameCode lobby s username klienta
                Console.WriteLine("před databází - " + username);
                string sCommand = $"select code from lobby where user1 = '{username}' or user2 = '{username}' or user3 = '{username}' or user4 = '{username}'";
                string code = "";
                var command = new MySqlCommand(sCommand, databaseConnection);
                try
                {
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        code = (reader.GetString(0));
                    }
                    reader.Close();
                    command.Dispose();
                sCommand = $"update lobby set user{ID} = null where code = '{code}'";
                command = new MySqlCommand(sCommand, databaseConnection);
                command.ExecuteNonQuery();
                if (NumberOfPlayersInLobby(databaseConnection, code) == -1)
                {
                    sCommand = $"delete from lobby where code = '{code}'";
                    command = new MySqlCommand(sCommand, databaseConnection);
                    command.ExecuteNonQuery();
                }
                Console.WriteLine("po databázi - " + username);
                }
                catch (MySqlException x)
                {
                    Console.WriteLine("--------------------------------------------------------\n" + x + "\n--------------------------------------------------------");
                }
                Console.WriteLine("po databázi2 - " + username);
            }
        }
        static public string GenerateGameCode()
        {
            Random random = new Random();
            string code = Convert.ToString(random.Next(1, 1000000));
            int codeLength = code.Length;
            for (int i = 0; i < 6 - codeLength; i++)
            {
                code = "0" + code;
            }
            return code;
        }
        static public async Task CreateLobby(WebSocket serverWebSocket, MySqlConnection databaseConnection, string username)
        {
            string gameCode;

            string message = "";
            while (true)
            {
                gameCode = GenerateGameCode();
                string sCommand = "select code from lobby where code = " + gameCode;
                var command = new MySqlCommand(sCommand, databaseConnection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    message = (reader.GetString(0));
                }
                reader.Close();
                command.Dispose();
                Console.WriteLine("Přečteno:" + message);
                if (!message.Any())
                {
                    AddLobbyToDatabase(gameCode, databaseConnection, username);
                    await SendAsync(new { action = "lobbyCreated", gameCode = gameCode, success = 1 }, serverWebSocket);
                    break;
                }
            }
        }
        static public bool CheckGameCode(string gameCode, WebSocket serverWebSocket)
        {
            if (gameCode.Length != 6)
            {
                SendAsync(new { success = 0, message = "gameCodeIsNotSixDigit" }, serverWebSocket);
                return false;
            }
            try
            {
                int test = Convert.ToInt32(gameCode);
            }
            catch (Exception e)
            {
                SendAsync(new { success = 0, message = "gameCodeIsNotNumber" }, serverWebSocket);
                return false;
            }
            return true;
        }
        static public async void JoinLobby(WebSocket serverWebSocket, string gameCode, MySqlConnection databaseConnection, string username, int numberOfPlayersInLobby)
        {
            if (numberOfPlayersInLobby == -1)
            {
                await SendAsync(new { success = 0, message = "lobbyDoesNotExists" }, serverWebSocket);
                return;
            }
            else if (numberOfPlayersInLobby == 5)
            {
                await SendAsync(new { success = 0, message = "fullLobby" }, serverWebSocket);
                return;
            }
            string sCommand;
            MySqlCommand command;
            
            sCommand = $"update lobby set user{numberOfPlayersInLobby} = '{username}' where code = '{gameCode}'";
            command = new MySqlCommand(sCommand, databaseConnection);
            command.ExecuteNonQuery();
            



            await SendAsync(new { success = 1, userID = numberOfPlayersInLobby, action = "lobbyJoined" }, serverWebSocket);
        }
        static public async Task LeaveLobby(WebSocket serverWebSocket, string gameCode, string userID, MySqlConnection databaseConnection)
        {
            string sCommand = $"update lobby set user{userID} = null where code = " + gameCode;
            var command = new MySqlCommand(sCommand, databaseConnection);
            command.ExecuteNonQuery();
            if (NumberOfPlayersInLobby(databaseConnection, gameCode) == -1)
            {
                sCommand = $"delete from lobby where code = '{gameCode}'";
                command = new MySqlCommand(sCommand, databaseConnection);
                command.ExecuteNonQuery();
            }
            await SendAsync(new { success = "1", action = "lobbyLeft" }, serverWebSocket);
        }
        static public MySqlConnection StartDatabase()
        {
            // Databáze
            //connection string
            DotNetEnv.Env.Load(@"../../../.env");
            string server = "Server=127.0.0.1;";
            string user = "User ID=spojeni;";
            string password = "Password=" + Environment.GetEnvironmentVariable("mySQLPassword") + ";";
            string database = "Database=alhambra";
            string connectionString = server + user + password + database + ";Pooling=true;";

            //connection to database
            var databaseConnection = new MySqlConnection(connectionString);
            databaseConnection.Open();

            return databaseConnection;
        }
        static public void AddLobbyToDatabase(string gameCode, MySqlConnection databaseConnection, string username)
        {
            string sCommand = $"insert into lobby (code, user1) values ('{gameCode}', '{username}')";
            var command = new MySqlCommand(sCommand, databaseConnection);
            command.ExecuteNonQuery();
        }
        static public async Task SendAsync(dynamic jsonObject, WebSocket serverWebSocket)
        {
            string jsonString = JsonConvert.SerializeObject(jsonObject);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
            await serverWebSocket.SendAsync(new ArraySegment<byte>(jsonBytes), WebSocketMessageType.Text, false, CancellationToken.None);
            Console.WriteLine(jsonString);
        }
        static public int NumberOfPlayersInLobby(MySqlConnection databaseConnection, string code)
        {
            //RETURNS 5 FOR FULL LOBBY, -1 FOR EMPTY LOBBY OR FIRST EMPTY SPACE
            int numberOfPlayersInLobby = 0;
            for (int i = 4; i > 0; i--)
            {
                string message = "";
                string sCommand = $"select user{i} from lobby where code = '{code}'";
                var command = new MySqlCommand(sCommand, databaseConnection);
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    if (reader.IsDBNull(0))
                    {
                        message = "";
                    }
                    else
                    {
                        message = (reader.GetString(0));
                    }
                }
                command.Dispose();
                reader.Close();

                if (message.Any())
                {
                    numberOfPlayersInLobby++;
                }
            }
            if (numberOfPlayersInLobby == 4)
            {
                return 5;
            }
            else if (numberOfPlayersInLobby == 0)
            {
                return -1;
            }

            for (int i = 1; i < 5; i++)
            {
                string message = "";
                string sCommand = $"select user{i} from lobby where code = {code}";
                var command = new MySqlCommand(sCommand, databaseConnection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.IsDBNull(0))
                    {
                        message = "";
                    }
                    else
                    {
                        message = (reader.GetString(0));
                    }
                }
                command.Dispose();
                reader.Close();

                if (!message.Any())
                {
                    return i;
                }
            }
            return 0;
        }
        static public string[] UsersInLobby(string gameCode, MySqlConnection databaseConnection)
        {
            string[] users = new string[4];
            int lastUser = 0;
            for (int i = 1; i < 5; i++)
            {
                string message = "";
                string sCommand = $"select user{i} from lobby where code = {gameCode}";
                var command = new MySqlCommand(sCommand, databaseConnection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.IsDBNull(0))
                    {
                        message = "";
                    }
                    else
                    {
                        message = (reader.GetString(0));
                    }
                }
                command.Dispose();
                reader.Close();

                if (!message.Any())
                {
                    users[lastUser] = message;
                }
            }
            return users;
        }
    }
}
