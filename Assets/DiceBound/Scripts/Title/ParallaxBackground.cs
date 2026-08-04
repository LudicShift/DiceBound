using UnityEngine;

namespace DiceBound
{
    public class ParallaxBackground : MonoBehaviour
    {
        [Header("�з����� ����")]
        public float parallaxIntensityX; // ī�޶� ���󰡴� ���� (0~1)
        public float independantSpeed;   // �ڵ� ��ũ�� �ӵ�

        private Transform mainCamera;
        private float spriteWidth;
        private Vector2 initialPos;
        private float translationOffset = 0;

        private void Start()
        {
            mainCamera = Camera.main.transform;

            // �̹����� ���� ���̸� 3���� ������ 1ĭ�� �ʺ� ���մϴ�.
            spriteWidth = GetComponent<SpriteRenderer>().bounds.size.x / 3;

            // �� �信�� ���ڰ� ���� X, Y ��ġ�� ����صӴϴ�.
            initialPos = transform.position;
        }

        private void LateUpdate()
        {
            translationOffset += independantSpeed * Time.deltaTime * parallaxIntensityX;

            // X�� �з����� ���
            float parallaxOffsetX = (mainCamera.position.x * (1 - (parallaxIntensityX / 2))) + translationOffset;

            // �� ���� �Ϻ� �ذ�: �̻��� Y�� ������ �� �����, �� �信�� ������ Y��(initialPos.y)�� ������ �����մϴ�!
            transform.position = new Vector2(initialPos.x + parallaxOffsetX, initialPos.y);

            float cameraOffsetX = mainCamera.position.x - transform.position.x;

            // ȭ�� ������ ������ �̹����� ������ �̵����� ���� �ݺ�
            if (cameraOffsetX > spriteWidth / 2)
                initialPos.x += spriteWidth;
            else if (cameraOffsetX < -spriteWidth / 2)
                initialPos.x -= spriteWidth;
        }
    }
}