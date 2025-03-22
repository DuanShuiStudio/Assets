using UnityEngine;
using MoreMountains.Feedbacks;
#if MM_CINEMACHINE
using Cinemachine;
#elif MM_CINEMACHINE3
using Unity.Cinemachine;
#endif
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.FeedbacksForThirdParty
{
	[AddComponentMenu("")]
	#if MM_CINEMACHINE || MM_CINEMACHINE3
	[FeedbackPath("Camera/Cinemachine Impulse Clear")]
	#endif
	[MovedFrom(false, null, "MoreMountains.Feedbacks.Cinemachine")]
	[FeedbackHelp("此反馈可让你触发虚拟相机（Cinemachine）脉冲清除操作，能立即停止任何可能正在播放的脉冲效果。 ")]
	public class MMF_CinemachineImpulseClear : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用所有此类反馈。
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈在检视面板中的颜色。
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
		#endif

		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			CinemachineImpulseManager.Instance.Clear();
			#endif
		}
	}
}