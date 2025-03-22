using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;
#if MM_UI
using UnityEngine.UI;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 此反馈在播放时会触发一个闪烁事件（由MMFlash捕捉）
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("在播放时，此反馈将广播一个MMFlashEvent。如果您创建一个带有MMFlash组件的UI图像（请参见演示场景中的示例），它将拦截该事件并进行闪烁（通常您希望它占据整个屏幕大小，但这并不是必需的）。在反馈的检查器中，您可以定义闪烁的颜色、持续时间、透明度以及FlashID。为了使反馈和MMFlash协同工作，反馈和MMFlash上的FlashID需要相同。这允许您在场景中拥有多个MMFlash，并分别使它们闪烁。")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Camera/Flash")]
	public class MMF_Flash : MMF_Feedback
	{
        /// 一个静态布尔值用于一次性禁用此类型的所有反馈
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
		public override string RequiredTargetText => RequiredChannelText;
		public override bool HasCustomInspectors => true;
		public override bool HasAutomaticShakerSetup => true;
#endif
        /// 此反馈的持续时间就是闪烁的持续时间
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(FlashDuration); } set { FlashDuration = value; } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;

		[MMFInspectorGroup("Flash", true, 37)]
		/// the color of the flash
		[Tooltip("闪烁的颜色")]
		public Color FlashColor = Color.white;
		/// the flash duration (in seconds)
		[Tooltip("\r\n闪烁一次的持续时间是多少毫秒？\r\n闪烁的持续时间有多长？\r\n\r\n")]
		public float FlashDuration = 0.2f;
		/// the alpha of the flash
		[Tooltip("闪烁的透明度（阿尔法值）")]
		public float FlashAlpha = 1f;
		/// the ID of the flash (usually 0). You can specify on each MMFlash object an ID, allowing you to have different flash images in one scene and call them separately (one for damage, one for health pickups, etc)
		[Tooltip("闪烁的ID（通常为0）。您可以在每个MMFlash对象上指定一个ID，从而允许您在一个场景中拥有不同的闪烁图像，并分别调用它们（一个用于伤害，一个用于拾取生命值等）。")]
		public int FlashID = 0;

		[Header("Optional Target可选目标")] 
		/// this field lets you bind a specific MMFlash to this feedback. If left empty, the feedback will trigger a MMFlashEvent instead, targeting all matching flashes. If you fill it, only that specific MMFlash will be targeted.
		[Tooltip("此字段允许您将特定的MMFlash与该反馈绑定。如果留空，该反馈将触发一个MMFlashEvent，目标是所有匹配的闪烁对象。如果您填写了此字段，则只有那个特定的MMFlash会被作为目标。")]
		public MMFlash TargetFlash;

        /// <summary>
        /// 在播放时，我们触发一个闪烁事件
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
			if (TargetFlash != null)
			{
				TargetFlash.Flash(FlashColor, FlashDuration * intensityMultiplier, FlashAlpha, ComputedTimescaleMode);
			}
			else
			{
				MMFlashEvent.Trigger(FlashColor, FeedbackDuration * intensityMultiplier, FlashAlpha, FlashID, ChannelData, ComputedTimescaleMode);	
			}
		}

        /// <summary>
        /// 在停止时，我们停止我们的过渡动画
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
			MMFlashEvent.Trigger(FlashColor, FeedbackDuration, FlashAlpha, FlashID, ChannelData, ComputedTimescaleMode, stop:true);
		}

        /// <summary>
        /// 在恢复时，我们恢复我们的初始状态
        /// </summary>
        protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			MMFlashEvent.Trigger(FlashColor, FeedbackDuration, FlashAlpha, FlashID, ChannelData, ComputedTimescaleMode, stop:true);
		}

        /// <summary>
        /// 自动尝试向场景中添加MMFlash设置
        /// </summary>
        public override void AutomaticShakerSetup()
		{
			if (GameObject.FindObjectOfType<MMFlash>() != null)
			{
				return;
			}
			
			(Canvas canvas, bool createdNewCanvas) = Owner.gameObject.MMFindOrCreateObjectOfType<Canvas>("FlashCanvas", null);
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			(Image image, bool createdNewImage) = canvas.gameObject.MMFindOrCreateObjectOfType<Image>("FlashImage", canvas.transform, true);
			image.raycastTarget = false;
			image.color = Color.white;
			
			RectTransform rectTransform = image.GetComponent<RectTransform>();
			rectTransform.anchorMin = new Vector2(0f, 0f);
			rectTransform.anchorMax = new Vector2(1f, 1f);
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;
			
			image.gameObject.AddComponent<MMFlash>();
			image.gameObject.GetComponent<CanvasGroup>().alpha = 0;
			image.gameObject.GetComponent<CanvasGroup>().interactable = false;

			MMDebug.DebugLogInfo("已向场景中添加了MMFlash。您已全部设置完毕");
		}
	}
}
#endif