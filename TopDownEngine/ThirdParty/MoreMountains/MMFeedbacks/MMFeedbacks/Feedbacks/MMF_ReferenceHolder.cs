using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback allows you to hold a reference, that can then be used by other feedbacks to automatically set their target.
	/// It doesn't do anything when played. 
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈允许您持有一个参考，然后其他反馈可使用它来自动设置它们的目标。在播放时，它不做任何事情")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Feedbacks/MMF Reference Holder")]
	public class MMF_ReferenceHolder : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用所有这种类型的反馈
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色。
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.FeedbacksColor; } }
		public override string RequiredTargetText => GameObjectReference != null ? GameObjectReference.name : "";
#endif
        /// 此反馈的持续时间为 0
        public override float FeedbackDuration => 0f;
		public override bool DisplayFullHeaderColor => true;

		[MMFInspectorGroup("References", true, 37, true)]
		/// the game object to set as the target (or on which to look for a specific component as a target) of all feedbacks that may look at this reference holder for a target
		[Tooltip("要设置为所有反馈的目标（或在其中查找特定组件作为目标）的游戏对象，这些反馈可能会查看此参考持有者以获取目标")] 
		public GameObject GameObjectReference;
		/// whether or not to force this reference holder on all compatible feedbacks in the MMF Player's list
		[Tooltip("是否强制此参考持有者应用于 MMF Player 列表中的所有兼容反馈")] 
		public bool ForceReferenceOnAll = false;

        /// <summary>
        /// 在初始化时，如果需要，我们会在所有反馈上强制使用我们的参考
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if (ForceReferenceOnAll)
			{
				for (int index = 0; index < Owner.FeedbacksList.Count; index++)
				{
					if (Owner.FeedbacksList[index].HasAutomatedTargetAcquisition)
					{
						Owner.FeedbacksList[index].SetIndexInFeedbacksList(index);
						Owner.FeedbacksList[index].ForcedReferenceHolder = this;
						Owner.FeedbacksList[index].ForceAutomateTargetAcquisition();
					}
				}
			}
		}

        /// <summary>
        /// 在播放时，我们什么都不做
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			return;
		}
	}
}