using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback allows you to trigger a target MMFeedbacks, or any MMFeedbacks on the specified Channel within a certain range. You'll need an MMFeedbacksShaker on them.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈允许你触发目标 MMFeedback，或在指定通道范围内的任何 MMFeedback。你需要在这些反馈上放置一个 MMFeedbacksShaker")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Feedbacks/Feedbacks Player")]
	public class MMF_Feedbacks : MMF_Feedback
	{
        ///  一个静态布尔值用于一次性禁用此类型的所有反馈
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.FeedbacksColor; } }
		public override string RequiredTargetText => RequiredChannelText;
#endif
        /// 此反馈的持续时间是我们目标反馈的持续时间
        public override float FeedbackDuration 
		{
			get
			{
				if (TargetFeedbacks == Owner)
				{
					return 0f;
				}
				if ((Mode == Modes.PlayTargetFeedbacks) && (TargetFeedbacks != null))
				{
					return TargetFeedbacks.TotalDuration;
				}
				else
				{
					return 0f;    
				}
			} 
		}
		public override bool HasChannel => true;
        
		public enum Modes { PlayFeedbacksInArea, PlayTargetFeedbacks }
        
		[MMFInspectorGroup("Feedbacks", true, 79)]
        
		/// the selected mode for this feedback
		[Tooltip("所选的反馈模式")]
		public Modes Mode = Modes.PlayFeedbacksInArea;
        
		/// a specific MMFeedbacks / MMF_Player to play
		[MMFEnumCondition("Mode", (int)Modes.PlayTargetFeedbacks)]
		[Tooltip("特定的MM反馈/MMF_Player来播放")]
		public MMFeedbacks TargetFeedbacks;
        
		/// whether or not to use a range
		[MMFEnumCondition("Mode", (int)Modes.PlayFeedbacksInArea)]
		[Tooltip("是否使用一个范围")]
		public bool OnlyTriggerPlayersInRange = false;
		/// the range of the event, in units
		[MMFEnumCondition("Mode", (int)Modes.PlayFeedbacksInArea)]
		[Tooltip("该事件的范围（以单位计）")]
		public float EventRange = 100f;
		/// the transform to use to broadcast the event as origin point
		[MMFEnumCondition("Mode", (int)Modes.PlayFeedbacksInArea)]
		[Tooltip("用于将该事件作为原点进行广播的变换")]
		public Transform EventOriginTransform;

        /// <summary>
        /// 在初始化时，如果需要的话，我们会关闭灯光
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
            
			if (EventOriginTransform == null)
			{
				EventOriginTransform = owner.transform;
			}
		}

        /// <summary>
        /// 在播放时，我们会触发我们的目标反馈或触发一个反馈震动事件，以使该区域的反馈产生震动
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (TargetFeedbacks == Owner)
			{
				return;
			}
			
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (Mode == Modes.PlayFeedbacksInArea)
			{
				MMFeedbacksShakeEvent.Trigger(ChannelData, OnlyTriggerPlayersInRange, EventRange, EventOriginTransform.position);    
			}
			else if (Mode == Modes.PlayTargetFeedbacks)
			{
				TargetFeedbacks?.PlayFeedbacks(position, feedbacksIntensity);
			}
		}
	}
}