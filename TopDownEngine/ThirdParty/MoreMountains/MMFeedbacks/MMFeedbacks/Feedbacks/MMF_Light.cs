using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 此反馈将让您在播放时控制灯光的颜色和强度
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("此反馈可让您在特定持续时间内（或立即）控制场景中灯光的颜色和强度。")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Lights/Light")]
	public class MMF_Light : MMF_Feedback
	{
        /// 一个静态布尔值用于一次性禁用此类型的所有反馈
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.LightColor; } }
		public override bool EvaluateRequiresSetup() { return (BoundLight == null); }
		public override string RequiredTargetText { get { return BoundLight != null ? BoundLight.name : "";  } }
		public override string RequiresSetupText { get { return "此反馈要求设置一个BoundLight才能正常工作。您可以在下面设置一个"; } }
#endif

        /// 此反馈的持续时间是灯光的持续时间，如果是立即则为0
        public override float FeedbackDuration { get { return (Mode == Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { Duration = value; } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => BoundLight = FindAutomatedTarget<Light>();

		/// the possible modes for this feedback
		public enum Modes { OverTime, Instant, ShakerEvent, ToDestination }

		[MMFInspectorGroup("Light", true, 37, true)]
		/// the light to affect when playing the feedback
		[Tooltip("播放反馈时要影响的灯光。")]
		public Light BoundLight;
		/// whether the feedback should affect the light instantly or over a period of time
		[Tooltip("反馈是否应该立即影响灯光，延时等待后影响")]
		public Modes Mode = Modes.OverTime;
		/// how long the light should change over time
		[Tooltip("延时等待影响灯光的时间")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent, (int)Modes.ToDestination)]
		public float Duration = 0.2f;
		/// whether or not that light should be turned off on start
		[Tooltip("是否在开始时关闭该灯光")]
		public bool StartsOff = true;
		/// if this is true, the light will be disabled when this feedbacks is stopped
		[Tooltip("如果为真，当此反馈停止时，灯光将被禁用")] 
		public bool DisableOnStop = false;
		/// whether or not the values should be relative or not
		[Tooltip("值是否应该是相对的或绝对的")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent, (int)Modes.Instant)]
		public bool RelativeValues = true;
		/// whether or not to reset shaker values after shake
		[Tooltip("是否在摇动后重置摇动值")]
		[MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("是否在摇动后重置目标的值")]
		[MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
		public bool ResetTargetValuesAfterShake = true;
		/// whether or not to broadcast a range to only affect certain shakers
		[Tooltip("是否广播一个范围以仅影响某些摇动器")]
		[MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
		public bool OnlyBroadcastInRange = false;
		/// the range of the event, in units
		[Tooltip("事件的范围（以单位计）")]
		[MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
		public float EventRange = 100f;
		/// the transform to use to broadcast the event as origin point
		[Tooltip("用作广播事件原点的变换")]
		[MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
		public Transform EventOriginTransform;
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("如果为真，调用该反馈将触发它，即使它正在进行中。如果为假，它将阻止任何新的播放，直到当前的播放结束。")] 
		public bool AllowAdditivePlays = false;

		[MMFInspectorGroup("Color", true, 38, true)]
		/// whether or not to modify the color of the light
		[Tooltip("是否修改灯光的颜色")]
		public bool ModifyColor = true;
		/// the colors to apply to the light over time
		[Tooltip("随时间应用于灯光的颜色")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
		public Gradient ColorOverTime;
		/// the color to move to in instant mode
		[Tooltip("立即模式下要移动到的颜色")]
		[MMFEnumCondition("Mode", (int)Modes.Instant, (int)Modes.ShakerEvent)]
		public Color InstantColor = Color.red;
		/// the color to move to in destination mode
		[Tooltip("目标模式下要移动到的颜色")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public Color ToDestinationColor = Color.red;

		[MMFInspectorGroup("Intensity", true, 39, true)]
		/// whether or not to modify the intensity of the light
		[Tooltip("是否修改灯光的强度")]
		public bool ModifyIntensity = true;
		/// the curve to tween the intensity on
		[Tooltip("要调整强度的曲线")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent, (int)Modes.ToDestination)]
		public AnimationCurve IntensityCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
		/// the value to remap the intensity curve's 0 to
		[Tooltip("将强度曲线的0值重新映射到的值")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
		public float RemapIntensityZero = 0f;
		/// the value to remap the intensity curve's 1 to
		[Tooltip("将强度曲线的1值重新映射到的值")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
		public float RemapIntensityOne = 1f;
		/// the value to move the intensity to in instant mode
		[Tooltip("在即时模式下要移动到的强度值")]
		[MMFEnumCondition("Mode", (int)Modes.Instant)]
		public float InstantIntensity = 1f;
		/// the value to move the intensity to in ToDestination mode
		[Tooltip("在目标模式下要移动到的强度值")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public float ToDestinationIntensity = 1f;

		[MMFInspectorGroup("Range", true, 40, true)]
		/// whether or not to modify the range of the light
		[Tooltip("是否修改灯光的范围")]
		public bool ModifyRange = true;
		/// the range to apply to the light over time
		[Tooltip("随时间应用于灯光的范围")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent, (int)Modes.ToDestination)]
		public AnimationCurve RangeCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
		/// the value to remap the range curve's 0 to
		[Tooltip("将范围曲线的0值重新映射到的值")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
		public float RemapRangeZero = 0f;
		/// the value to remap the range curve's 0 to
		[Tooltip("将范围曲线的1值重新映射到的值")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
		public float RemapRangeOne = 10f;
		/// the value to move the intensity to in instant mode
		[Tooltip("在立即模式下要移动到的范围值")]
		[MMFEnumCondition("Mode", (int)Modes.Instant)]
		public float InstantRange = 10f;
		/// the value to move the intensity to in ToDestination mode
		[Tooltip("在目标模式下要移动到的范围值")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public float ToDestinationRange = 10f;

		[MMFInspectorGroup("Shadow Strength", true, 41, true)]
		/// whether or not to modify the shadow strength of the light
		[Tooltip("是否修改灯光的阴影强度")]
		public bool ModifyShadowStrength = true;
		/// the range to apply to the light over time
		[Tooltip("随时间应用于灯光的阴影范围")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent, (int)Modes.ToDestination)]
		public AnimationCurve ShadowStrengthCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
		/// the value to remap the shadow strength's curve's 0 to
		[Tooltip("将阴影强度曲线的0值重新映射到的值")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
		public float RemapShadowStrengthZero = 0f;
		/// the value to remap the shadow strength's curve's 1 to
		[Tooltip("将阴影强度曲线的1值重新映射到的值")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
		public float RemapShadowStrengthOne = 1f;
		/// the value to move the shadow strength to in instant mode
		[Tooltip("在立即模式下要移动到的阴影强度值")]
		[MMFEnumCondition("Mode", (int)Modes.Instant)]
		public float InstantShadowStrength = 1f;
		/// the value to move the shadow strength to in ToDestination mode
		[Tooltip("在目标模式下要移动到的阴影强度值")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public float ToDestinationShadowStrength = 1f;

		protected float _initialRange;
		protected float _initialShadowStrength;
		protected float _initialIntensity;
		protected Coroutine _coroutine;
		protected Color _initialColor;
		protected Color _targetColor;

        /// <summary>
        /// 在初始化时，如果需要的话，我们会关闭灯光
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);

			if (BoundLight == null)
			{
				return;
			}
            
			_initialRange = BoundLight.range;
			_initialShadowStrength = BoundLight.shadowStrength;
			_initialIntensity = BoundLight.intensity;
			_initialColor = BoundLight.color;

			if (EventOriginTransform == null)
			{
				EventOriginTransform = owner.transform;
			}

			if (Active)
			{
				if (StartsOff)
				{
					Turn(false);
				}
			}
		}

        /// <summary>
        /// 在播放时，我们会打开灯光，并在需要时开始一个随时间变化的协程
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (Mode == Modes.ToDestination)
			{
				_initialRange = BoundLight.range;
				_initialShadowStrength = BoundLight.shadowStrength;
				_initialIntensity = BoundLight.intensity;
				_initialColor = BoundLight.color;
			}
			
			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
			Turn(true);
			switch (Mode)
			{
				case Modes.Instant:
					BoundLight.intensity = InstantIntensity * intensityMultiplier;
					BoundLight.shadowStrength = InstantShadowStrength;
					BoundLight.range = InstantRange;
					if (ModifyColor)
					{
						BoundLight.color = InstantColor;
					}                        
					break;
				case Modes.OverTime:
				case Modes.ToDestination:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }
					_coroutine = Owner.StartCoroutine(LightSequence(intensityMultiplier));
					break;
				case Modes.ShakerEvent:
					MMLightShakeEvent.Trigger(FeedbackDuration, RelativeValues, ModifyColor, ColorOverTime, IntensityCurve,
						RemapIntensityZero, RemapIntensityOne, RangeCurve, RemapRangeZero * intensityMultiplier, RemapRangeOne * intensityMultiplier,
						ShadowStrengthCurve, RemapShadowStrengthZero, RemapShadowStrengthOne, feedbacksIntensity,
						ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake,
						OnlyBroadcastInRange, EventRange, EventOriginTransform.position);
					break;
			}
		}

        /// <summary>
        /// 这个随时间变化的协程将随时间修改灯光的强度和颜色
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator LightSequence(float intensityMultiplier)
		{
			IsPlaying = true;
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

				SetLightValues(remappedTime, intensityMultiplier);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			SetLightValues(FinalNormalizedTime, intensityMultiplier);
			if (DisableOnStop)
			{
				Turn(false);
			}            
			IsPlaying = false;
			_coroutine = null;
			yield return null;
		}


        /// <summary>
        /// 在指定时间（介于0和1之间）设置灯光的各种值
        /// </summary>
        /// <param name="time"></param>
        protected virtual void SetLightValues(float time, float intensityMultiplier)
		{
			float intensity = 0f;
			float range = 0f;
			float shadowStrength = 0f;    
			
			switch (Mode)
			{
				case Modes.OverTime:
					intensity = MMFeedbacksHelpers.Remap(IntensityCurve.Evaluate(time), 0f, 1f, RemapIntensityZero, RemapIntensityOne);
					range = MMFeedbacksHelpers.Remap(RangeCurve.Evaluate(time), 0f, 1f, RemapRangeZero, RemapRangeOne);
					shadowStrength = MMFeedbacksHelpers.Remap(ShadowStrengthCurve.Evaluate(time), 0f, 1f, RemapShadowStrengthZero, RemapShadowStrengthOne);    
					_targetColor = ColorOverTime.Evaluate(time);
					break;
				case Modes.ToDestination:
					intensity = Mathf.Lerp(_initialIntensity, ToDestinationIntensity, IntensityCurve.Evaluate(time));
					range = Mathf.Lerp(_initialRange, ToDestinationRange, RangeCurve.Evaluate(time));
					shadowStrength = Mathf.Lerp(_initialShadowStrength, ToDestinationShadowStrength, ShadowStrengthCurve.Evaluate(time));
					_targetColor = Color.Lerp(_initialColor, ToDestinationColor, time);
					break;
			}    

			if (RelativeValues && (Mode != Modes.ToDestination))
			{
				intensity += _initialIntensity;
				shadowStrength += _initialShadowStrength;
				range += _initialRange;
			}

			if (ModifyIntensity)
			{
				BoundLight.intensity = intensity * intensityMultiplier;	
			}
			if (ModifyRange)
			{
				BoundLight.range = range;	
			}
			if (ModifyShadowStrength)
			{
				BoundLight.shadowStrength = Mathf.Clamp01(shadowStrength);	
			}
			if (ModifyColor)
			{
				BoundLight.color = _targetColor;
			}
		}

        /// <summary>
        /// 在停止时关闭灯光
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!FeedbackTypeAuthorized)
			{
				return;
			}
            
			base.CustomStopFeedback(position, feedbacksIntensity);
			IsPlaying = false;
			if (Active && (_coroutine != null))
			{
				Owner.StopCoroutine(_coroutine);
				_coroutine = null;
			}
			if (Active && DisableOnStop)
			{
				Turn(false);
			}
		}

        /// <summary>
        /// 打开或关闭灯光
        /// </summary>
        /// <param name="status"></param>
        protected virtual void Turn(bool status)
		{
			BoundLight.enabled = status;
		}

        /// <summary>
        /// 在恢复时，我们将对象放回其初始位置
        /// </summary>
        protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			
			BoundLight.range = _initialRange;
			BoundLight.shadowStrength = _initialShadowStrength;
			BoundLight.intensity = _initialIntensity;
			BoundLight.color = _initialColor;

			if (StartsOff)
			{
				Turn(false);
			}
		}
	}
}