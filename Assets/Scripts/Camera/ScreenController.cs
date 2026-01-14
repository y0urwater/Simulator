using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class ScreenController : MonoBehaviour
{
    private Image image;

    private bool isLoaded = false;

    public void SetLoading(bool loading) => isLoaded = loading;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void StartFadeRoutine(float start, float end, float time, bool isWaiting = false)
    {
        StartCoroutine(FadeRoutine(start, end, time, isWaiting));
    }

    private IEnumerator FadeRoutine(float start, float end, float time, bool isWaiting = false)
    {
        if (isWaiting)
            yield return new WaitUntil(() => isLoaded == true);

        isLoaded = false;

        float cur = 0f;
        Color tempColor = image.color;

        while (cur < time)
        {
            cur += Time.deltaTime;

            float rate = cur / time;

            float curAlpha = Mathf.Lerp(start, end, rate);

            tempColor.a = curAlpha;
            image.color = tempColor;

            yield return null;
        }

        tempColor.a = end;
        image.color = tempColor;
    }
}