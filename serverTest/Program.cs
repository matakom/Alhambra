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
            Console.WriteLine("Waiting for WS connection...\n");
            Console.WriteLine(url + "\n");

            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                if (context.Request.IsWebSocketRequest)
                {
                    ProcessRequest(context);
                }
            }
        }
        static async void ProcessRequest(HttpListenerContext context)
        {
            var source = new CancellationTokenSource();
            source.CancelAfter(500000);
            var webSocketContext = await context.AcceptWebSocketAsync(subProtocol: null);
            WebSocket serverWebSocket = webSocketContext.WebSocket;
            Console.WriteLine("WS connected");
            while (serverWebSocket.State == WebSocketState.Open)
            {
                ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
                Console.WriteLine(serverWebSocket.State);
                WebSocketReceiveResult result = await serverWebSocket.ReceiveAsync(buffer, source.Token);
                string message = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                Console.WriteLine(message);

                switch (message)
                {
                    case "ahoj":
                        message = "Zdravím tě dobrodruhu";
                        break;
                    case "matyáš je frajer":
                        message = "Hahaha. ten byl dobrej";
                        break;
                }

                buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
                Console.WriteLine(serverWebSocket.State);
                try
                {
                    await serverWebSocket.SendAsync(buffer, WebSocketMessageType.Text, true, source.Token);
                    Console.WriteLine("Sent: " + message);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                    Console.ReadKey();
                }
            }
        }


    }
}
