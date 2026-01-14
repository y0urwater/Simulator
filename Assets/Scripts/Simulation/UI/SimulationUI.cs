using TMPro;
using UnityEngine;
using System;
using System.Collections;

public class SimulationUI : MonoBehaviour
{
    private int agentNum;
    private int escaped = 0;
    private int dead = 0;
    private int alive;

    private Vector2 sizeDelta;

    [Header("Image")]
    [SerializeField] private RectTransform imgEscaped;
    [SerializeField] private RectTransform imgAlive;
    [SerializeField] private RectTransform imgDead;

    [Header("Text")]
    [SerializeField] private TMP_Text txtEscaped;
    [SerializeField] private TMP_Text txtAlive;
    [SerializeField] private TMP_Text txtDead;

    [SerializeField] private TMP_Text txtInjury;
    [SerializeField] private TMP_Text txtConfusion;
    [SerializeField] private TMP_Text txtTime;

    [SerializeField] private TMP_Text txtResultDuration;
    [SerializeField] private TMP_Text txtResultTotal;
    [SerializeField] private TMP_Text txtResultDead;
    [SerializeField] private TMP_Text txtResultEscaped;
    [SerializeField] private TMP_Text txtResultInjuryRate;
    [SerializeField] private TMP_Text txtResultConfusionRate;

    [SerializeField] private TMP_Text txtSaved;

    [Header("Object")]
    [SerializeField] private GameObject objInfoPanel;
    [SerializeField] private GameObject objInjury;
    [SerializeField] private GameObject objConfusion;
    [SerializeField] private GameObject objState;
    [SerializeField] private GameObject objResult;

    [Header("Canvas Group")]
    [SerializeField] private CanvasGroup resultNotification;
    [SerializeField] private CanvasGroup resultWindow;

    private void OnEnable()
    {
        SimulationButtons.onColorNone += NoneColor;
        SimulationButtons.onColorState += StateColor;
        SimulationButtons.onColorInjury += InjuryColor;
        SimulationButtons.onColorConfusion += ConfusionColor;
        SimulationButtons.onQuit += StartQuitRoutine;
        SimulationButtons.onSave += OnSaveClicked;
        AgentManager.onCompleted += StartQuitRoutine;
    }

    private void OnDisable()
    {
        SimulationButtons.onColorNone -= NoneColor;
        SimulationButtons.onColorState -= StateColor;
        SimulationButtons.onColorInjury -= InjuryColor;
        SimulationButtons.onColorConfusion -= ConfusionColor;
        SimulationButtons.onQuit -= StartQuitRoutine;
        SimulationButtons.onSave -= OnSaveClicked;
        AgentManager.onCompleted -= StartQuitRoutine;
    }

    private void Awake()
    {
        agentNum = DataManager.Instance.GetPreData<int>(PreSetData.AgentNum);
        alive = agentNum;
        txtAlive.text = alive.ToString();
        sizeDelta = new Vector2(160 / agentNum, 0);
    }

    private void OnSaveClicked()
    {
        StartCoroutine(SavedTextRoutine());
    }

    private IEnumerator SavedTextRoutine()
    {
        float elapsed = 0;
        float fadeTime = 0.5f;
        Color c = txtSaved.color;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;

            float alpha = Mathf.Lerp(1f, 0f, t);

            c.a = alpha;
            txtSaved.color = c;

            yield return null;
        }

        Color finalColor = txtSaved.color;
        finalColor.a = 0f;
        txtSaved.color = finalColor;
    }

    public void SetResultTime(int minute, int second)
    {
        txtResultDuration.text = $"{minute}m {second}s";        
    }

    public void SetResultNumbers(int dead, int escaped, float injury, float confusion)
    {
        txtResultTotal.text = agentNum.ToString();
        txtResultDead.text = dead.ToString();
        txtResultEscaped.text = escaped.ToString();
        txtResultInjuryRate.text = $"{injury} %";
        txtResultConfusionRate.text = $"{confusion} %";
    }

    private void NoneColor()
    {
        objInjury.SetActive(false);
        objConfusion.SetActive(false);
        objState.SetActive(false);
        objInfoPanel.SetActive(false);
    }

    private void StateColor()
    {
        objInfoPanel.SetActive(true);
        objConfusion.SetActive(false);
        objState.SetActive(true);
        objInjury.SetActive(false);
    }

    private void InjuryColor()
    {
        objInfoPanel.SetActive(true);
        objInjury.SetActive(true);
        objConfusion.SetActive(false);
        objState.SetActive(false);
    }

    private void ConfusionColor()
    {
        objInfoPanel.SetActive(true);
        objInjury.SetActive(false);
        objConfusion.SetActive(true);
        objState.SetActive(false);
    }

    public void Escaped()
    {
        escaped++;
        sizeDelta = imgEscaped.sizeDelta;
        sizeDelta.x = 160 * escaped / agentNum;
        imgEscaped.sizeDelta = sizeDelta;
        txtEscaped.text = escaped.ToString();
    }

    public void UpdateAlive()
    {
        alive--;
        sizeDelta = imgAlive.sizeDelta;
        sizeDelta.x = 160 * alive / agentNum;
        imgAlive.sizeDelta = sizeDelta;        
        txtAlive.text = alive.ToString();
    }

    public void Dead()
    {
        dead++;
        sizeDelta = imgDead.sizeDelta;
        sizeDelta.x = 160 * dead / agentNum;
        imgDead.sizeDelta = sizeDelta;
        txtDead.text = dead.ToString();
    }

    public void UpdateInjuryRate(float rate)
    {
        float round = (float)Math.Round(rate, 1);
        txtInjury.text = $"{round}%";
    }

    public void UpdateConfusionRate(float rate)
    {
        float round = (float)Math.Round(rate, 1);
        txtConfusion.text = $"{round}%";
    }

    public void UpdateTime(int min, int sec)
    {
        txtTime.text = $"{min} : {sec}";
    }

    public void StartQuitRoutine()
    {
        StartCoroutine(QuitRoutine());
    }

    private IEnumerator QuitRoutine()
    {
        foreach (Transform t in transform)
            t.gameObject.SetActive(false);

        objResult.SetActive(true);

        StartCoroutine(CanvasFade(resultNotification, 0.5f, 1));

        yield return new WaitForSeconds(1.5f);

        StartCoroutine(CanvasFade(resultNotification, 0.5f, 0));

        yield return new WaitForSeconds(1f);

        StartCoroutine(CanvasFade(resultWindow, 0.5f, 1));
    }

    private IEnumerator CanvasFade(CanvasGroup canvas, float duration, float end)
    {
        float alpha = canvas.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            canvas.alpha = Mathf.Lerp(alpha, end, t);

            yield return null;
        }

        canvas.alpha = end;
    }
}
