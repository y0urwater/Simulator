using UnityEngine;
using UnityEngine.UI;

public class UIGradientBar : MonoBehaviour
{
    public Gradient gradient;

    void Start()
    {
        var image = GetComponent<Image>();
        var tex = new Texture2D(100, 1);
        for (int i = 0; i < 100; i++)
        {
            tex.SetPixel(i, 0, gradient.Evaluate(i / 100f));
        }
        tex.Apply();

        image.sprite = Sprite.Create(tex, new Rect(0, 0, 100, 1), Vector2.zero);
    }
}