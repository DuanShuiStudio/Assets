using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
	[AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Rotate Towards Target 3D")]
	//[RequireComponent(typeof(CharacterOrientation3D))]
	public class AIActionRotateTowardsTarget3D : AIAction
	{
		[Header("Lock Rotation锁定旋转")]
		/// whether or not to lock the X rotation. If set to false, the model will rotate on the x axis, to aim up or down 
		[Tooltip("是否锁定X轴旋转。如果设置为false，模型将在X轴上旋转，以向上或向下瞄准")]
		public bool LockRotationX = false;

		protected CharacterOrientation3D _characterOrientation3D;
		protected Vector3 _targetPosition;
		protected bool _originalForcedRotation;

        /// <summary>
        /// 在init中，我们获取CharacterOrientation3D能力
        /// </summary>
        public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			_characterOrientation3D = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterOrientation3D>();
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
			_characterOrientation3D.ForcedRotationDirection = (_targetPosition - this.transform.position).normalized;
		}

        /// <summary>
        /// 在进入状态时，我们重置我们的标志
        /// </summary>
        public override void OnEnterState()
		{
			base.OnEnterState();
			if (_characterOrientation3D == null)
			{
				_characterOrientation3D = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterOrientation3D>();
			}
			if (_characterOrientation3D != null)
			{
				_originalForcedRotation = _characterOrientation3D.ForcedRotation;
				_characterOrientation3D.ForcedRotation = true;
			}
		}

        /// <summary>
        /// 在进入状态时，我们重置我们的标志
        /// </summary>
        public override void OnExitState()
		{
			base.OnExitState();
			if (_characterOrientation3D != null)
			{
				_characterOrientation3D.ForcedRotation = _originalForcedRotation;
			}
		}
	}
}