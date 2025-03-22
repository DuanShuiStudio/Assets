using UnityEngine;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine.Rendering;
#if MM_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace MoreMountains.FeedbacksForThirdParty
{
    /// <summary>
    /// 将这个类添加到一个带有高动态范围渲染管线（HDRP）泛光后期处理效果的相机上，这样它就能通过接收事件来“抖动”其相关参数值。  
    /// </summary>
#if MM_HDRP
	[RequireComponent(typeof(Volume))]
#endif
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/PostProcessing/MM Bloom Shaker HDRP")]
	public class MMBloomShaker_HDRP : MMShaker
	{
        /// 是否要添加到初始值上 
        public bool RelativeValues = true;

		[MMInspectorGroup("Bloom Intensity", true, 42)]
		/// the curve used to animate the intensity value on
		[Tooltip("用于对强度值进行动画处理的曲线。 ")]
		public AnimationCurve ShakeIntensity = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		public float RemapIntensityZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		public float RemapIntensityOne = 1f;

		[MMInspectorGroup("Bloom Threshold", true, 43)]
		/// the curve used to animate the threshold value on
		[Tooltip("用于对阈值进行动画处理的曲线。 ")]
		public AnimationCurve ShakeThreshold = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		public float RemapThresholdZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		public float RemapThresholdOne = 0f;

#if MM_HDRP
		protected Volume _volume;
		protected Bloom _bloom;
		protected float _initialIntensity;
		protected float _initialThreshold;
		protected float _originalShakeDuration;
		protected bool _originalRelativeIntensity;
		protected AnimationCurve _originalShakeIntensity;
		protected float _originalRemapIntensityZero;
		protected float _originalRemapIntensityOne;
		protected AnimationCurve _originalShakeThreshold;
		protected float _originalRemapThresholdZero;
		protected float _originalRemapThresholdOne;

		/// <summary>
		/// 在初始化时，我们初始化我们的值
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_volume = this.gameObject.GetComponent<Volume>();
			_volume.profile.TryGet(out _bloom);
		}

		/// <summary>
		/// 随着时间推移使数值产生晃动变化。
		/// </summary>
		protected override void Shake()
		{
			float newIntensity = ShakeFloat(ShakeIntensity, RemapIntensityZero, RemapIntensityOne, RelativeValues, _initialIntensity);
			_bloom.intensity.Override(newIntensity);
			float newThreshold = ShakeFloat(ShakeThreshold, RemapThresholdZero, RemapThresholdOne, RelativeValues, _initialThreshold);
			_bloom.threshold.Override(newThreshold);
		}

		/// <summary>
		/// 收集目标对象上的初始值。
		/// </summary>
		protected override void GrabInitialValues()
		{
			_initialIntensity = _bloom.intensity.value;
			_initialThreshold = _bloom.threshold.value;
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
		public virtual void OnBloomShakeEvent(AnimationCurve intensity, float duration, float remapMin, float remapMax,
			AnimationCurve threshold, float remapThresholdMin, float remapThresholdMax, bool relativeIntensity = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, 
			bool resetTargetValuesAfterShake = true, bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, 
			bool stop = false, bool restore = false)
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
				_originalShakeIntensity = ShakeIntensity;
				_originalRemapIntensityZero = RemapIntensityZero;
				_originalRemapIntensityOne = RemapIntensityOne;
				_originalRelativeIntensity = RelativeValues;
				_originalShakeThreshold = ShakeThreshold;
				_originalRemapThresholdZero = RemapThresholdZero;
				_originalRemapThresholdOne = RemapThresholdOne;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				ShakeIntensity = intensity;
				RemapIntensityZero = remapMin * attenuation;
				RemapIntensityOne = remapMax * attenuation;
				RelativeValues = relativeIntensity;
				ShakeThreshold = threshold;
				RemapThresholdZero = remapThresholdMin;
				RemapThresholdOne = remapThresholdMax;
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
			_bloom.intensity.Override(_initialIntensity);
			_bloom.threshold.Override(_initialThreshold);
		}

		/// <summary>
		/// 重置抖动器的数值。
		/// </summary>
		protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalShakeDuration;
			ShakeIntensity = _originalShakeIntensity;
			RemapIntensityZero = _originalRemapIntensityZero;
			RemapIntensityOne = _originalRemapIntensityOne;
			RelativeValues = _originalRelativeIntensity;
			ShakeThreshold = _originalShakeThreshold;
			RemapThresholdZero = _originalRemapThresholdZero;
			RemapThresholdOne = _originalRemapThresholdOne;
		}

		/// <summary>
		/// 开始监听事件。
		/// </summary>
		public override void StartListening()
		{
			base.StartListening();
			MMBloomShakeEvent_HDRP.Register(OnBloomShakeEvent);
		}

		/// <summary>
		/// 停止监听事件
		/// </summary>
		public override void StopListening()
		{
			base.StopListening();
			MMBloomShakeEvent_HDRP.Unregister(OnBloomShakeEvent);
		}
#endif
    }

    /// <summary>
    /// 用于触发渐晕（画面边缘模糊或变暗效果）抖动的一个事件。 
    /// </summary>
    public struct MMBloomShakeEvent_HDRP
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }
		
		public delegate void Delegate(AnimationCurve intensity, float duration, float remapMin, float remapMax,
			AnimationCurve threshold, float remapThresholdMin, float remapThresholdMax, bool relativeIntensity = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, 
			bool resetTargetValuesAfterShake = true, bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, 
			bool stop = false, bool restore = false);

		static public void Trigger(AnimationCurve intensity, float duration, float remapMin, float remapMax,
			AnimationCurve threshold, float remapThresholdMin, float remapThresholdMax, bool relativeIntensity = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true,
			bool resetTargetValuesAfterShake = true, bool forwardDirection = true,
			TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke(intensity, duration, remapMin, remapMax, threshold, remapThresholdMin, remapThresholdMax, relativeIntensity,
				attenuation, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore);
		}
	}
}