using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将这个脚本添加到一个平台上，当可玩角色走过时，它就会倒塌
    /// Add an AutoRespawn component to your platform and it'll get reset when your character dies
    /// </summary>
    [AddComponentMenu("TopDown Engine/Environment/Falling Platform 3D")]
	public class FallingPlatform3D : TopDownMonoBehaviour 
	{
        /// 掉落平台可能的状态
        public enum FallingPlatformStates { Idle, Shaking, Falling }

		/// the platform's current state
		[MMReadOnly]
		[Tooltip("掉落平台的当前状态")]
		public FallingPlatformStates State;

		/// if this is true, the platform will fall inevitably once touched
		[Tooltip("如果这是真的，平台一旦被触碰就会不可避免地掉落")]
		public bool InevitableFall = false;
		/// the time (in seconds) before the fall of the platform
		[Tooltip("平台掉落前的时间（以秒为单位）")]
		public float TimeBeforeFall = 2f;
		/// if this is true, the object's rigidbody will be turned non kinematic when falling. Only works in 3D.
		[Tooltip("如果这是真的，那么当物体下落时，其刚体将变为非运动学状态。这只在3D环境中有效")]
		public bool UsePhysics = true;
		/// the speed at which the platforms falls
		[Tooltip("平台下落的速度")]
		public float NonPhysicsFallSpeed = 2f;

        // 私有物品
        protected Animator _animator;
		protected Vector2 _newPosition;
		protected Bounds _bounds;
		protected Collider _collider;
		protected Vector3 _initialPosition;
		protected float _timer;
		protected float _platformTopY;
		protected Rigidbody _rigidbody;
		protected bool _contact = false;

        /// <summary>
        /// 初始化
        /// </summary>
        protected virtual void Start()
		{
			Initialization ();
		}

        /// <summary>
        /// 抓取组件并保存初始位置和计时器
        /// </summary>
        protected virtual void Initialization()
		{
			// we get the animator
			State = FallingPlatformStates.Idle;
			_animator = GetComponent<Animator>();
			_collider = GetComponent<Collider> ();
			_bounds=LevelManager.Instance.LevelBounds;
			_initialPosition = this.transform.position;
			_timer = TimeBeforeFall;
			_rigidbody = GetComponent<Rigidbody> ();

		}

        /// <summary>
        /// 每帧被调用
        /// </summary>
        protected virtual void FixedUpdate()
		{
            // 我们将各种状态发送给动画器			
            UpdateAnimator();	

			if (_contact)
			{
				_timer -= Time.deltaTime;
			}

			if (_timer < 0)
			{
				State = FallingPlatformStates.Falling;
				if (UsePhysics)
				{
					_rigidbody.isKinematic = false;
				}
				else
				{
					_newPosition = new Vector2(0,-NonPhysicsFallSpeed*Time.deltaTime);
					transform.Translate(_newPosition,Space.World);

					if (transform.position.y < _bounds.min.y)
					{
						DisableFallingPlatform ();
					}	
				}
			}
		}

        /// <summary>
        /// 禁用掉落平台。我们不销毁它，因此我们可以在重生时复活它
        /// </summary>
        protected virtual void DisableFallingPlatform()
		{
			this.gameObject.SetActive (false);					
			this.transform.position = _initialPosition;		
			_timer = TimeBeforeFall;
			State = FallingPlatformStates.Idle;
		}

        /// <summary>
        /// 更新方块的动画器
        /// </summary>
        protected virtual void UpdateAnimator()
		{				
			if (_animator!=null)
			{
				MMAnimatorExtensions.UpdateAnimatorBool(_animator, "Shaking", (State == FallingPlatformStates.Shaking));	
			}
		}

        /// <summary>
        /// 当一个TopDownController触碰到平台时触发
        /// </summary>
        /// <param name="controller">The TopDown controller that collides with the platform.</param>		
        public virtual void OnTriggerStay(Collider collider)
		{
			TopDownController controller = collider.gameObject.MMGetComponentNoAlloc<TopDownController>();
			if (controller==null)
			{
				return;
			}

			if (State == FallingPlatformStates.Falling)
			{
				return;
			}

			if (TimeBeforeFall>0)
			{
				_contact = true;
				State = FallingPlatformStates.Shaking;
			}	
			else
			{
				if (!InevitableFall)
				{
					_contact = false;
					State = FallingPlatformStates.Idle;
				}
			}
		}
        /// <summary>
        /// 当一个TopDownController离开平台时触发
        /// </summary>
        /// <param name="controller">The TopDown controller that collides with the platform.</param>
        protected virtual void OnTriggerExit(Collider collider)
		{
			if (InevitableFall)
			{
				return;
			}

			TopDownController controller = collider.gameObject.GetComponent<TopDownController>();
			if (controller==null)
				return;

			_contact = false;
			if (State == FallingPlatformStates.Shaking)
			{
				State = FallingPlatformStates.Idle;
			}
		}

        /// <summary>
        /// 在复活时，我们恢复这个平台的状态
        /// </summary>
        protected virtual void OnRevive()
		{
			this.transform.position = _initialPosition;		
			_timer = TimeBeforeFall;
			State = FallingPlatformStates.Idle;

		}
	}
}