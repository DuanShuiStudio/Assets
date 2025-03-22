using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 反转PerformAction上巡逻的方向
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Invert Patrol Direction")]
	public class AIActionInvertPatrolDirection : AIAction
	{
		[Header("Invert Patrol Action Bindings反转巡逻动作绑定")]
		/// the AIActionMovePatrol2D to invert the patrol direction on 
		[Tooltip("要反转巡逻方向的AIActionMovePatrol2D")]
		public AIActionMovePatrol2D _movePatrol2D;
		/// the AIActionMovePatrol3D to invert the patrol direction on 
		[Tooltip("要反转巡逻方向的AIActionMovePatrol3D")]
		public AIActionMovePatrol3D _movePatrol3D;

        /// <summary>
        /// 在init上，我们抓取我们的动作
        /// </summary>
        public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			if (_movePatrol2D == null)
			{
				_movePatrol2D = this.gameObject.GetComponentInParent<AIActionMovePatrol2D>();    
			}
			if (_movePatrol3D == null)
			{
				_movePatrol3D = this.gameObject.GetComponentInParent<AIActionMovePatrol3D>();    
			}
		}

        /// <summary>
        /// 反转巡逻方向
        /// </summary>
        public override void PerformAction()
		{
			_movePatrol2D?.ChangeDirection();
			_movePatrol3D?.ChangeDirection();
		}
	}
}