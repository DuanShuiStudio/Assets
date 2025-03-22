using UnityEngine;
using System.Collections;
using MoreMountains.Feedbacks;
using System;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 将这个类添加到一个你期望从 objectPooler 中池化的对象上
    /// 请注意，这些对象不能通过调用 Destroy() 来销毁，它们只会被设置为非活动状态（这正是整个要点）
    /// </summary>
    public class MMMiniPoolableObject : MonoBehaviour 
	{
		public delegate void Events();
		public event Events OnSpawnComplete;

        /// 对象的生存时间（以秒为单位）。如果设置为 0，它将永远存活；如果设置为任何正数，在那个时间后它会被设置为非活动状态
        public float LifeTime = 0f;

        /// <summary>
        /// 将实例设置为非活动状态，以便最终重用它
        /// </summary>
        public virtual void Destroy()
		{
			gameObject.SetActive(false);
		}

        /// <summary>
        /// 当对象被启用时（通常在从 ObjectPooler 中取出后），我们开始它的死亡倒计时
        /// </summary>
        protected virtual void OnEnable()
		{
			if (LifeTime > 0)
			{
				Invoke("Destroy", LifeTime);	
			}
		}

        /// <summary>
        /// 当对象被禁用时（可能是因为它超出了边界），我们取消它的预定死亡
        /// </summary>
        protected virtual void OnDisable()
		{
			CancelInvoke();
		}

        /// <summary>
        /// 触发“生成完成”事件
        /// </summary>
        public virtual void TriggerOnSpawnComplete()
		{
			if(OnSpawnComplete != null)
			{
				OnSpawnComplete();
			}
		}
	}
}