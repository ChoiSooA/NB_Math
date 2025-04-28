using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FloatingObj : MonoBehaviour
{
    float floatSpeed = 0.2f;
    public List<GameObject> floatingObj = new List<GameObject>();

    private void Start()
    {
        foreach (GameObject obj in floatingObj)
        {
            AnimateFloating(obj);
        }
    }

    void AnimateFloating(GameObject obj)
    {
        // 랜덤한 이동 방향 및 거리 설정 (Y축 위아래로 + 살짝 X축도 움직이게)
        Vector3 randomOffset = new Vector3(
            Random.Range(-0.3f, 0.3f), // X 방향 (좌우 흔들림)
            Random.Range(0.5f, 1.0f), // Y 방향 (위아래 이동)
            0f
        );

        // DOTween을 사용하여 반복 애니메이션 생성
        obj.transform.DOMove(obj.transform.position + randomOffset, Random.Range(2f, 3f)) // 2~3초 동안 이동
            .SetEase(Ease.InOutSine) // 부드러운 왕복 효과
            .SetLoops(-1, LoopType.Yoyo); // 무한 반복 (위-아래-위)
    }
}
