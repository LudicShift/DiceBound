using KCoreKit;
using UnityEngine;

namespace YatchDungeon
{
    public class CameraManager : Singleton<CameraManager>
    {
        public Camera mainCamera;

        public static Camera GetMainCamera()
        {
            return GetInstance().mainCamera;
        }
        
    }
}