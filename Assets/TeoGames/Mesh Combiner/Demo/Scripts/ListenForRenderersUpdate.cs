using TeoGames.Mesh_Combiner.Scripts.Combine;
using TeoGames.Mesh_Combiner.Scripts.Combine.Interfaces;
using UnityEngine;

namespace TeoGames.Mesh_Combiner.Demo.Scripts {
	[AddComponentMenu("Mesh Combiner/Demo/MC Listen for renderers update")]
	public class ListenForRenderersUpdate : MonoBehaviour {
		[SerializeField] private AbstractMeshCombiner combiner;

		private void Awake() {
			if (combiner is not IRenderListener listener) return;

			listener.OnRenderersUpdated += list => Debug.LogError($"{combiner.name} > {list.Length}");
		}
	}
}