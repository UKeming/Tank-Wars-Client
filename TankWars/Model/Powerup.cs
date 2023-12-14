// @File: Powerup.cs
// @Created: 2021/04/01
// @Last Modified: 2021/04/07
// @Author: Keming Chen, Yifei Sun

using Newtonsoft.Json;
using TankWars;

namespace Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Powerup
    {
        [JsonProperty(PropertyName = "died")] private bool died;

        [JsonProperty(PropertyName = "power")] private int ID;

        [JsonProperty(PropertyName = "loc")] private Vector2D location;

        /// <summary>
        ///     Get Powerup ID.
        /// </summary>
        /// <returns>Power ID.</returns>
        public int GetID()
        {
            return ID;
        }

        /// <summary>
        ///     Get location in Vector2D.
        /// </summary>
        /// <returns>Location in Vector2D.</returns>
        public Vector2D GetLocation()
        {
            return location;
        }

        /// <summary>
        ///     Is dead or not.
        /// </summary>
        /// <returns>Bool.</returns>
        public bool isDead()
        {
            return died;
        }
    }
}