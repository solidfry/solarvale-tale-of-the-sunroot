using System;
using UnityEngine;

namespace TeoGames.Mesh_Combiner.Scripts.BlendShape {
	[Serializable]
	public class BlendShapeFrame {
		public float weight;

		public Vector3[] vertices;
		public Vector3[] normals;
		public Vector3[] tangents;
	}
}