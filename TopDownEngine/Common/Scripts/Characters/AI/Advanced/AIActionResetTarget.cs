using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 一个Action，将目标设置为null，重置它
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Reset Target")]
	public class AIActionResetTarget : AIAction
	{
        /// <summary>
        /// 我们重新设定了目标
        /// </summary>
        public override void PerformAction()
		{
			_brain.Target = null;
		}
	}
}