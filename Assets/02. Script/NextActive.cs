using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NextActive : MonoBehaviour
{
    public Button nextButton; //다음 버튼
    [Header("Setting Start")]
    public GameObject[] startTrueObject; //시작할때 활성화할 오브젝트
    public GameObject[] startFalseObject; //시작할때 비활성화할 오브젝트
    public UnityEvent startEvent;
    [Header("Setting Next")]
    public GameObject[] trueObject;
    public GameObject[] falseObject;
    AutoFade autoFade;
    public int SceneIndex = 0;

    private void OnEnable()
    {
        
        autoFade = FindObjectOfType<AutoFade>();
        startEvent.Invoke();
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(() =>
            {
                autoFade.FadeInOut();
                Next();
            }); ;
        }
        else
        {
            Debug.Log("다음 버튼을 없음");
        }
        if (startFalseObject.Length != 0)
        {
            for (int i = 0; i < startFalseObject.Length; i++)
            {
                startFalseObject[i].SetActive(false);
            }
        }

        if (startTrueObject.Length != 0)
        {
            for (int i = 0; i < startTrueObject.Length; i++)
            {
                startTrueObject[i].SetActive(true);
            }
        }
    }

    public void Next()
    {
        StartCoroutine(NextCo());
    }

    IEnumerator NextCo()
    {
        yield return new WaitForSeconds(autoFade.fadeDurationTime);  //fadein 시간만큼 기다림
        if (trueObject.Length == 0)
        {
            Debug.Log("trueObject를 설정해주세요");
        }
        else
        {
            for (int i = 0; i < trueObject.Length; i++)
            {
                trueObject[i].SetActive(true);
            }
        }
        if (falseObject.Length == 0)
        {
            Debug.Log("falseObject를 설정해주세요");
        }
        else
        {
            for (int i = 0; i < falseObject.Length; i++)
            {
                falseObject[i].SetActive(false);
            }
        }
    }
}
