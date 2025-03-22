using UnityEngine;
using MoreMountains.Tools;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此组件添加到角色中，它将能够激活按钮区域
    /// 动画器参数：激活（bool）
    /// </summary>
    [MMHiddenProperties("AbilityStopFeedbacks")]
	[AddComponentMenu("TopDown Engine/Character/Abilities/Character Button Activation")] 
	public class CharacterButtonActivation : CharacterAbility 
	{
        /// 这个方法仅用于在能力的检查器开始时显示一个帮助框文本
        public override string HelpBoxText() { return "这个组件允许你的角色与按钮驱动的对象（对话区域，开关……）进行交互。 "; }
        /// 如果角色在一个对话区域，则为真。
        public bool InButtonActivatedZone {get;set;}
        /// 如果该区域是自动化的，则为真
        public virtual bool InButtonAutoActivatedZone { get; set; }
		/// if this is true, characters won't be able to jump while in a button activated zone
		[Tooltip("如果这是真的，角色在按钮激活的区域中将无法跳跃")]
		public bool PreventJumpInButtonActivatedZone = true; 
		/// the current button activated zone
		[Tooltip("该角色所在的当前按钮激活区域")]
		[MMReadOnly]
		public ButtonActivated ButtonActivatedZone;

		protected bool _activating = false;
		protected const string _activatingAnimationParameterName = "Activating";
		protected int _activatingAnimationParameter;

        /// <summary>
        /// 获取和存储组件以供以后使用
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			InButtonActivatedZone = false;
			ButtonActivatedZone = null;
			InButtonAutoActivatedZone = false;
		}

        /// <summary>
        /// 每一帧，我们都会检查输入，看看是否需要暂停/取消游戏
        /// </summary>
        protected override void HandleInput()
		{
			if (!AbilityAuthorized)
			{
				return;
			}
			if (InButtonActivatedZone && (ButtonActivatedZone != null))
			{
				bool buttonPressed = false;
				switch (ButtonActivatedZone.InputType)
				{
					case ButtonActivated.InputTypes.Default:
						buttonPressed = (_inputManager.InteractButton.State.CurrentState == MMInput.ButtonStates.ButtonDown);
						break;
					#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
						case ButtonActivated.InputTypes.Button:
						case ButtonActivated.InputTypes.Key:
							buttonPressed = ButtonActivatedZone.InputActionPerformed;
							break;
					#else
						case ButtonActivated.InputTypes.Button:
							buttonPressed = (Input.GetButtonDown(_character.PlayerID + "_" + ButtonActivatedZone.InputButton));
							break;
						case ButtonActivated.InputTypes.Key:
							buttonPressed = (Input.GetKeyDown(ButtonActivatedZone.InputKey));
							break;
					#endif
				}

				if (buttonPressed)
				{
					ButtonActivation();
				}
			}
		}

        /// <summary>
        /// 试图激活按钮激活区
        /// </summary>
        protected virtual void ButtonActivation()
		{
            // 如果玩家处于按钮激活区域，我们就会处理它
            if ((InButtonActivatedZone)
			    && (ButtonActivatedZone!=null)
			    && (_condition.CurrentState == CharacterStates.CharacterConditions.Normal || _condition.CurrentState == CharacterStates.CharacterConditions.Frozen)
			    && (_movement.CurrentState != CharacterStates.MovementStates.Dashing))
			{
                // 如果这个按钮只能在接地时激活，如果我们没有接地，我们什么都不做，然后退出
                if (ButtonActivatedZone.CanOnlyActivateIfGrounded && !_controller.Grounded)
				{
					return;
				}

                // 	如果是自动激活区，我们什么都不做
                if (ButtonActivatedZone.AutoActivation)
				{
					return;
				}

                // 我们触发一个角色事件
                MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.ButtonActivation);

				ButtonActivatedZone.TriggerButtonAction();
				PlayAbilityStartFeedbacks();
				_activating = true;
			}
		}

        /// <summary>	
        /// 在死亡中，我们失去了与按钮激活区域的任何联系
        /// </summary>	
        protected override void OnDeath()
		{
			base.OnDeath();
			ResetFlags();
		}

        /// <summary>
        /// 在启用时，我们确保我们重置了我们的标志
        /// </summary>
        protected override void OnEnable()
		{
			base.OnEnable();
			ResetFlags();
		}

        /// <summary>
        /// 重置我们的区域标志
        /// </summary>
        protected virtual void ResetFlags()
		{
			InButtonActivatedZone = false;
			ButtonActivatedZone = null;
			InButtonAutoActivatedZone = false;
		}

        /// <summary>
        /// 添加所需的动画器参数到动画器参数列表（如果存在的话）
        /// </summary>
        protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter (_activatingAnimationParameterName, AnimatorControllerParameterType.Bool, out _activatingAnimationParameter);
		}

        /// <summary>
        /// 在能力循环结束时，我们将当前的蹲伏和爬行状态发送给动画师
        /// </summary>
        public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _activatingAnimationParameter, _activating, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			if (_activating && (ButtonActivatedZone != null) && (ButtonActivatedZone.AnimationTriggerParameterName != ""))
			{
				_animator.SetTrigger(ButtonActivatedZone.AnimationTriggerParameterName);
			}
			_activating = false;
		}
	}
}