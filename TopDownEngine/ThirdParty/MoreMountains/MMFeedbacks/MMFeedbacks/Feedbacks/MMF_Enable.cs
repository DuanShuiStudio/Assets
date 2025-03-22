using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 在反馈的各个阶段使对象变为激活或非激活状态。
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("此反馈允许你在初始化、播放、停止或重置时，将目标游戏对象上的行为状态从激活变为非激活（或反之）。" +
                  "对于这些选项中的每一个，你都可以指定是强制设置为某个状态（启用或禁用），还是切换状态（启用变为禁用，禁用变为启用）。")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("GameObject/Enable Behaviour")]
	public class MMF_Enable : MMF_Feedback
	{
        /// 一个静态布尔值用于一次性禁用此类型的所有反馈
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetBehaviour == null); }
		public override string RequiredTargetText { get { return TargetBehaviour != null ? TargetBehaviour.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetBehaviour be set to be able to work properly. You can set one below."; } }
#endif

        /// 反馈可能对目标对象状态产生的影响
        public enum PossibleStates { Enabled, Disabled, Toggle }

		[MMFInspectorGroup("Enable Target Monobehaviour", true, 86, true)]
		/// the gameobject we want to change the active state of
		[Tooltip("我们想要改变其激活状态的游戏对象")]
		public Behaviour TargetBehaviour;
		/// a list of extra gameobjects we want to change the active state of
		[Tooltip("我们想要改变其激活状态的额外游戏对象列表")]
		public List<Behaviour> ExtraTargetBehaviours;
		/// whether or not we should alter the state of the target object on init
		[Tooltip("是否我们应该在初始化时改变目标对象的状态。")]
		public bool SetStateOnInit = false;
		/// how to change the state on init
		[MMFCondition("SetStateOnInit", true)]
		[Tooltip("如何在初始化时改变状态。")]
		public PossibleStates StateOnInit = PossibleStates.Disabled;
		/// whether or not we should alter the state of the target object on play
		[Tooltip("是否我们应该在播放时改变目标对象的状态")]
		public bool SetStateOnPlay = false;
		/// how to change the state on play
		[MMFCondition("SetStateOnPlay", true)]
		[Tooltip("如何在播放时改变状态")]
		public PossibleStates StateOnPlay = PossibleStates.Disabled;
		/// whether or not we should alter the state of the target object on stop
		[Tooltip("是否我们应该在停止时改变目标对象的状态")]
		public bool SetStateOnStop = false;
		/// how to change the state on stop
		[Tooltip("如何在停止时改变状态")]
		[MMFCondition("SetStateOnStop", true)]
		public PossibleStates StateOnStop = PossibleStates.Disabled;
		/// whether or not we should alter the state of the target object on reset
		[Tooltip("是否我们应该在重置时改变目标对象的状态")]
		public bool SetStateOnReset = false;
		/// how to change the state on reset
		[Tooltip("如何在重置时改变状态")]
		[MMFCondition("SetStateOnReset", true)]
		public PossibleStates StateOnReset = PossibleStates.Disabled;

		protected bool _initialState;

        /// <summary>
        /// 在初始化时，如果需要，我们改变行为的状态
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if (Active && (TargetBehaviour != null))
			{
				if (SetStateOnInit)
				{
					SetStatus(StateOnInit);
				}
			}
		}

        /// <summary>
        /// 在播放时，如果需要，我们改变行为的状态
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetBehaviour == null))
			{
				return;
			}
			if (SetStateOnPlay)
			{
				SetStatus(StateOnPlay);
			}
		}

        /// <summary>
        /// 在停止时，如果需要，我们改变行为的状态
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetBehaviour == null))
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);

			if (SetStateOnStop)
			{
				SetStatus(StateOnStop);
			}
		}

        /// <summary>
        /// 在重置时，如果需要，我们改变行为的状态
        /// </summary>
        protected override void CustomReset()
		{
			base.CustomReset();

			if (InCooldown)
			{
				return;
			}
            
			if (!Active || !FeedbackTypeAuthorized || (TargetBehaviour == null))
			{
				return;
			}
            
			if (SetStateOnReset)
			{
				SetStatus(StateOnReset);
			}
		}

        /// <summary>
        /// 改变行为的状态。
        /// </summary>
        /// <param name="state"></param>
        protected virtual void SetStatus(PossibleStates state)
		{
			SetStatus(state, TargetBehaviour);
			foreach (Behaviour extra in ExtraTargetBehaviours)
			{
				SetStatus(state, extra);
			}
		}

        /// <summary>
        /// 将指定的状态设置到目标行为上
        /// </summary>
        /// <param name="state"></param>
        /// <param name="target"></param>
        protected virtual void SetStatus(PossibleStates state, Behaviour target)
		{
			_initialState = target.enabled;
			switch (state)
			{
				case PossibleStates.Enabled:
					target.enabled = NormalPlayDirection ? true : false;
					break;
				case PossibleStates.Disabled:
					target.enabled = NormalPlayDirection ? false : true;
					break;
				case PossibleStates.Toggle:
					target.enabled = !target.enabled;
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
			
			TargetBehaviour.enabled = _initialState;
			foreach (Behaviour extra in ExtraTargetBehaviours)
			{
				extra.enabled = _initialState;
			}
		}
	}
}