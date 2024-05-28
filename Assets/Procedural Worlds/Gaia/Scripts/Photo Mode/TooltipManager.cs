using UnityEngine;
using UnityEngine.UI;

namespace Gaia
{
    public enum TooltipTheme { Light, Dark }
    public enum InteractionMode { UI, SceneObjects, Both }

    [System.Serializable]
    public class TooplipThemeSetup
    {
        public TooltipTheme m_theme = TooltipTheme.Dark;
        public Image m_background;
        public Text m_header;
        public Text m_content;
    }

    public class TooltipManager : MonoBehaviour
    {
        public static TooltipManager Instance
        {
            get { return m_instance; }
        }
        [SerializeField] private static TooltipManager m_instance;

        public TooltipProfile m_tooltipProfile;
        public InteractionMode m_interactionMode = InteractionMode.Both;
        public Tooltip m_tooltip;
        public float m_tooltipYOffsetBottomHalf = 2f;
        public float m_tooltipYOffsetTopHalf = 2f;
        public float m_tooltipDelay = 2f;

        public TooplipThemeSetup m_themeSettings = new TooplipThemeSetup();

        private void Awake()
        {
            SetInstance();
        }
        private void Update()
        {
            ProcessTheme();
            Vector2 mousePos = Input.mousePosition;

            float pivotPosX = mousePos.x / Screen.width;
            float pivotPosY = mousePos.y / Screen.height;

            if (mousePos.y <= Screen.height / 2f)
            {
                m_tooltip.m_tooltipRect.pivot = new Vector2(pivotPosX, pivotPosY * m_tooltipYOffsetBottomHalf);
            }
            else
            {
                m_tooltip.m_tooltipRect.pivot = new Vector2(pivotPosX, pivotPosY * m_tooltipYOffsetTopHalf);
            }

            m_tooltip.transform.position = mousePos;

            if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.anyKeyDown)
            {
                HideTooltip();
            }
        }

        public void SetInstance()
        {
            m_instance = this;
        }
        /// <summary>
        /// Gets a tooltip profile
        /// </summary>
        /// <param name="tooltipID"></param>
        /// <returns></returns>
        public TooltipProfileData GetTooltip(string tooltipID)
        {
            if (m_tooltipProfile == null)
            {
                Debug.LogWarning("Tooltip profile is null on the tooltip manager");
                return null;
            }

            if (m_tooltipProfile.m_tooltips.Count < 1)
            {
                Debug.LogWarning("No tooltips have been created");
                return null;
            }

            foreach (TooltipProfileData profile in m_tooltipProfile.m_tooltips)
            {
                if (profile.m_tooltipID.Contains(tooltipID))
                {
                    return profile;
                }
            }

            Debug.LogWarning("Tooltip '" + tooltipID + "' ID was not found please make sure you include it in the tooltips from the Tooltip Manager");
            return null;
        }
        /// <summary>
        /// Adds a new tooltip
        /// </summary>
        /// <param name="tooltipProfile"></param>
        public void AddTooltip(TooltipProfileData tooltipProfile)
        {
            if (m_tooltipProfile == null)
            {
                Debug.LogWarning("Tooltip profile is null on the tooltip manager");
                return;
            }

            if (tooltipProfile != null)
            {
                m_tooltipProfile.m_tooltips.Add(tooltipProfile);
            }
        }
        /// <summary>
        /// Adds a new tooltip
        /// </summary>
        /// <param name="tooltipProfile"></param>
        public void AddTooltip(string ID, string header, string text)
        {
            if (m_tooltipProfile == null)
            {
                Debug.LogWarning("Tooltip profile is null on the tooltip manager");
                return;
            }

            TooltipProfileData tooltipProfile = new TooltipProfileData
            {
                m_tooltipID = ID, m_header = header, m_text = text
            };

            if (tooltipProfile != null)
            {
                m_tooltipProfile.m_tooltips.Add(tooltipProfile);
            }
        }
        /// <summary>
        /// Removes a tooltip ID
        /// </summary>
        /// <param name="tooltipID"></param>
        /// <returns></returns>
        public bool RemoveTooltip(string tooltipID)
        {
            if (m_tooltipProfile == null)
            {
                Debug.LogWarning("Tooltip profile is null on the tooltip manager");
                return false;
            }

            if (m_tooltipProfile.m_tooltips.Count > 0)
            {
                for (int i = 0; i < m_tooltipProfile.m_tooltips.Count; i++)
                {
                    if (m_tooltipProfile.m_tooltips[i].m_tooltipID.Contains(tooltipID))
                    {
                        m_tooltipProfile.m_tooltips.RemoveAt(i);
                        return true;
                    }
                }
            }

            return false;
        }
        /// <summary>
        /// Updates the tooptip theme
        /// </summary>
        public void ProcessTheme()
        {
            if (m_themeSettings.m_background == null || m_themeSettings.m_header == null || m_themeSettings.m_content == null)
            {
                return;
            }

            switch (m_themeSettings.m_theme)
            {
                case TooltipTheme.Light:
                {
                    m_themeSettings.m_background.color = Color.white;
                    m_themeSettings.m_header.color = Color.black;
                    m_themeSettings.m_content.color = Color.black;
                    break;
                }
                case TooltipTheme.Dark:
                {
                    m_themeSettings.m_background.color = new Color(0.1981132f, 0.1981132f, 0.1981132f, 1f);
                    m_themeSettings.m_header.color = Color.white;
                    m_themeSettings.m_content.color = Color.white;
                    break;
                }
            }
        }
        /// <summary>
        /// Shows the tooltip UI
        /// </summary>
        /// <param name="content"></param>
        /// <param name="header"></param>
        public static void ShowTooltip(string content, string header)
        {
            if (Instance != null)
            {
                if (Instance.m_tooltip != null)
                {
                    Instance.m_tooltip.SetTooltipText(content, header);
                    Instance.m_tooltip.gameObject.SetActive(true);
                }
            }
        }
        /// <summary>
        /// Hides the tooltip UI
        /// </summary>
        public static void HideTooltip()
        {
            if (Instance != null)
            {
                if (Instance.m_tooltip != null)
                {
                    Instance.m_tooltip.gameObject.SetActive(false);
                }
            }
        }
    }
}