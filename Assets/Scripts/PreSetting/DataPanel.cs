using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public struct LoadedPreData
{
    public int agentNum;
    public string hazardType;
    public List<ExitInfo> exitInfo;
    public int opened;
    public int closed;

    public LoadedPreData(int agentNum, string hazardType, List<ExitInfo> exitInfo, int opened, int closed)
    {
        this.agentNum = agentNum;
        this.hazardType = hazardType;
        this.exitInfo = exitInfo;
        this.opened = opened;
        this.closed = closed;
    }
}

public class DataPanel : MonoBehaviour
{
    private PreSettingManager preSettingManager;

    [SerializeField] private Button load;
    [SerializeField] private Button delete;

    [SerializeField] private TMP_Text txtDate;
    [SerializeField] private TMP_Text txtTotalAgent;
    [SerializeField] private TMP_Text txtHazardType;
    [SerializeField] private TMP_Text txtExit;
    [SerializeField] private TMP_Text txtTime;
    [SerializeField] private TMP_Text txtDead;
    [SerializeField] private TMP_Text txtEscaped;
    [SerializeField] private TMP_Text txtInjury;
    [SerializeField] private TMP_Text txtConfusion;

    private string fileName;
    private int agent;
    private int opened = 0;
    private int closed = 0;
    private string hazard;

    private List<ExitInfo> exitInfos = new List<ExitInfo>();

    public void Initialize(SimulationResult res, PreSettingManager manager)
    {
        preSettingManager = manager;

        fileName = res.fileName;
        agent = res.agentNum;
        hazard = res.hazardType;

        txtDate.text = res.saveTime;
                
        txtTotalAgent.text = res.agentNum.ToString();
        txtHazardType.text = res.hazardType;
        txtTime.text = $"{res.totalMinutes}:{res.totalSeconds}";
        txtDead.text = res.deadCount.ToString();
        txtEscaped.text = res.escapedCount.ToString();
        txtInjury.text = res.avgInjury.ToString();
        txtConfusion.text = res.avgConfusion.ToString();

        exitInfos = res.exits;
        CountOpen();
        txtExit.text = $"{opened} / {closed}";        
    }

    private void OnEnable()
    {
        delete.onClick.AddListener(() => OnDeleteClicked(fileName));
        load.onClick.AddListener(OnLoadClicked);
    }

    private void OnDisable()
    {
        delete.onClick.RemoveListener(() => OnDeleteClicked(fileName));
        load.onClick.RemoveListener(OnLoadClicked);
    }

    private void CountOpen()
    {
        for (int i = 0; i < exitInfos.Count; i++)
        {
            if (exitInfos[i].isOpened)
                opened++;
            else
                closed++;            
        }
    }    

    private void OnDeleteClicked(string name)
    {
        preSettingManager.DataDeleteButtonClicked(name);
    }

    private void OnLoadClicked()
    {
        LoadedPreData data = new LoadedPreData(agent, hazard, exitInfos, opened, closed);
        preSettingManager.DataLoadButtonClicked(data);
    }
}
