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

namespace serverTest
{
    class Program
    {

        static void Main(string[] args)
        {
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
                    UserProcess(context);
                }
            }
        }
        static async Task UserProcess(HttpListenerContext context)
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
                Console.WriteLine("RECEIVED FROM CLIENT: " + response.id + "\nMESSAGE: " + response.messageJson);

                switch (response.action.ToString())
                {
                    case "createLobby":
                        CreateLobby(serverWebSocket);
                        break;
                    case "leaveLobby":
                        LeaveLobby(serverWebSocket);
                        break;
                }
                /*

                switch (response.messageJson.ToString())
                {
                    case "ahoj":
                        response.messageJson = "Zdravím tě dobrodruhu";
                        break;
                    case "matyáš je frajer":
                        response.messageJson = "Hahaha. ten byl dobrej";
                        break;
                    default:
                        response.messageJson = "ECHO";
                        break;
                }
                jsonString = JsonConvert.SerializeObject(response);
                buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(jsonString));
                try
                {
                    await serverWebSocket.SendAsync(buffer, WebSocketMessageType.Text, false, CancellationToken.None);
                    Console.WriteLine("Sent: " + response.messageJson);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception SENDASYNC:\n" + e);
                    Console.ReadKey();
                }
                */
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

        static public async void CreateLobby(WebSocket serverWebSocket)
        {
            string gameCode = GenerateGameCode();
            // Upravit databázi her

            var jsonObject = new {gameCode = gameCode, id = 1 };


            //Poslání zprávy
            string jsonString = JsonConvert.SerializeObject(jsonObject);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
            await serverWebSocket.SendAsync(new ArraySegment<byte>(jsonBytes), WebSocketMessageType.Text, false, CancellationToken.None);

        }

        static public void LeaveLobby(WebSocket serverWebSocket)
        {
            // upravit databázi
        }
    }
}
