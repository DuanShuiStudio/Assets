using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 需要一个CharacterGridMovement技能。
    /// 让角色在网格中随机移动，直到在其路径上发现障碍，在这种情况下，它将随机选择一个新方向
    /// 支持2D和3D网格
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Move Randomly Grid")]
	//[RequireComponent(typeof(CharacterGridMovement))]
	public class AIActionMoveRandomlyGrid : AIAction
	{
		public enum Modes { TwoD, ThreeD }

		[Header("Dimension范围")]
		public Modes Mode = Modes.ThreeD;

		[Header("Duration持续时间")]
		/// the maximum time a character can spend going in a direction without changing
		[Tooltip("角色在不改变方向的情况下，可以持续朝一个方向前进的最长时间")]
		public float MaximumDurationInADirection = 3f;

		[Header("Obstacles障碍物")]
		/// the layers the character will try to avoid
		[Tooltip("角色会尽量避免的图层")]
		public LayerMask ObstacleLayerMask = LayerManager.ObstaclesLayerMask;
		/// the minimum distance from the target this Character can reach.
		[Tooltip("这个角色能够达到的离目标最近距离")]
		public float ObstaclesDetectionDistance = 1f;
		/// the frequency (in seconds) at which to check for obstacles
		[Tooltip("检查障碍物的频率（以秒为单位）")]
		public float ObstaclesCheckFrequency = 1f;
		/// the minimal random direction to randomize from
		[Tooltip("从哪个最小随机方向开始随机化")]
		public Vector2 MinimumRandomDirection = new Vector2(-1f, -1f);
		/// the maximum random direction to randomize from
		[Tooltip("要随机化的最大随机方向")]
		public Vector2 MaximumRandomDirection = new Vector2(1f, 1f);
		/// if this is true, the AI will avoid 180° turns if possible
		[Tooltip("如果这是真的，AI将尽可能避免180°转弯")]
		public bool Avoid180 = true;

		protected CharacterGridMovement _characterGridMovement;
		protected TopDownController _topDownController;
		protected Vector2 _direction;
		protected Collider _collider;
		protected Collider2D _collider2D;
		protected float _lastObstacleDetectionTimestamp = 0f;
		protected float _lastDirectionChangeTimestamp = 0f;
		protected Vector3 _rayDirection;
		protected Vector2 _temp2DVector;
		protected Vector3 _temp3DVector;

		protected Vector2[] _raycastDirections2D;
		protected Vector3[] _raycastDirections3D;
		protected RaycastHit _hit;
		protected RaycastHit2D _hit2D;

        /// <summary>
        /// 开始时，我们抓取角色移动组件并选择一个随机方向
        /// </summary>
        public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			_characterGridMovement = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterGridMovement>();
			_topDownController = this.gameObject.GetComponentInParent<TopDownController>();
			_collider = this.gameObject.GetComponentInParent<Collider>();
			_collider2D = this.gameObject.GetComponentInParent<Collider2D>();

			_raycastDirections2D = new[] { Vector2.right, Vector2.left, Vector2.up, Vector2.down };
			_raycastDirections3D = new[] { Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

			PickNewDirection();
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
			_characterGridMovement.SetMovement(_direction);
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

			_lastObstacleDetectionTimestamp = Time.time;

			if (Mode == Modes.ThreeD)
			{
				_temp3DVector = _direction;
				_temp3DVector.z = _direction.y;
				_temp3DVector.y = 0;
				_hit = MMDebug.Raycast3D(_collider.bounds.center, _temp3DVector, ObstaclesDetectionDistance, ObstacleLayerMask, Color.gray);
				if (_topDownController.CollidingWithCardinalObstacle)
				{
					PickNewDirection();
				}
			}
			else
			{
				_temp2DVector = _direction;
				_hit2D = MMDebug.RayCast(_collider2D.bounds.center, _temp2DVector, ObstaclesDetectionDistance, ObstacleLayerMask, Color.gray);
				if (_topDownController.CollidingWithCardinalObstacle)
				{
					PickNewDirection();
				}

			}
		}

        /// <summary>
        /// 测试并选择一个新的前进方向
        /// </summary>
        protected virtual void PickNewDirection()
		{
			int retries = 0;
			switch (Mode)
			{

				case Modes.ThreeD:  
					while (retries < 10)
					{
						retries++;
						int random = MMMaths.RollADice(4) - 1;
						_temp3DVector = _raycastDirections3D[random];
                        
						if (Avoid180)
						{
							if ((_temp3DVector.x == -_direction.x) && (Mathf.Abs(_temp3DVector.x) > 0))
							{
								continue;
							}
							if ((_temp3DVector.y == -_direction.y) && (Mathf.Abs(_temp3DVector.y) > 0))
							{
								continue;
							}
						}

						_hit = MMDebug.Raycast3D(_collider.bounds.center, _temp3DVector, ObstaclesDetectionDistance, ObstacleLayerMask, Color.gray);
						if (_hit.collider == null)
						{
							_direction = _temp3DVector;
							_direction.y = _temp3DVector.z;

							return;
						}
					}
					break;

				case Modes.TwoD:
					while (retries < 10)
					{
						retries++;
						int random = MMMaths.RollADice(4) - 1;
						_temp2DVector = _raycastDirections2D[random];

						if (Avoid180)
						{
							if ((_temp2DVector.x == -_direction.x) && (Mathf.Abs(_temp2DVector.x) > 0))
							{
								continue;
							}
							if ((_temp2DVector.y == -_direction.y) && (Mathf.Abs(_temp2DVector.y) > 0))
							{
								continue;
							}
						}

						_hit2D = MMDebug.RayCast(_collider2D.bounds.center, _temp2DVector, ObstaclesDetectionDistance, ObstacleLayerMask, Color.gray);
						if (_hit2D.collider == null)
						{
							_direction = _temp2DVector;

							return;
						}
					}
					break;
			}
		}

        /// <summary>
        /// 检查我们是否应该随机选择一个新方向
        /// </summary>
        protected virtual void CheckForDuration()
		{
			if (Time.time - _lastDirectionChangeTimestamp > MaximumDurationInADirection)
			{
				PickNewDirection();
				_lastDirectionChangeTimestamp = Time.time;
			}
		}

        /// <summary>
        /// 在退出状态下，我们停止运动
        /// </summary>
        public override void OnExitState()
		{
			base.OnExitState();

			_characterGridMovement?.StopMovement();
		}
	}
}