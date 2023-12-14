// @File: Tank.cs
// @Created: 2021/04/01
// @Last Modified: 2021/04/07
// @Author: Keming Chen, Yifei Sun

using Newtonsoft.Json;
using TankWars;

namespace Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Tank
    {
        [JsonProperty(PropertyName = "tdir")] private Vector2D aiming = new Vector2D(0, -1);

        [JsonProperty(PropertyName = "died")] private bool died;

        [JsonProperty(PropertyName = "dc")] private bool disconnected;

        [JsonProperty(PropertyName = "hp")] private int hitPoints;

        [JsonProperty(PropertyName = "tank")] private int ID;

        [JsonProperty(PropertyName = "join")] private bool joined;

        [JsonProperty(PropertyName = "loc")] private Vector2D location;

        [JsonProperty(PropertyName = "name")] private string name;

        [JsonProperty(PropertyName = "bdir")] private Vector2D orientation;

        [JsonProperty(PropertyName = "score")] private int score;

        /// <summary>
        ///     Return the location of the Tank.
        /// </summary>
        /// <returns></returns>
        public Vector2D GetLocation()
        {
            return location;
        }

        /// <summary>
        ///     Return the direction of the tank.
        /// </summary>
        /// <returns></returns>
        public Vector2D GetOrientation()
        {
            return orientation;
        }

        /// <summary>
        ///     Return the aiming direction.
        /// </summary>
        /// <returns></returns>
        public Vector2D GetAiming()
        {
            return aiming;
        }

        /// <summary>
        ///     Return the ID of the tank.
        /// </summary>
        /// <returns></returns>
        public int GetID()
        {
            return ID;
        }

        /// <summary>
        ///     Return the hit points of the tank.
        /// </summary>
        /// <returns></returns>
        public int GetHP()
        {
            return hitPoints;
        }

        /// <summary>
        ///     Return the name of the tank.
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return name;
        }

        /// <summary>
        ///     Return the score of the tank.
        /// </summary>
        /// <returns></returns>
        public int GetScore()
        {
            return score;
        }

        /// <summary>
        ///     Return a bool value indicate whether the tank is dead.
        /// </summary>
        /// <returns></returns>
        public bool IsDead()
        {
            return died;
        }

        /// <summary>
        ///     Return a bool value indicate whether the tank has disconnected.
        ///     The value will only be true in one frame.
        /// </summary>
        /// <returns></returns>
        public bool DisConnected()
        {
            return disconnected;
        }

        /// <summary>
        ///     Return a bool value indicate whether the tank has joined.
        ///     The value will only be true in one frame.
        /// </summary>
        /// <returns></returns>
        public bool Joined()
        {
            return joined;
        }
    }
}