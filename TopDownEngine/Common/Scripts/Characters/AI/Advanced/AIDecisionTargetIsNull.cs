using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 如果Brain的当前目标为空，该决策将返回true，否则返回false
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AI Decision Target Is Null")]
	public class AIDecisionTargetIsNull : AIDecision
	{
        /// <summary>
        /// 在决定上，我们检查目标是否为空
        /// </summary>
        /// <returns></returns>
        public override bool Decide()
		{
			return CheckIfTargetIsNull();
		}

        /// <summary>
        /// 如果Brain的Target为空，则返回true
        /// </summary>
        /// <returns></returns>
        protected virtual bool CheckIfTargetIsNull()
		{
			if (_brain.Target == null)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}