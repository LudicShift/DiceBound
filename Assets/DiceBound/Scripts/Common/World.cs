using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    public class World : Singleton<World>
    {
        public static Transform GetTransform()
        {
            return GetInstance().transform;
        }
    }
}