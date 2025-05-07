using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MissionOne : MonoBehaviour
{
    [Header("오브젝트 설정")]
    public GameObject objectPrefab;
    public Transform spawnCenter;
    public float radius = 0.6f;
    public float objectScale = 0.3f;

    [Header("퀴즈 설정")]
    public int maxObjects = 10;
    public int maxQuizCount = 3;

    [Header("패널")]
    public GameObject[] panel; // 0: 정답, 1: 오답, 2: 클리어

    [Header("오디오")]
    public AudioClip firstClip;       // 첫 설명 멘트
    public AudioClip[] audioClips;     // 0: 정답, 1: 오답, 2: 클리어
    public Audio_Manager audio_Manager;

    [Header("UI 버튼")]
    public Button checkButton;
    public Button nextButton;
    public Button spawnButton;
    public Button removeButton;

    [Header("텍스트")]
    public TMP_Text Text_Count; // 퀴즈 진행 텍스트
    public TMP_Text Text_Quiz;  // 문제 지시 텍스트

    [Header("스포너")]
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
                Debug.LogError("ObjectSpawner를 찾을 수 없습니다!");
                return;
            }
        }

        // ObjectSpawner 설정
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
            panel[0].SetActive(true); // 정답
            Audio_Manager.Instance.PlayEffect(1);
            Audio_Manager.Instance.PlayMent(audioClips[0]);

            if (nowQuizCount >= maxQuizCount)
            {
                panel[2].SetActive(true); // 클리어
                Audio_Manager.Instance.PlayEffect(4);
                Audio_Manager.Instance.PlayMent(audioClips[2]);
            }
        }
        else
        {
            panel[1].SetActive(true); // 오답
            Audio_Manager.Instance.PlayEffect(2);
            Audio_Manager.Instance.PlayMent(audioClips[1]);
        }
    }

    public void NextQuiz()
    {
        if (nowQuizCount >= maxQuizCount)
        {
            Debug.Log("모든 문제 완료!");
            return;
        }

        objectSpawner.ResetAll();
        UpdateButtonState();

        // 문제 수 갱신
        int newCount;
        do
        {
            newCount = Random.Range(0, maxObjects);
        } while (newCount == rightCount); // 중복 방지

        rightCount = newCount;

        // 퀴즈 텍스트 갱신
        if ((maxQuizCount - nowQuizCount) == 1)
        {
            Text_Count.text = "마지막 문제예요!";
        }
        else
        {
            Text_Count.text = "문제가 " + (maxQuizCount - nowQuizCount - 1).ToString() + "개 남았어요!";
        }

        Text_Quiz.text = "구슬을 " + rightCount + "개 꺼내주세요!";
        nowQuizCount++;
    }
}
