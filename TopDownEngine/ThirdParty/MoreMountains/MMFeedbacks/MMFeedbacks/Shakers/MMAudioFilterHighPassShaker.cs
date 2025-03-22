using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 将此添加到音频高通滤波器中，以使它的值沿着一条曲线重新映射并产生抖动。
    /// </summary>
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Audio/MM Audio Filter High Pass Shaker")]
	[RequireComponent(typeof(AudioHighPassFilter))]
	public class MMAudioFilterHighPassShaker : MMShaker
	{
		[MMInspectorGroup("High Pass", true, 53)]
		/// whether or not to add to the initial value
		[Tooltip("是否要添加到初始值当中")]
		public bool RelativeHighPass = false;
		/// the curve used to animate the intensity value on
		[Tooltip("用于对强度值进行动画处理的曲线")]
		public AnimationCurve ShakeHighPass = new AnimationCurve(new Keyframe(0, 0f), new Keyframe(0.5f, 1f), new Keyframe(1, 0f));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[Range(10f, 22000f)]
		public float RemapHighPassZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[Range(10f, 22000f)]
		public float RemapHighPassOne = 10000f;

        /// 要控制的音频源。
        protected AudioHighPassFilter _targetAudioHighPassFilter;
		protected float _initialHighPass;
		protected float _originalShakeDuration;
		protected bool _originalRelativeHighPass;
		protected AnimationCurve _originalShakeHighPass;
		protected float _originalRemapHighPassZero;
		protected float _originalRemapHighPassOne;

        /// <summary>
        /// 在初始化时，我们初始化我们的值
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			_targetAudioHighPassFilter = this.gameObject.GetComponent<AudioHighPassFilter>();
		}

        /// <summary>
        /// 当那个抖动器被添加时，我们会初始化它的抖动持续时间。 
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
			float newHighPassLevel = ShakeFloat(ShakeHighPass, RemapHighPassZero, RemapHighPassOne, RelativeHighPass, _initialHighPass);
			_targetAudioHighPassFilter.cutoffFrequency = newHighPassLevel;
		}

        /// <summary>
        /// 收集目标对象上的初始值。 
        /// </summary>
        protected override void GrabInitialValues()
		{
			_initialHighPass = _targetAudioHighPassFilter.cutoffFrequency;
		}

        /// <summary>
        /// 当我们接收到合适的事件时，我们就会触发一次抖动。 
        /// </summary>
        /// <param name="highPassCurve"></param>
        /// <param name="duration"></param>
        /// <param name="amplitude"></param>
        /// <param name="relativeHighPass"></param>
        /// <param name="feedbacksIntensity"></param>
        /// <param name="channel"></param>
        public virtual void OnMMAudioFilterHighPassShakeEvent(AnimationCurve highPassCurve, float duration, float remapMin, float remapMax, bool relativeHighPass = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, bool forwardDirection = true, 
			TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
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
				_originalShakeHighPass = ShakeHighPass;
				_originalRemapHighPassZero = RemapHighPassZero;
				_originalRemapHighPassOne = RemapHighPassOne;
				_originalRelativeHighPass = RelativeHighPass;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				ShakeHighPass = highPassCurve;
				RemapHighPassZero = remapMin * feedbacksIntensity;
				RemapHighPassOne = remapMax * feedbacksIntensity;
				RelativeHighPass = relativeHighPass;
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
			_targetAudioHighPassFilter.cutoffFrequency = _initialHighPass;
		}

        /// <summary>
        /// 重置抖动器的数值。
        /// </summary>
        protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalShakeDuration;
			ShakeHighPass = _originalShakeHighPass;
			RemapHighPassZero = _originalRemapHighPassZero;
			RemapHighPassOne = _originalRemapHighPassOne;
			RelativeHighPass = _originalRelativeHighPass;
		}

        /// <summary>
        /// 开始监听事件。 
        /// </summary>
        public override void StartListening()
		{
			base.StartListening();
			MMAudioFilterHighPassShakeEvent.Register(OnMMAudioFilterHighPassShakeEvent);
		}

        /// <summary>
        /// 停止监听事件。
        /// </summary>
        public override void StopListening()
		{
			base.StopListening();
			MMAudioFilterHighPassShakeEvent.Unregister(OnMMAudioFilterHighPassShakeEvent);
		}
	}

    /// <summary>
    /// 用于触发渐晕抖动效果的事件
    /// </summary>
    public struct MMAudioFilterHighPassShakeEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(AnimationCurve highPassCurve, float duration, float remapMin, float remapMax, bool relativeHighPass = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false);

		static public void Trigger(AnimationCurve highPassCurve, float duration, float remapMin, float remapMax, bool relativeHighPass = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke(highPassCurve, duration, remapMin, remapMax, relativeHighPass,
				feedbacksIntensity, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore);
		}
	}
}