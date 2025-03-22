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
    /// 将此添加到 Cinemachine 虚拟相机中，它将使你能够随时间控制其正交投影尺寸，并且可以由 MMFeedbackCameraOrthographicSize 来进行操控。 
    /// </summary>
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Cinemachine/MM Cinemachine Orthographic Size Shaker")]
	#if MM_CINEMACHINE
	[RequireComponent(typeof(CinemachineVirtualCamera))]
	#elif MM_CINEMACHINE3
	[RequireComponent(typeof(CinemachineCamera))]
	#endif
	public class MMCinemachineOrthographicSizeShaker : MMShaker
	{
		[MMInspectorGroup("Orthographic Size", true, 43)]
		/// whether or not to add to the initial value
		[Tooltip("是否要添加到初始值当中")]
		public bool RelativeOrthographicSize = false;
		/// the curve used to animate the intensity value on
		[Tooltip("用于对强度值进行动画处理的曲线")]
		public AnimationCurve ShakeOrthographicSize = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		public float RemapOrthographicSizeZero = 5f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个")]
		public float RemapOrthographicSizeOne = 10f;

		#if MM_CINEMACHINE
		protected CinemachineVirtualCamera _targetCamera;
		#elif  MM_CINEMACHINE3	
		protected CinemachineCamera _targetCamera;
		#endif
		protected float _initialOrthographicSize;
		protected float _originalShakeDuration;
		protected bool _originalRelativeOrthographicSize;
		protected AnimationCurve _originalShakeOrthographicSize;
		protected float _originalRemapOrthographicSizeZero;
		protected float _originalRemapOrthographicSizeOne;

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
			float newOrthographicSize = ShakeFloat(ShakeOrthographicSize, RemapOrthographicSizeZero, RemapOrthographicSizeOne, RelativeOrthographicSize, _initialOrthographicSize);
			#if MM_CINEMACHINE
			_targetCamera.m_Lens.OrthographicSize = newOrthographicSize;
			#elif  MM_CINEMACHINE3	
			_targetCamera.Lens.OrthographicSize = newOrthographicSize;
			#endif
		}

        /// <summary>
        /// 收集目标对象上的初始值。
        /// </summary>
        protected override void GrabInitialValues()
		{
			#if MM_CINEMACHINE
			_initialOrthographicSize = _targetCamera.m_Lens.OrthographicSize;
			#elif  MM_CINEMACHINE3	
			_initialOrthographicSize = _targetCamera.Lens.OrthographicSize;
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
        public virtual void OnMMCameraOrthographicSizeShakeEvent(AnimationCurve distortionCurve, float duration, float remapMin, float remapMax, bool relativeDistortion = false,
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
				_originalShakeOrthographicSize = ShakeOrthographicSize;
				_originalRemapOrthographicSizeZero = RemapOrthographicSizeZero;
				_originalRemapOrthographicSizeOne = RemapOrthographicSizeOne;
				_originalRelativeOrthographicSize = RelativeOrthographicSize;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				ShakeOrthographicSize = distortionCurve;
				RemapOrthographicSizeZero = remapMin * feedbacksIntensity;
				RemapOrthographicSizeOne = remapMax * feedbacksIntensity;
				RelativeOrthographicSize = relativeDistortion;
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
			#if MM_CINEMACHINE
			_targetCamera.m_Lens.OrthographicSize = _initialOrthographicSize;
			#elif  MM_CINEMACHINE3	
			_targetCamera.Lens.OrthographicSize = _initialOrthographicSize;
			#endif
		}

        /// <summary>
        /// 重置抖动器的数值。
        /// </summary>
        protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalShakeDuration;
			ShakeOrthographicSize = _originalShakeOrthographicSize;
			RemapOrthographicSizeZero = _originalRemapOrthographicSizeZero;
			RemapOrthographicSizeOne = _originalRemapOrthographicSizeOne;
			RelativeOrthographicSize = _originalRelativeOrthographicSize;
		}

        /// <summary>
        /// 开始监听事件。
        /// </summary>
        public override void StartListening()
		{
			base.StartListening();
			MMCameraOrthographicSizeShakeEvent.Register(OnMMCameraOrthographicSizeShakeEvent);
		}

        /// <summary>
        /// 停止监听事件
        /// </summary>
        public override void StopListening()
		{
			base.StopListening();
			MMCameraOrthographicSizeShakeEvent.Unregister(OnMMCameraOrthographicSizeShakeEvent);
		}
	}
}