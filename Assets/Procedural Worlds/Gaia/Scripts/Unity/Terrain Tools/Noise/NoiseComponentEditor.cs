#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Gaia
{
    [CustomEditor(typeof(NoiseComponent))]
    public class NoiseComponentEditor : Editor
    {

    }
}
#endif