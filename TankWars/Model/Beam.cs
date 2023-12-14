// @File: Beam.cs
// @Created: 2021/04/01
// @Last Modified: 2021/04/07
// @Author: Keming Chen, Yifei Sun

using System.Collections.Generic;
using Newtonsoft.Json;
using TankWars;

namespace Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Beam
    {
        public static List<int> CompletedBeams = new List<int>();

        public static int TotalFrame = 90;

        public int currentFrame;

        [JsonProperty(PropertyName = "beam")] private int ID;

        [JsonProperty(PropertyName = "dir")] private Vector2D orientation;

        [JsonProperty(PropertyName = "org")] private Vector2D origin;

        [JsonProperty(PropertyName = "owner")] private int owner;

        /// <summary>
        ///     Get end position.
        /// </summary>
        /// <returns>End position in Vector2D.</returns>
        public Vector2D GetEndPosition()
        {
            return new Vector2D(0, -currentFrame * 60);
        }

        /// <summary>
        ///     Get start position.
        /// </summary>
        /// <returns>Start position in Vector2D.</returns>
        public Vector2D GetStartPosition()
        {
            if (currentFrame > TotalFrame / 3)
                return new Vector2D(0, -(currentFrame - TotalFrame / 3) * 60);
            return new Vector2D(0, 0);
        }

        /// <summary>
        ///     Next frame.
        /// </summary>
        /// <returns>Next frame.</returns>
        public int nextFrame()
        {
            if (currentFrame < TotalFrame)
                currentFrame++;
            else
                currentFrame = -1;
            return currentFrame;
        }

        /// <summary>
        ///     Get Beam ID.
        /// </summary>
        /// <returns>Beam ID.</returns>
        public int GetID()
        {
            return ID;
        }

        /// <summary>
        ///     Get current orientation.
        /// </summary>
        /// <returns>Current orientation in Vector2D.</returns>
        public Vector2D GetOrientation()
        {
            return orientation;
        }

        /// <summary>
        ///     Origin.
        /// </summary>
        /// <returns>Origin in Vector2D.</returns>
        public Vector2D GetOrigin()
        {
            return origin;
        }

        /// <summary>
        ///     Owner.
        /// </summary>
        /// <returns>Return owner.</returns>
        public int GetOwner()
        {
            return owner;
        }

        /// <summary>
        ///     Progress.
        /// </summary>
        /// <returns>Progress.</returns>
        public float GetProgress()
        {
            return currentFrame / (float) TotalFrame;
        }
    }
}