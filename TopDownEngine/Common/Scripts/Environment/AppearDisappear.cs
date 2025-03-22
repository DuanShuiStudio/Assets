using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;
using Random = UnityEngine.Random;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此类添加到一个对象（通常是平台，但实际上可以是任何对象）上，使其在一个出现/消失的循环中运行，例如在《Mega Man》中出现的平台
    /// </summary>
    [AddComponentMenu("TopDown Engine/Environment/Appear and Disappear")]
	public class AppearDisappear : TopDownMonoBehaviour
	{
        /// 此对象可能处于的状态
        public enum AppearDisappearStates { Visible, Hidden, VisibleToHidden, HiddenToVisible }
        /// 可能的启动模式（automatic将在Start时开始，PlayerContact是在带有Player标签的对象与该对象碰撞时触发，而Script允许您手动触发）
        public enum StartModes { Automatic, PlayerContact, Script }

		public enum CyclingModes { Forever, Limited}
        
		[Header("Settings设置")]

		/// whether the object is active right now or not
		[Tooltip("此对象当前是否处于活动状态")]
		public bool Active = true;
		/// the initial state (visible or hidden) the object should start in
		[Tooltip("此对象应该从哪个初始状态（可见或隐藏）开始")]
		public AppearDisappearStates InitialState;
		/// how the object should be activated
		[Tooltip("此对象应该如何被激活")]
		public StartModes StartMode = StartModes.Automatic;
		/// how the object should cycle states (forever, a limited amount of times, or never)
		[Tooltip("此对象应该如何在不同状态之间循环（永远循环、有限次数循环或从不循环）")]
		public CyclingModes CyclingMode = CyclingModes.Forever;
		/// the number of cycles this object can go through before it stops (only used if CyclingMode is Limited)
		[Tooltip("此对象在停止之前可以经历的循环次数（仅在使用有限循环模式时有用）")]
		[MMEnumCondition("CyclingMode", (int)CyclingModes.Limited)]
		public int CyclesAmount = 1;


		[Header("Timing计时")]

		/// the initial offset to apply to the object's first state change (in seconds)
		[MMVector("Min", "Max")]
		[Tooltip("要应用于对象第一次状态变化的初始偏移量（以秒为单位）")]
		public Vector2 InitialOffset = new Vector2(0f, 0f);
		/// the min and max duration of the visible state (in seconds)
		[MMVector("Min", "Max")]
		[Tooltip("可见状态的最小和最大持续时间（以秒为单位）")]
		public Vector2 VisibleDuration = new Vector2(1f, 1f);
		/// the min and max duration of the hidden state (in seconds)
		[MMVector("Min", "Max")]
		[Tooltip("隐藏状态的最小和最大持续时间（以秒为单位）")]
		public Vector2 HiddenDuration = new Vector2(1f, 1f);
		/// the min and max duration of the visible to hidden state (in seconds)
		[MMVector("Min", "Max")]
		[Tooltip("从可见状态到隐藏状态的最小和最大持续时间（以秒为单位）")]
		public Vector2 VisibleToHiddenDuration = new Vector2(1f, 1f);
		/// the min and max duration of the hidden to visible state (in seconds)
		[MMVector("Min", "Max")]
		[Tooltip("从隐藏状态到可见状态的最小和最大持续时间（以秒为单位）")]
		public Vector2 HiddenToVisibleDuration = new Vector2(1f, 1f);
                
		[Header("Feedbacks反馈")]

		/// the feedback to trigger when reaching the visible state
		[Tooltip("当达到可见状态时触发的反馈")]
		public MMFeedbacks VisibleFeedback;
		/// the feedback to trigger when reaching the visible to hidden state
		[Tooltip("当达到从可见状态到隐藏状态时触发的反馈")]
		public MMFeedbacks VisibleToHiddenFeedback;
		/// the feedback to trigger when reaching the hidden state
		[Tooltip("当达到隐藏状态时触发的反馈")]
		public MMFeedbacks HiddenFeedback;
		/// the feedback to trigger when reaching the hidden to visible state
		[Tooltip("当达到从隐藏状态到可见状态时触发的反馈")]
		public MMFeedbacks HiddenToVisibleFeedback;

		[Header("Bindings绑定")]
		/// the animator to update
		[Tooltip("要更新的动画器")]
		public Animator TargetAnimator;
		/// the game object to show/hide
		[Tooltip("要显示/隐藏的游戏对象")]
		public GameObject TargetModel;
		/// whether or not the object should update its animator (set at the same level) when changing state
		[Tooltip("在更改状态时，对象是否应该更新其动画器（在同一级别设置）")]
		public bool UpdateAnimator = true;
		/// whether or not the object should update its Collider or Collider2D (set at the same level) when changing state
		[Tooltip("在更改状态时，对象是否应该更新其Collider或Collider2D（在同一级别设置）")]
		public bool EnableDisableCollider = true;
		/// whether or not the object should hide/show a model when changing state
		[Tooltip("在更改状态时，对象是否应该隐藏/显示一个模型")]
		public bool ShowHideModel = false;

		[Header("Trigger Area触发区域")]

		/// the area used to detect the presence of a character
		[Tooltip("用于检测角色存在与否的区域")]
		public CharacterDetector TriggerArea;
		/// whether or not we should prevent this component from appearing when a character is in the area
		[Tooltip("是否应该防止此组件在角色位于区域内时出现")]
		public bool PreventAppearWhenCharacterInArea = true;
		/// whether or not we should prevent this component from disappearing when a character is in the area
		[Tooltip("是否应该防止此组件在角色位于区域内时消失")]
		public bool PreventDisappearWhenCharacterInArea = false;

		[Header("Debug调试")]

		/// the current state this object is in
		[MMReadOnly]
		[Tooltip("此对象当前所处的状态")]
		public AppearDisappearStates _currentState;
		/// the state this object will be in next
		[MMReadOnly]
		[Tooltip("此对象下一个所处的状态")]
		public AppearDisappearStates _nextState;
		/// the last time this object changed state
		[MMReadOnly]
		[Tooltip("此对象上一次改变状态的时间")]
		public float _lastStateChangedAt = 0f;
		[MMReadOnly]
		[Tooltip("此对象上一次改变状态的时间")]
		public int _cyclesLeft;

		protected const string _animationParameter = "Visible";

		protected float _visibleDuration;
		protected float _hiddenDuration;
		protected float _visibleToHiddenDuration;
		protected float _hiddenToVisibleDuration;

		protected float _nextChangeIn;
		protected MMFeedbacks _nextFeedback;

		protected Collider _collider;
		protected Collider2D _collider2D;

		protected bool _characterInTriggerArea = false;

        /// <summary>
        /// 在开始（On Start）时，我们初始化我们的对象
        /// </summary>
        protected virtual void Start()
		{
			Initialization();
		}

        /// <summary>
        /// 在初始化（On Init）时，我们设置活动状态，获取组件，并确定下一个状态
        /// </summary>
        protected virtual void Initialization()
		{
			_currentState = InitialState;
			_lastStateChangedAt = Time.time;
			_cyclesLeft = CyclesAmount;

			Active = (StartMode == StartModes.Automatic);

			if (_currentState == AppearDisappearStates.HiddenToVisible) { _currentState = AppearDisappearStates.Visible; }
			if (_currentState == AppearDisappearStates.VisibleToHidden) { _currentState = AppearDisappearStates.Hidden; }

			if (TargetAnimator == null)
			{
				TargetAnimator = this.gameObject.GetComponent<Animator>();	
			}
			
			_collider = this.gameObject.GetComponent<Collider>();
			_collider2D = this.gameObject.GetComponent<Collider2D>();

			RandomizeDurations();

			_visibleDuration += Random.Range(InitialOffset.x, InitialOffset.y);
			_hiddenDuration += Random.Range(InitialOffset.x, InitialOffset.y);

			UpdateBoundComponents(_currentState == AppearDisappearStates.Visible);

			DetermineNextState();
		}

        /// <summary>
        /// 激活或禁用出现/消失的行为
        /// </summary>
        /// <param name="status"></param>
        public virtual void Activate(bool status)
		{
			Active = status;
		}

        /// <summary>
        /// 在更新（On Update）时，我们处理状态机
        /// </summary>
        protected virtual void Update()
		{
			ProcessTriggerArea();
			ProcessStateMachine();
		}

		protected virtual void ProcessTriggerArea()
		{
			_characterInTriggerArea = false;
			if (TriggerArea == null)
			{
				return;
			}
			_characterInTriggerArea = TriggerArea.CharacterInArea;
		}

        /// <summary>
        /// 如果时间需要，将状态更改为下一个状态。
        /// </summary>
        protected virtual void ProcessStateMachine()
		{
			if (!Active)
			{
				return;
			}

			if (Time.time - _lastStateChangedAt > _nextChangeIn)
			{
				ChangeState();
			}
		}

        /// <summary>
        /// 确定此对象下一个应该处于的状态
        /// </summary>
        protected virtual void DetermineNextState()
		{
			switch (_currentState)
			{
				case AppearDisappearStates.Visible:
					_nextChangeIn = _visibleDuration;
					_nextState = AppearDisappearStates.VisibleToHidden;
					_nextFeedback = VisibleToHiddenFeedback;
					break;
				case AppearDisappearStates.Hidden:
					_nextChangeIn = _hiddenDuration;
					_nextState = AppearDisappearStates.HiddenToVisible;
					_nextFeedback = HiddenToVisibleFeedback;
					break;
				case AppearDisappearStates.HiddenToVisible:
					_nextChangeIn = _hiddenToVisibleDuration;
					_nextState = AppearDisappearStates.Visible;
					_nextFeedback = VisibleFeedback;
					break;
				case AppearDisappearStates.VisibleToHidden:
					_nextChangeIn = _visibleToHiddenDuration;
					_nextState = AppearDisappearStates.Hidden;
					_nextFeedback = HiddenFeedback;
					break;
			}
		}

        /// <summary>
        /// 更改下一行的状态
        /// </summary>
        public virtual void ChangeState()
		{
			if (((_nextState == AppearDisappearStates.HiddenToVisible) || (_nextState == AppearDisappearStates.Visible))
			    && _characterInTriggerArea
			    && PreventAppearWhenCharacterInArea)
			{
				return;
			}

			if (((_nextState == AppearDisappearStates.VisibleToHidden) || (_nextState == AppearDisappearStates.Hidden))
			    && _characterInTriggerArea
			    && PreventDisappearWhenCharacterInArea)
			{
				return;
			}

			_lastStateChangedAt = Time.time;
			_currentState = _nextState;
			_nextFeedback?.PlayFeedbacks();
			RandomizeDurations();

			if (_currentState == AppearDisappearStates.Hidden)
			{
				UpdateBoundComponents(false);   
			}

			if (_currentState == AppearDisappearStates.Visible)
			{
				UpdateBoundComponents(true);
			}
            
			DetermineNextState();

			if (CyclingMode == CyclingModes.Limited)
			{
				if (_currentState == AppearDisappearStates.Hidden || _currentState == AppearDisappearStates.Visible)
				{
					_cyclesLeft--;
					if (_cyclesLeft <= 0)
					{
						Active = false;
					}
				}
			}
		}

        /// <summary>
        /// 根据可见状态更新动画器、碰撞器和渲染器
        /// </summary>
        /// <param name="visible"></param>
        protected virtual void UpdateBoundComponents(bool visible)
		{
			if (UpdateAnimator && (TargetAnimator != null))
			{
				TargetAnimator.SetBool(_animationParameter, visible);
			}
			if (EnableDisableCollider)
			{
				if (_collider != null)
				{
					_collider.enabled = visible;
				}
				if (_collider2D != null)
				{
					_collider2D.enabled = visible;
				}
			}
			if (ShowHideModel && (TargetModel != null))
			{
				TargetModel.SetActive(visible);
			}
		}

        /// <summary>
        /// 根据在Inspector中设置的最小值和最大值，使每个状态的持续时间随机化
        /// </summary>
        protected virtual void RandomizeDurations()
		{
			_visibleDuration = Random.Range(VisibleDuration.x, VisibleDuration.y);
			_hiddenDuration = Random.Range(HiddenDuration.x, HiddenDuration.y);
			_visibleToHiddenDuration = Random.Range(VisibleToHiddenDuration.x, VisibleToHiddenDuration.y);
			_hiddenToVisibleDuration = Random.Range(HiddenToVisibleDuration.x, HiddenToVisibleDuration.y);
		}

        /// <summary>
        /// 当与另一个对象发生碰撞时，我们检查它是否是玩家，如果是并且我们应该在与玩家接触时启动，我们就启用我们的对象
        /// </summary>
        /// <param name="collider"></param>
        public virtual void OnTriggerEnter2D(Collider2D collider)
		{
			if (StartMode != StartModes.PlayerContact)
			{
				return;
			}

			if (collider.CompareTag("Player"))
			{
				_lastStateChangedAt = Time.time;
				Activate(true);
			}
		}

		public virtual void ResetCycling()
		{
			_cyclesLeft = CyclesAmount;
		}
	}
}