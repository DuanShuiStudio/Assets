using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 需要“角色移动”技能。
    /// 让角色随机移动，直到在其路径上发现障碍，在这种情况下，它将随机选择一个新方向
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Move Randomly 2D")]
	//[RequireComponent(typeof(CharacterMovement))]
	public class AIActionMoveRandomly2D : AIAction
	{
		[Header("Duration持续时间")]
		/// the maximum time a character can spend going in a direction without changing
		[Tooltip("角色在不改变方向的情况下，可以持续朝一个方向前进的最长时间")]
		public float MaximumDurationInADirection = 2f;
		[Header("Obstacles障碍物")]
		/// the layers the character will try to avoid
		[Tooltip("角色会尽量避免的图层")]
		public LayerMask ObstacleLayerMask = LayerManager.ObstaclesLayerMask;
		/// the minimum distance from the target this Character can reach.
		[Tooltip("这个角色能够达到的离目标最近距离")]
		public float ObstaclesDetectionDistance = 1f;
		/// the frequency (in seconds) at which to check for obstacles
		[Tooltip("检查障碍物的频率（以秒为单位）")]
		public float ObstaclesCheckFrequency = 0f;
		/// the minimal random direction to randomize from
		[Tooltip("从哪个最小随机方向开始随机化")]
		public Vector2 MinimumRandomDirection = new Vector2(-1f, -1f);
		/// the maximum random direction to randomize from
		[Tooltip("要随机化的最大随机方向")]
		public Vector2 MaximumRandomDirection = new Vector2(1f, 1f);

		protected CharacterMovement _characterMovement;
		protected Vector2 _direction;
		protected Collider2D _collider;
		protected float _lastObstacleDetectionTimestamp = 0f;
		protected float _lastDirectionChangeTimestamp = 0f;

        /// <summary>
        /// 开始时，我们抓取角色移动组件并选择一个随机方向
        /// </summary>
        public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			_characterMovement = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterMovement>();
			_collider = this.gameObject.GetComponentInParent<Collider2D>();
			PickRandomDirection();
		}

        /// <summary>
        /// 在PerformAction上我们移动
        /// </summary>
        public override void PerformAction()
		{
			CheckForObstacles();
			CheckForDuration();
			Move();
		}

        /// <summary>
        /// 移动角色
        /// </summary>
        protected virtual void Move()
		{
			_characterMovement.SetMovement(_direction);
		}

        /// <summary>
        /// 通过投射一道射线来检查障碍物
        /// </summary>
        protected virtual void CheckForObstacles()
		{
			if (Time.time - _lastObstacleDetectionTimestamp < ObstaclesCheckFrequency)
			{
				return;
			}

			RaycastHit2D hit = Physics2D.BoxCast(_collider.bounds.center, _collider.bounds.size, 0f, _direction.normalized, _direction.magnitude, ObstacleLayerMask);
			if (hit)
			{
				PickRandomDirection();
			}

			_lastObstacleDetectionTimestamp = Time.time;
		}

        /// <summary>
        /// 检查我们是否应该随机选择一个新方向
        /// </summary>
        protected virtual void CheckForDuration()
		{
			if (Time.time - _lastDirectionChangeTimestamp > MaximumDurationInADirection)
			{
				PickRandomDirection();
			}
		}

        /// <summary>
        /// 随机选择一个方向
        /// </summary>
        protected virtual void PickRandomDirection()
		{
			_direction.x = UnityEngine.Random.Range(MinimumRandomDirection.x, MaximumRandomDirection.x);
			_direction.y = UnityEngine.Random.Range(MinimumRandomDirection.y, MaximumRandomDirection.y);
			_lastDirectionChangeTimestamp = Time.time;
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