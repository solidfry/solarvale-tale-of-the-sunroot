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
                Debug.LogError($"Scene {sceneName.ToString()} does not exist.");
            }
        } 
    }
}