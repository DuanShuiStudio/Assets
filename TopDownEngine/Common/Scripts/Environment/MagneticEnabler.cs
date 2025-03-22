using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 这个对象将启用场景中的磁性物体，当它们进入其相关联的2D碰撞器时（确保你添加了一个）
    /// 虽然磁性物体可以独立工作，并自行处理它们的范围检测，但你也可以使用不同的架构，其中一个启用器让它们移动
    /// 一个典型的使用案例是将其添加到一个角色上，并嵌套在其顶层下：
    /// 
    /// MyCharacter (top level, with a Character, controller, abilities, etc)
    /// - MyMagneticEnabler (使用此类和一个例如CircleCollider2D的圆形碰撞器作为例子)
    /// 
    /// 那么，在你的场景中，你会拥有磁性物体，并且禁用了StartMagnetOnEnter
    /// 你的磁性启用器会让它们在进入时跟随这个特定的目标
    /// 从启用器中，你还可以覆盖跟随速度和加速度。
    /// </summary>
    public class MagneticEnabler : TopDownMonoBehaviour
	{
		[Header("Detection检测")]
		/// the layermask this magnetic enabler looks at to enable magnetic elements
		[Tooltip("这个磁性启用器用于启用磁性元素的层掩码。")]
		public LayerMask TargetLayerMask = LayerManager.PlayerLayerMask;
		/// a list of the magnetic type ID this enabler targets
		[Tooltip("这个启用器目标的磁性类型ID列表")]
		public List<int> MagneticTypeIDs;

		[Header("Overrides覆盖")]
		/// if this is true, the follow position speed will be overridden with the one specified here
		[Tooltip("如果这是真的，跟随位置的速度将被这里指定的速度覆盖")]
		public bool OverrideFollowPositionSpeed = false;
		/// the value with which to override the speed
		[Tooltip("用于覆盖速度的速度")]
		[MMCondition("OverrideFollowPositionSpeed", true)]
		public float FollowPositionSpeed = 5f;
		/// if this is true, the acceleration will be overridden with the one specified here
		[Tooltip("如果这是真的，加速度将被这里指定的速度覆盖")]
		public bool OverrideFollowAcceleration = false;
		/// the value with which to override the acceleration
		[Tooltip("用于覆盖加速度的速度")]
		[MMCondition("OverrideFollowAcceleration", true)]
		public float FollowAcceleration = 0.75f;

		protected Collider2D _collider2D;
		protected Magnetic _magnetic;

        /// <summary>
        /// 在Awake时，我们初始化我们的磁性启用器
        /// </summary>
        protected virtual void Awake()
		{
			Initialization();
		}

        /// <summary>
        /// 抓取Collider2D并确保它被设置为触发器
        /// </summary>
        protected virtual void Initialization()
		{
			_collider2D = this.gameObject.GetComponent<Collider2D>();
			if (_collider2D != null)
			{
				_collider2D.isTrigger = true;
			}
		}

        /// <summary>
        /// 当有东西进入我们的2D触发器时，如果它是一个合适的目标，我们开始跟随它
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void OnTriggerEnter2D(Collider2D colliding)
		{
			OnTriggerEnterInternal(colliding.gameObject);
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
        /// 如果我们用一个磁性物体触发，并且ID匹配，我们就启用它
        /// </summary>
        /// <param name="colliding"></param>
        protected virtual void OnTriggerEnterInternal(GameObject colliding)
		{
			if (!TargetLayerMask.MMContains(colliding.layer))
			{
				return;
			}

			_magnetic = colliding.MMGetComponentNoAlloc<Magnetic>();
			if (_magnetic == null)
			{
				return;
			}

			bool idFound = false;
			if (_magnetic.MagneticTypeID == 0)
			{
				idFound = true;
			}
			else
			{
				foreach (int id in MagneticTypeIDs)
				{
					if (id == _magnetic.MagneticTypeID)
					{
						idFound = true;
					}
				}
			}            

			if (!idFound)
			{
				return;
			}

			if (OverrideFollowAcceleration)
			{
				_magnetic.FollowAcceleration = FollowAcceleration;
			}

			if (OverrideFollowPositionSpeed)
			{
				_magnetic.FollowPositionSpeed = FollowPositionSpeed;
			}

			_magnetic.SetTarget(this.transform);
			_magnetic.StartFollowing();
		}
	}
}