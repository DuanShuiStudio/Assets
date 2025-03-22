using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 将此添加到音频回声滤波器中，以便沿着一条曲线重新映射并抖动其数值。 
    /// </summary>
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Audio/MM Audio Filter Echo Shaker")]
	[RequireComponent(typeof(AudioEchoFilter))]
	public class MMAudioFilterEchoShaker : MMShaker
	{
		[MMInspectorGroup("Echo", true, 52)]
		/// whether or not to add to the initial value
		[Tooltip("是否要添加到初始值当中")]
		public bool RelativeEcho = false;
		/// the curve used to animate the intensity value on
		[Tooltip("用于对强度值进行动画处理的曲线")]
		public AnimationCurve ShakeEcho = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[Range(0f, 1f)]
		public float RemapEchoZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[Range(0f, 1f)]
		public float RemapEchoOne = 1f;

        /// 要操控的音频源。 
        protected AudioEchoFilter _targetAudioEchoFilter;
		protected float _initialEcho;
		protected float _originalShakeDuration;
		protected bool _originalRelativeEcho;
		protected AnimationCurve _originalShakeEcho;
		protected float _originalRemapEchoZero;
		protected float _originalRemapEchoOne;

        /// <summary>
        /// 在初始化时，我们初始化我们的值
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			_targetAudioEchoFilter = this.gameObject.GetComponent<AudioEchoFilter>();
		}

        /// <summary>
        /// 当添加那个抖动器时，我们会初始化它的抖动持续时间。 
        /// </summary>
        protected virtual void Reset()
		{
			ShakeDuration = 2f;
		}

        /// <summary>
        /// 随着时间的推移使数值产生抖动变化。 
        /// </summary>
        protected override void Shake()
		{
			float newEchoLevel = ShakeFloat(ShakeEcho, RemapEchoZero, RemapEchoOne, RelativeEcho, _initialEcho);
			_targetAudioEchoFilter.wetMix = newEchoLevel;
		}

        /// <summary>
        /// 收集目标对象上的初始值。
        /// </summary>
        protected override void GrabInitialValues()
		{
			_initialEcho = _targetAudioEchoFilter.wetMix;
		}

        /// <summary>
        /// 当我们收到合适的事件时，我们就会触发一次抖动。 
        /// </summary>
        /// <param name="echoCurve"></param>
        /// <param name="duration"></param>
        /// <param name="amplitude"></param>
        /// <param name="relativeEcho"></param>
        /// <param name="feedbacksIntensity"></param>
        /// <param name="channel"></param>
        public virtual void OnMMAudioFilterEchoShakeEvent(AnimationCurve echoCurve, float duration, float remapMin, float remapMax, bool relativeEcho = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			if (!CheckEventAllowed(channelData) || (!Interruptible && Shaking))
			{
				return;
			}
            
			if (stop)
			{
				Stop();
				return;
			}
			
			if (restore)
			{
				ResetTargetValues();
				return;
			}
            
			_resetShakerValuesAfterShake = resetShakerValuesAfterShake;
			_resetTargetValuesAfterShake = resetTargetValuesAfterShake;

			if (resetShakerValuesAfterShake)
			{
				_originalShakeDuration = ShakeDuration;
				_originalShakeEcho = ShakeEcho;
				_originalRemapEchoZero = RemapEchoZero;
				_originalRemapEchoOne = RemapEchoOne;
				_originalRelativeEcho = RelativeEcho;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				ShakeEcho = echoCurve;
				RemapEchoZero = remapMin * feedbacksIntensity;
				RemapEchoOne = remapMax * feedbacksIntensity;
				RelativeEcho = relativeEcho;
				ForwardDirection = forwardDirection;
			}

			Play();
		}

        /// <summary>
        /// 重置目标的各项数值。 
        /// </summary>
        protected override void ResetTargetValues()
		{
			base.ResetTargetValues();
			_targetAudioEchoFilter.wetMix = _initialEcho;
		}

        /// <summary>
        /// 重置抖动器的数值。
        /// </summary>
        protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalShakeDuration;
			ShakeEcho = _originalShakeEcho;
			RemapEchoZero = _originalRemapEchoZero;
			RemapEchoOne = _originalRemapEchoOne;
			RelativeEcho = _originalRelativeEcho;
		}

        /// <summary>
        /// 开始监听事件。
        /// </summary>
        public override void StartListening()
		{
			base.StartListening();
			MMAudioFilterEchoShakeEvent.Register(OnMMAudioFilterEchoShakeEvent);
		}

        /// <summary>
        /// 停止监听事件
        /// </summary>
        public override void StopListening()
		{
			base.StopListening();
			MMAudioFilterEchoShakeEvent.Unregister(OnMMAudioFilterEchoShakeEvent);
		}
	}

    /// <summary>
    /// 一个用于触发渐晕效果抖动的事件
    /// </summary>
    public struct MMAudioFilterEchoShakeEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(AnimationCurve echoCurve, float duration, float remapMin, float remapMax, bool relativeEcho = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false);

		static public void Trigger(AnimationCurve echoCurve, float duration, float remapMin, float remapMax, bool relativeEcho = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke(echoCurve, duration, remapMin, remapMax, relativeEcho,
				feedbacksIntensity, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore);
		}
	}
}