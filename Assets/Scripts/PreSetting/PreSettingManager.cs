using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PreSettingManager : MonoBehaviour
{
    private List<Vector2> size = new List<Vector2>();

    private Vector2 max = new Vector2(5, 5);
    private Vector2 min = new Vector2(-5, -5);

    [SerializeField] private Button agentSetButton;
    [SerializeField] private Button exitSettingButton;
    [SerializeField] private Button startButton;
    [SerializeField] private Button dataButton;
    [SerializeField] private Button returnPresettingButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TMP_Dropdown hazardType;
    [SerializeField] private TMP_InputField agentNum;

    private PreSettingUIManager preSettingUIManager;
    private ExitSettingMode exitSetting;
    private ScreenController screenController;
    private string input = string.Empty;

    private List<SimulationResult> datas = new List<SimulationResult>();

    private void OnEnable()
    {
        agentSetButton.onClick.AddListener(AgentSetButtonClicked);
        hazardType.onValueChanged.AddListener(HazardTypeSelected);
        exitSettingButton.onClick.AddListener(ExitSettingButtonClicked);
        startButton.onClick.AddListener(StartButtonClicked);
        dataButton.onClick.AddListener(DataButtonClicked);
        returnPresettingButton.onClick.AddListener(ReturnPresettingButtonClicked);
        quitButton.onClick.AddListener(Quit);
    }

    private void OnDisable()
    {
        agentSetButton.onClick.RemoveListener(AgentSetButtonClicked);
        hazardType.onValueChanged.RemoveListener(HazardTypeSelected);
        exitSettingButton.onClick.RemoveListener(ExitSettingButtonClicked);
        startButton.onClick.RemoveListener(StartButtonClicked);
        dataButton.onClick.RemoveListener(DataButtonClicked);
        returnPresettingButton.onClick.RemoveListener(ReturnPresettingButtonClicked);
        quitButton.onClick.RemoveListener(Quit);
    }

    private void Quit()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void ReturnPresettingButtonClicked()
    {
        preSettingUIManager.DataToSettingWindowRoutine();
    }

    private void DataButtonClicked()
    {
        preSettingUIManager.SettingToDataWindowRoutine();
        DataSetting();
    }

    public void DataDeleteButtonClicked(string name)
    {
        DataManager.Instance.DeleteReport(name);        
        DataSetting();
    }

    public void DataLoadButtonClicked(LoadedPreData data)
    {
        DataManager.Instance.SetPreData(PreSetData.Exits, data.exitInfo);
        DataManager.Instance.SetPreData(PreSetData.HazardType, data.hazardType);
        DataManager.Instance.SetPreData(PreSetData.AgentNum, data.agentNum);

        preSettingUIManager.SetText(texts.HazardType, data.hazardType);
        preSettingUIManager.SetText(texts.AgentNum, data.agentNum.ToString());
        preSettingUIManager.SetText(texts.OpenedExitNum, $"Opened Exits: {data.opened}");
        preSettingUIManager.SetText(texts.ClosedExitNum, $"Closed Exits: {data.closed}");

        exitSetting.LoadExit();
    }

    private void DataSetting()
    {
        datas.Clear();
        preSettingUIManager.DestroyAllDataPanels();
        SimulationResult result;
        datas = DataManager.Instance.LoadAllReports();

        for (int i = 0; i < datas.Count; i++)
        {
            result = datas[i];
            preSettingUIManager.DataSetting(result, this);
        }
    }

    private void HazardTypeSelected(int idx)
    {
        input = hazardType.options[idx].text;
        preSettingUIManager.SetText(texts.HazardType, input);
        DataManager.Instance.SetPreData(PreSetData.HazardType, input);
    }

    private void StartButtonClicked()
    {
        if (CheckStart())
        {
            SceneChangeManager.Instance.StartSceneChangeRoutine(1f, 1f, "SimulationScene", true);
        }

        else
            preSettingUIManager.SetNotice("설정 값을 다시 확인해주세요!");
    }

    private void AgentSetButtonClicked()
    {
        input = agentNum.text;
        preSettingUIManager.AgentInput();
        if (ValidateInput(input, out int cnt))
        {
            preSettingUIManager.SetText(texts.AgentNum, input);
            DataManager.Instance.SetPreData(PreSetData.AgentNum, cnt);
        }
        else
        {
            preSettingUIManager.SetNotice("올바른 값을 입력해주세요!");
        }
    }

    private void ExitSettingButtonClicked()
    {
        preSettingUIManager.ExitSettingClicked();
        exitSetting.gameObject.SetActive(true);
    }

    public void ExitSettingFinished()
    {
        preSettingUIManager.ExitSettingFinished();
        exitSetting.gameObject.SetActive(false);

        int close;
        int open = CalculateExits(out close);

        preSettingUIManager.SetText(texts.OpenedExitNum, $"Opened Exits: {open}");
        preSettingUIManager.SetText(texts.ClosedExitNum, $"Closed Exits: {close}");
    }

    private bool CheckStart()
    {
        int agents = DataManager.Instance.GetPreData<int>(PreSetData.AgentNum);
        string type = DataManager.Instance.GetPreData<string>(PreSetData.HazardType);
        List<ExitInfo> infos = DataManager.Instance.GetPreData<List<ExitInfo>>(PreSetData.Exits);
        int close;
        int open = 0;
        if (infos != null)
            open = CalculateExits(out close);

        if (agents != 0 && type != null && infos != null && open != 0)
            return true;

        return false;
    }

    private int CalculateExits(out int cl)
    {
        int open = 0;
        int close = 0;

        foreach (ExitInfo info in DataManager.Instance.GetPreData<List<ExitInfo>>(PreSetData.Exits))
        {
            if (info.isOpened)
                open++;
            else
                close++;
        }

        cl = close;

        return open;
    }

    private bool ValidateInput(string input, out int count)
    {
        if (int.TryParse(input, out count))
        {
            if (count > 0)
                return true;

            count = 0;
            return false;
        }
        count = 0;
        return false;
    }

    void Start()
    {
        screenController = FindAnyObjectByType<ScreenController>();
        preSettingUIManager = FindAnyObjectByType<PreSettingUIManager>();
        exitSetting = FindAnyObjectByType<ExitSettingMode>(FindObjectsInactive.Include);        
        size.Add(max);
        size.Add(min);
        DataManager.Instance.SetPreData(PreSetData.Size, size);

        exitSetting.CreateWall();

        screenController.StartFadeRoutine(1, 0, 1f);
    }
}
