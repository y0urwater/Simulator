using UnityEngine;

public class CameraDisable : MonoBehaviour
{
    private void OnDisable()
    {
        CameraController.Instance.enabled = true;
    }

    private void OnEnable()
    {
        CameraController.Instance.enabled = false;
    }
}
