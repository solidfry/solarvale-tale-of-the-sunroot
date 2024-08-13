using TeoGames.Mesh_Combiner.Scripts.Combine.CombinedMaterial.MaterialBake;
using UnityEngine;

namespace TeoGames.Mesh_Combiner.Demo.Scripts {
	[AddComponentMenu("Mesh Combiner/Demo/MC Force Material Baker Feature")]
	public class ForceEnableBaker : MonoBehaviour {
		[SerializeField] private AbstractShaderBaker baker;

		private void Awake() => Instantiate(baker).Inject(MaterialBaker.Instance);
	}
}