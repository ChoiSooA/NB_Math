using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveSet : MonoBehaviour
{
    public GameObject[] trueActive;
    public GameObject[] falseActive;

    private void Awake()
    {
        for (int i = 0; i < trueActive.Length; i++)
        {
            trueActive[i].SetActive(true);
        }
        for (int i = 0; i < falseActive.Length; i++)
        {
            falseActive[i].SetActive(false);
        }
    }
}
