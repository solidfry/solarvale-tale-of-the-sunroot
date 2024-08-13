using System;
using TeoGames.Mesh_Combiner.Scripts.Combine.Interfaces;
using UnityEngine;

namespace TeoGames.Mesh_Combiner.Scripts.Combine {
	public partial class MeshCombiner : IRenderListener {
		public Action<Renderer[]> OnRenderersUpdated { get; set; }
	}
}