using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 *demo
 *demo
 *demo
 *demo
 */
namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 一个用于使背景元素跳跃的装饰性类
    /// </summary>
    public class ExplodudesDecoration : TopDownMonoBehaviour, MMEventListener<MMGameEvent>
	{
		/// the minimum force to apply to the background elements
		[Tooltip("demo-应用于背景元素的最小力")]
		public Vector3 MinForce;
		/// the maximum force to apply to the background elements
		[Tooltip("demo-应用于背景元素的最大力")]
		public Vector3 MaxForce;

		/// 一个测试按钮
		[MMInspectorButton("Jump")]
		public bool JumpButton;

		protected Rigidbody _rigidbody;
		protected const string eventName = "Bomb";
		protected Vector3 _force;

        /// <summary>
        /// 一开始我们获取刚体（rigidbody）
        /// </summary>
        protected virtual void Start()
		{
			_rigidbody = this.gameObject.GetComponent<Rigidbody>();
		}

        /// <summary>
        /// 当我们收到炸弹事件时，我们使刚体跳跃
        /// </summary>
        /// <param name="gameEvent"></param>
        public virtual void OnMMEvent(MMGameEvent gameEvent)
		{
			if (gameEvent.EventName == eventName)
			{
				Jump();
			}
		}

        /// <summary>
        /// 向刚体添加一个随机力
        /// </summary>
        public virtual void Jump()
		{
			_force.x = Random.Range(MinForce.x, MaxForce.x);
			_force.y = Random.Range(MinForce.y, MaxForce.y);
			_force.z = Random.Range(MinForce.z, MaxForce.z);
			_rigidbody.AddForce(_force, ForceMode.Impulse);
		}

        /// <summary>
        /// 启用时我们开始监听炸弹事件
        /// </summary>
        protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMGameEvent>();
		}

        /// <summary>
        /// 禁用时我们停止监听炸弹事件
        /// </summary>
        protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMGameEvent>();
		}
	}
}