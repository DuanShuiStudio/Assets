using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 将这个添加到一个光源上，使其能够接收来自反馈的MMLightShakeEvents（多媒体灯光抖动事件），或者让它在本地进行抖动。 
    /// </summary>
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Lights/MM Light Shaker")]
	[RequireComponent(typeof(Light))]
	public class MMLightShaker : MMShaker
	{
		[MMInspectorGroup("Light", true, 37)]
		/// the light to affect when playing the feedback
		[Tooltip("播放反馈时要影响的灯光。 ")]
		public Light BoundLight;
		/// whether or not that light should be turned off on start
		[Tooltip("那盏灯在开始时是否应该被关闭 ")]
		public bool StartsOff = true;
		/// whether or not the values should be relative or not
		[Tooltip("这些数值是否应该是相对的。 ")]
		public bool RelativeValues = true;

		[MMInspectorGroup("Color", true, 41)]
		/// whether or not this shaker should modify color 
		[Tooltip("这个抖动器是否应该修改颜色。 ")]
		public bool ModifyColor = true;
		/// the colors to apply to the light over time
		[Tooltip("随着时间推移应用到灯光上的颜色。 ")]
		public Gradient ColorOverTime;

		[MMInspectorGroup("Intensity", true, 40)]
		/// the intensity to apply to the light over time
		/// the curve to tween the intensity on
		[Tooltip("随着时间推移应用到灯光上的强度。 ")]
		public AnimationCurve IntensityCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
		/// the value to remap the intensity curve's 0 to
		[Tooltip("用于将强度曲线的0值重新映射到的那个值。 ")]
		public float RemapIntensityZero = 0f;
		/// the value to remap the intensity curve's 1 to
		[Tooltip("用于将强度曲线的1值重新映射到的那个值。 ")]
		public float RemapIntensityOne = 1f;

		[MMInspectorGroup("Range", true, 39)]
		/// the range to apply to the light over time
		[Tooltip("随着时间推移应用到灯光上的范围。 ")]
		public AnimationCurve RangeCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
		/// the value to remap the range curve's 0 to
		[Tooltip("用于将范围曲线的0值重新映射到的那个值。 ")]
		public float RemapRangeZero = 0f;
		/// the value to remap the range curve's 0 to
		[Tooltip("用于将范围曲线的1值重新映射到的那个值。 ")]
		public float RemapRangeOne = 10f;

		[MMInspectorGroup("Shadow Strength", true, 38)]
		/// the range to apply to the light over time
		[Tooltip("随着时间推移应用于灯光的范围。 ")]
		public AnimationCurve ShadowStrengthCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
		/// the value to remap the shadow strength's curve's 0 to
		[Tooltip("用于将阴影强度曲线的0值重新映射到的值。 ")]
		public float RemapShadowStrengthZero = 0f;
		/// the value to remap the shadow strength's curve's 1 to
		[Tooltip("用于将阴影强度曲线的1值重新映射到的值。 ")]
		public float RemapShadowStrengthOne = 1f;

		protected Color _initialColor;
		protected float _initialRange;
		protected float _initialIntensity;
		protected float _initialShadowStrength;

		protected bool _originalRelativeValues;
		protected bool _originalModifyColor;
		protected float _originalShakeDuration;
		protected Gradient _originalColorOverTime;
		protected AnimationCurve _originalIntensityCurve;
		protected float _originalRemapIntensityZero;
		protected float _originalRemapIntensityOne;
		protected AnimationCurve _originalRangeCurve;
		protected float _originalRemapRangeZero;
		protected float _originalRemapRangeOne;
		protected AnimationCurve _originalShadowStrengthCurve;
		protected float _originalRemapShadowStrengthZero;
		protected float _originalRemapShadowStrengthOne;

        /// <summary>
        /// 在初始化时，我们对各项数值进行初始化。 
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			if (BoundLight == null)
			{
				BoundLight = this.gameObject.GetComponent<Light>();
			}
		}

        /// <summary>
        /// 当那个抖动器被添加时，我们会初始化它的抖动持续时间。 
        /// </summary>
        protected virtual void Reset()
		{
			ShakeDuration = 1f;
		}

        /// <summary>
        /// 随着时间推移抖动各项数值。 
        /// </summary>
        protected override void Shake()
		{
			float newRange = ShakeFloat(RangeCurve, RemapRangeZero, RemapRangeOne, RelativeValues, _initialRange);
			BoundLight.range = newRange;
			float newIntensity = ShakeFloat(IntensityCurve, RemapIntensityZero, RemapIntensityOne, RelativeValues, _initialIntensity);
			BoundLight.intensity = newIntensity;
			float newShadowStrength = ShakeFloat(ShadowStrengthCurve, RemapShadowStrengthZero, RemapShadowStrengthOne, RelativeValues, _initialShadowStrength);
			BoundLight.shadowStrength = Mathf.Clamp01(newShadowStrength);
			if (ModifyColor)
			{
				BoundLight.color = ColorOverTime.Evaluate(_remappedTimeSinceStart);
			}            
		}

        /// <summary>
        /// 收集目标上的初始值。
        /// </summary>
        protected override void GrabInitialValues()
		{
			_initialColor = BoundLight.color;
			_initialRange = BoundLight.range;
			_initialIntensity = BoundLight.intensity;
			_initialShadowStrength = BoundLight.shadowStrength;
		}

        /// <summary>
        /// 重置目标的各项数值。
        /// </summary>
        protected override void ResetTargetValues()
		{
			base.ResetTargetValues();
			BoundLight.color = _initialColor;
			BoundLight.range = _initialRange;
			BoundLight.intensity = _initialIntensity;
			BoundLight.shadowStrength = _initialShadowStrength;
		}

        /// <summary>
        /// 重置抖动器的数值。
        /// </summary>
        protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ModifyColor = _originalModifyColor;
			RelativeValues = _originalRelativeValues;
			ShakeDuration = _originalShakeDuration;
			ColorOverTime = _originalColorOverTime;
			IntensityCurve = _originalIntensityCurve;
			RemapIntensityZero = _originalRemapIntensityZero;
			RemapIntensityOne = _originalRemapIntensityOne;
			RangeCurve = _originalRangeCurve;
			RemapRangeZero  =_originalRemapRangeZero;
			RemapRangeOne = _originalRemapRangeOne;
			ShadowStrengthCurve = _originalShadowStrengthCurve;
			RemapShadowStrengthZero = _originalRemapShadowStrengthZero;
			RemapShadowStrengthOne = _originalRemapShadowStrengthOne;
		}

        /// <summary>
        /// 开始监听事件。
        /// </summary>
        public override void StartListening()
		{
			base.StartListening();
			MMLightShakeEvent.Register(OnMMLightShakeEvent);
		}

        /// <summary>
        /// 停止监听事件
        /// </summary>
        public override void StopListening()
		{
			base.StopListening();
			MMLightShakeEvent.Unregister(OnMMLightShakeEvent);
		}

		public virtual void OnMMLightShakeEvent(float shakeDuration, bool relativeValues, bool modifyColor, Gradient colorOverTime,
			AnimationCurve intensityCurve, float remapIntensityZero, float remapIntensityOne,
			AnimationCurve rangeCurve, float remapRangeZero, float remapRangeOne,
			AnimationCurve shadowStrengthCurve, float remapShadowStrengthZero, float remapShadowStrengthOne,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true,
			bool useRange = false, float eventRange = 0f, Vector3 eventOriginPosition = default(Vector3))
		{
			if (!CheckEventAllowed(channelData, useRange, eventRange, eventOriginPosition) || (!Interruptible && Shaking))
			{
				return;
			}

			_resetShakerValuesAfterShake = resetShakerValuesAfterShake;
			_resetTargetValuesAfterShake = resetTargetValuesAfterShake;

			if (resetShakerValuesAfterShake)
			{
				_originalModifyColor = ModifyColor;
				_originalRelativeValues = RelativeValues;
				_originalShakeDuration = ShakeDuration;
				_originalColorOverTime = ColorOverTime;
				_originalIntensityCurve = IntensityCurve;
				_originalRemapIntensityZero = RemapIntensityZero;
				_originalRemapIntensityOne = RemapIntensityOne;
				_originalRangeCurve = RangeCurve;
				_originalRemapRangeZero = RemapRangeZero;
				_originalRemapRangeOne = RemapRangeOne;
				_originalShadowStrengthCurve = ShadowStrengthCurve;
				_originalRemapShadowStrengthZero = RemapShadowStrengthZero;
				_originalRemapShadowStrengthOne = RemapShadowStrengthOne;
			}

			if (!OnlyUseShakerValues)
			{
				ModifyColor = modifyColor;
				RelativeValues = relativeValues;
				ShakeDuration = shakeDuration;
				ColorOverTime = colorOverTime;
				IntensityCurve = intensityCurve;
				RemapIntensityZero = remapIntensityZero;
				RemapIntensityOne = remapIntensityOne;
				RangeCurve = rangeCurve;
				RemapRangeZero = remapRangeZero;
				RemapRangeOne = remapRangeOne;
				ShadowStrengthCurve = shadowStrengthCurve;
				RemapShadowStrengthZero = remapShadowStrengthZero;
				RemapShadowStrengthOne = remapShadowStrengthOne;
			}

			Play();
		}
	}
           
	public struct MMLightShakeEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(float shakeDuration, bool relativeValues, bool modifyColor, Gradient colorOverTime, 
			AnimationCurve intensityCurve, float remapIntensityZero, float remapIntensityOne,
			AnimationCurve rangeCurve, float remapRangeZero, float remapRangeOne, 
			AnimationCurve shadowStrengthCurve, float remapShadowStrengthZero, float remapShadowStrengthOne,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true,
			bool useRange = false, float eventRange = 0f, Vector3 eventOriginPosition = default(Vector3));

		static public void Trigger(float shakeDuration, bool relativeValues, bool modifyColor, Gradient colorOverTime,
			AnimationCurve intensityCurve, float remapIntensityZero, float remapIntensityOne,
			AnimationCurve rangeCurve, float remapRangeZero, float remapRangeOne,
			AnimationCurve shadowStrengthCurve, float remapShadowStrengthZero, float remapShadowStrengthOne,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true,
			bool useRange = false, float eventRange = 0f, Vector3 eventOriginPosition = default(Vector3))
		{
			OnEvent?.Invoke(shakeDuration, relativeValues, modifyColor, colorOverTime,
				intensityCurve, remapIntensityZero, remapIntensityOne,
				rangeCurve, remapRangeZero, remapRangeOne,
				shadowStrengthCurve, remapShadowStrengthZero, remapShadowStrengthOne, 
				feedbacksIntensity, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, 
				useRange, eventRange, eventOriginPosition);
		}
	}
}