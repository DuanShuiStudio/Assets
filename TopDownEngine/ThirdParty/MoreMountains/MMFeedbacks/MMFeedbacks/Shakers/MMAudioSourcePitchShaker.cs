using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 将这个添加到音频源中，使其音高沿着一条曲线重新映射并产生抖动效果。 
    /// </summary>
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Audio/MM Audio Source Pitch Shaker")]
	[RequireComponent(typeof(AudioSource))]
	public class MMAudioSourcePitchShaker : MMShaker
	{
		[MMInspectorGroup("Pitch", true, 57)]
		/// whether or not to add to the initial value
		[Tooltip("是否要添加到初始值当中")]
		public bool RelativePitch = false;
		/// the curve used to animate the intensity value on
		[Tooltip("用于对强度值进行动画处理的曲线")]
		public AnimationCurve ShakePitch = new AnimationCurve(new Keyframe(0, 1f), new Keyframe(0.5f, 0f), new Keyframe(1, 1f));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[Range(-3f, 3f)]
		public float RemapPitchZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[Range(-3f, 3f)]
		public float RemapPitchOne = 1f;

        /// 要操控的音频源。
        protected AudioSource _targetAudioSource;
		protected float _initialPitch;
		protected float _originalShakeDuration;
		protected bool _originalRelativePitch;
		protected AnimationCurve _originalShakePitch;
		protected float _originalRemapPitchZero;
		protected float _originalRemapPitchOne;

        /// <summary>
        /// 在初始化时，我们初始化我们的值
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			_targetAudioSource = this.gameObject.GetComponent<AudioSource>();
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
			float newPitch = ShakeFloat(ShakePitch, RemapPitchZero, RemapPitchOne, RelativePitch, _initialPitch);
			_targetAudioSource.pitch = newPitch;
		}

        /// <summary>
        /// 收集目标对象上的初始值。
        /// </summary>
        protected override void GrabInitialValues()
		{
			_initialPitch = _targetAudioSource.pitch;
		}

        /// <summary>
        /// 当我们接收到合适的事件时，我们就会触发一次抖动。
        /// </summary>
        /// <param name="pitchCurve"></param>
        /// <param name="duration"></param>
        /// <param name="amplitude"></param>
        /// <param name="relativePitch"></param>
        /// <param name="feedbacksIntensity"></param>
        /// <param name="channel"></param>
        public virtual void OnMMAudioSourcePitchShakeEvent(AnimationCurve pitchCurve, float duration, float remapMin, float remapMax, bool relativePitch = false,
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
				_originalShakePitch = ShakePitch;
				_originalRemapPitchZero = RemapPitchZero;
				_originalRemapPitchOne = RemapPitchOne;
				_originalRelativePitch = RelativePitch;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				ShakePitch = pitchCurve;
				RemapPitchZero = remapMin * feedbacksIntensity;
				RemapPitchOne = remapMax * feedbacksIntensity;
				RelativePitch = relativePitch;
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
			_targetAudioSource.pitch = _initialPitch;
		}

        /// <summary>
        /// 重置抖动器的数值
        /// </summary>
        protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalShakeDuration;
			ShakePitch = _originalShakePitch;
			RemapPitchZero = _originalRemapPitchZero;
			RemapPitchOne = _originalRemapPitchOne;
			RelativePitch = _originalRelativePitch;
		}

        /// <summary>
        /// 开始监听事件。
        /// </summary>
        public override void StartListening()
		{
			base.StartListening();
			MMAudioSourcePitchShakeEvent.Register(OnMMAudioSourcePitchShakeEvent);
		}

        /// <summary>
        /// 停止监听事件
        /// </summary>
        public override void StopListening()
		{
			base.StopListening();
			MMAudioSourcePitchShakeEvent.Unregister(OnMMAudioSourcePitchShakeEvent);
		}
	}

    /// <summary>
    /// 用于触发渐晕（画面边缘模糊或变暗效果）抖动的一个事件。 
    /// </summary>
    public struct MMAudioSourcePitchShakeEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(AnimationCurve pitchCurve, float duration, float remapMin, float remapMax, bool relativePitch = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false);

		static public void Trigger(AnimationCurve pitchCurve, float duration, float remapMin, float remapMax, bool relativePitch = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke(pitchCurve, duration, remapMin, remapMax, relativePitch,
				feedbacksIntensity, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore);
		}
	}
}