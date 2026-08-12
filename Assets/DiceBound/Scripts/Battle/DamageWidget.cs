using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using KCoreKit;
using TMPro;
using UnityEngine;

namespace DiceBound
{
    public class DamageWidget : TextWidget
    {
        [SerializeField]
        private TweenAnimationPlayer tween;

        private TextMeshProUGUI _tmpText;

        // 💡 텍스처를 한 번만 생성하고 재사용하기 위한 정적(Static) 캐시 딕셔너리
        private static Dictionary<DamageType, Texture2D> _textureCache = new Dictionary<DamageType, Texture2D>();

        private void Awake()
        {
            _tmpText = GetComponentInChildren<TextMeshProUGUI>();
        }

        public void Setup(int value, DamageType type)
        {
            if (type == DamageType.Miss)
            {
                SetText("MISS");
            }
            else
            {
                SetText($"{value}");
            }

            ApplyProceduralGradient(type);
        }

        public void Play(int hitIndex, Action<DamageWidget> callback)
        {
            StartCoroutine(PlayAnimation(hitIndex, callback));
        }

        private IEnumerator PlayAnimation(int hitIndex, Action<DamageWidget> callback)
        {
            if (hitIndex > 0)
            {
                yield return new WaitForSeconds(hitIndex * 0.04f);
            }

            // 💡 기존의 인스펙터 트윈 애니메이션은 주석 처리하거나 지웁니다.
            // yield return tween.Play();

            // 1. 연출 초기화: 스케일을 살짝 줄이고, 투명도를 100%로 맞춥니다.
            transform.localScale = Vector3.one * 0.7f;
            _tmpText.alpha = 1f;

            // DOTween 시퀀스 생성
            Sequence seq = DOTween.Sequence();

            // 2. 팝업 효과 (때리는 맛): 0.15초 만에 살짝 커졌다가 1배율로 돌아옴
            seq.Append(transform.DOScale(1.2f, 0.15f).SetEase(Ease.OutBack));
            seq.Append(transform.DOScale(1.0f, 0.1f));

            // 3. 위로 떠오르기: 현재 위치에서 Y축으로 60만큼 부드럽게 위로 이동 (0.6초 동안)
            RectTransform rect = GetComponent<RectTransform>();
            seq.Join(rect.DOAnchorPosY(rect.anchoredPosition.y + 30f, 0.6f).SetEase(Ease.OutQuad));

            // 4. 페이드아웃: 전체 0.6초 애니메이션 중, 0.4초 지점부터 글자가 서서히 투명해짐
            seq.Insert(0.4f, _tmpText.DOFade(0f, 0.3f));

            // 시퀀스가 완전히 끝날 때까지 대기
            yield return seq.WaitForCompletion();

            callback?.Invoke(this);
        }

        private void ApplyProceduralGradient(DamageType type)
        {
            if (_tmpText == null) return;

            // 텍스처로 그라데이션을 덮어씌울 것이므로, 기존 Vertex Gradient는 끕니다.
            _tmpText.enableVertexGradient = false;
            // 기본 글자색이 흰색이어야 텍스처 색상이 원본 그대로 나옵니다.
            _tmpText.color = Color.white;

            // 캐시에 해당 타입의 텍스처가 없다면 새로 생성합니다.
            if (!_textureCache.ContainsKey(type))
            {
                _textureCache[type] = GenerateGradientTexture(type);
            }

            // TMP 매터리얼의 _FaceTex (텍스처 속성)에 생성한 텍스처를 할당합니다.
            // fontMaterial을 사용하면 이 위젯만의 독립적인 매터리얼 인스턴스가 생성됩니다.
            _tmpText.fontMaterial.SetTexture("_FaceTex", _textureCache[type]);
            // 외곽선 두께 설정 (폰트 크기에 따라 0.15f ~ 0.3f 사이로 조절해보세요)
            _tmpText.outlineWidth = 9f;

            // 외곽선 색상 설정 (완전 검은색보다 살짝 투명하거나 어두운 톤이 이쁠 수 있습니다)
            _tmpText.outlineColor = new Color32(0, 0, 0, 255);
        }

        // 💡 0%(밑), 20%(중간), 100%(위) 비율로 텍스처를 그려내는 핵심 로직
        private Texture2D GenerateGradientTexture(DamageType type)
        {
            // 색상 정의
            Color bottomColor = Color.white; // 0% 구간 (항상 흰색)
            Color middleColor = Color.white; // 20% 구간
            Color topColor = Color.white;    // 100% 구간

            switch (type)
            {
                case DamageType.Normal:
                    ColorUtility.TryParseHtmlString("#FFD700", out middleColor); // 연한 주황/노랑
                    ColorUtility.TryParseHtmlString("#FF8C00", out topColor);    // 진한 주황
                    break;
                case DamageType.Critical:
                    ColorUtility.TryParseHtmlString("#FFB6C1", out middleColor); // 연핑크
                    ColorUtility.TryParseHtmlString("#FF1493", out topColor);    // 진핑크
                    break;
                case DamageType.Miss:
                    ColorUtility.TryParseHtmlString("#E6E6FA", out middleColor); // 연보라
                    ColorUtility.TryParseHtmlString("#8A2BE2", out topColor);    // 진보라
                    break;
                case DamageType.Heal:
                    ColorUtility.TryParseHtmlString("#90EE90", out middleColor); // 연두색
                    ColorUtility.TryParseHtmlString("#008000", out topColor);    // 진초록
                    break;
            }

            // 텍스처 해상도 (높을수록 부드럽지만, 64~128이면 그라데이션용으로 충분합니다)
            int height = 128;
            Texture2D tex = new Texture2D(2, height, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp; // 위아래 색상이 반복되지 않도록 고정

            Color[] pixels = new Color[2 * height];
            float midPoint = 0.2f; // 💡 20% 지점 설정

            for (int y = 0; y < height; y++)
            {
                float t = (float)y / (height - 1); // 0.0 ~ 1.0
                Color pixelColor;

                if (t <= midPoint)
                {
                    // 0% ~ 20% 구간: 흰색 -> 중간색
                    pixelColor = Color.Lerp(bottomColor, middleColor, t / midPoint);
                }
                else
                {
                    // 20% ~ 100% 구간: 중간색 -> 진한색
                    pixelColor = Color.Lerp(middleColor, topColor, (t - midPoint) / (1f - midPoint));
                }

                // 2픽셀 너비로 채워넣기
                pixels[y * 2] = pixelColor;
                pixels[y * 2 + 1] = pixelColor;
            }

            tex.SetPixels(pixels);
            tex.Apply(); // 메모리에 텍스처 적용

            return tex;
        }
    }
}