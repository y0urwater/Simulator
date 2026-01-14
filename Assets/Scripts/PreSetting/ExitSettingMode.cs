using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI.Table;

public struct ExitInfo
{
    public int idx;
    public bool isOpened;
    public float[] rotation;

    [JsonIgnore]
    public Quaternion Rotation
    {
        get
        {
            if (rotation != null && rotation.Length >= 3)
            {
                return Quaternion.Euler(rotation[0], rotation[1], rotation[2]);
            }
            return Quaternion.identity;
        }
    }

    public ExitInfo(int idx, bool isOpened, Quaternion rot)
    {
        this.idx = idx;
        this.isOpened = isOpened;
        rotation = new float[] { rot.eulerAngles.x, rot.eulerAngles.y, rot.eulerAngles.z };
    }
}

public class ExitSettingMode : MonoBehaviour
{
    private PreSettingManager preSettingManager;

    private Dictionary<int, WallObject> wallDictionary = new Dictionary<int, WallObject>();

    [SerializeField] private List<ExitInfo> exitInfo = new List<ExitInfo>(); // DataManager·Î Àü´Þ

    private List<bool> exitState = new List<bool>();

    [SerializeField] private List<GameObject> exitObjs = new List<GameObject>();

    private List<Vector2> size;

    private int idx = 0;

    private int horizontal;
    private int vertical;

    private WallPool wallPool;

    [SerializeField] private GameObject exitPrefab;

    [SerializeField] private ExitUI exitUI;
    [SerializeField] private Button save;


    public void LoadExit()
    {
        int cnt;

        if (exitObjs.Count != 0)
        {
            cnt = exitObjs.Count;

            for (int i = 0; i < cnt; i++)
            {
                DeleteExit(0);
            }
        }
        List<ExitInfo> list = DataManager.Instance.GetPreData<List<ExitInfo>>(PreSetData.Exits);

        for (int i = 0; i < list.Count; i++)
        {
            OnWallClicked("", list[i].idx);
        }

        for (int i = 0; i < list.Count; i++)
        {
            UpdateOpen(i, list[i].isOpened);            
        }
        exitUI.UpdateExitControl(exitObjs.Count, exitState);
    }


    private int CalculateSize()
    {
        Vector2 max = size[0];
        Vector2 min = size[1];

        horizontal = (int)(max.x - min.x);
        vertical = (int)(max.y - min.y);

        return (((horizontal * 10) + (vertical * 10)) * 2);
    }

    public void CreateWall()
    {
        wallPool = FindAnyObjectByType<WallPool>();
        size = DataManager.Instance.GetPreData<List<Vector2>>(PreSetData.Size);
        wallPool.Create(CalculateSize());

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
            WallObject wallObj = wallPool.Get();
            wallObj.transform.position = startPos + ((direction * 0.1f) * i) + new Vector3(0, 0.5f, 0);
            wallObj.transform.rotation = Quaternion.Euler(rotation);
            wallObj.SetIdx(idx + i);
            wallObj.onWallClicked += OnWallClicked;
            wallDictionary[idx + i] = wallObj;

            if (i <= 3 || i >= num - 3)
                wallObj.SetLayer("ExitFalse");
            else
                wallObj.SetLayer("ExitTrue");
        }
        idx += num;
    }

    private void OnWallClicked(string name, int idx)
    {
        if (name == "ExitFalse")
            return;
        else
        {
            CreateNewExit(idx);
            exitState.Clear();
            foreach (ExitInfo info in exitInfo)
            {
                exitState.Add(info.isOpened);
            }
            exitUI.UpdateExitControl(exitObjs.Count, exitState);
            exitUI.UpdateExitInfo(exitObjs.Count, exitState);
        }
    }

    public void UpdateOpen(int idx, bool isOpened)
    {
        ExitInfo tempExit = exitInfo[idx];
        tempExit.isOpened = isOpened;
        exitInfo[idx] = tempExit;

        SetExit(idx, isOpened);

        exitState.Clear();
        foreach (ExitInfo info in exitInfo)
        {
            exitState.Add(info.isOpened);
        }

        exitUI.UpdateExitInfo(exitObjs.Count, exitState);
    }

    public void DeleteExit(int idx)
    {
        int index = exitInfo[idx].idx;
        Quaternion quaternion = exitInfo[idx].Rotation;
        Vector3 position = exitObjs[idx].transform.position - new Vector3(0, 0.5f, 0);
        Quaternion newdir = Quaternion.Euler(0, -90f, 0) * quaternion;
        Vector3 direction = newdir * Vector3.forward;

        WallObject wall;

        for (int i = 0; i < 7; i++)
        {
            wall = wallPool.Get();
            wall.SetIdx(index - 3 + i);
            wallDictionary[index - 3 + i] = wall;
            wall.transform.position = position - (direction * 0.1f) * (3 - i);
            wall.transform.rotation = quaternion;
        }

        Destroy(exitObjs[idx]);
        exitObjs.RemoveAt(idx);
        exitInfo.RemoveAt(idx);

        SetLayer();

        exitState.Clear();
        foreach (ExitInfo info in exitInfo)
        {
            exitState.Add(info.isOpened);
        }
        exitUI.UpdateExitControl(exitObjs.Count, exitState);
        exitUI.UpdateExitInfo(exitObjs.Count, exitState);
    }

    private void SetLayer()
    {
        int index;

        for (int i = 0; i < wallDictionary.Count; i++)
        {
            wallDictionary[i].SetLayer("ExitTrue");
            wallDictionary[i].ResetColor();
        }

        for (int i = 0; i < exitObjs.Count; i++)
        {
            index = exitInfo[i].idx;
            for (int j = 0; j < 3; j++)
            {
                wallDictionary[index + (4 + j)].SetLayer("ExitFalse");
                wallDictionary[index - (4 + j)].SetLayer("ExitFalse");
            }
        }
    }

    private void SetExit(int idx, bool isOpened)
    {
        GameObject exit = exitObjs[idx];
        Transform doorL = exit.transform.Find("DoorTheaterL");
        Transform doorR = exit.transform.Find("DoorTheaterR");

        if (isOpened)
        {
            doorL.localRotation = Quaternion.Euler(0, 90f, 0);
            doorR.localRotation = Quaternion.Euler(0, -90f, 0);
        }
        else
        {
            doorL.localRotation = Quaternion.Euler(0, 0, 0);
            doorR.localRotation = Quaternion.Euler(0, 0, 0);
        }
    }

    private void CreateNewExit(int idx)
    {
        GameObject exit = Instantiate(exitPrefab, wallDictionary[idx].transform.position + new Vector3(0, 0.5f, 0), wallDictionary[idx].transform.rotation);
        ExitInfo info = new ExitInfo(idx, false, exit.transform.rotation);
        exitInfo.Add(info);
        exitObjs.Add(exit);

        for (int i = 0; i < 3; i++)
        {
            wallDictionary[idx + (4 + i)].SetLayer("ExitFalse");
            wallDictionary[idx - (4 + i)].SetLayer("ExitFalse");
        }

        for (int i = 0; i < 7; i++)
        {
            wallPool.Return(wallDictionary[idx - 3 + i]);
        }
    }

    private void OnSaveClicked()
    {
        SaveData();
        preSettingManager.ExitSettingFinished();
    }

    private void SaveData()
    {
        List<ExitInfo> existingList = DataManager.Instance.GetPreData<List<ExitInfo>>(PreSetData.Exits);

        if (existingList == null)
        {
            List<ExitInfo> newList = exitInfo.ToList();
            DataManager.Instance.SetPreData(PreSetData.Exits, newList);
        }
        else
        {
            existingList.Clear();
            existingList.AddRange(exitInfo);
        }
    }

    private void OnEnable()
    {
        save.onClick.AddListener(OnSaveClicked);
    }

    private void OnDisable()
    {
        save.onClick.RemoveListener(OnSaveClicked);
    }

    private void Awake()
    {
        preSettingManager = FindAnyObjectByType<PreSettingManager>();
    }
}
