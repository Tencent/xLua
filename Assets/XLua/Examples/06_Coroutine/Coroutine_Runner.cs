using UnityEngine;
using XLua;
using System.Collections.Generic;
using System.Collections;
using System;

[LuaCallCSharp]
public class Coroutine_Runner : MonoBehaviour
{
    public void YieldAndCallback(object to_yield, Action callback)
    {
        StartCoroutine(CoBody(to_yield, callback));
    }

    private IEnumerator CoBody(object to_yield, Action callback)
    {
        if (to_yield is IEnumerator)
            yield return StartCoroutine((IEnumerator)to_yield);
        else
            yield return to_yield;
        callback();
    }
}

public class CoroutineConfig : GenConfig
{

    public List<Type> LuaCallCSharp
    {
        get
        {
            return new List<Type>()
            {
                typeof(WaitForSeconds),
                typeof(WWW)
            };
        }
    }

    public List<Type> CSharpCallLua
    {
        get
        {
            return new List<Type>()
            {
                
            };
        }
    }

    public List<List<string>> BlackList
    {
        get { return null; }
    }
}
