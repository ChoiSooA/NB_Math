using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;

public class AutoFade : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDurationTime = 1f;

    private void Start()
    {
        fadeImage.gameObject.SetActive(false);
    }

    public void FadeIn()
    {
        fadeImage.gameObject.SetActive(true);
        fadeImage.DOFade(1, fadeDurationTime).SetEase(Ease.Linear);
    }
    public void FadeOut()
    {
        fadeImage.DOFade(0, fadeDurationTime).SetEase(Ease.Linear);
    }
    public void FadeInOut()
    {
        fadeImage.gameObject.SetActive(true);
        StartCoroutine(FadeCoroutine());
    }
    IEnumerator FadeCoroutine()     //FadeIn -> FadeOut
    {
        fadeImage.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.01f);
        fadeImage.DOFade(1, fadeDurationTime).SetEase(Ease.Linear);
        yield return new WaitForSeconds(fadeDurationTime + 0.4f);   //fade time 이외에 0.4초 대기
        FadeOut();
        yield return new WaitForSeconds(fadeDurationTime);
        fadeImage.gameObject.SetActive(false);
    }
}
