// @File: GameController.cs
// @Created: 2021/04/01
// @Last Modified: 2021/04/06
// @Author: Keming Chen, Yifei Sun

using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Model;
using NetworkUtil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TankWars;

namespace GameController
{
    /// <summary>
    ///     The Controller can control all the data received from server and process it. Change Model by the data processed,
    ///     and notice View to update changes.
    /// </summary>
    public class Controller
    {
        public delegate void BeamHandler(int owner, Vector2D org, Vector2D dir);

        // Delegates:
        public delegate void ErrorHandler(string message);

        public delegate void UpdateHandler();

        // Locks:
        public static object tankLock = new object();
        public static object projectileLock = new object();
        public static object beamLock = new object();
        public static object powerupLock = new object();

        // Used to store the data received from server.
        private readonly Queue<string> dataList;


        // The object save the current state of players.
        private readonly SendObject sendObject;

        // The world that saves all data: Tank, Projectile, Powerups....
        private readonly World world;

        /// <summary>
        ///     Indicate whether the game has started.
        /// </summary>
        private bool gameStart;

        // The player's name, will only be used when connect to the server.
        private string nameToSend;

        // The ID of player who are playing this game on the window.
        private int playerID;

        // If the last data received is not end with '\n', then it will be saved here and combine with other data until the next data that is ending with '\n' is received.
        private string remainedData = "";

        // The client socketState.
        private SocketState socketState;

        private string tobeSend = "";

        /// <summary>
        ///     Initialize the Controller.
        /// </summary>
        public Controller()
        {
            dataList = new Queue<string>();
            world = new World(0);
            sendObject = new SendObject("none", "none", new Vector2D(0, 0));
        }

        // Call when there is problem when connecting/receive from server.
        public event ErrorHandler ErrorArrived;

        // UpdateArrived will be called after finish process a data from dataList.
        public event UpdateHandler UpdateArrived;

        /// <summary>
        ///     Get the world that saves all data: Tank, Projectile, Powerups....
        /// </summary>
        /// <returns></returns>
        public World getWorld()
        {
            return world;
        }

        /// <summary>
        ///     Get the ID of player who is playing this game on the window.
        /// </summary>
        /// <returns></returns>
        public int getPlayerID()
        {
            return playerID;
        }

        /// <summary>
        ///     Connect to the sever with given address.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="name"></param>
        public void ConnectToServer(string address, int port, string name)
        {
            Networking.ConnectToServer(OnConnection, address, port);
            nameToSend = name;
        }

        /// <summary>
        ///     Do when connection is successfully established.
        /// </summary>
        /// <param name="ss"></param>
        public void OnConnection(SocketState ss)
        {
            if (ss.ErrorOccurred)
            {
                ErrorArrived(ss.ErrorMessage);
            }
            else
            {
                // Send player's name to server.
                Networking.Send(ss.TheSocket, nameToSend + "\n");
                socketState = ss;

                // Start receive data from server.
                ss.OnNetworkAction = ProcessIDAndSize;
                Networking.GetData(ss);
            }
        }

        /// <summary>
        ///     The method will automatically be called when receive data from sever after the first receive.
        /// </summary>
        /// <param name="ss"></param>
        public void ProcessObject(SocketState ss)
        {
            if (ss.ErrorOccurred)
            {
                ErrorArrived(ss.ErrorMessage);
            }
            else
            {
                // If data is successfully got, process data.
                GetData(ss);
                while (dataList.Count > 1)
                {
                    var data = dataList.Dequeue();
                    var obj = JObject.Parse(data);

                    var tankToken = obj["tank"];
                    var projToken = obj["proj"];
                    var wallToken = obj["wall"];
                    var beamToken = obj["beam"];
                    var powerupToken = obj["power"];

                    // Tank case.
                    lock (tankLock)
                    {
                        if (tankToken != null)
                        {
                            var newTank = JsonConvert.DeserializeObject<Tank>(data);
                            var id = newTank.GetID();
                            world.tanks[id] = newTank;
                            if (newTank.DisConnected())
                                world.tanks.Remove(id);
                            if (!gameStart && world.tanks.ContainsKey(playerID))
                                gameStart = true;
                        }
                    }

                    // Projectile case.
                    lock (projectileLock)
                    {
                        if (projToken != null)
                        {
                            var newProjectile = JsonConvert.DeserializeObject<Projectile>(data);
                            var id = newProjectile.GetID();
                            world.projectiles[id] = newProjectile;
                            if (world.projectiles[id].isDead())
                                world.projectiles.Remove(id);
                        }
                    }

                    // Powerup case.
                    lock (powerupLock)
                    {
                        if (powerupToken != null)
                        {
                            var newPowerup = JsonConvert.DeserializeObject<Powerup>(data);
                            var id = newPowerup.GetID();
                            world.powerups[id] = newPowerup;
                            if (world.powerups[id].isDead())
                                world.powerups.Remove(id);
                        }
                    }

                    lock (beamLock)
                    {
                        if (beamToken != null)
                        {
                            var newBeam = JsonConvert.DeserializeObject<Beam>(data);
                            world.beams[newBeam.GetID()] = newBeam;
                        }
                    }

                    // Wall case.
                    if (wallToken != null)
                    {
                        var newWall = JsonConvert.DeserializeObject<Wall>(data);
                        world.walls[newWall.GetID()] = newWall;
                    }
                }

                UpdateArrived();
                SendData();
                Networking.GetData(ss);
            }
        }

        /// <summary>
        ///     Call when first receive data, here, I assume the playerID and world size will be gotten when first receive.
        /// </summary>
        /// <param name="ss"></param>
        public void ProcessIDAndSize(SocketState ss)
        {
            if (ss.ErrorOccurred)
            {
                gameStart = false;
                ErrorArrived(ss.ErrorMessage);
            }
            else
            {
                // If data is successfully got, process data.
                GetData(ss);
                playerID = int.Parse(dataList.Dequeue());
                world.size = int.Parse(dataList.Dequeue());
                UpdateArrived();
                ss.OnNetworkAction = ProcessObject;
                Networking.GetData(ss);
            }
        }


        /// <summary>
        ///     Send current player's state to server.
        /// </summary>
        /// <param name="data"></param>
        public void SendData()
        {
            tobeSend += JsonConvert.SerializeObject(sendObject) + '\n';
            Networking.Send(socketState.TheSocket, tobeSend);
            tobeSend = "";
        }

        /// <summary>
        ///     Give player's ID, return his tank.
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public Tank GetPlayerByID(int ID)
        {
            lock (tankLock)
            {
                return world.tanks[ID];
            }
        }

        /// <summary>
        ///     Get data from socket, process it by seperating them by '\n' to list and save to dataList.
        /// </summary>
        /// <param name="ss"></param>
        public void GetData(SocketState ss)
        {
            var data = ss.GetData();
            ss.RemoveData(0, data.Length);
            foreach (var item in Regex.Split(data, @"(?<=[\n])"))
                if (item.Length > 0)
                {
                    if (remainedData != "" && item[item.Length - 1] == '\n')
                    {
                        dataList.Enqueue(remainedData + item);
                        remainedData = "";
                    }
                    else if (item[item.Length - 1] != '\n')
                    {
                        remainedData += item;
                    }
                    else
                    {
                        dataList.Enqueue(item);
                    }
                }
        }

        /// <summary>
        ///     Move player base on key input, D,A,S,W, corrosponds to right, left, down, up.
        /// </summary>
        /// <param name="key"></param>
        public void MovePlayer(Keys key)
        {
            if (gameStart) sendObject.moving = keyToCommand(key);
        }

        /// <summary>
        ///     Let player shoot, left button for main shoot, right for beam shoot.
        /// </summary>
        /// <param name="button"></param>
        public void Fire(MouseButtons button)
        {
            if (gameStart)
            {
                if (button == MouseButtons.Left)
                {
                    sendObject.fire = "main";
                    tobeSend += JsonConvert.SerializeObject(sendObject) + '\n';
                    sendObject.fire = "none";
                }

                if (button == MouseButtons.Right)
                {
                    sendObject.fire = "alt";
                    tobeSend += JsonConvert.SerializeObject(sendObject) + '\n';
                    sendObject.fire = "none";
                }
            }
        }

        /// <summary>
        ///     Change the target of tank to X,Y.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        public void Target(int X, int Y)
        {
            if (gameStart)
            {
                sendObject.tdir = new Vector2D(X, Y);
                sendObject.tdir.Normalize();
            }
        }

        /// <summary>
        ///     Stop the player if the input key is not W,A,S,D
        /// </summary>
        /// <param name="key"></param>
        public void StopPlayer(Keys key)
        {
            if (gameStart && keyToCommand(key) == sendObject.moving && keyToCommand(key) != "none")
                sendObject.moving = "none";
        }

        /// <summary>
        ///     Convert keycode to commant, D,A,S,W, corrosponds to right, left, down, up. none if none of those.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private string keyToCommand(Keys key)
        {
            if (key == Keys.D)
                return "right";
            if (key == Keys.A)
                return "left";
            if (key == Keys.S)
                return "down";
            if (key == Keys.W)
                return "up";
            return "none";
        }

        /// <summary>
        ///     Return whether the game has started. Will be used by DrawingPanel to determine whether or not draw the game.
        /// </summary>
        /// <returns></returns>
        public bool gameHasStarted()
        {
            return gameStart;
        }

        /// <summary>
        ///     The class represent a player's state.
        /// </summary>
        [JsonObject(MemberSerialization.OptIn)]
        private class SendObject
        {
            /// <summary>
            ///     The fire mode, can be alt, main, none.
            /// </summary>
            [JsonProperty] public string fire;

            /// <summary>
            ///     The player's move direction, can be left, right, up, down, none.
            /// </summary>
            [JsonProperty] public string moving;

            /// <summary>
            ///     Target direction, the direction of where the player is shooting.
            /// </summary>
            [JsonProperty] public Vector2D tdir;

            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="moveMode"></param>
            /// <param name="fireMode"></param>
            /// <param name="dir"></param>
            public SendObject(string moveMode, string fireMode, Vector2D dir)
            {
                moving = moveMode;
                fire = fireMode;
                tdir = dir;
            }
        }
    }
}