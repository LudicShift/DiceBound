using UnityEngine;

namespace DiceBound.Contents.Image.Backgrounds.Forest_Backgrounds_Pixel_Art.Scripts
{
    // �� �̻� ��������(Rigidbody2D)�� �ʿ� �����Ƿ� RequireComponent�� �����߽��ϴ�.
    public class Player : MonoBehaviour
    {
        [SerializeField]
        [Range(2.0f, 8.0f)]
        private float speed = 4.0f; // �ȴ� �ӵ�

        private Transform mainCamera;
        private float cameraZ; // ī�޶��� ���� Z�� ��ġ(����)�� ����ϱ� ����

        private void Start()
        {
            mainCamera = Camera.main.transform;
            cameraZ = mainCamera.position.z;
        }

        private void Update()
        {
            // 1. ����(�÷��̾�)�� ���������� ��� �ڵ� �̵��մϴ�.
            // (���� ������ �ȴ� �ִϸ��̼��� �ִٸ� �ִϸ����͸� �ѵνø� �˴ϴ�)
            transform.Translate(Vector3.right * speed * Time.deltaTime);

            // 2. ���� ī�޶� ������ X���� �״�� ���󰡵��� ����ϴ�.
            // Y��� Z���� ���� ī�޶� ��ġ�� �����մϴ�.
            if (mainCamera != null)
            {
                mainCamera.position = new Vector3(transform.position.x, mainCamera.position.y, cameraZ);
            }
        }
    }
}