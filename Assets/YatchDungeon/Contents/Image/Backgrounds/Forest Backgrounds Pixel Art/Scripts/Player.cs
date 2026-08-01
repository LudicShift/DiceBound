using UnityEngine;

namespace ForestBackgroundsPixelArt
{
    // 더 이상 물리엔진(Rigidbody2D)이 필요 없으므로 RequireComponent는 삭제했습니다.
    public class Player : MonoBehaviour
    {
        [SerializeField]
        [Range(2.0f, 8.0f)]
        private float speed = 4.0f; // 걷는 속도

        private Transform mainCamera;
        private float cameraZ; // 카메라의 원래 Z축 위치(깊이)를 기억하기 위함

        private void Start()
        {
            mainCamera = Camera.main.transform;
            cameraZ = mainCamera.position.z;
        }

        private void Update()
        {
            // 1. 유닛(플레이어)이 오른쪽으로 계속 자동 이동합니다.
            // (만약 유닛이 걷는 애니메이션이 있다면 애니메이터만 켜두시면 됩니다)
            transform.Translate(Vector3.right * speed * Time.deltaTime);

            // 2. 메인 카메라가 유닛의 X축을 그대로 따라가도록 만듭니다.
            // Y축과 Z축은 원래 카메라 위치를 유지합니다.
            if (mainCamera != null)
            {
                mainCamera.position = new Vector3(transform.position.x, mainCamera.position.y, cameraZ);
            }
        }
    }
}