using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class IncidentManager : MonoBehaviour
{
    private bool isOccurred = false;

    public GameObject explosionPrefab;
    public GameObject firePrefab;
    public LayerMask layer;

    public static event Action<Vector3> OnIncidentOccurred;
    public static event Action OnIncidentOccurredNormal;

    private IEnumerator TriggerIncidentByMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        List<Vector2> s = DataManager.Instance.GetPreData<List<Vector2>>(PreSetData.Size);
        Vector2 size = s[0];

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layer) && Mathf.Abs(hit.point.x) < size.x && Mathf.Abs(hit.point.z) < size.y && !isOccurred)
        {
            isOccurred = true;

            if (explosionPrefab != null)
            {
                GameObject ef = Instantiate(explosionPrefab, hit.point + new Vector3(0, 1, 0), Quaternion.identity);
                Destroy(ef, 1f);
            }

            OnIncidentOccurred?.Invoke(hit.point);
            OnIncidentOccurredNormal?.Invoke();
            yield return new WaitForSeconds(0.3f);
            Instantiate(firePrefab, hit.point, Quaternion.identity);
        }
    }

    private bool IsMouseOverUI()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            if (result.gameObject.layer == LayerMask.NameToLayer("UI"))
                return true;
        }
        return false;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (IsMouseOverUI() || Time.timeScale == 0)
            {
                return;
            }

            StartCoroutine(TriggerIncidentByMouse());
        }
    }
}
