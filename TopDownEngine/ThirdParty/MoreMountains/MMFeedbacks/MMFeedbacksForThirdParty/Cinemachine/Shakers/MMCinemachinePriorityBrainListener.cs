using System.Collections;
using UnityEngine;
#if MM_CINEMACHINE
using Cinemachine;
#elif MM_CINEMACHINE3
using Unity.Cinemachine;
#endif
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
    /// <summary>
    /// 将此添加到 Cinemachine 大脑中，它将能够接受自定义的混合过渡效果（与 MMFeedbackCinemachineTransition 配合使用）。 
    /// </summary>
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Cinemachine/MM Cinemachine Priority Brain Listener")]
	#if MM_CINEMACHINE || MM_CINEMACHINE3
	[RequireComponent(typeof(CinemachineBrain))]
	#endif
	public class MMCinemachinePriorityBrainListener : MonoBehaviour
	{
        
		[HideInInspector] 
		public TimescaleModes TimescaleMode = TimescaleModes.Scaled;
        
        
		public virtual float GetTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.time : Time.unscaledTime; }
		public virtual float GetDeltaTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; }
    
		#if MM_CINEMACHINE || MM_CINEMACHINE3
		protected CinemachineBrain _brain;
		protected CinemachineBlendDefinition _initialDefinition;
		#endif
		protected Coroutine _coroutine;

        /// <summary>
        /// 在唤醒（Awake）阶段，我们获取我们的Cinemachine大脑。  
        /// </summary>
        protected virtual void Awake()
		{
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			_brain = this.gameObject.GetComponent<CinemachineBrain>();
			#endif
		}

#if MM_CINEMACHINE || MM_CINEMACHINE3
        /// <summary>
        /// 当接收到一个事件时，如果有需要，我们会更改默认的过渡效果。 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="forceMaxPriority"></param>
        /// <param name="newPriority"></param>
        /// <param name="forceTransition"></param>
        /// <param name="blendDefinition"></param>
        /// <param name="resetValuesAfterTransition"></param>
        public virtual void OnMMCinemachinePriorityEvent(MMChannelData channelData, bool forceMaxPriority, int newPriority, bool forceTransition, CinemachineBlendDefinition blendDefinition, bool resetValuesAfterTransition, TimescaleModes timescaleMode, bool restore = false)
		{
			if (forceTransition)
			{
				if (_coroutine != null)
				{
					StopCoroutine(_coroutine);
				}
				else
				{
					#if MM_CINEMACHINE
					_initialDefinition = _brain.m_DefaultBlend;
					#elif MM_CINEMACHINE3
					_initialDefinition = _brain.DefaultBlend;
					#endif
				}
				#if MM_CINEMACHINE
					_brain.m_DefaultBlend = blendDefinition;
				#elif MM_CINEMACHINE3
					_brain.DefaultBlend = blendDefinition;
				#endif
				TimescaleMode = timescaleMode;
				#if MM_CINEMACHINE
				_coroutine = StartCoroutine(ResetBlendDefinition(blendDefinition.m_Time));    
				#elif MM_CINEMACHINE3
				_coroutine = StartCoroutine(ResetBlendDefinition(blendDefinition.Time));    
				#endif            
			}
		}
#endif

        /// <summary>
        /// 一个用于将默认过渡效果重置为其初始值的协程 
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        protected virtual IEnumerator ResetBlendDefinition(float delay)
		{
			for (float timer = 0; timer < delay; timer += GetDeltaTime())
			{
				yield return null;
			}
			#if MM_CINEMACHINE
			_brain.m_DefaultBlend = _initialDefinition;
			#elif MM_CINEMACHINE3
			_brain.DefaultBlend = _initialDefinition;
			#endif
			_coroutine = null;
		}

        /// <summary>
        /// 在启用时，我们开始监听事件。
        /// </summary>
        protected virtual void OnEnable()
		{
			_coroutine = null;
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			MMCinemachinePriorityEvent.Register(OnMMCinemachinePriorityEvent);
			#endif
		}

        /// <summary>
        /// 停止监听事件
        /// </summary>
        protected virtual void OnDisable()
		{
			if (_coroutine != null)
			{
				StopCoroutine(_coroutine);
			}
			_coroutine = null;
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			MMCinemachinePriorityEvent.Unregister(OnMMCinemachinePriorityEvent);
			#endif
		}
	}
}