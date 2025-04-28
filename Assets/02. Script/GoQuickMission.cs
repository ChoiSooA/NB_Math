using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GoQuickMission : MonoBehaviour
{
    public GameObject[] trueObject;  // 활성화할 오브젝트
    public GameObject[] falseObject; // 비활성화할 오브젝트
    public Button[] BT_Mission;      // 버튼들

    public DoTEffect doTEffect;

    private void Start()
    {
        doTEffect = GetComponent<DoTEffect>();

        for (int i = 0; i < BT_Mission.Length; i++)
        {
            int missionIndex = i;
            BT_Mission[i].onClick.AddListener(() => { GoMission(missionIndex); });
        }
    }

    public void GoMission(int missionNum)
    {
        Debug.Log("GoMission called with missionNum: " + missionNum);

        // 모든 falseObject 중 활성화된 것 끄기
        foreach (GameObject obj in falseObject)
        {
            if (obj != null && obj.activeSelf)
            {
                obj.SetActive(false);
            }
        }

        doTEffect.Close();

        StartCoroutine(ActivateTrueObject(missionNum));
    }

    IEnumerator ActivateTrueObject(int missionNum)
    {
        yield return new WaitForSeconds(0.01f);

        // trueObject[missionNum]을 반드시 다시 켜준다
        if (missionNum >= 0 && missionNum < trueObject.Length && trueObject[missionNum] != null)
        {
            trueObject[missionNum].SetActive(false); // 일단 껐다가
            yield return new WaitForSeconds(0.01f);
            trueObject[missionNum].SetActive(true);
        }
    }
}
