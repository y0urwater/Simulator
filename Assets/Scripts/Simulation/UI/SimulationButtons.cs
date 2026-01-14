using System;
using UnityEngine;
using UnityEngine.UI;

public class SimulationButtons : MonoBehaviour
{
    [Header("Color Modes")]
    [SerializeField] private Button btnInjury;
    [SerializeField] private Button btnConfusion;
    [SerializeField] private Button btnState;
    [SerializeField] private Button btnNone;

    [Header("Simulation Speed")]
    [SerializeField] private Button btnPause;
    [SerializeField] private Button btnSlow;
    [SerializeField] private Button btnNormal;
    [SerializeField] private Button btnFast;

    [Header("ETC")]
    [SerializeField] private Button btnQuit;

    [Header("Result")]
    [SerializeField] private Button btnReturn;
    [SerializeField] private Button btnSave;
    [SerializeField] private Button btnExit;

    public static event Action onColorInjury;
    public static event Action onColorConfusion;
    public static event Action onColorState;
    public static event Action onColorNone;
    public static event Action onPause;
    public static event Action onSlow;
    public static event Action onNormal;
    public static event Action onFast;
    public static event Action onQuit;
    public static event Action onReturn;
    public static event Action onSave;
    public static event Action onExit;

    private void OnEnable()
    {
        btnInjury.onClick.AddListener(OnInjuryColorClicked);
        btnConfusion.onClick.AddListener(OnConfusionColorClicked);
        btnState.onClick.AddListener(OnStateColorClicked);
        btnNone.onClick.AddListener(OnNoneColorClicked);

        btnPause.onClick.AddListener(OnPauseClicked);
        btnSlow.onClick.AddListener(OnSlowClicked);
        btnNormal.onClick.AddListener(OnNormalClicked);
        btnFast.onClick.AddListener(OnFastClicked);

        btnQuit.onClick.AddListener(OnQuitClicked);

        btnReturn.onClick.AddListener(OnReturnClicked);
        btnSave.onClick.AddListener(OnSaveClicked);
        btnExit.onClick.AddListener(OnExitClicked);
    }

    private void OnDisable()
    {
        btnInjury.onClick.RemoveListener(OnInjuryColorClicked);
        btnConfusion.onClick.RemoveListener(OnConfusionColorClicked);
        btnState.onClick.RemoveListener(OnStateColorClicked);
        btnNone.onClick.RemoveListener(OnNoneColorClicked);

        btnPause.onClick.RemoveListener(OnPauseClicked);
        btnSlow.onClick.RemoveListener(OnSlowClicked);
        btnNormal.onClick.RemoveListener(OnNormalClicked);
        btnFast.onClick.RemoveListener(OnFastClicked);

        btnQuit.onClick.RemoveListener(OnQuitClicked);

        btnReturn.onClick.RemoveListener(OnReturnClicked);
        btnSave.onClick.RemoveListener(OnSaveClicked);
        btnExit.onClick.RemoveListener(OnExitClicked);
    }

    private void OnSaveClicked()
    {
        btnSave.interactable = false;
        onSave.Invoke();
    }

    private void OnInjuryColorClicked()
    {
        onColorInjury.Invoke();
    }

    private void OnConfusionColorClicked()
    {
        onColorConfusion.Invoke();
    }

    private void OnStateColorClicked()
    {
        onColorState.Invoke();
    }

    private void OnNoneColorClicked()
    {
        onColorNone.Invoke();
    }

    private void OnPauseClicked()
    {
        onPause.Invoke();
    }

    private void OnSlowClicked()
    {
        onSlow.Invoke();
    }

    private void OnNormalClicked()
    {
        onNormal.Invoke();
    }

    private void OnFastClicked()
    {
        onFast.Invoke();
    }

    private void OnQuitClicked()
    {
        onQuit.Invoke();
    }

    private void OnReturnClicked()
    {
        onReturn.Invoke();
    }

    private void OnExitClicked()
    {
        onExit.Invoke();
    }
}
