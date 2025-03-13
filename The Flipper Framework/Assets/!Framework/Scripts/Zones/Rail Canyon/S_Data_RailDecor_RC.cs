using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Data_RailDecor_RC : S_Data_Base
{
	[SerializeField, DrawTickBoxBefore("_showUnderPlatform"), Delayed]
	private string _underPlatform = "Ob_RailUnderPlatform_RC";
	[SerializeField, HideInInspector] bool _showUnderPlatform;

	[SerializeField, DrawTickBoxBefore("_showSignPostLeft"), Delayed]
	private string _leftSignPost = "Ob_Signpost_RC Left";
	[SerializeField, HideInInspector] bool _showSignPostLeft;

	[SerializeField, DrawTickBoxBefore("_showSignPostRight"), Delayed]
	private string _rightSignPost = "Ob_Signpost_RC Right";
	[SerializeField, HideInInspector] bool _showSignPostRight;

	[SerializeField, DrawTickBoxBefore("_showLeftRailWall"), Delayed]
	private string _leftRailWall = "Ob_RailSideDecorator_RC Left";
	[SerializeField, HideInInspector] bool _showLeftRailWall;

	[SerializeField,DrawTickBoxBefore("_showRightRailWall"), Delayed]
	private string _rightRailWall = "Ob_RailSideDecorator_RC Right";
	[SerializeField, HideInInspector] bool _showRightRailWall;

	[SerializeField,DrawTickBoxBefore("_showLeftRailDirector"), Delayed]
	private string _leftRailDirector = "Ob_SideDecor_RC Left";
	[SerializeField, HideInInspector] bool _showLeftRailDirector;

	[SerializeField,DrawTickBoxBefore("_showRightRailDirector"), Delayed]
	private string _rightRailDirector = "Ob_SideDecor_RC Right";
	[SerializeField, HideInInspector] bool _showRightRailDirector;

	[SerializeField,DrawTickBoxBefore("_showUnderClamps"), Delayed]
	private string _underClamps = "Ob_WallClamps2_RC";
	[SerializeField, HideInInspector] bool _showUnderClamps;

	private void OnValidate () {
		S_S_Editor.FindObjectAndSetActive(_underPlatform, _showUnderPlatform, transform);
		S_S_Editor.FindObjectAndSetActive(_leftSignPost, _showSignPostLeft, transform);
		S_S_Editor.FindObjectAndSetActive(_rightSignPost, _showSignPostRight, transform);
		S_S_Editor.FindObjectAndSetActive(_leftRailWall, _showLeftRailWall, transform);
		S_S_Editor.FindObjectAndSetActive(_rightRailWall, _showRightRailWall, transform);
		S_S_Editor.FindObjectAndSetActive(_leftRailDirector, _showLeftRailDirector, transform);
		S_S_Editor.FindObjectAndSetActive(_rightRailDirector, _showRightRailDirector, transform);
		S_S_Editor.FindObjectAndSetActive(_underClamps, _showUnderClamps, transform);
	}
}
