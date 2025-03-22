using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 顾名思义，什么也不做的动作。就在那里等着。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Do Nothing")]
	public class AIActionDoNothing : AIAction
	{
        /// <summary>
        /// 对于PerformAction，我们什么都不做
        /// </summary>
        public override void PerformAction()
		{

		}
	}
}