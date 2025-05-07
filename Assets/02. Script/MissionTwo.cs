using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MissionTwo : MonoBehaviour
{
    [Header("게임 설정")]
    public int questionCount = 5;
    public Transform leftPosition;
    public Transform rightPosition;
    public GameObject objectPrefab;
    public float radius = 0.5f;
    public float objectScale = 0.3f;
    public Material One_Mat;
    public Material Two_Mat;

    [Header("UI")]
    public TMP_Text instructionText;
    public Button leftButton;
    public Button rightButton;
    public Button nextButton;
    public GameObject successPanel;

    [Header("구슬 수 텍스트")]
    public TMP_Text leftCountText;
    public TMP_Text rightCountText;

    [Header("스포너")]
    public ObjectSpawner leftSpawner;
    public ObjectSpawner rightSpawner;

    [Header("캐릭터 프리팹")]
    public GameObject[] leftCharacters;
    public GameObject[] rightCharacters;

    [Header("애니메이터 설정")]
    public RuntimeAnimatorController leftAnimatorController;
    public RuntimeAnimatorController rightAnimatorController;

    [Header("캐릭터 표시 위치")]
    public Transform leftCharacterAnchor;
    public Transform rightCharacterAnchor;

    [Header("멘트 오디오")]
    public Audio_Manager audio_Manager;
    public AudioClip infoClip;
    public AudioClip correctClip;
    public AudioClip wrongClip;
    public AudioClip clearClip;


    private GameObject currentLeftCharacter;
    private GameObject currentRightCharacter;
    private Animator leftAnimator;
    private Animator rightAnimator;

    private int currentQuestion = 0;
    private int leftCount;
    private int rightCount;

    [HideInInspector] public int score = 0;

    private void Awake()
    {
        leftButton.onClick.AddListener(() => OnAnswerSelected("left"));
        rightButton.onClick.AddListener(() => OnAnswerSelected("right"));
        nextButton.onClick.AddListener(NextQuestion);

        SetupSpawner(leftSpawner, leftPosition, One_Mat);
        SetupSpawner(rightSpawner, rightPosition, Two_Mat);

        NextQuestion();
    }

    private void OnEnable()
    {
        currentQuestion = 0;
        successPanel.SetActive(false);
        nextButton.gameObject.SetActive(true);
        audio_Manager.PlayMent(infoClip);
        NextQuestion();
    }

    void SetupSpawner(ObjectSpawner spawner, Transform positionAnchor, Material mat)
    {
        spawner.objectPrefab = objectPrefab;
        spawner.spawnCenter = positionAnchor;
        spawner.radius = radius;
        spawner.objectScale = objectScale;
        spawner.objectMat = mat;
    }

    void NextQuestion()
    {
        if (currentQuestion >= questionCount)
        {
            successPanel.SetActive(true);
            instructionText.text = "모든 문제를 완료했어요!";
            leftButton.interactable = false;
            rightButton.interactable = false;
            nextButton.gameObject.SetActive(false);

            

            return;
        }

        currentQuestion++;

        ShowCharacters(currentQuestion - 1);

        leftSpawner.ResetAll();
        rightSpawner.ResetAll();

        leftCountText.text = "";
        rightCountText.text = "";

        leftCount = Random.Range(1, 11);
        do { rightCount = Random.Range(1, 11); } while (rightCount == leftCount);

        leftSpawner.SpawnMultiple(leftCount);
        rightSpawner.SpawnMultiple(rightCount);

        instructionText.text = "구슬이 더 많은 캐릭터를 누르세요!";
        leftButton.interactable = true;
        rightButton.interactable = true;
        nextButton.gameObject.SetActive(false);
    }

    void ShowCharacters(int index)
    {
        if (currentLeftCharacter != null) Destroy(currentLeftCharacter);
        if (currentRightCharacter != null) Destroy(currentRightCharacter);

        int leftIndex = index % leftCharacters.Length;
        int rightIndex = index % rightCharacters.Length;

        currentLeftCharacter = Instantiate(leftCharacters[leftIndex], leftCharacterAnchor);
        currentRightCharacter = Instantiate(rightCharacters[rightIndex], rightCharacterAnchor);

        leftAnimator = currentLeftCharacter.GetComponent<Animator>();
        rightAnimator = currentRightCharacter.GetComponent<Animator>();

        if (leftAnimator != null)
            leftAnimator.runtimeAnimatorController = leftAnimatorController;

        if (rightAnimator != null)
            rightAnimator.runtimeAnimatorController = rightAnimatorController;
    }

    void OnAnswerSelected(string side)
    {
        bool isLeftCorrect = leftCount > rightCount;
        bool isRightCorrect = rightCount > leftCount;

        bool answeredCorrectly = (side == "left" && isLeftCorrect) || (side == "right" && isRightCorrect);

        instructionText.text = "";

        leftButton.interactable = false;
        rightButton.interactable = false;

        leftCountText.text = leftSpawner.NowCount.ToString();
        rightCountText.text = rightSpawner.NowCount.ToString();

        StartCoroutine(ReduceMarblesCoroutine(leftSpawner, rightSpawner, answeredCorrectly));
    }

    public int GetNormalizedScore()
    {
        float normalized = (float)score / questionCount * 3f;
        return Mathf.RoundToInt(normalized);
    }

    IEnumerator ReduceMarblesCoroutine(ObjectSpawner left, ObjectSpawner right, bool answeredCorrectly)
    {
        nextButton.gameObject.SetActive(false);

        int steps = Mathf.Min(left.NowCount, right.NowCount);
        for (int i = 0; i < steps; i++)
        {
            if (left.NowCount > 0) left.DespawnOne();
            if (right.NowCount > 0) right.DespawnOne();

            leftCountText.text = left.NowCount.ToString();
            rightCountText.text = rightSpawner.NowCount.ToString();

            yield return new WaitForSeconds(0.6f);
        }

        if (leftCount > rightCount)
        {
            leftAnimator?.Play("Nice");
            rightAnimator?.Play("Cry");
        }
        else
        {
            rightAnimator?.Play("Nice");
            leftAnimator?.Play("Cry");
        }

        if (answeredCorrectly)
        {
            score++;
            Audio_Manager.Instance.PlayMent(correctClip);
        }
        else
        {
            Audio_Manager.Instance.PlayMent(wrongClip);
        }

        int diff = Mathf.Abs(left.NowCount - right.NowCount);

        string resultMessage = answeredCorrectly
            ? $"선택한 캐릭터가 <size=110%>{diff}개</size> 더 많아서 <color=#76B467>정답</color>이에요!"
            : $"선택한 캐릭터가 <size=110%>{diff}개</size> 적어서 <color=#DD6C6C>틀렸어요</color>!";

        if (currentQuestion < questionCount)
        {
            instructionText.text = resultMessage + "\n<size=80%>다음 문제를 눌러주세요.</size>";
        }
        else
        {
            instructionText.text = resultMessage + "\n<size=80%>모든 퀴즈가 끝났어요! 다음으로 넘어가요.</size>";

            // 정답 여부에 따라 맞는 clip 넘기기
            AudioClip firstClip = answeredCorrectly ? correctClip : wrongClip;
            StartCoroutine(PlaySequentialClips(firstClip, clearClip));
        }

        nextButton.gameObject.SetActive(true);
    }

    IEnumerator PlaySequentialClips(AudioClip first, AudioClip second)
    {
        audio_Manager.Ment_audioSource.clip = first;
        audio_Manager.Ment_audioSource.Play();
        yield return new WaitForSeconds(first.length+0.3f);

        audio_Manager.Ment_audioSource.clip = second;
        audio_Manager.Ment_audioSource.Play();
    }
}