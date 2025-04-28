using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MissionOne : MonoBehaviour
{
    public GameObject objectPrefab;
    public Transform spawnCenter;
    public float radius = 0.6f;
    public float objectScale = 0.3f;

    public int maxObjects = 10;
    public int maxQuizCount = 3;
    int nowQuizCount = 0;
    int rightCount = 0;

    public AudioClip firstClip;
    public GameObject[] pannel; // 0: Correct, 1: Incorrect, 2: Clear
    public AudioClip[] audioClips;  //// 0: Correct, 1: Incorrect

    public Button checkButton;
    public Button nextButton;
    public Button spawnButton;
    public Button removeButton;
    public TMP_Text Text_Count;
    public TMP_Text Text_Quiz;

    public ObjectSpawner objectSpawner;
    public Audio_Manager audio_Manager;

    private void OnEnable()
    {
        audio_Manager.PlayMent(firstClip);
    }
    private void Start()
    {
        if (objectSpawner == null)
        {
            this.GetComponent<ObjectSpawner>();
            Debug.LogError("ObjectSpawner�� ã�� �� �����ϴ�!");
            return;
        }
        NextQuiz();

        // ObjectSpawner ����
        objectSpawner.objectPrefab = objectPrefab;
        objectSpawner.spawnCenter = spawnCenter;
        objectSpawner.maxObjects = maxObjects;
        objectSpawner.radius = radius;
        objectSpawner.objectScale = objectScale;

        spawnButton.onClick.AddListener(OnSpawnClicked);
        removeButton.onClick.AddListener(OnRemoveClicked);

        checkButton.onClick.AddListener(CheckNowCount);
        nextButton.onClick.AddListener(NextQuiz);

    }

    void OnSpawnClicked()
    {
        objectSpawner.SpawnOne();
        UpdateButtonState();
    }

    void OnRemoveClicked()
    {
        objectSpawner.DespawnOne();
        UpdateButtonState();
    }

    void UpdateButtonState()
    {
        int count = objectSpawner.NowCount;
        spawnButton.interactable = count < maxObjects;
        removeButton.interactable = count > 0;
    }

    void CheckNowCount()
    {
        if (objectSpawner.NowCount == rightCount)
        {
            if (nowQuizCount >= maxQuizCount)
            {
                pannel[2].SetActive(true);
                Audio_Manager.Instance.PlayMent(audioClips[2]);
                return;
            }
            pannel[0].SetActive(true);
            Audio_Manager.Instance.PlayMent(audioClips[0]);
            
        }
        else
        {
            pannel[1].SetActive(true);
            Audio_Manager.Instance.PlayMent(audioClips[1]);
        }

        /*// ����/���� ���� ������ ���� Ŭ���� üũ
        if (nowQuizCount >= maxQuizCount)
        {
            pannel[2].SetActive(true);
            Audio_Manager.Instance.PlayMent(audioClips[2]);
        }*/
    }

    public void NextQuiz()
    {
        if ((maxQuizCount - nowQuizCount) == 1)
        {
            Text_Count.text = "������ ��������!";
        }
        else
        {
            Text_Count.text = "������ " + (maxQuizCount - nowQuizCount - 1).ToString() + "�� ���Ҿ��!";
        }

        objectSpawner.ResetAll();
        UpdateButtonState();

        rightCount = Random.Range(0, maxObjects);
        Text_Quiz.text = "������ " + rightCount + "�� �����ּ���!";
        nowQuizCount++;
    }
}
