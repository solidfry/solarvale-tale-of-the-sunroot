using System;
using UnityEngine;

namespace TeoGames.Mesh_Combiner.Scripts.Combine.Interfaces {
	public interface IRenderListener {
		public Action<Renderer[]> OnRenderersUpdated { get; set; }
	}
}