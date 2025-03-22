using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 在2D中，如果代理和大脑目标之间的直线上没有障碍物，这个决定将返回true
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AI Decision Line Of Sight To Target 2D")]
	public class AIDecisionLineOfSightToTarget2D : AIDecision
	{
		/// the layermask to consider as obstacles when trying to determine whether a line of sight is present
		[Tooltip("在尝试确定是否存在视线时，要作为障碍物考虑的层掩码")]
		public LayerMask ObstacleLayerMask = LayerManager.ObstaclesLayerMask;
		/// the offset to apply (from the collider's center) when casting a ray from the agent to its target
		[Tooltip("在从代理到目标投射射线时应用的偏移量（从碰撞体的中心开始）")]
		public Vector3 LineOfSightOffset = new Vector3(0, 0, 0);

		protected Vector2 _directionToTarget;
		protected Collider2D _collider;
		protected Vector2 _raycastOrigin;

        /// <summary>
        /// init时，我们抓取collider
        /// </summary>
        public override void Initialization()
		{
			_collider = this.gameObject.GetComponentInParent<Collider2D>();
		}

        /// <summary>
        /// 在决定，我们检查我们是否有视线
        /// </summary>
        /// <returns></returns>
        public override bool Decide()
		{
			return CheckLineOfSight();
		}

        /// <summary>
        /// 检查代理和目标之间是否有障碍物
        /// </summary>
        /// <returns></returns>
        protected virtual bool CheckLineOfSight()
		{
			if (_brain.Target == null)
			{
				return false;
			}

			_raycastOrigin = _collider.bounds.center + LineOfSightOffset / 2;
			_directionToTarget = (Vector2)_brain.Target.transform.position - _raycastOrigin;
            
			RaycastHit2D hit = MMDebug.RayCast(_raycastOrigin, _directionToTarget.normalized, _directionToTarget.magnitude, ObstacleLayerMask, Color.yellow, true);
			if (hit.collider == null)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}