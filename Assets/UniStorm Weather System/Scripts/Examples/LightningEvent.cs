using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniStorm;

namespace UniStorm.Example
{
    public class LightningEvent : MonoBehaviour
    {
        void Start()
        {
            UniStormSystem.Instance.OnLightningStrikeObjectEvent.AddListener(() => TestLightningEvent());
        }

        //Debug logs the name of the successfully struck object 
        void TestLightningEvent()
        {
            if (UniStormSystem.Instance.LightningStruckObject != null)
            {
                Debug.Log(UniStormSystem.Instance.LightningStruckObject.name);
            }
        }
    }
}