using MoreMountains.FeedbacksForThirdParty;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback lets you control a camera's field of view over time. You'll need a MMCameraFieldOfViewShaker on your camera.
	/// </summary>
	[AddComponentMenu("")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Camera/Field of View")]
	[FeedbackHelp(
        "这个反馈允许你随时间控制相机的视野范围。你需要在相机上添加一个MMCameraFieldOfViewShaker")]
	public class MMF_CameraFieldOfView : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用所有此类反馈
        public static bool FeedbackTypeAuthorized = true;

        /// 设置此反馈的检查器颜色
#if UNITY_EDITOR
        public override Color FeedbackColor => MMFeedbacksInspectorColors.CameraColor; 
		public override string RequiredTargetText => RequiredChannelText;
		public override bool HasCustomInspectors => true; 
		public override bool HasAutomaticShakerSetup => true;
#endif
        /// 返回反馈的持续时间
        public override float FeedbackDuration
		{
			get { return ApplyTimeMultiplier(Duration); }
			set { Duration = value; }
		}

		public override bool HasChannel => true;
		public override bool CanForceInitialValue => true;
		public override bool ForceInitialValueDelayed => true;
		public override bool HasRandomness => true;

		[MMFInspectorGroup("Field of View", true, 37)]
		/// the duration of the shake, in seconds
		[Tooltip("震动的持续时间（以秒为单位）")]
		public float Duration = 2f;

		/// whether or not to reset shaker values after shake
		[Tooltip("是否在震动后重置震动器值")]
		public bool ResetShakerValuesAfterShake = true;

		/// whether or not to reset the target's values after shake
		[Tooltip("是否在震动后重置目标的值")]
		public bool ResetTargetValuesAfterShake = true;

		/// whether or not to add to the initial value
		[Tooltip("是否添加到初始值")]
		public bool RelativeFieldOfView = false;

		/// the curve used to animate the intensity value on
		[Tooltip("用于动画化强度值的曲线")]
		public AnimationCurve ShakeFieldOfView =
			new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));

		/// the value to remap the curve's 0 to
		[Tooltip("将曲线的0值重新映射到的值")] [Range(0f, 179f)]
		public float RemapFieldOfViewZero = 60f;

		/// the value to remap the curve's 1 to
		[Tooltip("将曲线的1值重新映射到的值")] [Range(0f, 179f)]
		public float RemapFieldOfViewOne = 120f;

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
			MMCameraFieldOfViewShakeEvent.Trigger(ShakeFieldOfView, FeedbackDuration, RemapFieldOfViewZero,
				RemapFieldOfViewOne, RelativeFieldOfView,
				intensityMultiplier, ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake,
				NormalPlayDirection, ComputedTimescaleMode);
		}

        /// <summary>
        /// 在停止时，我们停止过渡
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
			MMCameraFieldOfViewShakeEvent.Trigger(ShakeFieldOfView, FeedbackDuration, RemapFieldOfViewZero,
				RemapFieldOfViewOne, stop: true);
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

			MMCameraFieldOfViewShakeEvent.Trigger(ShakeFieldOfView, FeedbackDuration, RemapFieldOfViewZero,
				RemapFieldOfViewOne, restore: true);
		}

        /// <summary>
        /// 如果没有MMCameraFieldOfViewShaker，则自动尝试向主相机添加一个
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
			
			MMCameraFieldOfViewShaker fieldOfViewShaker = (MMCameraFieldOfViewShaker)Object.FindObjectOfType(typeof(MMCameraFieldOfViewShaker));
			if (fieldOfViewShaker != null)
			{
				return;
			}

			Camera.main.gameObject.MMGetOrAddComponent<MMCameraFieldOfViewShaker>(); 
			MMDebug.DebugLogInfo("已向主相机添加MMCameraFieldOfViewShaker。一切就绪");
		}
	}
}