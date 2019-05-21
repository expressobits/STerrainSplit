using UnityEditor;
using UnityEngine;

namespace STerrainSplit
{
    public class Utils
    {

        /// <summary>
        /// Determines whether this instance is power of two the specified x.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this instance is power of two the specified x; otherwise, <c>false</c>.
        /// </returns>
        /// <param name='x'>
        /// If set to <c>true</c> x.
        /// </param>
        static bool IsPowerOfTwo(int x)
        {
            return (x & (x - 1)) == 0;
        }


        public static bool CheckValidGameObject(GameObject gameObject)
        {
            if (gameObject == null)
            {
                Debug.LogWarning("Gameobject terrain is null!");
                return false;
            }
            Terrain parentTerrain = gameObject.GetComponent<Terrain>() as Terrain;

            if (parentTerrain == null)
            {
                Debug.LogWarning("Current selection is not a terrain");
                return false;
            }
            return true;
        }

    }
}
