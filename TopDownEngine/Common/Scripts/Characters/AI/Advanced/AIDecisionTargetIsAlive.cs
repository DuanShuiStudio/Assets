using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 如果大脑当前的目标还活着，这个决定将返回true，否则返回false
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AI Decision Target Is Alive")]
	public class AIDecisionTargetIsAlive : AIDecision
	{
		protected Character _character;

        /// <summary>
        /// 在判定中我们检查目标是死是活
        /// </summary>
        /// <returns></returns>
        public override bool Decide()
		{
			return CheckIfTargetIsAlive();
		}

		/// <summary>
		/// 如果大脑的目标是活的返回true，否则返回false
		/// </summary>
		/// <returns></returns>
		protected virtual bool CheckIfTargetIsAlive()
		{
			if (_brain.Target == null)
			{
				return false;
			}

			_character = _brain.Target.gameObject.MMGetComponentNoAlloc<Character>();
			if (_character != null)
			{
				if (_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead)
				{
					return false;
				}
				else
				{ 
					return true;
				}
			}            

			return false;
		}
	}
}