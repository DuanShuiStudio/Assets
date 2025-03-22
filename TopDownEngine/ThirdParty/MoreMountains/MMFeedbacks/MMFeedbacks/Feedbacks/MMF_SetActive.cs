using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 在反馈的不同阶段，使一个对象处于激活或非激活状态。
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("这种反馈机制允许你在初始化、播放、停止或重置操作时，将目标游戏对象的状态从激活变为非激活（或者反过来）。对于上述每一个操作，你都可以指定是要强制设置为某种状态（激活或非激活），还是进行切换（激活的变为非激活，非激活的变为激活）。")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("GameObject/Set Active")]
	public class MMF_SetActive : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用此类所有反馈。
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色
#if UNITY_EDITOR
        public override Color FeedbackColor => MMFeedbacksInspectorColors.GameObjectColor; 
		public override bool EvaluateRequiresSetup() => (TargetGameObject == null); 
		public override string RequiredTargetText => TargetGameObject != null ? TargetGameObject.name : "";
		public override string RequiredTargetTextExtra
		{
			get
			{
				if (ExtraTargetGameObjects.Count > 0)
				{
					return " (+"+ExtraTargetGameObjects.Count+")";
				}
				return "";
			}
		}
		public override string RequiresSetupText => "此反馈功能需要设置一个目标游戏对象（TargetGameObject）才能正常工作。你可以在下方设置一个。"; 
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetGameObject = FindAutomatedTargetGameObject();

        /// 这种反馈可能对目标对象的状态产生的各种影响。
        public enum PossibleStates { Active, Inactive, Toggle }
        
		[MMFInspectorGroup("Set Active Target", true, 12, true)]
		/// the gameobject we want to change the active state of
		[Tooltip("我们想要改变其激活状态的那个游戏对象。")]
		public GameObject TargetGameObject;
		/// a list of extra gameobjects we want to change the active state of
		[Tooltip("我们想要改变其激活状态的额外游戏对象的列表。")]
		public List<GameObject> ExtraTargetGameObjects;
        
		[MMFInspectorGroup("States", true, 14)]
		/// whether or not we should alter the state of the target object on init
		[Tooltip("我们在初始化时是否应该改变目标对象的状态。")]
		public bool SetStateOnInit = false;
		[MMFCondition("SetStateOnInit", true)]
		/// how to change the state on init
        [Tooltip("在初始化时如何改变状态")]
        public PossibleStates StateOnInit = PossibleStates.Inactive;
		/// whether or not we should alter the state of the target object on play
		[Tooltip("我们在播放时是否应该改变目标对象的状态。")]
		public bool SetStateOnPlay = false;
		/// how to change the state on play
		[Tooltip("在播放时如何改变状态")]
		[MMFCondition("SetStateOnPlay", true)]
		public PossibleStates StateOnPlay = PossibleStates.Inactive;
		/// whether or not we should alter the state of the target object on stop
		[Tooltip("我们在停止时是否应该改变目标对象的状态")]
		public bool SetStateOnStop = false;
		/// how to change the state on stop
		[Tooltip("在停止时如何改变状态")]
		[MMFCondition("SetStateOnStop", true)]
		public PossibleStates StateOnStop = PossibleStates.Inactive;
		/// whether or not we should alter the state of the target object on reset
		[Tooltip("我们在重置时是否应该改变目标对象的状态。")]
		public bool SetStateOnReset = false;
		/// how to change the state on reset
		[Tooltip("在重置时如何改变状态")]
		[MMFCondition("SetStateOnReset", true)]
		public PossibleStates StateOnReset = PossibleStates.Inactive;
		/// whether or not we should alter the state of the target object on skip
		[Tooltip("我们在执行跳过操作时是否应该改变目标对象的状态。")]
		public bool SetStateOnSkip = false;
		/// how to change the state on skip
		[Tooltip("在跳过时如何改变状态")]
		[MMFCondition("SetStateOnSkip", true)]
		public PossibleStates StateOnSkip = PossibleStates.Inactive;
		/// whether or not we should alter the state of the target object when the player this feedback belongs to is done playing all its feedbacks
		[Tooltip("当此反馈所属的玩家完成其所有反馈的播放后，我们是否应该改变目标对象的状态。")]
		public bool SetStateOnPlayerComplete = false;
		/// how to change the state on player complete
		[Tooltip("在完成时如何改变状态")]
		[MMFCondition("SetStateOnPlayerComplete", true)]
		public PossibleStates StateOnPlayerComplete = PossibleStates.Inactive;

		protected bool _initialState;
		protected List<bool> _initialStates;

        /// <summary>
        /// 在初始化时，如果有需要，我们会改变对象的状态。
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);

			_initialStates = new List<bool>(ExtraTargetGameObjects.Count);
			
			if (Active && (TargetGameObject != null))
			{
				_initialState = TargetGameObject.activeInHierarchy;
				
				for (int i = 0; i < ExtraTargetGameObjects.Count; i++)
				{
					_initialStates.Add(ExtraTargetGameObjects[i].activeInHierarchy);
				}

				if (SetStateOnInit)
				{
					SetStatus(StateOnInit);
				}
			}
		}

        /// <summary>
        /// 在播放时，如果有需要，我们会改变对象的状态。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetGameObject == null))
			{
				return;
			}
            
			if (SetStateOnPlay)
			{
				SetStatus(StateOnPlay);
			}
		}

        /// <summary>
        /// 在停止时，如果有需要，我们会改变对象的状态
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			base.CustomStopFeedback(position, feedbacksIntensity);

			if (Active && FeedbackTypeAuthorized && (TargetGameObject != null))
			{
				if (SetStateOnStop)
				{
					SetStatus(StateOnStop);
				}
			}
		}

        /// <summary>
        ///在重置时，如果有必要，我们会改变对象的状态。
        /// </summary>
        protected override void CustomReset()
		{
			base.CustomReset();

			if (InCooldown)
			{
				return;
			}

			if (Active && FeedbackTypeAuthorized && (TargetGameObject != null))
			{
				if (SetStateOnReset)
				{
					SetStatus(StateOnReset);
				}
			}
		}

        /// <summary>
        /// 当玩家完成操作时，如果有需要，我们会改变对象的状态
        /// </summary>
        protected override void CustomPlayerComplete()
		{
			base.CustomPlayerComplete();

			if (InCooldown)
			{
				return;
			}

			if (Active && FeedbackTypeAuthorized && (TargetGameObject != null))
			{
				if (SetStateOnPlayerComplete)
				{
					SetStatus(StateOnPlayerComplete);
				}
			}
		}


        /// <summary>
        /// 在执行跳过操作时，如果有需要，会改变我们目标对象的状态。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomSkipToTheEnd(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			base.CustomSkipToTheEnd(position, feedbacksIntensity);

			if (InCooldown)
			{
				return;
			}

			if (Active && FeedbackTypeAuthorized && (TargetGameObject != null))
			{
				if (SetStateOnSkip)
				{
					SetStatus(StateOnSkip);
				}
			}
		}

        /// <summary>
        /// 改变对象的状态。
        /// </summary>
        /// <param name="state"></param>
        protected virtual void SetStatus(PossibleStates state)
		{
			bool newState = false;
			switch (state)
			{
				case PossibleStates.Active:
					newState = NormalPlayDirection ? true : false;
					break;
				case PossibleStates.Inactive:
					newState = NormalPlayDirection ? false : true;
					break;
				case PossibleStates.Toggle:
					newState = !TargetGameObject.activeInHierarchy;
					break;
			}
			
			ApplyStatus(TargetGameObject, newState);
			foreach (GameObject go in ExtraTargetGameObjects)
			{
				ApplyStatus(go, newState);
			}
		}

        /// <summary>
        /// 将该状态应用到目标游戏对象上。
        /// </summary>
        /// <param name="target"></param>
        /// <param name="newState"></param>
        protected virtual void ApplyStatus(GameObject target, bool newState)
		{
			target.SetActive(newState);
		}

        /// <summary>
        /// 在恢复操作时，我们会将对象放回其初始位置。
        /// </summary>
        protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			TargetGameObject.SetActive(_initialState);
			for (int i = 0; i < ExtraTargetGameObjects.Count; i++)
			{
				ExtraTargetGameObjects[i].SetActive(_initialStates[i]);
			}
		}
	}
}