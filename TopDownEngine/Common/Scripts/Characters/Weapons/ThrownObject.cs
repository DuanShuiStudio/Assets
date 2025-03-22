using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 一种投掷型弹药，适用于手榴弹等
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
	[AddComponentMenu("TopDown Engine/Weapons/Thrown Object")]
	public class ThrownObject : Projectile 
	{
		protected Vector2 _throwingForce;
		protected bool _forceApplied = false;

        /// <summary>
        /// 在初始化时，我们获取我们的刚体
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			_rigidBody2D = this.GetComponent<Rigidbody2D>();
		}

        /// <summary>
        /// 在启用时，我们重置对象的速度
        /// </summary>
        protected override void OnEnable()
		{
			base.OnEnable();
			_forceApplied = false;
		}

        /// <summary>
        /// 每帧处理弹药的运动
        /// </summary>
        public override void Movement()
		{
			if (!_forceApplied && (Direction != Vector3.zero))
			{
				_throwingForce = Direction * Speed;
				_rigidBody2D.AddForce (_throwingForce);
				_forceApplied = true;
			}
		}
	}
}