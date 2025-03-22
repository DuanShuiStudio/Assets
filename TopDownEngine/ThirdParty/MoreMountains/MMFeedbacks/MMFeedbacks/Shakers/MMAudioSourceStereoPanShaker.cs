using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// Add this to an AudioSource to shake its stereo pan values remapped along a curve 
    /// </summary>
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Audio/MM Audio Source Stereo Pan Shaker")]
	[RequireComponent(typeof(AudioSource))]
	public class MMAudioSourceStereoPanShaker : MMShaker
	{
		[MMInspectorGroup("Stereo Pan", true, 57)]
		/// whether or not to add to the initial value
		[Tooltip("是否要添加到初始值当中")]
		public bool RelativeStereoPan = false;
		/// the curve used to animate the intensity value on
		[Tooltip("用于对强度值进行动画处理的曲线")]
		public AnimationCurve ShakeStereoPan = new AnimationCurve(new Keyframe(0, 0f), new Keyframe(0.3f, 1f), new Keyframe(0.6f, -1f), new Keyframe(1, 0f));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[Range(-1f, 1f)]
		public float RemapStereoPanZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[Range(-1f, 1f)]
		public float RemapStereoPanOne = 1f;

        /// 要操控的音频源。
        protected AudioSource _targetAudioSource;
		protected float _initialStereoPan;
		protected float _originalShakeDuration;
		protected bool _originalRelativeValues;
		protected AnimationCurve _originalShakeStereoPan;
		protected float _originalRemapStereoPanZero;
		protected float _originalRemapStereoPanOne;

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
        /// 随着时间推移使数值产生晃动变化
        /// </summary>
        protected override void Shake()
		{
			float newStereoPan = ShakeFloat(ShakeStereoPan, RemapStereoPanZero, RemapStereoPanOne, RelativeStereoPan, _initialStereoPan);
			_targetAudioSource.panStereo = newStereoPan;
		}

        /// <summary>
        /// 收集目标对象上的初始值。
        /// </summary>
        protected override void GrabInitialValues()
		{
			_initialStereoPan = _targetAudioSource.panStereo;
		}

        /// <summary>
        /// 当我们接收到合适的事件时，我们就会触发一次抖动。
        /// </summary>
        /// <param name="stereoPanCurve"></param>
        /// <param name="duration"></param>
        /// <param name="amplitude"></param>
        /// <param name="relativeStereoPan"></param>
        /// <param name="feedbacksIntensity"></param>
        /// <param name="channel"></param>
        public virtual void OnMMAudioSourceStereoPanShakeEvent(AnimationCurve stereoPanCurve, float duration, float remapMin, float remapMax, bool relativeStereoPan = false,
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
				_originalShakeStereoPan = ShakeStereoPan;
				_originalRemapStereoPanZero = RemapStereoPanZero;
				_originalRemapStereoPanOne = RemapStereoPanOne;
				_originalRelativeValues = RelativeStereoPan;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				ShakeStereoPan = stereoPanCurve;
				RemapStereoPanZero = remapMin * feedbacksIntensity;
				RemapStereoPanOne = remapMax * feedbacksIntensity;
				RelativeStereoPan = relativeStereoPan;
				ForwardDirection = forwardDirection;
			}

			Play();
		}

        /// <summary>
        /// 重置目标的数值
        /// </summary>
        protected override void ResetTargetValues()
		{
			base.ResetTargetValues();
			_targetAudioSource.panStereo = _initialStereoPan;
		}

        /// <summary>
        /// 重置抖动器的数值。
        /// </summary>
        protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalShakeDuration;
			ShakeStereoPan = _originalShakeStereoPan;
			RemapStereoPanZero = _originalRemapStereoPanZero;
			RemapStereoPanOne = _originalRemapStereoPanOne;
			RelativeStereoPan = _originalRelativeValues;
		}

        /// <summary>
        /// 开始监听事件
        /// </summary>
        public override void StartListening()
		{
			base.StartListening();
			MMAudioSourceStereoPanShakeEvent.Register(OnMMAudioSourceStereoPanShakeEvent);
		}

        /// <summary>
        /// 停止监听事件
        /// </summary>
        public override void StopListening()
		{
			base.StopListening();
			MMAudioSourceStereoPanShakeEvent.Unregister(OnMMAudioSourceStereoPanShakeEvent);
		}
	}

    /// <summary>
    /// 用于触发渐晕（画面边缘模糊或变暗效果）抖动的一个事件。
    /// </summary>
    public struct MMAudioSourceStereoPanShakeEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(AnimationCurve stereoPanCurve, float duration, float remapMin, float remapMax, bool relativeStereoPan = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false);

		static public void Trigger(AnimationCurve stereoPanCurve, float duration, float remapMin, float remapMax, bool relativeStereoPan = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke(stereoPanCurve, duration, remapMin, remapMax, relativeStereoPan,
				feedbacksIntensity, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore);
		}
	}
}