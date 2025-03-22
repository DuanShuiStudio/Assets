using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
#if MM_CINEMACHINE
using Cinemachine;
#elif MM_CINEMACHINE3
using Unity.Cinemachine;
#endif

namespace MoreMountains.TopDownEngine
{
	public enum MMCinemachineBrainEventTypes { ChangeBlendDuration }

    /// <summary>
    /// 一个用于与相机大脑交互的事件
    /// </summary>
    public struct MMCinemachineBrainEvent
	{
		public MMCinemachineBrainEventTypes EventType;
		public float Duration;

		public MMCinemachineBrainEvent(MMCinemachineBrainEventTypes eventType, float duration)
		{
			EventType = eventType;
			Duration = duration;
		}

		static MMCinemachineBrainEvent e;
		public static void Trigger(MMCinemachineBrainEventTypes eventType, float duration)
		{
			e.EventType = eventType;
			e.Duration = duration;
			MMEventManager.TriggerEvent(e);
		}
	}

    /// <summary>
    /// 这个类设计用于控制CinemachineBrains，使你能够通过来自任何类的事件的默认混合值进行控制
    /// </summary>
#if MM_CINEMACHINE || MM_CINEMACHINE3
    [RequireComponent(typeof(CinemachineBrain))]
	#endif
	public class CinemachineBrainController : TopDownMonoBehaviour, MMEventListener<MMCinemachineBrainEvent>
	{
		#if MM_CINEMACHINE || MM_CINEMACHINE3
		protected CinemachineBrain _brain;
#endif

        /// <summary>
        /// 在唤醒时，我们存储大脑的引用
        /// </summary>
        protected virtual void Awake()
		{
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			_brain = this.gameObject.GetComponent<CinemachineBrain>();
			#endif
		}

        /// <summary>
        /// 将此大脑的默认混合持续时间更改为参数中设置的持续时间
        /// </summary>
        /// <param name="newDuration"></param>
        public virtual void SetDefaultBlendDuration(float newDuration)
		{
			#if MM_CINEMACHINE 
			_brain.m_DefaultBlend.m_Time = newDuration;
			#elif MM_CINEMACHINE3
			_brain.DefaultBlend.Time = newDuration;
			#endif
		}

        /// <summary>
        /// 当我们收到一个大脑事件时，我们对其进行处理
        /// </summary>
        /// <param name="cinemachineBrainEvent"></param>
        public virtual void OnMMEvent(MMCinemachineBrainEvent cinemachineBrainEvent)
		{
			switch (cinemachineBrainEvent.EventType)
			{
				case MMCinemachineBrainEventTypes.ChangeBlendDuration:
					SetDefaultBlendDuration(cinemachineBrainEvent.Duration);
					break;
			}
		}

        /// <summary>
        /// 在启用时，我们开始监听事件
        /// </summary>
        protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMCinemachineBrainEvent>();
		}

        /// <summary>
        /// 在禁用时，我们停止监听事件
        /// </summary>
        protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMCinemachineBrainEvent>();
		}
	}
}