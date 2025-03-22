using UnityEngine;
using System.Collections;
using UnityEngine.Scripting.APIUpdating;
#if MM_UI
using UnityEngine.UI;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback lets you control the color of a target Text over time
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("这种反馈功能使你能够随着时间的推移控制目标文本的颜色。")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("UI/Text Color")]
	public class MMF_TextColor : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用此类所有反馈。
        public static bool FeedbackTypeAuthorized = true;
		public enum ColorModes { Instant, Gradient, Interpolate }

        /// 此反馈的持续时间就是颜色过渡的时长，若为即时变化，则持续时间为 0 。
        public override float FeedbackDuration { get { return (ColorMode == ColorModes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { Duration = value; } }

        /// 设置此反馈的检查器颜色。
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetText == null); }
		public override string RequiredTargetText { get { return TargetText != null ? TargetText.name : "";  } }
		public override string RequiresSetupText { get { return "这种反馈需要设置一个目标文本才能正常工作。你可以在下面设置一个。"; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetText = FindAutomatedTarget<Text>();

		[MMFInspectorGroup("Target", true, 58, true)]
		/// the Text component to control
		[Tooltip(" 要控制的文本组件。")]
		public Text TargetText;

		[MMFInspectorGroup("Color", true, 36)]
		/// the selected color mode :
		/// None : nothing will happen,
		/// gradient : evaluates the color over time on that gradient, from left to right,
		/// interpolate : lerps from the current color to the destination one 
		[Tooltip("所选择的颜色模式：" +
                 "None 无: 不会发生任何事情," +
                 "gradient渐变：随着时间推移，按照该渐变（效果）从左至右来评估颜色变化，" +
                 "interpolate插值：从当前颜色线性插值（通过线性差值运算）到目标颜色。")]
		public ColorModes ColorMode = ColorModes.Interpolate;
		/// how long the color of the text should change over time
		[Tooltip("文本颜色随时间变化所应持续的时长。\r\n")]
		[MMFEnumCondition("ColorMode", (int)ColorModes.Interpolate, (int)ColorModes.Gradient)]
		public float Duration = 0.2f;
		/// the color to apply
		[Tooltip("要应用的颜色。")]
		[MMFEnumCondition("ColorMode", (int)ColorModes.Instant)]
		public Color InstantColor = Color.yellow;
		/// the gradient to use to animate the color over time
		[Tooltip("用于随时间对颜色进行动画处理的渐变效果。 ")]
		[MMFEnumCondition("ColorMode", (int)ColorModes.Gradient)]
		[GradientUsage(true)]
		public Gradient ColorGradient;
		/// the destination color when in interpolate mode
		[Tooltip("在插值模式下的目标颜色。")]
		[MMFEnumCondition("ColorMode", (int)ColorModes.Interpolate)]
		public Color DestinationColor = Color.yellow;
		/// the curve to use when interpolating towards the destination color
		[Tooltip("在朝着目标颜色进行插值运算时所使用的曲线。 ")]
		[MMFEnumCondition("ColorMode", (int)ColorModes.Interpolate)]
		public AnimationCurve ColorCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("如果此项为真，即便反馈处于进行中，调用该反馈也会触发它。如果为假，在当前反馈结束之前，将阻止任何新的播放操作。 ")] 
		public bool AllowAdditivePlays = false;

		protected Color _initialColor;
		protected Coroutine _coroutine;

        /// <summary>
        /// 在初始化时，我们存储其初始颜色。 
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);

			if (TargetText == null)
			{
				return;
			}

			_initialColor = TargetText.color;
		}

        /// <summary>
        /// 在播放时，我们会更改文本的颜色。 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetText == null))
			{
				return;
			}

			switch (ColorMode)
			{
				case ColorModes.Instant:
					TargetText.color = InstantColor;
					break;
				case ColorModes.Gradient:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }
					_coroutine = Owner.StartCoroutine(ChangeColor());
					break;
				case ColorModes.Interpolate:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }
					_coroutine = Owner.StartCoroutine(ChangeColor());
					break;
			}
		}

        /// <summary>
        /// 随着时间的推移改变文本的颜色。 
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator ChangeColor()
		{
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;
            
			IsPlaying = true;
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

				SetColor(remappedTime);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			SetColor(FinalNormalizedTime);
			_coroutine = null;
			IsPlaying = false;
			yield break;
		}

        /// <summary>
        /// 应用颜色更改。
        /// </summary>
        /// <param name="time"></param>
        protected virtual void SetColor(float time)
		{
			if (ColorMode == ColorModes.Gradient)
			{
				TargetText.color = ColorGradient.Evaluate(time);
			}
			else if (ColorMode == ColorModes.Interpolate)
			{
				float factor = ColorCurve.Evaluate(time);
				TargetText.color = Color.LerpUnclamped(_initialColor, DestinationColor, factor);
			}
		}

        /// <summary>
        /// 如有必要，停止协程。 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			base.CustomStopFeedback(position, feedbacksIntensity);
			if (Active && (_coroutine != null))
			{
				Owner.StopCoroutine(_coroutine);
				_coroutine = null;
			}
		}

        /// <summary>
        /// 在恢复（状态）时，我们将对象放回其初始位置。 
        /// </summary>
        protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			TargetText.color = _initialColor;
		}
	}
}
#endif