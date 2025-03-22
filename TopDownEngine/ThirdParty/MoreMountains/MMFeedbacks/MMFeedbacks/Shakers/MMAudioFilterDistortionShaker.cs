using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 将此添加到音频失真滤波器中，以便沿着一条曲线重新映射并抖动其参数值。 
    /// </summary>
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Audio/MM Audio Filter Distortion Shaker")]
	[RequireComponent(typeof(AudioDistortionFilter))]
	public class MMAudioFilterDistortionShaker : MMShaker
	{
		[MMInspectorGroup("Distortion", true, 51)]
		/// whether or not to add to the initial value
		[Tooltip("是否要添加到初始值中。 ")]
		public bool RelativeDistortion = false;
		/// the curve used to animate the intensity value on
		[Tooltip("用于对强度值进行动画处理的曲线。 ")]
		public AnimationCurve ShakeDistortion = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值。 ")]
		[Range(0f, 1f)]
		public float RemapDistortionZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值。 ")]
		[Range(0f, 1f)]
		public float RemapDistortionOne = 1f;

		/// the audio source to pilot
		protected AudioDistortionFilter _targetAudioDistortionFilter;
		protected float _initialDistortion;
		protected float _originalShakeDuration;
		protected bool _originalRelativeDistortion;
		protected AnimationCurve _originalShakeDistortion;
		protected float _originalRemapDistortionZero;
		protected float _originalRemapDistortionOne;

        /// <summary>
        /// 在初始化时，我们初始化我们的值。 
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			_targetAudioDistortionFilter = this.gameObject.GetComponent<AudioDistortionFilter>();
		}

        /// <summary>
        /// 当添加那个震动器时，我们会初始化它的震动持续时间。 
        /// </summary>
        protected virtual void Reset()
		{
			ShakeDuration = 2f;
		}

        /// <summary>
        /// 随着时间推移抖动数值。 
        /// </summary>
        protected override void Shake()
		{
			float newDistortionLevel = ShakeFloat(ShakeDistortion, RemapDistortionZero, RemapDistortionOne, RelativeDistortion, _initialDistortion);
			_targetAudioDistortionFilter.distortionLevel = newDistortionLevel;
		}

        /// <summary>
        /// 收集目标对象上的初始值。
        /// </summary>
        protected override void GrabInitialValues()
		{
			_initialDistortion = _targetAudioDistortionFilter.distortionLevel;
		}

        /// <summary>
        ///当我们接收到合适的事件时，我们就触发一次抖动。 
        /// </summary>
        /// <param name="distortionCurve"></param>
        /// <param name="duration"></param>
        /// <param name="amplitude"></param>
        /// <param name="relativeDistortion"></param>
        /// <param name="feedbacksIntensity"></param>
        /// <param name="channel"></param>
        public virtual void OnMMAudioFilterDistortionShakeEvent(AnimationCurve distortionCurve, float duration, float remapMin, float remapMax, bool relativeDistortion = false,
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
				_originalShakeDistortion = ShakeDistortion;
				_originalRemapDistortionZero = RemapDistortionZero;
				_originalRemapDistortionOne = RemapDistortionOne;
				_originalRelativeDistortion = RelativeDistortion;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				ShakeDistortion = distortionCurve;
				RemapDistortionZero = remapMin * feedbacksIntensity;
				RemapDistortionOne = remapMax * feedbacksIntensity;
				RelativeDistortion = relativeDistortion;
				ForwardDirection = forwardDirection;
			}

			Play();
		}

        /// <summary>
        /// 重置目标对象的值。
        /// </summary>
        protected override void ResetTargetValues()
		{
			base.ResetTargetValues();
			_targetAudioDistortionFilter.distortionLevel = _initialDistortion;
		}

        /// <summary>
        /// 重置震动器的值。
        /// </summary>
        protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalShakeDuration;
			ShakeDistortion = _originalShakeDistortion;
			RemapDistortionZero = _originalRemapDistortionZero;
			RemapDistortionOne = _originalRemapDistortionOne;
			RelativeDistortion = _originalRelativeDistortion;
		}

        /// <summary>
        /// 开始监听事件。
        /// </summary>
        public override void StartListening()
		{
			base.StartListening();
			MMAudioFilterDistortionShakeEvent.Register(OnMMAudioFilterDistortionShakeEvent);
		}

        /// <summary>
        /// 停止监听事件。
        /// </summary>
        public override void StopListening()
		{
			base.StopListening();
			MMAudioFilterDistortionShakeEvent.Unregister(OnMMAudioFilterDistortionShakeEvent);
		}
	}

    /// <summary>
    /// 一个用于触发渐晕抖动效果的事件。 
    /// </summary>
    public struct MMAudioFilterDistortionShakeEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(AnimationCurve distortionCurve, float duration, float remapMin, float remapMax, bool relativeDistortion = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false);

		static public void Trigger(AnimationCurve distortionCurve, float duration, float remapMin, float remapMax, bool relativeDistortion = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke(distortionCurve, duration, remapMin, remapMax, relativeDistortion,
				feedbacksIntensity, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore);
		}
	}
}