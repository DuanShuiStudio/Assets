using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 这个AI动作将让具有CharacterRotation2D能力（设置为ForcedRotation:true）的代理旋转到面对其目标
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Rotate Towards Target 2D")]
	//[RequireComponent(typeof(CharacterRotation2D))]
	public class AIActionRotateTowardsTarget2D : AIAction
	{
		[Header("Lock Rotation锁定旋转")]
		/// whether or not to lock the X rotation. If set to false, the model will rotate on the x axis, to aim up or down 
		[Tooltip("是否锁定X轴旋转。如果设置为false，模型将在X轴上旋转，以向上或向下瞄准")]
		public bool LockRotationX = false;

		protected CharacterRotation2D _characterRotation2D;
		protected Vector3 _targetPosition;

        /// <summary>
        /// 在init中，我们获取CharacterOrientation3D能力
        /// </summary>
        public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			_characterRotation2D = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterRotation2D>();
		}

        /// <summary>
        /// 在PerformAction上我们移动
        /// </summary>
        public override void PerformAction()
		{
			Rotate();
		}

        /// <summary>
        /// 使定向3D能力向大脑目标旋转
        /// </summary>
        protected virtual void Rotate()
		{
			if (_brain.Target == null)
			{
				return;
			}
			_targetPosition = _brain.Target.transform.position;
			if (LockRotationX)
			{
				_targetPosition.y = this.transform.position.y;
			}
			_characterRotation2D.ForcedRotationDirection = (_targetPosition - this.transform.position).normalized;
		}
	}
}