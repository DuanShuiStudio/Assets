using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 使用这个类进行基于物理的抛射物（意味着要由ProjectileWeapon投掷）
    /// </summary>
    public class PhysicsProjectile : Projectile
	{
		[Header("Physics")] 
		public float InitialForce = 10f;
		public Vector3 InitialRotation = Vector3.zero;

		public ForceMode InitialForceMode = ForceMode.Impulse;
		public ForceMode2D InitialForceMode2D = ForceMode2D.Impulse;

		public override void Movement()
		{
			//do nothing
		}
		
		public override void SetDirection(Vector3 newDirection, Quaternion newRotation, bool spawnerIsFacingRight = true)
		{
			base.SetDirection(newDirection, newRotation, spawnerIsFacingRight);
			
			this.transform.Rotate(InitialRotation, Space.Self);

			newDirection = this.transform.forward;
			
			if (_rigidBody != null)
			{
				_rigidBody.AddForce(newDirection * InitialForce, InitialForceMode);	
			}
			if (_rigidBody2D != null)
			{
				_rigidBody2D.AddForce(newDirection * InitialForce, InitialForceMode2D);
			}
		}

        /// <summary>
        /// 根据状态将关联的刚体或刚体2D设置为运动学或非运动学
        /// </summary>
        /// <param name="state"></param>
        protected virtual void SetRigidbody(bool state)
		{
			if (_rigidBody != null)
			{
				_rigidBody.isKinematic = state;
			}

			if (_rigidBody2D != null)
			{
				_rigidBody2D.isKinematic = state;
			}
		}

        /// <summary>
        /// 在启用时，我们强制我们的刚体不是运动学的
        /// </summary>
        protected override void OnEnable()
		{
			base.OnEnable();
			SetRigidbody(false);
		}

        /// <summary>
        /// 在禁用时，我们强制我们的刚体成为运动学的，以消除任何剩余的速度
        /// </summary>
        protected override void OnDisable()
		{
			base.OnDisable();
			SetRigidbody(true);
		}
	}	
}