using System.Collections;
using System.Collections.Generic;
using MoreMountains.FeedbacksForThirdParty;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback lets you control a camera's orthographic size over time. You'll need a MMCameraOrthographicSizeShaker on your camera.
	/// </summary>
	[AddComponentMenu("")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Camera/Orthographic Size")]
	[FeedbackHelp("这个反馈让你能够控制相机的正交大小随时间变化。你需要在相机上添加一个 MMCameraOrthographicSizeShaker。")]
	public class MMF_CameraOrthographicSize : MMF_Feedback
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
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }
		public override bool HasChannel => true;

		[MMFInspectorGroup("Orthographic Size", true, 41)]
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
		public bool RelativeOrthographicSize = false;
		/// the curve used to animate the intensity value on
		[Tooltip("用于动画化强度值的曲线")]
		public AnimationCurve ShakeOrthographicSize = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("将曲线的0值重新映射到的值")]
		public float RemapOrthographicSizeZero = 5f;
		/// the value to remap the curve's 1 to
		[Tooltip("将曲线的1值重新映射到的值")]
		public float RemapOrthographicSizeOne = 10f;

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
			MMCameraOrthographicSizeShakeEvent.Trigger(ShakeOrthographicSize, FeedbackDuration, RemapOrthographicSizeZero, RemapOrthographicSizeOne, RelativeOrthographicSize,
				feedbacksIntensity, ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, ComputedTimescaleMode);
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
			MMCameraOrthographicSizeShakeEvent.Trigger(ShakeOrthographicSize, FeedbackDuration,
				RemapOrthographicSizeZero, RemapOrthographicSizeOne, stop: true);
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
			MMCameraOrthographicSizeShakeEvent.Trigger(ShakeOrthographicSize, FeedbackDuration,
				RemapOrthographicSizeZero, RemapOrthographicSizeOne, restore: true);
		}

        /// <summary>
        ///如果没有MMCameraFieldOfViewShaker，则自动尝试向主相机添加一个
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
			
			MMCameraOrthographicSizeShaker orthographicSizeShaker = (MMCameraOrthographicSizeShaker)Object.FindObjectOfType(typeof(MMCameraOrthographicSizeShaker));
			if (orthographicSizeShaker != null)
			{
				return;
			}

			Camera.main.gameObject.MMGetOrAddComponent<MMCameraOrthographicSizeShaker>();

            MMDebug.DebugLogInfo("已为主相机添加了 MMCameraOrthographicSizeShaker。一切准备就绪");
		}
	}
}