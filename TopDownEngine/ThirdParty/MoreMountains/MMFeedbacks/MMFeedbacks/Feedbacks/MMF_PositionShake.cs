using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback lets you emit a PositionShake event. This will be caught by MMPositionShakers (on the specified channel).
	/// Position shakers, as the name suggests, are used to shake the position of a transform, along a direction, with optional noise and other fine control options.
	/// </summary>
	[AddComponentMenu("")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("此反馈允许您发射位置震动器（PositionShake）事件。这将由MMPositionShakers（在指定通道上）捕获。" +
                  "顾名思义，位置震动器用于沿方向震动变换组件的位置，带有可选的噪声和其他精细控制选项。")]
	public class MMF_PositionShake : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用所有这种类型的反馈
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
		public override string RequiredTargetText => RequiredChannelText;
#endif
        /// 返回此反馈的持续时间
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;
		public override bool HasRange => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetShaker = FindAutomatedTarget<MMPositionShaker>();

		[MMFInspectorGroup("Optional Target", true, 33)]
		/// a specific (and optional) shaker to target, regardless of its channel
		[Tooltip("一个特定的（可选的）震动器，无论其通道如何，都能作为目标")]
		public MMPositionShaker TargetShaker;
		
		[MMFInspectorGroup("Position Shake", true, 28)]
		/// the duration of the shake, in seconds
		[Tooltip("震动的持续时间（以秒为单位）")]
		public float Duration = 0.5f;
		/// whether or not to reset shaker values after shake
		[Tooltip("是否在震动后重置震动器的值")]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("是否在震动后重置目标的值")]
		public bool ResetTargetValuesAfterShake = true;
		
		[MMFInspectorGroup("Shake Settings", true, 42)]
		/// the speed at which the transform should shake
		[Tooltip("变换震动的速度")]
		public float ShakeSpeed = 20f;
		/// the maximum distance from its initial position the transform will move to during the shake
		[Tooltip("在震动过程中，变换将从其初始位置移动的最大距离")]
		public float ShakeRange = 0.5f;
        
		[MMFInspectorGroup("Direction", true, 43)]
		/// the direction along which to shake the transform's position
		[Tooltip("震动变换位置的方向")]
		public Vector3 ShakeMainDirection = Vector3.up;
		/// if this is true, instead of using ShakeMainDirection as the direction of the shake, a random vector3 will be generated, randomized between ShakeMainDirection and ShakeAltDirection
		[Tooltip("如果启用此选项，则不会使用 Shak eMainDirection 作为震动的方向，而是在 ShakeMainDirection 和 ShakeAltDirection 之间生成一个随机的三维向量作为震动方向")]
		public bool RandomizeDirection = false;
		/// when in RandomizeDirection mode, a vector against which to randomize the main direction
		[Tooltip("在 “随机方向” 模式下，用于随机化主方向的向量")]
		[MMFCondition("RandomizeDirection", true)]
		public Vector3 ShakeAltDirection = Vector3.up;
		/// if this is true, a new direction will be randomized every time a shake happens
		[Tooltip("如果启用此选项，则每次发生震动时都会随机生成一个新方向")]
		public bool RandomizeDirectionOnPlay = false;
		/// whether or not to randomize the x value of the main direction
		[Tooltip("是否对主方向的 x 值进行随机化")]
		public bool RandomizeDirectionX = true;
		/// whether or not to randomize the y value of the main direction
		[Tooltip("是否对主方向的 y 值进行随机化")]
		public bool RandomizeDirectionY = true;
		/// whether or not to randomize the z value of the main direction
		[Tooltip("是否对主方向的 z 值进行随机化")]
		public bool RandomizeDirectionZ= true;
		
		[MMFInspectorGroup("Directional Noise", true, 47)]
		/// whether or not to add noise to the main direction
		[Tooltip("是否在主方向上添加噪声")]
		public bool AddDirectionalNoise = true;
		/// when adding directional noise, noise strength will be randomized between this value and DirectionalNoiseStrengthMax
		[Tooltip("在添加方向噪声时，噪声强度将在该值与 DirectionalNoiseStrengthMax 之间随机化")]
		[MMFCondition("AddDirectionalNoise", true)]
		public Vector3 DirectionalNoiseStrengthMin = new Vector3(0.25f, 0.25f, 0.25f);
		/// when adding directional noise, noise strength will be randomized between this value and DirectionalNoiseStrengthMin
		[Tooltip("在添加方向噪声时，噪声强度将在该值与 DirectionalNoiseStrengthMin 之间随机化")]
		[MMFCondition("AddDirectionalNoise", true)]
		public Vector3 DirectionalNoiseStrengthMax = new Vector3(0.25f, 0.25f, 0.25f);
        
		[MMFInspectorGroup("Randomness", true, 44)]
		/// a unique seed you can use to get different outcomes when shaking more than one transform at once
		[Tooltip("一个独特的种子，可在同时震动多个变换时获得不同的结果")]
		public Vector3 RandomnessSeed;
		/// whether or not to generate a unique seed automatically on every shake
		[Tooltip("是否在每次震动时自动生成一个独特的种子")]
		public bool RandomizeSeedOnShake = true;

		[MMFInspectorGroup("One Time", true, 45)]
		/// whether or not to use attenuation, which will impact the amplitude of the shake, along the defined curve
		[Tooltip("是否使用衰减，这将沿着定义的曲线影响震动的振幅")]
		public bool UseAttenuation = true;
		/// the animation curve used to define attenuation, impacting the amplitude of the shake
		[Tooltip("用于定义衰减的动画曲线，影响震动的振幅")]
		[MMFCondition("UseAttenuation", true)]
		public AnimationCurve AttenuationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));

        /// <summary>
        /// 触发相应的协程
        /// </summary>
        /// <param name="position"></param>
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
				MMPositionShakeEvent.Trigger(Duration, ShakeSpeed,  ShakeRange,  ShakeMainDirection,  RandomizeDirection,  ShakeAltDirection,  RandomizeDirectionOnPlay,  
					RandomizeDirectionX, RandomizeDirectionY, RandomizeDirectionZ, AddDirectionalNoise, 
					DirectionalNoiseStrengthMin,  DirectionalNoiseStrengthMax,  RandomnessSeed,  RandomizeSeedOnShake,  UseAttenuation,  AttenuationCurve,
					UseRange, RangeDistance, UseRangeFalloff, RangeFalloff, RemapRangeFalloff, position,
					intensityMultiplier, ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, ComputedTimescaleMode);
			}
			else
			{
				TargetShaker?.OnMMPositionShakeEvent(Duration, ShakeSpeed,  ShakeRange,  ShakeMainDirection,  RandomizeDirection,  ShakeAltDirection,  RandomizeDirectionOnPlay,  
					RandomizeDirectionX, RandomizeDirectionY, RandomizeDirectionZ, AddDirectionalNoise, 
					DirectionalNoiseStrengthMin,  DirectionalNoiseStrengthMax,  RandomnessSeed,  RandomizeSeedOnShake,  UseAttenuation,  AttenuationCurve,
					UseRange, RangeDistance, UseRangeFalloff, RangeFalloff, RemapRangeFalloff, position,
					intensityMultiplier, TargetShaker.ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, ComputedTimescaleMode);	
			}
		}

        /// <summary>
        /// 停止时，我们停止转换
        /// </summary>
        /// <param name="position"></param>
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
				MMPositionShakeEvent.Trigger(Duration, ShakeSpeed,  ShakeRange,  ShakeMainDirection,  RandomizeDirection,  ShakeAltDirection,  RandomizeDirectionOnPlay,  
					RandomizeDirectionX, RandomizeDirectionY, RandomizeDirectionZ, AddDirectionalNoise, 
					DirectionalNoiseStrengthMin,  DirectionalNoiseStrengthMax,  RandomnessSeed,  RandomizeSeedOnShake,  UseAttenuation,  AttenuationCurve, stop:true);	
			}
			else
			{
				TargetShaker?.OnMMPositionShakeEvent(Duration, ShakeSpeed,  ShakeRange,  ShakeMainDirection,  RandomizeDirection,  ShakeAltDirection,  RandomizeDirectionOnPlay,  
					RandomizeDirectionX, RandomizeDirectionY, RandomizeDirectionZ, AddDirectionalNoise, 
					DirectionalNoiseStrengthMin,  DirectionalNoiseStrengthMax,  RandomnessSeed,  RandomizeSeedOnShake,  UseAttenuation,  AttenuationCurve, channelData:TargetShaker.ChannelData, stop:true);	
			}
		}

        /// <summary>
        /// 在恢复时，我们恢复初始状态
        /// </summary>
        protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (TargetShaker == null)
			{
				MMPositionShakeEvent.Trigger(Duration, ShakeSpeed,  ShakeRange,  ShakeMainDirection,  RandomizeDirection,  ShakeAltDirection,  RandomizeDirectionOnPlay,  
					RandomizeDirectionX, RandomizeDirectionY, RandomizeDirectionZ, AddDirectionalNoise, 
					DirectionalNoiseStrengthMin,  DirectionalNoiseStrengthMax,  RandomnessSeed,  RandomizeSeedOnShake,  UseAttenuation,  AttenuationCurve, restore:true);	
			}
			else
			{
				TargetShaker?.OnMMPositionShakeEvent(Duration, ShakeSpeed,  ShakeRange,  ShakeMainDirection,  RandomizeDirection,  ShakeAltDirection,  RandomizeDirectionOnPlay,  
					RandomizeDirectionX, RandomizeDirectionY, RandomizeDirectionZ, AddDirectionalNoise, 
					DirectionalNoiseStrengthMin,  DirectionalNoiseStrengthMax,  RandomnessSeed,  RandomizeSeedOnShake,  UseAttenuation,  AttenuationCurve, channelData:TargetShaker.ChannelData, restore:true);	
			}
		}
	}
}