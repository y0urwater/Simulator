using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExitControl : MonoBehaviour
{
    private ExitSettingMode exitSettingMode;

    public Button open;
    public Button close;
    public Button delete;
    public TMP_Text idx;

    private int index;

    private void Awake()
    {
        exitSettingMode = FindAnyObjectByType<ExitSettingMode>(FindObjectsInactive.Include);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Initialize(int index, bool isOpened)
    {
        idx.text = index.ToString();
        this.index = index;

        open.interactable = !isOpened;
        close.interactable = isOpened;
    }

    private void OnEnable()
    {
        open.onClick.AddListener(OnOpenClicked);
        close.onClick.AddListener(OnCloseClicked);
        delete.onClick.AddListener(OnDeleteClicked);
    }

    private void OnDisable()
    {
        open.onClick.RemoveListener(OnOpenClicked);
        close.onClick.RemoveListener(OnCloseClicked);
        delete.onClick.RemoveListener(OnDeleteClicked);
    }

    private void OnOpenClicked()
    {
        exitSettingMode.UpdateOpen(index, true);
        Initialize(index, true);
    }

    private void OnCloseClicked()
    {
        exitSettingMode.UpdateOpen(index, false);
        Initialize(index, false);
    }

    private void OnDeleteClicked()
    {
        exitSettingMode.DeleteExit(index);
    }
}
