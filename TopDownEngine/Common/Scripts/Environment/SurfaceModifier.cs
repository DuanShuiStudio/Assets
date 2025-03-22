using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此组件添加到平台，并定义将应用于在其上行走的任何TopDownController的新摩擦力或力
    /// 待办事项，仍在进行中。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Environment/Surface Modifier")]
	public class SurfaceModifier : TopDownMonoBehaviour 
	{
		[Header("Friction摩擦")]
		[MMInformation("将摩擦力设置为0.01到0.99之间，以获得光滑的表面（接近0表示非常光滑，接近1表示不太光滑）\n或者将其设置在1以上以获得粘性表面。数值越高，表面越粘", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// the amount of friction to apply to a TopDownController walking over this surface		
		[Tooltip("应用于在这个表面上行走的TopDownController的摩擦力量")]
		public float Friction;

		[Header("Force力")]
		[MMInformation("使用这些来向任何在这个表面接地的TopDownController添加X或Y（或两者）力。添加X力将创建一个磨盘（负值>磨盘向左，正值>磨盘向右）。正Y值将创建一个跳板、弹性表面或跳跃者等", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// the amount of force to add to a TopDownController walking over this surface
		[Tooltip("向在这个表面上行走的TopDownController添加的力量大小")]
		public Vector3 AddedForce=Vector3.zero;

        /// <summary>
        /// 当TopDownController与该表面碰撞时触发
        /// </summary>
        /// <param name="collider">Collider.</param>
        /*public virtual void OnTriggerStay2D(Collider2D collider)
		{
			TODO
		}*/
    }
}