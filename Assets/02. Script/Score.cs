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

    private int activatedStarCount = 0; // ���� �� �� ����

    private void Awake()
    {
        if (missionTwo == null)
        {
            missionTwo = FindObjectOfType<MissionTwo>();
            if (missionTwo == null)
            {
                Debug.LogWarning("MissionTwo�� ã�� �� �����ϴ�.");
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

        int score = missionTwo.GetNormalizedScore(); // ����ȭ�� ���ھ� ���
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

        resultText.text = finalMessage + "\n\n<size=40%>�Ʒ� ��ư�� ���� �������� �Ѿ��!</size>";
        resultText.transform.localScale = Vector3.zero;
        resultText.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
    }

    string GetMessageByStarCount(int starCount)
    {
        switch (starCount)
        {
            case 3:
                return "�Ϻ��ؿ�! �ְ���!";
            case 2:
                return "���Ҿ��!\n���� �� ������!";
            case 1:
                return "�����ƿ�,\n���ݸ� �� �����غ���!";
            default:
                return "��������\n�� ���� �� ���� �ſ���!";
        }
    }
}
