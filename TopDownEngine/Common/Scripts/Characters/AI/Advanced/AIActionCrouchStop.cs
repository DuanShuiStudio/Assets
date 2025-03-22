using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 这个动作可以迫使角色停止蹲伏
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Crouch Stop")]
	//[RequireComponent(typeof(CharacterCrouch))]
	public class AIActionCrouchStop : AIAction
	{
		protected CharacterCrouch _characterCrouch;
		protected Character _character;

        /// <summary>
        /// 抓住依赖性
        /// </summary>
        public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			_character = this.gameObject.GetComponentInParent<Character>();
			_characterCrouch = _character?.FindAbility<CharacterCrouch>();
		}

        /// <summary>
        /// 在PerformAction中，我们停止蹲伏
        /// </summary>
        public override void PerformAction()
		{
			if ((_character == null) || (_characterCrouch == null))
			{
				return;
			}

			if ((_character.MovementState.CurrentState == CharacterStates.MovementStates.Crouching)
			    || (_character.MovementState.CurrentState == CharacterStates.MovementStates.Crawling))
			{
				_characterCrouch.StopForcedCrouch();
			}
		}
	}
}