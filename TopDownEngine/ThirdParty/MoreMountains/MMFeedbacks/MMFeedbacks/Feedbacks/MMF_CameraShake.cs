using MoreMountains.FeedbacksForThirdParty;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will send a shake event when played
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("定义相机抖动属性（持续时间（以秒为单位）、振幅和频率），然后这将广播一个带有相同设置的 MMCameraShakeEvent" +
                  "要使此功能正常工作，你需要在相机上添加一个 MMCameraShaker（或者在使用 Cinemachine 的情况下，在虚拟相机上添加一个 MMCinemachineCameraShaker 组件）" +
                  "请注意，尽管此事件和系统是为相机而构建的，但从技术上讲，你也可以用它来抖动其他对象")]
	[FeedbackPath("Camera/Camera Shake")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	public class MMF_CameraShake : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用所有此类反馈
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
		public override string RequiredTargetText => RequiredChannelText;
		public override bool HasCustomInspectors => true;
		public override bool HasAutomaticShakerSetup => true;
#endif

        /// 此反馈的持续时间就是抖动的持续时间。
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(CameraShakeProperties.Duration); } set { CameraShakeProperties.Duration = value; } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;

		[MMFInspectorGroup("Camera Shake", true, 57)]
		/// whether or not this shake should repeat forever, until stopped
		[Tooltip("此抖动是否应永远重复，直到停止")]
		public bool RepeatUntilStopped = false;
		/// the properties of the shake (duration, intensity, frequenc)
		[Tooltip("抖动的属性（持续时间、强度、频率）")]
		public MMCameraShakeProperties CameraShakeProperties = new MMCameraShakeProperties(0.1f, 0.2f, 40f);

        /// <summary>
        /// 在播放时，发送一个抖动相机事件
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
			MMCameraShakeEvent.Trigger(FeedbackDuration, CameraShakeProperties.Amplitude * intensityMultiplier, CameraShakeProperties.Frequency, 
				CameraShakeProperties.AmplitudeX * intensityMultiplier, CameraShakeProperties.AmplitudeY * intensityMultiplier, CameraShakeProperties.AmplitudeZ * intensityMultiplier,
				RepeatUntilStopped, ChannelData, ComputedTimescaleMode == TimescaleModes.Unscaled);
		}

		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);
			MMCameraShakeStopEvent.Trigger(ChannelData);
		}

        /// <summary>
        /// 如果没有相机装备，则自动尝试添加一个
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
			
			MMCameraShaker camShaker = (MMCameraShaker)Object.FindObjectOfType(typeof(MMCameraShaker));
			if (camShaker != null)
			{
				return;
			}
			
			GameObject cameraRig = new GameObject("CameraRig");
			cameraRig.transform.position = Camera.main.transform.position;
			GameObject cameraShaker = new GameObject("CameraShaker");
			cameraShaker.transform.SetParent(cameraRig.transform);
			cameraShaker.transform.localPosition = Vector3.zero;
			cameraShaker.AddComponent<MMCameraShaker>();
			MMWiggle wiggle = cameraShaker.GetComponent<MMWiggle>(); 
			wiggle.PositionActive = true;
			wiggle.PositionWiggleProperties = new WiggleProperties();
			wiggle.PositionWiggleProperties.WigglePermitted = false;
			wiggle.PositionWiggleProperties.WiggleType = WiggleTypes.Noise; 
			Camera.main.transform.SetParent(cameraShaker.transform);
			
			MMDebug.DebugLogInfo("已为主相机添加了 CameraRig。一切准备就绪"); 
		}
	}
}