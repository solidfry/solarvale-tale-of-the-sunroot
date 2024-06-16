using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;

namespace Core
{
    public class BootStrapDataManager : SingletonPersistent<BootStrapDataManager>
    {
        [SerializeField] private bool doBootstrap = true;

        private void OnValidate()
        {
            PerformBootStrap.SetDoBootstrap(doBootstrap);
        }

        public override void Awake()
        {
            base.Awake();
            PerformBootStrap.SetDoBootstrap(doBootstrap);
        }
    }

    public static class PerformBootStrap
    {
        const string SceneName = "Init";
        static bool doBootstrap = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Execute()
        {
            if (!doBootstrap) return;

            bool isInitSceneLoaded = false;

            // Traverse currently loaded scenes 
            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                var scene = SceneManager.GetSceneAt(sceneIndex);
                if (scene.name == SceneName)
                {
                    isInitSceneLoaded = true;
                    break;
                }
            }

            if (!isInitSceneLoaded)
            {
                if (SceneManager.GetActiveScene().name != SceneName)
                {
                    SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
                }
            }
        }

        public static void SetDoBootstrap(bool value)
        {
            doBootstrap = value;
        }
    }
}