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
using System.Data.SqlClient;
using DotNetEnv;
using MySqlConnector;
using System.Net.Http;
using System.IO;
using static Server.games;
using System.Numerics;

namespace Server
{
    class Program
    {
        public static Dictionary<string, WebSocket> AllConnection = new Dictionary<string, WebSocket>();
        public static Queue<dynamic> queue = new Queue<dynamic>();
        public static MySqlConnection databaseConnection;
        public static Dictionary<string, lobby> Lobby = new Dictionary<string, lobby>();
        public static Dictionary<string, games> Games = new Dictionary<string, games>();
        static void Main(string[] args)
        {

            databaseConnection = StartDatabase();

            Thread ConnectUsers = new Thread(Program.ConnectUsers);
            ConnectUsers.Start();

            DoTasks();

        }
        static async void DoTasks()
        {
            while (true)
            {
                if (queue.Count == 0)
                {
                    continue;
                }
                await DoTaskFromQueue();
            }
        }
        static async Task DoTaskFromQueue()
        {
            dynamic response = queue.Dequeue();
            switch (response.action.ToString())
            {
                case "createLobby":
                    await CreateLobby(response);
                    break;
                case "joinLobby":
                    JoinLobby(response);
                    break;
                case "leaveLobby":
                    LeaveLobby(response);
                    break;
                case "startGame":
                    StartGame(response);
                    break;
                case "getUsernameAndIDForStartingGame":
                    InitializeGame(response);
                    break;
                case "pickCards":
                    PickCard(response);
                    break;
                case "pickBuilding":
                    PickBuilding(response);
                    break;
                case "crashedClient":
                    Console.WriteLine(response.username.ToString() + " crashed :(");
                    DeleteClient(response);
                    break;
            }
        }
        static async void ConnectUsers()
        {
            string port = "5000";
            string url = @"http://localhost:" + port + "/";
            //string url = "http://192.168.1.111:" + port + "/";
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
                    StartConnection(context);
                }
            }
        }
        static public async void StartConnection(HttpListenerContext context)
        {
            Dictionary<string, WebSocket> LobbyConnection = new Dictionary<string, WebSocket>();
            var webSocketContext = await context.AcceptWebSocketAsync(subProtocol: null);
            WebSocket serverWebSocket = webSocketContext.WebSocket;

            //get username and add user to AllConnection
            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
            WebSocketReceiveResult result = await serverWebSocket.ReceiveAsync(buffer, CancellationToken.None);
            string jsonString = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
            Console.WriteLine(jsonString);
            dynamic response = JsonConvert.DeserializeObject(jsonString);
            AllConnection.Add(response.username.ToString(), serverWebSocket);

            //start to get messages
            int ID = 0;
            while (serverWebSocket.State == WebSocketState.Open)
            {
                ID = await GetMessage(serverWebSocket, response, ID);
            }
        }
        static public async Task<int> GetMessage(WebSocket serverWebSocket, dynamic responsePreviusMethod, int ID)
        {
            try
            {
                ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
                WebSocketReceiveResult result = await serverWebSocket.ReceiveAsync(buffer, CancellationToken.None);
                string jsonString = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                dynamic response = JsonConvert.DeserializeObject(jsonString);
                Console.WriteLine("-----------------------------------------------\n" + jsonString + "\n-----------------------------------------------");
                if (response.action == "getID")
                {
                    ID = Convert.ToInt32(response.userID.ToString());
                    return ID;
                }
                queue.Enqueue(response);
            }
            catch (Exception e)
            {
                Console.WriteLine("-----------------------------------------------\n" + e + "\n-----------------------------------------------");
                queue.Enqueue(new { action = "crashedClient", username = responsePreviusMethod.username.ToString(), userID = ID });
            }
            return ID;
        }
        static public async void PickCard(dynamic response)
        {
            // check if it's players round
            if (Convert.ToInt16(response.userID) != Games[response.gameCode.ToString()].PlayingUser)
            {
                SendToAllAsync(new { success = 0, message = "notThePlayersRound", ID = response.userID }, UsersInLobby(response.gameCode.ToString()));
                return;
            }

            // check if the card fits with user info
            for (int i = 0; i < response.cardName.Count; i++)
            {
                if (response.cardName[i] != Games[response.gameCode.ToString()].MoneyOnTable[Convert.ToInt16(response.slot[i])].name)
                {
                    SendToAllAsync(new { success = 0, message = "cardIsNotRight", ID = response.userID }, UsersInLobby(response.gameCode.ToString()));
                    return;
                }
            }

            // check if money if of max 5
            if (response.cardName.Count > 1)
            {
                int allValues = 0;

                for (int i = 0; i < response.cardName.Count; i++)
                {
                    int value = Convert.ToInt16(response.cardName[i].ToString().Substring(2, 1));
                    allValues += value;
                }
                if (allValues > 6)
                {
                    SendToAllAsync(new { success = 0, message = "valueOfCardsIsToHigh", ID = response.userID }, UsersInLobby(response.gameCode.ToString()));
                    return;
                }

            }

            // add card to the user
            for (int i = 0; i < Games[response.gameCode.ToString()].Users.Count; i++)
            {
                if (Games[response.gameCode.ToString()].Users[i].Username == response.username.ToString())
                {
                    for (int j = 0; j < response.slot.Count; j++)
                    {
                        Games[response.gameCode.ToString()].Users[i].Money.Add(Games[response.gameCode.ToString()].MoneyOnTable[Convert.ToInt16(response.slot[j])]);
                    }
                    break;
                }
            }

            // draw card from deck
            for (int j = 0; j < response.slot.Count; j++)
            {
                Games[response.gameCode.ToString()].MoneyOnTable[Convert.ToInt16(response.slot[j])] = Games[response.gameCode.ToString()].DrawMoneyCard();
            }

            for (int i = 0; i < 4; i++)
            {
                if (Games[response.gameCode.ToString()].BuildingsOnTable[i].path == "")
                {
                    Games[response.gameCode.ToString()].BuildingsOnTable[i] = Games[response.gameCode.ToString()].DrawBuildingCard();
                }
            }

            // next turn
            Games[response.gameCode.ToString()].NextPlayer(true);

            // send info
            SendToAllAsync(new { success = 1, action = "moneyCardTaken", BuildingsOnTable = Games[response.gameCode.ToString()].BuildingsOnTable, MoneyOnTable = Games[response.gameCode.ToString()].MoneyOnTable, TakenCards = response.slot, ID = response.userID, Game = Games[response.gameCode.ToString()] }, UsersInLobby(response.gameCode.ToString()));
            for (int i = 0; i < response.slot.Count; i++)
            {
                Games[response.gameCode.ToString()].Users[Convert.ToInt32(response.userID)].Money.Add(new Money(Games[response.gameCode.ToString()].MoneyOnTable[Convert.ToInt16(response.slot[i])].name.ToString(),
                                                                                                                Games[response.gameCode.ToString()].MoneyOnTable[Convert.ToInt16(response.slot[i])].path.ToString(),
                                                                                                                Convert.ToInt32(Games[response.gameCode.ToString()].MoneyOnTable[Convert.ToInt16(response.slot[i])].value),
                                                                                                                Games[response.gameCode.ToString()].MoneyOnTable[Convert.ToInt16(response.slot[i])].color.ToString(),
                                                                                                                Convert.ToBoolean(Games[response.gameCode.ToString()].MoneyOnTable[Convert.ToInt16(response.slot[i])].special)));
            }
        }
        static public async void PickBuilding(dynamic response)
        {
            // check if player is playing
            if (Convert.ToInt16(response.userID) != Games[response.gameCode.ToString()].PlayingUser)
            {
                SendToAllAsync(new { success = 0, message = "notThePlayersRound", ID = response.userID }, UsersInLobby(response.gameCode.ToString()));
                return;
            }


            // check if building are right
            for (int i = 0; i < response.cardName.Count; i++)
            {
                if (response.cardName[i] != Games[response.gameCode.ToString()].BuildingsOnTable[Convert.ToInt16(response.slot[i])].name)
                {
                    SendToAllAsync(new { success = 0, message = "buildingCardIsNotRight", ID = response.userID }, UsersInLobby(response.gameCode.ToString()));
                    return;
                }
            }

            // get id of user
            int indexOfUser = -1;
            for (int i = 0; i < Games[response.gameCode.ToString()].Users.Count; i++)
            {
                if (Games[response.gameCode.ToString()].Users[i].Username == response.username.ToString())
                {
                    indexOfUser = i;
                }
            }

            // check if money is same type
            string color = response.moneyCardsType[0].ToString();
            for (int i = 0; i < response.moneyCardsType.Count; i++)
            {
                if (response.moneyCardsType[i].ToString() != color)
                {
                    SendToAllAsync(new { success = 0, message = "moneyIsNotSameColor", ID = response.ID }, UsersInLobby(response.gameCode.ToString()));
                    return;
                }
            }

            // check if money is right color
            if (response.slot[0] == 0)
            {
                if (response.moneyCardsType[0].ToString() != "yellow")
                {
                    SendToAllAsync(new { success = 0, message = "moneyIsNotRightColor", ID = response.userID }, UsersInLobby(response.gameCode.ToString()));
                    return;
                }
            }
            else if (response.slot[0] == 1)
            {
                if (response.moneyCardsType[0].ToString() != "blue")
                {
                    SendToAllAsync(new { success = 0, message = "moneyIsNotRightColor", ID = response.userID }, UsersInLobby(response.gameCode.ToString()));
                    return;
                }
            }
            else if (response.slot[0] == 2)
            {
                if (response.moneyCardsType[0].ToString() != "green")
                {
                    SendToAllAsync(new { success = 0, message = "moneyIsNotRightColor", ID = response.userID }, UsersInLobby(response.gameCode.ToString()));
                    return;
                }
            }
            else if (response.slot[0] == 3)
            {
                if (response.moneyCardsType[0].ToString() != "brown")
                {
                    SendToAllAsync(new { success = 0, message = "moneyIsNotRightColor", ID = response.userID }, UsersInLobby(response.gameCode.ToString()));
                    return;
                }
            }
            else
            {
                Console.WriteLine("\n\n\n\nBIG PROBLEM");
            }

            // check if money is enough
            int value = 0;
            for (int i = 0; i < response.moneyCardsValue.Count; i++)
            {
                value += Convert.ToInt16(response.moneyCardsValue[i]);
            }
            bool moveFinished = true;
            if(value == Convert.ToInt16(response.price))
            {
                moveFinished = false;
            }
            if (value < Convert.ToInt16(response.price))
            {
                SendToAllAsync(new { success = 0, message = "moneyIsNotEnough", ID = response.userID }, UsersInLobby(response.gameCode.ToString()));
                return;
            }
            else if (moveFinished)
            {
                Games[response.gameCode.ToString()].NextPlayer(true);
            }

            // add building to user
            Games[response.gameCode.ToString()].Users[indexOfUser].Buildings.Add(Games[response.gameCode.ToString()].BuildingsOnTable[Convert.ToInt16(response.slot[0])]);

            if (moveFinished)
            {
                // draw new building card
                for (int i = 0; i < 4; i++)
                {
                    if (Games[response.gameCode.ToString()].BuildingsOnTable[i].path == "" || Convert.ToInt16(response.slot[0]) == i)
                    {
                        Games[response.gameCode.ToString()].BuildingsOnTable[i] = Games[response.gameCode.ToString()].DrawBuildingCard();
                    }
                }
            }
            else
            {
                Games[response.gameCode.ToString()].BuildingsOnTable[Convert.ToInt16(response.slot[0])] = new Buildings("", "", -1, -1);
            }

            // remove money cards from user
            List<string> money = new List<string>();
            for(int i = 0; i < response.moneyCardsName.Count; i++)
            {
                money.Add(response.moneyCardsName[i].ToString());
            }

            try
            {

                while (money.Count > 0)
                {
                    int indexOfRemovedCard = -1;
                    bool foundACard = false;
                    for (int i = 0; i < Games[response.gameCode.ToString()].Users[indexOfUser - 1].Money.Count; i++)
                    {
                        if (Games[response.gameCode.ToString()].Users[indexOfUser - 1].Money[i].name == money[0])
                        {
                            money.RemoveAt(0);
                            indexOfRemovedCard = i;
                            foundACard = true;
                        }
                    }
                    if (foundACard)
                    {
                        Games[response.gameCode.ToString()].Users[indexOfUser - 1].Money.RemoveAt(indexOfRemovedCard);
                    }
                    else
                    {
                        throw new Exception("při odstraňování peněz z dat došlo k chybě");
                    }
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine(e);
            }


            int buildingRarity = Convert.ToInt32(response.cardName[0].ToString().Substring(0, 1));
            Games[response.gameCode.ToString()].Users[Convert.ToInt32(response.userID)].TakenBuildings[buildingRarity - 1]++;

            // send to all users
            SendToAllAsync(new { success = 1,
                                 moveFinished = moveFinished, 
                                 action = "buildingTaken", 
                                 playersBuildings = Games[response.gameCode.ToString()].Users[Convert.ToInt32(response.userID)].TakenBuildings,
                                 moneyUsed = response.moneyCardsName,
                                 slot = response.slot[0],
                                 ID = response.userID,
                                 buildingsOnTable = Games[response.gameCode.ToString()].BuildingsOnTable[Convert.ToInt32(response.slot[0])].path,
                                 BuildingsOnTable = Games[response.gameCode.ToString()].BuildingsOnTable,
                                 username = response.username,
                                 building = response.cardName,
                                 numberOfMoneyCards = response.moneyCardsName.Count,
                                 Game = Games[response.gameCode.ToString()] },
                                    UsersInLobby(response.gameCode.ToString()));
        }
        static public void StartGame(dynamic response)
        {
            if (response.userID.ToString() != "0")
            {
                SendAsync(new { success = 0, message = "notALobbyLeader" }, AllConnection[response.username.ToString()]);
                return;
            }
            if (UsersInLobby(response.gameCode.ToString()).Count < 2)
            {
                SendAsync(new { success = 0, message = "onlyOnePlayer" }, AllConnection[response.username.ToString()]);
                return;
            }

            int numberOfUsersInLobby = UsersInLobby(response.gameCode.ToString()).Count;

            Games.Add(response.gameCode.ToString(), new games());

            Games[response.gameCode.ToString()].NumberOfPlayers = numberOfUsersInLobby;

            SendToAllAsync(new { action = "gameStarted", success = "1" }, Lobby[response.gameCode.ToString()].users);

            Lobby.Remove(response.gameCode.ToString());
        }
        static public void InitializeGame(dynamic response)
        {
            Games[response.gameCode.ToString()].AddUser(Convert.ToInt16(response.userID), response.username.ToString());
            if (Games[response.gameCode.ToString()].Users.Count == Games[response.gameCode.ToString()].NumberOfPlayers)
            {
                SendToAllAsync(new { action = "allPlayersIn", success = "1" }, UsersInLobby(response.gameCode.ToString()));
                RandomizeCards(response);
                Games[response.gameCode.ToString()].PlayingUser = 0;
                //Draw cards to table
                for (int i = 0; i < 4; i++)
                {
                    Games[response.gameCode.ToString()].MoneyOnTable[i] = Games[response.gameCode.ToString()].DrawMoneyCard();
                    Games[response.gameCode.ToString()].BuildingsOnTable[i] = Games[response.gameCode.ToString()].DrawBuildingCard();
                }
                //Draw money to players
                for (int i = 0; i < Games[response.gameCode.ToString()].NumberOfPlayers; i++)
                {
                    int sum = 0;
                    while (sum < 20)
                    {
                        Money card = Games[response.gameCode.ToString()].DrawMoneyCard();
                        Games[response.gameCode.ToString()].Users[i].Money.Add(card);
                        sum += card.value;
                    }
                }
                SendToAllAsync(new
                {
                    action = "prepareGame",
                    success = "1",
                    moneyOfPlayers = Games[response.gameCode.ToString()].Users,
                    moneyCards = Games[response.gameCode.ToString()].MoneyOnTable,
                    buildingCards = Games[response.gameCode.ToString()].BuildingsOnTable,
                    usersInGame = Games[response.gameCode.ToString()].Users,
                    Game = Games[response.gameCode.ToString()]
                }, UsersInLobby(response.gameCode.ToString()));
            }
        }
        static public void RandomizeCards(dynamic response)
        {
            using (StreamReader reader = new StreamReader(@"../../../Assets/moneyCards.txt"))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                    {
                        line = line.Trim();
                        Games[response.gameCode.ToString()].DeckOfMoney.Add(new Money(line, $@"/Assets/{line}.png", Convert.ToInt16(Convert.ToString(line[2])), Convert.ToString(line[0]) + Convert.ToString(line[1]), false));
                    }
                }
            }
            Shuffle(Games[response.gameCode.ToString()].DeckOfMoney);

            using (StreamReader reader = new StreamReader(@"../../../Assets/buildingCards.txt"))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                    {
                        line = line.Trim();
                        Games[response.gameCode.ToString()].DeckOfBuildings.Add(new Buildings(line, $@"/Assets/{line}.png", Convert.ToInt16(Convert.ToString(line[1]) + Convert.ToString(line[2])), Convert.ToInt16(Convert.ToString(line[0]))));
                    }
                }
            }
            Shuffle(Games[response.gameCode.ToString()].DeckOfBuildings);

            int indexOfA = new Random().Next(33, 54);
            int indexOfB = new Random().Next(76, 97);

            Games[response.gameCode.ToString()].DeckOfMoney.Insert(indexOfA, new Money("a", $@"/Assets/a.png", -1, "-1", true));
            Games[response.gameCode.ToString()].DeckOfMoney.Insert(indexOfB, new Money("b", $@"/Assets/b.png", -1, "-1", true));
        }
        static void Shuffle<T>(List<T> list)
        {
            Random random = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        static public string GenerateGameCode()
        {
            while (true)
            {
                Random random = new Random();
                string code = Convert.ToString(random.Next(1, 1000000));
                int codeLength = code.Length;
                for (int i = 0; i < 6 - codeLength; i++)
                {
                    code = "0" + code;
                }
                string message = "";
                string sCommand = $"select code from lobby where code = '{code}'";
                var command = new MySqlCommand(sCommand, databaseConnection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    message = (reader.GetString(0));
                }
                reader.Close();
                command.Dispose();
                if (!message.Any())
                {
                    return code;
                }
            }

        }
        static public async Task CreateLobby(dynamic response)
        {
            CreateLobbyInDB(response);
        }
        static public bool CheckGameCode(string gameCode, WebSocket serverWebSocket)
        {
            if (gameCode.Length != 6)
            {
                SendAsync(new { success = 0, message = "gameCodeIsNotSixDigit" }, serverWebSocket);
                return false;
            }
            try
            {
                int test = Convert.ToInt32(gameCode);
            }
            catch (Exception e)
            {
                SendAsync(new { success = 0, message = "gameCodeIsNotNumber" }, serverWebSocket);
                return false;
            }
            return true;
        }
        static public async void CreateLobbyInDB(dynamic response)
        {
            string gameCode = GenerateGameCode();
            AddLobbyToDatabase(gameCode);
            AddUserToLobby(new { gameCode = gameCode, username = response.username.ToString() });

            Lobby.Add(gameCode, new lobby(gameCode, UsersInLobby(gameCode)));

            await SendAsync(new { action = "lobbyCreated", users = Lobby[gameCode].users, gameCode = gameCode, success = 1 }, AllConnection[response.username.ToString()]);
        }
        public static int AddUserToLobby(dynamic response)
        {
            int firstEmptySpaceInLobby = FirstEmptySpaceInLobby(response.gameCode.ToString());
            if (firstEmptySpaceInLobby == -1)
            {
                firstEmptySpaceInLobby = 1;
            }
            string sCommand = $"update lobby set user{firstEmptySpaceInLobby} = '{response.username.ToString()}' where code = '{response.gameCode.ToString()}'";
            MySqlCommand command = new MySqlCommand(sCommand, databaseConnection);
            command.ExecuteNonQuery();
            return firstEmptySpaceInLobby;
        }
        static public string CheckTheLobby(dynamic response)
        {
            if (response.gameCode.ToString().Length != 6)
            {
                return "gameCodeIsNotSixDigit";
            }
            try
            {
                int test = Convert.ToInt32(response.gameCode.ToString());
            }
            catch (Exception e)
            {
                return "gameCodeIsNotNumber";
            }
            int firstEmptySpaceInLobby = FirstEmptySpaceInLobby(response.gameCode.ToString());
            if (firstEmptySpaceInLobby == -1)
            {
                return "lobbyDoesNotExists";
            }
            else if (firstEmptySpaceInLobby == 5)
            {
                return "fullLobby";
            }
            return "lobbyIsOk";
        }
        static public async void JoinLobby(dynamic response)
        {
            string status = CheckTheLobby(response);

            if (status != "lobbyIsOk")
            {
                await SendAsync(new { success = 0, message = status }, AllConnection[response.username.ToString()]);
                return;
            }

            int ID = AddUserToLobby(response);

            Lobby[response.gameCode.ToString()].users.Add(response.username.ToString());

            await SendAsync(new { users = Lobby[response.gameCode.ToString()].users, success = 1, userID = ID, action = "lobbyJoined" }, AllConnection[response.username.ToString()]);

            await SendToAllAsync(new { action = "updatePlayers", users = Lobby[response.gameCode.ToString()].users, success = "1" }, Lobby[response.gameCode.ToString()].users);
        }
        static public async Task LeaveLobby(dynamic response)
        {
            int userID = Convert.ToInt16(response.userID) + 1;
            string sCommand = $"update lobby set user{userID} = null where code = '{response.gameCode.ToString()}'";
            var command = new MySqlCommand(sCommand, databaseConnection);
            command.ExecuteNonQuery();
            if (FirstEmptySpaceInLobby(response.gameCode.ToString()) == -1)
            {
                sCommand = $"delete from lobby where code = '{response.gameCode.ToString()}'";
                command = new MySqlCommand(sCommand, databaseConnection);
                command.ExecuteNonQuery();
            }

            Lobby[response.gameCode.ToString()].users.Remove(response.username.ToString());
            if (Lobby[response.gameCode.ToString()].users.Count == 0)
            {
                Lobby.Remove(response.gameCode.ToString());
            }
            else
            {
                await SendToAllAsync(new { action = "updatePlayers", users = Lobby[response.gameCode.ToString()].users, success = "1" }, Lobby[response.gameCode.ToString()].users);
            }

            await SendAsync(new { success = "1", action = "lobbyLeft" }, AllConnection[response.username.ToString()]);
        }
        static public MySqlConnection StartDatabase()
        {
            // Databáze
            //connection string
            DotNetEnv.Env.Load(@"../../../.env");
            string server = "Server=127.0.0.1;";
            string user = "User ID=spojeni;";
            string password = "Password=" + Environment.GetEnvironmentVariable("mySQLPassword") + ";";
            string database = "Database=alhambra";
            string connectionString = server + user + password + database + ";Pooling=true;";

            //connection to database
            var databaseConnection = new MySqlConnection(connectionString);
            databaseConnection.Open();

            return databaseConnection;
        }
        static public void AddLobbyToDatabase(string gameCode)
        {
            string sCommand = $"insert into lobby (code) values ('{gameCode}')";
            var command = new MySqlCommand(sCommand, databaseConnection);
            command.ExecuteNonQuery();
        }
        static public async Task SendAsync(dynamic jsonObject, WebSocket serverWebSocket)
        {
            string jsonString = JsonConvert.SerializeObject(jsonObject);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
            await serverWebSocket.SendAsync(new ArraySegment<byte>(jsonBytes), WebSocketMessageType.Text, false, CancellationToken.None);
            Console.WriteLine(jsonString);
        }
        static public async Task SendToAllAsync(dynamic jsonObject, List<string> lobbyConnection)
        {
            foreach (string connection in lobbyConnection)
            {
                await SendAsync(jsonObject, AllConnection[connection]);
            }
        }
        static public int FirstEmptySpaceInLobby(string code)
        {
            //RETURNS 5 FOR FULL LOBBY, -1 FOR EMPTY LOBBY OR FIRST EMPTY SPACE
            int firstEmptySpaceInLobby = 0;
            for (int i = 4; i > 0; i--)
            {
                string message = "";
                string sCommand = $"select user{i} from lobby where code = '{code}'";
                var command = new MySqlCommand(sCommand, databaseConnection);
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    if (reader.IsDBNull(0))
                    {
                        message = "";
                    }
                    else
                    {
                        message = (reader.GetString(0));
                    }
                }
                command.Dispose();
                reader.Close();

                if (message.Any())
                {
                    firstEmptySpaceInLobby++;
                }
            }
            if (firstEmptySpaceInLobby == 4)
            {
                return 5;
            }
            else if (firstEmptySpaceInLobby == 0)
            {
                return -1;
            }

            for (int i = 1; i < 5; i++)
            {
                string message = "";
                string sCommand = $"select user{i} from lobby where code = {code}";
                var command = new MySqlCommand(sCommand, databaseConnection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.IsDBNull(0))
                    {
                        message = "";
                    }
                    else
                    {
                        message = (reader.GetString(0));
                    }
                }
                command.Dispose();
                reader.Close();

                if (!message.Any())
                {
                    return i;
                }
            }
            return 0;
        }
        static public List<string> UsersInLobby(string gameCode)
        {
            List<string> users = new List<string>();
            for (int i = 1; i < 5; i++)
            {
                string message = "";
                string sCommand = $"select user{i} from lobby where code = '{gameCode}'";
                var command = new MySqlCommand(sCommand, databaseConnection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.IsDBNull(0))
                    {
                        message = "";
                    }
                    else
                    {
                        message = (reader.GetString(0));
                    }
                }
                command.Dispose();
                reader.Close();

                if (message.Any())
                {
                    users.Add(message);
                }
            }
            return users;
        }
        static public async void DeleteClient(dynamic response)
        {
            AllConnection.Remove(response.username.ToString());

            //get gameCode where is client
            string sCommand = $"select code from lobby where user1 = '{response.username.ToString()}' or user2 = '{response.username.ToString()}' or user3 = '{response.username.ToString()}' or user4 = '{response.username.ToString()}'";
            string gameCode = "";
            var command = new MySqlCommand(sCommand, databaseConnection);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                gameCode = (reader.GetString(0));
            }
            reader.Close();
            command.Dispose();
            if (!gameCode.Any())
            {
                return;
            }

            //leave lobby
            sCommand = $"update lobby set user{Convert.ToString(Convert.ToInt16(response.userID.ToString()) + 1)} = null where code = '{gameCode}'";
            command = new MySqlCommand(sCommand, databaseConnection);
            command.ExecuteNonQuery();
            if (FirstEmptySpaceInLobby(gameCode) == -1)
            {
                sCommand = $"delete from lobby where code = '{gameCode}'";
                command = new MySqlCommand(sCommand, databaseConnection);
                command.ExecuteNonQuery();
            }

            //leave lobby object
            if (Lobby.ContainsKey(gameCode))
            {
                if (Lobby[gameCode].users.Contains(response.username.ToString()))
                {
                    Lobby[gameCode].users.Remove(response.username.ToString());
                    if (Lobby[gameCode].users.Count == 0)
                    {
                        Lobby.Remove(gameCode);
                    }
                    else
                    {
                        await SendToAllAsync(new { action = "updatePlayers", users = Lobby[gameCode].users, success = "1" }, Lobby[gameCode].users);
                    }
                }
                return;
            }
            if (Games.ContainsKey(gameCode))
            {
                if (Games[gameCode].Users.Contains(new User { Username = response.username.ToString() }))
                {
                    Games[gameCode].Users.Remove(response.username.ToString());
                    if (Games[gameCode].Users.Count == 0)
                    {
                        Games.Remove(gameCode);
                    }
                    else
                    {
                        await SendToAllAsync(new { action = "updatePlayers", users = Games[gameCode].Users, success = "1" }, Lobby[gameCode].users);
                    }
                }
            }
        }
    }
}