using UnityEngine;

public class ExitControlPool : MonoBehaviour
{
    public ExitControl controlPrefab;
    private GenericPool<ExitControl> pool;

    public void Create(int size)
    {
        pool = new GenericPool<ExitControl>(controlPrefab, size, transform);
    }

    public ExitControl Get()
    {
        return pool.Get();
    }

    public void Return(ExitControl obj)
    {
        pool.Return(obj);
    }
}
