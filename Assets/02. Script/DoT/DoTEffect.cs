/*
 * 사이즈는 작은 사이즈로 만들어둬야함, 피봇은 사이즈가 퍼지게 할 위치로 설정해야함
 * 바운드와 무브는 원래 위치가 아닌 등장할 위치로 설정해야함
 * 알파는 기본적으로 0으로 설정해두어야함(나타나는게 시작, 사라지는 게 끝)
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Microsoft.Unity.VisualStudio.Editor;

public class DoTEffect : MonoBehaviour
{

    public enum State
    {
        Size,
        Bound,
        Move,
        Alpha,
        Blink,
    }
    public State state;                 //사이즈, 바운드, 무브, 알파 등 사용할 효과
    public float durationTime = 1f;     //지속시간
    public Vector3 targetVector;        //타겟 벡터(사이즈, 위치 등)
    Vector3 OriginalSize;               //원래 사이즈
    Vector3 OriginalPosition;           //원래 위치
    public GameObject activeOff;        //효과가 끝난 후 비활성화할 오브젝트

    private void Awake()
    {
        OriginalSize = transform.localScale;
        OriginalPosition = transform.position;
        if (activeOff != null)
            activeOff.SetActive(false);
    }
    private void OnEnable()
    {
        StartDoT();
    }
    public void Close()
    {
        EndDoT();
    }


    public void StartDoT()
    {
        switch (state)
        {
            case State.Size:
                SizeStart();
                break;
            case State.Bound:
                BoundStart();
                break;
            case State.Move:
                MoveStart();
                break;
            case State.Alpha:
                AlphaStart();
                break;
            case State.Blink:
                BlinkStart();
                break;
        }
    }
    public void EndDoT()
    {
        switch (state)
        {
            case State.Size:
                StartCoroutine(SizeEnd());
                break;
            case State.Alpha:
                StartCoroutine(AlphaEnd());
                break;
            default:
                StartCoroutine(OriginalEnd());
                break;
        }
    }

    void SizeStart()
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(targetVector, durationTime);
    }
    void BoundStart()
    {
        transform.DOLocalMove(targetVector, durationTime).SetEase(Ease.OutBounce);
    }
    void MoveStart()
    {
        transform.DOLocalMove(targetVector, durationTime);
    }
    void AlphaStart()
    {
        transform.GetComponent<UnityEngine.UI.Image>().DOFade(1, durationTime);
    }
    void BlinkStart()
    {
        transform.GetComponent<TextMeshProUGUI>().DOFade(0f, durationTime).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
    }
    IEnumerator SizeEnd()
    {
        transform.DOScale(Vector3.zero, durationTime);
        yield return new WaitForSeconds(durationTime * 0.8f);
        transform.localScale = Vector3.zero;
        activeOff.SetActive(false);
        yield return null;
    }
    IEnumerator OriginalEnd()
    {
        transform.DOMove(OriginalPosition, durationTime);
        yield return new WaitForSeconds(durationTime * 0.8f);
        transform.position = OriginalPosition;
        activeOff.gameObject.SetActive(false);
        yield return null;
    }
    IEnumerator AlphaEnd()
    {
        transform.GetComponent<UnityEngine.UI.Image>().DOFade(0, durationTime);
        yield return new WaitForSeconds(durationTime);
        activeOff.SetActive(false);
        yield return null;
    }
}
