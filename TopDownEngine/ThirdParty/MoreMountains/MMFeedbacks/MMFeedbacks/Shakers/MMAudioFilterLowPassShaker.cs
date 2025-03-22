using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 将这个添加到音频失真低通滤波器中，使其经过重新映射的值沿着一条曲线产生抖动变化。  
    /// </summary>
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Audio/MM Audio Filter Low Pass Shaker")]
	[RequireComponent(typeof(AudioLowPassFilter))]
	public class MMAudioFilterLowPassShaker : MMShaker
	{
		[MMInspectorGroup("Low Pass", true, 54)]
		/// whether or not to add to the initial value
		[Tooltip("是否要添加到初始值当中")]
		public bool RelativeLowPass = false;
		/// the curve used to animate the intensity value on
		[Tooltip("用于对强度值进行动画处理的曲线")]
		public AnimationCurve ShakeLowPass = new AnimationCurve(new Keyframe(0, 1f), new Keyframe(0.5f, 0f), new Keyframe(1, 1f));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[Range(10f, 22000f)]
		public float RemapLowPassZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[Range(10f, 22000f)]
		public float RemapLowPassOne = 10000f;

        /// 要操控的音频源。
        protected AudioLowPassFilter _targetAudioLowPassFilter;
		protected float _initialLowPass;
		protected float _originalShakeDuration;
		protected bool _originalRelativeLowPass;
		protected AnimationCurve _originalShakeLowPass;
		protected float _originalRemapLowPassZero;
		protected float _originalRemapLowPassOne;

        /// <summary>
        /// 在初始化时，我们初始化我们的值
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			_targetAudioLowPassFilter = this.gameObject.GetComponent<AudioLowPassFilter>();
		}

        /// <summary>
        /// 当添加那个抖动器时，我们会初始化它的抖动持续时间。 
        /// </summary>
        protected virtual void Reset()
		{
			ShakeDuration = 2f;
		}

        /// <summary>
        /// 随着时间推移使数值产生晃动变化。 
        /// </summary>
        protected override void Shake()
		{
			float newLowPassLevel = ShakeFloat(ShakeLowPass, RemapLowPassZero, RemapLowPassOne, RelativeLowPass, _initialLowPass);
			_targetAudioLowPassFilter.cutoffFrequency = newLowPassLevel;
		}

        /// <summary>
        /// 收集目标对象上的初始值。 
        /// </summary>
        protected override void GrabInitialValues()
		{
			_initialLowPass = _targetAudioLowPassFilter.cutoffFrequency;
		}

        /// <summary>
        /// 当我们接收到合适的事件时，我们就会触发一次抖动。 
        /// </summary>
        /// <param name="lowPassCurve"></param>
        /// <param name="duration"></param>
        /// <param name="amplitude"></param>
        /// <param name="relativeLowPass"></param>
        /// <param name="feedbacksIntensity"></param>
        /// <param name="channel"></param>
        public virtual void OnMMAudioFilterLowPassShakeEvent(AnimationCurve lowPassCurve, float duration, float remapMin, float remapMax, bool relativeLowPass = false,
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
				_originalShakeLowPass = ShakeLowPass;
				_originalRemapLowPassZero = RemapLowPassZero;
				_originalRemapLowPassOne = RemapLowPassOne;
				_originalRelativeLowPass = RelativeLowPass;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				ShakeLowPass = lowPassCurve;
				RemapLowPassZero = remapMin * feedbacksIntensity;
				RemapLowPassOne = remapMax * feedbacksIntensity;
				RelativeLowPass = relativeLowPass;
				ForwardDirection = forwardDirection;
			}

			Play();
		}

        /// <summary>
        /// 重置目标的数值。
        /// </summary>
        protected override void ResetTargetValues()
		{
			base.ResetTargetValues();
			_targetAudioLowPassFilter.cutoffFrequency = _initialLowPass;
		}

        /// <summary>
        /// 重置抖动器的数值。
        /// </summary>
        protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalShakeDuration;
			ShakeLowPass = _originalShakeLowPass;
			RemapLowPassZero = _originalRemapLowPassZero;
			RemapLowPassOne = _originalRemapLowPassOne;
			RelativeLowPass = _originalRelativeLowPass;
		}

        /// <summary>
        /// 开始监听事件。 
        /// </summary>
        public override void StartListening()
		{
			base.StartListening();
			MMAudioFilterLowPassShakeEvent.Register(OnMMAudioFilterLowPassShakeEvent);
		}

        /// <summary>
        /// 停止监听事件
        /// </summary>
        public override void StopListening()
		{
			base.StopListening();
			MMAudioFilterLowPassShakeEvent.Unregister(OnMMAudioFilterLowPassShakeEvent);
		}
	}

    /// <summary>
    /// 一个用于触发晕影抖动效果的事件。 
    /// </summary>
    public struct MMAudioFilterLowPassShakeEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(AnimationCurve lowPassCurve, float duration, float remapMin, float remapMax, bool relativeLowPass = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false);

		static public void Trigger(AnimationCurve lowPassCurve, float duration, float remapMin, float remapMax, bool relativeLowPass = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke(lowPassCurve, duration, remapMin, remapMax, relativeLowPass,
				feedbacksIntensity, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore);
		}
	}
}