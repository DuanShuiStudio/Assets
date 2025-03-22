using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 这个动作将使角色沿着定义的路径巡逻（参见MMPath检查器），直到它在跟随路径时碰到墙壁或洞。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Move Patrol 3D")]
	//[RequireComponent(typeof(MMPath))]
	//[RequireComponent(typeof(Character))]
	//[RequireComponent(typeof(CharacterMovement))]
	public class AIActionMovePatrol3D : AIAction
	{        
		[Header("Obstacle Detection障碍物检测")]
		/// If set to true, the agent will change direction when hitting an obstacle
		[Tooltip("如果设置为真，当遇到障碍时，代理将改变方向")]
		public bool ChangeDirectionOnObstacle = true;
		/// the distance to look for obstacles at
		[Tooltip("寻找障碍物的距离")]
		public float ObstacleDetectionDistance = 1f;
		/// the frequency (in seconds) at which to check for obstacles
		[Tooltip("检查障碍物的频率（以秒为单位）")]
		public float ObstaclesCheckFrequency = 1f;
		/// the layer(s) to look for obstacles on
		[Tooltip("寻找障碍物的层（多个层）")]
		public LayerMask ObstacleLayerMask = LayerManager.ObstaclesLayerMask;
        /// the coordinates of the last patrol point 
        [Tooltip("最后一个巡逻点的坐标")]
        public virtual Vector3 LastReachedPatrolPoint { get; set; }

		[Header("Debug")]
		/// the index of the current MMPath element this agent is patrolling towards
		[Tooltip("这个代理正在巡逻前往的当前MMPath元素的索引")]
		[MMReadOnly]
		public int CurrentPathIndex = 0;

		// private stuff
		protected TopDownController _controller;
		protected Character _character;
		protected CharacterMovement _characterMovement;
		protected Health _health;
		protected Vector3 _direction;
		protected Vector3 _startPosition;
		protected Vector3 _initialDirection;
		protected Vector3 _initialScale;
		protected float _distanceToTarget;
		protected Vector3 _initialPosition;
		protected MMPath _mmPath;
		protected Collider _collider;
		protected float _lastObstacleDetectionTimestamp = 0f;        
		protected int _indexLastFrame = -1;
		protected float _waitingDelay = 0f;
		protected float _lastPatrolPointReachedAt = 0f;

        /// <summary>
        /// 在init中，我们获取所需的所有组件
        /// </summary>
        protected override void Awake()
		{
			base.Awake();
			InitializePatrol();
		}

		protected virtual void InitializePatrol()
		{
			_collider = this.gameObject.GetComponentInParent<Collider>();
			_controller = this.gameObject.GetComponentInParent<TopDownController>();
			_character = this.gameObject.GetComponentInParent<Character>();
			_characterMovement = _character?.FindAbility<CharacterMovement>();
			_health = _character.CharacterHealth;
			_mmPath = this.gameObject.GetComponentInParent<MMPath>();
            // 初始化起始位置
            _startPosition = transform.position;
			_initialPosition = this.transform.position;
			_initialDirection = _direction;
			_initialScale = transform.localScale;
			CurrentPathIndex = 0;
			_indexLastFrame = -1;
			LastReachedPatrolPoint = this.transform.position;
			_lastPatrolPointReachedAt = Time.time;
		}

		public void ResetPatrol(Vector3 targetPos) // MMPath改变
        {
			CurrentPathIndex = 0;
			_indexLastFrame = -1;
			LastReachedPatrolPoint = targetPos;
		}

        /// <summary>
        /// 我们在PerformAction上巡逻
        /// </summary>
        public override void PerformAction()
		{
			Patrol();
		}

        /// <summary>
        /// 该方法启动所有必需的检查并移动字符
        /// </summary>
        protected virtual void Patrol()
		{
			if (_character == null)
			{
				return;
			}

			if ((_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead)
			    || (_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Frozen))
			{
				return;
			}

			if ((_mmPath.CycleOption == MMPath.CycleOptions.OnlyOnce) && _mmPath.EndReached)
			{
				StopMovement();
				return;
			}

			if (Time.time - _lastPatrolPointReachedAt < _waitingDelay)
			{
				StopMovement();
				return;
			}

            // 将代理移动到当前方向
            CheckForObstacles();

			CurrentPathIndex = _mmPath.CurrentIndex();
			if (CurrentPathIndex != _indexLastFrame)
			{
				LastReachedPatrolPoint = _mmPath.CurrentPoint();
				_lastPatrolPointReachedAt = Time.time;
				DetermineDelay();

				if (_waitingDelay > 0f)
				{
					_characterMovement.SetHorizontalMovement(0f);
					_characterMovement.SetVerticalMovement(0f);
					_indexLastFrame = CurrentPathIndex;
					return;
				}
			}

			_direction = _mmPath.CurrentPoint() - this.transform.position;
			_direction = _direction.normalized;

			_characterMovement.SetHorizontalMovement(_direction.x);
			_characterMovement.SetVerticalMovement(_direction.z);

			_indexLastFrame = CurrentPathIndex;
		}

		protected virtual void StopMovement()
		{
			_characterMovement.SetHorizontalMovement(0f);
			_characterMovement.SetVerticalMovement(0f);
		}

		protected virtual void DetermineDelay()
		{
			if ( (_mmPath.Direction > 0 && (CurrentPathIndex == 0))
			|| (_mmPath.Direction < 0) && (CurrentPathIndex == _mmPath.PathElements.Count - 1))
			{
				int previousPathIndex = _mmPath.Direction > 0 ? _mmPath.PathElements.Count - 1 : 1;
				_waitingDelay = _mmPath.PathElements[previousPathIndex].Delay; 
			}
			else 
			{
				int previousPathIndex = _mmPath.Direction > 0 ? CurrentPathIndex - 1 : CurrentPathIndex + 1;
				_waitingDelay = _mmPath.PathElements[previousPathIndex].Delay; 
			}
		}

        /// <summary>
        /// 绘制边界小玩意儿
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
		{
			if (_mmPath == null)
			{
				return;
			}
			Gizmos.color = MMColors.IndianRed;
			Gizmos.DrawLine(this.transform.position, _mmPath.CurrentPoint());
		}

        /// <summary>
        /// 当退出状态时，我们重置我们的运动
        /// </summary>
        public override void OnExitState()
		{
			base.OnExitState();
			_characterMovement?.SetHorizontalMovement(0f);
			_characterMovement?.SetVerticalMovement(0f);
		}

        /// <summary>
        /// 检查是否有墙，如果遇到墙就改变方向
        /// </summary>
        protected virtual void CheckForObstacles()
		{
			if (!ChangeDirectionOnObstacle)
			{
				return;
			}

			if (Time.time - _lastObstacleDetectionTimestamp < ObstaclesCheckFrequency)
			{
				return;
			}
                        
			bool hit = Physics.BoxCast(_collider.bounds.center, _collider.bounds.extents, _controller.CurrentDirection.normalized, this.transform.rotation, ObstacleDetectionDistance, ObstacleLayerMask);
			if (hit)
			{
				ChangeDirection();
			}

			_lastObstacleDetectionTimestamp = Time.time;
		}

        /// <summary>
        /// 改变当前的移动方向
        /// </summary>
        public virtual void ChangeDirection()
		{
			_direction = -_direction;
			_mmPath.ChangeDirection();
		}

        /// <summary>
        /// 当我们复活时，我们要确保我们的方向是正确设置的
        /// </summary>
        protected virtual void OnRevive()
		{            
			InitializePatrol();
		}

        /// <summary>
        /// 启用后，我们开始监听OnRevive事件
        /// </summary>
        protected virtual void OnEnable()
		{
			if (_health == null)
			{
				_health = (_character != null) ? _character.CharacterHealth : this.gameObject.GetComponent<Health>();
			}

			if (_health != null)
			{
				_health.OnRevive += OnRevive;
			}
		}

        /// <summary>
        /// 禁用时，我们停止监听OnRevive事件
        /// </summary>
        protected virtual void OnDisable()
		{
			if (_health != null)
			{
				_health.OnRevive -= OnRevive;
			}
		}
	}
}