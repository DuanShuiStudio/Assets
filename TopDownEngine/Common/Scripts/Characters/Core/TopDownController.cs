using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 不要直接使用这个类，使用TopDownController2D的2D角色，或TopDownController3D的3D角色
    /// 这两个类都继承自这个类
    /// </summary>
    public abstract class  TopDownController : TopDownMonoBehaviour 
	{
		[Header("Gravity重力")]
		/// the current gravity to apply to our character (positive goes down, negative goes up, higher value, higher acceleration)
		[Tooltip("当前应用于角色的重力（正值向下，负值向上，数值越大，加速度越大）")]
		public float Gravity = 40f;
		/// whether or not the gravity is currently being applied to this character
		[Tooltip("重力是否被应用到当前这个角色")]
		public bool GravityActive = true;

		[Header("通用射线投射")]
		/// by default, the length of the raycasts used to get back to normal size will be auto generated based on your character's normal/standing height, but here you can specify a different value
		[Tooltip("默认情况下，用于恢复到正常大小的射线投射长度将基于角色的正常/站立高度自动生成，但在这里你可以指定一个不同的值")]
		public float CrouchedRaycastLengthMultiplier = 1f;
		/// if this is true, extra raycasts will be cast on all 4 sides to detect obstacles and feed the CollidingWithCardinalObstacle bool, only useful when working with grid movement, or if you need that info for some reason
		[Tooltip("如果这个值为真，将在所有四个侧面投射额外的射线以检测障碍物，并反馈到‘CollidingWithCardinalObstacle’布尔值中，仅在处理网格移动时有用，或者如果你出于某种原因需要这些信息")]
		public bool PerformCardinalObstacleRaycastDetection = false;

		/// the current speed of the character
		[MMReadOnly]
		[Tooltip("角色当前的速度")]
		public Vector3 Speed;
		/// the current velocity
		[MMReadOnly]
		[Tooltip("以单位/秒为单位的当前速度")]
		public Vector3 Velocity;
		/// the velocity of the character last frame
		[MMReadOnly]
		[Tooltip("上一帧角色的速度")]
		public Vector3 VelocityLastFrame;
		/// the current acceleration
		[MMReadOnly]
		[Tooltip("当前加速度")]
		public Vector3 Acceleration;
		/// whether or not the character is grounded
		[MMReadOnly]
		[Tooltip("角色是否接地")]
		public bool Grounded;
		/// whether or not the character got grounded this frame
		[MMReadOnly]
		[Tooltip("角色是否在这一帧接地")]
		public bool JustGotGrounded;
		/// the current movement of the character
		[MMReadOnly]
		[Tooltip("角色当前的运动")]
		public Vector3 CurrentMovement;
		/// the direction the character is going in
		[MMReadOnly]
		[Tooltip("角色前进的方向")]
		public Vector3 CurrentDirection;
		/// the current friction
		[MMReadOnly]
		[Tooltip("当前的摩擦")]
		public float Friction;
		/// the current added force, to be added to the character's movement
		[MMReadOnly]
		[Tooltip("当前添加的力量，要添加到角色的运动中")]
		public Vector3 AddedForce;
		/// whether or not the character is in free movement mode or not
		[MMReadOnly]
		[Tooltip("角色是否处于自由移动模式")]
		public bool FreeMovement = true;

        /// 对撞机的中心坐标
        public virtual Vector3 ColliderCenter { get { return Vector3.zero; }  }
        /// 对撞机的底部坐标
        public virtual Vector3 ColliderBottom { get { return Vector3.zero; }  }
        /// 对撞机的顶部坐标
        public virtual Vector3 ColliderTop { get { return Vector3.zero; }  }
        /// 对象（如果有的话）低于我们的角色
        public virtual GameObject ObjectBelow { get; set; }
        /// 角色下面的表面修饰器对象（如果有的话）
        public virtual SurfaceModifier SurfaceModifierBelow { get; set; }
		public virtual Vector3 AppliedImpact { get { return _impact; } }
        /// 角色是否在移动平台上
        public virtual bool OnAMovingPlatform { get; set; }
        /// 移动平台的速度
        public virtual Vector3 MovingPlatformSpeed { get; set; }

        // 留给这个控制器的障碍（只在DetectObstacles被调用时更新）
        public virtual GameObject DetectedObstacleLeft { get; set; }
        // 这个控制器的障碍物（只在DetectObstacles被调用时更新）
        public virtual GameObject DetectedObstacleRight { get; set; }
        // 这个控制器的障碍物（仅在DetectObstacles被调用时更新）
        public virtual GameObject DetectedObstacleUp { get; set; }
        // 将障碍物向下移动到这个控制器（仅在DetectObstacles被调用时更新）
        public virtual GameObject DetectedObstacleDown { get; set; }
        // 如果在任何一个主要方向检测到障碍物，则为True
        public virtual bool CollidingWithCardinalObstacle { get; set; }

		protected Vector3 _positionLastFrame;
		protected Vector3 _speedComputation;
		protected bool _groundedLastFrame;
		protected Vector3 _impact;		
		protected const float _smallValue=0.0001f;

        /// <summary>
        /// 在awake时，我们初始化当前方向
        /// </summary>
        protected virtual void Awake()
		{			
			CurrentDirection = transform.forward;
		}

        /// <summary>
        /// 更新后，我们会检查我们是否停飞，并确定方向
        /// </summary>
        protected virtual void Update()
		{
			CheckIfGrounded ();
			HandleFriction ();
			DetermineDirection ();
		}

        /// <summary>
        /// 计算速度
        /// </summary>
        protected virtual void ComputeSpeed ()
		{
			if (Time.deltaTime != 0f)
			{
				Speed = (this.transform.position - _positionLastFrame) / Time.deltaTime;
			}
            // 把速度四舍五入到小数点后2位
            Speed.x = Mathf.Round(Speed.x * 100f) / 100f;
			Speed.y = Mathf.Round(Speed.y * 100f) / 100f;
			Speed.z = Mathf.Round(Speed.z * 100f) / 100f;
			_positionLastFrame = this.transform.position;
		}

        /// <summary>
        /// 确定控制器的当前方向
        /// </summary>
        protected virtual void DetermineDirection()
		{
			
		}

        /// <summary>
        /// “手动”执行障碍物检测
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="offset"></param>
        public virtual void DetectObstacles(float distance, Vector3 offset)
		{

		}

        /// <summary>
        /// 在fixeduupdate时调用
        /// </summary>
        protected virtual void FixedUpdate()
		{

		}

        /// <summary>
        /// 在LateUpdate上，计算代理的速度
        /// </summary>
        protected virtual void LateUpdate()
		{
		}

        /// <summary>
        /// 检查角色是否禁锢
        /// </summary>
        protected virtual void CheckIfGrounded()
		{
			JustGotGrounded = (!_groundedLastFrame && Grounded);
			_groundedLastFrame = Grounded;
		}

        /// <summary>
        /// 使用此参数对控制器施加冲击，以指定的力将其向指定的方向移动
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="force"></param>
        public virtual void Impact(Vector3 direction, float force)
		{

		}

        /// <summary>
        /// 设置重力活动或非活动
        /// </summary>
        /// <param name="status"></param>
        public virtual void SetGravityActive(bool status)
		{
			GravityActive = status;
		}

        /// <summary>
        /// 将指定的力添加到控制器
        /// </summary>
        /// <param name="movement"></param>
        public virtual void AddForce(Vector3 movement)
		{

		}

        /// <summary>
        /// 将控制器的当前运动设置为指定的Vector3
        /// </summary>
        /// <param name="movement"></param>
        public virtual void SetMovement(Vector3 movement)
		{

		}

        /// <summary>
        /// 将控制器移动到指定位置（在世界空间中）
        /// </summary>
        /// <param name="newPosition"></param>
        public virtual void MovePosition(Vector3 newPosition, bool targetTransform = false)
		{
			
		}

        /// <summary>
        /// 调整控制器碰撞器的大小
        /// </summary>
        /// <param name="newHeight"></param>
        public virtual void ResizeColliderHeight(float newHeight, bool translateCenter = false)
		{

		}

        /// <summary>
        /// 重置控制器的碰撞器大小
        /// </summary>
        public virtual void ResetColliderSize()
		{

		}

        /// <summary>
        /// 如果控制器的碰撞器可以回到原来的大小而不碰到障碍物则返回true，否则返回false
        /// </summary>
        /// <returns></returns>
        public virtual bool CanGoBackToOriginalSize()
		{
			return true;
		}

        /// <summary>
        /// 打开控制器的碰撞
        /// </summary>
        public virtual void CollisionsOn()
		{

		}

        /// <summary>
        /// 关闭控制器的碰撞
        /// </summary>
        public virtual void CollisionsOff()
		{

		}

        /// <summary>
        /// 将控制器的刚体设置为运动学（或非运动学）
        /// </summary>
        /// <param name="state"></param>
        public virtual void SetKinematic(bool state)
		{

		}

        /// <summary>
        /// 处理摩擦碰撞
        /// </summary>
        protected virtual void HandleFriction()
		{

		}

        /// <summary>
        /// 重置该控制器的所有值
        /// </summary>
        public virtual void Reset()
		{
			_impact = Vector3.zero;
			GravityActive = true;
			Speed = Vector3.zero;
			Velocity = Vector3.zero;
			VelocityLastFrame = Vector3.zero;
			Acceleration = Vector3.zero;
			Grounded = true;
			JustGotGrounded = false;
			CurrentMovement = Vector3.zero;
			CurrentDirection = Vector3.zero;
			AddedForce = Vector3.zero;
		}
	}
}