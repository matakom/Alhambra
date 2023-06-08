using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Klient.Models
{
    public class Global
    {
        public static string GameCode { get; set; }
        public static string Username { get; set; }
        public static ClientWebSocket WebSocketConnection { get; set; }
        public static string Status { get; set; }
        public static int ID { get; set; }
        static public async Task<dynamic> WaitingForMessage()
        {
            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[64000]);
            WebSocketReceiveResult result = await Global.WebSocketConnection.ReceiveAsync(buffer, CancellationToken.None);
            string jsonString = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
            Debug.WriteLine(jsonString);
            return JsonConvert.DeserializeObject(jsonString);
        }
        static public async Task SendAsync(dynamic jsonObject)
        {
            string jsonString = JsonConvert.SerializeObject(jsonObject);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
            Debug.WriteLine(jsonString);
            await Global.WebSocketConnection.SendAsync(new ArraySegment<byte>(jsonBytes), WebSocketMessageType.Text, false, CancellationToken.None);
        }
    }
    public static class Lobby
    {
        public static string[] Users = new string[4];
        //public static Dictionary<string, string> Cards = new Dictionary<string, string>();
    }
}
