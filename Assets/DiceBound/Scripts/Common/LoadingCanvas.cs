using System;
using System.Collections;
using DG.Tweening;
using KCoreKit;
using Unity.VisualScripting;
using UnityEngine;

public class LoadingCanvas : KCoreKit.Singleton<LoadingCanvas>
{
    [SerializeField] private ImageWidget fadeImage;
    [SerializeField] private float fadeDuration = 0.3f;


    public static Tween FadeIn(Action onComplete= null)
    {
        var instance = GetInstance();
        instance.fadeImage.Show();
        instance.fadeImage.canvasGroup.alpha = 1;
        return instance.fadeImage.canvasGroup.DOFade(0, instance.fadeDuration).OnComplete(() =>
        {
            onComplete?.Invoke();
            instance.fadeImage.Hide();
        });
    }

    public static Tween FadeOut(Action onComplete = null)
    {
        var instance = GetInstance();
        instance.fadeImage.Show();
        instance.fadeImage.canvasGroup.alpha = 0;
        return instance.fadeImage.canvasGroup.DOFade(1, instance.fadeDuration).OnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }
}