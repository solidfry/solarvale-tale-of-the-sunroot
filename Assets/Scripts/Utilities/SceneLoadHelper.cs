using UnityEngine;

namespace Utilities
{
    public class SceneLoadHelper : MonoBehaviour
    {
        public static void LoadScene(string sceneName)
        {
            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogError($"Scene {sceneName} does not exist.");
            }
        } 
        
        public static void LoadScene(int sceneIndex)
        {
            if (Application.CanStreamedLevelBeLoaded(sceneIndex))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
            }
            else
            {
                Debug.LogError($"Scene {sceneIndex} does not exist.");
            }
        }
        
        public static void LoadSceneAdditive(string sceneName)
        {
            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
            }
            else
            {
                Debug.LogError($"Scene {sceneName.ToString()} does not exist.");
            }
        }
        
        public static void LoadSceneAdditive(int sceneIndex)
        {
            if (Application.CanStreamedLevelBeLoaded(sceneIndex))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex, UnityEngine.SceneManagement.LoadSceneMode.Additive);
            }
            else
            {
                Debug.LogError($"Scene {sceneIndex} does not exist.");
            }
        }

        public static void QuitApplication()
        {
            Application.Quit();
        }

    }
}