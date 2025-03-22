using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
	[AddComponentMenu("TopDown Engine/Managers/Multiplayer GUI Manager")]
	public class MultiplayerGUIManager : GUIManager
	{
		[Header("Multiplayer GUI多人游戏图形用户界面")]
		/// the HUD to display when in split screen mode
		[Tooltip("在分屏模式下显示的HUD（血条蓝条头像等）")]
		public GameObject SplitHUD;
		/// the HUD to display when in group camera mode
		[Tooltip("在组相机模式下显示的HUD（血条蓝条头像等）")]
		public GameObject GroupHUD;
		/// a UI object used to display the splitters UI images
		[Tooltip("用于显示分屏器UI图像的UI对象")]
		public GameObject SplittersGUI;
	}
}