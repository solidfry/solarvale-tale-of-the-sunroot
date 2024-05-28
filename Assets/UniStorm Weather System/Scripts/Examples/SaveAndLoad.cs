using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniStorm.Example
{
    /// <summary>
    /// A simple save system that allows users to save UniStorm's time, date, weather, and player position.
    /// </summary>
    public class SaveAndLoad : MonoBehaviour
    {
        public SaveTypeEnum SaveType = SaveTypeEnum.Manual;
        public enum SaveTypeEnum {Manual, Auto};
        public LoadOnStartEnum LoadOnStart = LoadOnStartEnum.Enabled;
        public enum LoadOnStartEnum { Enabled, Disabled };
        public DebugLogsEnum DebugLogs = DebugLogsEnum.Enabled;
        public enum DebugLogsEnum { Enabled, Disabled };
        public int AutoSaveSeconds = 10;
        public Transform PlayerTransform;
        public Transform PlayerCamera;
        float m_AutoSaveTimer = 0;

        void Start()
        {
            if (PlayerPrefs.HasKey("UniStorm Player Position") && LoadOnStart == LoadOnStartEnum.Enabled)
            {
                StartCoroutine(LoadAutoSavedData());
            }
        }

        void Update()
        {
            m_AutoSaveTimer += Time.deltaTime;

            //Save player data
            if (Input.GetKeyDown(KeyCode.T) && SaveType == SaveTypeEnum.Manual || m_AutoSaveTimer >= AutoSaveSeconds && SaveType == SaveTypeEnum.Auto)
            {
                PlayerPrefs.SetInt("UniStorm Hour", UniStormSystem.Instance.Hour);
                PlayerPrefs.SetInt("UniStorm Minute", UniStormSystem.Instance.Minute);
                PlayerPrefs.SetInt("UniStorm Temperature", UniStormSystem.Instance.Temperature);
                PlayerPrefs.SetString("UniStorm Weather", UniStormSystem.Instance.CurrentWeatherType.WeatherTypeName);
                PlayerPrefs.SetInt("UniStorm Month", UniStormSystem.Instance.Month);
                PlayerPrefs.SetInt("UniStorm Day", UniStormSystem.Instance.Day);
                PlayerPrefs.SetInt("UniStorm Year", UniStormSystem.Instance.Year);

                PlayerPrefs.SetString("UniStorm Player Position", PlayerTransform.position.ToString());
                PlayerPrefs.SetString("UniStorm Player Rotation", PlayerTransform.eulerAngles.ToString());
                PlayerPrefs.SetString("UniStorm Camera Rotation", PlayerCamera.eulerAngles.ToString());
                m_AutoSaveTimer = 0;

                if (DebugLogs == DebugLogsEnum.Enabled)
                {
                    Debug.Log("Data Saved: UniStorm Time: " + UniStormSystem.Instance.Hour.ToString() + ":" + UniStormSystem.Instance.Minute.ToString("00") + 
                        " - UniStorm Weather: " + UniStormSystem.Instance.CurrentWeatherType.WeatherTypeName + " - UniStorm Temperature: " + UniStormSystem.Instance.Temperature.ToString() + "°" + " " +
                        "- UniStorm Date: " + UniStormSystem.Instance.Month.ToString() + "/" + UniStormSystem.Instance.Day.ToString() + "/" + UniStormSystem.Instance.Year.ToString());
                }
            }

            //Load player data
            if (Input.GetKeyDown(KeyCode.Y) && SaveType == SaveTypeEnum.Manual)
            {
                LoadData();
            }
        }

        void LoadData ()
        {
            if (UniStormSystem.Instance.UniStormInitialized && PlayerPrefs.HasKey("UniStorm Player Position"))
            {
                UniStormManager.Instance.SetTime(PlayerPrefs.GetInt("UniStorm Hour"), PlayerPrefs.GetInt("UniStorm Minute"));
                UniStormSystem.Instance.Temperature = PlayerPrefs.GetInt("UniStorm Temperature");
                UniStormManager.Instance.SetDate(PlayerPrefs.GetInt("UniStorm Month"), PlayerPrefs.GetInt("UniStorm Day"), PlayerPrefs.GetInt("UniStorm Year"));

                PlayerTransform.position = StringToVector3(PlayerPrefs.GetString("UniStorm Player Position"));
                PlayerTransform.eulerAngles = StringToVector3(PlayerPrefs.GetString("UniStorm Player Rotation"));
                PlayerCamera.eulerAngles = StringToVector3(PlayerPrefs.GetString("UniStorm Camera Rotation"));

                string LoadedWeatherTypeName = PlayerPrefs.GetString("UniStorm Weather");
                //To allow weather to be assigned using a string, look through each weather type for a matching Weather Type Name
                foreach (WeatherType WT in UniStormSystem.Instance.AllWeatherTypes.ToArray())
                {
                    if (WT.WeatherTypeName == LoadedWeatherTypeName)
                    {
                        //Apply the weather type to UniStorm once found
                        UniStormManager.Instance.ChangeWeatherInstantly(WT);
                    }
                }

                if (DebugLogs == DebugLogsEnum.Enabled)
                {
                    Debug.Log("Data Loaded");
                }
            }
        }

        IEnumerator LoadAutoSavedData ()
        {
            yield return new WaitUntil(() => UniStormSystem.Instance.UniStormInitialized);
            LoadData();
        }

        public static Vector3 StringToVector3(string StringVector)
        {
            //Remove the parentheses within the Vector3
            if (StringVector.StartsWith("(") && StringVector.EndsWith(")"))
            {
                StringVector = StringVector.Substring(1, StringVector.Length - 2);
            }

            //Separate each value
            string[] VectorArray = StringVector.Split(',');

            //Store each value to the Cector3
            Vector3 ConvertedVector = new Vector3(
                float.Parse(VectorArray[0]),
                float.Parse(VectorArray[1]),
                float.Parse(VectorArray[2]));

            return ConvertedVector;
        }
    }
}