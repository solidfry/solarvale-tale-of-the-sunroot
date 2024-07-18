using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace OccaSoftware.Buto.Runtime
{
    [ExecuteAlways]
    [RequireComponent(typeof(WindZone))]
    [AddComponentMenu("OccaSoftware/Buto/Wind Zone")]
    public class ButoWindZone : MonoBehaviour
    {
        private void OnEnable()
        {
            WindZone windZone = GetComponent<WindZone>();
            WindOffsetHandler.SetWindZone(windZone);
        }

        private void OnDisable()
        {
            if (WindOffsetHandler.windZone == GetComponent<WindZone>())
            {
                WindOffsetHandler.ClearWindZone();
            }
        }
    }
}
