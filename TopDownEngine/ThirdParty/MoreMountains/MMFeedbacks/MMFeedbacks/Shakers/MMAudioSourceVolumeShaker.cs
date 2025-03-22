using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 将这个添加到音频源中，使其音量沿着一条曲线重新映射并产生抖动效果。 
    /// </summary>
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Audio/MM Audio Source Volume Shaker")]
	[RequireComponent(typeof(AudioSource))]
	public class MMAudioSourceVolumeShaker : MMShaker
	{
		[MMInspectorGroup("Volume", true, 59)]
		/// whether or not to add to the initial value
		[Tooltip("是否要添加到初始值当中")]
		public bool RelativeVolume = false;
		/// the curve used to animate the intensity value on
		[Tooltip("用于对强度值进行动画处理的曲线")]
		public AnimationCurve ShakeVolume = new AnimationCurve(new Keyframe(0, 1f), new Keyframe(0.5f, 0f), new Keyframe(1, 1f));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[Range(-1f, 1f)]
		public float RemapVolumeZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[Range(-1f, 1f)]
		public float RemapVolumeOne = 1f;

		/// the audio source to pilot
		protected AudioSource _targetAudioSource;
		protected float _initialVolume;
		protected float _originalShakeDuration;
		protected bool _originalRelativeValues;
		protected AnimationCurve _originalShakeVolume;
		protected float _originalRemapVolumeZero;
		protected float _originalRemapVolumeOne;

        /// <summary>
        /// 要操控的音频源。
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			_targetAudioSource = this.gameObject.GetComponent<AudioSource>();
		}

        /// <summary>
        /// 在初始化时，我们初始化我们的值
        /// </summary>
        protected virtual void Reset()
		{
			ShakeDuration = 2f;
		}

        /// <summary>
        /// 当添加那个抖动器时，我们会初始化它的抖动持续时间。 
        /// </summary>
        protected override void Shake()
		{
			float newVolume = ShakeFloat(ShakeVolume, RemapVolumeZero, RemapVolumeOne, RelativeVolume, _initialVolume);
			_targetAudioSource.volume = newVolume;
		}

        /// <summary>
        /// 随着时间推移使数值产生晃动变化。
        /// </summary>
        protected override void GrabInitialValues()
		{
			_initialVolume = _targetAudioSource.volume;
		}

        /// <summary>
        /// 当我们接收到合适的事件时，我们就会触发一次抖动。
        /// </summary>
        /// <param name="volumeCurve"></param>
        /// <param name="duration"></param>
        /// <param name="amplitude"></param>
        /// <param name="relativeVolume"></param>
        /// <param name="feedbacksIntensity"></param>
        /// <param name="channel"></param>
        public virtual void OnMMAudioSourceVolumeShakeEvent(AnimationCurve volumeCurve, float duration, float remapMin, float remapMax, bool relativeVolume = false,
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
				_originalShakeVolume = ShakeVolume;
				_originalRemapVolumeZero = RemapVolumeZero;
				_originalRemapVolumeOne = RemapVolumeOne;
				_originalRelativeValues = RelativeVolume;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				ShakeVolume = volumeCurve;
				RemapVolumeZero = remapMin * feedbacksIntensity;
				RemapVolumeOne = remapMax * feedbacksIntensity;
				RelativeVolume = relativeVolume;
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
			_targetAudioSource.volume = _initialVolume;
		}

        /// <summary>
        /// 重置抖动器的数值
        /// </summary>
        protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalShakeDuration;
			ShakeVolume = _originalShakeVolume;
			RemapVolumeZero = _originalRemapVolumeZero;
			RemapVolumeOne = _originalRemapVolumeOne;
			RelativeVolume = _originalRelativeValues;
		}

        /// <summary>
        /// 开始监听事件。
        /// </summary>
        public override void StartListening()
		{
			base.StartListening();
			MMAudioSourceVolumeShakeEvent.Register(OnMMAudioSourceVolumeShakeEvent);
		}

        /// <summary>
        /// 停止监听事件
        /// </summary>
        public override void StopListening()
		{
			base.StopListening();
			MMAudioSourceVolumeShakeEvent.Unregister(OnMMAudioSourceVolumeShakeEvent);
		}
	}

    /// <summary>
    /// 用于触发渐晕（画面边缘模糊或变暗效果）抖动的一个事件。 
    /// </summary>
    public struct MMAudioSourceVolumeShakeEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(AnimationCurve volumeCurve, float duration, float remapMin, float remapMax, bool relativeVolume = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false);

		static public void Trigger(AnimationCurve volumeCurve, float duration, float remapMin, float remapMax, bool relativeVolume = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke(volumeCurve, duration, remapMin, remapMax, relativeVolume,
				feedbacksIntensity, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore);
		}
	}
}