using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
	[AddComponentMenu("TopDown Engine/Character/Abilities/Character Damage Dash 2D")]
	public class CharacterDamageDash2D : CharacterDash2D
	{
		[Header("Damage Dash冲刺伤害")]
		/// the DamageOnTouch object to activate when dashing (usually placed under the Character's model, will require a Collider2D of some form, set to trigger
		[Tooltip("当冲刺时激活的DamageOnTouch对象（通常放置在角色模型下，需要一个设置为触发器的Collider2D")]
		public DamageOnTouch TargetDamageOnTouch;

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
	}
}