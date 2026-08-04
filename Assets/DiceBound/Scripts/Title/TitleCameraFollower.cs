using UnityEngine;

namespace DiceBound
{
    public class TitleCameraFollower : MonoBehaviour
    {
        [Header("������ ��� (����)")]
        public Transform targetUnit;

        [Header("ī�޶� ����")]
        public bool smoothCamera = true;
        public float smoothSpeed = 5.0f;

        [Header("���� ���� ����")]
        [Tooltip("üũ�ϸ� �� �信�� ���� ī�޶��� Y�� ���̸� �״�� �����մϴ�.")]
        public bool lockYAxis = true;

        private float initialY;
        private float initialZ;

        private void Start()
        {
            // ���� ���� ��, �� �信�� ����ڰ� ���� ������ ī�޶��� Y, Z ��ġ�� ����صӴϴ�.
            initialY = transform.position.y;
            initialZ = transform.position.z;
        }

        private void LateUpdate()
        {
            if (targetUnit == null) return;

            // X���� ������ ���󰩴ϴ�.
            // Y���� lockYAxis�� üũ�Ǿ� ������ �� �信�� ���� ó�� ���̸� �״�� �����մϴ�.
            float targetY = lockYAxis ? initialY : targetUnit.position.y;

            Vector3 desiredPosition = new Vector3(targetUnit.position.x, targetY, initialZ);

            // �ε巴�� ���󰡱�
            if (smoothCamera)
            {
                transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            }
            else
            {
                transform.position = desiredPosition;
            }
        }
    }
}