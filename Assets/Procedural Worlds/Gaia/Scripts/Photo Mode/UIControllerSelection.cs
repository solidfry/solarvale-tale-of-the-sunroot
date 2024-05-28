using System.Collections.Generic;
using UnityEngine;

namespace Gaia
{
    [System.Serializable]
    public class UIControllerType
    {
        public string m_name = "Controller Type";
        public GaiaConstants.EnvironmentControllerType m_controllerType = GaiaConstants.EnvironmentControllerType.FlyingCamera;
        public GameObject m_controllerText;
    }

    public class UIControllerSelection : MonoBehaviour
    {
        public List<UIControllerType> m_controllerTypes = new List<UIControllerType>();

        /// <summary>
        /// Refreshes the UI and enables the right controller UI
        /// </summary>
        /// <param name="currentController"></param>
        public void RefreshUI(GaiaConstants.EnvironmentControllerType currentController)
        {
            if (m_controllerTypes.Count > 0)
            {
                int selected = -1;
                for (int i = 0; i < m_controllerTypes.Count; i++)
                {
                    if (m_controllerTypes[i].m_controllerType == currentController)
                    {
                        selected = i;
                        break;
                    }
                }

                if (selected != -1)
                {
                    for (int i = 0; i < m_controllerTypes.Count; i++)
                    {
                        if (i == selected)
                        {
                            m_controllerTypes[i].m_controllerText.SetActive(true);
                        }
                        else
                        {
                            m_controllerTypes[i].m_controllerText.SetActive(false);
                        }
                    }
                }
            }
        }
    }
}