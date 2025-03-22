using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 将此（组件或脚本等）添加到摄像机上，它就能让你随时间控制摄像机的视野范围，并且可以通过 `MMFeedbackCameraFieldOfView` 来进行操控。 
    /// </summary>
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Camera/MM Camera Field Of View Shaker")]
	[RequireComponent(typeof(Camera))]
	public class MMCameraFieldOfViewShaker : MMShaker
	{
		[MMInspectorGroup("Field of View", true, 34)]
		/// whether or not to add to the initial value
		[Tooltip("是否要添加到初始值当中")]
		public bool RelativeFieldOfView = false;
		/// the curve used to animate the intensity value on
		[Tooltip("用于对强度值进行动画处理的曲线")]
		public AnimationCurve ShakeFieldOfView = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[Range(0f, 179f)]
		public float RemapFieldOfViewZero = 60f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[Range(0f, 179f)]
		public float RemapFieldOfViewOne = 120f;

		protected Camera _targetCamera;
		protected float _initialFieldOfView;
		protected float _originalShakeDuration;
		protected bool _originalRelativeFieldOfView;
		protected AnimationCurve _originalShakeFieldOfView;
		protected float _originalRemapFieldOfViewZero;
		protected float _originalRemapFieldOfViewOne;

        /// <summary>
        /// 在初始化时，我们初始化我们的值 
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			_targetCamera = this.gameObject.GetComponent<Camera>();
		}

        /// <summary>
        /// 当添加那个抖动器时，我们会初始化它的抖动持续时间。 
        /// </summary>
        protected virtual void Reset()
		{
			ShakeDuration = 0.5f;
		}

        /// <summary>
        /// 随着时间推移使数值产生晃动变化。
        /// </summary>
        protected override void Shake()
		{
			float newFieldOfView = ShakeFloat(ShakeFieldOfView, RemapFieldOfViewZero, RemapFieldOfViewOne, RelativeFieldOfView, _initialFieldOfView);
			_targetCamera.fieldOfView = newFieldOfView;
		}

        /// <summary>
        /// 收集目标对象上的初始值。 
        /// </summary>
        protected override void GrabInitialValues()
		{
			_initialFieldOfView = _targetCamera.fieldOfView;
		}

        /// <summary>
        /// 当我们接收到合适的事件时，我们就会触发一次抖动。
        /// </summary>
        /// <param name="distortionCurve"></param>
        /// <param name="duration"></param>
        /// <param name="amplitude"></param>
        /// <param name="relativeDistortion"></param>
        /// <param name="feedbacksIntensity"></param>
        /// <param name="channel"></param>
        public virtual void OnMMCameraFieldOfViewShakeEvent(AnimationCurve distortionCurve, float duration, float remapMin, float remapMax, bool relativeDistortion = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, bool forwardDirection = true, 
			TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			if (!CheckEventAllowed(channelData))
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
            
			if (!Interruptible && Shaking)
			{
				return;
			}
            
			_resetShakerValuesAfterShake = resetShakerValuesAfterShake;
			_resetTargetValuesAfterShake = resetTargetValuesAfterShake;

			if (resetShakerValuesAfterShake)
			{
				_originalShakeDuration = ShakeDuration;
				_originalShakeFieldOfView = ShakeFieldOfView;
				_originalRemapFieldOfViewZero = RemapFieldOfViewZero;
				_originalRemapFieldOfViewOne = RemapFieldOfViewOne;
				_originalRelativeFieldOfView = RelativeFieldOfView;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				ShakeFieldOfView = distortionCurve;
				RemapFieldOfViewZero = remapMin * feedbacksIntensity;
				RemapFieldOfViewOne = remapMax * feedbacksIntensity;
				RelativeFieldOfView = relativeDistortion;
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
			_targetCamera.fieldOfView = _initialFieldOfView;
		}

        /// <summary>
        /// 重置抖动器的数值。
        /// </summary>
        protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalShakeDuration;
			ShakeFieldOfView = _originalShakeFieldOfView;
			RemapFieldOfViewZero = _originalRemapFieldOfViewZero;
			RemapFieldOfViewOne = _originalRemapFieldOfViewOne;
			RelativeFieldOfView = _originalRelativeFieldOfView;
		}

        /// <summary>
        /// 开始监听事件。
        /// </summary>
        public override void StartListening()
		{
			base.StartListening();
			MMCameraFieldOfViewShakeEvent.Register(OnMMCameraFieldOfViewShakeEvent);
		}

        /// <summary>
        /// 停止监听事件
        /// </summary>
        public override void StopListening()
		{
			base.StopListening();
			MMCameraFieldOfViewShakeEvent.Unregister(OnMMCameraFieldOfViewShakeEvent);
		}
	}

    /// <summary>
    /// 用于触发渐晕（画面边缘模糊或变暗效果）抖动的一个事件。 
    /// </summary>
    public struct MMCameraFieldOfViewShakeEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(AnimationCurve animCurve, float duration, float remapMin, float remapMax, bool relativeValue = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, bool forwardDirection = true, 
			TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false);

		static public void Trigger(AnimationCurve animCurve, float duration, float remapMin, float remapMax, bool relativeValue = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, bool forwardDirection = true, 
			TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke(animCurve, duration, remapMin, remapMax, relativeValue,
				feedbacksIntensity, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore);
		}
	}
}