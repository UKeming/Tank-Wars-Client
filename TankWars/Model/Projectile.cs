// @File: Projectile.cs
// @Created: 2021/04/01
// @Last Modified: 2021/04/07
// @Author: Keming Chen, Yifei Sun

using Newtonsoft.Json;
using TankWars;

namespace Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Projectile
    {
        [JsonProperty(PropertyName = "died")] private bool died;

        [JsonProperty(PropertyName = "proj")] private int ID;

        [JsonProperty(PropertyName = "loc")] private Vector2D location;

        [JsonProperty(PropertyName = "dir")] private Vector2D orientation;

        [JsonProperty(PropertyName = "owner")] private int owner;

        /// <summary>
        ///     Get projectile ID.
        /// </summary>
        /// <returns>Projectile ID.</returns>
        public int GetID()
        {
            return ID;
        }

        /// <summary>
        ///     Get Owner.
        /// </summary>
        /// <returns>Owner ID.</returns>
        public int GetOwner()
        {
            return owner;
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
        ///     Get orientation.
        /// </summary>
        /// <returns>Orientation in Vector2D.</returns>
        public Vector2D GetOrientation()
        {
            return orientation;
        }

        /// <summary>
        ///     Whether is dead or not.
        /// </summary>
        /// <returns>Bool</returns>
        public bool isDead()
        {
            return died;
        }
    }
}