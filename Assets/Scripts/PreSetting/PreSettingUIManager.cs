using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum texts
{
    AgentNum,
    HazardType,
    OpenedExitNum,
    ClosedExitNum
}

public class PreSettingUIManager : MonoBehaviour
{
    [SerializeField] private Transform controls;
    [SerializeField] private Transform controlTexts;
    [SerializeField] private MessageUI messageObject;
    [SerializeField] private GameObject preSettingWindow;
    [SerializeField] private GameObject exitPreSetting;
    [SerializeField] private GameObject dataWindow;
    [SerializeField] private Transform dataPanelParentTransform;
    [SerializeField] private DataPanel dataPanel;
    
    private TMP_InputField agentNum;

    [Header("Control Texts")]
    [SerializeField] private TMP_Text txtAgentNum;
    [SerializeField] private TMP_Text txtHazardType;
    [SerializeField] private TMP_Text txtOpenedExitNum;
    [SerializeField] private TMP_Text txtClosedExitNum;

    private Dictionary<texts, TMP_Text> textMap;

    private void Awake()
    {
        agentNum = controls.GetComponentInChildren<TMP_InputField>();

        InitializeMap();
    }

    private void InitializeMap()
    {
        textMap = new Dictionary<texts, TMP_Text>
        {
            {texts.AgentNum, txtAgentNum },
            {texts.HazardType, txtHazardType },
            {texts.OpenedExitNum, txtOpenedExitNum },
            {texts.ClosedExitNum, txtClosedExitNum }
        };
    }

    public void SetNotice(string notice)
    {
        messageObject.gameObject.SetActive(true);
        messageObject.SetText(notice);
    }

    public void SetText(texts key, string value)
    {
        if (textMap.ContainsKey(key))
            textMap[key].text = value;

        else
            Debug.Log($"[PreSettingUIManager] {key}가 존재하지 않습니다!");
    }

    public void AgentInput()
    {
        agentNum.text = string.Empty;
    }

    public void ExitSettingFinished()
    {
        preSettingWindow.SetActive(true);

        CanvasGroup pre = preSettingWindow.GetComponent<CanvasGroup>();
        CanvasGroup exit = exitPreSetting.GetComponent<CanvasGroup>();

        StartCoroutine(WindowSwitchRoutine(exit, pre));
    }

    public void ExitSettingClicked()
    {
        CanvasGroup pre = preSettingWindow.GetComponent<CanvasGroup>();
        CanvasGroup exit = exitPreSetting.GetComponent<CanvasGroup>();

        StartCoroutine(WindowSwitchRoutine(pre, exit));
    }

    public void SettingToDataWindowRoutine()
    {
        CanvasGroup pre = preSettingWindow.GetComponent<CanvasGroup>();
        CanvasGroup data = dataWindow.GetComponent<CanvasGroup>();

        StartCoroutine(WindowSwitchRoutine(pre, data));
    }

    public void DestroyAllDataPanels()
    {
        foreach (Transform t in dataPanelParentTransform)
        {
            Destroy(t.gameObject);
        }
    }

    public void DataSetting(SimulationResult res, PreSettingManager manager)
    {
        DataPanel panel = Instantiate(dataPanel, dataPanelParentTransform);
        panel.Initialize(res, manager);
    }

    public void DataToSettingWindowRoutine()
    {
        CanvasGroup pre = preSettingWindow.GetComponent<CanvasGroup>();
        CanvasGroup data = dataWindow.GetComponent<CanvasGroup>();

        StartCoroutine(WindowSwitchRoutine(data, pre));
    }

    private IEnumerator WindowSwitchRoutine(CanvasGroup from, CanvasGroup to)
    {
        float startAlpha = from.alpha;
        float elapsed = 0f;
        float fadeTime = 0.5f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;
            from.alpha = Mathf.Lerp(1, 0, t);

            yield return null;
        }
        from.alpha = 0;
        from.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.2f);

        to.gameObject.SetActive(true);
        startAlpha = to.alpha;
        elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;
            to.alpha = Mathf.Lerp(0, 1, t);

            yield return null;
        }
        to.alpha = 1;
    }
}
