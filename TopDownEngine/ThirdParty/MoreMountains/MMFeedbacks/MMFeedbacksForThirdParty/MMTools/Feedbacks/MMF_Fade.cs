#if MM_UI
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UI;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// 此反馈将触发对目标浮点控制器进行一次性播放操作。 
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback lets you trigger a fade event.")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.MMTools")]
	[FeedbackPath("Camera/Fade")]
	public class MMF_Fade : MMF_Feedback
	{
		/// 一个用于一次性禁用所有此类反馈的静态布尔值。
		public static bool FeedbackTypeAuthorized = true;
		/// 设置此反馈在检查器中的颜色。 
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
		public override string RequiredTargetText { get { return "ID "+ID;  } }
		public override bool HasCustomInspectors => true;
		public override bool HasAutomaticShakerSetup => true;
		#endif
		/// 不同的、可能的渐变类型 
		public enum FadeTypes { FadeIn, FadeOut, Custom }
		/// 将位置发送到渐变器的不同方式： 
		/// - FeedbackPosition反馈位置：在反馈的位置进行渐变，另外还可添加一个可选的偏移量。 
		/// - Transform变换：在指定的变换（Transform）的位置进行渐变，另外还可添加一个可选的偏移量。 
		/// - WorldPosition世界位置：在指定的世界位置向量处进行渐变，另外还可添加一个可选的偏移量。 
		/// - Script脚本：调用该反馈时在参数中传入的位置。 
		public enum PositionModes { FeedbackPosition, Transform, WorldPosition, Script }

		[MMFInspectorGroup("Fade", true, 43)]
		/// the type of fade we want to use when this feedback gets played
		[Tooltip("当这个反馈被播放时，我们想要使用的渐变类型。 ")]
		public FadeTypes FadeType;
		/// the ID of the fader(s) to pilot
		[Tooltip("要控制的渐变器（一个或多个）的 ID ")]
		public int ID = 0;
		/// the duration (in seconds) of the fade
		[Tooltip("渐变的持续时间（以秒为单位）")]
		public float Duration = 1f;
		/// the curve to use for this fade
		[Tooltip("用于此次渐变的曲线")]
		public MMTweenType Curve = new MMTweenType(MMTween.MMTweenCurve.EaseInCubic);
		/// whether or not this fade should ignore timescale
		[Tooltip("此次渐变是否应该忽略时间比例。 ")]
		public bool IgnoreTimeScale = true;

		[Header("Custom自定义")]
		/// the target alpha we're aiming for with this fade
		[Tooltip("我们通过这次渐变所期望达到的目标透明度（alpha值）。 ")]
		public float TargetAlpha;

		[Header("Position位置")]
		/// the chosen way to position the fade 
		[Tooltip("所选择的定位渐变的方式")]
		public PositionModes PositionMode = PositionModes.FeedbackPosition;
		/// the transform on which to center the fade
		[Tooltip("用于将渐变以其为中心的那个变换（对象） ")]
		[MMFEnumCondition("PositionMode", (int)PositionModes.Transform)]
		public Transform TargetTransform;
		/// the coordinates on which to center the fadet
		[Tooltip("进行渐变时作为中心的坐标 ")]
		[MMFEnumCondition("PositionMode", (int)PositionModes.WorldPosition)]
		public Vector3 TargetPosition;
		/// the position offset to apply when centering the fade
		[Tooltip("在将渐变居中时要应用的位置偏移量 ")]
		public Vector3 PositionOffset;

		[Header("Optional Target可选目标")] 
		/// this field lets you bind a specific MMFader to this feedback. If left empty, the feedback will trigger a MMFadeEvent instead, targeting all matching faders. If you fill it, only that specific fader will be targeted.
		[Tooltip("此字段允许您将特定的MMFader（多模式渐变器）绑定到这个反馈上。如果此字段留空，该反馈将改为触发一个MMFadeEvent（多模式渐变事件），目标是所有匹配的渐变器。如果您填写了此字段，那么只有那个特定的渐变器会成为目标。 ")]
		public MMFader TargetFader;

		/// 此反馈的持续时间就是渐变的持续时间。 
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value;  } }

		protected Vector3 _position;
		protected FadeTypes _fadeType;

		/// <summary>
		/// 在播放时，我们会触发所选的渐变事件。 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			_position = GetPosition(position);
			_fadeType = FadeType;
			if (!NormalPlayDirection)
			{
				if (FadeType == FadeTypes.FadeIn)
				{
					_fadeType = FadeTypes.FadeOut;
				}
				else if (FadeType == FadeTypes.FadeOut)
				{
					_fadeType = FadeTypes.FadeIn;
				}
			}

			if (TargetFader != null)
			{
				switch (_fadeType)
				{
					case FadeTypes.Custom:
						TargetFader.Fade(TargetAlpha, FeedbackDuration, Curve, IgnoreTimeScale);
						break;
					case FadeTypes.FadeIn:
						TargetFader.FadeIn(FeedbackDuration, Curve, IgnoreTimeScale);
						break;
					case FadeTypes.FadeOut:
						TargetFader.FadeOut(FeedbackDuration, Curve, IgnoreTimeScale);
						break;
				}
			}
			else
			{
				switch (_fadeType)
				{
					case FadeTypes.Custom:
						MMFadeEvent.Trigger(FeedbackDuration, TargetAlpha, Curve, ID, IgnoreTimeScale, _position);
						break;
					case FadeTypes.FadeIn:
						MMFadeInEvent.Trigger(FeedbackDuration, Curve, ID, IgnoreTimeScale, _position);
						break;
					case FadeTypes.FadeOut:
						MMFadeOutEvent.Trigger(FeedbackDuration, Curve, ID, IgnoreTimeScale, _position);
						break;
				}
			}
		}

		/// <summary>
		/// 如果有需要，就停止该动画。 
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
			MMFadeStopEvent.Trigger(ID);
		}

		/// <summary>
		/// 计算出此次渐变的合适位置。 
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		protected virtual Vector3 GetPosition(Vector3 position)
		{
			switch (PositionMode)
			{
				case PositionModes.FeedbackPosition:
					return Owner.transform.position + PositionOffset;
				case PositionModes.Transform:
					return TargetTransform.position + PositionOffset;
				case PositionModes.WorldPosition:
					return TargetPosition + PositionOffset;
				case PositionModes.Script:
					return position + PositionOffset;
				default:
					return position + PositionOffset;
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
			MMFadeStopEvent.Trigger(ID, true);
		}
		
		/// <summary>
		/// 自动尝试将一个MMFader（多模式渐变器）设置添加到场景中。 
		/// </summary>
		public override void AutomaticShakerSetup()
		{
			if (GameObject.FindObjectOfType<MMFader>() != null)
			{
				return;
			}
			
			(Canvas canvas, bool createdNewCanvas) = Owner.gameObject.MMFindOrCreateObjectOfType<Canvas>("FadeCanvas", null);
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			(Image image, bool createdNewImage) = canvas.gameObject.MMFindOrCreateObjectOfType<Image>("FadeImage", canvas.transform, true);
			image.raycastTarget = false;
			image.color = Color.black;
			
			RectTransform rectTransform = image.GetComponent<RectTransform>();
			rectTransform.anchorMin = new Vector2(0f, 0f);
			rectTransform.anchorMax = new Vector2(1f, 1f);
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;
			
			image.gameObject.AddComponent<MMFader>();
			image.gameObject.GetComponent<CanvasGroup>().alpha = 0;
			image.gameObject.GetComponent<CanvasGroup>().interactable = false;

			MMDebug.DebugLogInfo("已将一个MMFader（多模式渐变器）添加到场景中。一切已准备就绪。 ");
		}
	}
}
#endif