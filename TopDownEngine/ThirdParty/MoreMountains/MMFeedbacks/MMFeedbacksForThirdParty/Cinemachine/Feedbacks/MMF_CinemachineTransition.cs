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
	/// <summary>
	/// This feedback will let you change the priorities of your cameras. 
	/// It requires a bit of setup : adding a MMCinemachinePriorityListener to your different cameras, with unique Channel values on them.
	/// Optionally, you can add a MMCinemachinePriorityBrainListener on your Cinemachine Brain to handle different transition types and durations.
	/// Then all you have to do is pick a channel and a new priority on your feedback, and play it. Magic transition!
	/// </summary>
	[AddComponentMenu("")]
	#if MM_CINEMACHINE || MM_CINEMACHINE3
	[FeedbackPath("Camera/Cinemachine Transition")]
	#endif
	[MovedFrom(false, null, "MoreMountains.Feedbacks.Cinemachine")]
	[FeedbackHelp("此反馈将使你能够更改相机的优先级。这需要进行一些设置：  " +
                  "在你不同的相机上添加一个MMCinemachine优先级监听器，并为它们设置唯一的通道值。  " +
                  "可选地，你可以在虚拟相机（Cinemachine）的大脑组件（Cinemachine Brain）上添加一个MM虚拟相机（MMCinemachine）优先级大脑监听器，以便处理不同的过渡类型和持续时间。  " +
                  "然后，你所需要做的就是在反馈中选择一个通道和一个新的优先级，然后播放它。神奇的过渡效果就出现啦！ ")]
	public class MMF_CinemachineTransition : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用所有此类反馈。
        public static bool FeedbackTypeAuthorized = true;
		public enum Modes { Event, Binding }

        /// 设置此反馈在检视面板中的颜色。
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
		public override string RequiredTargetText => RequiredChannelText;
#endif
#if MM_CINEMACHINE
		/// 此反馈的持续时间就是抖动的持续时间。
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(BlendDefintion.m_Time); } set { BlendDefintion.m_Time = value; } }
#elif MM_CINEMACHINE3
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(BlendDefintion.Time); } set { BlendDefintion.Time = value; } }
		#endif
		#if MM_CINEMACHINE || MM_CINEMACHINE3
		public override bool HasAutomatedTargetAcquisition => true;
		#endif
		#if MM_CINEMACHINE
		protected override void AutomateTargetAcquisition() => TargetVirtualCamera = FindAutomatedTarget<CinemachineVirtualCamera>();
		#elif MM_CINEMACHINE3
		protected override void AutomateTargetAcquisition() => TargetCinemachineCamera = FindAutomatedTarget<CinemachineCamera>();
		#endif
		public override bool HasChannel => true;

		[MMFInspectorGroup("Cinemachine Transition", true, 52)]
		/// the selected mode (either via event, or via direct binding of a specific camera)
		[Tooltip("所选模式（可以通过事件来选择，也可以通过直接绑定特定相机来选择） ")]
		public Modes Mode = Modes.Event;
#if MM_CINEMACHINE
		/// the virtual camera to target
		[Tooltip("要瞄准的虚拟相机（Cinemachine）相机 ")]
		[MMFEnumCondition("Mode", (int)Modes.Binding)]
		public CinemachineVirtualCamera TargetVirtualCamera;
#elif MM_CINEMACHINE3
        /// the Cinemachine camera to target
        [Tooltip("要瞄准的虚拟相机（Cinemachine）相机 ")]
		[MMFEnumCondition("Mode", (int)Modes.Binding)]
		public CinemachineCamera TargetCinemachineCamera;
		#endif
		/// whether or not to reset the target's values after shake
		[Tooltip("抖动之后是否重置目标的各项数值")]
		public bool ResetValuesAfterTransition = true;

		[Header("Priority优先级")]
		/// the new priority to apply to all virtual cameras on the specified channel
		[Tooltip("要应用于指定通道上所有虚拟相机的新优先级")]
		public int NewPriority = 10;
		/// whether or not to force all virtual cameras on other channels to reset their priority to zero
		[Tooltip("是否强制其他通道上的所有虚拟相机将其优先级重置为零 ")]
		public bool ForceMaxPriority = true;
		/// whether or not to apply a new blend
		[Tooltip("是否应用一种新的混合效果")]
		public bool ForceTransition = false;
		#if MM_CINEMACHINE || MM_CINEMACHINE3
		/// the new blend definition to apply
		[Tooltip("要应用的新混合定义")]
		[MMFCondition("ForceTransition", true)]
		public CinemachineBlendDefinition BlendDefintion;

		protected CinemachineBlendDefinition _tempBlend;
#endif

        /// <summary>
        /// 在处于监听状态的虚拟相机上触发优先级更改 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			_tempBlend = BlendDefintion;
			#endif
			#if MM_CINEMACHINE
			_tempBlend.m_Time = FeedbackDuration;
			#elif MM_CINEMACHINE3
			_tempBlend.Time = FeedbackDuration;
			#endif
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			if (Mode == Modes.Event)
			{
				MMCinemachinePriorityEvent.Trigger(ChannelData, ForceMaxPriority, NewPriority, ForceTransition, _tempBlend, ResetValuesAfterTransition, ComputedTimescaleMode);    
			}
			else
			{
				MMCinemachinePriorityEvent.Trigger(ChannelData, ForceMaxPriority, 0, ForceTransition, _tempBlend, ResetValuesAfterTransition, ComputedTimescaleMode); 
				SetPriority(NewPriority);
			}
			#endif
		}
		
		protected virtual void SetPriority(int newPriority)
		{
			#if MM_CINEMACHINE 
			TargetVirtualCamera.Priority = newPriority;
			#elif MM_CINEMACHINE3
			PrioritySettings prioritySettings = TargetCinemachineCamera.Priority;
			prioritySettings.Value = newPriority;
			TargetCinemachineCamera.Priority = prioritySettings;
			#endif
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
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			MMCinemachinePriorityEvent.Trigger(ChannelData, ForceMaxPriority, 0, ForceTransition, _tempBlend, ResetValuesAfterTransition, ComputedTimescaleMode, true); 
			#endif
		}
	}
}