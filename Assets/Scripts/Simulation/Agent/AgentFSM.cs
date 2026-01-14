using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentFSM : MonoBehaviour
{
    private NavMeshAgent agent;
    private NavMeshObstacle obstacle;
    private Rigidbody rb;

    [SerializeField] private Vector3 target = new Vector3();

    private Coroutine activeCoroutine;

    public State currentState = State.None;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        obstacle = GetComponent<NavMeshObstacle>();
        rb = GetComponent<Rigidbody>();
    }

    public enum State
    {
        None,
        Idle,
        Evacuating,
        Blocked,
        Escaped,
        Dead
    }

    public void ChangeState(State state)
    {
        if (currentState == state || currentState == State.Dead) return;

        currentState = state;

        EnterNewState(state);
    }

    private void EnterNewState(State state)
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }

        switch (state)
        {
            case State.Idle:
                activeCoroutine = StartCoroutine(IdleCycle());
                break;                
            case State.Evacuating:
                activeCoroutine = StartCoroutine(EvacuateCycle());
                break;
            case State.Blocked:
                activeCoroutine = StartCoroutine(BlockedCycle());
                break;
            case State.Escaped:
                StopAllCoroutines();
                break;
            case State.Dead:
                agent.isStopped = true;
                StartCoroutine(Dead());
                break;
        }
    }

    public void SetTarget(Vector3 pos)
    {
        target = pos;
    }

    public void MoveToTarget()
    {
        agent.isStopped = false;
        agent.ResetPath();
        agent.SetDestination(target);
        agent.stoppingDistance = 0;
    }

    private IEnumerator BlockedCycle()
    {
        yield return null;
        ChangeState(State.Evacuating);
    }

    private IEnumerator EvacuateCycle()
    {
        float delay = Random.Range(1f, 4f);
        agent.ResetPath();
        agent.velocity = Vector3.zero;

        yield return new WaitForSeconds(delay);

        MoveToTarget();
    }

    private IEnumerator IdleCycle()
    {        
        float waitTime = Random.Range(1f, 5f);
        yield return new WaitForSeconds(waitTime);
                
        if (Random.value > 0.5f)
        {
            Vector3 dest = GetRandomWanderPosition();
            agent.SetDestination(dest);
                        
            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            {
                yield return new WaitForSeconds(0.2f);
            }
        }
        
        if (currentState == State.Idle)
        {
            activeCoroutine = StartCoroutine(IdleCycle());
        }
    }

    private Vector3 GetRandomWanderPosition()
    {
        float RandomX = Random.Range(DataManager.Instance.GetPreData<List<Vector2>>(PreSetData.Size)[0].x, DataManager.Instance.GetPreData<List<Vector2>>(PreSetData.Size)[1].x);
        float RandomZ = Random.Range(DataManager.Instance.GetPreData<List<Vector2>>(PreSetData.Size)[0].y, DataManager.Instance.GetPreData<List<Vector2>>(PreSetData.Size)[1].y);

        Vector3 randomDirection = new Vector3(RandomX, 0, RandomZ);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, 5f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return transform.position;
    }

    private IEnumerator Dead()
    {
        agent.enabled = false;

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;

        rb.AddForce(-transform.forward * 2.0f, ForceMode.Impulse);
        rb.AddTorque(transform.right * 5.0f, ForceMode.Impulse);

        yield return new WaitForSeconds(1.5f);

        rb.isKinematic = true;
        obstacle.enabled = true;
        obstacle.carving = true;
    }
}
