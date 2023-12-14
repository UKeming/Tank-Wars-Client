// @File: Explosion.cs
// @Created: 2021/04/01
// @Last Modified: 2021/04/07
// @Author: Keming Chen, Yifei Sun

using System.Collections.Generic;

namespace Model
{
    public class Explosion
    {
        public static int TotalFrame = 40;

        public static List<int> CompletedExplosions = new List<int>();
        private readonly int ID;

        public int currentFrame;

        public Explosion(int id)
        {
            ID = id;
        }

        /// <summary>
        ///     Next frame.
        /// </summary>
        /// <returns>Next frame</returns>
        public int nextFrame()
        {
            if (currentFrame < TotalFrame)
                currentFrame++;
            else
                currentFrame = -1;
            return currentFrame;
        }

        /// <summary>
        ///     Next explosion ID.
        /// </summary>
        /// <returns>Explosion ID.</returns>
        public int GetID()
        {
            return ID;
        }

        /// <summary>
        ///     Get progress.
        /// </summary>
        /// <returns>Progress.</returns>
        public float GetProgress()
        {
            return currentFrame / (float) TotalFrame;
        }
    }
}