using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 将此添加到音频混响滤波器中，以便让其经过重新映射的值沿着一条曲线产生抖动效果。 
    /// </summary>
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Audio/MM Audio Filter Reverb Shaker")]
	[RequireComponent(typeof(AudioReverbFilter))]
	public class MMAudioFilterReverbShaker : MMShaker
	{
		[MMInspectorGroup("Reverb", true, 55)]
		/// whether or not to add to the initial value
		[Tooltip("是否要添加到初始值当中")]
		public bool RelativeReverb = false;
		/// the curve used to animate the intensity value on
		[Tooltip("用于对强度值进行动画处理的曲线")]
		public AnimationCurve ShakeReverb = new AnimationCurve(new Keyframe(0, 0f), new Keyframe(0.5f, 1f), new Keyframe(1, 0f));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[Range(-10000f, 2000f)]
		public float RemapReverbZero = -10000f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[Range(-10000f, 2000f)]
		public float RemapReverbOne = 2000f;

        /// 要操控的音频源。
        protected AudioReverbFilter _targetAudioReverbFilter;
		protected float _initialReverb;
		protected float _originalShakeDuration;
		protected bool _originalRelativeReverb;
		protected AnimationCurve _originalShakeReverb;
		protected float _originalRemapReverbZero;
		protected float _originalRemapReverbOne;

        /// <summary>
        /// 在初始化时，我们初始化我们的值
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			_targetAudioReverbFilter = this.gameObject.GetComponent<AudioReverbFilter>();
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
			float newReverbLevel = ShakeFloat(ShakeReverb, RemapReverbZero, RemapReverbOne, RelativeReverb, _initialReverb);
			_targetAudioReverbFilter.reverbLevel = newReverbLevel;
		}

        /// <summary>
        /// 收集目标对象上的初始值。 
        /// </summary>
        protected override void GrabInitialValues()
		{
			_initialReverb = _targetAudioReverbFilter.reverbLevel;
		}

        /// <summary>
        /// 当我们接收到合适的事件时，我们就会触发一次抖动。
        /// </summary>
        /// <param name="reverbCurve"></param>
        /// <param name="duration"></param>
        /// <param name="amplitude"></param>
        /// <param name="relativeReverb"></param>
        /// <param name="feedbacksIntensity"></param>
        /// <param name="channel"></param>
        public virtual void OnMMAudioFilterReverbShakeEvent(AnimationCurve reverbCurve, float duration, float remapMin, float remapMax, bool relativeReverb = false,
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
				_originalShakeReverb = ShakeReverb;
				_originalRemapReverbZero = RemapReverbZero;
				_originalRemapReverbOne = RemapReverbOne;
				_originalRelativeReverb = RelativeReverb;
			}
			
			if (restore)
			{
				ResetTargetValues();
				return;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				ShakeReverb = reverbCurve;
				RemapReverbZero = remapMin * feedbacksIntensity;
				RemapReverbOne = remapMax * feedbacksIntensity;
				RelativeReverb = relativeReverb;
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
			_targetAudioReverbFilter.reverbLevel = _initialReverb;
		}

        /// <summary>
        /// 重置抖动器的数值。
        /// </summary>
        protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalShakeDuration;
			ShakeReverb = _originalShakeReverb;
			RemapReverbZero = _originalRemapReverbZero;
			RemapReverbOne = _originalRemapReverbOne;
			RelativeReverb = _originalRelativeReverb;
		}

        /// <summary>
        /// 开始监听事件。
        /// </summary>
        public override void StartListening()
		{
			base.StartListening();
			MMAudioFilterReverbShakeEvent.Register(OnMMAudioFilterReverbShakeEvent);
		}

        /// <summary>
        /// 停止监听事件
        /// </summary>
        public override void StopListening()
		{
			base.StopListening();
			MMAudioFilterReverbShakeEvent.Unregister(OnMMAudioFilterReverbShakeEvent);
		}
	}

    /// <summary>
    /// 用于触发渐晕（画面边缘模糊或变暗效果）抖动的一个事件。 
    /// </summary>
    public struct MMAudioFilterReverbShakeEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(AnimationCurve reverbCurve, float duration, float remapMin, float remapMax, bool relativeReverb = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false);

		static public void Trigger(AnimationCurve reverbCurve, float duration, float remapMin, float remapMax, bool relativeReverb = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke(reverbCurve, duration, remapMin, remapMax, relativeReverb,
				feedbacksIntensity, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore);
		}
	}
}