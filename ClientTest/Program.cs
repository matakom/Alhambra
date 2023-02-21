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
                        string message = Console.ReadLine();
                        byte[] messageyBytes = Encoding.UTF8.GetBytes(message);
                        await myWebSocket.SendAsync(new ArraySegment<byte>(messageyBytes), WebSocketMessageType.Binary, false, CancellationToken.None);// Wait(CancellationToken.None);
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
            }
        }
    }
}
