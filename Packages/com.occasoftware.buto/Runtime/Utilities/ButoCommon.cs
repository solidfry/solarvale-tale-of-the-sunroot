using UnityEngine;

namespace OccaSoftware.Buto.Runtime
{
  internal static class ButoCommon
  {
    internal static readonly int _MAXLIGHTCOUNT = 16;

    internal static void CheckMaxLightCount(int c, Object o)
    {
      if (c > _MAXLIGHTCOUNT)
        Debug.LogWarning(
          $"You have more than {_MAXLIGHTCOUNT} Buto Lights in scene. Buto will cull all but the nearest {_MAXLIGHTCOUNT} lights, but this will incur additional overhead.",
          o
        );
    }

    internal static readonly int _MAXVOLUMECOUNT = 8;

    internal static void CheckMaxFogVolumeCount(int c, Object o)
    {
      if (c > _MAXVOLUMECOUNT)
        Debug.LogWarning(
          $"You have more than {_MAXVOLUMECOUNT} Buto Fog Volumes in scene. Buto will cull all but the nearest {_MAXVOLUMECOUNT} fog volumes, but this will incur additional overhead.",
          o
        );
    }
  }
}

public static class ExtensionMethods
{
  public static Vector3 Abs(this Vector3 v)
  {
    return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
  }
}
