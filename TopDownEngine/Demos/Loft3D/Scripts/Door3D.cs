using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
/*
 *demo
 *demo
 *demo
 *demo
 */
namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 一个用于处理Loft演示中3D门的类
    /// </summary>
    public class Door3D : TopDownMonoBehaviour
	{
		[Header("demo-Angles角度")]

		/// the min angle the door can open at
		[Tooltip("demo-门可以打开的最小角度")]
		public float MinAngle = 90f;
		/// the max angle the door can open at
		[Tooltip("demo-门可以打开的最大角度")]
		public float MaxAngle = 270f;
		/// the min angle at which the door locks when open
		[Tooltip("demo-门打开时锁定的最小角度")]
		public float MinAngleLock = 90f;
		/// the max angle at which the door locks when open
		[Tooltip("demo-门打开时锁定的最大角度")]
		public float MaxAngleLock = 270f;
		[Header("demo-Safe Lock安全锁定")]
		/// the duration of the "safe lock", a period during which the door is set to kinematic, to prevent glitches. That period ends after that safe lock duration, once the player has exited the door's area
		[Tooltip("demo-“安全锁定”的持续时间，在此期间门被设置为运动学状态，以防止滑动。当玩家离开门的区域后，这个时间段就会结束")]
		public float SafeLockDuration = 1f;

		[Header("demo-Binding绑定")]

		/// the rigidbody associated to this door
		[Tooltip("demo-与该门相关联的刚体")]
		public Rigidbody Door;

		protected Vector3 _eulerAngles;
		protected Vector3 _initialPosition;
		protected Vector2 _initialDirection;
		protected Vector2 _currentDirection;
		protected float _lastContactTimestamp;
		protected Vector3 _minAngleRotation;
		protected Vector3 _maxAngleRotation;

        /// <summary>
        /// 在开始时，我们计算初始方向和旋转
        /// </summary>
        protected virtual void Start()
		{
			Door = Door.gameObject.GetComponent<Rigidbody>();
			_initialDirection.x = Door.transform.right.x;
			_initialDirection.y = Door.transform.right.z;

			_minAngleRotation = Vector3.zero;
			_minAngleRotation.y = MinAngleLock;
			_maxAngleRotation = Vector3.zero;
			_maxAngleRotation.y = MaxAngleLock;

			_initialPosition = Door.transform.position;
		}

        /// <summary>
        /// 在更新时，如果需要的话，我们会锁定门。
        /// </summary>
        protected virtual void Update()
		{
			_currentDirection.x = Door.transform.right.x;
			_currentDirection.y = Door.transform.right.z;
			float Angle = MMMaths.AngleBetween(_initialDirection, _currentDirection);

			if ((Angle > MinAngle) && (Angle < MaxAngle) && (!Door.isKinematic))
			{
				if (Angle > 180)
				{
					Door.transform.localRotation = Quaternion.Euler(_maxAngleRotation);
				}
				else
				{
					Door.transform.localRotation = Quaternion.Euler(_minAngleRotation);
				}
				Door.transform.position = _initialPosition;
				Door.collisionDetectionMode = CollisionDetectionMode.Discrete;
				Door.isKinematic = true;
			}

            // 如果经过了足够的时间，我们会重置门
            if ((Time.time - _lastContactTimestamp > SafeLockDuration) && (Door.isKinematic))
			{
				Door.isKinematic = false;
			}
		}

        /// <summary>
        /// 当我们与某物发生碰撞时，我们会存储时间戳以供将来使用
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void OnTriggerStay(Collider collider)
		{
			_lastContactTimestamp = Time.time;
		}
	}
}