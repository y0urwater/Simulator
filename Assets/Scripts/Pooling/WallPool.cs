using UnityEngine;

public class WallPool : MonoBehaviour
{
    public WallObject wallPrefab;
    private GenericPool<WallObject> wallPool;

    public void Create(int size)
    {
        wallPool = new GenericPool<WallObject>(wallPrefab, size, transform);
    }

    public WallObject Get()
    {
        return wallPool.Get();
    }

    public void Return(WallObject obj)
    {
        wallPool.Return(obj);
    }
}
