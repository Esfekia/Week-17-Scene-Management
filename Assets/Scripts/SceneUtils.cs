using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneUtils
{
    // So that we can use these names in our GoToScene.cs
    public enum SceneId
    {
        Lobby,
        Maze,
        MoodMatic
    }

    public static readonly string[] scenes = { Names.Lobby, Names.Maze, Names.MoodMatic };
    public static class Names
    {
        public static readonly string XRPersistent = "XR Persistent";
        public static readonly string Maze = "Maze";
        public static readonly string MoodMatic = "MoodMatic";
        public static readonly string Lobby = "Lobby";
    }

    // Align the position of our PersistentScene XR Origin with the XR Rigs found in our other scenes.
    public static void AlignXRRig(Scene persistentScene, Scene currentScene)
    {
        GameObject[] currentObjects = currentScene.GetRootGameObjects();
        GameObject[] persistentObjects = persistentScene.GetRootGameObjects();
        foreach (var origin in currentObjects)
        {
            if (origin.CompareTag("XRRigOrigin"))
            {
                foreach(var rig in persistentObjects)
                { 
                    if(rig.CompareTag("XRRig"))
                    {
                        rig.transform.position = origin.transform.position;
                        rig.transform.rotation = origin.transform.rotation;
                        return;
                    }
                }
            }
        }

    }

}
