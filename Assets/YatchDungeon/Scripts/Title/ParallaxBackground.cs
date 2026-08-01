using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("패럴랙스 설정")]
    public float parallaxIntensityX; // 카메라를 따라가는 정도 (0~1)
    public float independantSpeed;   // 자동 스크롤 속도

    private Transform mainCamera;
    private float spriteWidth;
    private Vector2 initialPos;
    private float translationOffset = 0;

    private void Start()
    {
        mainCamera = Camera.main.transform;

        // 이미지의 가로 길이를 3으로 나누어 1칸의 너비를 구합니다.
        spriteWidth = GetComponent<SpriteRenderer>().bounds.size.x / 3;

        // 씬 뷰에서 예쁘게 맞춘 X, Y 위치를 기억해둡니다.
        initialPos = transform.position;
    }

    private void LateUpdate()
    {
        translationOffset += independantSpeed * Time.deltaTime * parallaxIntensityX;

        // X축 패럴랙스 계산
        float parallaxOffsetX = (mainCamera.position.x * (1 - (parallaxIntensityX / 2))) + translationOffset;

        // ★ 문제 완벽 해결: 이상한 Y축 공식을 싹 지우고, 씬 뷰에서 세팅한 Y값(initialPos.y)에 무조건 고정합니다!
        transform.position = new Vector2(initialPos.x + parallaxOffsetX, initialPos.y);

        float cameraOffsetX = mainCamera.position.x - transform.position.x;

        // 화면 밖으로 나가면 이미지를 앞으로 이동시켜 무한 반복
        if (cameraOffsetX > spriteWidth / 2)
            initialPos.x += spriteWidth;
        else if (cameraOffsetX < -spriteWidth / 2)
            initialPos.x -= spriteWidth;
    }
}