using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将目标的最后已知位置设置为新目标的Action
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Set Last Known Position As Target")]
	public class AIActionSetLastKnownPositionAsTarget : AIAction
	{
		protected Transform _targetTransform;

        /// <summary>
        /// 在init中，我们准备一个新的游戏对象作为新的目标
        /// </summary>
        public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			GameObject newGo = new GameObject();
			newGo.name = "AIActionSetLastKnownPositionAsTarget_target";
			newGo.transform.SetParent(null);
			_targetTransform = newGo.transform;
		}

        /// <summary>
        /// 我们把目标移到最后一个已知位置，并把它分配给大脑
        /// </summary>
        public override void PerformAction()
		{
			_targetTransform.position = _brain._lastKnownTargetPosition;
			_brain.Target = _targetTransform;
		}
	}
}