using System;
using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.UI;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此类添加到武器中，它将在接近障碍物（由ObstacleLayerMask定义）时防止射击
    /// </summary>
    [RequireComponent(typeof(Weapon))]
	[AddComponentMenu("TopDown Engine/Weapons/Weapon Prevent Shooting when Close to Walls 2D")]
	public class WeaponPreventShootingWhenCloseToWalls2D : WeaponPreventShooting
	{
		/// the angle to consider when deciding whether or not there's a wall in front of the weapon (usually 5 degrees is fine)
		[Tooltip("在决定武器前方是否有墙壁时要考虑的角度（通常5度就足够了）。")]
		public float Angle = 5f;
		/// the max distance to the wall we want to prevent shooting from
		[Tooltip("我们想阻止射击的、到墙壁的最大距离")]
		public float Distance = 2f;
		/// the offset to apply to the detection (in addition and relative to the weapon's position)
		[Tooltip("要应用于检测的偏移量（除了相对于武器位置的偏移量之外）。")]
		public Vector3 RaycastOriginOffset = Vector3.zero;
		/// the layers to consider as obstacles
		[Tooltip("要考虑为障碍物的图层。")]
		public LayerMask ObstacleLayerMask = LayerManager.ObstaclesLayerMask;
        
		protected RaycastHit2D _hitLeft;
		protected RaycastHit2D _hitMiddle;
		protected RaycastHit2D _hitRight;
		protected WeaponAim _weaponAim;

        /// <summary>
        /// 在唤醒（On Awake）时，我们获取我们的武器。
        /// </summary>
        protected virtual void Awake()
		{
			_weaponAim = this.GetComponent<WeaponAim>();
		}

        /// <summary>
        /// 在武器前方投射射线以检查是否有障碍物。
        /// 如果找到障碍物，则返回true
        /// </summary>
        /// <returns></returns>
        protected virtual bool CheckForObstacles()
		{
			_hitLeft = MMDebug.RayCast(this.transform.position + _weaponAim.CurrentRotation * RaycastOriginOffset, (Quaternion.Euler(0f, 0f, -Angle / 2f) * _weaponAim.CurrentAimAbsolute).normalized, Distance, ObstacleLayerMask, Color.yellow, true);
			_hitMiddle = MMDebug.RayCast(this.transform.position + _weaponAim.CurrentRotation * RaycastOriginOffset, _weaponAim.CurrentAimAbsolute.normalized, Distance, ObstacleLayerMask, Color.yellow, true);
			_hitRight = MMDebug.RayCast(this.transform.position + _weaponAim.CurrentRotation * RaycastOriginOffset, (Quaternion.Euler(0f, 0f, Angle / 2f) * _weaponAim.CurrentAimAbsolute).normalized, Distance, ObstacleLayerMask, Color.yellow, true);

			if ((_hitLeft.collider == null) && (_hitMiddle.collider == null) && (_hitRight.collider == null))
			{
				return false;
			}
			else
			{
				return true;
			}
		}

        /// <summary>
        /// 如果武器前方没有障碍物，则允许射击
        /// </summary>
        /// <returns></returns>
        public override bool ShootingAllowed()
		{
			return !CheckForObstacles();
		}
	}
}