#if MM_UI
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 此反馈将让您随时间改变目标精灵渲染器的颜色，并在X或Y轴上翻转它。您还可以使用它来控制一个或多个MMSpriteRendererShakers。
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("此反馈将让您随时间改变目标图像的颜色。您还可以使用它来控制一个或多个MMImageShakers")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("UI/Image")]
	public class MMF_Image : MMF_Feedback
	{
        /// 一个静态布尔值用于一次性禁用此类型的所有反馈
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		public override bool EvaluateRequiresSetup() { return (BoundImage == null); }
		public override string RequiredTargetText { get { return BoundImage != null ? BoundImage.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a BoundImage be set to be able to work properly. You can set one below."; } }
#endif

        /// 此反馈的持续时间是图像的时长，如果瞬间完成则为0
        public override float FeedbackDuration { get { return (Mode == Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { Duration = value; } }
		public override bool HasChannel => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => BoundImage = FindAutomatedTarget<Image>();

        /// 此反馈的可能模式
        public enum Modes { OverTime, Instant }

		[MMFInspectorGroup("Image", true, 54, true)]
		/// the Image to affect when playing the feedback
		[Tooltip("播放反馈时要影响的图片")]
		public Image BoundImage;
		/// whether the feedback should affect the Image instantly or over a period of time
		[Tooltip("反馈是否应该立即影响图片，还是延时影响")]
		public Modes Mode = Modes.OverTime;
		/// how long the Image should change over time
		[Tooltip("延时影响图片的时间")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public float Duration = 0.2f;
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("如果为真，即使反馈正在进行中，调用该反馈也会触发它。如果为假，它将阻止任何新的播放，直到当前的播放结束")] 
		public bool AllowAdditivePlays = false;
		/// whether or not to modify the color of the image
		[Tooltip("是否修改图片的颜色")]
		public bool ModifyColor = true;
		/// the colors to apply to the Image over time
		[Tooltip("随时间应用于图片的颜色")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public Gradient ColorOverTime;
		/// the color to move to in instant mode
		[Tooltip("在即时模式下要切换到的颜色")]
		[MMFEnumCondition("Mode", (int)Modes.Instant)]
		public Color InstantColor;
		/// whether or not that Image should be turned off on start
		[Tooltip("是否在开始时关闭该图片")]
		[FormerlySerializedAs("StartsOff")]
		public bool DisableOnInit = false;
		/// if this is true, the target will be enabled when this feedback gets played
		[Tooltip("如果为真，当播放此反馈时将启用目标")] 
		public bool EnableOnPlay = true;
		/// if this is true, the target disabled after the color over time change ends
		[Tooltip("如果为真，在颜色随时间变化结束后将禁用目标")]
		public bool DisableOnSequenceEnd = false;
		/// if this is true, the target will be disabled when this feedbacks is stopped
		[Tooltip("如果为真，当此反馈停止时将禁用目标")] 
		public bool DisableOnStop = false;

		protected Coroutine _coroutine;
		protected Color _initialColor;

        /// <summary>
        /// 在初始化时，如果需要则关闭图片
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);

			if (Active)
			{
				if (DisableOnInit)
				{
					Turn(false);
				}
			}
		}

        /// <summary>
        /// 在播放时，如果需要则打开图片并启动一个随时间变化的协程
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
        
			_initialColor = BoundImage.color;
			if (EnableOnPlay)
			{
				Turn(true);	
			}
			switch (Mode)
			{
				case Modes.Instant:
					if (ModifyColor)
					{
						BoundImage.color = InstantColor;
					}
					break;
				case Modes.OverTime:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }
					_coroutine = Owner.StartCoroutine(ImageSequence());
					break;
			}
		}

        /// <summary>
        /// 这个协程将修改图片上的值
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator ImageSequence()
		{
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;

			IsPlaying = true;
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

				SetImageValues(remappedTime);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			SetImageValues(FinalNormalizedTime);
			if (DisableOnSequenceEnd)
			{
				Turn(false);
			}
			IsPlaying = false;
			_coroutine = null;
			yield return null;
		}

        /// <summary>
        /// 在指定时间（介于0和1之间）设置精灵渲染器上的不同值
        /// </summary>
        /// <param name="time"></param>
        protected virtual void SetImageValues(float time)
		{
			if (ModifyColor)
			{
				BoundImage.color = ColorOverTime.Evaluate(time);
			}
		}

        /// <summary>
        /// 在停止时关闭精灵渲染器。
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
        /// 开启或关闭精灵渲染器
        /// </summary>
        /// <param name="status"></param>
        protected virtual void Turn(bool status)
		{
			BoundImage.gameObject.SetActive(status);
			BoundImage.enabled = status;
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
			BoundImage.color = _initialColor;
		}
	}
}
#endif