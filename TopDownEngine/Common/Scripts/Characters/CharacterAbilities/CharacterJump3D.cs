using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 这个能力将允许角色在3D中跳跃
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Jump 3D")]
	public class CharacterJump3D : CharacterAbility 
	{
		[Header("Jump Settings跳跃设置")]
		/// whether or not the jump should be proportional to press (if yes, releasing the button will stop the jump)
		[Tooltip("跳跃是否应该与按下的比例成正比（如果是，松开按钮将停止跳跃）")]
		public bool JumpProportionalToPress = true;
		/// the minimum amount of time after the jump's start before releasing the jump button has any effect
		[Tooltip("跳跃开始后释放跳跃产生影响的最短时间")]
		public float MinimumPressTime = 0.4f;
		/// the force to apply to the jump, the higher the jump, the faster the jump
		[Tooltip("应用于跳跃的力，跳跃越高，跳跃越快")]
		public float JumpForce = 800f;
		/// the height the jump should have
		[Tooltip("跳跃应该达到的高度")]
		public float JumpHeight = 4f;

		[Header("Slopes斜坡")]
		/// whether or not the character can jump if standing on a slope too steep to walk on
		[Tooltip("角色是否可以在坡度太陡无法行走的斜坡上跳跃")]
		public bool CanJumpOnTooSteepSlopes = true;
		/// whether or not standing on a slope too steep to walk on should reset jump counters 
		[Tooltip("站在坡度太陡无法行走的斜坡上是否应重置跳跃计数器")]
		public bool ResetJumpsOnTooSteepSlopes = false;
        
		[Header("Number of Jumps跳跃次数")]
		/// the maximum number of jumps allowed (0 : no jump, 1 : normal jump, 2 : double jump, etc...)
		[Tooltip("允许的最大跳跃次数（0：不跳跃，1：正常跳跃，2：双跳，等等…）")]
		public int NumberOfJumps = 1;
		/// the number of jumps left to the character
		[MMReadOnly]
		[Tooltip("角色剩余的跳跃次数")]
		public int NumberOfJumpsLeft = 0;

		[Header("Feedbacks反馈")]
		/// the feedback to play when the jump starts
		[Tooltip("跳跃开始时播放的反馈")]
		public MMFeedbacks JumpStartFeedback;
		/// the feedback to play when the jump stops
		[Tooltip("跳跃停止时播放的反馈")]
		public MMFeedbacks JumpStopFeedback;

		protected bool _doubleJumping;
		protected Vector3 _jumpForce;
		protected Vector3 _jumpOrigin;
		protected CharacterButtonActivation _characterButtonActivation;
		protected CharacterCrouch _characterCrouch;
		protected bool _jumpStopped = false;
		protected float _jumpStartedAt = 0f;
		protected bool _buttonReleased = false;
		protected int _initialNumberOfJumps;

		protected const string _jumpingAnimationParameterName = "Jumping";
		protected const string _doubleJumpingAnimationParameterName = "DoubleJumping";
		protected const string _hitTheGroundAnimationParameterName = "HitTheGround";
		protected int _jumpingAnimationParameter;
		protected int _doubleJumpingAnimationParameter;
		protected int _hitTheGroundAnimationParameter;

        /// <summary>
        ///在init中，我们获取其他组件
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization ();
			ResetNumberOfJumps();
			_jumpStopped = true;
			_characterButtonActivation = _character?.FindAbility<CharacterButtonActivation> ();
			_characterCrouch = _character?.FindAbility<CharacterCrouch> ();
			JumpStartFeedback?.Initialization(this.gameObject);
			JumpStopFeedback?.Initialization(this.gameObject);
			_initialNumberOfJumps = NumberOfJumps;
		}

        /// <summary>
        /// 监视输入并在需要时触发跳转
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
        /// 在处理能力上，我们检查是否应该停止跳跃
        /// </summary>
        public override void ProcessAbility()
		{
			if (_controller.JustGotGrounded)
			{
				ResetNumberOfJumps();
			}

            // 如果移动被阻止，或者角色死亡/冻结/无法移动，我们就退出游戏，什么也不做
            if (!AbilityAuthorized
			    || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal))
			{
				return;
			}

			if (!_jumpStopped
			    &&
			    ((_movement.CurrentState == CharacterStates.MovementStates.Idle)
			     || (_movement.CurrentState == CharacterStates.MovementStates.Walking)
			     || (_movement.CurrentState == CharacterStates.MovementStates.Running)
			     || (_movement.CurrentState == CharacterStates.MovementStates.Crouching)
			     || (_movement.CurrentState == CharacterStates.MovementStates.Crawling)
			     || (_movement.CurrentState == CharacterStates.MovementStates.Pushing)
			     || (_movement.CurrentState == CharacterStates.MovementStates.Falling)
			    ))
			{
				JumpStop();
			}

			if (_movement.CurrentState == CharacterStates.MovementStates.Jumping)
			{
				if (_buttonReleased 
				    && !_jumpStopped
				    && JumpProportionalToPress 
				    && (Time.time - _jumpStartedAt > MinimumPressTime))
				{
					JumpStop();
				}
	            
				if (!_jumpStopped)
				{
					if ((this.transform.position.y - _jumpOrigin.y > JumpHeight)
					    || CeilingTest())
					{
						JumpStop();
						_controller3D.Grounded = _controller3D.IsGroundedTest();
						if (_controller.Grounded)
						{
							ResetNumberOfJumps();  
						}
					}
					else
					{
						_jumpForce = Vector3.up * JumpForce * Time.deltaTime;
						_controller.AddForce(_jumpForce);
					}
				}
			}

			if (!ResetJumpsOnTooSteepSlopes && _controller3D.ExitedTooSteepSlopeThisFrame && _controller3D.Grounded)
			{
				ResetNumberOfJumps();
			}
		}

        /// <summary>
        /// 如果在单元格上方找到角色返回true，否则返回false
        /// </summary>
        protected virtual bool CeilingTest()
		{
			bool returnValue = _controller3D.CollidingAbove();
			return returnValue;
		}

        /// <summary>
        /// 在跳跃启动时，我们将状态更改为跳跃
        /// </summary>
        public virtual void JumpStart()
		{
			if (!EvaluateJumpConditions())
			{
				return;
			}

			if (NumberOfJumpsLeft != NumberOfJumps)
			{
				_doubleJumping = true;
			}

            // 我们减少剩下的跳跃次数
            NumberOfJumpsLeft = NumberOfJumpsLeft - 1;

			_movement.ChangeState(CharacterStates.MovementStates.Jumping);	
			MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.Jump);
			JumpStartFeedback?.PlayFeedbacks(this.transform.position);
			_jumpOrigin = this.transform.position;
			_jumpStopped = false;
			_jumpStartedAt = Time.time;
			_controller.Grounded = false;
			_controller.GravityActive = false;
			_buttonReleased = false;

			PlayAbilityStartSfx();
			PlayAbilityUsedSfx();
			PlayAbilityStartFeedbacks();
		}

        /// <summary>
        /// 停止跳跃
        /// </summary>
        public virtual void JumpStop()
		{
			_controller.GravityActive = true;
			if (_controller.Velocity.y > 0)
			{
				_controller.Velocity.y = 0f;
			}
			_jumpStopped = true;
			_buttonReleased = false;
			PlayAbilityStopSfx();
			StopAbilityUsedSfx();
			StopStartFeedbacks();
			PlayAbilityStopFeedbacks();
			JumpStopFeedback?.PlayFeedbacks(this.transform.position);
		}

        /// <summary>
        /// 重置跳跃次数。
        /// </summary>
        public virtual void ResetNumberOfJumps()
		{
			bool shouldResetJumps = true;

			if (!ResetJumpsOnTooSteepSlopes)
			{
				if (_controller3D.TooSteep())
				{
					shouldResetJumps = false;
				}
			}

			if (shouldResetJumps)
			{
				NumberOfJumpsLeft = NumberOfJumps;
			}
			
			_doubleJumping = false;
		}

        /// <summary>
        /// 计算跳转条件，如果可以执行跳转，则返回true
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
				    && _characterButtonActivation.InButtonActivatedZone
				    && _characterButtonActivation.PreventJumpInButtonActivatedZone)
				{
					return false;
				}
			}

			if (!CanJumpOnTooSteepSlopes)
			{
				if (_controller3D.TooSteep())
				{
					return false;
				}
			}

			if (_characterCrouch != null)
			{
				if (_characterCrouch.InATunnel)
				{
					return false;
				}
			}

			if (CeilingTest())
			{
				return false;
			}

			if (NumberOfJumpsLeft <= 0)
			{
				return false;
			}

			if (_movement.CurrentState == CharacterStates.MovementStates.Dashing)
			{
				return false;
			}
			return true;
		}

        /// <summary>
        /// 添加所需的动画器参数到动画器参数列表（如果存在的话）
        /// </summary>
        protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter (_jumpingAnimationParameterName, AnimatorControllerParameterType.Bool, out _jumpingAnimationParameter);
			RegisterAnimatorParameter (_doubleJumpingAnimationParameterName, AnimatorControllerParameterType.Bool, out _doubleJumpingAnimationParameter);
			RegisterAnimatorParameter (_hitTheGroundAnimationParameterName, AnimatorControllerParameterType.Bool, out _hitTheGroundAnimationParameter);
		}

        /// <summary>
        /// 在每个循环结束时，向角色的动画师发送跳跃状态
        /// </summary>
        public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _jumpingAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Jumping),_character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _doubleJumpingAnimationParameter, _doubleJumping,_character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool (_animator, _hitTheGroundAnimationParameter, _controller.JustGotGrounded, _character._animatorParameters, _character.RunAnimatorSanityChecks);
		}
	}
}