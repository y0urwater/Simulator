using UnityEngine;
using System.Collections.Generic;

public class GenericPool<T> where T : MonoBehaviour
{
    private T prefab;
    private Queue<T> pool = new Queue<T>();

    public GenericPool(T prefab, int initialSize, Transform parent)
    {
        this.prefab = prefab;
        for (int i = 0; i < initialSize; i++)
        {
            T obj = GameObject.Instantiate(prefab, parent);
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public T Get()
    {
        if (pool.Count > 0)
        {
            T obj = pool.Dequeue();
            obj.gameObject.SetActive(true);
            return obj;
        }
        else
        {
            T obj = GameObject.Instantiate(prefab);
            return obj;
        }
    }

    public T Get(Vector3 spawnPosition)
    {
        if (pool.Count > 0)
        {
            T obj = pool.Dequeue();
            obj.transform.position = spawnPosition;
            obj.gameObject.SetActive(true);
            return obj;
        }
        else
        {
            T obj = GameObject.Instantiate(prefab);
            obj.transform.position = spawnPosition;
            return obj;
        }
    }


public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }
}
