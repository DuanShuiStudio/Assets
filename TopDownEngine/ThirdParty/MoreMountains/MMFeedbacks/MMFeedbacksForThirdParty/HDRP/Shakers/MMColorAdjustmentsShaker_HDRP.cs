using UnityEngine;
using UnityEngine.Rendering;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;
#if MM_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace MoreMountains.FeedbacksForThirdParty
{
    /// <summary>
    /// 将这个类添加到一个带有高动态范围渲染管线（HDRP）色彩调整后期处理功能的摄像机上，它就能够通过接收事件来“抖动”其参数值。 
    /// </summary>
#if MM_HDRP
	[RequireComponent(typeof(Volume))]
#endif
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/PostProcessing/MM Color Adjustments Shaker HDRP")]
	public class MMColorAdjustmentsShaker_HDRP : MMShaker
	{
        /// 是否要添加到初始值当中
        public bool RelativeValues = true;

		[MMInspectorGroup("Post Exposure", true, 44)]
		/// the curve used to animate the focus distance value on
		[Tooltip("用于对焦距值进行动画处理的曲线。 ")]
		public AnimationCurve ShakePostExposure = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		public float RemapPostExposureZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		public float RemapPostExposureOne = 1f;

		[MMInspectorGroup("Hue Shift", true, 45)]
		/// the curve used to animate the aperture value on
		[Tooltip("用于对光圈值进行动画处理的曲线。")]
		public AnimationCurve ShakeHueShift = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Range(-180f, 180f)]
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		public float RemapHueShiftZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[Range(-180f, 180f)]
		public float RemapHueShiftOne = 180f;

		[MMInspectorGroup("Saturation", true, 46)]
		/// the curve used to animate the focal length value on
		[Tooltip("用于对焦距值进行动画处理的曲线。 ")]
		public AnimationCurve ShakeSaturation = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[Range(-100f, 100f)]
		public float RemapSaturationZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[Range(-100f, 100f)]
		public float RemapSaturationOne = 100f;

		[MMInspectorGroup("Contrast", true, 47)]
		/// the curve used to animate the focal length value on
		[Tooltip("用于使焦距值产生动画效果的曲线。 ")]
		public AnimationCurve ShakeContrast = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[Range(-100f, 100f)]
		public float RemapContrastZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[Range(-100f, 100f)]
		public float RemapContrastOne = 100f;
        
		public enum ColorFilterModes { None, Gradient, Interpolate }
 
		[MMInspectorGroup("Color Filter", true, 48)]
		/// the color filter mode to work with (none, over a gradient, or interpolate to a destination color
		[Tooltip("要使用的颜色滤镜模式（无、基于渐变，或者插值到目标颜色） ")]
		public ColorFilterModes ColorFilterMode = ColorFilterModes.None;
		/// the gradient over which to modify the color filter
		[Tooltip("用于修改颜色滤镜的渐变效果。 ")]
		[MMFEnumCondition("ColorFilterMode", (int)ColorFilterModes.Gradient)]
		[GradientUsage(true)]
		public Gradient ColorFilterGradient;
		/// the destination color to match when in Interpolate mode
		[Tooltip("在“插值模式”下要匹配的目标颜色。 ")]
		[MMFEnumCondition("ColorFilterMode", (int) ColorFilterModes.Interpolate)]
		public Color ColorFilterDestination = Color.yellow;
		/// the curve over which to interpolate the color filter
		[Tooltip("用于对颜色滤镜进行插值处理的曲线。 ")]
		[MMFEnumCondition("ColorFilterMode", (int) ColorFilterModes.Interpolate)]
		public AnimationCurve ColorFilterCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));

#if MM_HDRP
		protected Volume _volume;
		protected ColorAdjustments _colorAdjustments;
		protected float _initialPostExposure;
		protected float _initialHueShift;
		protected float _initialSaturation;
		protected float _initialContrast;
		protected Color _initialColorFilterColor;
		protected float _originalShakeDuration;
		protected bool _originalRelativeValues;
		protected AnimationCurve _originalShakePostExposure;
		protected float _originalRemapPostExposureZero;
		protected float _originalRemapPostExposureOne;
		protected AnimationCurve _originalShakeHueShift;
		protected float _originalRemapHueShiftZero;
		protected float _originalRemapHueShiftOne;
		protected AnimationCurve _originalShakeSaturation;
		protected float _originalRemapSaturationZero;
		protected float _originalRemapSaturationOne;
		protected AnimationCurve _originalShakeContrast;
		protected float _originalRemapContrastZero;
		protected float _originalRemapContrastOne;
		protected ColorFilterModes _originalColorFilterMode;
		protected Gradient _originalColorFilterGradient;
		protected Color _originalColorFilterDestination;
		protected AnimationCurve _originalColorFilterCurve;  

		/// <summary>
		/// 在初始化时，我们初始化我们的值
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_volume = this.gameObject.GetComponent<Volume>();
			_volume.profile.TryGet(out _colorAdjustments);
		}

		/// <summary>
		/// 当添加那个抖动器时，我们会初始化它的抖动持续时间。 
		/// </summary>
		protected virtual void Reset()
		{
			ShakeDuration = 0.8f;
		}

		/// <summary>
		/// 随着时间推移使数值产生晃动变化。
		/// </summary>
		protected override void Shake()
		{
			float newPostExposure = ShakeFloat(ShakePostExposure, RemapPostExposureZero, RemapPostExposureOne, RelativeValues, _initialPostExposure);
			_colorAdjustments.postExposure.Override(newPostExposure);
			float newHueShift = ShakeFloat(ShakeHueShift, RemapHueShiftZero, RemapHueShiftOne, RelativeValues, _initialHueShift);
			_colorAdjustments.hueShift.Override(newHueShift);
			float newSaturation = ShakeFloat(ShakeSaturation, RemapSaturationZero, RemapSaturationOne, RelativeValues, _initialSaturation);
			_colorAdjustments.saturation.Override(newSaturation);
			float newContrast = ShakeFloat(ShakeContrast, RemapContrastZero, RemapContrastOne, RelativeValues, _initialContrast);
			_colorAdjustments.contrast.Override(newContrast);

			_remappedTimeSinceStart = MMFeedbacksHelpers.Remap(Time.time - _shakeStartedTimestamp, 0f, ShakeDuration, 0f, 1f);
	            
			if (ColorFilterMode == ColorFilterModes.Gradient)
			{
				_colorAdjustments.colorFilter.Override(ColorFilterGradient.Evaluate(_remappedTimeSinceStart));    
			}
			else if (ColorFilterMode == ColorFilterModes.Interpolate)
			{
				float factor = ColorFilterCurve.Evaluate(_remappedTimeSinceStart);
				_colorAdjustments.colorFilter.Override(Color.LerpUnclamped(_initialColorFilterColor, ColorFilterDestination, factor));
			}
		}

		/// <summary>
		/// 收集目标对象上的初始值。
		/// </summary>
		protected override void GrabInitialValues()
		{
			_initialPostExposure = _colorAdjustments.postExposure.value;
			_initialHueShift = _colorAdjustments.hueShift.value;
			_initialSaturation = _colorAdjustments.saturation.value;
			_initialContrast = _colorAdjustments.contrast.value;
			_initialColorFilterColor = _colorAdjustments.colorFilter.value;
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
		public virtual void OnMMColorGradingShakeEvent(AnimationCurve shakePostExposure, float remapPostExposureZero, float remapPostExposureOne,
			AnimationCurve shakeHueShift, float remapHueShiftZero, float remapHueShiftOne,
			AnimationCurve shakeSaturation, float remapSaturationZero, float remapSaturationOne,
			AnimationCurve shakeContrast, float remapContrastZero, float remapContrastOne,
			ColorFilterModes colorFilterMode, Gradient colorFilterGradient, Color colorFilterDestination,AnimationCurve colorFilterCurve,  
			float duration, bool relativeValues = false,
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
				_originalRelativeValues = RelativeValues;
				_originalShakePostExposure = ShakePostExposure;
				_originalRemapPostExposureZero = RemapPostExposureZero;
				_originalRemapPostExposureOne = RemapPostExposureOne;
				_originalShakeHueShift = ShakeHueShift;
				_originalRemapHueShiftZero = RemapHueShiftZero;
				_originalRemapHueShiftOne = RemapHueShiftOne;
				_originalShakeSaturation = ShakeSaturation;
				_originalRemapSaturationZero = RemapSaturationZero;
				_originalRemapSaturationOne = RemapSaturationOne;
				_originalShakeContrast = ShakeContrast;
				_originalRemapContrastZero = RemapContrastZero;
				_originalRemapContrastOne = RemapContrastOne;
				_originalColorFilterMode = ColorFilterMode;
				_originalColorFilterGradient = ColorFilterGradient;
				_originalColorFilterDestination = ColorFilterDestination;
				_originalColorFilterCurve = ColorFilterCurve;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				RelativeValues = relativeValues;
				ShakePostExposure = shakePostExposure;
				RemapPostExposureZero = remapPostExposureZero;
				RemapPostExposureOne = remapPostExposureOne;
				ShakeHueShift = shakeHueShift;
				RemapHueShiftZero = remapHueShiftZero;
				RemapHueShiftOne = remapHueShiftOne;
				ShakeSaturation = shakeSaturation;
				RemapSaturationZero = remapSaturationZero;
				RemapSaturationOne = remapSaturationOne;
				ShakeContrast = shakeContrast;
				RemapContrastZero = remapContrastZero;
				RemapContrastOne = remapContrastOne;
				ColorFilterMode = colorFilterMode;
				ColorFilterGradient = colorFilterGradient;
				ColorFilterDestination = colorFilterDestination;
				ColorFilterCurve = colorFilterCurve;
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
			_colorAdjustments.postExposure.Override(_initialPostExposure);
			_colorAdjustments.hueShift.Override(_initialHueShift);
			_colorAdjustments.saturation.Override(_initialSaturation);
			_colorAdjustments.contrast.Override(_initialContrast);
			_colorAdjustments.colorFilter.Override(_initialColorFilterColor);
		}

		/// <summary>
		/// 重置抖动器的数值。
		/// </summary>
		protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalShakeDuration;
			RelativeValues = _originalRelativeValues;
			ShakePostExposure = _originalShakePostExposure;
			RemapPostExposureZero = _originalRemapPostExposureZero;
			RemapPostExposureOne = _originalRemapPostExposureOne;
			ShakeHueShift = _originalShakeHueShift;
			RemapHueShiftZero = _originalRemapHueShiftZero;
			RemapHueShiftOne = _originalRemapHueShiftOne;
			ShakeSaturation = _originalShakeSaturation;
			RemapSaturationZero = _originalRemapSaturationZero;
			RemapSaturationOne = _originalRemapSaturationOne;
			ShakeContrast = _originalShakeContrast;
			RemapContrastZero = _originalRemapContrastZero;
			RemapContrastOne = _originalRemapContrastOne;
			ColorFilterMode = _originalColorFilterMode;
			ColorFilterGradient = _originalColorFilterGradient;
			ColorFilterDestination = _originalColorFilterDestination;
			ColorFilterCurve = _originalColorFilterCurve;
		}

		/// <summary>
		/// 开始监听事件。
		/// </summary>
		public override void StartListening()
		{
			base.StartListening();
			MMColorAdjustmentsShakeEvent_HDRP.Register(OnMMColorGradingShakeEvent);
		}

		/// <summary>
		/// 停止监听事件
		/// </summary>
		public override void StopListening()
		{
			base.StopListening();
			MMColorAdjustmentsShakeEvent_HDRP.Unregister(OnMMColorGradingShakeEvent);
		}
#endif
    }

    /// <summary>
    /// 用于触发渐晕（画面边缘模糊或变暗效果）抖动的一个事件。
    /// </summary>
    public struct MMColorAdjustmentsShakeEvent_HDRP
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }
		
		public delegate void Delegate(AnimationCurve shakePostExposure, float remapPostExposureZero, float remapPostExposureOne,
			AnimationCurve shakeHueShift, float remapHueShiftZero, float remapHueShiftOne,
			AnimationCurve shakeSaturation, float remapSaturationZero, float remapSaturationOne,
			AnimationCurve shakeContrast, float remapContrastZero, float remapContrastOne,
			MMColorAdjustmentsShaker_HDRP.ColorFilterModes colorFilterMode, Gradient colorFilterGradient, Color colorFilterDestination,AnimationCurve colorFilterCurve,  
			float duration, bool relativeValues = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false);

		static public void Trigger(AnimationCurve shakePostExposure, float remapPostExposureZero, float remapPostExposureOne,
			AnimationCurve shakeHueShift, float remapHueShiftZero, float remapHueShiftOne,
			AnimationCurve shakeSaturation, float remapSaturationZero, float remapSaturationOne,
			AnimationCurve shakeContrast, float remapContrastZero, float remapContrastOne,
			MMColorAdjustmentsShaker_HDRP.ColorFilterModes colorFilterMode, Gradient colorFilterGradient, Color colorFilterDestination,AnimationCurve colorFilterCurve,  
			float duration, bool relativeValues = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke(shakePostExposure, remapPostExposureZero, remapPostExposureOne,
				shakeHueShift, remapHueShiftZero, remapHueShiftOne,
				shakeSaturation, remapSaturationZero, remapSaturationOne,
				shakeContrast, remapContrastZero, remapContrastOne,
				colorFilterMode, colorFilterGradient, colorFilterDestination, colorFilterCurve,
				duration, relativeValues, attenuation, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore);
		}
	}
}