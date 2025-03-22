using UnityEngine;
#if MM_CINEMACHINE
using Cinemachine;
#elif MM_CINEMACHINE3
using Unity.Cinemachine;
#endif
using MoreMountains.Feedbacks;
using MoreMountains.Tools;

namespace MoreMountains.FeedbacksForThirdParty
{
    /// <summary>
    ///将此添加到 Cinemachine 虚拟相机中，它将使你能够随时间控制其视野范围，并且可以由 MMFeedbackCameraFieldOfView 来进行操控。 
    /// </summary>
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Cinemachine/MM Cinemachine Field Of View Shaker")]
	#if MM_CINEMACHINE
	[RequireComponent(typeof(CinemachineVirtualCamera))]
	#elif MM_CINEMACHINE3
	[RequireComponent(typeof(CinemachineCamera))]
	#endif
	public class MMCinemachineFieldOfViewShaker : MMShaker
	{
		[MMInspectorGroup("Field of view", true, 41)]
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

		#if MM_CINEMACHINE
		protected CinemachineVirtualCamera _targetCamera;
		#elif  MM_CINEMACHINE3
		protected CinemachineCamera _targetCamera;
		#endif
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
			#if MM_CINEMACHINE
			_targetCamera = this.gameObject.GetComponent<CinemachineVirtualCamera>();
			#elif  MM_CINEMACHINE3
			_targetCamera = this.gameObject.GetComponent<CinemachineCamera>();
			#endif
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
			SetFieldOfView(newFieldOfView);
		}

		protected virtual void SetFieldOfView(float newFieldOfView)
		{
			#if MM_CINEMACHINE
			_targetCamera.m_Lens.FieldOfView = newFieldOfView;
			#elif  MM_CINEMACHINE3
			_targetCamera.Lens.FieldOfView = newFieldOfView;
			#endif
		}

        /// <summary>
        /// 收集目标对象上的初始值。 
        /// </summary>
        protected override void GrabInitialValues()
		{
			#if MM_CINEMACHINE
			_initialFieldOfView = _targetCamera.m_Lens.FieldOfView;
			#elif  MM_CINEMACHINE3
			_initialFieldOfView = _targetCamera.Lens.FieldOfView;
			#endif
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
			SetFieldOfView(_initialFieldOfView);
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
        /// 开始监听事件
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
}