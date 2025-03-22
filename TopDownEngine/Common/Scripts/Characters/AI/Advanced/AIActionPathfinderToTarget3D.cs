using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 需要“角色移动”技能。使角色在目标方向向上移动到指定的MinimumDistance。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Pathfinder To Target 3D")]
	//[RequireComponent(typeof(CharacterMovement))]
	//[RequireComponent(typeof(CharacterPathfinder3D))]
	public class AIActionPathfinderToTarget3D : AIAction
	{
		public float MinimumDelayBeforeUpdatingTarget = 0.3f;
		
		protected CharacterMovement _characterMovement;
		protected CharacterPathfinder3D _characterPathfinder3D;
		protected float _lastSetNewDestinationAt = -Single.MaxValue;

        /// <summary>
        /// 在init中，我们获取我们的CharacterMovement能力
        /// </summary>
        public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			_characterMovement = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterMovement>();
			_characterPathfinder3D = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterPathfinder3D>();
			if (_characterPathfinder3D == null)
			{
				Debug.LogWarning(this.name + " : AIActionPathfinderToTarget3D AI动作需要CharacterPathfinder3D能力");
			}
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
			if (Time.time - _lastSetNewDestinationAt < MinimumDelayBeforeUpdatingTarget)
			{
				return;
			}

			_lastSetNewDestinationAt = Time.time;
			
			if (_brain.Target == null)
			{
				_characterPathfinder3D.SetNewDestination(null);
				return;
			}
			else
			{
				_characterPathfinder3D.SetNewDestination(_brain.Target.transform);
			}
		}

        /// <summary>
        /// 在退出状态下，我们停止运动
        /// </summary>
        public override void OnExitState()
		{
			base.OnExitState();
            
			_characterPathfinder3D?.SetNewDestination(null);
			_characterMovement?.SetHorizontalMovement(0f);
			_characterMovement?.SetVerticalMovement(0f);
		}
	}
}