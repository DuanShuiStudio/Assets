using UnityEngine;
using UnityEngine.Rendering;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
#if MM_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace MoreMountains.FeedbacksForThirdParty
{
    /// <summary>
    /// 将这个类添加到一个带有高动态范围渲染管线（HDRP）曝光后期处理功能的相机上，它就能通过接收事件来“抖动”其相关参数值。 
    /// </summary>
#if MM_HDRP
	[RequireComponent(typeof(Volume))]
#endif
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/PostProcessing/MM Exposure Shaker HDRP")]
	public class MMExposureShaker_HDRP : MMShaker
	{
		[MMInspectorGroup("Exposure Intensity", true, 46)]
		/// whether or not to add to the initial value
		[Tooltip("是否要添加到初始值当中")]
		public bool RelativeIntensity = false;
		/// the curve used to animate the intensity value on
		[Tooltip("用于使强度值产生动画效果的曲线。 ")]
		public AnimationCurve ShakeFixedExposure = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		public float RemapFixedExposureZero = 8.5f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		public float RemapFixedExposureOne = 6f;

#if MM_HDRP
		protected Volume _volume;
		protected Exposure _exposure;
		protected float _initialFixedExposure;
		protected float _originalShakeDuration;
		protected AnimationCurve _originalShakeFixedExposure;
		protected float _originalRemapFixedExposureZero;
		protected float _originalRemapFixedExposureOne;
		protected bool _originalRelativeFixedExposure;

		/// <summary>
		/// 在初始化时，我们初始化我们的值
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_volume = this.gameObject.GetComponent<Volume>();
			_volume.profile.TryGet(out _exposure);
		}

		/// <summary>
		/// 随着时间推移使数值产生晃动变化。
		/// </summary>
		protected override void Shake()
		{
			float newValue = ShakeFloat(ShakeFixedExposure, RemapFixedExposureZero, RemapFixedExposureOne, RelativeIntensity, _initialFixedExposure);
			_exposure.fixedExposure.Override(newValue);
		}

		/// <summary>
		/// 收集目标对象上的初始值。 
		/// </summary>
		protected override void GrabInitialValues()
		{
			_initialFixedExposure = _exposure.fixedExposure.value;
		}

		/// <summary>
		/// 当我们接收到合适的事件时，我们就会触发一次抖动。
		/// </summary>
		/// <param name="intensity"></param>
		/// <param name="duration"></param>
		/// <param name="amplitude"></param>
		/// <param name="relativeIntensity"></param>
		/// <param name="attenuation"></param>
		/// <param name="channel"></param>
		public virtual void OnExposureShakeEvent(AnimationCurve intensity, float duration, float remapMin, float remapMax, bool relativeIntensity = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
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
				_originalShakeFixedExposure = ShakeFixedExposure;
				_originalRemapFixedExposureZero = RemapFixedExposureZero;
				_originalRemapFixedExposureOne = RemapFixedExposureOne;
				_originalRelativeFixedExposure = RelativeIntensity;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				ShakeFixedExposure = intensity;
				RemapFixedExposureZero = remapMin * attenuation;
				RemapFixedExposureOne = remapMax * attenuation;
				RelativeIntensity = relativeIntensity;
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
			_exposure.fixedExposure.Override(_initialFixedExposure);
		}

		/// <summary>
		/// 重置抖动器的数值。
		/// </summary>
		protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalShakeDuration;
			ShakeFixedExposure = _originalShakeFixedExposure;
			RemapFixedExposureZero = _originalRemapFixedExposureZero;
			RemapFixedExposureOne = _originalRemapFixedExposureOne;
			RelativeIntensity = _originalRelativeFixedExposure;
		}

		/// <summary>
		/// 开始监听事件。
		/// </summary>
		public override void StartListening()
		{
			base.StartListening();
			MMExposureShakeEvent_HDRP.Register(OnExposureShakeEvent);
		}

		/// <summary>
		/// 停止监听事件
		/// </summary>
		public override void StopListening()
		{
			base.StopListening();
			MMExposureShakeEvent_HDRP.Unregister(OnExposureShakeEvent);
		}
#endif
    }

    /// <summary>
    /// 用于触发渐晕（画面边缘模糊或变暗效果）抖动的一个事件。 
    /// </summary>
    public struct MMExposureShakeEvent_HDRP
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }
		
		public delegate void Delegate(AnimationCurve fixedExposure, float duration, float remapMin, float remapMax, bool relativeFixedExposure = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false);
		
		static public void Trigger(AnimationCurve fixedExposure, float duration, float remapMin, float remapMax, bool relativeFixedExposure = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke(fixedExposure, duration, remapMin, remapMax, relativeFixedExposure, attenuation, channelData, resetShakerValuesAfterShake, 
				resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore);
		}
	}
}