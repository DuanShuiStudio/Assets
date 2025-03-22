using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 将这个添加到摄像机上，它就能让你控制摄像机的近裁剪平面和远裁剪平面。  
    /// </summary>
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Camera/MM Camera Clipping Planes Shaker")]
	[RequireComponent(typeof(Camera))]
	public class MMCameraClippingPlanesShaker : MMShaker
	{
		[MMInspectorGroup("Clipping Planes", true, 31)]
		/// whether or not to add to the initial value
		[Tooltip("是否要添加到初始值当中")]
		public bool RelativeClippingPlanes = false;
        
		[MMInspectorGroup("Near Plane", true, 32)]
		/// the curve used to animate the intensity value on
		[Tooltip("用于对强度值进行动画处理的曲线")]
		public AnimationCurve ShakeNear = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to        
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		public float RemapNearZero = 0.3f;
		/// the value to remap the curve's 1 to        
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		public float RemapNearOne = 100f;

		[MMInspectorGroup("Far Plane", true, 33)]
		/// the curve used to animate the intensity value on
		[Tooltip("用于对强度值进行动画处理的曲线")]
		public AnimationCurve ShakeFar = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to        
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		public float RemapFarZero = 1000f;
		/// the value to remap the curve's 1 to        
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		public float RemapFarOne = 1000f;
        
		protected Camera _targetCamera;
		protected float _initialNear;
		protected float _initialFar;

		protected float _originalShakeDuration;
		protected bool _originalRelativeClippingPlanes;

		protected AnimationCurve _originalShakeNear;
		protected float _originalRemapNearZero;
		protected float _originalRemapNearOne;

		protected AnimationCurve _originalShakeFar;
		protected float _originalRemapFarZero;
		protected float _originalRemapFarOne;

        /// <summary>
        /// 在初始化（init）时，我们对数值进行初始化操作。 
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
			float newNear = ShakeFloat(ShakeNear, RemapNearZero, RemapNearOne, RelativeClippingPlanes, _initialNear);
			_targetCamera.nearClipPlane = newNear;
			float newFar = ShakeFloat(ShakeFar, RemapFarZero, RemapFarOne, RelativeClippingPlanes, _initialFar);
			_targetCamera.farClipPlane = newFar;
		}

        /// <summary>
        /// 收集目标对象上的初始值。
        /// </summary>
        protected override void GrabInitialValues()
		{
			_initialNear = _targetCamera.nearClipPlane;
			_initialFar = _targetCamera.farClipPlane;
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
        public virtual void OnMMCameraClippingPlanesShakeEvent(AnimationCurve animNearCurve, float duration, float remapNearMin, float remapNearMax, AnimationCurve animFarCurve, float remapFarMin, float remapFarMax, bool relativeValues = false,
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
				_originalShakeNear = ShakeNear;
				_originalShakeFar = ShakeFar;
				_originalRemapNearZero = RemapNearZero;
				_originalRemapNearOne = RemapNearOne;
				_originalRemapFarZero = RemapFarZero;
				_originalRemapFarOne = RemapFarOne;
				_originalRelativeClippingPlanes = RelativeClippingPlanes;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				ShakeNear = animNearCurve;
				RemapNearZero = remapNearMin * feedbacksIntensity;
				RemapNearOne = remapNearMax * feedbacksIntensity;
				ShakeFar = animFarCurve;
				RemapFarZero = remapFarMin * feedbacksIntensity;
				RemapFarOne = remapFarMax * feedbacksIntensity;
				RelativeClippingPlanes = relativeValues;
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
			_targetCamera.nearClipPlane = _initialNear;
			_targetCamera.farClipPlane = _initialFar;
		}

        /// <summary>
        /// 重置抖动器的数值。
        /// </summary>
        protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalShakeDuration;
			ShakeNear = _originalShakeNear;
			ShakeFar = _originalShakeFar;
			RemapNearZero = _originalRemapNearZero;
			RemapNearOne = _originalRemapNearOne;
			RemapFarZero = _originalRemapFarZero;
			RemapFarOne = _originalRemapFarOne;
			RelativeClippingPlanes = _originalRelativeClippingPlanes;
		}

        /// <summary>
        /// 开始监听事件
        /// </summary>
        public override void StartListening()
		{
			base.StartListening();
			MMCameraClippingPlanesShakeEvent.Register(OnMMCameraClippingPlanesShakeEvent);
		}

        /// <summary>
        /// 停止监听事件
        /// </summary>
        public override void StopListening()
		{
			base.StopListening();
			MMCameraClippingPlanesShakeEvent.Unregister(OnMMCameraClippingPlanesShakeEvent);
		}
	}

    /// <summary>
    /// 用于触发渐晕（画面边缘模糊或变暗效果）抖动的一个事件。 
    /// </summary>
    public struct MMCameraClippingPlanesShakeEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(AnimationCurve animNearCurve, float duration, float remapNearMin, float remapNearMax, AnimationCurve animFarCurve, float remapFarMin, float remapFarMax, bool relativeValue = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, bool forwardDirection = true, 
			TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false);

		static public void Trigger(AnimationCurve animNearCurve, float duration, float remapNearMin, float remapNearMax, AnimationCurve animFarCurve, float remapFarMin, float remapFarMax, bool relativeValue = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, bool forwardDirection = true, 
			TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke(animNearCurve, duration, remapNearMin, remapNearMax, animFarCurve, remapFarMin, remapFarMax, relativeValue,
				feedbacksIntensity, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore);
		}
	}
}