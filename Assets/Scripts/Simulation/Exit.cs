using UnityEngine;

public class Exit : MonoBehaviour
{
    private int targetLayer = 8;

    private AgentManager agentManager;

    private void Awake()
    {
        agentManager = FindAnyObjectByType<AgentManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == targetLayer)
        {
            Agent agent = other.gameObject.GetComponent<Agent>();
            agent.Escaped();
            agentManager.Return(agent);
        }
    }
}
