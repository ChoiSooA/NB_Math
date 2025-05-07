using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MissionOne : MonoBehaviour
{
    [Header("������Ʈ ����")]
    public GameObject objectPrefab;
    public Transform spawnCenter;
    public float radius = 0.6f;
    public float objectScale = 0.3f;

    [Header("���� ����")]
    public int maxObjects = 10;
    public int maxQuizCount = 3;

    [Header("�г�")]
    public GameObject[] panel; // 0: ����, 1: ����, 2: Ŭ����

    [Header("�����")]
    public AudioClip firstClip;       // ù ���� ��Ʈ
    public AudioClip[] audioClips;     // 0: ����, 1: ����, 2: Ŭ����
    public Audio_Manager audio_Manager;

    [Header("UI ��ư")]
    public Button checkButton;
    public Button nextButton;
    public Button spawnButton;
    public Button removeButton;

    [Header("�ؽ�Ʈ")]
    public TMP_Text Text_Count; // ���� ���� �ؽ�Ʈ
    public TMP_Text Text_Quiz;  // ���� ���� �ؽ�Ʈ

    [Header("������")]
    public ObjectSpawner objectSpawner;

    private int nowQuizCount = 0;
    private int rightCount = 0;

    private void OnEnable()
    {
        InitializeGame();
    }

    private void Start()
    {
        if (objectSpawner == null)
        {
            objectSpawner = GetComponent<ObjectSpawner>();
            if (objectSpawner == null)
            {
                Debug.LogError("ObjectSpawner�� ã�� �� �����ϴ�!");
                return;
            }
        }

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

    private void InitializeGame()
    {
        nowQuizCount = 0;
        rightCount = 0;

        if (audio_Manager != null && firstClip != null)
        {
            audio_Manager.PlayMent(firstClip);
        }

        foreach (var p in panel)
        {
            p.SetActive(false);
        }

        objectSpawner?.ResetAll();
        UpdateButtonState();
        NextQuiz();
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
            panel[0].SetActive(true); // ����
            Audio_Manager.Instance.PlayEffect(1);
            Audio_Manager.Instance.PlayMent(audioClips[0]);

            if (nowQuizCount >= maxQuizCount)
            {
                panel[2].SetActive(true); // Ŭ����
                Audio_Manager.Instance.PlayEffect(4);
                Audio_Manager.Instance.PlayMent(audioClips[2]);
            }
        }
        else
        {
            panel[1].SetActive(true); // ����
            Audio_Manager.Instance.PlayEffect(2);
            Audio_Manager.Instance.PlayMent(audioClips[1]);
        }
    }

    public void NextQuiz()
    {
        if (nowQuizCount >= maxQuizCount)
        {
            Debug.Log("��� ���� �Ϸ�!");
            return;
        }

        objectSpawner.ResetAll();
        UpdateButtonState();

        // ���� �� ����
        int newCount;
        do
        {
            newCount = Random.Range(0, maxObjects);
        } while (newCount == rightCount); // �ߺ� ����

        rightCount = newCount;

        // ���� �ؽ�Ʈ ����
        if ((maxQuizCount - nowQuizCount) == 1)
        {
            Text_Count.text = "������ ��������!";
        }
        else
        {
            Text_Count.text = "������ " + (maxQuizCount - nowQuizCount - 1).ToString() + "�� ���Ҿ��!";
        }

        Text_Quiz.text = "������ " + rightCount + "�� �����ּ���!";
        nowQuizCount++;
    }
}
