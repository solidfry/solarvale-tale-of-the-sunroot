using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShaderFloatPropertyOverride : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] string shaderProperty = "_UnscaledTime";
    [SerializeField] bool log = false;

    private float value;

    private void Start()
    {
        value = image.material.GetFloat( shaderProperty);
    }

    private void Update()
    {
        if (image.material == null) return;
        image.material.SetFloat(shaderProperty, Time.unscaledTime);
        
        Log();
    }

    private void Log()
    {
        if (!log) return;
        Debug.Log(" Time.unscaledTime: " + Time.unscaledTime + " is being set to " + image.material.name + " shader property: " + shaderProperty + " on " + image.name);
    }
    
    private void OnDestroy()
    {
        image.material.SetFloat(shaderProperty, value);
    }
}
