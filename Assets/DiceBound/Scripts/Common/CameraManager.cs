using KCoreKit;
using UnityEngine;

namespace DiceBound
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