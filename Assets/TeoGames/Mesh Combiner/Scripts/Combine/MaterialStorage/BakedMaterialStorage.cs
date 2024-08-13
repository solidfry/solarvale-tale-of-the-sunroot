using System;
using System.Collections.Generic;
using TeoGames.Mesh_Combiner.Scripts.Combine.CombinedMaterial;
using TeoGames.Mesh_Combiner.Scripts.Combine.CombinedMaterial.MaterialBake;
using TeoGames.Mesh_Combiner.Scripts.Extension;
using TeoGames.Mesh_Combiner.Scripts.Profile;
using UnityEngine;
using UnityEngine.Rendering;

namespace TeoGames.Mesh_Combiner.Scripts.Combine.MaterialStorage {
	using MaterialType = ValueTuple<MeshParser, BasicMaterial>;

	public class BakedMaterialStorage : AbstractMaterialStorage {
		private readonly Dictionary<long, MaterialType> _MaterialsResult = new Dictionary<long, MaterialType>();
		private readonly Dictionary<long, BasicMaterial> _Materials = new Dictionary<long, BasicMaterial>();

		public override IEnumerable<BasicMaterial> List => _Materials.Values;

		public override bool TryFind(long matID, out BasicMaterial mat) {
			var res = _MaterialsResult.TryGetValue(matID, out var tuple);
			mat = tuple.Item2;

			return res;
		}

		public override MaterialType Get(long mID, Material mat, int offset, ShadowCastingMode shadow, bool isStatic) {
			if (_MaterialsResult.TryGetValue(mID, out var res)) return res;

			var (bakedMat, parser) = mat
				? MaterialBaker.Instance.RegisterMaterial(mat)
				: (mat, null);

			var bakedId = bakedMat.GetCombineID(offset);
			if (!_Materials.TryGetValue(bakedId, out var material)) {
				material = GetMaterialInstance(shadow, isStatic, bakedMat);
				_Materials.Add(bakedId, material);
				ProfilerModule.TotalMaterials.Value++;
			}

			res = (parser, material);
			_MaterialsResult.Add(mID, res);

			return res;
		}
	}
}