using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class Util
{
    public static void DelayFunc(Action func, float delay)
    {
        GameManager.instance.StartCoroutine(DelayFuncCo(func, delay));
    }

    public static IEnumerator DelayFuncCo(Action func, float delay)
    {
        yield return new WaitForSeconds(delay);
        func();
    }
}
