using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;

namespace Core
{
    public class BootStrapDataManager : SingletonPersistent<BootStrapDataManager>
    {
        
        
    }

    public static class PerformBootStrap
    {
        const string SceneName = "Init";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Execute()
        {
            // traverse currently loaded scenes 
            for (int sceneIndex = 0; sceneIndex < UnityEngine.SceneManagement.SceneManager.sceneCount; sceneIndex++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(sceneIndex);
                if (scene.name == SceneName)
                {
                    return;
                } else if (SceneManager.GetActiveScene().name != SceneName)
                {
                    SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().name);
                    SceneManager.LoadScene(SceneName, LoadSceneMode.Single);
                }
                
            }
        }
    }
}