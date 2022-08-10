using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class SceneLoader : Singleton<SceneLoader>
{
    public Material screenFade = null;
    
    [Min(0.001f)]
    public float fadeSpeed = 1.0f;
    [Range(0.0f, 5.0f)]
    public float addedWaitTime = 2.0f;
   
    public UnityEvent onLoadStart = new UnityEvent();
    public UnityEvent onBeforeUnload = new UnityEvent();
    public UnityEvent onLoadFinish = new UnityEvent();

    bool m_isLoading = false;

    // control whether we are at 9 or 1 
    float m_fadeAmount = 0.0f;

    // keep track of the coroutine that is doing the fading,
    // in case we need to stop that coroutine and start it for fading in and out quickly
    Coroutine m_fadeCoroutine = null;

    // cache the shader property ID by using the m_fadeAmount above
    static readonly int m_fadeAmountPropID = Shader.PropertyToID("_FadeAmount");

    Scene m_persistentScene;

    private void Awake()
    {
        SceneManager.sceneLoaded += SetActiveScene;

        //get the active scene when the script wakes up so that we can reference the persistent scene
        m_persistentScene = SceneManager.GetActiveScene();

        // just to know that something is happening and that we are not in the editor
        if (!Application.isEditor)
        {
            SceneManager.LoadSceneAsync(SceneUtils.Names.Lobby, LoadSceneMode.Additive);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= SetActiveScene;
    }
    public void LoadScene(string name)
    {
        if (!m_isLoading)
        {
            // use coroutines and async operation so that nothing is stopped and done in the background
            StartCoroutine(Load(name));
        }

    }

    void SetActiveScene(Scene scene, LoadSceneMode mode)
    {
        SceneManager.SetActiveScene(scene);

        //Once this scene is loaded and is set active we will use sceneUtils to aling the XR rig/origin
        SceneUtils.AlignXRRig(m_persistentScene, scene);

    }


    IEnumerator Load(string name)
    {
        m_isLoading = true;

        //We will use the ? syntax to optionally load, in case nobody is plugged into it, to avoid an error
        onLoadStart?.Invoke();
        yield return FadeOut();

        onBeforeUnload?.Invoke();
        // just in case, wait for the next frame to come around, to be able to clean up before next frame
        yield return new WaitForSeconds(0);
        yield return StartCoroutine(UnloadCurrentScene());

        // to ensure the fade in out is not instantaneous
        yield return new WaitForSeconds(addedWaitTime);

        yield return StartCoroutine(LoadCurrentScene(name));
        yield return FadeIn();

        // optionally invoke any callbacks that are waiting for this to finish
        onLoadFinish?.Invoke();
        m_isLoading = false;
    }

    IEnumerator UnloadCurrentScene()
    {
        // get the active scene from SceneManager and unload it asynchronously
        AsyncOperation unload = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        while (!unload.isDone)
            yield return null;
    }

    IEnumerator LoadCurrentScene(string name)
    {
        // add the scene to our already existing scene using Additive Mode
        AsyncOperation load = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
        while (!load.isDone)
            yield return null;
    }

    IEnumerator FadeOut()
    {
        if (m_fadeCoroutine != null)
        {
            StopCoroutine(m_fadeCoroutine);
        }
        m_fadeCoroutine = StartCoroutine(Fade(1.0f));
        yield return m_fadeCoroutine;
    }

    IEnumerator FadeIn()
    {
        if (m_fadeCoroutine != null)
        {
            StopCoroutine(m_fadeCoroutine);
        }
        m_fadeCoroutine = StartCoroutine(Fade(0.0f));
        yield return m_fadeCoroutine;
    }

    IEnumerator Fade(float target)
    {
        // update our fade amount and pass it to the shader in the material
        while (!Mathf.Approximately(m_fadeAmount, target))
        {
            m_fadeAmount = Mathf.MoveTowards(m_fadeAmount, target, fadeSpeed * Time.deltaTime);
            screenFade.SetFloat(m_fadeAmountPropID, m_fadeAmount);
            yield return null;
        }

        screenFade.SetFloat(m_fadeAmountPropID, m_fadeAmount);
    }

}
