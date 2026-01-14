using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class AgentManager : MonoBehaviour
{
    private SimulationUI simulationUI;

    private int agentNum;
    private int dead = 0;
    private int escaped = 0;
    private float avgConfusionRate = 0f;
    private float avgInjuryRate = 0f;
    private List<Vector2> size;

    public static event Action onCompleted;
    public static event Action onLoaded;

    private AgentPool agentPool;

    private List<Agent> agents = new List<Agent>();

    private void Awake()
    {
        simulationUI = FindAnyObjectByType<SimulationUI>();

        agentPool = FindAnyObjectByType<AgentPool>();

        agentNum = DataManager.Instance.GetPreData<int>(PreSetData.AgentNum);
        size = DataManager.Instance.GetPreData<List<Vector2>>(PreSetData.Size);
    }

    private void OnEnable()
    {
        SimulationButtons.onQuit += OnSimulationStopped;
        onCompleted += OnSimulationStopped;
    }

    private void OnDisable()
    {
        SimulationButtons.onQuit -= OnSimulationStopped;
        onCompleted -= OnSimulationStopped;
    }

    private void OnSimulationStopped()
    {
        StopAllCoroutines();

        DataManager.Instance.SetSimulData(SimulationData.Dead, dead);
        DataManager.Instance.SetSimulData(SimulationData.Escaped, escaped);
        DataManager.Instance.SetSimulData(SimulationData.Confusion, avgConfusionRate);
        DataManager.Instance.SetSimulData(SimulationData.Injury, avgInjuryRate);

        simulationUI.SetResultNumbers(DataManager.Instance.GetSimulData<int>(SimulationData.Dead), DataManager.Instance.GetSimulData<int>(SimulationData.Escaped),
                                                DataManager.Instance.GetSimulData<float>(SimulationData.Injury), DataManager.Instance.GetSimulData<float>(SimulationData.Confusion));

        for (int i = 0; i < agents.Count; i++)
        {
            agents[i].StopAllCoroutine();
            agentPool.Return(agents[i]);
        }        
    }

    private void CheckCompleted()
    {
        int sum = dead + escaped;
        print(sum);
        if (sum == agentNum)
            onCompleted.Invoke();
    }

    public void SpawnAgent(List<Vector3> pos, List<Vector3> pos2)
    {
        StartCoroutine(SpawnRoutine(pos, pos2));
    }

    IEnumerator SpawnRoutine(List<Vector3> position, List<Vector3> position2)
    {
        agentPool.Create(agentNum);

        for (int i = 0; i < agentNum; i++)
        {
            Vector3 pos = GetSpawnPosition();
            Agent agent = agentPool.Get(pos);
            agent.onDead += Dead;
            agents.Add(agent);
            agent.Initialize(position, position2);
            yield return null;
        }
        StartConfusionRateRoutine();
        StartInjuryRateRoutine();

        onLoaded.Invoke();
    }

    private Vector3 GetSpawnPosition()
    {
        float checkRadius = 0.2f;

        while(true)
        {
            float x = UnityEngine.Random.Range(size[1].x, size[0].x);
            float z = UnityEngine.Random.Range(size[1].y, size[0].y);
            Vector3 randomPos = new Vector3(x, 0.5f, z);
            NavMeshHit hit;

            if (NavMesh.SamplePosition(randomPos, out hit, 3.0f, NavMesh.AllAreas))
            {
                Vector3 candidatePos = hit.position;

                int layerMask = LayerMask.GetMask("Agent") | LayerMask.GetMask("Wall");

                if (!Physics.CheckSphere(candidatePos, checkRadius, layerMask))
                {
                    return candidatePos;
                }                
            }
        }        
    }

    public void Return(Agent agent)
    {
        escaped++;
        UpdateAgentDetectRange();
        simulationUI.Escaped();
        simulationUI.UpdateAlive();
        CheckCompleted();
        agent.onDead -= Dead;
        agentPool.Return(agent);
    }

    private void UpdateAgentDetectRange()
    {
        float ratio = (agentNum - (dead + escaped)) / agentNum;
        for (int i = 0; i < agents.Count; i++)
        {
            if (agents[i].enabled)
                agents[i].UpdateLimitedExponentialRange(ratio);
        }
    }

    private void Dead()
    {
        dead++;
        simulationUI.Dead();
        simulationUI.UpdateAlive();
        CheckCompleted();
    }

    private void StartConfusionRateRoutine()
    {
        StartCoroutine(ConfusionRateRoutine());
    }

    private void StartInjuryRateRoutine()
    {
        StartCoroutine(InjuryRateRoutine());
    }

    private IEnumerator ConfusionRateRoutine()
    {
        float sum = 0;
        float avg;
        while (true)
        {
            for (int i = 0; i < agentNum; i++)
            {
                sum += agents[i].ConfusionRate;
            }
            avg = sum / agentNum * 100;
            avgConfusionRate = (float)Math.Round(avg, 1);
            simulationUI.UpdateConfusionRate(avg);

            sum = 0;
            yield return new WaitForSecondsRealtime(0.2f);
        }
    }

    private IEnumerator InjuryRateRoutine()
    {
        float sum = 0;
        float avg;
        while (true)
        {
            for (int i = 0; i < agentNum; i++)
            {
                sum += agents[i].InjuryLevel;
            }
            avg = sum / agentNum * 100;
            avgInjuryRate = (float)Math.Round(avg, 1);
            simulationUI.UpdateInjuryRate(avg);

            sum = 0;
            yield return new WaitForSecondsRealtime(0.2f);
        }
    }
}
