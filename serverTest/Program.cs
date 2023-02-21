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
            Console.WriteLine("Waiting for WS connection...");
            Console.WriteLine(url);

            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                if (context.Request.IsWebSocketRequest)
                {
                    ProcessRequest(context);
                    while (true)
                    {
                        Thread.Sleep(10000);
                    }
                }
            }
        }
        static async Task ProcessRequest(HttpListenerContext context)
        {
            var webSocketContext = await context.AcceptWebSocketAsync(subProtocol: null);
            WebSocket serverWebSocket = webSocketContext.WebSocket;
            Console.Write("WS connected\n--------------------");
            while (serverWebSocket.State == WebSocketState.Open)
            {
                ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
                string message = "";
                try
                {
                    WebSocketReceiveResult result = await serverWebSocket.ReceiveAsync(buffer, CancellationToken.None);
                    message = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                    Console.WriteLine("RECEIVED: " + message);
                }
                catch(Exception e)
                {
                    Console.WriteLine("Exception RECEIVEASYNC:\n" + e);
                    Console.ReadKey();
                }

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
                try
                {
                    await serverWebSocket.SendAsync(buffer, WebSocketMessageType.Text, false, CancellationToken.None);
                    Console.WriteLine("Sent: " + message);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception SENDASYNC:\n" + e);
                    Console.ReadKey();
                }
            }
        }
    }
}
