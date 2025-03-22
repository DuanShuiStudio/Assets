using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此组件添加到角色中，它将能够运行
    /// 动画参数 : Running
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Run")] 
	public class CharacterRun : CharacterAbility
	{
        /// 此方法仅用于在功能检查器的开头显示帮助框文本
        public override string HelpBoxText() { return "这个组件允许你的角色在按下运行按钮时改变速度（在这里定义）。"; }

		[Header("Speed速度")]

		/// the speed of the character when it's running
		[Tooltip("角色奔跑时的速度")]
		public float RunSpeed = 16f;

		[Header("AutoRun自动奔跑")]

		/// whether or not run should auto trigger if you move the joystick far enough
		[Tooltip("当你移动操纵杆足够远，是否应该运行自动奔跑，")]
		public bool AutoRun = false;
		/// the input threshold on the joystick (normalized)
		[Tooltip("操纵杆上的输入阈值（标准化）")]
		public float AutoRunThreshold = 0.6f;

		protected const string _runningAnimationParameterName = "Running";
		protected int _runningAnimationParameter;
		protected bool _runningStarted = false;

        /// <summary>
        /// 在每个循环开始时，我们检查是否按下或释放了运行按钮
        /// </summary>
        protected override void HandleInput()
		{
			if (AutoRun)
			{
				if (_inputManager.PrimaryMovement.magnitude > AutoRunThreshold)
				{
					_inputManager.RunButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed);
				}
			}

			if (_inputManager.RunButton.IsDown || _inputManager.RunButton.IsPressed)
			{
				RunStart();
			}

			if (_runningStarted)
			{
				if (_inputManager.RunButton.IsUp || _inputManager.RunButton.IsOff)
				{
					RunStop();
				}
				else
				{
					if (AutoRun)
					{
						if (_inputManager.PrimaryMovement.magnitude <= AutoRunThreshold)
						{
							_inputManager.RunButton.State.ChangeState(MMInput.ButtonStates.ButtonUp);
							RunStop();
						}
					}
				}          
			}
		}

        /// <summary>
        /// 每一帧我们都要确保不退出运行状态
        /// </summary>
        public override void ProcessAbility()
		{
			base.ProcessAbility();
			HandleRunningExit();
		}

        /// <summary>
        /// 检查是否应该退出奔跑状态
        /// </summary>
        protected virtual void HandleRunningExit()
		{
			if (_condition.CurrentState != CharacterStates.CharacterConditions.Normal)
			{
				StopAbilityUsedSfx();
			}
			if (_movement.CurrentState == CharacterStates.MovementStates.Running && AbilityInProgressSfx != null && _abilityInProgressSfx == null)
			{
				PlayAbilityUsedSfx();
			}

            // 如果我们在奔跑而不是接地，我们的状态就会变成Falling
            if (!_controller.Grounded
			    && (_condition.CurrentState == CharacterStates.CharacterConditions.Normal)
			    && (_movement.CurrentState == CharacterStates.MovementStates.Running))
			{
				_movement.ChangeState(CharacterStates.MovementStates.Falling);
				StopFeedbacks();
				StopSfx ();
			}

            // 如果我们动作不够快，我们就会回到Idle状态
            if ((Mathf.Abs(_controller.CurrentMovement.magnitude) < RunSpeed / 10) && (_movement.CurrentState == CharacterStates.MovementStates.Running))
			{
				_movement.ChangeState (CharacterStates.MovementStates.Idle);
				StopFeedbacks();
				StopSfx ();
			}
			
			if (!_controller.Grounded && _abilityInProgressSfx != null)
			{
				StopFeedbacks();
				StopSfx ();
			}
		}

        /// <summary>
        /// 使角色开始奔跑。
        /// </summary>
        public virtual void RunStart()
		{		
			if ( !AbilityAuthorized // 如果这种能力是不允许的
                 || (!_controller.Grounded) // 或者我们没有被禁足
                 || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal) // 或者如果我们不是在正常情况下
                 || (_movement.CurrentState != CharacterStates.MovementStates.Walking)) // 或者如果我们不走
            {
                // 我们什么都不做，然后离开
                return;
			}

            // 如果玩家按下奔跑按钮，如果我们在地面上，没有蹲下，我们可以自由移动，
            // 然后我们改变控制器参数中的移动速度。
            if (_characterMovement != null)
			{
				_characterMovement.MovementSpeed = RunSpeed;
			}

            // 如果我们还没有跑起来，我们就会触发我们的声音
            if (_movement.CurrentState != CharacterStates.MovementStates.Running)
			{
				PlayAbilityStartSfx();
				PlayAbilityUsedSfx();
				PlayAbilityStartFeedbacks();
				_runningStarted = true;
			}

			_movement.ChangeState(CharacterStates.MovementStates.Running);
		}

        /// <summary>
        /// 使角色停止奔跑。
        /// </summary>
        public virtual void RunStop()
		{
			if (_runningStarted)
			{
                // 如果跑按钮被释放，我们恢复到步行速度。
                if ((_characterMovement != null))
				{
					_characterMovement.ResetSpeed();
					_movement.ChangeState(CharacterStates.MovementStates.Idle);
				}
				StopFeedbacks();
				StopSfx();
				_runningStarted = false;
			}            
		}

        /// <summary>
        /// 停止所有奔跑反馈
        /// </summary>
        protected virtual void StopFeedbacks()
		{
			if (_startFeedbackIsPlaying)
			{
				StopStartFeedbacks();
				PlayAbilityStopFeedbacks();
			}
		}

        /// <summary>
        /// 停止所有奔跑声音
        /// </summary>
        protected virtual void StopSfx()
		{
			StopAbilityUsedSfx();
			PlayAbilityStopSfx();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			if (AutoRun)
			{
				RunStop();
				if ((_inputManager != null) && (_inputManager.PrimaryMovement.magnitude > AutoRunThreshold))
				{
					_inputManager.RunButton.State.ChangeState(MMInput.ButtonStates.Off);
				}
			}
		}

        /// <summary>
        /// 添加所需的动画器参数到动画器参数列表（如果存在的话）
        /// </summary>
        protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter (_runningAnimationParameterName, AnimatorControllerParameterType.Bool, out _runningAnimationParameter);
		}

        /// <summary>
        /// 在每个循环结束时，我们将运行状态发送给角色的动画师
        /// </summary>
        public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _runningAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Running),_character._animatorParameters, _character.RunAnimatorSanityChecks);
		}
	}
}