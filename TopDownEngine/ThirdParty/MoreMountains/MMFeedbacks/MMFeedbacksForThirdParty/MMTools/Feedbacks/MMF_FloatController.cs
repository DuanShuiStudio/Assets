using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// 此反馈将在目标浮点控制器上触发一次播放操作。 
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈可让您在目标浮点控制器上触发一次播放操作。 ")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.MMTools")]
	[FeedbackPath("GameObject/FloatController")]
	public class MMF_FloatController : MMF_Feedback
	{
		/// 一个用于一次性禁用所有此类反馈的静态布尔值。
		public static bool FeedbackTypeAuthorized = true;
		/// 不同的可能模式
		public enum Modes { OneTime, ToDestination }
		/// 设置此反馈在检查器中的颜色。 
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetFloatController == null); }
		public override string RequiredTargetText { get { return TargetFloatController != null ? TargetFloatController.name : "";  } }
		public override string RequiresSetupText { get { return "此反馈要求设置一个目标浮点控制器，以便能够正常工作。您可以在下面设置一个。 "; } }
		#endif
		public override bool HasRandomness => true;
		public override bool CanForceInitialValue => true;
		public override bool ForceInitialValueDelayed => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetFloatController = FindAutomatedTarget<FloatController>();

		[MMFInspectorGroup("Float Controller", true, 36, true)]
		/// the mode this controller is in
		[Tooltip("这个控制器所处的模式")]
		public Modes Mode = Modes.OneTime;
		/// the float controller to trigger a one time play on
		[Tooltip("要在其上触发一次播放操作的浮点控制器 ")]
		public FloatController TargetFloatController;
		/// a list of extra and optional float controllers to trigger a one time play on
		[Tooltip("一个额外且可选的浮点控制器列表，用于在这些控制器上触发一次播放操作 ")]
		public List<FloatController> ExtraTargetFloatControllers;
		/// whether this should revert to original at the end
		[Tooltip("这在最后是否应该恢复到原始状态 ")]
		public bool RevertToInitialValueAfterEnd = false;
		/// the duration of the One Time shake
		[Tooltip("一次性抖动的持续时间")]
		[MMFEnumCondition("Mode", (int)Modes.OneTime)]
		public float OneTimeDuration = 1f;
		/// the amplitude of the One Time shake (this will be multiplied by the curve's height)
		[Tooltip("一次性抖动的振幅（此值将与曲线的高度相乘） ")]
		[MMFEnumCondition("Mode", (int)Modes.OneTime)]
		public float OneTimeAmplitude = 1f;
		/// the low value to remap the normalized curve value to 
		[Tooltip("要将归一化曲线值重新映射到的下限值 ")]
		[MMFEnumCondition("Mode", (int)Modes.OneTime)]
		public float OneTimeRemapMin = 0f;
		/// the high value to remap the normalized curve value to 
		[Tooltip("要将归一化曲线值重新映射到的上限值 ")]
		[MMFEnumCondition("Mode", (int)Modes.OneTime)]
		public float OneTimeRemapMax = 1f;
		/// the curve to apply to the one time shake
		[Tooltip("要应用于一次性抖动的曲线")]
		[MMFEnumCondition("Mode", (int)Modes.OneTime)]
		public AnimationCurve OneTimeCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to move this float controller to
		[Tooltip("要将此浮点控制器移动到的值")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public float ToDestinationValue = 1f;
		/// the duration over which to move the value
		[Tooltip("移动该值所花费的时间长度 ")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public float ToDestinationDuration = 1f;
		/// the curve over which to move the value in ToDestination mode
		[Tooltip("在“ToDestination（到目标位置）”模式下，用于移动该值的曲线 ")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public AnimationCurve ToDestinationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));

		/// 此反馈的持续时间就是那一次命中的持续时间。 
		public override float FeedbackDuration
		{
			get { return (Mode == Modes.OneTime) ? ApplyTimeMultiplier(OneTimeDuration) : ApplyTimeMultiplier(ToDestinationDuration); } 
			set { OneTimeDuration = value; ToDestinationDuration = value; }
		}

		protected float _oneTimeDurationStorage;
		protected float _oneTimeAmplitudeStorage;
		protected float _oneTimeRemapMinStorage;
		protected float _oneTimeRemapMaxStorage;
		protected AnimationCurve _oneTimeCurveStorage;
		protected float _toDestinationValueStorage;
		protected float _toDestinationDurationStorage;
		protected AnimationCurve _toDestinationCurveStorage;
		protected bool _revertToInitialValueAfterEndStorage;

		/// <summary>
		/// 在初始化时，我们获取目标浮点控制器上的初始值。 
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			if (Active && (TargetFloatController != null))
			{
				_oneTimeDurationStorage = TargetFloatController.OneTimeDuration;
				_oneTimeAmplitudeStorage = TargetFloatController.OneTimeAmplitude;
				_oneTimeCurveStorage = TargetFloatController.OneTimeCurve;
				_oneTimeRemapMinStorage = TargetFloatController.OneTimeRemapMin;
				_oneTimeRemapMaxStorage = TargetFloatController.OneTimeRemapMax;
				_toDestinationCurveStorage = TargetFloatController.ToDestinationCurve;
				_toDestinationDurationStorage = TargetFloatController.ToDestinationDuration;
				_toDestinationValueStorage = TargetFloatController.ToDestinationValue;
				_revertToInitialValueAfterEndStorage = TargetFloatController.RevertToInitialValueAfterEnd;
			}
		}

		/// <summary>
		/// 在播放时，我们在目标浮点控制器上触发一次播放操作或者“到目标位置”的播放操作。 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetFloatController == null))
			{
				return;
			}
            
			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
			HandleFloatController(TargetFloatController, intensityMultiplier);
			foreach (FloatController floatController in ExtraTargetFloatControllers)
			{
				HandleFloatController(floatController, intensityMultiplier);
			}
		}

		/// <summary>
		/// 将值应用于目标浮点控制器并触发该控制器。 
		/// </summary>
		/// <param name="target"></param>
		/// <param name="intensityMultiplier"></param>
		protected virtual void HandleFloatController(FloatController target, float intensityMultiplier)
		{
			target.RevertToInitialValueAfterEnd = RevertToInitialValueAfterEnd;

			if (Mode == Modes.OneTime)
			{
				target.OneTimeDuration = FeedbackDuration;
				target.OneTimeAmplitude = OneTimeAmplitude;
				target.OneTimeCurve = OneTimeCurve;
				if (NormalPlayDirection)
				{
					target.OneTimeRemapMin = OneTimeRemapMin * intensityMultiplier;
					target.OneTimeRemapMax = OneTimeRemapMax * intensityMultiplier;
				}
				else
				{
					target.OneTimeRemapMin = OneTimeRemapMax * intensityMultiplier;
					target.OneTimeRemapMax = OneTimeRemapMin * intensityMultiplier;   
				}
				target.OneTime();
			}
			if (Mode == Modes.ToDestination)
			{
				target.ToDestinationCurve = ToDestinationCurve;
				target.ToDestinationDuration = FeedbackDuration;
				target.ToDestinationValue = ToDestinationValue;
				target.ToDestination();
			}
		}

		/// <summary>
		/// 在重置时，我们会用最初存储的值来重置目标控制器上的各项数值。 
		/// </summary>
		protected override void CustomReset()
		{
			base.CustomReset();
			if (Active && FeedbackTypeAuthorized && (TargetFloatController != null))
			{
				ResetFloatController(TargetFloatController);
				foreach (FloatController controller in ExtraTargetFloatControllers)
				{
					ResetFloatController(controller);
				}
			}
		}

		protected virtual void ResetFloatController(FloatController controller)
		{
			controller.OneTimeDuration = _oneTimeDurationStorage;
			controller.OneTimeAmplitude = _oneTimeAmplitudeStorage;
			controller.OneTimeCurve = _oneTimeCurveStorage;
			controller.OneTimeRemapMin = _oneTimeRemapMinStorage;
			controller.OneTimeRemapMax = _oneTimeRemapMaxStorage;
			controller.ToDestinationCurve = _toDestinationCurveStorage;
			controller.ToDestinationDuration = _toDestinationDurationStorage;
			controller.ToDestinationValue = _toDestinationValueStorage;
			controller.RevertToInitialValueAfterEnd = _revertToInitialValueAfterEndStorage;
		}


		/// <summary>
		/// 在停止时，如果移动操作处于激活状态，我们就会中断该移动。 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			if (TargetFloatController != null)
			{
				TargetFloatController.Stop();
				foreach (FloatController controller in ExtraTargetFloatControllers)
				{
					controller.Stop();
				}
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
			TargetFloatController.RestoreInitialValues();
			foreach (FloatController controller in ExtraTargetFloatControllers)
			{
				controller.RestoreInitialValues();
			}
		}
	}
}