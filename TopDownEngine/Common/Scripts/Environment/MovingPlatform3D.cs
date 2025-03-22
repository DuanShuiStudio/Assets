using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 一个在3D空间中处理移动平台的类，该平台沿着一组节点移动
    /// </summary>
    [AddComponentMenu("TopDown Engine/Environment/Moving Platform 3D")]
	public class MovingPlatform3D : MMPathMovement
	{
		/// The force to apply when pushing a character that'd be in the way of the moving platform
		[Tooltip("当移动平台会遇到障碍物（角色）时，施加的推力。")]
		public float PushForce = 5f;       
	}
}