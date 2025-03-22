using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 这种能力允许角色在按下蹲下按钮时“蹲下”，从而调整碰撞器的大小
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Crouch")]
	public class CharacterCrouch : CharacterAbility 
	{
        /// 这个方法仅用于在能力的检查器开始时显示一个帮助框文本
        public override string HelpBoxText() { return "这个组件处理蹲伏和爬行行为。在这里，你可以决定蹲下的速度，以及是否对撞机应该调整蹲下时的大小（例如，爬进隧道）。如果应该，请在这里设置它的新大小。"; }

		public enum InputModes { Pressed, Toggle }
		
		/// if this is true, the character is in ForcedCrouch mode. A CrouchZone or an AI script can do that.
		[MMReadOnly]
		[Tooltip("如果这个条件为真，那么角色处于强制蹲伏模式。一个蹲伏区域或者AI脚本可以实现这一点")]
		public bool ForcedCrouch = false;

		[Header("Input输入")] 
		/// the selected input mode. Pressed will require you keep the button pressed to remain crouched, while Toggle will require you to press once to crouch, and once to get back up
		[Tooltip("选择的输入模式。按下将要求你持续按住按钮以保持蹲伏状态，而切换模式则需要你按一次蹲下，再按一次站起来。")]
		public InputModes InputMode = InputModes.Pressed;
		
		[Header("Crawl爬行")]
		/// if this is set to false, the character won't be able to crawl, just to crouch
		[Tooltip("如果这个设置为假，角色将无法爬行，只能蹲下")]
		public bool CrawlAuthorized = true;
		/// the speed of the character when it's crouching
		[Tooltip("角色蹲下时的速度")]
		public float CrawlSpeed = 4f;

		[Space(10)]	
		[Header("Crouching蹲下")]
		/// if this is true, the collider will be resized when crouched
		[Tooltip("如果这个条件为真，那么在蹲下时碰撞体将被重新调整大小")]
		public bool ResizeColliderWhenCrouched = false;
		/// if this is true, the collider will be vertically translated on resize, this can avoid your controller getting teleported into the ground if its center isn't at its y:0
		[Tooltip("如果这个条件为真，那么在调整大小时碰撞体将在垂直方向上进行平移。这可以避免控制器的中心不在y:0位置时被传送到地下的情况")]
		[MMCondition("ResizeColliderWhenCrouched", true)]
		public bool TranslateColliderOnCrouch = false;
		/// the size to apply to the collider when crouched (if ResizeColliderWhenCrouched is true, otherwise this will be ignored)
		[Tooltip("蹲下时应用于碰撞体的大小（如果ResizeColliderWhenCrouched为真，否则此设置将被忽略）")]
		public float CrouchedColliderHeight = 1.25f;

		[Space(10)]	
		[Header("Offset偏移")]
		/// a list of objects to offset when crouching
		[Tooltip("蹲伏时要偏移的对象列表")]
		public List<GameObject> ObjectsToOffset;
		/// the offset to apply to objects when crouching
		[Tooltip("在蹲伏时应用于对象的偏移量")]
		public Vector3 OffsetCrouch;
		/// the offset to apply to objects when crouching AND moving
		[Tooltip("在蹲下和移动时应用于对象的偏移量")]
		public Vector3 OffsetCrawl;
		/// the speed at which to offset objects
		[Tooltip("使物体偏移的速度")]
		public float OffsetSpeed = 5f;

		/// whether or not the character is in a tunnel right now and can't get up
		[MMReadOnly]
		[Tooltip("不管角色现在是不是在隧道里，站不起来")]
		public bool InATunnel;

		protected List<Vector3> _objectsToOffsetOriginalPositions;
		protected const string _crouchingAnimationParameterName = "Crouching";
		protected const string _crawlingAnimationParameterName = "Crawling";
		protected int _crouchingAnimationParameter;
		protected int _crawlingAnimationParameter;
		protected bool _crouching = false;
		protected CharacterRun _characterRun;

        /// <summary>
        /// 在Start（）中，我们将隧道标志设置为false
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			InATunnel = false;
			_characterRun = _character.FindAbility<CharacterRun>();

			// we store our objects to offset's initial positions
			if (ObjectsToOffset.Count > 0)
			{
				_objectsToOffsetOriginalPositions = new List<Vector3> ();
				foreach(GameObject go in ObjectsToOffset)
				{
					if (go != null)
					{
						_objectsToOffsetOriginalPositions.Add(go.transform.localPosition);
					}					
				}
			}
		}

        /// <summary>
        /// 每一帧，我们都会检查自己是否蹲着，是否应该蹲着
        /// </summary>
        public override void ProcessAbility()
		{
			base.ProcessAbility();
			HandleForcedCrouch();
			DetermineState ();
			 
			if (InputMode != InputModes.Toggle)
			{
				CheckExitCrouch();
			}
			
			OffsetObjects ();
		}

        /// <summary>
        /// 如果我们被迫蹲下，我们就蹲下
        /// </summary>
        protected virtual void HandleForcedCrouch()
		{
			if (ForcedCrouch && (_movement.CurrentState != CharacterStates.MovementStates.Crouching) && (_movement.CurrentState != CharacterStates.MovementStates.Crawling))
			{
				Crouch();
			}
		}

        /// <summary>
        /// 在能力循环的开始，我们检查我们是否按下了。如果是，我们叫克劳奇（）
        /// </summary>
        protected override void HandleInput()
		{			
			base.HandleInput ();

			switch (InputMode)
			{
				case InputModes.Pressed:
                    // 蹲伏检测：如果玩家按下“下”键，如果角色被接地，并且蹲伏动作被启用
                    if (_inputManager.CrouchButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)		
					{
						Crouch();
					}
					break;
				case InputModes.Toggle:
                    // 蹲伏检测：如果玩家按下“下”键，如果角色被接地，并且蹲伏动作被启用
                    if (_inputManager.CrouchButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)		
					{
						if (_crouching)
						{
							CheckForTunnel();
						}
						else
						{
							Crouch();
						}
					}
					break;
			}
		}

        /// <summary>
        /// 开始强制蹲伏
        /// </summary>
        public virtual void StartForcedCrouch()
		{
			ForcedCrouch = true;
			_crouching = true;
		}

        /// <summary>
        /// 停止强制蹲伏
        /// </summary>
        public virtual void StopForcedCrouch()
		{
			ForcedCrouch = false;
			_crouching = false;
		}

        /// <summary>
        /// 如果我们按下，我们检查我们是否可以蹲下或爬行，并相应地改变状态
        /// </summary>
        protected virtual void Crouch()
		{
			if (!AbilityAuthorized// 如果这种能力是不允许的
                || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal)// or if we're not in our normal stance
			    || (!_controller.Grounded))// 或者我们被禁足了
                                           // 我们什么都不做，然后离开
            {
                return;
			}

            // 如果这是我们第一次来这里，我们会发出声音
            if ((_movement.CurrentState != CharacterStates.MovementStates.Crouching) && (_movement.CurrentState != CharacterStates.MovementStates.Crawling))
			{
                // 我们播放蹲下开始的声音
                PlayAbilityStartFeedbacks();
				PlayAbilityStartSfx();
				PlayAbilityUsedSfx();
			}

			if (_movement.CurrentState == CharacterStates.MovementStates.Running)
			{
				_characterRun.RunStop();
			}

			_crouching = true;

            // 我们将角色的状态设置为蹲伏，如果它也在移动，我们将其设置为爬行
            _movement.ChangeState(CharacterStates.MovementStates.Crouching);
			if ( (Mathf.Abs(_horizontalInput) > 0) && (CrawlAuthorized) )
			{
				_movement.ChangeState(CharacterStates.MovementStates.Crawling);
			}

            // 我们调整对撞机的大小以匹配角色的新形状（通常是蹲着的时候更小）。
            if (ResizeColliderWhenCrouched)
			{
				_controller.ResizeColliderHeight(CrouchedColliderHeight, TranslateColliderOnCrouch);		
			}

            // 我们改变角色的速度
            if (_characterMovement != null)
			{
				_characterMovement.MovementSpeed = CrawlSpeed;
			}

            // 如果我们不能爬行，我们就不能移动
            if (!CrawlAuthorized)
			{
				_characterMovement.MovementSpeed = 0f;
			}
		}

		protected virtual void OffsetObjects ()
		{
            // 我们移动所有我们想移动的物体
            if (ObjectsToOffset.Count > 0)
			{
				for (int i = 0; i < ObjectsToOffset.Count; i++)
				{
					Vector3 newOffset = Vector3.zero;
					if (_movement.CurrentState == CharacterStates.MovementStates.Crouching)
					{
						newOffset = OffsetCrouch;
					}
					if (_movement.CurrentState == CharacterStates.MovementStates.Crawling)
					{
						newOffset = OffsetCrawl;
					}
					if (ObjectsToOffset[i] != null)
					{
						ObjectsToOffset[i].transform.localPosition = Vector3.Lerp(ObjectsToOffset[i].transform.localPosition, _objectsToOffsetOriginalPositions[i] + newOffset, Time.deltaTime * OffsetSpeed);
					}					
				}
			}
		}

        /// <summary>
        /// 运行每一帧来检查我们是否应该从蹲下切换到爬行或其他方式
        /// </summary>
        protected virtual void DetermineState()
		{
			if ((_movement.CurrentState == CharacterStates.MovementStates.Crouching) || (_movement.CurrentState == CharacterStates.MovementStates.Crawling))
			{
				if ( (_controller.CurrentMovement.magnitude > 0) && (CrawlAuthorized) )
				{
					_movement.ChangeState(CharacterStates.MovementStates.Crawling);
				}
				else
				{
					_movement.ChangeState(CharacterStates.MovementStates.Crouching);
				}
			}
		}

        /// <summary>
        /// 每一帧，我们检查是否应该退出蹲伏（或爬行）状态
        /// </summary>
        protected virtual void CheckExitCrouch()
		{
            // 如果我们现在停飞
            if ( (_movement.CurrentState == CharacterStates.MovementStates.Crouching)
			     || (_movement.CurrentState == CharacterStates.MovementStates.Crawling)
			     || _crouching)
			{	
				if (_inputManager == null)
				{
					if (!ForcedCrouch)
					{
						ExitCrouch();
					}
					return;
				}

                // 但我们不再施压了，或者说我们不再被禁足了
                if ( (!_controller.Grounded) 
				     || ((_movement.CurrentState != CharacterStates.MovementStates.Crouching) 
				         && (_movement.CurrentState != CharacterStates.MovementStates.Crawling)
				         && (_inputManager.CrouchButton.IsOff || _inputManager.CrouchButton.IsUp) && (!ForcedCrouch))
				     || ((_inputManager.CrouchButton.IsOff || _inputManager.CrouchButton.IsUp) && (!ForcedCrouch)))
				{
					CheckForTunnel();
				}
			}
		}

		protected virtual void CheckForTunnel()
		{
            // 我们在上面进行光线投射，看看我们是否有足够的空间回到正常的大小
            InATunnel = !_controller.CanGoBackToOriginalSize();

            // 如果角色不在隧道中，我们可以恢复到正常大小
            if (!InATunnel)
			{
				ExitCrouch();
			}
		}

        /// <summary>
        /// 将角色返回到正常姿态
        /// </summary>
        protected virtual void ExitCrouch()
		{
			_crouching = false;

            // 我们回到正常的步行速度
            if (_characterMovement != null)
			{
				_characterMovement.ResetSpeed();
			}

            // 我们播放退出的声音
            StopAbilityUsedSfx();
			PlayAbilityStopSfx();
			StopStartFeedbacks();
			PlayAbilityStopFeedbacks();

            // 我们回到Idle状态并重置collider的大小
            if ((_movement.CurrentState == CharacterStates.MovementStates.Crawling) ||
			    (_movement.CurrentState == CharacterStates.MovementStates.Crouching))
			{
				_movement.ChangeState(CharacterStates.MovementStates.Idle);    
			}
            
			_controller.ResetColliderSize();
		}

        /// <summary>
        /// 添加所需的动画器参数到动画器参数列表（如果存在的话）
        /// </summary>
        protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter (_crouchingAnimationParameterName, AnimatorControllerParameterType.Bool, out _crouchingAnimationParameter);
			RegisterAnimatorParameter (_crawlingAnimationParameterName, AnimatorControllerParameterType.Bool, out _crawlingAnimationParameter);
		}

        /// <summary>
        /// 在能力循环结束时，我们将当前的蹲伏和爬行状态发送给动画师
        /// </summary>
        public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _crouchingAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Crouching), _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(_animator,_crawlingAnimationParameter,(_movement.CurrentState == CharacterStates.MovementStates.Crawling), _character._animatorParameters, _character.RunAnimatorSanityChecks);
		}
	}
}