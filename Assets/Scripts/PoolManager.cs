using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class PoolBaseData
{
    public GameObject poolObj;
    public Transform parent;
    public int defaultSpawnCount;
    public string poolKey;
}

public static class PoolManager
{
    public static Dictionary<string, ObjPool> poolDic = new Dictionary<string, ObjPool>();

    public static void CreatePool(GameObject prefab, Transform parent, int count, string key = "")
    {
        string k = key == "" ? prefab.name : key;
        if (!poolDic.ContainsKey(k))
        {
            poolDic.Add(k, new ObjPool(prefab, parent, count));
        }
        else
        {
            Debug.Log("풀 생성 실패 : " + k);
        }
    }

    public static void CreatePool(PoolBaseData poolBaseDatas)
    {

        string k = poolBaseDatas.poolKey;
        if (!poolDic.ContainsKey(k))
        {
            poolDic.Add(k, new ObjPool(poolBaseDatas.poolObj, poolBaseDatas.parent, poolBaseDatas.defaultSpawnCount));
        }
        else
        {
            Debug.Log("풀 생성 실패 : " + k);
        }
    }

    public static GameObject GetItem(string key, Vector3 pos, float exist = -1f)
    {
        GameObject o = poolDic[key].GetItem();
        o.transform.position = pos;

        if(exist > 0f)
        {
            Util.DelayFunc(() => o.SetActive(false), exist);
        }

        return o;
    }
    public static T GetItem<T>(string key) => poolDic[key].GetItem<T>();

}

public class ObjPool
{
    private Queue<GameObject> queue = new Queue<GameObject>();
    private Transform parent;
    private GameObject prefab;

    public ObjPool(GameObject prefab, Transform parent, int count)
    {
        this.prefab = prefab;
        this.parent = parent;
        for (int i = 0; i < count; i++)
        {
            GameObject o = GameObject.Instantiate(prefab, parent);
            o.SetActive(false);
            queue.Enqueue(o);
        }
    }

    public GameObject GetItem()
    {
        GameObject o = queue.Peek();
        if (o.activeSelf)
        {
            o = GameObject.Instantiate(prefab, parent);
        }
        else
        {
            o = queue.Dequeue();
            o.SetActive(true);
        }

        queue.Enqueue(o);
        return o;
    }

    public T GetItem<T>()
    {
        GameObject o = queue.Peek();
        if (o.activeSelf)
        {
            o = GameObject.Instantiate(prefab, parent);
        }
        else
        {
            o = queue.Dequeue();
            o.SetActive(true);
        }

        queue.Enqueue(o);
        return o.GetComponent<T>();
    }
}

