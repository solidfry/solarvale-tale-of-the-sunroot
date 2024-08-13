using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Polyart
{
    public abstract class Interactable_Dreamscape : MonoBehaviour
    {

        public virtual void Awake()
        {
            gameObject.layer = 7;
        }
        public abstract void OnInteract();
        public abstract void OnFocus();
        public abstract void OnLoseFocus();

    }
}