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
                Thread.Sleep(10000);
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
                while (myWebSocket.State == WebSocketState.Open)
                {
                    try
                    {
                        Console.WriteLine("1. Založte hru\n2. Připojte se do hry");

                        //Načtení inputu a vytvoření json
                        var jsonObject = new {action = "", id = -1};
                        switch (Convert.ToInt16(Console.ReadLine()))
                        {
                            case 1:
                                jsonObject = new { action = "createLobby", id = 2 };
                                break;
                            case 2:
                                jsonObject = new { action = "leaveLobby", id = 1 };
                                break;
                        }

                        //var jsonObject = new { messageJson = Console.ReadLine() , id = 1};


                        //Poslání zprávy
                        string jsonString = JsonConvert.SerializeObject(jsonObject);
                        byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
                        await myWebSocket.SendAsync(new ArraySegment<byte>(jsonBytes), WebSocketMessageType.Text, false, CancellationToken.None);
                        
                        
                        ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
                        WebSocketReceiveResult result = await myWebSocket.ReceiveAsync(buffer, CancellationToken.None);
                        jsonString = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                        dynamic response = JsonConvert.DeserializeObject(jsonString);
                        Console.WriteLine("ID: " + response.id + "\nMessage: " + response.gameCode);
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("Exception:\n" + e);
                    }
                }
            }
        }
    }
}
