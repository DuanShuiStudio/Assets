using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此脚本添加到平台，当可玩角色走在上面时它会掉落
    /// 将一个AutoRespawn组件添加到您的平台，当您的角色死亡时它将被重置
    /// </summary>
    [AddComponentMenu("TopDown Engine/Environment/Falling Platform 2D")]
	public class FallingPlatform2D : TopDownMonoBehaviour 
	{
        /// 掉落平台可能的状态
        public enum FallingPlatformStates { Idle, Shaking, Falling, ColliderOff }

		/// the current state of the falling platform
		[MMReadOnly]
		[Tooltip("掉落平台的当前状态")]
		public FallingPlatformStates State;

		/// if this is true, the platform will fall inevitably once touched
		[Tooltip("如果这是真的，平台一旦被触碰就会不可避免地掉落")]
		public bool InevitableFall = false;
		/// the time (in seconds) before the fall of the platform
		[Tooltip("平台掉落前的时间（以秒为单位）")]
		public float TimeBeforeFall = 2f;
		/// the time (in seconds) before the collider turns itself off once the fall has started
		[Tooltip("平台开始掉落后，碰撞体关闭自己的时间（以秒为单位）")]
		public float DelayBetweenFallAndColliderOff = 0.5f;

        // 私有物品
        protected Animator _animator;
		protected Vector2 _newPosition;
		protected Bounds _bounds;
		protected Collider2D _collider;
		protected Vector3 _initialPosition;
		protected float _timeLeftBeforeFall;
		protected float _fallStartedAt;
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
			_collider = GetComponent<Collider2D> ();
			_collider.enabled = true;
			_bounds =LevelManager.Instance.LevelBounds;
			_initialPosition = this.transform.position;
			_timeLeftBeforeFall = TimeBeforeFall;

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
				_timeLeftBeforeFall -= Time.deltaTime;
			}

			if (_timeLeftBeforeFall < 0)
			{
				if (State != FallingPlatformStates.Falling)
				{
					_fallStartedAt = Time.time;
				}
				State = FallingPlatformStates.Falling;
			}

			if (State == FallingPlatformStates.Falling)
			{
				if (Time.time - _fallStartedAt >= DelayBetweenFallAndColliderOff)
				{
					_collider.enabled = false;
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
			_timeLeftBeforeFall = TimeBeforeFall;
			State = FallingPlatformStates.Idle;
		}

        /// <summary>
        /// 更新方块的动画器
        /// </summary>
        protected virtual void UpdateAnimator()
		{				
			if (_animator!=null)
			{
				MMAnimatorExtensions.UpdateAnimatorBool(_animator, "Idle", (State == FallingPlatformStates.Idle));
				MMAnimatorExtensions.UpdateAnimatorBool(_animator, "Shaking", (State == FallingPlatformStates.Shaking));
				MMAnimatorExtensions.UpdateAnimatorBool(_animator, "Falling", (State == FallingPlatformStates.Falling));
			}
		}

        /// <summary>
        /// 当一个TopDownController触碰到平台时触发
        /// </summary>
        /// <param name="controller">The TopDown controller that collides with the platform.</param>		
        public virtual void OnTriggerStay2D(Collider2D collider)
		{
			TopDownController2D controller = collider.gameObject.MMGetComponentNoAlloc<TopDownController2D>();
			if (controller == null)
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
        protected virtual void OnTriggerExit2D(Collider2D collider)
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
			_timeLeftBeforeFall = TimeBeforeFall;
			State = FallingPlatformStates.Idle;

		}
	}
}