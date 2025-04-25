using System;
using UnityEngine;
using SplineMesh;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
[SelectionBase]
[DisallowMultipleComponent]
public class S_Trigger_Path : S_Trigger_Base
{
	[Header("Trigger")]
	public bool _isExit_;
	public S_Trigger_Path _ExternalPathData_;

	public StrucAutoPathData _PathData_ = new StrucAutoPathData ()
	{
		_speedLimits_ = new Vector2(30, 200),
		_canPlayerReverse_ = false,
		_canPlayerSlow_ = false,
		_lockPlayerFor_ = 40,
		_removeVerticalVelocityOnStart_ = true,
	};

	[Serializable]
	public struct StrucAutoPathData {
		public Spline spline;
		public Vector2 _speedLimits_;
		public bool         _canPlayerReverse_;
		public bool         _canPlayerSlow_ ;
		public int          _lockPlayerFor_;
		public bool         _removeVerticalVelocityOnStart_;
	}


	private void OnEnable () {
		_PathData_.spline = GetComponentInParent<Spline>();
	}

}
