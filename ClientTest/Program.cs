using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;

namespace ClientTest
{
    internal class Program
    {

        static void Main()
        {
            ConnectToServer();
        }
        static async void ConnectToServer()
        {
            Uri serverUri = new Uri("ws://localhost:5000");

            var source = new CancellationTokenSource();
            source.CancelAfter(50000);

            using (ClientWebSocket myWebSocket = new ClientWebSocket())
            {

                try
                {
                    myWebSocket.ConnectAsync(serverUri, source.Token).Wait(source.Token);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception:" + e);
                    Console.ReadKey();
                }
                Console.WriteLine("Connected to server\n");
                while (myWebSocket.State == WebSocketState.Open)
                {
                    try
                    {
                        string message = Console.ReadLine();
                        byte[] messageyBytes = Encoding.UTF8.GetBytes(message);
                        await myWebSocket.SendAsync(new ArraySegment<byte>(messageyBytes), WebSocketMessageType.Binary, false, source.Token);
                        ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
                        WebSocketReceiveResult result = await myWebSocket.ReceiveAsync(buffer, CancellationToken.None);
                        message = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                        Console.WriteLine(message);
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("Exception:\n" + e);
                    }
                }
                while (true)
                {
                    Console.WriteLine("Jsem nepreskocitelny");
                }
                Console.ReadKey();
            }
        }
    }
}
