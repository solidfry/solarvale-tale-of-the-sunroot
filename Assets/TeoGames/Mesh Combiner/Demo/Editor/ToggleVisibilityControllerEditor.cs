using TeoGames.Mesh_Combiner.Demo.Scripts;
using TeoGames.Mesh_Combiner.Scripts.Extension;
using UnityEditor;
using UnityEngine;

namespace TeoGames.Mesh_Combiner.Demo.Editor {
	[CustomEditor(typeof(ToggleVisibilityController), true)]
	[CanEditMultipleObjects]
	public class ToggleVisibilityControllerEditor : UnityEditor.Editor {
		public void OnSceneGUI() {
			var obj = (ToggleVisibilityController)target;
			obj.list.ForEach(
				i => {
					Handles.color = i.IsVisible ? Color.green : Color.red;
					var isCLicked = Handles.Button(
						i.transform.position + Vector3.up * 10,
						Quaternion.identity,
						1,
						2,
						Handles.SphereHandleCap
					);

					if (isCLicked) i.IsVisible = !i.IsVisible;
				}
			);
		}
	}
}