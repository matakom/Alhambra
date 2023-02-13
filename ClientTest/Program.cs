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
            //Implementation of timeout of 5000 ms
            var source = new CancellationTokenSource();
            source.CancelAfter(500000);

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
                    string message = Console.ReadLine();
                    byte[] messageyBytes = Encoding.UTF8.GetBytes(message);
                    await myWebSocket.SendAsync(new ArraySegment<byte>(messageyBytes), WebSocketMessageType.Text, true, source.Token);
                    ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
                    WebSocketReceiveResult result = await myWebSocket.ReceiveAsync(buffer, CancellationToken.None);
                    message = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                    Console.WriteLine(message);
                }
                Console.ReadKey();
            }
        }
    }
}
