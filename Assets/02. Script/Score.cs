using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class Score : MonoBehaviour
{
    public MissionTwo missionTwo;
    public GameObject[] starIcons;
    public TMP_Text resultText;

    public float starDelay = 0.3f;
    public float textDelay = 0.5f;

    private int activatedStarCount = 0; // 켜진 별 수 저장

    private void Awake()
    {
        if (missionTwo == null)
        {
            missionTwo = FindObjectOfType<MissionTwo>();
            if (missionTwo == null)
            {
                Debug.LogWarning("MissionTwo를 찾을 수 없습니다.");
            }
        }
    }

    private void OnEnable()
    {
        resultText.text = "";
        StartCoroutine(ShowStarsAndMessageCoroutine());
    }

    IEnumerator ShowStarsAndMessageCoroutine()
    {
        if (missionTwo == null) yield break;

        int score = missionTwo.GetNormalizedScore(); // 정상화된 스코어 사용
        activatedStarCount = 0;

        foreach (GameObject star in starIcons)
        {
            if (star != null)
            {
                star.SetActive(false);
                star.transform.localScale = Vector3.zero;
            }
        }

        for (int i = 0; i < score && i < starIcons.Length; i++)
        {
            yield return new WaitForSeconds(starDelay);
            starIcons[i].SetActive(true);
            starIcons[i].transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
            activatedStarCount++;
        }

        yield return new WaitForSeconds(textDelay);

        string finalMessage = GetMessageByStarCount(activatedStarCount);

        resultText.text = finalMessage + "\n\n<size=40%>아래 버튼을 눌러 다음으로 넘어가요!</size>";
        resultText.transform.localScale = Vector3.zero;
        resultText.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
    }

    string GetMessageByStarCount(int starCount)
    {
        switch (starCount)
        {
            case 3:
                return "완벽해요! 최고예요!";
            case 2:
                return "좋았어요!\n거의 다 맞췄어요!";
            case 1:
                return "괜찮아요,\n조금만 더 집중해봐요!";
            default:
                return "다음에는\n더 잘할 수 있을 거예요!";
        }
    }
}
