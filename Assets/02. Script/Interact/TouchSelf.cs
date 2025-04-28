using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using System;

public class TouchSelf : MonoBehaviour
{
    public UnityEvent onClick;

    public void OnClick()
    {
        onClick.Invoke();
        Debug.Log($"{name} onclick ½ÇÇàµÊ");
    }

}