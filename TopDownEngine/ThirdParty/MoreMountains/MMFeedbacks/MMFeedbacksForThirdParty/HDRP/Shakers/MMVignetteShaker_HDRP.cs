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
    /// 将这个类添加到具有高动态范围渲染管线（HDRP）暗角后期处理功能的相机上，它将能够通过接收事件来“抖动”其参数值。 
    /// </summary>
#if MM_HDRP
	[RequireComponent(typeof(Volume))]
#endif
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/PostProcessing/MM Vignette Shaker HDRP")]
	public class MMVignetteShaker_HDRP : MMShaker
	{
		[Header("Intensity强度")]
		[MMInspectorGroup("Vignette Intensity", true, 46)]
		/// whether or not to add to the initial value
		[Tooltip("是否要添加到初始值当中")]
		public bool RelativeIntensity = false;
		/// the curve used to animate the intensity value on
		[Tooltip("用于使强度值产生动画效果的曲线。 ")]
		public AnimationCurve ShakeIntensity = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[Range(0f, 1f)]
		public float RemapIntensityZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[Range(0f, 1f)]
		public float RemapIntensityOne = 1f;

		[MMFInspectorGroup("Vignette Color", true, 60)]
		/// whether or not to also animate  the vignette's color
		[Tooltip("是否也对渐晕（暗角）的颜色进行动画处理 ")]
		public bool InterpolateColor = false;
		/// the curve to animate the color on
		[Tooltip("用于使颜色产生动画效果的曲线。 ")]
		public AnimationCurve ColorCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.05f, 1f), new Keyframe(0.95f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[Range(0, 1)]
		public float RemapColorZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[Range(0f, 1f)]
		public float RemapColorOne = 1f;
		/// the color to lerp towards
		[Tooltip("要向其进行线性插值的颜色")]
		public Color TargetColor = Color.red;

#if MM_HDRP
		protected Volume _volume;
		protected Vignette _vignette;
		protected float _initialIntensity;
		protected float _originalShakeDuration;
		protected AnimationCurve _originalShakeIntensity;
		protected float _originalRemapIntensityZero;
		protected float _originalRemapIntensityOne;
		protected bool _originalRelativeIntensity;
		protected bool _originalInterpolateColor;
		protected AnimationCurve _originalColorCurve;
		protected float _originalRemapColorZero;
		protected float _originalRemapColorOne;
		protected Color _originalTargetColor;
		protected Color _initialColor;

		/// <summary>
		/// 在初始化时，我们初始化我们的值
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_volume = this.gameObject.GetComponent<Volume>();
			_volume.profile.TryGet(out _vignette);
		}

		/// <summary>
		/// 随着时间推移使数值产生晃动变化
		/// </summary>
		protected override void Shake()
		{
			float newValue = ShakeFloat(ShakeIntensity, RemapIntensityZero, RemapIntensityOne, RelativeIntensity, _initialIntensity);
			_vignette.intensity.Override(newValue);

			if (InterpolateColor)
			{
				float newColorValue = ShakeFloat(ColorCurve, RemapColorZero, RemapColorOne, RelativeIntensity, 0);
				_vignette.color.Override(Color.Lerp(_initialColor, TargetColor, newColorValue));
			}
		}

		/// <summary>
		/// 收集目标对象上的初始值。 
		/// </summary>
		protected override void GrabInitialValues()
		{
			_initialIntensity = _vignette.intensity.value;
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
		public virtual void OnVignetteShakeEvent(AnimationCurve intensity, float duration, float remapMin, float remapMax, bool relativeIntensity = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false,
			bool interpolateColor = false, AnimationCurve colorCurve = null, float remapColorZero = 0f, float remapColorOne = 1f, Color targetColor = default(Color))
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
				_originalRelativeIntensity = RelativeIntensity;
				_originalInterpolateColor = InterpolateColor;
				_originalColorCurve = ColorCurve;
				_originalRemapColorZero = RemapColorZero;
				_originalRemapColorOne = RemapColorOne;
				_originalTargetColor = TargetColor;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				ShakeIntensity = intensity;
				RemapIntensityZero = remapMin * attenuation;
				RemapIntensityOne = remapMax * attenuation;
				RelativeIntensity = relativeIntensity;
				ForwardDirection = forwardDirection;
				InterpolateColor = interpolateColor;
				ColorCurve = colorCurve;
				RemapColorZero = remapColorZero;
				RemapColorOne = remapColorOne;
				TargetColor = targetColor;
			}

			Play();
		}

		/// <summary>
		/// 重置目标的数值。
		/// </summary>
		protected override void ResetTargetValues()
		{
			base.ResetTargetValues();
			_vignette.intensity.Override(_initialIntensity);
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
			RelativeIntensity = _originalRelativeIntensity;
			InterpolateColor = _originalInterpolateColor;
			ColorCurve = _originalColorCurve;
			RemapColorZero = _originalRemapColorZero;
			RemapColorOne = _originalRemapColorOne;
			TargetColor = _originalTargetColor;
		}

		/// <summary>
		/// 开始监听事件。
		/// </summary>
		public override void StartListening()
		{
			base.StartListening();
			MMVignetteShakeEvent_HDRP.Register(OnVignetteShakeEvent);
		}

		/// <summary>
		/// 停止监听事件
		/// </summary>
		public override void StopListening()
		{
			base.StopListening();
			MMVignetteShakeEvent_HDRP.Unregister(OnVignetteShakeEvent);
		}
#endif
    }

    /// <summary>
    /// 用于触发渐晕（画面边缘模糊或变暗效果）抖动的一个事件。 
    /// </summary>
    public struct MMVignetteShakeEvent_HDRP
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }
		
		public delegate void Delegate(AnimationCurve intensity, float duration, float remapMin, float remapMax, bool relativeIntensity = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false,
			bool interpolateColor = false, AnimationCurve colorCurve = null, float remapColorZero = 0f, float remapColorOne = 1f, Color targetColor = default(Color));
		
		static public void Trigger(AnimationCurve intensity, float duration, float remapMin, float remapMax, bool relativeIntensity = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false,
			bool interpolateColor = false, AnimationCurve colorCurve = null, float remapColorZero = 0f, float remapColorOne = 1f, Color targetColor = default(Color))
		{
			OnEvent?.Invoke(intensity, duration, remapMin, remapMax, relativeIntensity, attenuation, channelData, resetShakerValuesAfterShake, 
				resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore, interpolateColor, colorCurve, remapColorZero, remapColorOne, targetColor);
		}
	}
}