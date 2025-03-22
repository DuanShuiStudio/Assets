using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 一种用于改变变换（Transform）父对象的反馈机制。
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("此反馈信息可让你更改变换（组件）的父级")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Transform/Set Parent")]
	public class MMF_SetParent : MMF_Feedback 
	{
        /// 一个静态布尔值，用于一次性禁用此类所有反馈。
        public static bool FeedbackTypeAuthorized = true;

        /// 设置此反馈的检查器颜色。
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
		public override bool EvaluateRequiresSetup() { return (ObjectToParent == null); }
		public override string RequiredTargetText { get { return ObjectToParent != null ? ObjectToParent.name : "";  } }
		public override string RequiresSetupText { get { return "此反馈需要一个 “待设父级对象（ObjectToParent）”，该对象将被重新设置父级为 “新父级（NewParent）”。"; } } 
		#endif
		
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => ObjectToParent = FindAutomatedTarget<Transform>(); 

		[MMFInspectorGroup("Parenting", true, 12, true)]
		/// the object we want to change the parent of
		[Tooltip("我们想要更改其父级的那个对象")]
		public Transform ObjectToParent;
		/// the object ObjectToParent should now be parented to after playing this feedback
		[Tooltip("在播放这条反馈信息之后，对象 “ObjectToParent” 现在应该已被设置为（某个对象的）子级（即拥有了新的父级）。")]
		public Transform NewParent;
		/// if true, the parent-relative position, scale and rotation are modified such that the object keeps the same world space position, rotation and scale as before
		[Tooltip("如果为真，则会修改该对象相对于父对象的位置、缩放和旋转，以便让该对象在世界空间中的位置、旋转和缩放保持与之前一致。")]
		public bool WorldPositionStays = true;

        /// <summary>
        /// 播放时，更改目标变换（组件）的父级。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			if (ObjectToParent == null)
			{
				Debug.LogWarning("No object to parent was set for " + Owner.name);
				return;
			}
			ObjectToParent.SetParent(NewParent, WorldPositionStays);
		}
	}
}