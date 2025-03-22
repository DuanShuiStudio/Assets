using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.Serialization;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    ///  需要“角色移动”技能。使角色在目标方向向上移动到指定的MinimumDistance。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Move Towards Target 2D")]
	//[RequireComponent(typeof(CharacterMovement))]
	public class AIActionMoveTowardsTarget2D : AIAction
	{
		/// if this is true, movement will be constrained to not overstep a certain distance to the target on the x axis
		[Tooltip("如果这个值为真，移动将被限制在x轴上不超过某个特定距离的目标")]
		public bool UseMinimumXDistance = true;
		/// the minimum distance from the target this Character can reach on the x axis.
		[FormerlySerializedAs("MinimumDistance")] [Tooltip("角色在x轴上能够达到的离目标最近的距离")]
		public float MinimumXDistance = 1f;
		
		protected Vector2 _direction;
		protected CharacterMovement _characterMovement;
		protected int _numberOfJumps = 0;

        /// <summary>
        /// 在init中，我们获取我们的CharacterMovement能力
        /// </summary>
        public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			_characterMovement = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterMovement>();
		}

        /// <summary>
        /// 在PerformAction上我们移动
        /// </summary>
        public override void PerformAction()
		{
			Move();
		}

        /// <summary>
        /// 如果需要，移动角色到目标
        /// </summary>
        protected virtual void Move()
		{
			if (_brain.Target == null)
			{
				return;
			}

			if (UseMinimumXDistance)
			{
				if (this.transform.position.x < _brain.Target.position.x)
				{
					_characterMovement.SetHorizontalMovement(1f);
				}
				else
				{
					_characterMovement.SetHorizontalMovement(-1f);
				}

				if (this.transform.position.y < _brain.Target.position.y)
				{
					_characterMovement.SetVerticalMovement(1f);
				}
				else
				{
					_characterMovement.SetVerticalMovement(-1f);
				}
            
				if (Mathf.Abs(this.transform.position.x - _brain.Target.position.x) < MinimumXDistance)
				{
					_characterMovement.SetHorizontalMovement(0f);
				}

				if (Mathf.Abs(this.transform.position.y - _brain.Target.position.y) < MinimumXDistance)
				{
					_characterMovement.SetVerticalMovement(0f);
				}
			}
			else
			{
				_direction = (_brain.Target.position - this.transform.position).normalized;
				_characterMovement.SetMovement(_direction);
			}
			
		}

        /// <summary>
        /// 在退出状态下，我们停止运动
        /// </summary>
        public override void OnExitState()
		{
			base.OnExitState();

			_characterMovement?.SetHorizontalMovement(0f);
			_characterMovement?.SetVerticalMovement(0f);
		}
	}
}