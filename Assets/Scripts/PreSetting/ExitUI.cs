using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using TMPro;


public class ExitUI : MonoBehaviour
{
    public int controlNum;
    
    [SerializeField] private Transform verticalLayoutGroup;
    [SerializeField] private TMP_Text totalExits;
    [SerializeField] private TMP_Text openedExits;
    [SerializeField] private TMP_Text closedExits;

    [SerializeField] private ExitControlPool pool;

    private bool isFirst = true;

    public void UpdateExitControl(int num, List<bool> isOpened)
    {
        if (isFirst)
        {
            pool.Create(controlNum);
            isFirst = false;
        }

        bool state;

        ReturnChild(verticalLayoutGroup);

        for (int i = 0; i < num; i++)
        {
            state = isOpened[i];
            ExitControl ctrl = pool.Get();
            ctrl.Initialize(i, state);
            ctrl.transform.SetParent(verticalLayoutGroup);
        }        
    }

    public void UpdateExitInfo(int num, List<bool> isOpened)
    {
        int trueCount = isOpened.Count(item => item);
        int falseCount = num - trueCount;

        totalExits.text = $"Total: {num}";
        openedExits.text = $"Opened: {trueCount}";
        closedExits.text = $"Closed: {falseCount}";
    }

    private void ReturnChild(Transform parent)
    {
        int cnt = parent.childCount;

        for (int i = cnt - 1; i >= 0; i--)
        {            
            Transform child = parent.GetChild(i);
                        
            child.SetParent(pool.transform);

            ExitControl ctrl = child.GetComponent<ExitControl>();
            if (ctrl != null)
                pool.Return(ctrl);
        }
    }
}
