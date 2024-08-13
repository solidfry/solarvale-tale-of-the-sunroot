using UnityEditor;
using UnityEngine;

namespace TeoGames.Mesh_Combiner.Scripts.BlendShape.Editor {
	[CustomEditor(typeof(BlendShapeSync), true)]
	public class BlendShapeSyncEditor : UnityEditor.Editor {
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();

			var sync = (BlendShapeSync)target;
			GUILayout.Label($"Sync {sync.Map.Count} shapes");
		}
	}
}