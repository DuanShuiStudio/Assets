using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此组件添加到角色中，它将能够被击晕。要击晕一个角色，只需调用其stun或StunFor方法。您将在该组件检查器的底部找到测试按钮。你也可以使用StunZones来击晕你的角色。
    /// 动画参数 : Stunned (bool)
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Stun")] 
	public class CharacterStun : CharacterAbility
	{
        /// 此方法仅用于在功能检查器的开头显示帮助框文本
        public override string HelpBoxText() { return "将此组件添加到角色中，它将能够被击晕。要击晕一个角色，只需调用其stun或StunFor方法。您将在该组件检查器的底部找到测试按钮。你也可以使用StunZones来击晕你的角色。"; }
        
		[Header("IK反向动力学")]
		/// a weapon IK to pilot when stunned
		[Tooltip("a weapon IK to pilot when stunned在昏迷时控制的武器IK")]
		public WeaponIK BoundWeaponIK;
		/// whether or not to detach the left hand of the character from IK when stunned
		[Tooltip("是否在昏迷时将角色的左手从IK中分离")]
		public bool DetachLeftHand = false;
		/// whether or not to detach the right hand of the character from IK when stunned
		[Tooltip("是否在昏迷时将角色的右手从IK中分离")]
		public bool DetachRightHand = false;
        
		[Header("Weapon Models武器模型")]
		/// whether or not to disable the weapon model when stunned
		[Tooltip("是否在眩晕时禁用武器模型")]
		public bool DisableAimWeaponModelAtTargetDuringStun = false;
		/// the list of weapon models to disable when stunned
		[Tooltip("眩晕时要禁用的武器型号列表")]
		public List<WeaponModel> WeaponModels;
        
		[Header("Tests测试")]
        /// 一个测试按钮来击晕这个角色
        [MMInspectorButton("Stun眩晕")]
		public bool StunButton;
        /// 一个测试按钮来退出这个角色的眩晕状态
        [MMInspectorButton("ExitStun")]
		public bool ExitStunButton;
        
		protected const string _stunnedAnimationParameterName = "Stunned";
		protected int _stunnedAnimationParameter;
		protected Coroutine _stunCoroutine;
		protected CharacterStates.CharacterConditions _previousCondition;

        /// <summary>
        /// 击晕角色
        /// </summary>
        public virtual void Stun()
		{
			if ((_previousCondition != CharacterStates.CharacterConditions.Stunned) && (_condition.CurrentState != CharacterStates.CharacterConditions.Stunned))
			{
				_previousCondition = _condition.CurrentState;
			} 		
			_condition.ChangeState(CharacterStates.CharacterConditions.Stunned);
			_controller.SetMovement(Vector3.zero);
			AbilityStartFeedbacks?.PlayFeedbacks();
			DetachIK();
		}

        /// <summary>
        /// 在指定的时间内击晕角色
        /// </summary>
        /// <param name="duration"></param>
        public virtual void StunFor(float duration)
		{
			if (_stunCoroutine != null)
			{
				StopCoroutine(_stunCoroutine);
			}
			_stunCoroutine = StartCoroutine(StunCoroutine(duration));
		}

        /// <summary>
        /// 退出眩晕，将状态重置为前一个状态
        /// </summary>
        public virtual void ExitStun()
		{
			if (_condition.CurrentState != CharacterStates.CharacterConditions.Stunned)
			{
				return;
			}
			
			AbilityStopFeedbacks?.PlayFeedbacks();
			_condition.ChangeState(_previousCondition);
			AttachIK();
		}

        /// <summary>
        /// 击晕角色，等待指定的持续时间，然后退出昏迷
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        protected virtual IEnumerator StunCoroutine(float duration)
		{
			Stun();
			yield return MMCoroutine.WaitFor(duration);
			ExitStun();
		}

        /// <summary>
        /// 分离IK反向动力学
        /// </summary>
        protected virtual void DetachIK()
		{
			if (DetachLeftHand) { BoundWeaponIK.AttachLeftHand = false; }
			if (DetachRightHand) { BoundWeaponIK.AttachRightHand = false; }
			if (DisableAimWeaponModelAtTargetDuringStun)
			{
				foreach(WeaponModel model in WeaponModels)
				{
					model.AimWeaponModelAtTarget = false;
				}
			}
		}

        /// <summary>
        /// 高度IK反向动力学
        /// </summary>
        protected virtual void AttachIK()
		{
			if (DetachLeftHand) { BoundWeaponIK.AttachLeftHand = true; }
			if (DetachRightHand) { BoundWeaponIK.AttachRightHand = true; }
			if (DisableAimWeaponModelAtTargetDuringStun)
			{
				foreach (WeaponModel model in WeaponModels)
				{
					model.AimWeaponModelAtTarget = true;
				}
			}
		}

        /// <summary>
        /// 添加所需的动画器参数到动画器参数列表（如果存在的话）
        /// </summary>
        protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter (_stunnedAnimationParameterName, AnimatorControllerParameterType.Bool, out _stunnedAnimationParameter);
		}

        /// <summary>
        /// 在每个循环结束时，我们将运行状态发送给角色的动画师
        /// </summary>
        public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _stunnedAnimationParameter, (_condition.CurrentState == CharacterStates.CharacterConditions.Stunned),_character._animatorParameters, _character.RunAnimatorSanityChecks);
		}
	}
}