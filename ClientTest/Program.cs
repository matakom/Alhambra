using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using Newtonsoft.Json;

namespace ClientTest
{
    internal class Program
    {

        static void Main()
        {
            ConnectToServer();
            while (true)
            {
                Thread.Sleep(1000);
            }
        }
        static async Task ProcessUser(ClientWebSocket myWebSocket)
        {
            while (myWebSocket.State == WebSocketState.Open)
            {
                Console.WriteLine("1. Založte hru\n2. Připojte se do hry");

                //Načtení inputu a vytvoření json
                //V budoucnu vstup (kliknutí)
                switch (Convert.ToInt16(Console.ReadLine()))
                {
                    case 1:
                        CreateLobby(myWebSocket);
                        break;
                    case 2:
                        Console.Clear();
                        JoinGame(myWebSocket);
                        break;
                }
            }
        }
        static async Task ConnectToServer()
        {
            Uri serverUri = new Uri("ws://localhost:5000");

            using (ClientWebSocket myWebSocket = new ClientWebSocket())
            {

                try
                {
                    await myWebSocket.ConnectAsync(serverUri, CancellationToken.None);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception:" + e);
                    Console.ReadKey();
                }
                Console.WriteLine("Connected to server\n--------------------");
                await ProcessUser(myWebSocket);
            }
        }
        static public string InputGameCode()
        {
            string codeString = "";
            while (true)
            {
                Console.WriteLine("Zadejte šest cifer kódu hry: ");
                try
                {
                    int code = Convert.ToInt32(Console.ReadLine());
                    codeString = Convert.ToString(code);
                    if (codeString.Length != 6)
                    {
                        throw new Exception("Kód nemá 6 cifer");
                    }
                    break;
                }
                catch (Exception)
                {
                    Console.WriteLine($"Kód " + codeString + " je chybně zapsán. Zadejte správný kód!");
                }
            }
            return codeString;
        }
        static public async void JoinGame(ClientWebSocket myWebSocket)
        {

            dynamic response = await SendAndReceiveAsync(new { action = "joinLobby", gameCode = InputGameCode() }, myWebSocket);
            Console.WriteLine("Jsme tady");
            if (response.success == 1)
            {
                Console.WriteLine("You have joined a game!");

            }
            else if (response.success == 0)
            {
                Console.WriteLine("There is no game with this code! " + response.message);
            }

        }
        static public async Task<object> SendAndReceiveAsync(dynamic jsonObject, ClientWebSocket myWebSocket)
        {
            string jsonString = JsonConvert.SerializeObject(jsonObject);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
            await myWebSocket.SendAsync(new ArraySegment<byte>(jsonBytes), WebSocketMessageType.Text, false, CancellationToken.None);


            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
            WebSocketReceiveResult result = await myWebSocket.ReceiveAsync(buffer, CancellationToken.None);
            jsonString = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
            return JsonConvert.DeserializeObject(jsonString);
        }
        static public async void CreateLobby(ClientWebSocket myWebSocket)
        {
            dynamic response = await SendAndReceiveAsync(new { action = "createLobby", id = 2 }, myWebSocket);
            Console.WriteLine("GameCode: " + response.gameCode);
        }
    }
}
