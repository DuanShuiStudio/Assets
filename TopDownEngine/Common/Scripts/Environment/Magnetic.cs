using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
	public class Magnetic : TopDownMonoBehaviour
	{
		/// the possible update modes
		public enum UpdateModes { Update, FixedUpdate, LateUpdate }

		[Header("Magnetic磁性")]        
		/// the layermask this magnetic element is attracted to
		[Tooltip("这个磁性元素被吸引的层掩码")]
		public LayerMask TargetLayerMask = LayerManager.PlayerLayerMask;
		/// whether or not to start moving when something on the target layer mask enters this magnetic element's trigger
		[Tooltip("当目标层掩码上的某物进入这个磁性元素的触发器时，是否开始移动")]
		public bool StartMagnetOnEnter = true;
		/// whether or not to stop moving when something on the target layer mask exits this magnetic element's trigger
		[Tooltip("当目标层掩码上的某物离开这个磁性元素的触发器时，是否停止移动")]
		public bool StopMagnetOnExit = false;
		/// a unique ID for this type of magnetic objects. This can then be used by a MagneticEnabler to target only that specific ID. An ID of 0 will be picked by all MagneticEnablers automatically.
		[Tooltip("这种磁性物体的唯一ID。然后，一个MagneticEnabler可以使用它来仅目标定位那个特定的ID。所有MagneticEnablers将自动选择ID为0的物体")]
		public int MagneticTypeID = 0;

		[Header("Target目标")]
		/// the offset to apply to the followed target
		[Tooltip("要应用到跟随目标的偏移量")]
		public Vector3 Offset;

		[Header("Position Interpolation位置插值")]
		/// whether or not we need to interpolate the movement
		[Tooltip("我们是否需要插值运动")]
		public bool InterpolatePosition = true;
		/// the speed at which to interpolate the follower's movement
		[MMCondition("InterpolatePosition", true)]
		[Tooltip("插值跟随者运动的速度")]
		public float FollowPositionSpeed = 5f;
		/// the acceleration to apply to the object once it starts following
		[MMCondition("InterpolatePosition", true)]
		[Tooltip("一旦开始跟随，要应用到该物体上的加速度")]
		public float FollowAcceleration = 0.75f;

		[Header("Mode模式")]
		/// the update at which the movement happens
		[Tooltip("运动发生的更新时刻")]
		public UpdateModes UpdateMode = UpdateModes.Update;

		[Header("State状态")]
		/// an object this magnetic object should copy the active state on
		[Tooltip("这个磁性物体应该复制其活动状态的对象")]
		public GameObject CopyState;
		
		[Header("Debug调试")]
		/// the target to follow, read only, for debug only
		[Tooltip("要跟随的目标，只读，仅用于调试")]
		[MMReadOnly]
		public Transform Target;
		/// whether or not the object is currently following its target's position
		[Tooltip("该物体当前是否正在跟随其目标的位置")]
		[MMReadOnly]
		public bool FollowPosition = true;

		protected Collider2D _collider2D;
		protected Collider _collider;
		protected Vector3 _velocity = Vector3.zero;
		protected Vector3 _newTargetPosition;
		protected Vector3 _lastTargetPosition;
		protected Vector3 _direction;
		protected Vector3 _newPosition;
		protected float _speed;
		protected Vector3 _initialPosition;

        /// <summary>
        /// 在Awake时，我们初始化我们的磁铁
        /// </summary>
        protected virtual void Awake()
		{
			Initialization();
		}
		protected virtual void OnEnable()
		{
			Reset();
		}

        /// <summary>
        /// 抓取碰撞器并确保它被设置为触发器，初始化速度
        /// </summary>
        protected virtual void Initialization()
		{
			_initialPosition = this.transform.position;
			
			_collider2D = this.gameObject.GetComponent<Collider2D>();
			if (_collider2D != null) { _collider2D.isTrigger = true; }
			
			_collider = this.gameObject.GetComponent<Collider>();
			if (_collider != null) { _collider.isTrigger = true; }
			
			Reset();
		}

        /// <summary>
        /// 调用这个来重置这个磁性物体的目标和速度
        /// </summary>
        public virtual void Reset()
		{
			Target = null;
			_speed = 0f;
		}

        /// <summary>
        /// 调用这个来重置磁性物体的位置到其初始位置。
        /// </summary>
        public virtual void ResetPosition()
		{
			this.transform.position = _initialPosition;
			Reset();
		}

        /// <summary>
        /// 当有东西进入我们的触发器时，如果它是一个合适的目标，我们开始跟随它
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void OnTriggerEnter2D(Collider2D colliding)
		{
			OnTriggerEnterInternal(colliding.gameObject);
		}

        /// <summary>
        /// 当有东西离开我们的触发器时，我们停止跟随它
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void OnTriggerExit2D(Collider2D colliding)
		{
			OnTriggerExitInternal(colliding.gameObject);
		}

        /// <summary>
        /// 当有东西进入我们的触发器时，如果它是一个合适的目标，我们开始跟随它
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void OnTriggerEnter(Collider colliding)
		{
			OnTriggerEnterInternal(colliding.gameObject);
		}

        /// <summary>
        /// 当有东西离开我们的触发器时，我们停止跟随它
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void OnTriggerExit(Collider colliding)
		{
			OnTriggerExitInternal(colliding.gameObject);
		}

        /// <summary>
        /// 如果满足条件，开始跟随我们触发的对象
        /// </summary>
        /// <param name="colliding"></param>
        protected virtual void OnTriggerEnterInternal(GameObject colliding)
		{
			if (!StartMagnetOnEnter)
			{
				return;
			}

			if (!TargetLayerMask.MMContains(colliding.layer))
			{
				return;
			}

			Target = colliding.transform;
			StartFollowing();
		}

        /// <summary>
        /// 如果满足条件，停止跟随我们触发的对象
        /// </summary>
        /// <param name="colliding"></param>
        protected virtual void OnTriggerExitInternal(GameObject colliding)
		{
			if (!StopMagnetOnExit)
			{
				return;
			}

			if (!TargetLayerMask.MMContains(colliding.layer))
			{
				return;
			}

			StopFollowing();
		}

        /// <summary>
        /// 在更新时，我们跟随我们的目标
        /// </summary>
        protected virtual void Update()
		{
			if (CopyState != null)
			{
				this.gameObject.SetActive(CopyState.activeInHierarchy);
			}            

			if (Target == null)
			{
				return;
			}
			if (UpdateMode == UpdateModes.Update)
			{
				FollowTargetPosition();
			}
		}

        /// <summary>
        /// 在固定的更新时刻，我们跟随我们的目标
        /// </summary>
        protected virtual void FixedUpdate()
		{
			if (UpdateMode == UpdateModes.FixedUpdate)
			{
				FollowTargetPosition();
			}
		}

        /// <summary>
        /// 在较晚的更新时刻，我们跟随我们的目标。
        /// </summary>
        protected virtual void LateUpdate()
		{
			if (UpdateMode == UpdateModes.LateUpdate)
			{
				FollowTargetPosition();
			}
		}

        /// <summary>
        /// 跟随目标，根据检查器中定义的内容进行位置插值或不插值。
        /// </summary>
        protected virtual void FollowTargetPosition()
		{
			if (Target == null)
			{
				return;
			}

			if (!FollowPosition)
			{
				return;
			}

			_newTargetPosition = Target.position + Offset;

			float trueDistance = 0f;
			_direction = (_newTargetPosition - this.transform.position).normalized;
			trueDistance = Vector3.Distance(this.transform.position, _newTargetPosition);

			_speed = (_speed < FollowPositionSpeed) ? _speed + FollowAcceleration * Time.deltaTime : FollowPositionSpeed;

			float interpolatedDistance = trueDistance;
			if (InterpolatePosition)
			{
				interpolatedDistance = MMMaths.Lerp(0f, trueDistance, _speed, Time.deltaTime);
				this.transform.Translate(_direction * interpolatedDistance, Space.World);
			}
			else
			{
				this.transform.Translate(_direction * interpolatedDistance, Space.World);
			}
		}

        /// <summary>
        /// 防止物体再次跟随目标。
        /// </summary>
        public virtual void StopFollowing()
		{
			FollowPosition = false;
		}

        /// <summary>
        /// 使物体跟随目标。
        /// </summary>
        public virtual void StartFollowing()
		{
			FollowPosition = true;
		}

        /// <summary>
        /// 为这个物体设置一个新的目标，使其产生磁性吸引
        /// </summary>
        /// <param name="newTarget"></param>
        public virtual void SetTarget(Transform newTarget)
		{
			Target = newTarget;
		}
	}
}