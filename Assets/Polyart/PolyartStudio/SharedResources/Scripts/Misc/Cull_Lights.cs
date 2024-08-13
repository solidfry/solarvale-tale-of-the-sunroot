using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Polyart
{

    public class Cull_Lights : MonoBehaviour
    {
        [SerializeField] private float lightCullDistance;
        [SerializeField] private float shadowCullDistance;
        [SerializeField] private bool shadowsEnabled;

        [SerializeField] private GameObject playerController;
        private Light lightSource;


        private void Awake()
        {
            lightSource = gameObject.GetComponent<Light>();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            float playerDistance = Vector3.Distance(playerController.transform.position, gameObject.transform.position);

            if (playerDistance <= lightCullDistance)
            {
                lightSource.enabled = true;
                if (playerDistance <= shadowCullDistance && shadowsEnabled == true)
                {
                    lightSource.shadows = LightShadows.Soft;
                }
                else
                {
                    lightSource.shadows = LightShadows.None;
                }
            }
            else
            {
                lightSource.enabled = false;
            }

        }
    }
}
