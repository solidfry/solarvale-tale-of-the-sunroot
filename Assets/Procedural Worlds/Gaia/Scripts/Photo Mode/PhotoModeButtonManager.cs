using UnityEngine;
using UnityEngine.UI;

namespace Gaia
{
    public class PhotoModeButtonManager : MonoBehaviour
    {
        public PhotoModePanelButton m_photoModeButton;
        public PhotoModePanelButton m_cameraButton;
        public PhotoModePanelButton m_unityButton;
        public PhotoModePanelButton m_terrainButton;
        public PhotoModePanelButton m_lightingButton;
        public PhotoModePanelButton m_waterButton;
        public PhotoModePanelButton m_postFXButton;

        /// <summary>
        /// Function used to load up the starting panel
        /// </summary>
        public void Startup()
        {
            if (UIConfiguration.Instance == null)
            {
                return;
            }

            switch (UIConfiguration.Instance.m_startingPanel)
            {
                case 0:
                {
                    ProcessCameraSelected();
                    break;
                }
                case 1:
                {
                    ProcessUnitySelected();
                    break;
                }
                case 2:
                {
                    ProcessTerrainSelected();
                    break;
                }
                case 3:
                {
                    ProcessLightingSelected();
                    break;
                }
                case 4:
                {
                    ProcessWaterSelected();
                    break;
                }
                case 5:
                {
                    ProcessPostFXSelected();
                    break;
                }
                case 6:
                {
                    ProcessPhotoModeSelected();
                    break;
                }
            }
        }

        public void ProcessCameraSelected()
        {
            if (ValidateButtons())
            {
                //Buttons
                m_photoModeButton.SetButtonState(false);
                m_cameraButton.SetButtonState(true);
                m_unityButton.SetButtonState(false);
                m_terrainButton.SetButtonState(false);
                m_lightingButton.SetButtonState(false);
                m_waterButton.SetButtonState(false);
                m_postFXButton.SetButtonState(false);
                //Panels
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_photoMode, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_camera, true);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_unity, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_terrain, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_lighting, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_water, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_postFX, false);
                //Scroll rect
                SetScrollRect(PhotoMode.Instance.m_transformSettings.m_camera);

                UIConfiguration.Instance.m_startingPanel = 0;
            }
        }
        public void ProcessUnitySelected()
        {
            if (ValidateButtons())
            {
                //Buttons
                m_photoModeButton.SetButtonState(false);
                m_cameraButton.SetButtonState(false);
                m_unityButton.SetButtonState(true);
                m_terrainButton.SetButtonState(false);
                m_lightingButton.SetButtonState(false);
                m_waterButton.SetButtonState(false);
                m_postFXButton.SetButtonState(false);
                //Panels
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_photoMode, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_camera, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_unity, true);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_terrain, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_lighting, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_water, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_postFX, false);
                //Scroll rect
                SetScrollRect(PhotoMode.Instance.m_transformSettings.m_unity);

                UIConfiguration.Instance.m_startingPanel = 1;
            }
        }
        public void ProcessTerrainSelected()
        {
            if (ValidateButtons())
            {
                //Buttons
                m_photoModeButton.SetButtonState(false);
                m_cameraButton.SetButtonState(false);
                m_unityButton.SetButtonState(false);
                m_terrainButton.SetButtonState(true);
                m_lightingButton.SetButtonState(false);
                m_waterButton.SetButtonState(false);
                m_postFXButton.SetButtonState(false);
                //Panels
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_photoMode, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_camera, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_unity, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_terrain, true);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_lighting, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_water, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_postFX, false);
                //Scroll rect
                SetScrollRect(PhotoMode.Instance.m_transformSettings.m_terrain);

                UIConfiguration.Instance.m_startingPanel = 2;
            }
        }
        public void ProcessLightingSelected()
        {
            if (ValidateButtons())
            {
                //Buttons
                m_photoModeButton.SetButtonState(false);
                m_cameraButton.SetButtonState(false);
                m_unityButton.SetButtonState(false);
                m_terrainButton.SetButtonState(false);
                m_lightingButton.SetButtonState(true);
                m_waterButton.SetButtonState(false);
                m_postFXButton.SetButtonState(false);
                //Panels
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_photoMode, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_camera, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_unity, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_terrain, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_lighting, true);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_water, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_postFX, false);
                //Scroll rect
                SetScrollRect(PhotoMode.Instance.m_transformSettings.m_lighting);

                UIConfiguration.Instance.m_startingPanel = 3;
            }
        }
        public void ProcessWaterSelected()
        {
            if (ValidateButtons())
            {
                //Buttons
                m_photoModeButton.SetButtonState(false);
                m_cameraButton.SetButtonState(false);
                m_unityButton.SetButtonState(false);
                m_terrainButton.SetButtonState(false);
                m_lightingButton.SetButtonState(false);
                m_waterButton.SetButtonState(true);
                m_postFXButton.SetButtonState(false);
                //Panels
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_photoMode, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_camera, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_unity, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_terrain, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_lighting, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_water, true);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_postFX, false);
                //Scroll rect
                SetScrollRect(PhotoMode.Instance.m_transformSettings.m_water);

                UIConfiguration.Instance.m_startingPanel = 4;
            }
        }
        public void ProcessPostFXSelected()
        {
            if (ValidateButtons())
            {
                //Buttons
                m_photoModeButton.SetButtonState(false);
                m_cameraButton.SetButtonState(false);
                m_unityButton.SetButtonState(false);
                m_terrainButton.SetButtonState(false);
                m_lightingButton.SetButtonState(false);
                m_waterButton.SetButtonState(false);
                m_postFXButton.SetButtonState(true);
                //Panels
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_photoMode, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_camera, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_unity, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_terrain, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_lighting, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_water, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_postFX, true);
                //Scroll rect
                SetScrollRect(PhotoMode.Instance.m_transformSettings.m_postFX);

                UIConfiguration.Instance.m_startingPanel = 5;
            }
        }
        public void ProcessPhotoModeSelected()
        {
            if (ValidateButtons())
            {
                //Buttons
                m_photoModeButton.SetButtonState(true);
                m_cameraButton.SetButtonState(false);
                m_unityButton.SetButtonState(false);
                m_terrainButton.SetButtonState(false);
                m_lightingButton.SetButtonState(false);
                m_waterButton.SetButtonState(false);
                m_postFXButton.SetButtonState(false);
                //Panels
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_photoMode, true);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_camera, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_unity, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_terrain, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_lighting, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_water, false);
                SetPanelState(PhotoMode.Instance.m_transformSettings.m_postFX, false);
                //Scroll rect
                SetScrollRect(PhotoMode.Instance.m_transformSettings.m_photoMode);

                UIConfiguration.Instance.m_startingPanel = 6;
            }
        }
        /// <summary>
        /// Sets the scroll rect contents transform
        /// </summary>
        /// <param name="rect"></param>
        private void SetScrollRect(RectTransform rect)
        {
            if (PhotoMode.Instance != null)
            {
                if (PhotoMode.Instance.m_scrollRect != null)
                {
                    PhotoMode.Instance.m_scrollRect.content = rect;
                    PhotoMode.Instance.m_scrollRect.Rebuild(CanvasUpdate.Layout);
                    if (PhotoMode.Instance.m_scrollRect.verticalScrollbar != null)
                    {
                        PhotoMode.Instance.m_scrollRect.verticalScrollbar.value = 1f;
                        PhotoMode.Instance.m_scrollRect.verticalScrollbar.Rebuild(CanvasUpdate.Layout);
                    }
                }
            }
        }
        /// <summary>
        /// Sets the panel object state based on the bool provided
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="state"></param>
        private void SetPanelState(RectTransform panel, bool state)
        {
            if (panel != null)
            {
                panel.gameObject.SetActive(state);
            }
        }
        /// <summary>
        /// Validates all the systems are present
        /// </summary>
        /// <returns></returns>
        private bool ValidateButtons()
        {
            if (PhotoMode.Instance == null)
            {
                return false;
            }
            if (m_photoModeButton == null)
            {
                return false;
            }
            if (m_cameraButton == null)
            {
                return false;
            }
            if (m_unityButton == null)
            {
                return false;
            }
            if (m_terrainButton == null)
            {
                return false;
            }
            if (m_lightingButton == null)
            {
                return false;
            }
            if (m_waterButton == null)
            {
                return false;
            }
            if (m_postFXButton == null)
            {
                return false;
            }

            return true;
        }
    }
}