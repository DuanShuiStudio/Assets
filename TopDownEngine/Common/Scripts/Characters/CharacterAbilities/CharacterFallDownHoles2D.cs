using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将这个组件添加到角色中，它将使你的角色在2D中陷入困境
    /// </summary>
    [MMHiddenProperties("AbilityStartFeedbacks")]
	//[RequireComponent(typeof(TopDownController2D))]
	[AddComponentMenu("TopDown Engine/Character/Abilities/Character Fall Down Holes 2D")]
	public class CharacterFallDownHoles2D : CharacterAbility
	{
		/// the feedback to play when falling
		[Tooltip("坠落时播放的反馈")]
		public MMFeedbacks FallingFeedback;

		protected Collider2D _holesTest;
		protected const string _fallingDownHoleAnimationParameterName = "FallingDownHole";
		protected int _fallingDownHoleAnimationParameter;

        /// <summary>
        /// 在加工能力方面，我们检查是否有漏洞
        /// </summary>
        public override void ProcessAbility()
		{
			base.ProcessAbility();
			CheckForHoles();
		}

        /// <summary>
        /// 如果我们在自己的角色之下发现了漏洞，我们就杀死自己的角色
        /// </summary>
        protected virtual void CheckForHoles()
		{
			if (!AbilityAuthorized)
			{
				return;
			}
            
			if (_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead)
			{
				return;
			}

			if (_controller2D.OverHole && !_controller2D.Grounded)
			{ 
				if ((_movement.CurrentState != CharacterStates.MovementStates.Jumping)
				    && (_movement.CurrentState != CharacterStates.MovementStates.Dashing)
				    && (_condition.CurrentState != CharacterStates.CharacterConditions.Dead))
				{
					_movement.ChangeState(CharacterStates.MovementStates.FallingDownHole);
					FallingFeedback?.PlayFeedbacks(this.transform.position);
					PlayAbilityStartFeedbacks();
					_health.Kill();
				}
			}
		}

        /// <summary>
        /// 添加所需的动画器参数到动画器参数列表（如果存在的话）
        /// </summary>
        protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter(_fallingDownHoleAnimationParameterName, AnimatorControllerParameterType.Bool, out _fallingDownHoleAnimationParameter);
		}

        /// <summary>
        /// 在每个循环结束时，我们将运行状态发送给角色的动画师
        /// </summary>
        public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _fallingDownHoleAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.FallingDownHole), _character._animatorParameters, _character.RunAnimatorSanityChecks);
		}
	}
}