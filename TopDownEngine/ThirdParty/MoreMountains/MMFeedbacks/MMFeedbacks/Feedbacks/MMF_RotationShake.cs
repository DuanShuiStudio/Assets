using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback lets you emit a RotationShake event. This will be caught by MMRotationShakers (on the specified channel).
	/// Rotation shakers, as the name suggests, are used to shake the rotation of a transform, along a direction, with optional noise and other fine control options.
	/// </summary>
	[AddComponentMenu("")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Transform/Rotation Shake")]
	[FeedbackHelp("该反馈能让你发出一个旋转抖动（RotationShake）事件。此事件将被 MMRotationShakers（在指定频道上）捕获。" +
                  " 旋转抖动器，顾名思义，用于沿着某个方向使变换的旋转产生抖动，同时具备可选的噪声设置以及其他精细控制选项。")]
	public class MMF_RotationShake : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用此类所有反馈。
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈在检视面板中的颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
		public override string RequiredTargetText { get { return RequiredChannelText; } }
#endif
        /// 返回该反馈的持续时长。
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;
		public override bool HasRange => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetShaker = FindAutomatedTarget<MMRotationShaker>();

		[MMFInspectorGroup("Optional Target", true, 33)]
		/// a specific (and optional) shaker to target, regardless of its channel
		[Tooltip("一个特定（且可选）的抖动器作为目标，无论其所在频道。")]
		public MMRotationShaker TargetShaker;
		
		[MMFInspectorGroup("Rotation Shake", true, 28)]
		/// the duration of the shake, in seconds
		[Tooltip("抖动持续的时长，以秒为单位。")]
		public float Duration = 0.5f;
		/// whether or not to reset shaker values after shake
		[Tooltip("抖动结束后是否重置抖动器的值。")]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("抖动结束后是否重置目标对象的值。")]
		public bool ResetTargetValuesAfterShake = true;
		
		[MMFInspectorGroup("Shake Settings", true, 42)]
		/// the speed at which the transform should shake
		[Tooltip("变换组件应抖动的速度")]
		public float ShakeSpeed = 20f;
		/// the maximum distance from its initial rotation the transform will move to during the shake
		[Tooltip("在抖动过程中，变换组件相对于其初始旋转位置移动的最大距离")]
		public float ShakeRange = 50f;
        
		[MMFInspectorGroup("Direction", true, 43)]
		/// the direction along which to shake the transform's rotation
		[Tooltip("用于使变换组件的旋转产生抖动的方向")]
		public Vector3 ShakeMainDirection = Vector3.up;
		/// if this is true, instead of using ShakeMainDirection as the direction of the shake, a random vector3 will be generated, randomized between ShakeMainDirection and ShakeAltDirection
		[Tooltip("如果该项为真，那么将不再使用 “ShakeMainDirection”（主抖动方向）作为抖动方向，而是会在 “ShakeMainDirection” 和 “ShakeAltDirection”（备用抖动方向）之间生成一个随机的三维向量。\r\n")]
		public bool RandomizeDirection = false;
		/// when in RandomizeDirection mode, a vector against which to randomize the main direction
		[Tooltip("在 “随机化方向” 模式下，用于使主方向产生随机变化的一个向量")]
		[MMFCondition("RandomizeDirection", true)]
		public Vector3 ShakeAltDirection = Vector3.up;
		/// if this is true, a new direction will be randomized every time a shake happens
		[Tooltip("如果这一项为真，那么每次发生抖动时都会随机生成一个新方向。")]
		public bool RandomizeDirectionOnPlay = false;

		[MMFInspectorGroup("Directional Noise", true, 47)]
		/// whether or not to add noise to the main direction
		[Tooltip("是否在主方向上添加噪声")]
		public bool AddDirectionalNoise = true;
		/// when adding directional noise, noise strength will be randomized between this value and DirectionalNoiseStrengthMax
		[Tooltip("在添加方向噪声时，噪声强度将在此值与 “DirectionalNoiseStrengthMax”（方向噪声强度最大值）之间随机取值。\r\n")]
		[MMFCondition("AddDirectionalNoise", true)]
		public Vector3 DirectionalNoiseStrengthMin = new Vector3(0.25f, 0.25f, 0.25f);
		/// when adding directional noise, noise strength will be randomized between this value and DirectionalNoiseStrengthMin
		[Tooltip("当添加方向噪声时，噪声强度将在该值与 “DirectionalNoiseStrengthMin”（方向噪声强度最小值）之间随机化。")]
		[MMFCondition("AddDirectionalNoise", true)]
		public Vector3 DirectionalNoiseStrengthMax = new Vector3(0.25f, 0.25f, 0.25f);
        
		[MMFInspectorGroup("Randomness", true, 44)]
		/// a unique seed you can use to get different outcomes when shaking more than one transform at once
		[Tooltip("一个独特的随机种子，当同时抖动多个变换组件时，你可以用它来获得不同的抖动效果。")]
		public Vector3 RandomnessSeed;
		/// whether or not to generate a unique seed automatically on every shake
		[Tooltip("每次抖动时是否自动生成一个唯一的随机种子")]
		public bool RandomizeSeedOnShake = true;

		[MMFInspectorGroup("One Time", true, 45)]
		/// whether or not to use attenuation, which will impact the amplitude of the shake, along the defined curve
		[Tooltip("是否使用衰减，这将根据定义的曲线影响抖动的幅度。")]
		public bool UseAttenuation = true;
		/// the animation curve used to define attenuation, impacting the amplitude of the shake
		[Tooltip("用于定义衰减的动画曲线，该曲线会影响抖动的幅度。\r\n")]
		[MMFCondition("UseAttenuation", true)]
		public AnimationCurve AttenuationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));

        /// <summary>
        /// 触发相应的协同程序。
        /// </summary>
        /// <param name="rotation"></param>
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
				MMRotationShakeEvent.Trigger(Duration, ShakeSpeed,  ShakeRange,  ShakeMainDirection,  RandomizeDirection,  ShakeAltDirection,  RandomizeDirectionOnPlay,  AddDirectionalNoise, 
					DirectionalNoiseStrengthMin,  DirectionalNoiseStrengthMax,  RandomnessSeed,  RandomizeSeedOnShake,  UseAttenuation,  AttenuationCurve,
					UseRange, RangeDistance, UseRangeFalloff, RangeFalloff, RemapRangeFalloff, position,
					intensityMultiplier, ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, ComputedTimescaleMode);
			}
			else
			{
				TargetShaker?.OnMMRotationShakeEvent(Duration, ShakeSpeed,  ShakeRange,  ShakeMainDirection,  RandomizeDirection,  ShakeAltDirection,  RandomizeDirectionOnPlay,  AddDirectionalNoise, 
					DirectionalNoiseStrengthMin,  DirectionalNoiseStrengthMax,  RandomnessSeed,  RandomizeSeedOnShake,  UseAttenuation,  AttenuationCurve,
					UseRange, RangeDistance, UseRangeFalloff, RangeFalloff, RemapRangeFalloff, position,
					intensityMultiplier, TargetShaker.ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, ComputedTimescaleMode);
			}
		}

        /// <summary>
        /// 停止时，我们停止过渡操作。
        /// </summary>
        /// <param name="rotation"></param>
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
				MMRotationShakeEvent.Trigger(Duration, ShakeSpeed,  ShakeRange,  ShakeMainDirection,  RandomizeDirection,  ShakeAltDirection,  RandomizeDirectionOnPlay,  AddDirectionalNoise, 
					DirectionalNoiseStrengthMin,  DirectionalNoiseStrengthMax,  RandomnessSeed,  RandomizeSeedOnShake,  UseAttenuation,  AttenuationCurve, stop:true);
			}
			else
			{
				TargetShaker?.OnMMRotationShakeEvent(Duration, ShakeSpeed,  ShakeRange,  ShakeMainDirection,  RandomizeDirection,  ShakeAltDirection,  RandomizeDirectionOnPlay,  AddDirectionalNoise, 
					DirectionalNoiseStrengthMin,  DirectionalNoiseStrengthMax,  RandomnessSeed,  RandomizeSeedOnShake,  UseAttenuation,  AttenuationCurve, channelData: TargetShaker.ChannelData, stop:true);
			}
		}

        /// <summary>
        /// 恢复时，我们将恢复初始状态。
        /// </summary>
        protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			
			if (TargetShaker == null)
			{
				MMRotationShakeEvent.Trigger(Duration, ShakeSpeed,  ShakeRange,  ShakeMainDirection,  RandomizeDirection,  ShakeAltDirection,  RandomizeDirectionOnPlay,  AddDirectionalNoise, 
					DirectionalNoiseStrengthMin,  DirectionalNoiseStrengthMax,  RandomnessSeed,  RandomizeSeedOnShake,  UseAttenuation,  AttenuationCurve, restore:true);
			}
			else
			{
				TargetShaker?.OnMMRotationShakeEvent(Duration, ShakeSpeed,  ShakeRange,  ShakeMainDirection,  RandomizeDirection,  ShakeAltDirection,  RandomizeDirectionOnPlay,  AddDirectionalNoise, 
					DirectionalNoiseStrengthMin,  DirectionalNoiseStrengthMax,  RandomnessSeed,  RandomizeSeedOnShake,  UseAttenuation,  AttenuationCurve, channelData: TargetShaker.ChannelData, restore:true);
			}
		}
	}
}