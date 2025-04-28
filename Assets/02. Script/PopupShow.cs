using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PopupShow : MonoBehaviour
{
    public float showTime = 0.5f; // �˾��� ���̴� �ð�
    public UnityEvent onShowComplete; // �˾��� ������ �� ȣ��Ǵ� �̺�Ʈ
    private void OnEnable()
    {
        StartCoroutine(ShowLittleTime());
    }

    IEnumerator ShowLittleTime()
    {
        yield return new WaitForSeconds(showTime);
        gameObject.SetActive(false);
        if(onShowComplete != null)
            onShowComplete?.Invoke(); // show event�� ������ ���� ȣ��
    }
}
