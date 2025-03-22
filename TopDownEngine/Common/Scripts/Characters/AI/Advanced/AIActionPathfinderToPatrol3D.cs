using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 这个动作会让角色找到回到最后一个巡逻点的路
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Pathfinder To Patrol 3D")]
	//[RequireComponent(typeof(AIActionMovePatrol3D))]
	//[RequireComponent(typeof(CharacterPathfinder3D))]
	//[RequireComponent(typeof(CharacterMovement))]
	public class AIActionPathfinderToPatrol3D : AIAction
	{
		protected CharacterMovement _characterMovement;
		protected CharacterPathfinder3D _characterPathfinder3D;
		protected Transform _backToPatrolTransform;
		protected AIActionMovePatrol3D _aiActionMovePatrol3D;
		
		static Transform _BackToPatrolBeacons;
		public static Transform BackToPatrolBeaconsRoot
		{
			get
			{
				if (_BackToPatrolBeacons != null)
				{
					return _BackToPatrolBeacons;
				}
				GameObject newRoot = new GameObject(nameof(AIActionPathfinderToPatrol3D)+"_BackToPatrolBeacons");
				_BackToPatrolBeacons = newRoot.transform;
				return _BackToPatrolBeacons;
			}
		}

        /// <summary>
        /// 在init中，我们获取我们的CharacterMovement能力
        /// </summary>
        public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			_characterMovement = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterMovement>();
			_characterPathfinder3D = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterPathfinder3D>();
			_aiActionMovePatrol3D = this.gameObject.GetComponent<AIActionMovePatrol3D>();

			GameObject backToPatrolBeacon = new GameObject();
			backToPatrolBeacon.name = this.gameObject.name + "BackToPatrolBeacon";
			_backToPatrolTransform = backToPatrolBeacon.transform;			
			_backToPatrolTransform.parent = BackToPatrolBeaconsRoot;
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
			if (_aiActionMovePatrol3D == null)
			{
				return;
			}

			_backToPatrolTransform.position = _aiActionMovePatrol3D.LastReachedPatrolPoint;
			_characterPathfinder3D.SetNewDestination(_backToPatrolTransform);
			_brain.Target = _backToPatrolTransform;
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