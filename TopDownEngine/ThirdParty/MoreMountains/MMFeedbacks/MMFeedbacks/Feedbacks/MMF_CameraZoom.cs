using MoreMountains.FeedbacksForThirdParty;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// A feedback that will allow you to change the zoom of a (3D) camera when played
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("定义缩放属性：将在一定持续时间内将缩放设置为指定的参数， " +
                  "设置将使它们永远保持那样。缩放属性包括视场角、缩放过渡的持续时间（以秒为单位）， " +
                  "以及缩放持续时间（相机应保持缩放状态的时间，以秒为单位）。 " +
                  "要使此功能正常工作，你需要在相机上添加一个 MMCameraZoom 组件，" +
                  "或者在使用虚拟相机的情况下添加 MMCinemachineZoom。")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Camera/Camera Zoom")]
	public class MMF_CameraZoom : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用所有此类反馈
        public static bool FeedbackTypeAuthorized = true;
        /// 返回反馈的持续时间
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
		public override string RequiredTargetText => RequiredChannelText;
		public override bool HasCustomInspectors => true; 
		public override bool HasAutomaticShakerSetup => true;
#endif

        /// 此反馈的持续时间就是缩放的持续时间
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(ZoomDuration); } set { ZoomDuration = value; } }
		public override bool HasChannel => true;
		public override bool CanForceInitialValue => true;

		[MMFInspectorGroup("Camera Zoom", true, 72)]
		/// the zoom mode (for : forward for TransitionDuration, static for Duration, backwards for TransitionDuration)
		[Tooltip("缩放模式（向前为过渡持续时间，静止为持续时间，向后为过渡持续时间）")]
		public MMCameraZoomModes ZoomMode = MMCameraZoomModes.For;
		/// the target field of view
		[Tooltip("目标视野范围")]
		public float ZoomFieldOfView = 30f;
		/// the zoom transition duration
		[Tooltip("缩放过渡持续时间")]
		public float ZoomTransitionDuration = 0.05f;
		/// the duration for which the zoom is at max zoom
		[Tooltip("缩放达到最大缩放状态的持续时间")]
		public float ZoomDuration = 0.1f;
		/// whether or not ZoomFieldOfView should add itself to the current camera's field of view value
		[Tooltip("缩放视野范围是否应将其值添加到当前摄像机的视野范围值上")]
		public bool RelativeFieldOfView = false;
		[Header("Transition Speed过渡速度")]
		/// the animation curve to apply to the zoom transition
		[Tooltip("应用于缩放过渡的动画曲线")]
		public MMTweenType ZoomTween = new MMTweenType( new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f)));

        /// <summary>
        /// 在播放时，触发一个缩放事件
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			MMCameraZoomEvent.Trigger(ZoomMode, ZoomFieldOfView, ZoomTransitionDuration, FeedbackDuration, ChannelData, 
				ComputedTimescaleMode == TimescaleModes.Unscaled, false, RelativeFieldOfView, tweenType: ZoomTween);
		}

        /// <summary>
        /// 停止时，我们停止过渡
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
			MMCameraZoomEvent.Trigger(ZoomMode, ZoomFieldOfView, ZoomTransitionDuration, FeedbackDuration, ChannelData, 
				ComputedTimescaleMode == TimescaleModes.Unscaled, stop:true, tweenType: ZoomTween);
		}

        /// <summary>
        /// 在恢复时，我们恢复初始状态。
        /// </summary>
        protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			MMCameraZoomEvent.Trigger(ZoomMode, ZoomFieldOfView, ZoomTransitionDuration, FeedbackDuration, ChannelData, 
				ComputedTimescaleMode == TimescaleModes.Unscaled, restore:true, tweenType: ZoomTween);
		}

        /// <summary>
        /// 如果不存在，则自动尝试为主相机添加 MMCameraZoom。
        /// </summary>
        public override void AutomaticShakerSetup()
		{
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			bool virtualCameraFound = false;
			#endif
			
			#if MMCINEMACHINE 
				CinemachineVirtualCamera virtualCamera = (CinemachineVirtualCamera)Object.FindObjectOfType(typeof(CinemachineVirtualCamera));
				virtualCameraFound = (virtualCamera != null);
			#elif MMCINEMACHINE3
				CinemachineCamera virtualCamera = (CinemachineCamera)Object.FindObjectOfType(typeof(CinemachineCamera));
				virtualCameraFound = (virtualCamera != null);
			#endif
			
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			if (virtualCameraFound)
			{
				MMCinemachineHelpers.AutomaticCinemachineShakersSetup(Owner, "CinemachineImpulse");
				return;
			}
			#endif
			
			MMCameraZoom camZoom = (MMCameraZoom)Object.FindObjectOfType(typeof(MMCameraZoom));
			if (camZoom != null)
			{
				return;
			}

			Camera.main.gameObject.MMGetOrAddComponent<MMCameraZoom>(); 
			MMDebug.DebugLogInfo("已为主相机添加了 MMCameraZoom。一切准备就绪");
		}
	}
}