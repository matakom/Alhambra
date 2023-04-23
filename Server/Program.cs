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

namespace Server
{
    class Program
    {
        public static Dictionary<string, WebSocket> AllConnection = new Dictionary<string, WebSocket>();
        public static Queue<dynamic> queue = new Queue<dynamic>();
        public static MySqlConnection databaseConnection;
        public static Dictionary<string, lobby> Lobby = new Dictionary<string, lobby>();
        static void Main(string[] args)
        {
            databaseConnection = StartDatabase();

            Thread DoTasks = new Thread(Program.DoTasks);
            DoTasks.Start();

            ConnectUsers();
        }
        static async void DoTasks()
        {
            while (true)
            {
                if (queue.Count == 0)
                {
                    continue;
                }
                await DoTaskFromQueue();
            }
        }
        static async Task DoTaskFromQueue()
        {
            dynamic response = queue.Dequeue();
            switch (response.action.ToString())
            {
                case "createLobby":
                    await CreateLobby(response);
                    break;
                case "joinLobby":
                    JoinLobby(response);
                    //----------------------------------------------------------------------------------
                    //Add connections from lobby to lobbyConnection
                    /*
                    List<string> users = UsersInLobby(response.gameCode.ToString(), databaseConnection);
                    foreach (string usernameFromList in users)
                    {
                        if (!LobbyConnection.ContainsKey(usernameFromList))
                        {
                            LobbyConnection.Add(usernameFromList, AllConnection[usernameFromList]);
                        }
                    }
                    await SendToAllAsync(new { users = users, numberOfUsers = users.Count, action = "morePlayersInLobby" }, LobbyConnection);
                    //----------------------------------------------------------------------------------
                    */
                    break;
                case "leaveLobby":
                    LeaveLobby(response);
                    break;
                case "crashedClient":
                    Console.WriteLine(response.username.ToString() + " crashed :(");
                    DeleteClient(response);
                    break;
            }
        }
        static async void ConnectUsers()
        {
            string port = "5000";
            string url = @"http://localhost:" + port + "/";
            //string url = "http://192.168.1.111:" + port + "/";
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
                    StartConnection(context);
                }
            }
        }
        static public async void StartConnection(HttpListenerContext context)
        {
            Dictionary<string, WebSocket> LobbyConnection = new Dictionary<string, WebSocket>();
            var webSocketContext = await context.AcceptWebSocketAsync(subProtocol: null);
            WebSocket serverWebSocket = webSocketContext.WebSocket;

            //get username and add user to AllConnection
            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
            WebSocketReceiveResult result = await serverWebSocket.ReceiveAsync(buffer, CancellationToken.None);
            string jsonString = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
            dynamic response = JsonConvert.DeserializeObject(jsonString);
            AllConnection.Add(response.username.ToString(), serverWebSocket);

            //start to get messages
            int ID = 0;
            while (serverWebSocket.State == WebSocketState.Open)
            {
                ID = await GetMessage(serverWebSocket, response, ID);
            }
        }
        static public async Task<int> GetMessage(WebSocket serverWebSocket, dynamic responsePreviusMethod, int ID)
        {
            try
            {
                ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
                WebSocketReceiveResult result = await serverWebSocket.ReceiveAsync(buffer, CancellationToken.None);
                string jsonString = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                dynamic response = JsonConvert.DeserializeObject(jsonString);
                Console.WriteLine("-----------------------------------------------\n" + jsonString + "\n-----------------------------------------------");
                if (response.action == "getID")
                {
                    ID = Convert.ToInt32(response.userID.ToString());
                    return ID;
                }
                queue.Enqueue(response);
            }
            catch (Exception e)
            {
                Console.WriteLine("-----------------------------------------------\n" + e + "\n-----------------------------------------------");
                queue.Enqueue(new { action = "crashedClient", username = responsePreviusMethod.username.ToString(), userID = ID });
            }
            return ID;
        }
        static public string GenerateGameCode()
        {
            while (true)
            {
                Random random = new Random();
                string code = Convert.ToString(random.Next(1, 1000000));
                int codeLength = code.Length;
                for (int i = 0; i < 6 - codeLength; i++)
                {
                    code = "0" + code;
                }
                string message = "";
                string sCommand = $"select code from lobby where code = '{code}'";
                var command = new MySqlCommand(sCommand, databaseConnection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    message = (reader.GetString(0));
                }
                reader.Close();
                command.Dispose();
                if (!message.Any())
                {
                    return code;
                }
            }

        }
        static public async Task CreateLobby(dynamic response)
        {
            CreateLobbyInDB(response);
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
        static public async void CreateLobbyInDB(dynamic response)
        {
            string gameCode = GenerateGameCode();
            AddLobbyToDatabase(gameCode);
            AddUserToLobby(new { gameCode = gameCode, username = response.username.ToString() });

            Lobby.Add(gameCode, new lobby(gameCode, UsersInLobby(gameCode)));

            await SendAsync(new { action = "lobbyCreated", users = Lobby[gameCode].users, gameCode = gameCode, success = 1 }, AllConnection[response.username.ToString()]);
        }
        public static int AddUserToLobby(dynamic response)
        {
            int firstEmptySpaceInLobby = FirstEmptySpaceInLobby(response.gameCode.ToString());
            if (firstEmptySpaceInLobby == -1)
            {
                firstEmptySpaceInLobby = 1;
            }
            string sCommand = $"update lobby set user{firstEmptySpaceInLobby} = '{response.username.ToString()}' where code = '{response.gameCode.ToString()}'";
            MySqlCommand command = new MySqlCommand(sCommand, databaseConnection);
            command.ExecuteNonQuery();
            return firstEmptySpaceInLobby;
        }
        static public string CheckTheLobby(dynamic response)
        {
            if (response.gameCode.ToString().Length != 6)
            {
                return "gameCodeIsNotSixDigit";
            }
            try
            {
                int test = Convert.ToInt32(response.gameCode.ToString());
            }
            catch (Exception e)
            {
                return "gameCodeIsNotNumber";
            }
            int firstEmptySpaceInLobby = FirstEmptySpaceInLobby(response.gameCode.ToString());
            if (firstEmptySpaceInLobby == -1)
            {
                return "lobbyDoesNotExists";
            }
            else if (firstEmptySpaceInLobby == 5)
            {
                return "fullLobby";
            }
            return "lobbyIsOk";
        }
        static public async void JoinLobby(dynamic response)
        {
            string status = CheckTheLobby(response);

            if (status != "lobbyIsOk")
            {
                await SendAsync(new { success = 0, message = status }, AllConnection[response.username.ToString()]);
                return;
            }

            int ID = AddUserToLobby(response);

            Lobby[response.gameCode.ToString()].users.Add(response.username.ToString());

            await SendAsync(new { users = Lobby[response.gameCode.ToString()].users, success = 1, userID = ID, action = "lobbyJoined" }, AllConnection[response.username.ToString()]);

            await SendToAllAsync(new { action = "updatePlayers", users = Lobby[response.gameCode.ToString()].users }, Lobby[response.gameCode.ToString()].users);
        }
        static public async Task LeaveLobby(dynamic response)
        {
            string sCommand = $"update lobby set user{response.userID.ToString()} = null where code = '{response.gameCode.ToString()}'";
            var command = new MySqlCommand(sCommand, databaseConnection);
            command.ExecuteNonQuery();
            if (FirstEmptySpaceInLobby(response.gameCode.ToString()) == -1)
            {
                sCommand = $"delete from lobby where code = '{response.gameCode.ToString()}'";
                command = new MySqlCommand(sCommand, databaseConnection);
                command.ExecuteNonQuery();
            }

            Lobby[response.gameCode.ToString()].users.Remove(response.username.ToString());
            if(Lobby[response.gameCode.ToString()].users.Count == 0)
            {
                Lobby.Remove(response.gameCode.ToString());
            }
            else
            {
                await SendToAllAsync(new { action = "updatePlayers", users = Lobby[response.gameCode.ToString()].users }, Lobby[response.gameCode.ToString()].users);
            }

            await SendAsync(new { success = "1", action = "lobbyLeft" }, AllConnection[response.username.ToString()]);
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
        static public void AddLobbyToDatabase(string gameCode)
        {
            string sCommand = $"insert into lobby (code) values ('{gameCode}')";
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
        static public async Task SendToAllAsync(dynamic jsonObject, List<string> lobbyConnection)
        {
            foreach (string connection in lobbyConnection)
            {
                await SendAsync(jsonObject, AllConnection[connection]);
            }
        }
        static public int FirstEmptySpaceInLobby(string code)
        {
            //RETURNS 5 FOR FULL LOBBY, -1 FOR EMPTY LOBBY OR FIRST EMPTY SPACE
            int firstEmptySpaceInLobby = 0;
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
                    firstEmptySpaceInLobby++;
                }
            }
            if (firstEmptySpaceInLobby == 4)
            {
                return 5;
            }
            else if (firstEmptySpaceInLobby == 0)
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
        static public List<string> UsersInLobby(string gameCode)
        {
            List<string> users = new List<string>();
            for (int i = 1; i < 5; i++)
            {
                string message = "";
                string sCommand = $"select user{i} from lobby where code = '{gameCode}'";
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
                    users.Add(message);
                }
            }
            return users;
        }
        static public async void DeleteClient(dynamic response)
        {
            AllConnection.Remove(response.username.ToString());

            //get gameCode where is client
            string sCommand = $"select code from lobby where user1 = '{response.username.ToString()}' or user2 = '{response.username.ToString()}' or user3 = '{response.username.ToString()}' or user4 = '{response.username.ToString()}'";
            string gameCode = "";
            var command = new MySqlCommand(sCommand, databaseConnection);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                gameCode = (reader.GetString(0));
            }
            reader.Close();
            command.Dispose();
            if (!gameCode.Any())
            {
                return;
            }

            //leave lobby
            sCommand = $"update lobby set user{response.userID.ToString()} = null where code = '{gameCode}'";
            command = new MySqlCommand(sCommand, databaseConnection);
            command.ExecuteNonQuery();
            if (FirstEmptySpaceInLobby(gameCode) == -1)
            {
                sCommand = $"delete from lobby where code = '{gameCode}'";
                command = new MySqlCommand(sCommand, databaseConnection);
                command.ExecuteNonQuery();
            }

            //leave lobby object
            Lobby[gameCode].users.Remove(response.username.ToString());
            if (Lobby[gameCode].users.Count == 0)
            {
                Lobby.Remove(gameCode);
            }
            else
            {
                await SendToAllAsync(new { action = "updatePlayers", users = Lobby[gameCode].users }, Lobby[gameCode].users);
            }
        }
    }
}