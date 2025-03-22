using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback allows you to destroy a target gameobject, either via Destroy, DestroyImmediate, or SetActive:False
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈允许你通过 Destroy、DestroyImmediate 或 SetActive:False 来销毁目标游戏对象")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("GameObject/Destroy")]
	public class MMF_Destroy : MMF_Feedback
	{
        /// 一个静态布尔值用于一次性禁用此类型的所有反馈
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetGameObject == null); }
		public override string RequiredTargetText { get { return TargetGameObject != null ? TargetGameObject.name : "";  } }
		public override string RequiresSetupText { get { return "此反馈需要设置一个 TargetGameObject 才能正常工作。你可以在下面设置一个"; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetGameObject = FindAutomatedTargetGameObject();

		/// the possible ways to destroy an object
		public enum Modes { Destroy, DestroyImmediate, Disable }

		[MMFInspectorGroup("Destruction", true, 18, true)]
		/// the gameobject we want to change the active state of
		[Tooltip("我们想要销毁的游戏对象")]
		public GameObject TargetGameObject;
		/// the optional list of extra gameobjects we want to change the active state of
		[Tooltip("我们想要改变其激活状态的额外游戏对象可选列表")]
		public List<GameObject> ExtraTargetGameObjects;
		
		/// the selected destruction mode 
		[Tooltip("所选的销毁模式")]
		public Modes Mode;

		protected bool _initialActiveState;

        /// <summary>
        /// 在播放时，如果需要，我们改变行为的状态
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetGameObject == null))
			{
				return;
			}
			ProceedWithDestruction(TargetGameObject);
			foreach (GameObject go in ExtraTargetGameObjects)
			{
				ProceedWithDestruction(go);
			}
		}

        /// <summary>
        /// 改变行为的状态
        /// </summary>
        /// <param name="state"></param>
        protected virtual void ProceedWithDestruction(GameObject go)
		{
			switch (Mode)
			{
				case Modes.Destroy:
					Owner.ProxyDestroy(go);
					break;
				case Modes.DestroyImmediate:
					Owner.ProxyDestroyImmediate(go);
					break;
				case Modes.Disable:
					_initialActiveState = go.activeInHierarchy;
					go.SetActive(false);
					break;
			}
		}

        /// <summary>
        /// 在恢复时，我们将对象放回其初始位置
        /// </summary>
        protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (Mode == Modes.Disable)
			{
				TargetGameObject.SetActive(_initialActiveState);
			}
		}
	}
}