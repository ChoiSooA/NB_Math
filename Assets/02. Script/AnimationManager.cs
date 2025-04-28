using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using UnityEditor;

public class AnimationManager : MonoBehaviour
{
    public GameObject Character;
    public Animator animator;

    public Vector3 resetPosition;
    public Vector3 resetRotation;
    public Vector3 resetScale;

    Coroutine currentCoroutine;

    public static AnimationManager Instance { get; private set; }

    private void Awake()
    {
        resetPosition = Character.transform.localPosition;
        resetRotation = Character.transform.localRotation.eulerAngles;
        resetScale = Character.transform.localScale;
        animator = Character.transform.GetComponent<Animator>();
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        resetPosition = Character.transform.localPosition;
        resetRotation = Character.transform.localRotation.eulerAngles;
        resetScale = Character.transform.localScale;
    }

    public void ResetVector()
    {
        Character.transform.localPosition = resetPosition;
        Character.transform.localRotation = Quaternion.Euler(resetRotation);
        Character.transform.localScale = resetScale;
    }

    private void OnDisable()
    {
        PlayAnimation("Idle");
        ResetVector();
    }

    public void PlayAnimation(string animationName)
    {
        Debug.Log($"[PlayAnimation] {animationName}");

        foreach (var param in animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger)
            {
                animator.ResetTrigger(param.name);
            }
        }

        if (animationName != "Idle")
            animator.Play("Idle", 0, 0f); // 강제 리셋

        StartCoroutine(TriggerNextFrame(animationName));
    }

    IEnumerator TriggerNextFrame(string triggerName)
    {
        yield return null;
        animator.SetTrigger(triggerName);
    }

    public void PlayOneTime(string animationName)
    {
        if (currentCoroutine != null)           //기존 실행 중인 코루틴 정지
            StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(PlayOneTimeCo(animationName));    //새 애니메이션 트리거 실행
    }

    IEnumerator PlayOneTimeCo(string animName)
    {
        PlayAnimation(animName);

        // 상태 진입 대기
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(animName));

        float length = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(length);

        Debug.Log(animName + " 애니메이션 종료");
        PlayAnimation("Idle");

        currentCoroutine = null;
    }

    public void playAnimSet(int animnum)
    {
        playAnimSetCode(animnum, null);
    }
    public void playAnimSetCode(int animnum, System.Action onComplete = null)
    {
        gameObject.SetActive(true);
        StopAllCoroutines();
        switch (animnum)
        {
            case 0: StartCoroutine(AnimOne(onComplete)); break;
            case 1: StartCoroutine(AnimTwo(onComplete)); break;
            case 2: StartCoroutine(AnimThree(onComplete)); break;
        }
    }

    void jump()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Jump")) return;
        PlayOneTime("Jump");
        Character.transform.DOMoveY(-2.2f, 0.7f).SetEase(Ease.Linear);
    }

    

    IEnumerator AnimOne(System.Action onComplete = null)   //0
    {
        ResetVector();
        yield return new WaitForSeconds(0.5f);
        jump();
        yield return new WaitForSeconds(1f);
        PlayAnimation("Hi");
        yield return new WaitForSeconds(1.84f);
        PlayAnimation("Idle");
        yield return new WaitForSeconds(6f);
        PlayAnimation("Nice");
        yield return new WaitForSeconds(3f);
        PlayAnimation("Idle");
        yield return new WaitForSeconds(12f);
        PlayOneTime("Cheer");

        onComplete?.Invoke();
    }


    IEnumerator AnimTwo(System.Action onComplete = null)        //1
    {
        ResetVector();
        yield return new WaitForSeconds(0.5f);
        jump();/*
        yield return new WaitForSeconds(3.27f);
        PlayAnimation("Idle");*/
        yield return new WaitForSeconds(20f);
        PlayAnimation("Cheer");

        onComplete?.Invoke();
    }

    IEnumerator AnimThree(System.Action onComplete = null)      //2
    {
        Character.transform.localPosition = new Vector3(1f, -5.2f, 0f);
        yield return new WaitForSeconds(0.5f);
        jump();
        yield return new WaitForSeconds(0.9f);
        PlayOneTime("Clap");
        yield return new WaitForSeconds(10f);
        PlayOneTime("Hi");

        onComplete?.Invoke();
    }
    
}
