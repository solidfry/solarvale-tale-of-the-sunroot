using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gaia
{
    [ExecuteAlways]
    public class GaiaTask : MonoBehaviour
    {
        public static GaiaTask Instance
        {
            get
            {
                if (m_instance == null)
                {
                    GaiaTask taskManager = GaiaUtils.FindOOT<GaiaTask>();
                    if (taskManager == null)
                    {
                        GameObject gaiaTaskObject = new GameObject("Gaia Task System")
                        {
                            hideFlags = HideFlags.HideAndDontSave
                        };


                        gaiaTaskObject.transform.SetParent(GaiaUtils.GetGaiaGameObject().transform);
                        taskManager = gaiaTaskObject.AddComponent<GaiaTask>();
                    }

                    m_instance = taskManager;
                }

                return m_instance;
            }
        }
        [SerializeField]
        private static GaiaTask m_instance;

        public List<GaiaTaskBase> Tasks = new List<GaiaTaskBase>();
        public int m_currentTasksInQueue = 0;
        public bool m_taskSystemRunning = false;

        private void OnEnable()
        {
            m_instance = this;
#if UNITY_EDITOR
            EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;
#endif
        }
        private void OnDisable()
        {
            StopTaskProcess(true);
        }
        private void OnDestroy()
        {
            StopTaskProcess(true);
        }

        private void EditorUpdate()
        {
            //Clean up finished tasks
            for (int i = Tasks.Count; i-- > 0;)
            {
                if (Tasks[i] != null)
                {
                    if (Tasks[i].TaskFinished)
                    {
                        Tasks.RemoveAt(i);
                    }
                }
            }

            if (Tasks.Count == 0)
            {
                StopTaskProcess();
            }

            //Assign task count
            m_currentTasksInQueue = Tasks.Count;
        }

        public void AddTask(GaiaTaskBase task)
        {
            if (task != null)
            {
                Tasks.Add(task);
            }

            if (!m_taskSystemRunning)
            {
                StartCoroutine(ProcessTasks());
            }
        }
        private IEnumerator ProcessTasks()
        {
            while (Tasks.Count > 0)
            {
                m_taskSystemRunning = true;
                //Process Task
                for (int i = 0; i < Tasks.Count; i++)
                {
                    if (Tasks[i] != null && !Tasks[i].TaskFinished)
                    {
                        yield return new WaitForSeconds(Tasks[i].TaskWaitTime);
                        Tasks[i].DoTask();
                    }
                }

                yield return new WaitForEndOfFrame();
            }
        }

        private void StopTaskProcess(bool stopEditorUpdate = false)
        {
            if (stopEditorUpdate)
            {
#if UNITY_EDITOR
                EditorApplication.update -= EditorUpdate;
#endif
            }

            m_taskSystemRunning = false;
            StopAllCoroutines();
        }
    }
}