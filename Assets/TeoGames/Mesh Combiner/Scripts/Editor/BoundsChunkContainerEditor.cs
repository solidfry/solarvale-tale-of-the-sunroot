using TeoGames.Mesh_Combiner.Scripts.Combine.ChunkContainer;
using UnityEditor;
using UnityEngine;

namespace TeoGames.Mesh_Combiner.Scripts.Editor {
	[CustomEditor(typeof(BoundsChunkContainer), true)]
	[CanEditMultipleObjects]
	public class BoundsChunkContainerEditor : AbstractChunkContainerEditor {
		private bool _ShowLiveSync;

		public void OnSceneGUI() {
			var chunk = (BoundsChunkContainer)target;
			var pos = chunk.transform.position + chunk.bounds.center;

			EditorGUI.BeginChangeCheck();

			Handles.color = Color.green;
			Handles.DrawWireCube(pos, chunk.bounds.size);

			Handles.color = Handles.xAxisColor;
			var bounds = chunk.bounds;
			bounds = DrawHandle(pos, Vector3.right, bounds);
			bounds = DrawHandle(pos, Vector3.left, bounds);

			Handles.color = Handles.yAxisColor;
			bounds = DrawHandle(pos, Vector3.up, bounds);
			bounds = DrawHandle(pos, Vector3.down, bounds);

			Handles.color = Handles.zAxisColor;
			bounds = DrawHandle(pos, Vector3.forward, bounds);
			bounds = DrawHandle(pos, Vector3.back, bounds);

			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(chunk, $"Change bounds size at {chunk.name}");
				chunk.bounds = bounds;
			}
		}

		private Bounds DrawHandle(Vector3 position, Vector3 direction, Bounds bounds) {
			var pos = position + Vector3.Scale(direction, bounds.extents);
			var size = HandleUtility.GetHandleSize(pos) * .04f;
			var dif = Handles.Slider(pos, direction, size, Handles.DotHandleCap, 0) - pos;

			if (dif.magnitude > 0) {
				bounds.center += dif / 2;
				bounds.size += Vector3.Scale(dif, direction);
			}

			return bounds;
		}
	}
}