using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PopupShow : MonoBehaviour
{
    public float showTime = 0.5f; // 팝업이 보이는 시간
    public UnityEvent onShowComplete; // 팝업이 끝났을 때 호출되는 이벤트
    private void OnEnable()
    {
        StartCoroutine(ShowLittleTime());
    }

    IEnumerator ShowLittleTime()
    {
        yield return new WaitForSeconds(showTime);
        gameObject.SetActive(false);
        if(onShowComplete != null)
            onShowComplete?.Invoke(); // show event가 끝나고 나서 호출
    }
}
