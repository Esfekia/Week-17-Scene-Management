using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class SceneLoader : Singleton<SceneLoader>
{
    public Material screenfade = null;
    
    public UnityEvent onLoadStart = new UnityEvent();
    public UnityEvent onLoadFinish = new UnityEvent();

    bool m_isLoading = false;
    public void LoadScene(string name)
    {
        if (!m_isLoading)
        {
            // use coroutines and async operation so that nothing is stopped and done in the background
            StartCoroutine(Load(name));
        }

    }

    IEnumerator Load(string name)
    {
        m_isLoading = true;

        //We will use the ? syntax to optionally load, in case nobody is plugged into it, to avoid an error
        onLoadStart?.Invoke();
        yield return FadeOut();
        yield return StartCoroutine(UnloadCurrentScene());

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

    }

    IEnumerator FadeIn()
    {

    }


}
