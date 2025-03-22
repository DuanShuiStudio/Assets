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
	[FeedbackPath("Camera/Cinemachine Impulse Source")]
	#endif
	[MovedFrom(false, null, "MoreMountains.Feedbacks.Cinemachine")]
	[FeedbackHelp("此反馈可让你在虚拟相机（Cinemachine）脉冲源上生成一个脉冲。要使此功能生效，你需要在相机上添加一个虚拟相机（Cinemachine）脉冲监听器。 ")]
	public class MMF_CinemachineImpulseSource : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用所有此类反馈。
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈在检视面板中的颜色。
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
			#if MM_CINEMACHINE || MM_CINEMACHINE3
				public override bool EvaluateRequiresSetup() { return (ImpulseSource == null); }
				public override string RequiredTargetText { get { return ImpulseSource != null ? ImpulseSource.name : "";  } }
			#endif
			public override string RequiresSetupText { get { return "此反馈要求设置一个脉冲源才能正常工作。你可以在下方设置一个。 "; } }
		#endif
		

		[MMFInspectorGroup("Cinemachine Impulse Source", true, 28)]

		/// the velocity to apply to the impulse shake
		[Tooltip("要应用于脉冲抖动的速度。")]
		public Vector3 Velocity = new Vector3(1f,1f,1f);
		#if MM_CINEMACHINE || MM_CINEMACHINE3
			/// the impulse definition to broadcast
			[Tooltip("要广播的脉冲定义")]
			public CinemachineImpulseSource ImpulseSource;
			
			public override bool HasAutomatedTargetAcquisition => true;
			protected override void AutomateTargetAcquisition() => ImpulseSource = FindAutomatedTarget<CinemachineImpulseSource>();
		#endif
		/// whether or not to clear impulses (stopping camera shakes) when the Stop method is called on that feedback
		[Tooltip("当对该反馈调用“停止”方法时，是否清除脉冲（停止相机抖动）。 ")]
		public bool ClearImpulseOnStop = false;
        
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			#if MM_CINEMACHINE || MM_CINEMACHINE3
			if (ImpulseSource != null)
			{
				ImpulseSource.GenerateImpulse(Velocity);
			}
			#endif
		}

        /// <summary>
        /// 如有需要，停止该动画。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized || !ClearImpulseOnStop)
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);
            
			#if MM_CINEMACHINE || MM_CINEMACHINE3
				CinemachineImpulseManager.Instance.Clear();
			#endif
		}

        /// <summary>
        /// 在恢复时，我们将对象放回其初始位置。 
        /// </summary>
        protected override void CustomRestoreInitialValues()
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