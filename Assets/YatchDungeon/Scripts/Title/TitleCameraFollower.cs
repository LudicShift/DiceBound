using UnityEngine;

public class TitleCameraFollower : MonoBehaviour
{
    [Header("추적할 대상 (유닛)")]
    public Transform targetUnit;

    [Header("카메라 설정")]
    public bool smoothCamera = true;
    public float smoothSpeed = 5.0f;

    [Header("높이 유지 설정")]
    [Tooltip("체크하면 씬 뷰에서 맞춘 카메라의 Y축 높이를 그대로 유지합니다.")]
    public bool lockYAxis = true;

    private float initialY;
    private float initialZ;

    private void Start()
    {
        // 게임 시작 시, 씬 뷰에서 사용자가 직접 세팅한 카메라의 Y, Z 위치를 기억해둡니다.
        initialY = transform.position.y;
        initialZ = transform.position.z;
    }

    private void LateUpdate()
    {
        if (targetUnit == null) return;

        // X축은 유닛을 따라갑니다.
        // Y축은 lockYAxis가 체크되어 있으면 씬 뷰에서 맞춘 처음 높이를 그대로 유지합니다.
        float targetY = lockYAxis ? initialY : targetUnit.position.y;

        Vector3 desiredPosition = new Vector3(targetUnit.position.x, targetY, initialZ);

        // 부드럽게 따라가기
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