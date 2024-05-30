using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    public float moveZ;

    public void OnMove(InputAction.CallbackContext context)
    {
        moveZ = context.ReadValue<float>();
    }
        


        


}
