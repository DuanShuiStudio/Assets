using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 这个动作将使角色沿着定义的路径巡逻（参见MMPath检查器），直到它在跟随路径时碰到墙壁或洞。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Move Patrol 2D")]
	//[RequireComponent(typeof(MMPath))]
	//[RequireComponent(typeof(Character))]
	//[RequireComponent(typeof(CharacterOrientation2D))]
	//[RequireComponent(typeof(CharacterMovement))]
	public class AIActionMovePatrol2D : AIAction
	{        
		[Header("Obstacle Detection障碍物检测")]
		/// If set to true, the agent will change direction when hitting an obstacle
		[Tooltip("如果设置为真，当遇到障碍时，代理将改变方向")]
		public bool ChangeDirectionOnObstacle = true;
		/// the layermask to look for obstacles on
		[Tooltip("寻找障碍物的图层蒙版")]
		public LayerMask ObstaclesLayerMask = LayerManager.ObstaclesLayerMask;
		/// the length of the raycast used to detect obstacles
		[Tooltip("用于检测障碍物的射线投射的长度")]
		public float ObstaclesDetectionRaycastLength = 1f;
		/// the frequency (in seconds) at which to check for obstacles
		[Tooltip("检查障碍物的频率（以秒为单位）")]
		public float ObstaclesCheckFrequency = 1f;
        /// the coordinates of the last patrol point 
        [Tooltip("最后一个巡逻点的坐标")]
        public virtual Vector3 LastReachedPatrolPoint { get; set; }

		// private stuff
		protected TopDownController _controller;
		protected Character _character;
		protected CharacterOrientation2D _orientation2D;
		protected CharacterMovement _characterMovement;
		protected Health _health;
		protected Vector2 _direction;
		protected Vector2 _startPosition;
		protected Vector3 _initialScale;
		protected MMPath _mmPath;
		protected float _lastObstacleDetectionTimestamp = 0f;
		protected float _lastPatrolPointReachedAt = 0f;

		protected int _currentIndex = 0;
		protected int _indexLastFrame = -1;
		protected float _waitingDelay = 0f;

        /// <summary>
        /// 在init中，我们获取所需的所有组件
        /// </summary>
        protected override void Awake()
		{
			base.Awake();
			InitializePatrol();
		}

        /// <summary>
        /// 在init中，我们获取所需的所有组件
        /// </summary>
        protected virtual void InitializePatrol()
		{
			_controller = this.gameObject.GetComponentInParent<TopDownController>();
			_character = this.gameObject.GetComponentInParent<Character>();
			_orientation2D = _character?.FindAbility<CharacterOrientation2D>();
			_characterMovement = _character?.FindAbility<CharacterMovement>();
			_health = _character?.CharacterHealth;
			_mmPath = this.gameObject.GetComponentInParent<MMPath>();
            // 初始化起始位置
            _startPosition = transform.position;
            // 初始化方向
            _direction = _orientation2D.IsFacingRight ? Vector2.right : Vector2.left;
			_initialScale = transform.localScale;
			_currentIndex = 0;
			_indexLastFrame = -1;
			_waitingDelay = 0;
			_initialized = true;
			_lastPatrolPointReachedAt = Time.time;
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

			_currentIndex = _mmPath.CurrentIndex();
			if (_currentIndex != _indexLastFrame)
			{
				LastReachedPatrolPoint = _mmPath.CurrentPoint();
				_lastPatrolPointReachedAt = Time.time;
				DetermineDelay();
			}

			_direction = _mmPath.CurrentPoint() - this.transform.position;
			_direction = _direction.normalized;

			_characterMovement.SetHorizontalMovement(_direction.x);
			_characterMovement.SetVerticalMovement(_direction.y);

			_indexLastFrame = _currentIndex;
		}
		
		protected virtual void StopMovement()
		{
			_characterMovement.SetHorizontalMovement(0f);
			_characterMovement.SetVerticalMovement(0f);
		}

		protected virtual void DetermineDelay()
		{
			if ( (_mmPath.Direction > 0 && (_currentIndex == 0))
			     || (_mmPath.Direction < 0) && (_currentIndex == _mmPath.PathElements.Count - 1))
			{
				int previousPathIndex = _mmPath.Direction > 0 ? _mmPath.PathElements.Count - 1 : 1;
				_waitingDelay = _mmPath.PathElements[previousPathIndex].Delay;
			}
			else 
			{
				int previousPathIndex = _mmPath.Direction > 0 ? _currentIndex - 1 : _currentIndex + 1;
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

			RaycastHit2D raycast = MMDebug.RayCast(_controller.ColliderCenter, _direction, ObstaclesDetectionRaycastLength, ObstaclesLayerMask, MMColors.Gold, true);

            //如果代理与什么东西碰撞，让它掉头
            if (raycast)
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
        /// 将巡逻代理的位置重置为路径的起点，重新初始化路径
        /// </summary>
        public void ResetPatrol()
		{
			this.transform.position = _startPosition;
			_mmPath.Initialization();
			InitializePatrol();
		}

        /// <summary>
        /// 当我们复活时，我们要确保我们的方向是正确设置的
        /// </summary>
        protected virtual void OnRevive()
		{
			if (!_initialized)
			{
				return;
			}
            
			if (_orientation2D != null)
			{
				_direction = _orientation2D.IsFacingRight ? Vector2.right : Vector2.left;
			}
            
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