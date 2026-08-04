using UnityEngine;

namespace DiceBound
{
    public class TitleUnitMover : MonoBehaviour
    {
        [Header("�̵� ����")]
        [SerializeField]
        [Range(2.0f, 15.0f)]
        private float speed = 4.0f; // �ȴ� �ӵ�

        private void Update()
        {
            // ���� ������ ���� ���������� �ȱ⸸ �մϴ�.
            // ī�޶�� TitleCameraFollower�� �˾Ƽ� �ε巴�� ����ɴϴ�.
            transform.Translate(Vector3.right * speed * Time.deltaTime);
        }
    }
}