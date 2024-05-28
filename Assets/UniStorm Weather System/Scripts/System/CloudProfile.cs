using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniStorm
{
    [CreateAssetMenu(fileName = "New Cloud Profile", menuName = "UniStorm/New Cloud Profile")]
    public class CloudProfile : ScriptableObject
    {
        public string ProfileName = "New Cloud Profile Name";
        public float EdgeSoftness = 0.067f;
        public float BaseSoftness = 0.13f;
        public float DetailStrength = 0.114f;
        public float Density = 0.9f;
        public float CoverageBias = 0.0175f;
        public int DetailScale = 730;
    }
}