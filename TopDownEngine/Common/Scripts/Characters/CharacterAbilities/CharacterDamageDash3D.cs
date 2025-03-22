using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
	[AddComponentMenu("TopDown Engine/Character/Abilities/Character Damage Dash 3D")]
	public class CharacterDamageDash3D : CharacterDash3D
	{
		[Header("Damage Dash冲刺伤害")]
		/// the DamageOnTouch object to activate when dashing (usually placed under the Character's model, will require a Collider2D of some form, set to trigger
		[Tooltip("当冲刺时激活的DamageOnTouch对象（通常放置在角色模型下，需要一个设置为触发器的Collider2D")]
		public DamageOnTouch TargetDamageOnTouch;
        
		protected const string _damageDashingAnimationParameterName = "DamageDashing";
		protected int _damageDashingAnimationParameter;

        /// <summary>
        /// 在初始化时，我们在触摸对象上禁用伤害
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			TargetDamageOnTouch?.gameObject.SetActive(false);
		}

        /// <summary>
        /// 当我们开始冲刺时，我们会激活我们的伤害对象
        /// </summary>
        public override void DashStart()
		{
			base.DashStart();
			TargetDamageOnTouch?.gameObject.SetActive(true);
		}

        /// <summary>
        /// 当我们停止奔跑时，我们禁用了伤害对象
        /// </summary>
        public override void DashStop()
		{
			base.DashStop();
			TargetDamageOnTouch?.gameObject.SetActive(false);
		}

        /// <summary>
        /// 添加所需的动画器参数到动画器参数列表（如果存在的话）
        /// </summary>
        protected override void InitializeAnimatorParameters()
		{
			base.InitializeAnimatorParameters();
			RegisterAnimatorParameter(_damageDashingAnimationParameterName, AnimatorControllerParameterType.Bool, out _damageDashingAnimationParameter);
		}

        /// <summary>
        /// 在每个循环结束时，我们将运行状态发送给角色的动画师
        /// </summary>
        public override void UpdateAnimator()
		{
			base.UpdateAnimator();
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _damageDashingAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Dashing), _character._animatorParameters, _character.RunAnimatorSanityChecks);
			_dashStartedThisFrame = false;
		}
	}
}