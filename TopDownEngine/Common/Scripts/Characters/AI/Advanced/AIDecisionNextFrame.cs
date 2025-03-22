using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 当进入该decision所在的状态时，该decision将返回true。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AI Decision Next Frame")]
	public class AIDecisionNextFrame : AIDecision
	{
        /// <summary>
        /// 我们在决定时返回true
        /// </summary>
        /// <returns></returns>
        public override bool Decide()
		{
			return true;
		}
	}
}