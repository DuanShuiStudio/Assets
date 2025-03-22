using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 需要“角色移动”技能。使角色远离目标。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Move Away From Target 2D")]
	//[RequireComponent(typeof(CharacterMovement))]
	public class AIActionMoveAwayFromTarget2D : AIAction
	{
		/// the maximum distance away from the target this Character can reach.
		[Tooltip("这个角色能够达到的离目标最远距离")]
		public float MaximumDistance = 5f;

		protected CharacterMovement _characterMovement;

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
            
			if (this.transform.position.x < _brain.Target.position.x)
			{
				_characterMovement.SetHorizontalMovement(-1f);
			}
			else
			{
				_characterMovement.SetHorizontalMovement(1f);
			}

			if (this.transform.position.y < _brain.Target.position.y)
			{
				_characterMovement.SetVerticalMovement(-1f);
			}
			else
			{
				_characterMovement.SetVerticalMovement(1f);
			}
            
			if (Mathf.Abs(this.transform.position.x - _brain.Target.position.x) > MaximumDistance)
			{
				_characterMovement.SetHorizontalMovement(0f);
			}

			if (Mathf.Abs(this.transform.position.y - _brain.Target.position.y) > MaximumDistance)
			{
				_characterMovement.SetVerticalMovement(0f);
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