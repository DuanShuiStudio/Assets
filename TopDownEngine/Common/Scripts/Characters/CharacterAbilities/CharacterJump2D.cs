using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将这种能力添加到角色中，它将能够在2D中跳跃。这意味着没有实际的移动，只有对撞机关闭和打开。移动将由动画本身处理。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Jump 2D")]
	public class CharacterJump2D : CharacterAbility 
	{
		/// the duration of the jump
		[Tooltip("跳跃的持续时间")]
		public float JumpDuration = 1f;
		/// whether or not jump should be proportional to press (if yes, releasing the button will stop the jump)
		[Tooltip("跳跃是否应该与按下的比例成正比（如果是，松开按钮将停止跳跃）")]
		public bool JumpProportionalToPress = true;
		/// the minimum amount of time after the jump starts before releasing the jump has any effect
		[Tooltip("跳跃开始后释放跳跃产生影响的最短时间")]
		public float MinimumPressTime = 0.4f;
		/// the feedback to play when the jump starts
		[Tooltip("跳跃开始时播放的反馈")]
		public MMFeedbacks JumpStartFeedback;
		/// the feedback to play when the jump stops
		[Tooltip("跳跃停止时播放的反馈")]
		public MMFeedbacks JumpStopFeedback;

		protected CharacterButtonActivation _characterButtonActivation;
		protected bool _jumpStopped = false;
		protected float _jumpStartedAt = 0f;
		protected bool _buttonReleased = false;
		protected const string _jumpingAnimationParameterName = "Jumping";
		protected const string _hitTheGroundAnimationParameterName = "HitTheGround";
		protected int _jumpingAnimationParameter;
		protected int _hitTheGroundAnimationParameter;

        /// <summary>
        /// 在init中，我们获取组件
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization ();
			_characterButtonActivation = _character?.FindAbility<CharacterButtonActivation> ();
			JumpStartFeedback?.Initialization(this.gameObject);
			JumpStopFeedback?.Initialization(this.gameObject);
		}

        /// <summary>
        /// 在HandleInput中，我们监视跳转输入并在需要时触发跳转
        /// </summary>
        protected override void HandleInput()
		{
			base.HandleInput();
            // 如果移动被阻止，或者角色死亡/冻结/无法移动，我们就退出游戏，什么也不做
            if (!AbilityAuthorized
			    || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal))
			{
				return;
			}
			if (_inputManager.JumpButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				JumpStart();
			}
			if (_inputManager.JumpButton.State.CurrentState == MMInput.ButtonStates.ButtonUp)
			{
				_buttonReleased = true;
			}
		}

        /// <summary>
        /// 在处理能力上，如果需要，我们会停止跳跃
        /// </summary>
        public override void ProcessAbility()
		{
			if (_movement.CurrentState == CharacterStates.MovementStates.Jumping)
			{
				if (!_jumpStopped)
				{
					if (Time.time - _jumpStartedAt >= JumpDuration)
					{
						JumpStop();
					}
					else
					{
						_movement.ChangeState(CharacterStates.MovementStates.Jumping);
					}
				}
				if (_buttonReleased
				    && !_jumpStopped
				    && JumpProportionalToPress
				    && (Time.time - _jumpStartedAt > MinimumPressTime))
				{
					JumpStop();
				}
			}
		}

        /// <summary>
        /// 开始跳跃
        /// </summary>
        public virtual void JumpStart()
		{
			if (!EvaluateJumpConditions())
			{
				return;
			}
			_movement.ChangeState(CharacterStates.MovementStates.Jumping);	
			MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.Jump);
			JumpStartFeedback?.PlayFeedbacks(this.transform.position);
			PlayAbilityStartFeedbacks();

			_jumpStopped = false;
			_jumpStartedAt = Time.time;
			_buttonReleased = false;
		}

        /// <summary>
        /// 停止跳跃
        /// </summary>
        public virtual void JumpStop()
		{
			_jumpStopped = true;
			_movement.ChangeState(CharacterStates.MovementStates.Idle);
			_buttonReleased = false;
			JumpStopFeedback?.PlayFeedbacks(this.transform.position);
			StopStartFeedbacks();
			PlayAbilityStopFeedbacks();
		}

        /// <summary>
        /// 如果满足跳转条件则返回true
        /// </summary>
        /// <returns></returns>
        protected virtual bool EvaluateJumpConditions()
		{
			if (!AbilityAuthorized)
			{
				return false;
			}
			if (_characterButtonActivation != null)
			{
				if (_characterButtonActivation.AbilityAuthorized
				    && _characterButtonActivation.InButtonActivatedZone)
				{
					return false;
				}
			}
			return true;
		}

        /// <summary>
        /// 添加所需的动画器参数到动画器参数列表（如果存在的话）
        /// </summary>
        protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter (_jumpingAnimationParameterName, AnimatorControllerParameterType.Bool, out _jumpingAnimationParameter);
			RegisterAnimatorParameter (_hitTheGroundAnimationParameterName, AnimatorControllerParameterType.Bool, out _hitTheGroundAnimationParameter);
		}

        /// <summary>
        /// 在每个循环结束时，向角色的动画师发送跳跃状态
        /// </summary>
        public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _jumpingAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Jumping),_character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool (_animator, _hitTheGroundAnimationParameter, _controller.JustGotGrounded, _character._animatorParameters, _character.RunAnimatorSanityChecks);
		}
	}
}