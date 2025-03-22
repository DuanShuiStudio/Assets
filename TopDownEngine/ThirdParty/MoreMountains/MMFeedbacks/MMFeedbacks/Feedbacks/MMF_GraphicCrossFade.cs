#if MM_UI
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you trigger cross fades on a target Graphic.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈将让您在目标图形上触发交叉淡出效果")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("UI/Graphic CrossFade")]
	public class MMF_GraphicCrossFade : MMF_Feedback
	{
        /// 一个静态布尔值用于一次性禁用此类型的所有反馈
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetGraphic == null); }
		public override string RequiredTargetText { get { return TargetGraphic != null ? TargetGraphic.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetGraphic be set to be able to work properly. You can set one below."; } }
#endif

        /// 此反馈的持续时间是图像的持续时间，如果是立即生效则为0
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }
		public override bool HasChannel => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetGraphic = FindAutomatedTarget<Graphic>();

        /// 此反馈的可能模式
        public enum Modes { Alpha, Color }

		[MMFInspectorGroup("Graphic Cross Fade", true, 54, true)]
		/// the Graphic to affect when playing the feedback
		[Tooltip("在播放反馈时受到影响的图形")]
		public Graphic TargetGraphic;
		/// whether the feedback should affect the Image instantly or over a period of time
		[Tooltip("反馈是否应该立即影响图像，还是在一定时间内影响图像")]
		public Modes Mode = Modes.Alpha;
		/// how long the Graphic should change over time
		[Tooltip("图形随时间变化的长度")]
		public float Duration = 0.2f;
		/// the target alpha
		[Tooltip("目标透明度")]
		[MMFEnumCondition("Mode", (int)Modes.Alpha)]
		public float TargetAlpha = 0.2f;
		/// the target color
		[Tooltip("目标颜色")]
		[MMFEnumCondition("Mode", (int)Modes.Color)]
		public Color TargetColor = Color.red;
		/// whether or not the crossfade should also tween the alpha channel
		[Tooltip("交叉淡出是否也应该扭曲透明度通道")]
		[MMFEnumCondition("Mode", (int)Modes.Color)]
		public bool UseAlpha = true;
		/// if this is true, the target will be disabled when this feedbacks is stopped
		[Tooltip("如果为真，当此反馈停止时目标将被禁用")] 
		public bool DisableOnStop = false;
        
		protected Coroutine _coroutine;
		protected Color _initialColor;
		
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);

			if (TargetGraphic != null)
			{
				_initialColor = TargetGraphic.color;	
			}
		}

        /// <summary>
        /// 在播放时，我们会打开图形，并在需要时开始一个随时间变化的协程
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetGraphic == null))
			{
				return;
			}
        
			Turn(true);
			bool ignoreTimeScale = !InScaledTimescaleMode;
			switch (Mode)
			{
				case Modes.Alpha:
                    // 以下几行代码修复了CrossFadeAlpha的bug
                    _initialColor.a = NormalPlayDirection ? 1 : 0;
					TargetGraphic.color = NormalPlayDirection ? _initialColor : TargetColor;
					TargetGraphic.CrossFadeAlpha(NormalPlayDirection ? 0f : 1f, 0f, true);
	                
					TargetGraphic.CrossFadeAlpha(NormalPlayDirection ? TargetAlpha : _initialColor.a, Duration, ignoreTimeScale);
					break;
				case Modes.Color:
					TargetGraphic.CrossFadeColor(NormalPlayDirection ? TargetColor : _initialColor, Duration, ignoreTimeScale, UseAlpha);
					break;
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
		}

        /// <summary>
        /// 开启或关闭图形
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