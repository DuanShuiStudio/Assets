using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace  MoreMountains.Feedbacks
{
    /// <summary>
    /// 当播放一系列反馈效果时，由 MMFeedbacks 触发的事件。
    /// - play : 当一个 MMFeedbacks 开始播放时
    /// - pause : 当一个 MMFeedbacks 开始暂停时
    /// - resume : 在暂停保持后恢复播放时
    /// - revert : 当一个 MMFeedbacks 反转其播放方向时
    /// - complete : 当一个 `MMFeedbacks` 播放完其最后一个反馈效果时
    ///
    /// 要监听这些事件：
    ///
    /// public virtual void OnMMFeedbacksEvent(MMFeedbacks source, EventTypes type)
    /// {
    ///     // do something
    /// }
    /// 
    /// protected virtual void OnEnable()
    /// {
    ///     MMFeedbacksEvent.Register(OnMMFeedbacksEvent);
    /// }
    /// 
    /// protected virtual void OnDisable()
    /// {
    ///     MMFeedbacksEvent.Unregister(OnMMFeedbacksEvent);
    /// }
    /// 
    /// </summary>
    public struct MMFeedbacksEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public enum EventTypes { Play, Pause, Resume, Revert, Complete, SkipToTheEnd, RestoreInitialValues, Loop, Enable, Disable, InitializationComplete }
		public delegate void Delegate(MMFeedbacks source, EventTypes type);
		static public void Trigger(MMFeedbacks source, EventTypes type)
		{
			OnEvent?.Invoke(source, type);
		}
	}

    /// <summary>
    /// 一个用于在所有监听该事件的反馈上设置 “范围中心（RangeCenter）” 的事件。
    /// </summary>
    public struct MMSetFeedbackRangeCenterEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }
		
		public delegate void Delegate(Transform newCenter);

		static public void Trigger(Transform newCenter)
		{
			OnEvent?.Invoke(newCenter);
		}
	}

    /// <summary>
    /// `MMFeedbacks` 的一个子类，包含了可以播放的 Unity 事件。 
    /// </summary>
    [Serializable]
	public class MMFeedbacksEvents
	{
		/// whether or not this MMFeedbacks should fire MMFeedbacksEvents
		[Tooltip("这个 `MMFeedbacks` 是否应该触发 `MMFeedbacksEvents` 。 ")] 
		public bool TriggerMMFeedbacksEvents = false; 
		/// whether or not this MMFeedbacks should fire Unity Events
		[Tooltip("这个 `MMFeedbacks` 是否应该触发 Unity 事件。 ")] 
		public bool TriggerUnityEvents = true;
		/// This event will fire every time this MMFeedbacks gets played
		[Tooltip("每次这个 `MMFeedbacks` 开始播放时，此事件都会触发。")]
		public UnityEvent OnPlay;
		/// This event will fire every time this MMFeedbacks starts a holding pause
		[Tooltip("每次这个 MMFeedbacks 开始保持暂停时，此事件都会触发。")]
		public UnityEvent OnPause;
		/// This event will fire every time this MMFeedbacks resumes after a holding pause
		[Tooltip("每次这个 `MMFeedbacks` 在暂停保持后恢复播放时，此事件都会触发。 ")]
		public UnityEvent OnResume;
		/// This event will fire every time this MMFeedbacks reverts its play direction
		[Tooltip("每次这个 `MMFeedbacks` 反转其播放方向时，此事件都会触发。 ")]
		public UnityEvent OnRevert;
		/// This event will fire every time this MMFeedbacks plays its last MMFeedback
		[Tooltip("每次这个 `MMFeedbacks` 播放完其最后一个 `MMFeedback` 时，此事件将会触发。 ")]
		public UnityEvent OnComplete;
		/// This event will fire every time this MMFeedbacks gets restored to its initial values
		[Tooltip("每次这个 `MMFeedbacks` 恢复到其初始值时，此事件都会触发。")]
		public UnityEvent OnRestoreInitialValues;
		/// This event will fire every time this MMFeedbacks gets skipped to the end
		[Tooltip("每次这个 `MMFeedbacks` 被直接跳至结尾播放时，此事件都会触发。 ")]
		public UnityEvent OnSkipToTheEnd;
		/// This event will fire after the MMF Player is done initializing
		[Tooltip("在 `MMF Player` 完成初始化之后，此事件将会触发。 ")]
		public UnityEvent OnInitializationComplete;
		/// This event will fire every time this MMFeedbacks' game object gets enabled
		[Tooltip("每次这个 `MMFeedbacks` 所在的游戏对象被启用时，此事件都会触发。")]
		public UnityEvent OnEnable;
		/// This event will fire every time this MMFeedbacks' game object gets disabled
		[Tooltip("每次这个 `MMFeedbacks` 的游戏对象被禁用时，此事件都会触发。 ")]
		public UnityEvent OnDisable;

		public virtual bool OnPlayIsNull { get; protected set; }
		public virtual bool OnPauseIsNull { get; protected set; }
		public virtual bool OnResumeIsNull { get; protected set; }
		public virtual bool OnRevertIsNull { get; protected set; }
		public virtual bool OnCompleteIsNull { get; protected set; }
		public virtual bool OnRestoreInitialValuesIsNull { get; protected set; }
		public virtual bool OnSkipToTheEndIsNull { get; protected set; }
		public virtual bool OnInitializationCompleteIsNull { get; protected set; }
		public virtual bool OnEnableIsNull { get; protected set; }
		public virtual bool OnDisableIsNull { get; protected set; }

        /// <summary>
        /// 在初始化时，对于每一个事件，我们都会存储我们是否有一个事件需要去调用。 
        /// </summary>
        public virtual void Initialization()
		{
			OnPlayIsNull = OnPlay == null;
			OnPauseIsNull = OnPause == null;
			OnResumeIsNull = OnResume == null;
			OnRevertIsNull = OnRevert == null;
			OnCompleteIsNull = OnComplete == null;
			OnRestoreInitialValuesIsNull = OnRestoreInitialValues == null;
			OnSkipToTheEndIsNull = OnSkipToTheEnd == null;
			OnInitializationCompleteIsNull = OnInitializationComplete == null;
			OnEnableIsNull = OnEnable == null;
			OnDisableIsNull = OnDisable == null;
		}

        /// <summary>
        /// 如果有需要，将触发播放事件。
        /// </summary>
        /// <param name="source"></param>
        public virtual void TriggerOnPlay(MMFeedbacks source)
		{
			if (!OnPlayIsNull && TriggerUnityEvents)
			{
				OnPlay.Invoke();
			}

			if (TriggerMMFeedbacksEvents)
			{
				MMFeedbacksEvent.Trigger(source, MMFeedbacksEvent.EventTypes.Play);
			}
		}

        /// <summary>
        /// 如果有需要，就会触发暂停事件。
        /// </summary>
        /// <param name="source"></param>
        public virtual void TriggerOnPause(MMFeedbacks source)
		{
			if (!OnPauseIsNull && TriggerUnityEvents)
			{
				OnPause.Invoke();
			}

			if (TriggerMMFeedbacksEvents)
			{
				MMFeedbacksEvent.Trigger(source, MMFeedbacksEvent.EventTypes.Pause);
			}
		}

        /// <summary>
        /// 如果需要的话，会触发恢复事件。
        /// </summary>
        /// <param name="source"></param>
        public virtual void TriggerOnResume(MMFeedbacks source)
		{
			if (!OnResumeIsNull && TriggerUnityEvents)
			{
				OnResume.Invoke();
			}

			if (TriggerMMFeedbacksEvents)
			{
				MMFeedbacksEvent.Trigger(source, MMFeedbacksEvent.EventTypes.Resume);
			}
		}

        /// <summary>
        /// 如果有需要，就会触发反转事件。
        /// </summary>
        /// <param name="source"></param>
        public virtual void TriggerOnRevert(MMFeedbacks source)
		{
			if (!OnRevertIsNull && TriggerUnityEvents)
			{
				OnRevert.Invoke();
			}

			if (TriggerMMFeedbacksEvents)
			{
				MMFeedbacksEvent.Trigger(source, MMFeedbacksEvent.EventTypes.Revert);
			}
		}

        /// <summary>
        /// 如果有需要，就会触发完成事件。
        /// </summary>
        /// <param name="source"></param>
        public virtual void TriggerOnComplete(MMFeedbacks source)
		{
			if (!OnCompleteIsNull && TriggerUnityEvents)
			{
				OnComplete.Invoke();
			}

			if (TriggerMMFeedbacksEvents)
			{
				MMFeedbacksEvent.Trigger(source, MMFeedbacksEvent.EventTypes.Complete);
			}
		}

        /// <summary>
        /// 如果有需要，将触发跳过事件。
        /// </summary>
        /// <param name="source"></param>
        public virtual void TriggerOnSkipToTheEnd(MMFeedbacks source)
		{
			if (!OnSkipToTheEndIsNull && TriggerUnityEvents)
			{
				OnSkipToTheEnd.Invoke();
			}

			if (TriggerMMFeedbacksEvents)
			{
				MMFeedbacksEvent.Trigger(source, MMFeedbacksEvent.EventTypes.SkipToTheEnd);
			}
		}

		public virtual void TriggerOnInitializationComplete(MMFeedbacks source)
		{
			if (!OnInitializationCompleteIsNull && TriggerUnityEvents)
			{
				OnInitializationComplete.Invoke();
			}

			if (TriggerMMFeedbacksEvents)
			{
				MMFeedbacksEvent.Trigger(source, MMFeedbacksEvent.EventTypes.InitializationComplete);
			}
		}

        /// <summary>
        /// 如果需要的话，会触发反转事件。
        /// </summary>
        /// <param name="source"></param>
        public virtual void TriggerOnRestoreInitialValues(MMFeedbacks source)
		{
			if (!OnRestoreInitialValuesIsNull && TriggerUnityEvents)
			{
				OnRestoreInitialValues.Invoke();
			}

			if (TriggerMMFeedbacksEvents)
			{
				MMFeedbacksEvent.Trigger(source, MMFeedbacksEvent.EventTypes.RestoreInitialValues);
			}
		}

        /// <summary>
        /// 如果有需要，将触发启用事件。
        /// </summary>
        /// <param name="source"></param>
        public virtual void TriggerOnEnable(MMF_Player source)
		{
			if (!OnEnableIsNull && TriggerUnityEvents)
			{
				OnEnable.Invoke();
			}

			if (TriggerMMFeedbacksEvents)
			{
				MMFeedbacksEvent.Trigger(source, MMFeedbacksEvent.EventTypes.Enable);
			}
		}

        /// <summary>
        /// 如果有需要，就会触发禁用事件。 
        /// </summary>
        /// <param name="source"></param>
        public virtual void TriggerOnDisable(MMF_Player source)
		{
			if (!OnDisableIsNull && TriggerUnityEvents)
			{
				OnDisable.Invoke();
			}

			if (TriggerMMFeedbacksEvents)
			{
				MMFeedbacksEvent.Trigger(source, MMFeedbacksEvent.EventTypes.Disable);
			}
		}
	}
   
}