using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEditor;

// no need for monobehavior, we are just going to do an editor extension.
public static class SceneMenu
{
    // turn these functions into menu items:
    [MenuItem("Scenes/Lobby")]
    static void OpenLobby()
    {
        OpenScene(SceneUtils.Names.Lobby);
    }
    [MenuItem("Scenes/Maze")]
    static void OpenMaze()
    {
        OpenScene(SceneUtils.Names.Maze);
    }
    [MenuItem("Scenes/MoodMatic")]
    static void OpenMoodMatic()
    {
        OpenScene(SceneUtils.Names.MoodMatic);
    }

    static void OpenScene(string name)
    {
        // make sure this is the active scene when we open up since it has the XR interaction manager
        // single mode closes all open scenes and opens the scene specified:
        Scene persistentScene = EditorSceneManager.OpenScene("Assets/Scenes/" + SceneUtils.Names.XRPersistent + ".unity", OpenSceneMode.Single);
        
        // Additive: add a scene to the current scene and open it.
        Scene currentScene = EditorSceneManager.OpenScene("Assets/Scenes/" + name + ".unity", OpenSceneMode.Additive);

        // Align the positions of Persistent Scene XR Origin with the XR Rig/Origins in other "target" scenes.
        SceneUtils.AlignXRRig(persistentScene, currentScene);

    }


}
