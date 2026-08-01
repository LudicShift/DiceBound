using UnityEngine;

public class TitleUnitMover : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField]
    [Range(2.0f, 15.0f)]
    private float speed = 4.0f; // 걷는 속도

    private void Update()
    {
        // 이제 유닛은 오직 오른쪽으로 걷기만 합니다.
        // 카메라는 TitleCameraFollower가 알아서 부드럽게 따라옵니다.
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }
}