using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you change the speed of a target animator, either once, or instantly and then reset it, or interpolate it over time
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈将允许你改变目标动画器的速度，可以改变一次，或瞬间改变然后重置，或者随时间插值改变")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Animation/Animator Speed")]
	public class MMF_AnimatorSpeed : MMF_Feedback 
	{
        /// 一个静态布尔值，用于一次性禁用此类型的所有反馈
        public static bool FeedbackTypeAuthorized = true;

        /// 设置此反馈在检查器中的颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.AnimationColor; } }
		public override bool EvaluateRequiresSetup() { return (BoundAnimator == null); }
		public override string RequiredTargetText { get { return BoundAnimator != null ? BoundAnimator.name : "";  } }
		public override string RequiresSetupText { get { return "此反馈需要绑定一个动画器才能正常工作。你可以在下面设置一个"; } }
		#endif
		public override bool HasRandomness => true;
		public override bool CanForceInitialValue => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => BoundAnimator = FindAutomatedTarget<Animator>();

		public enum SpeedModes { Once, InstantThenReset, OverTime }
		
		[MMFInspectorGroup("Animation", true, 12, true)]
		/// the animator whose parameters you want to update
		[Tooltip("你想更新其参数的动画器")]
		public Animator BoundAnimator;

		[MMFInspectorGroup("Speed", true, 14, true)]
		/// whether to change the speed of the target animator once, instantly and reset it later, or have it change over time
		[Tooltip("是否改变目标动画器的速度一次，瞬间改变然后稍后重置，还是让它随时间改变")]
		public SpeedModes Mode = SpeedModes.Once; 
		/// the new minimum speed at which to set the animator - value will be randomized between min and max
		[Tooltip("要设置动画器的新的最小速度 - 该值将在最小值和最大值之间随机化")]
		public float NewSpeedMin = 0f; 
		/// the new maximum speed at which to set the animator - value will be randomized between min and max
		[Tooltip("要设置动画器的新最大速度 - 该值将在最小值和最大值之间随机化")]
		public float NewSpeedMax = 0f;
		/// when in instant then reset or over time modes, the duration of the effect
		[Tooltip("在“瞬间然后重置”或“随时间变化”模式中，效果的持续时间")]
		[MMFEnumCondition("Mode", (int)SpeedModes.InstantThenReset, (int)SpeedModes.OverTime)]
		public float Duration = 1f;
		/// when in over time mode, the curve against which to evaluate the new speed
		[Tooltip("在“随时间变化”模式中，用于评估新速度的曲线")]
		[MMFEnumCondition("Mode", (int)SpeedModes.OverTime)]
		public AnimationCurve Curve = new AnimationCurve(new Keyframe(0, 0f), new Keyframe(0.5f, 1f), new Keyframe(1, 0f));

		protected Coroutine _coroutine;
		protected float _initialSpeed;
		protected float _startedAt;

        /// <summary>
        /// 在播放时，检查是否绑定了动画器并触发参数
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (BoundAnimator == null)
			{
				Debug.LogWarning("没有为 " + Owner.name + " 设置动画器");
				return;
			}

			if (!IsPlaying)
			{
				_initialSpeed = BoundAnimator.speed;	
			}

			if (Mode == SpeedModes.Once)
			{
				BoundAnimator.speed = ComputeIntensity(DetermineNewSpeed(), position);
			}
			else
			{
				if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }
				_coroutine = Owner.StartCoroutine(ChangeSpeedCo());
			}
		}

        /// <summary>
        /// 在“持续一段时间”模式中使用的协程
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator ChangeSpeedCo()
		{
			if (Mode == SpeedModes.InstantThenReset)
			{
				IsPlaying = true;
				BoundAnimator.speed = DetermineNewSpeed();
				yield return MMCoroutine.WaitFor(Duration);
				BoundAnimator.speed = _initialSpeed;	
				IsPlaying = false;
			}
			else if (Mode == SpeedModes.OverTime)
			{
				IsPlaying = true;
				_startedAt = FeedbackTime;
				float newTargetSpeed = DetermineNewSpeed();
				while (FeedbackTime - _startedAt < Duration)
				{
					float time = MMFeedbacksHelpers.Remap(FeedbackTime - _startedAt, 0f, Duration, 0f, 1f);
					float t = Curve.Evaluate(time);
					BoundAnimator.speed = Mathf.Max(0f, MMFeedbacksHelpers.Remap(t, 0f, 1f, _initialSpeed, newTargetSpeed));
					yield return null;
				}
				BoundAnimator.speed = _initialSpeed;	
				IsPlaying = false;
			}
		}

        /// <summary>
        /// 确定目标动画器的新速度
        /// </summary>
        /// <returns></returns>
        protected virtual float DetermineNewSpeed()
		{
			return Mathf.Abs(Random.Range(NewSpeedMin, NewSpeedMax));
		}

        /// <summary>
        /// 在停止时，将布尔参数设置为false
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (_coroutine != null)
			{
				Owner.StopCoroutine(_coroutine);	
			}

			BoundAnimator.speed = _initialSpeed;
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

			BoundAnimator.speed = _initialSpeed;
		}
	}
}