using System.Collections;
using UnityEngine;
#if MM_UI
using UnityEngine.UI;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you change the color of a target Graphic over time.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈将让您随时间改变目标图形的颜色")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("UI/Graphic")]
	public class MMF_Graphic : MMF_Feedback
	{
        /// 一个静态布尔值用于一次性禁用此类型的所有反馈
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetGraphic == null); }
		public override string RequiredTargetText { get { return TargetGraphic != null ? TargetGraphic.name : "";  } }
		public override string RequiresSetupText { get { return "此反馈需要设置一个目标图形才能正常工作。您可以在下面设置一个"; } }
#endif

        /// 此反馈的持续时间是图形的持续时间，如果是立即生效则为0。
        public override float FeedbackDuration { get { return (Mode == Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { Duration = value; } }
		public override bool HasChannel => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetGraphic = FindAutomatedTarget<Graphic>();

        /// 此反馈的可能模式。
        public enum Modes { OverTime, Instant }

		[MMFInspectorGroup("Graphic", true, 54, true)]
		/// the Graphic to affect when playing the feedback
		[Tooltip("在播放反馈时受到影响的图形")]
		public Graphic TargetGraphic;
		/// whether the feedback should affect the Graphic instantly or over a period of time
		[Tooltip("反馈是否应该立即影响图形，还是在一定时间内影响图形")]
		public Modes Mode = Modes.OverTime;
		/// how long the Graphic should change over time
		[Tooltip("图形随时间变化的长度")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public float Duration = 0.2f;
		/// whether or not that Graphic should be turned off on start
		[Tooltip("是否在开始时关闭该图形")]
		public bool StartsOff = false;
		/// if this is true, the target will be disabled when this feedbacks is stopped
		[Tooltip("如果为真，当此反馈停止时目标将被禁用")] 
		public bool DisableOnStop = false;
        
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("如果为真，即使反馈正在进行中，调用该反馈也会触发它。如果为假，它会阻止任何新的播放，直到当前的播放结束")] 
		public bool AllowAdditivePlays = false;
		/// whether or not to modify the color of the Graphic
		[Tooltip("是否修改图形的颜色")]
		public bool ModifyColor = true;
		/// the colors to apply to the Graphic over time
		[Tooltip("随时间应用于图形的颜色")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public Gradient ColorOverTime;
		/// the color to move to in instant mode
		[Tooltip("在立即模式下要切换到的颜色")]
		[MMFEnumCondition("Mode", (int)Modes.Instant)]
		public Color InstantColor;

		protected Coroutine _coroutine;
		protected Color _initialColor;

        /// <summary>
        /// 在初始化时，如果需要的话，我们会关闭图形
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);

			if (Active)
			{
				if (StartsOff)
				{
					Turn(false);
				}
			}
		}

        /// <summary>
        /// 在播放时，我们会打开图形，并在需要时开始一个随时间变化的协程
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			_initialColor = TargetGraphic.color;
			Turn(true);
			switch (Mode)
			{
				case Modes.Instant:
					if (ModifyColor)
					{
						TargetGraphic.color = InstantColor;
					}
					break;
				case Modes.OverTime:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }
					_coroutine = Owner.StartCoroutine(GraphicSequence());
					break;
			}
		}

        /// <summary>
        /// 这个协程将修改图形上的值
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator GraphicSequence()
		{
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;

			IsPlaying = true;
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

				SetGraphicValues(remappedTime);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			SetGraphicValues(FinalNormalizedTime);
			if (StartsOff)
			{
				Turn(false);
			}
			IsPlaying = false;
			if (_coroutine != null)
			{
				Owner.StopCoroutine(_coroutine);	
			}
			_coroutine = null;
			yield return null;
		}

        /// <summary>
        /// 在指定时间（介于0和1之间）设置图形上的不同值
        /// </summary>
        /// <param name="time"></param>
        protected virtual void SetGraphicValues(float time)
		{
			if (ModifyColor)
			{
				TargetGraphic.color = ColorOverTime.Evaluate(time);
			}
		}

        /// <summary>
        /// 在停止时关闭图形
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			IsPlaying = false;
			base.CustomStopFeedback(position, feedbacksIntensity);
			if (Active && DisableOnStop)
			{
				Turn(false);    
			}

			if (_coroutine != null)
			{
				Owner.StopCoroutine(_coroutine);
			}

			_coroutine = null;
		}

        /// <summary>
        /// 打开或关闭图形
        /// </summary>
        /// <param name="status"></param>
        protected virtual void Turn(bool status)
		{
			TargetGraphic.gameObject.SetActive(status);
			TargetGraphic.enabled = status;
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
			TargetGraphic.color = _initialColor;
		}
	}
}
#endif