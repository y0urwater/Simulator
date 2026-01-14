using System.Collections.Generic;
using System.Collections;
using System.Drawing;
using UnityEngine;
using UnityEngine.AI;

public class SimultationManager : MonoBehaviour
{
    private int idx;
    private int horizontal;
    private int vertical;
    private int agentNum;
    private int exitIdx;
    private int minute = 0;
    private int second = 0;
    private bool exitOpen;
    private Quaternion exitRotation;
    private List<ExitInfo> exitinfos;

    public GameObject exitPrefab;
    public Transform exitParent;
    public Transform wallParent;
    private Dictionary<int, GameObject> wallDictionary = new Dictionary<int, GameObject>();

    public GameObject wallPrefab;

    private List<Vector2> size;
    [SerializeField] private List<Vector3> exitPosition;
    [SerializeField] private List<Vector3> preExitPosition;

    private AgentManager agentManager;
    private CameraController cameraController;
    private SimulationUI simulationUI;
    private ScreenController screenController;

    private void Awake()
    {
        size = DataManager.Instance.GetPreData<List<Vector2>>(PreSetData.Size);

        horizontal = (int)(size[0].x - size[1].x);
        vertical = (int)(size[0].y - size[1].y);

        idx = 0;
        agentNum = DataManager.Instance.GetPreData<int>(PreSetData.AgentNum);
        exitinfos = DataManager.Instance.GetPreData<List<ExitInfo>>(PreSetData.Exits);

        wallDictionary.Clear();

        agentManager = FindAnyObjectByType<AgentManager>();
        simulationUI = FindAnyObjectByType<SimulationUI>();
        cameraController = FindAnyObjectByType<CameraController>();
        screenController = cameraController.GetComponentInChildren<ScreenController>();
    }

    private void Start()
    {
        CreateWall();
        CreateExitPositionList();
        agentManager.SpawnAgent(exitPosition, preExitPosition);
        simulationUI.UpdateTime(minute, second);
    }

    private void OnEnable()
    {
        IncidentManager.OnIncidentOccurredNormal += StartTimer;
        SimulationButtons.onFast += OnFastClicked;
        SimulationButtons.onNormal += OnNormalClicked;
        SimulationButtons.onSlow += OnSlowClicked;
        SimulationButtons.onPause += OnPauseClicked;
        SimulationButtons.onQuit += OnSimulationStopped;
        AgentManager.onCompleted += OnSimulationStopped;
        AgentManager.onLoaded += OnLoaded;
        SimulationButtons.onExit += OnQuitButtonClicked;
        SimulationButtons.onReturn += OnReturnButtonClicked;
        SimulationButtons.onSave += OnSaveButtonClicked;
    }

    private void OnDisable()
    {
        IncidentManager.OnIncidentOccurredNormal -= StartTimer;
        SimulationButtons.onFast -= OnFastClicked;
        SimulationButtons.onNormal -= OnNormalClicked;
        SimulationButtons.onSlow -= OnSlowClicked;
        SimulationButtons.onPause -= OnPauseClicked;
        SimulationButtons.onQuit -= OnSimulationStopped;
        AgentManager.onCompleted -= OnSimulationStopped;
        AgentManager.onLoaded -= OnLoaded;
        SimulationButtons.onExit -= OnQuitButtonClicked;
        SimulationButtons.onReturn -= OnReturnButtonClicked;
        SimulationButtons.onSave -= OnSaveButtonClicked;
    }    

    private void OnLoaded()
    {
        screenController.SetLoading(true);
    }

    private void OnSaveButtonClicked()
    {
        DataManager.Instance.SaveReport();
    }

    private void OnReturnButtonClicked()
    {
        DataManager.Instance.ClearAllData();
        SceneChangeManager.Instance.StartSceneChangeRoutine(1f, 1f, "PreSettingScene");
        cameraController.enabled = true;
    }

    private void OnQuitButtonClicked()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void OnSimulationStopped()
    {
        SetTimeScale(1f);
        DataManager.Instance.SetSimulData(SimulationData.Minute, minute);
        DataManager.Instance.SetSimulData(SimulationData.Second, second);
        StopAllCoroutines();
        simulationUI.SetResultTime(DataManager.Instance.GetSimulData<int>(SimulationData.Minute), DataManager.Instance.GetSimulData<int>(SimulationData.Second));
        cameraController.enabled = false;
    }

    public void StartTimer()
    {
        StartCoroutine(TimerCoroutine());
    }

    private IEnumerator TimerCoroutine()
    {
        while (true)
        {
            while (second < 60)
            {
                simulationUI.UpdateTime(minute, second);
                yield return new WaitForSeconds(1f);
                second++;                
            }
            second = 0;
            minute++;            
        }
    }

    private void OnFastClicked()
    {
        SetTimeScale(2.0f);
    }

    private void OnNormalClicked()
    {
        SetTimeScale(1.0f);
    }

    private void OnSlowClicked()
    {
        SetTimeScale(0.5f);
    }

    private void OnPauseClicked()
    {
        SetTimeScale(0);
    }

    private void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    private void CreateExitPositionList()
    {
        for (int i = 0; i < exitinfos.Count; i++)
        {
            Vector3 originPosition = wallDictionary[exitinfos[i].idx].transform.position;
            Vector3 position = originPosition + wallDictionary[exitinfos[i].idx].transform.forward * 1f;
            Vector3 prePos = originPosition - wallDictionary[exitinfos[i].idx].transform.forward * 0.5f;

            position.y = 0;
            prePos.y = 0;

            exitPosition.Add(position);
            preExitPosition.Add(prePos);
        }
    }

    #region ÃÊ±â ¸Ê ¼¼ÆÃ
    private void CreateExits()
    {
        for (int i = 0; i < exitinfos.Count; i++)
        {
            exitIdx = exitinfos[i].idx;
            exitOpen = exitinfos[i].isOpened;
            exitRotation = exitinfos[i].Rotation;

            for (int j = 0; j < 7; j++)
            {
                wallDictionary[exitIdx - 3 + j].gameObject.SetActive(false);
            }

            SetExit(exitIdx, exitOpen, exitRotation);
        }
    }



    private void SetExit(int idx, bool isOpened, Quaternion rotation)
    {
        GameObject exit = Instantiate(exitPrefab, wallDictionary[idx].transform.position + new Vector3(0, 0.45f, 0), rotation, exitParent);
        NavMeshObstacle obs = exit.GetComponent<NavMeshObstacle>();
        Transform doorL = exit.transform.Find("DoorTheaterL");
        Transform doorR = exit.transform.Find("DoorTheaterR");

        if (isOpened)
        {
            
            obs.enabled = false;
            doorL.localRotation = Quaternion.Euler(0, 90f, 0);
            doorR.localRotation = Quaternion.Euler(0, -90f, 0);
        }
        else
        {
            doorL.localRotation = Quaternion.Euler(0, 0, 0);
            doorR.localRotation = Quaternion.Euler(0, 0, 0);
        }
    }

    private void CreateWall()
    {
        Vector3 pos = new Vector3(size[0].x, 0, size[0].y);
        Vector3 direction = new Vector3(-1, 0, 0);
        Vector3 rotDir = Vector3.zero;
        float angle = -90f;
        Quaternion rotation = Quaternion.Euler(0, angle, 0);

        for (int i = 0; i < 4; i++)
        {
            if (SetDirection(direction))
            {
                CreateSide(pos, horizontal * 10, direction, rotDir);
            }
            else
            {
                CreateSide(pos, vertical * 10, direction, rotDir);
            }

            pos += direction * horizontal;
            Vector3 newDir = rotation * direction;
            rotDir += new Vector3(0, angle, 0);
            direction = newDir;
        }

        CreateExits();
    }

    private bool SetDirection(Vector3 dir)
    {
        if (dir.x == 0)
            return false;
        else
            return true;
    }

    private void CreateSide(Vector3 startPos, int num, Vector3 direction, Vector3 rotation)
    {
        for (int i = 0; i < num; i++)
        {
            GameObject wallObj = Instantiate(wallPrefab, startPos + ((direction * 0.1f) * i) + new Vector3(0, 0.5f, 0), Quaternion.Euler(rotation), wallParent);
            wallDictionary[idx + i] = wallObj;
        }
        idx += num;
    }
    #endregion
}
