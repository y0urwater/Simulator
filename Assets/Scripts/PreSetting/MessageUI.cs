using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageUI : MonoBehaviour
{
    [SerializeField] TMP_Text passage;
    [SerializeField] Button OK;

    private void Awake()
    {
        OK = GetComponentInChildren<Button>();
    }

    public void SetText(string text)
    {
        passage.text = text;
    }

    private void OKButtonClicked()
    {
        passage.text = string.Empty;
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        OK.onClick.AddListener(OKButtonClicked);
    }

    private void OnDisable()
    {
        OK.onClick.RemoveListener(OKButtonClicked);
    }
}
