using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Agent : MonoBehaviour
{
    private AgentFSM fsm;
    private NavMeshAgent agent;

    public event Action onDead;

    private Vector3 incidentPos;

    [SerializeField] public bool arrived = false;

    private float panicIncreaseRate;
    [SerializeField] private float _confusionRate;
    private float _injuryLevel;
    private float baseSpeed = 1.3f;
    private float finalSpeed;
    private float detectRange;
    private float baseRange;
    private float crowdDetectRange;
    private float decayRate;
    private float maxDamagePerSec;
    [SerializeField] private bool isIncidentOccurred = false;
    [SerializeField] private bool isDetected = false;
    public float growthIntensity = 2.0f;

    private float stuckTimer = 0f;
    private float stuckThreshold = 1.5f;
    private float evacuateThreshold;

    private MeshRenderer meshRenderer;
    private Color originColor;
    private Coroutine activeCoroutine;

    [SerializeField] private Color idleColor;
    [SerializeField] private Color evacuatingColor;
    [SerializeField] private Color blockedColor;
    [SerializeField] private Color deadColor;

    public float InjuryLevel
    {
        get { return _injuryLevel; }
        set
        {
            if (value < 0) _injuryLevel = 0;
            else if (value >= 1f) _injuryLevel = 1f;
            else
                _injuryLevel = value;
        }
    }

    public float ConfusionRate
    {
        get { return _confusionRate; }
        set
        {
            if (value <0) _confusionRate = 0;
            else if (value >= 1f) _confusionRate = 1f;
            else
                _confusionRate = value;
        }
    }    

    [SerializeField] private List<Vector3> exitInfo;
    [SerializeField] private List<Vector3> preExitInfo;
    private Dictionary<int, bool> exitOpenInfo = new Dictionary<int, bool>();

    private int exitIdx = 100;

    public void Initialize(List<Vector3> pos, List<Vector3> pos2)
    {
        exitInfo = pos.ToList();
        preExitInfo = pos2.ToList();
        CreateExitOpenInfo();

        fsm.ChangeState(AgentFSM.State.Idle);
    }

    private void Awake()
    {
        fsm = GetComponent<AgentFSM>();
        agent = GetComponent<NavMeshAgent>();

        meshRenderer = GetComponent<MeshRenderer>();
        originColor = meshRenderer.material.color;

        ConfusionRate = UnityEngine.Random.Range(0, 0.1f);
        panicIncreaseRate = UnityEngine.Random.Range(0.02f, 0.06f);
        InjuryLevel = UnityEngine.Random.Range(0, 0.1f);
        evacuateThreshold = UnityEngine.Random.Range(0.3f, 0.4f);
        detectRange = UnityEngine.Random.Range(1.5f, 2f);
        baseRange = detectRange;
        crowdDetectRange = UnityEngine.Random.Range(3f, 5f);
        decayRate = UnityEngine.Random.Range(0.5f, 0.8f);
        maxDamagePerSec = UnityEngine.Random.Range(0.05f, 0.1f);
        agent.avoidancePriority = UnityEngine.Random.Range(0, 100);
    }

    private void OnEnable()
    {
        IncidentManager.OnIncidentOccurred += OnIncidentOccurred;
        SimulationButtons.onColorNone += NoneColor;
        SimulationButtons.onColorState += StartStateColorCoroutine;
        SimulationButtons.onColorInjury += StartInjuryColorCoroutine;
        SimulationButtons.onColorConfusion += StartConfusionColorCoroutine;

        StartCoroutine(PanicUpdateRoutine());
    }

    private void OnDisable()
    {
        IncidentManager.OnIncidentOccurred -= OnIncidentOccurred;
        SimulationButtons.onColorNone -= NoneColor;
        SimulationButtons.onColorState -= StartStateColorCoroutine;
        SimulationButtons.onColorInjury -= StartInjuryColorCoroutine;
        SimulationButtons.onColorConfusion -= StartConfusionColorCoroutine;

        StopAllCoroutines();
    }

    public void StopAllCoroutine()
    {
        StopAllCoroutines();
    }

    void Update()
    {
        if (fsm.currentState == AgentFSM.State.Dead) return;

        CheckDeathCondition();


        if (isIncidentOccurred && fsm.currentState == AgentFSM.State.Idle)
        {
            CheckEvacuation();
        }        

        if (fsm.currentState == AgentFSM.State.Evacuating)
        {
            CheckArrived();
            CheckDetect();            
        }

        CalculateSpeed();
    }

    public void UpdateLimitedExponentialRange(float ratio)
    {
        if (fsm.currentState != AgentFSM.State.Idle) return;

        float progress = 1f - ratio;

        float expValue = (Mathf.Exp(growthIntensity * progress) - 1f) / (Mathf.Exp(growthIntensity) - 1f);

        detectRange = Mathf.Lerp(baseRange, baseRange * 2f, expValue);
    }

    public void Escaped()
    {
        fsm.ChangeState(AgentFSM.State.Escaped);
    }

    private void CreateExitOpenInfo()
    {
        for (int i = 0; i < preExitInfo.Count; i++)
        {
            exitOpenInfo[i] = DataManager.Instance.GetPreData<List<ExitInfo>>(PreSetData.Exits)[i].isOpened;
        }
    }
    
    private void CheckArrived()
    {
        if (arrived || fsm.currentState != AgentFSM.State.Evacuating) return;

        Vector3 targetPos = preExitInfo[exitIdx];

        Vector3 offset = targetPos - transform.position;
        offset.y = 0;

        if (offset.sqrMagnitude < 2.25f)
        {
            OnExitEntered();
        }
    }

    public void OnExitEntered()
    {
        arrived = true;

        if (exitOpenInfo[exitIdx])
        {
            agent.ResetPath();

            fsm.SetTarget(exitInfo[exitIdx]);
            fsm.ChangeState(AgentFSM.State.Blocked);

            agent.isStopped = false;
        }
        else
        {
            arrived = false;

            preExitInfo[exitIdx] = Vector3.zero;
            exitIdx = 100;

            if (isDetected)
                fsm.SetTarget(GetBestExit());

            else
                fsm.SetTarget(GetClosestExit());
            
            fsm.ChangeState(AgentFSM.State.Blocked);            
        }
    }

    private void CheckDetect()
    {
        if (isDetected) return;

        float distToIncident = Vector3.Distance(transform.position, incidentPos);

        if (distToIncident < detectRange)
        {
            ConfusionRate += 0.2f;
            isDetected = true;
            fsm.SetTarget(GetBestExit());
            fsm.ChangeState(AgentFSM.State.Blocked);
        }
    }

    private IEnumerator PanicUpdateRoutine()
    {        
        while (fsm.currentState != AgentFSM.State.Dead)
        {
            HandleSocialPanic();
            
            yield return new WaitForSeconds(0.4f);
        }
    }

    private void HandleSocialPanic()
    {
        Collider[] nearbyAgents = Physics.OverlapSphere(transform.position, crowdDetectRange);
        int fleeingCount = 0;
        AgentFSM otherFSM;

        foreach (var col in nearbyAgents)
        {            
            if (col.gameObject != gameObject)
            {
                otherFSM = col.GetComponent<AgentFSM>();                
                if (otherFSM != null && (otherFSM.currentState == AgentFSM.State.Evacuating || otherFSM.currentState == AgentFSM.State.Dead))
                {
                    fleeingCount++;
                }
            }
        }
        
        if (fleeingCount > 0)
        {
            ConfusionRate += fleeingCount * panicIncreaseRate * Time.deltaTime;
        }
    }

    private void CheckEvacuation()
    {
        float distToIncident = Vector3.Distance(transform.position, incidentPos);

        if (distToIncident < detectRange)
        {
            ConfusionRate += 0.2f;
            isDetected = true;
            fsm.SetTarget(GetBestExit());
            fsm.ChangeState(AgentFSM.State.Evacuating);
        }
        else if (ConfusionRate >= evacuateThreshold)
        {
            fsm.SetTarget(GetClosestExit());
            fsm.ChangeState(AgentFSM.State.Evacuating);
        }
    }

    private void CheckDeathCondition()
    {
        if (InjuryLevel >= 1.0f && fsm.currentState != AgentFSM.State.Dead)
        {
            fsm.ChangeState(AgentFSM.State.Dead);
        }
    }

    private void CalculateSpeed()
    {
        if (fsm.currentState == AgentFSM.State.Dead) return;

        float calculated = baseSpeed * (1 - InjuryLevel);

        if (fsm.currentState == AgentFSM.State.Evacuating)
        {            
            float panicBonus = 1.0f + ConfusionRate;
            calculated *= panicBonus;
        }        

        finalSpeed = calculated;

        if (agent != null && agent.enabled)
            agent.speed = finalSpeed;
    }

    private Vector3 GetClosestExit()
    {
        float bestDistance = float.MaxValue;
        float distance;
        int idx = 0;

        for (int i = 0; i < preExitInfo.Count; i++)
        {
            if (exitIdx == i && preExitInfo.Count != 1)
                continue;
            if (preExitInfo[i] == Vector3.zero) continue;

            distance = Vector3.SqrMagnitude(transform.position - preExitInfo[i]);
            if (distance < bestDistance)
                idx = i;
        }

        exitIdx = idx;
        
        return preExitInfo[idx];
    }

    public Vector3 GetBestExit()
    {
        Vector3 bestExit = Vector3.zero;
        float bestScore = float.MaxValue;

        for (int i = 0; i < preExitInfo.Count; i++)
        {
            if (i == exitIdx && preExitInfo.Count != 1)
                continue;

            if (preExitInfo[i] == Vector3.zero) continue;

            float distFromMe = Vector3.Distance(transform.position, preExitInfo[i]);
            float distFromIncident = Vector3.Distance(incidentPos, preExitInfo[i]);

            if (distFromIncident < 1.5f) continue;

            float score = distFromMe - (distFromIncident * ConfusionRate);

            if (score < bestScore)
            {
                bestScore = score;
                bestExit = preExitInfo[i];
                exitIdx = i;
            }
        }

        if (bestExit == Vector3.zero)
        {
            exitIdx = 0;
            return GetClosestExit();
        }

        return bestExit;
    }

    private void StartInjuryCoroutine()
    {
        StartCoroutine(InjuryRoutine());
    }

    private IEnumerator InjuryRoutine()
    {
        while (InjuryLevel < 1.0f)
        {
            float dist = Vector3.Distance(transform.position, incidentPos);
                        
            float exponentialFactor = Mathf.Exp(-decayRate * dist);
                        
            float frameDamage = exponentialFactor * maxDamagePerSec * Time.deltaTime;
            InjuryLevel += frameDamage;

            if (InjuryLevel >= 1.0f)
            {
                InjuryLevel = 1.0f;
                fsm.ChangeState(AgentFSM.State.Dead);
                onDead.Invoke();
                yield break;
            }

            yield return null;
        }
    }

    private void CloseExplosion()
    {
        float dist = Vector3.Distance(transform.position, incidentPos);

        if (dist < 1f)
        {
            StopAllCoroutines();
            InjuryLevel = 1.0f;
            fsm.ChangeState(AgentFSM.State.Dead);
            onDead.Invoke();
        }            
    }

    public void OnIncidentOccurred(Vector3 pos)
    {
        incidentPos = pos;
        isIncidentOccurred = true;
        StartInjuryCoroutine();
        CloseExplosion();
    }

    #region Events

    private void NoneColor()
    {
        if (activeCoroutine == null) return;

        StopCoroutine(activeCoroutine);
        activeCoroutine = null;
        meshRenderer.material.color = originColor;
    }

    private void StartStateColorCoroutine()
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }

        activeCoroutine = StartCoroutine(StateColorCoroutine());
    }

    private void StartInjuryColorCoroutine()
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }

        activeCoroutine = StartCoroutine(InjuryColorCoroutine());
    }

    private void StartConfusionColorCoroutine()
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }

        activeCoroutine = StartCoroutine(ConfusionColorCoroutine());
    }

    private IEnumerator StateColorCoroutine()
    {
        AgentFSM.State state = AgentFSM.State.None;
        Color color = originColor;

        while (true)
        {

            state = fsm.currentState;

            switch ((int)state)
            {
                case 0:
                    break;
                case 1:
                    color = idleColor;
                    break;
                case 2:
                    color = evacuatingColor;
                    break;
                case 3:
                    color = blockedColor;
                    break;
                case 4:
                    break;
                case 5:
                    color = deadColor;
                    break;
            }

            meshRenderer.material.color = color;

            yield return new WaitForSecondsRealtime(0.2f);
        }
    }

    private IEnumerator InjuryColorCoroutine()
    {
        Color color;

        while (true)
        {
            color = GetInjuryColor(InjuryLevel);
            meshRenderer.material.color = color;

            if (color == Color.black) break;

            yield return new WaitForSecondsRealtime(0.2f);
        }        
    }

    private IEnumerator ConfusionColorCoroutine()
    {
        Color color;

        while (true)
        {
            color = GetConfusionColor(ConfusionRate);
            meshRenderer.material.color = color;            

            yield return new WaitForSecondsRealtime(0.2f);
        }
    }

    private Color GetInjuryColor(float val)
    {
        if (val >= 1.0f) return Color.black;
                
        if (val < 0.5f)
            return Color.Lerp(Color.white, Color.yellow, val * 2f);
        else
            return Color.Lerp(Color.yellow, Color.red, (val - 0.5f) * 2f);
    }

    private Color GetConfusionColor(float val)
    {
        if (val < 0.3f)
            return Color.Lerp(Color.white, Color.cyan, val / 0.3f);
        else
            return Color.Lerp(Color.cyan, new Color(0.5f, 0f, 1f), (val - 0.3f) / 0.7f);
    }

    #endregion
}
