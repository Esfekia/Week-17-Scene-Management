using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistOverSceneChange : MonoBehaviour
{
    // ensure that this script is applied to all the children of the gameobject, which might not necessarily have the same layer
    public bool applyToChildren = true;

    // layer masks are actually just integers
    int m_persistentLayer = 0;
    int m_currentLayer = 0;

    private void Awake()
    {
        m_persistentLayer = LayerMask.NameToLayer("XR Persistent");
        m_currentLayer = gameObject.layer;
    }

    // tie into sceneloaders onLoadStart and onLoadFinish events 
    // because that is what happens at the top of the fade and the end of the fade
    // we will then swap our gameobjects with PersistentXR layer

    private void OnEnable()
    {
        SceneLoader.Instance.onLoadStart.AddListener(StartPersist);
        SceneLoader.Instance.onLoadFinish.AddListener(EndPersist);
    }
    private void OnDisable()
    {
        // lets make sure the SceneLoader is not destroyed before we get here:
        var loader = SceneLoader.Instance;
        if (loader != null)
        {
            loader.onLoadStart.RemoveListener(StartPersist);
            loader.onLoadFinish.RemoveListener(EndPersist);
        }
        // if they are, great! no need for above.

    }

    void StartPersist()
    {
        // swap our layers with the XR Persistent Layer
        m_currentLayer = gameObject.layer;
        SetLayer(gameObject, m_persistentLayer, applyToChildren);

    }

    void EndPersist()
    {
        // when the loading is done we will swap it back to whatever it was before (cashed in m_current_Layer)

        SetLayer(gameObject, m_currentLayer, applyToChildren);
    }

    void SetLayer(GameObject obj, int newLayer, bool applyToChildren)
    {
        obj.layer = newLayer;

        if (applyToChildren)
        {
            foreach(Transform child in obj.transform)
            {
                SetLayer(child.gameObject, newLayer, applyToChildren);

            }
        }
    }
}
