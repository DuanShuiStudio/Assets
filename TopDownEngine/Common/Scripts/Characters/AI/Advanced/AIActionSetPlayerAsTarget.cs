using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 用于将当前玩家角色设置为目标的AIACtion
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Set Player As Target")]
	public class AIActionSetPlayerAsTarget : AIAction
	{
		public bool OnlyRunOnce = true;
        
		protected bool _alreadyRan = false;

        /// <summary>
        /// 在init中，我们初始化我们的动作
        /// </summary>
        public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			_alreadyRan = false;
		}

        /// <summary>
        /// 设定一个新目标
        /// </summary>
        public override void PerformAction()
		{
			if (OnlyRunOnce && _alreadyRan)
			{
				return;
			}

			if (LevelManager.HasInstance && LevelManager.Instance.Players != null && LevelManager.Instance.Players[0] != null)
			{
				_brain.Target = LevelManager.Instance.Players[0].transform;
			}
		}

        /// <summary>
        /// 在进入状态时，我们重置我们的标志
        /// </summary>
        public override void OnEnterState()
		{
			base.OnEnterState();
			_alreadyRan = false;
		}
	}
}