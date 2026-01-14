using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class WallObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private MeshRenderer rend;
    private Color originColor;
    private int layerIdx;
    private int idx;
    private string layer;

    private int colorID = Shader.PropertyToID("_ColorOverlay");
    private int opacityID = Shader.PropertyToID("_ColorOverlayOpacity");

    public event Action<string, int> onWallClicked;

    public void SetIdx(int index) => idx = index;

    public void SetLayer(string layer)
    {
        layerIdx = LayerMask.NameToLayer(layer);
        this.layer = layer;

        if (layerIdx != -1)
            gameObject.layer = layerIdx;
        else
            Debug.Log("레이어 이름 오류!");
    }

    public void SetColor()
    {
        rend.material.SetFloat(opacityID, 1f);

        if (layer == "ExitTrue")
            rend.material.SetColor(colorID, Color.green);
        else
            rend.material.SetColor(colorID, Color.red);
    }

    void Awake()
    {
        rend = GetComponent<MeshRenderer>();

        if (rend.material.HasProperty(colorID))
        {
            originColor = rend.material.GetColor(colorID);
        }
        else
        {
            originColor = Color.white;
        }
    }

    public void ResetColor()
    {
        rend.material.SetColor(colorID, originColor);
        rend.material.SetFloat(opacityID, 0f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onWallClicked.Invoke(layer, idx);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetColor();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetColor();
    }    
}
