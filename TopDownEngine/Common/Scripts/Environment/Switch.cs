using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEngine.Events;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 可以使用开关根据其当前状态（打开或关闭）触发操作。可用于打开门、箱子、传送门、桥梁等……
    /// </summary>
    [AddComponentMenu("TopDown Engine/Environment/Switch")]
	public class Switch : TopDownMonoBehaviour
	{
		[Header("Bindings绑定")]
		/// a SpriteReplace to represent the switch knob when it's on
		[Tooltip("一个SpriteReplace，用于表示开关旋钮打开时的状态")]
		public Animator SwitchAnimator;

        /// 开关的可能状态
        public enum SwitchStates { On, Off }
        /// 开关的当前状态
        public virtual SwitchStates CurrentSwitchState { get; set; }

		[Header("Switch开关")]

		/// the state the switch should start in
		[Tooltip("开关应该开始的状态")]
		public SwitchStates InitialState = SwitchStates.Off;

		[Header("Events事件")]

		/// the methods to call when the switch is turned on
		[Tooltip("当开关打开时要调用的方法")]
		public UnityEvent SwitchOn;
		/// the methods to call when the switch is turned off
		[Tooltip("当开关关闭时要调用的方法")]
		public UnityEvent SwitchOff;
		/// the methods to call when the switch is toggled
		[Tooltip("当开关切换时要调用的方法")]
		public UnityEvent SwitchToggle;

		[Header("Feedbacks反馈")]

		/// a feedback to play when the switch is toggled on
		[Tooltip("当开关打开时播放的反馈")]
		public MMFeedbacks SwitchOnFeedback;
		/// a feedback to play when the switch is turned off
		[Tooltip("当开关关闭时播放的反馈")]
		public MMFeedbacks SwitchOffFeedback;
		/// a feedback to play when the switch changes state
		[Tooltip("当开关改变状态时播放的反馈")]
		public MMFeedbacks ToggleFeedback;

		[MMInspectorButton("TurnSwitchOn")]
        /// 一个用于打开开关的测试按钮。
        public bool SwitchOnButton;
		[MMInspectorButton("TurnSwitchOff")]
        /// 一个用于关闭开关的测试按钮。
        public bool SwitchOffButton;
		[MMInspectorButton("ToggleSwitch")]
        /// 一个用于更改开关状态的测试按钮。
        public bool ToggleSwitchButton;

        /// <summary>
        /// 在初始化时，我们设置当前的开关状态
        /// </summary>
        protected virtual void Start()
		{
			CurrentSwitchState = InitialState;
			SwitchOffFeedback?.Initialization(this.gameObject);
			SwitchOnFeedback?.Initialization(this.gameObject);
			ToggleFeedback?.Initialization(this.gameObject);
		}

        /// <summary>
        /// 打开开关
        /// </summary>
        public virtual void TurnSwitchOn()
		{
			CurrentSwitchState = SwitchStates.On;
			if (SwitchOn != null) { SwitchOn.Invoke(); }
			if (SwitchToggle != null) { SwitchToggle.Invoke(); }
			SwitchOnFeedback?.PlayFeedbacks(this.transform.position);
		}

        /// <summary>
        /// 关闭开关
        /// </summary>
        public virtual void TurnSwitchOff()
		{
			CurrentSwitchState = SwitchStates.Off;
			if (SwitchOff != null) { SwitchOff.Invoke(); }
			if (SwitchToggle != null) { SwitchToggle.Invoke(); }
			SwitchOffFeedback?.PlayFeedbacks(this.transform.position);
		}

        /// <summary>
        /// 使用此方法从一种状态转到另一种状态。
        /// </summary>
        public virtual void ToggleSwitch()
		{
			if (CurrentSwitchState == SwitchStates.Off)
			{
				CurrentSwitchState = SwitchStates.On;
				if (SwitchOn != null) { SwitchOn.Invoke(); }
				if (SwitchToggle != null) { SwitchToggle.Invoke(); }
				SwitchOnFeedback?.PlayFeedbacks(this.transform.position);
			}
			else
			{
				CurrentSwitchState = SwitchStates.Off;
				if (SwitchOff != null) { SwitchOff.Invoke(); }
				if (SwitchToggle != null) { SwitchToggle.Invoke(); }
				SwitchOffFeedback?.PlayFeedbacks(this.transform.position);
			}
			ToggleFeedback?.PlayFeedbacks(this.transform.position);
		}

        /// <summary>
        /// 在更新时，我们更新开关的动画。
        /// </summary>
        protected virtual void Update()
		{
			if (SwitchAnimator != null)
			{
				SwitchAnimator.SetBool("SwitchOn", (CurrentSwitchState == SwitchStates.On));
			}            
		}
	}
}