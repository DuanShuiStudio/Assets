using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 如果Brain的当前目标正对着这个角色，这个决定将返回true。是的，这是鬼特有的。但是现在你也可以用它了！
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AI Decision Target Facing AI 2D")]
	public class AIDecisionTargetFacingAI2D : AIDecision
	{
		protected CharacterOrientation2D _orientation2D;

        /// <summary>
        /// 在决定时，我们检查目标是否正对着我们
        /// </summary>
        /// <returns></returns>
        public override bool Decide()
		{
			return EvaluateTargetFacingDirection();
		}

        /// <summary>
        /// 如果大脑的目标正对着我们返回true（这需要目标有一个Character组件）
        /// </summary>
        /// <returns></returns>
        protected virtual bool EvaluateTargetFacingDirection()
		{
			if (_brain.Target == null)
			{
				return false;
			}

			_orientation2D = _brain.Target.gameObject.GetComponent<Character>()?.FindAbility<CharacterOrientation2D>();
			if (_orientation2D != null)
			{
				if (_orientation2D.IsFacingRight && (this.transform.position.x > _orientation2D.transform.position.x))
				{
					return true;
				}
				if (!_orientation2D.IsFacingRight && (this.transform.position.x < _orientation2D.transform.position.x))
				{
					return true;
				}
			}            

			return false;
		}
	}
}