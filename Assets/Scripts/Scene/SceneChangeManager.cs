using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeManager : MonoBehaviour
{
    [SerializeField] private ScreenController controller;
        
    public static SceneChangeManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);        
    }

    public void StartSceneChangeRoutine(float fadeIn, float fadeOut, string scene, bool isWaiting = false)
    {
        StartCoroutine(SceneChangeRoutine(fadeIn, fadeOut, scene, isWaiting));
    }

    private IEnumerator SceneChangeRoutine(float fadeIn, float fadeOut, string scene, bool isWaiting = false)
    {
        controller.StartFadeRoutine(0, 1, fadeOut);

        yield return new WaitForSeconds(fadeOut);

        SceneManager.LoadScene(scene);

        controller.StartFadeRoutine(1, 0, fadeIn, isWaiting);
    }
}
