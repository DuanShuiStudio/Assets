using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback lets you emit a ScaleShake event. This will be caught by MMScaleShakers (on the specified channel).
	/// Scale shakers, as the name suggests, are used to shake the scale of a transform, along a direction, with optional noise and other fine control options.
	/// </summary>
	[AddComponentMenu("")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Transform/Scale Shake")]
	[FeedbackHelp("该反馈使你能够发出 “缩放抖动”（ScaleShake）事件。此事件将被 “MM 缩放抖动器”（MMScaleShakers ，在指定通道上）捕获。" +
                  " 缩放抖动器，顾名思义，用于沿着某个方向抖动变换的缩放比例，同时可选择添加噪声以及使用其他精细控制选项。")]
	public class MMF_ScaleShake : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用此类所有反馈。
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色。
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
		public override string RequiredTargetText => RequiredChannelText;
#endif
        /// 返回该反馈的时长。
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;
		public override bool HasRange => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetShaker = FindAutomatedTarget<MMScaleShaker>();

		[MMFInspectorGroup("Optional Target", true, 33)]
		/// a specific (and optional) shaker to target, regardless of its channel
		[Tooltip("一个特定的（并且是可选的）要作为目标的振动器，无论其处于哪个通道。")]
		public MMScaleShaker TargetShaker;
		
		[MMFInspectorGroup("Scale Shake", true, 28)]
		/// the duration of the shake, in seconds
		[Tooltip("震动的持续时间，以秒为单位")]
		public float Duration = 0.5f;
		/// whether or not to reset shaker values after shake
		[Tooltip("震动之后是否重置振动器的值")]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("震动之后是否重置目标对象的各项数值。\r\n")]
		public bool ResetTargetValuesAfterShake = true;
		
		[MMFInspectorGroup("Shake Settings", true, 42)]
		/// the speed at which the transform should shake
		[Tooltip("变换组件进行震动时应达到的速度")]
		public float ShakeSpeed = 20f;
		/// the maximum distance from its initial scale the transform will move to during the shake
		[Tooltip("在震动过程中，变换组件从其初始缩放比例移动到的最大距离。")]
		public float ShakeRange = 0.5f;
        
		[MMFInspectorGroup("Direction", true, 43)]
		/// the direction along which to shake the transform's scale
		[Tooltip("用于使变换组件的缩放比例产生震动的方向")]
		public Vector3 ShakeMainDirection = Vector3.up;
		/// if this is true, instead of using ShakeMainDirection as the direction of the shake, a random vector3 will be generated, randomized between ShakeMainDirection and ShakeAltDirection
		[Tooltip("如果此值为真，那么将不会使用 “ShakeMainDirection”（主摇晃方向）作为摇晃方向，而是会生成一个随机的三维向量，该向量在 “ShakeMainDirection”（主摇晃方向）和 “ShakeAltDirection”（备用摇晃方向）之间随机取值。")]
		public bool RandomizeDirection = false;
		/// when in RandomizeDirection mode, a vector against which to randomize the main direction
		[Tooltip("当处于 “随机化方向” 模式时，用于使主方向产生随机变化的一个向量。")]
		[MMFCondition("RandomizeDirection", true)]
		public Vector3 ShakeAltDirection = Vector3.up;
		/// if this is true, a new direction will be randomized every time a shake happens
		[Tooltip("如果这为真，那么每次发生摇晃时都会随机生成一个新的方向。")]
		public bool RandomizeDirectionOnPlay = false;

		[MMFInspectorGroup("Directional Noise", true, 47)]
		/// whether or not to add noise to the main direction
		[Tooltip("是否在主方向上添加噪点（干扰因素）")]
		public bool AddDirectionalNoise = true;
		/// when adding directional noise, noise strength will be randomized between this value and DirectionalNoiseStrengthMax
		[Tooltip("当添加方向噪声时，噪声强度将在此值与 “DirectionalNoiseStrengthMax”（方向噪声强度最大值）之间随机取值。\r\n")]
		[MMFCondition("AddDirectionalNoise", true)]
		public Vector3 DirectionalNoiseStrengthMin = new Vector3(0.25f, 0.25f, 0.25f);
		/// when adding directional noise, noise strength will be randomized between this value and DirectionalNoiseStrengthMin
		[Tooltip("当添加方向噪声时，噪声强度将在此值与 “DirectionalNoiseStrengthMin”（方向噪声强度最小值）之间随机取值。")]
		[MMFCondition("AddDirectionalNoise", true)]
		public Vector3 DirectionalNoiseStrengthMax = new Vector3(0.25f, 0.25f, 0.25f);
        
		[MMFInspectorGroup("Randomness", true, 44)]
		/// a unique seed you can use to get different outcomes when shaking more than one transform at once
		[Tooltip("一个独特的随机种子，当你同时摇晃多个变换（物体）时，可利用它来获得不同的效果。")]
		public Vector3 RandomnessSeed;
		/// whether or not to generate a unique seed automatically on every shake
		[Tooltip("是否在每次摇晃时自动生成一个唯一的随机种子。")]
		public bool RandomizeSeedOnShake = true;

		[MMFInspectorGroup("One Time", true, 45)]
		/// whether or not to use attenuation, which will impact the amplitude of the shake, along the defined curve
		[Tooltip("是否使用衰减，这将沿着定义的曲线影响摇晃的幅度。")]
		public bool UseAttenuation = true;
		/// the animation curve used to define attenuation, impacting the amplitude of the shake
		[Tooltip("用于定义衰减的动画曲线，它会对摇晃的幅度产生影响。")]
		[MMFCondition("UseAttenuation", true)]
		public AnimationCurve AttenuationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));

        /// <summary>
        /// 触发相应的协程。
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			
			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
			
			if (TargetShaker == null)
			{
				MMScaleShakeEvent.Trigger(Duration, ShakeSpeed,  ShakeRange,  ShakeMainDirection,  RandomizeDirection,  ShakeAltDirection,  RandomizeDirectionOnPlay,  AddDirectionalNoise, 
					DirectionalNoiseStrengthMin,  DirectionalNoiseStrengthMax,  RandomnessSeed,  RandomizeSeedOnShake,  UseAttenuation,  AttenuationCurve,
					UseRange, RangeDistance, UseRangeFalloff, RangeFalloff, RemapRangeFalloff, position,
					intensityMultiplier, ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, ComputedTimescaleMode);
			}
			else
			{
				TargetShaker?.OnMMScaleShakeEvent(Duration, ShakeSpeed,  ShakeRange,  ShakeMainDirection,  RandomizeDirection,  ShakeAltDirection,  RandomizeDirectionOnPlay,  AddDirectionalNoise, 
					DirectionalNoiseStrengthMin,  DirectionalNoiseStrengthMax,  RandomnessSeed,  RandomizeSeedOnShake,  UseAttenuation,  AttenuationCurve,
					UseRange, RangeDistance, UseRangeFalloff, RangeFalloff, RemapRangeFalloff, position,
					intensityMultiplier, TargetShaker.ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, ComputedTimescaleMode);
			}
		}

        /// <summary>
        /// 在停止时，我们停止我们的过渡。
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);
			
			if (TargetShaker == null)
			{
				MMScaleShakeEvent.Trigger(Duration, ShakeSpeed,  ShakeRange,  ShakeMainDirection,  RandomizeDirection,  ShakeAltDirection,  RandomizeDirectionOnPlay,  AddDirectionalNoise, 
					DirectionalNoiseStrengthMin,  DirectionalNoiseStrengthMax,  RandomnessSeed,  RandomizeSeedOnShake,  UseAttenuation,  AttenuationCurve, stop:true);
			}
			else
			{
				TargetShaker?.OnMMScaleShakeEvent(Duration, ShakeSpeed,  ShakeRange,  ShakeMainDirection,  RandomizeDirection,  ShakeAltDirection,  RandomizeDirectionOnPlay,  AddDirectionalNoise, 
					DirectionalNoiseStrengthMin,  DirectionalNoiseStrengthMax,  RandomnessSeed,  RandomizeSeedOnShake,  UseAttenuation,  AttenuationCurve, channelData:TargetShaker.ChannelData, stop:true);
			}
		}

        /// <summary>
        /// 在恢复时，我们恢复到初始状态。
        /// </summary>
        protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			
			if (TargetShaker == null)
			{
				MMScaleShakeEvent.Trigger(Duration, ShakeSpeed,  ShakeRange,  ShakeMainDirection,  RandomizeDirection,  ShakeAltDirection,  RandomizeDirectionOnPlay,  AddDirectionalNoise, 
					DirectionalNoiseStrengthMin,  DirectionalNoiseStrengthMax,  RandomnessSeed,  RandomizeSeedOnShake,  UseAttenuation,  AttenuationCurve, restore:true);
			}
			else
			{
				TargetShaker?.OnMMScaleShakeEvent(Duration, ShakeSpeed,  ShakeRange,  ShakeMainDirection,  RandomizeDirection,  ShakeAltDirection,  RandomizeDirectionOnPlay,  AddDirectionalNoise, 
					DirectionalNoiseStrengthMin,  DirectionalNoiseStrengthMax,  RandomnessSeed,  RandomizeSeedOnShake,  UseAttenuation,  AttenuationCurve, channelData:TargetShaker.ChannelData, restore:true);
			}
		}
	}
}