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

namespace serverTest
{

    class Program
    {

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
            var webSocketContext = await context.AcceptWebSocketAsync(subProtocol: null);
            WebSocket serverWebSocket = webSocketContext.WebSocket;
            Console.Write("WS connected\n--------------------\n");
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
                        CreateLobby(serverWebSocket, databaseConnection);
                        break;
                    case "joinLobby":
                        JoinLobby(serverWebSocket, response.gameCode.ToString(), databaseConnection);
                        break;
                    case "leaveLobby":
                        LeaveLobby(serverWebSocket);
                        break;
                    default:
                        Console.WriteLine("WRONG");
                        SendAsync(new { action = "wrongJSON" }, serverWebSocket);
                        break;
                }

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

        static public async void CreateLobby(WebSocket serverWebSocket, MySqlConnection databaseConnection)
        {
            dynamic jsonObject = null;

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
                    AddLobbyToDatabase(gameCode, databaseConnection);
                    SendAsync(new { action = "lobbyCreated", gameCode = gameCode }, serverWebSocket);
                    break;
                }
            }
        }

        static public void JoinLobby(WebSocket serverWebSocket, string gameCode, MySqlConnection databaseConnection)
        {
            int numberOfPlayersInLobby = NumberOfPlayersInLobby(databaseConnection, gameCode);

            if (numberOfPlayersInLobby == 0)
            {
                SendAsync(new { success = 0, message = "lobbyDoesNotExists" }, serverWebSocket);
                return;
            }
            else if (numberOfPlayersInLobby == 4)
            {
                SendAsync(new { success = 0, message = "fullLobby" }, serverWebSocket);
                return;
            }
            numberOfPlayersInLobby++;
            string sCommand = $"update lobby set user{numberOfPlayersInLobby} = 'My_Username' where code = '{gameCode}'";
            Console.WriteLine("TU");
            var command = new MySqlCommand(sCommand, databaseConnection);
            Console.WriteLine("TU");
            try
            {

                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception:\n" + e);
            }
            Console.WriteLine("TU");
            SendAsync(new { success = 1, UserID = numberOfPlayersInLobby + 1 }, serverWebSocket);
        }
        static public void LeaveLobby(WebSocket serverWebSocket)
        {

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
        static public void AddLobbyToDatabase(string gameCode, MySqlConnection databaseConnection)
        {
            string sCommand = $"insert into lobby (code, user1) values ('{gameCode}', 'franta1')";
            var command = new MySqlCommand(sCommand, databaseConnection);
            command.ExecuteNonQuery();
        }
        static public async void SendAsync(dynamic jsonObject, WebSocket serverWebSocket)
        {
            string jsonString = JsonConvert.SerializeObject(jsonObject);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
            await serverWebSocket.SendAsync(new ArraySegment<byte>(jsonBytes), WebSocketMessageType.Text, false, CancellationToken.None);
        }
        static public int NumberOfPlayersInLobby(MySqlConnection databaseConnection, string code)
        {


            for (int i = 4; i > 0; i--)
            {
                string message = "";
                string sCommand = $"select user{i} from lobby where code = {code}";
                var command = new MySqlCommand(sCommand, databaseConnection);
                var reader = command.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        message = (reader.GetString(0));
                    }
                    command.Dispose();
                    reader.Close();
                }
                catch (Exception e)
                {
                    command.Dispose();
                    reader.Close();
                    continue;
                }
                return i;

            }

            return 0;
        }
    }
}
