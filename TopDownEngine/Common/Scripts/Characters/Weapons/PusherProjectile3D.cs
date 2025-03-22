using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此组件添加到一个3D的弹药上，它将能够推动物体（例如开门）
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/Pusher Projectile 3D")]
	public class PusherProjectile3D : TopDownMonoBehaviour
	{
        /// 碰撞时施加的力量大小
        [Tooltip("碰撞时施加的力量大小")]
		public float PushPower = 10f;
		/// an offset to apply on the projectile's forward to account for super high speeds. This will affect the position the force is applied at. Usually 0 will be fine
		[Tooltip("在弹药的前进方向上应用的偏移量，以补偿超高速度。这会影响力量施加的位置。通常0就可以")]
		public float PositionOffset = 0f;

		protected Rigidbody _pushedRigidbody;
		protected Projectile _projectile;
		protected Vector3 _pushDirection;

        /// <summary>
        /// 在Awake时，我们获取我们的弹药组件
        /// </summary>
        protected virtual void Awake()
		{
			_projectile = this.gameObject.GetComponent<Projectile>();
		}

        /// <summary>
        /// 当我们的弹药与某物碰撞时，如果它具有刚体，我们就对其施加指定的力量
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void OnTriggerEnter(Collider collider)
		{
			_pushedRigidbody = collider.attachedRigidbody;

			if ((_pushedRigidbody == null) || (_pushedRigidbody.isKinematic))
			{
				return;
			}

			_pushDirection.x = _projectile.Direction.x;
			_pushDirection.y = 0;
			_pushDirection.z = _projectile.Direction.z;

			_pushedRigidbody.AddForceAtPosition(_pushDirection.normalized * PushPower, this.transform.position + this.transform.forward * PositionOffset);
		}		
	}
}