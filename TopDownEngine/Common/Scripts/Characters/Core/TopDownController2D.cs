using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 在俯视角中移动rigidbody2D和collider2D的控制器
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Core/TopDown Controller 2D")]
	public class TopDownController2D : TopDownController 
	{
		/// whether or not the character is above a hole right now
		[MMReadOnly]
		[Tooltip("角色当前是否处于一个洞上方")]
		public bool OverHole = false;
        /// 对撞机的中心位置
        public override Vector3 ColliderCenter { get { return (Vector2)this.transform.position + ColliderOffset; } }
        /// 对撞机的底部位置
        public override Vector3 ColliderBottom { get { return (Vector2)this.transform.position + ColliderOffset + Vector2.down * ColliderBounds.extents.y; } }
        /// the collider's top position
        public override Vector3 ColliderTop { get { return (Vector2)this.transform.position + ColliderOffset + Vector2.up * ColliderBounds.extents.y; } }
        ///  角色是否在移动平台上
        public override bool OnAMovingPlatform { get { return _movingPlatform; } }
        /// 移动平台的速度
        public override Vector3 MovingPlatformSpeed { get { if (_movingPlatform != null) { return _movingPlatform.CurrentSpeed; } else { return Vector3.zero; } } }

		/// the layer mask to consider as ground
		[Tooltip("要视为地面的层掩码")]
		public LayerMask GroundLayerMask = LayerManager.GroundLayerMask;
		/// the layer mask to consider as holes
		[Tooltip("要视为洞的层掩码")]
		public LayerMask HoleLayerMask = LayerManager.HoleLayerMask;
		/// the layer to consider as obstacles (will prevent movement)
		[Tooltip("要视为障碍物的层（将阻止移动）")]
		public LayerMask ObstaclesLayerMask = LayerManager.ObstaclesLayerMask;

		public Vector2 ColliderSize
		{
			get
			{
				if (!_boxColliderNull)
				{
					return _boxCollider.size;
				}
				if (!_capsuleColliderNull)
				{
					return _capsuleCollider.size;
				}
				if (!_circleColliderNull)
				{
					return Vector2.one * _circleCollider.radius;
				}
				return Vector2.zero;
			}
			set
			{
				if (!_boxColliderNull)
				{
					_boxCollider.size = value;
					return;
				}
				if (!_capsuleColliderNull)
				{
					_capsuleCollider.size = value;
					return;
				}
				if (!_circleColliderNull)
				{
					_circleCollider.radius = value.x;
					return;
				}
			}
		}
        
		public Vector2 ColliderOffset
		{
			get
			{
				if (!_boxColliderNull)
				{
					return _boxCollider.offset;
				}
				if (!_capsuleColliderNull)
				{
					return _capsuleCollider.offset;
				}
				if (!_circleColliderNull)
				{
					return _circleCollider.offset;
				}
				return Vector2.zero;
			}
			set
			{
				if (!_boxColliderNull)
				{
					_boxCollider.offset = value;
					return;
				}
				if (!_capsuleColliderNull)
				{
					_capsuleCollider.offset = value;
					return;
				}
				if (!_circleColliderNull)
				{
					_circleCollider.offset = value;
					return;
				}
			}
		}
        
		public Bounds ColliderBounds
		{
			get
			{
				if (!_boxColliderNull)
				{
					return _boxCollider.bounds;
				}
				if (!_capsuleColliderNull)
				{
					return _capsuleCollider.bounds;
				}
				if (!_circleColliderNull)
				{
					return _circleCollider.bounds;
				}
				return new Bounds();
			}
		}

		protected Rigidbody2D _rigidBody;
		protected BoxCollider2D _boxCollider;
		protected bool _boxColliderNull;
		protected CapsuleCollider2D _capsuleCollider;
		protected bool _capsuleColliderNull;
		protected CircleCollider2D _circleCollider;
		protected bool _circleColliderNull;
		protected Vector2 _originalColliderSize;
		protected Vector3 _originalColliderCenter;
		protected Vector3 _originalSizeRaycastOrigin;
		protected Vector3 _orientedMovement;
		protected Collider2D _groundedTest;
		protected Collider2D _holeTestMin;
		protected Collider2D _holeTestMax;
		protected MovingPlatform2D _movingPlatform;
		protected Vector3 _movingPlatformPositionLastFrame;

        // 碰撞检测
        protected RaycastHit2D _raycastUp;
		protected RaycastHit2D _raycastDown;
		protected RaycastHit2D _raycastLeft;
		protected RaycastHit2D _raycastRight;

        /// <summary>
        /// 醒来时，我们抓住我们的组件
        /// </summary>
        protected override void Awake()
		{
			base.Awake();
			_rigidBody = GetComponent<Rigidbody2D>();
			_boxCollider = GetComponent<BoxCollider2D>();
			_capsuleCollider = GetComponent<CapsuleCollider2D>();
			_circleCollider = GetComponent<CircleCollider2D>();
			_boxColliderNull = _boxCollider == null;
			_capsuleColliderNull = _capsuleCollider == null;
			_circleColliderNull = _circleCollider == null;
			_originalColliderSize = ColliderSize;
			_originalColliderCenter = ColliderOffset;
		}

        /// <summary>
        /// 决定这个角色是否接地
        /// </summary>
        protected override void CheckIfGrounded()
		{
			_groundedTest = Physics2D.OverlapPoint((Vector2)this.transform.position, GroundLayerMask);
			_holeTestMin = Physics2D.OverlapPoint((Vector2)ColliderBounds.min, HoleLayerMask);
			_holeTestMax = Physics2D.OverlapPoint((Vector2)ColliderBounds.max, HoleLayerMask);
			Grounded = (_groundedTest != null);
			OverHole = ((_holeTestMin != null) && (_holeTestMax != null));                        
			JustGotGrounded = (!_groundedLastFrame && Grounded);
			_groundedLastFrame = Grounded;
		}

        /// <summary>
        /// 在固定更新时，我们移动刚体
        /// </summary>
        protected override void FixedUpdate()
		{
			base.FixedUpdate();

			ApplyImpact();
			
			if (!FreeMovement)
			{
				return;
			}

			if (Friction > 1)
			{
				CurrentMovement = CurrentMovement / Friction;
			}

            // 如果摩擦力小（冰、弹珠……），我们就会相应地降低速度
            if (Friction > 0 && Friction < 1)
			{
				CurrentMovement = Vector3.Lerp(Speed, CurrentMovement, Time.fixedDeltaTime * Friction);
			}
            
			Vector2 newMovement = _rigidBody.position + (Vector2)(CurrentMovement + AddedForce) * Time.fixedDeltaTime;
            
			if (OnAMovingPlatform)
			{
				newMovement += (Vector2)_movingPlatform.CurrentSpeed * Time.fixedDeltaTime;
			}
			_rigidBody.MovePosition(newMovement);
			
			ComputeNewVelocity();
			ComputeSpeed();
		}

        /// <summary>
        /// 根据我们的位置和上一帧的位置确定新的Velocity值
        /// </summary>
        protected virtual void ComputeNewVelocity()
		{
			Velocity = (_rigidBody.transform.position - _positionLastFrame) / Time.fixedDeltaTime;
			Acceleration = (Velocity - VelocityLastFrame) / Time.fixedDeltaTime;
			VelocityLastFrame = Velocity;
		}

        /// <summary>
        /// 更新时，我们确定加速度
        /// </summary>
        protected override void Update()
		{
			base.Update();
		}

        /// <summary>
        /// 在后期更新时，我们会施加影响
        /// </summary>
        protected override void LateUpdate()
		{
			base.LateUpdate();
		}

        /// <summary>
        /// 处理摩擦，仍在进行中（待办事项）
        /// </summary>
        protected override void HandleFriction()
		{
			if (SurfaceModifierBelow == null)
			{
				Friction = 0f;
				AddedForce = Vector3.zero;
				return;
			}
			else
			{
				Friction = SurfaceModifierBelow.Friction;

				if (AddedForce.y != 0f)
				{
					AddForce(AddedForce);
				}

				AddedForce.y = 0f;
				AddedForce = SurfaceModifierBelow.AddedForce;
			}
		}

        /// <summary>
        /// 另一种方法是加一个指定方向的力
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="force"></param>
        public override void Impact(Vector3 direction, float force)
		{
			direction = direction.normalized;
			_impact += direction.normalized * force;
		}

        /// <summary>
        /// 应用当前影响
        /// </summary>
        protected virtual void ApplyImpact()
		{
			if (_impact.magnitude > 0.2f)
			{
				_rigidBody.AddForce(_impact);
			}
			_impact = Vector3.Lerp(_impact, Vector3.zero, 5f * Time.fixedDeltaTime);
		}

        /// <summary>
        /// 添加指定矢量的力
        /// </summary>
        /// <param name="movement"></param>
        public override void AddForce(Vector3 movement)
		{
			Impact(movement.normalized, movement.magnitude);
		}

        /// <summary>
        /// 设置当前移动
        /// </summary>
        /// <param name="movement"></param>
        public override void SetMovement(Vector3 movement)
		{
			_orientedMovement = movement;
			_orientedMovement.y = _orientedMovement.z;
			_orientedMovement.z = 0f;
			CurrentMovement = _orientedMovement;
		}

        /// <summary>
        /// 尝试移动到指定位置
        /// </summary>
        /// <param name="newPosition"></param>
        public override void MovePosition(Vector3 newPosition, bool targetTransform = false)
		{
			if (targetTransform)
			{
				this.transform.position = newPosition;
			}
			else
			{
				_rigidBody.MovePosition(newPosition);	
			}
		}

        /// <summary>
        /// 将碰撞器的大小调整为参数中设置的新大小
        /// </summary>
        /// <param name="newSize">New size.</param>
        public override void ResizeColliderHeight(float newHeight, bool translateCenter = false)
		{
			float newYOffset = _originalColliderCenter.y - (_originalColliderSize.y - newHeight) / 2;
			Vector2 newSize = ColliderSize;
			newSize.y = newHeight;
			ColliderSize = newSize;
			ColliderOffset = newYOffset * Vector3.up;
		}

        /// <summary>
        /// 将碰撞器返回到初始大小
        /// </summary>
        public override void ResetColliderSize()
		{
			ColliderSize = _originalColliderSize;
			ColliderOffset = _originalColliderCenter;
		}

        /// <summary>
        /// 确定控制器的当前方向
        /// </summary>
        protected override void DetermineDirection()
		{
			if (CurrentMovement != Vector3.zero)
			{
				CurrentDirection = CurrentMovement.normalized;
			}
		}

        /// <summary>
        /// 为该控制器设置一个移动平台
        /// </summary>
        /// <param name="platform"></param>
        public virtual void SetMovingPlatform(MovingPlatform2D platform)
		{
			_movingPlatform = platform;
		}

        /// <summary>
        /// 将这个刚体设置为运动学
        /// </summary>
        /// <param name="state"></param>
        public override void SetKinematic(bool state)
		{
			_rigidBody.isKinematic = state;
		}

        /// <summary>
        /// 启用碰撞器
        /// </summary>
        public override void CollisionsOn()
		{
			if (!_boxColliderNull)
			{
				_boxCollider.enabled = true;
			}
			if (!_capsuleColliderNull)
			{
				_capsuleCollider.enabled = true;
			}
			if (!_circleColliderNull)
			{
				_circleCollider.enabled = true;
			}
		}

        /// <summary>
        /// 禁用碰撞器
        /// </summary>
        public override void CollisionsOff()
		{
			if (!_boxColliderNull)
			{
				_boxCollider.enabled = false;
			}
			if (!_capsuleColliderNull)
			{
				_capsuleCollider.enabled = false;
			}
			if (!_circleColliderNull)
			{
				_circleCollider.enabled = false;
			}
		}

        /// <summary>
        /// 执行基本碰撞检查并存储碰撞对象信息
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="offset"></param>
        public override void DetectObstacles(float distance, Vector3 offset)
		{
			if (!PerformCardinalObstacleRaycastDetection)
			{
				return;
			}
            
			CollidingWithCardinalObstacle = false;
			_raycastRight = MMDebug.RayCast(this.transform.position + offset, Vector3.right, distance, ObstaclesLayerMask, Color.yellow, true);
			if (_raycastRight.collider != null) { DetectedObstacleRight = _raycastRight.collider.gameObject; CollidingWithCardinalObstacle = true; } else { DetectedObstacleRight = null; }
			_raycastLeft = MMDebug.RayCast(this.transform.position + offset, Vector3.left, distance, ObstaclesLayerMask, Color.yellow, true);
			if (_raycastLeft.collider != null) { DetectedObstacleLeft = _raycastLeft.collider.gameObject; CollidingWithCardinalObstacle = true; } else { DetectedObstacleLeft = null; }
			_raycastUp = MMDebug.RayCast(this.transform.position + offset, Vector3.up, distance, ObstaclesLayerMask, Color.yellow, true);
			if (_raycastUp.collider != null) { DetectedObstacleUp = _raycastUp.collider.gameObject; CollidingWithCardinalObstacle = true; } else { DetectedObstacleUp = null; }
			_raycastDown = MMDebug.RayCast(this.transform.position + offset, Vector3.down, distance, ObstaclesLayerMask, Color.yellow, true);
			if (_raycastDown.collider != null) { DetectedObstacleDown = _raycastDown.collider.gameObject; CollidingWithCardinalObstacle = true; } else { DetectedObstacleDown = null; }
		}


        /// <summary>
        /// 重置时，我们重置rb的速度
        /// </summary>
        public override void Reset()
		{
			base.Reset();
			if (_rigidBody != null)
			{
				_rigidBody.velocity = Vector2.zero;	
			}
		}
	}
}