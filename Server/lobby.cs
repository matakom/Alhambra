using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class lobby
    {
        public List<string> users = new List<string>();
        string gameCode;
        public lobby(string gameCode, List<string> users)
        {
            this.gameCode = gameCode;
            foreach(var user in users)
            {
                if (this.users.Contains(user))
                {
                    continue;
                }
                this.users.Add(user);
            }
        }
        public void AddUserToLobby(string user)
        {
            users.Add(user);
            Program.SendToAllAsync(new { action = "updatePlayers", users = users}, users = users);
        }
        public void RemoveUserFromLobby(string user)
        {
            users.Remove(user);
            Program.SendToAllAsync(new { action = "updatePlayers", users = users }, users = users);
        }
    }
}
