using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Triggered_MaterialSwap : S_Triggered_Base, ITriggerable
{
	[SerializeField] MeshRenderer _Renderer;
	[SerializeField] MaterialsToSwap[] _MaterialSwapping;

	public override void ChildTriggerObjectOn ( S_CharacterTools Player = null ) {
		SwitchMaterials();
	}

	void SwitchMaterials () {
		for (int i = 0 ; i < _MaterialSwapping.Length ; i++)
		{
			if (!_Renderer) { return; }

			MaterialsToSwap thisSwap = _MaterialSwapping[i];
			var materialsCopy = _Renderer.sharedMaterials;

			if (thisSwap._onMaterialA)
			{
				materialsCopy[thisSwap._materialIndex] = thisSwap._toMaterial;
			}
			else
			{
				materialsCopy[thisSwap._materialIndex] = thisSwap._fromMaterial;
			}

			_Renderer.materials = materialsCopy;
			thisSwap._onMaterialA = !thisSwap._onMaterialA;
		}
	}

	public override void ChildResetToOriginal () {
		SwitchMaterials();
	}
}

[Serializable]
public class MaterialsToSwap
{
	[SerializeField] public int _materialIndex;
	[ColourIfNull(0.7f, 0, 0, 1)]
	[SerializeField] public Material _fromMaterial;
	[ColourIfNull(0.7f, 0, 0, 1)]
	[SerializeField] public Material _toMaterial;
	[NonSerialized] public bool _onMaterialA = true;
}
