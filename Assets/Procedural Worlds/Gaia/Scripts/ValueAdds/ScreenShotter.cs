using System;
using UnityEngine;
using System.IO;
using UnityEngine.Experimental.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gaia
{
    /// <summary>
    /// A simple screen shot taker - with thanks to @jamiepollard on the Gaia forum 
    /// Adapted from the original script here:
    /// http://answers.unity3d.com/questions/22954/how-to-save-a-picture-take-screenshot-from-a-camer.html
    /// </summary>
    [GaiaScriptOrder(500)]
    public class ScreenShotter : MonoBehaviour
    {
        /// <summary>
        /// Key the screenshotter is bound to
        /// </summary>
        public KeyCode m_screenShotKey = KeyCode.F12;

        /// <summary>
        /// The storage format, JPG is smaller, PNG is higher quality
        /// </summary>
        public Gaia.GaiaConstants.ImageFileType m_imageFormat = Gaia.GaiaConstants.ImageFileType.Png;

        /// <summary>
        /// The target directory for the screenshots i.e. /Assets/targetdir
        /// </summary>
        public string m_targetDirectory = "Gaia User Data/Screenshots";

        public string m_buildTargetDirectory = "Procedural Worlds/Gaia";

        /// <summary>
        /// Target resolution used to quickly change both width and height
        /// </summary>
        public GaiaConstants.ScreenshotResolution m_screenshotResolution = GaiaConstants.ScreenshotResolution.Resolution1920X1080;

        /// <summary>
        /// Target screenshot width
        /// </summary>
        public int m_targetWidth = 1920;

        /// <summary>
        /// Target screenshot height
        /// </summary>
        public int m_targetHeight = 1080;

        /// <summary>
        /// If set the actual screen dimensions will be used instead of target dimensions
        /// </summary>
        public bool m_useScreenSize = true;

        /// <summary>
        /// The screen shot camera
        /// </summary>
        public Camera m_mainCamera;

        /// <summary>
        /// A toggle to cause the next updatre to take a shot
        /// </summary>
        private bool m_takeShot = false;

        /// <summary>
        /// A toggle to cause an asset db refresh
        /// </summary>
        private bool m_refreshAssetDB = false;

        /// <summary>
        /// Texture used for the watermark
        /// </summary>
        public Texture2D m_watermark;

        /// <summary>
        /// The last saved path
        /// </summary>
        public string m_lastSavedPath;
        /// <summary>
        /// Sets the max save path character limit
        /// </summary>
        public int m_maxSavePathCount = 40;
        /// <summary>
        /// Sets up the camera if not already done
        /// </summary>
        private void OnEnable()
        {
            if (m_mainCamera == null)
            {
                m_mainCamera = GaiaUtils.GetCamera(true);
            }

            //Create the target directory
            if (Application.isEditor)
            {
                string path = Path.Combine(Application.dataPath, m_targetDirectory);
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
#if UNITY_EDITOR
                    AssetDatabase.Refresh();
#endif
                }
            }
            else
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), m_buildTargetDirectory);
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }
            }
        }
        /// <summary>
        /// Get unity to refresh the files list
        /// </summary>
        private void OnDisable()
        {
            //Refresh the asset database
            if (m_refreshAssetDB)
            {
                m_refreshAssetDB = false;
                #if UNITY_EDITOR
                    AssetDatabase.Refresh();
                #endif
            }
        }
        /// <summary>
        /// On Start find camera
        /// </summary>
        private void Start()
        {
            if (m_mainCamera == null)
            {
                m_mainCamera = GaiaUtils.GetCamera(true);
            }
        }
        /// <summary>
        /// Takes the actual screen shot when the key is pressed or takeshot is true
        /// </summary>
        private void LateUpdate()
        {
            if (Input.GetKeyDown(m_screenShotKey) || m_takeShot)
            {
                TakeScreenshotAndSave();
            }
        }

        private void TakeScreenshotAndSave()
        {
            try
            {
                if (m_mainCamera == null)
                {
                    m_mainCamera = GaiaUtils.GetCamera(true);
                }

                //Pick up and use the actual screen dimensions
                if (m_useScreenSize)
                {
                    m_targetWidth = Screen.width;
                    m_targetHeight = Screen.height;
                }
                else
                {
                    UpdateScreenshotResolution(m_screenshotResolution);
                }

                m_refreshAssetDB = true;
                RenderTexture rt;
                if (m_imageFormat == GaiaConstants.ImageFileType.Exr)
                {
                    rt = new RenderTexture(m_targetWidth, m_targetHeight, 24, DefaultFormat.HDR);
                }
                else
                {
                    rt = new RenderTexture(m_targetWidth, m_targetHeight, 24, DefaultFormat.LDR);
                }
                m_mainCamera.targetTexture = rt;
                Texture2D screenShot;
                if (m_imageFormat == GaiaConstants.ImageFileType.Exr)
                {
                    screenShot = new Texture2D(m_targetWidth, m_targetHeight, TextureFormat.RGBAFloat, false);
                }
                else
                {
                    screenShot = new Texture2D(m_targetWidth, m_targetHeight, TextureFormat.RGB24, false);
                }

                bool allowHDR = m_mainCamera.allowHDR;
                if (m_imageFormat == GaiaConstants.ImageFileType.Exr)
                {
                    m_mainCamera.allowHDR = true;
                }

                if (GaiaUtils.GetActivePipeline() == GaiaConstants.EnvironmentRenderer.BuiltIn)
                {
                    m_mainCamera.Render();
                }
                else
                {
                    //Insure everything renders correctly
                    m_mainCamera.Render();
                    m_mainCamera.Render();
                    m_mainCamera.Render();
                    m_mainCamera.Render();
                    m_mainCamera.Render();
                }

                m_mainCamera.allowHDR = allowHDR;
                RenderTexture.active = rt;
                screenShot.ReadPixels(new Rect(0, 0, m_targetWidth, m_targetHeight), 0, 0);
                m_mainCamera.targetTexture = null;
                RenderTexture.active = null; // JC: added to avoid errors
                rt.Release();
                Destroy(rt);

                if (m_watermark != null)
                {
                    Gaia.GaiaUtils.MakeTextureReadable(m_watermark);
                    screenShot = AddWatermark(screenShot, m_watermark);
                }

                byte[] bytes = null;
                switch (m_imageFormat)
                {
                    case GaiaConstants.ImageFileType.Exr:
                        bytes = ImageConversion.EncodeToEXR(screenShot, Texture2D.EXRFlags.CompressZIP);
                        break;
                    case GaiaConstants.ImageFileType.Png:
                        bytes = ImageConversion.EncodeToPNG(screenShot);
                        break;
                    case GaiaConstants.ImageFileType.Tga:
                        bytes = ImageConversion.EncodeToTGA(screenShot);
                        break;
                    case GaiaConstants.ImageFileType.Jpg:
                        bytes = ImageConversion.EncodeToJPG(screenShot, 100);
                        break;
                }

                if (ScreenShotName(m_targetWidth, m_targetHeight, out string filename))
                {
                    PWCommon5.Utils.WriteAllBytes(filename, bytes);
                    Debug.Log(string.Format("Took screenshot to: {0}", filename));
                }

                m_takeShot = false;
            }
            catch (Exception e)
            {
                m_takeShot = false;
                Debug.LogError(e.Message + e.StackTrace);
            }
        }

        /// <summary>
        /// Updates the screenshot resolution
        /// </summary>
        /// <param name="screenshotResolution"></param>
        public void UpdateScreenshotResolution(GaiaConstants.ScreenshotResolution screenshotResolution)
        {
            switch (screenshotResolution)
            {
                case GaiaConstants.ScreenshotResolution.Resolution640X480:
                    m_targetWidth = 640;
                    m_targetHeight = 480;
                    break;
                case GaiaConstants.ScreenshotResolution.Resolution800X600:
                    m_targetWidth = 800;
                    m_targetHeight = 600;
                    break;
                case GaiaConstants.ScreenshotResolution.Resolution1280X720:
                    m_targetWidth = 1280;
                    m_targetHeight = 720;
                    break;
                case GaiaConstants.ScreenshotResolution.Resolution1366X768:
                    m_targetWidth = 1366;
                    m_targetHeight = 768;
                    break;
                case GaiaConstants.ScreenshotResolution.Resolution1600X900:
                    m_targetWidth = 1600;
                    m_targetHeight = 900;
                    break;
                case GaiaConstants.ScreenshotResolution.Resolution1920X1080:
                    m_targetWidth = 1920;
                    m_targetHeight = 1080;
                    break;
                case GaiaConstants.ScreenshotResolution.Resolution2560X1440:
                    m_targetWidth = 2560;
                    m_targetHeight = 1440;
                    break;
                case GaiaConstants.ScreenshotResolution.Resolution3840X2160:
                    m_targetWidth = 3840;
                    m_targetHeight = 2160;
                    break;
                case GaiaConstants.ScreenshotResolution.Resolution7680X4320:
                    m_targetWidth = 7680;
                    m_targetHeight = 4320;
                    break;
            }
        }
        /// <summary>
        /// Call this to take a screen shot in next late update
        /// </summary>
        public void TakeHiResShot()
        {
            m_takeShot = true;
            if (ScreenshotSavedManager.Instance != null)
            {
                ScreenshotSavedManager.Instance.ShowScreenshotSavedText();
            }
        }
        /// <summary>
        /// Assigns a name to the screen shot - by default puts in in the assets directory
        /// </summary>
        /// <param name="width">Width of screenshot</param>
        /// <param name="height">Height of screenshot</param>
        /// <returns>Screen shot name and full path</returns>
        private bool ScreenShotName(int width, int height, out string path)
        {
            path = "";
            if (Application.isEditor)
            { 
                path = Path.Combine(Application.dataPath, m_targetDirectory);
                path = path.Replace('\\', '/');

                if (path[path.Length - 1] == '/')
                {
                    path = path.Substring(0, path.Length - 1);
                }
            }
            else
            {
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), m_buildTargetDirectory);
                path = path.Replace('\\', '/');

                if (path[path.Length - 1] == '/')
                {
                    path = path.Substring(0, path.Length - 1);
                }
            }

            DirectoryInfo directory = Directory.GetParent(path);
            if (!directory.Exists)
            {
                return false;
            }

            m_lastSavedPath = path;
            if (m_lastSavedPath.Contains(Application.dataPath))
            {
                m_lastSavedPath = m_lastSavedPath.Replace(Application.dataPath, "Assets");
            }

            if (m_lastSavedPath.Length > m_maxSavePathCount)
            {
                m_lastSavedPath = m_lastSavedPath.Substring(m_maxSavePathCount, m_lastSavedPath.Length - m_maxSavePathCount);
            }

            switch (m_imageFormat)
            {
                case GaiaConstants.ImageFileType.Exr:
                    path = string.Format("{0}/Grab {1:yyyyMMddHHmmss} w{2}h{3} x{4}y{5}z{6}r{7}.exr",
                        path, System.DateTime.Now,
                        width,
                        height,
                        (int)m_mainCamera.transform.position.x,
                        (int)m_mainCamera.transform.position.y,
                        (int)m_mainCamera.transform.position.z,
                        (int)m_mainCamera.transform.rotation.eulerAngles.y
                    );
                    break;
                case GaiaConstants.ImageFileType.Png:
                    path =  string.Format("{0}/Grab {1:yyyyMMddHHmmss} w{2}h{3} x{4}y{5}z{6}r{7}.png",
                        path, System.DateTime.Now,
                        width,
                        height,
                        (int)m_mainCamera.transform.position.x,
                        (int)m_mainCamera.transform.position.y,
                        (int)m_mainCamera.transform.position.z,
                        (int)m_mainCamera.transform.rotation.eulerAngles.y
                    );
                    break;
                case GaiaConstants.ImageFileType.Tga:
                    path =  string.Format("{0}/Grab {1:yyyyMMddHHmmss} w{2}h{3} x{4}y{5}z{6}r{7}.tga",
                        path, System.DateTime.Now,
                        width,
                        height,
                        (int)m_mainCamera.transform.position.x,
                        (int)m_mainCamera.transform.position.y,
                        (int)m_mainCamera.transform.position.z,
                        (int)m_mainCamera.transform.rotation.eulerAngles.y
                    );
                    break;
                case GaiaConstants.ImageFileType.Jpg:
                    path =  string.Format("{0}/Grab {1:yyyyMMddHHmmss} w{2}h{3} x{4}y{5}z{6}r{7}.jpg",
                        path, System.DateTime.Now,
                        width,
                        height,
                        (int)m_mainCamera.transform.position.x,
                        (int)m_mainCamera.transform.position.y,
                        (int)m_mainCamera.transform.position.z,
                        (int)m_mainCamera.transform.rotation.eulerAngles.y
                    );
                    break;
            }

            return true;
        }
        /// <summary>
        /// Adds watermark the screenshot
        /// </summary>
        /// <param name="background"></param>
        /// <param name="watermark"></param>
        /// <returns></returns>
        private Texture2D AddWatermark(Texture2D background, Texture2D watermark)
        {
            int startX = background.width - watermark.width - 10;
            int endX = startX + watermark.width;
            //int startY = background.height - watermark.height - 20;
            int startY = 8;
            int endY = startY + watermark.height;

            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    Color bgColor = background.GetPixel(x, y);
                    Color wmColor = watermark.GetPixel(x - startX, y - startY);
                    Color final_color = Color.Lerp(bgColor, wmColor, wmColor.a / 1.0f);
                    background.SetPixel(x, y, final_color);
                }
            }

            background.Apply();
            return background;
        }
    }
}