using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Polyart
{
    public class MillRotate : MonoBehaviour
    {
        private float x;
        public float rotationSpeed = 25.0f;
        private float y;
        private float z;
        // Start is called before the first frame update
        void Start()
        {
            x = 0;
            y = transform.rotation.y;
            z = transform.rotation.z;
        }

        // Update is called once per frame
        private void FixedUpdate()
        {
            x += Time.deltaTime * rotationSpeed;
            if (x > 360.0f)
            {
                x = 0.0f;
            }
            transform.localRotation = Quaternion.Euler(x, y, z);
        }
    }
}
