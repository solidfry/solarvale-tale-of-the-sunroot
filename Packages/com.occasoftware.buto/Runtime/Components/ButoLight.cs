using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace OccaSoftware.Buto.Runtime
{
  [ExecuteAlways]
  [AddComponentMenu("OccaSoftware/Buto/Buto Light")]
  [RequireComponent(typeof(Light))]
  public sealed class ButoLight : ButoPlaceableObject
  {
    [SerializeField]
    private bool inheritDataFromLightComponent;

    [SerializeField]
    private Light lightComponent;

    [SerializeField]
    private Vector4 areaSizeCached;

    public Vector4 GetAreaSize()
    {
#if UNITY_EDITOR
      areaSizeCached = new Vector4(lightComponent.areaSize.x, lightComponent.areaSize.y, 0, 0);
#endif
      return areaSizeCached;
    }

    [SerializeField]
    [ColorUsage(false, false)]
    private Color lightColor = Color.white;

    public Vector4 LightColor
    {
      get
      {
        if (inheritDataFromLightComponent && lightComponent != null)
          return lightComponent.color;

        return lightColor;
      }
    }

    [SerializeField]
    [Min(0)]
    private float bias;

    [SerializeField]
    [Min(0)]
    private float lightRange;

    public float LightRange
    {
      get
      {
        if (inheritDataFromLightComponent && lightComponent != null)
          return lightComponent.range;

        return lightRange;
      }
    }

    [SerializeField]
    [Min(0)]
    private float lightIntensity = 10f;
    public float LightIntensity
    {
      get
      {
        if (inheritDataFromLightComponent && lightComponent != null)
          return lightComponent.intensity;

        return lightIntensity;
      }
    }

    public Vector4 LightDirection
    {
      get { return -transform.localToWorldMatrix.GetColumn(2); }
    }

    public Vector4 LightPosition
    {
      get
      {
        return new Vector4(
          transform.position.x,
          transform.position.y,
          transform.position.z,
          (int)lightComponent.type
        );
      }
    }

    LightAngle lightAngleData;
    public LightAngle LightAngleData
    {
      get
      {
        if (lightAngleData == null)
        {
          lightAngleData = new LightAngle(
            lightComponent.type,
            lightComponent.innerSpotAngle,
            lightComponent.spotAngle,
            LightRange,
            bias
          );
        }
        return lightAngleData;
      }
    }

    public class LightAngle
    {
      private LightType type;
      public LightType Type
      {
        get { return type; }
        set
        {
          if (type != value)
          {
            type = value;
            RecalculateShaderData();
          }
        }
      }

      private float innerSpotAngle;
      public float InnerSpotAngle
      {
        get { return innerSpotAngle; }
        set
        {
          if (innerSpotAngle != value)
          {
            innerSpotAngle = value;
            RecalculateShaderData();
          }
        }
      }
      private float spotAngle;
      public float SpotAngle
      {
        get { return spotAngle; }
        set
        {
          if (spotAngle != value)
          {
            spotAngle = value;
            RecalculateShaderData();
          }
        }
      }

      private float bias;

      public float Bias
      {
        get { return bias; }
        set
        {
          if (bias != value)
          {
            bias = value;
            SqrBias = value * value;
          }
        }
      }

      private float SqrBias;
      private float range;
      public float Range
      {
        get { return range; }
        set
        {
          if (range != value)
          {
            range = value;
            InverseSquareRange = 1.0f / (range * range);
          }
        }
      }
      public float InverseSquareRange;

      private Vector2 shaderLightAngle;

      public Vector4 GetAngleData()
      {
        return new Vector4(shaderLightAngle.x, shaderLightAngle.y, SqrBias, InverseSquareRange);
      }

      public LightAngle(
        LightType type,
        float innerSpotAngle,
        float spotAngle,
        float range,
        float bias
      )
      {
        this.type = type;
        this.innerSpotAngle = innerSpotAngle;
        this.spotAngle = spotAngle;
        this.Range = range;
        this.Bias = bias;

        RecalculateShaderData();
      }

      private void RecalculateShaderData()
      {
        shaderLightAngle = CalculateLightAngles();
        SqrBias = Bias * Bias;
      }

      private Vector2 CalculateLightAngles()
      {
        if (type != LightType.Spot)
          return new Vector2(0f, 1f);

        float innerHalfRads = Mathf.Cos(0.5f * innerSpotAngle * Mathf.Deg2Rad);
        float outerHalfRads = Mathf.Cos(0.5f * spotAngle * Mathf.Deg2Rad);
        float mapped = 1.0f / Mathf.Max(innerHalfRads - outerHalfRads, 0.001f);
        return new Vector2(mapped, mapped * -outerHalfRads);
      }
    }

    public static void SortByDistance(Vector3 c)
    {
      _Lights = _Lights.OrderBy(x => x.GetSqrMagnitude(c)).ToList();
    }

    private static List<ButoLight> _Lights = new List<ButoLight>();
    public static List<ButoLight> Lights
    {
      get { return _Lights; }
    }

    protected override void Reset()
    {
      ButoCommon.CheckMaxLightCount(Lights.Count, this);
    }

    private void OnValidate()
    {
      lightComponent = GetComponent<Light>();
      lightAngleData = new LightAngle(
        lightComponent.type,
        lightComponent.innerSpotAngle,
        lightComponent.spotAngle,
        LightRange,
        bias
      );
    }

    protected override void OnEnable()
    {
      lightComponent = GetComponent<Light>();
      _Lights.Add(this);
    }

    private void Update()
    {
      LightAngleData.Type = lightComponent.type;
      LightAngleData.Range = LightRange;
      LightAngleData.Bias = bias;
      LightAngleData.InnerSpotAngle = lightComponent.innerSpotAngle;
      LightAngleData.SpotAngle = lightComponent.spotAngle;
    }

    protected override void OnDisable()
    {
      _Lights.Remove(this);
    }

    public void CheckForLight()
    {
      lightComponent = GetComponent<Light>();
    }

    /// <summary>
    /// To make the light inherit the light component properties, set to true.
    /// </summary>
    /// <param name="state"></param>
    public void SetInheritance(bool state)
    {
      inheritDataFromLightComponent = state;
    }

    /// <summary>
    /// If you change the light type or angles during runtime, you should set the light dirty so that it recalculates some internal properties.
    /// </summary>
    public void SetDirty()
    {
      lightAngleData = new LightAngle(
        lightComponent.type,
        lightComponent.innerSpotAngle,
        lightComponent.spotAngle,
        LightRange,
        bias
      );
    }

    #region Editor
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
      if (!inheritDataFromLightComponent)
      {
        Gizmos.color = LightColor;
        if (lightComponent.type == LightType.Point)
        {
          Gizmos.DrawWireSphere(transform.position, lightRange);
        }
        else
        {
          Gizmos.DrawRay(transform.position, transform.forward * lightRange);
        }
      }
    }
#endif
    #endregion
  }
}
