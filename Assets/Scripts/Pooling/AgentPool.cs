using UnityEngine;

public class AgentPool : MonoBehaviour
{
    private GenericPool<Agent> pool;
    public Agent prefab;

    public void Create(int size)
    {
        pool = new GenericPool<Agent>(prefab, size, transform);
    }

    public Agent Get(Vector3 pos)
    {
        return pool.Get(pos);
    }

    public void Return(Agent obj)
    {
        pool.Return(obj);
    }
}
