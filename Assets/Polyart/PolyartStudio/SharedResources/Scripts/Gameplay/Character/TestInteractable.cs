using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Polyart
{
    public class TestInteractable : Interactable_Dreamscape
    {
        public override void OnFocus()
        {
            print("Looking at " + gameObject.name);
        }

        public override void OnInteract()
        {
            print("Interacted with " + gameObject.name);
        }

        public override void OnLoseFocus()
        {
            print("Stopped Looking at " + gameObject.name);
        }
    }
}