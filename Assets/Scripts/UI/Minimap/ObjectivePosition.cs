using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UI.Minimap
{
    public class ObjectivePosition : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            FindObjectOfType<QuestIconManager>().AddObjectiveMarker(this);
        }
    }
}
