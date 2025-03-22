using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// AIACtion用来让AI开始运行
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Run Start")]
	public class AIActionRunStart : AIAction
	{
        /// 如果为true，则此操作将在一个状态中只运行一次（此标志将在状态退出时重置）。
        public bool OnlyRunOnce = true;
        
		protected Character _character;
		protected CharacterRun _characterRun;
		protected bool _alreadyRan = false;

        /// <summary>
        /// 在init中，我们获取Run能力
        /// </summary>
        public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			_character = this.gameObject.GetComponentInParent<Character>();
			_characterRun = _character?.FindAbility<CharacterRun>();
		}

        /// <summary>
        /// 开始运行的请求
        /// </summary>
        public override void PerformAction()
		{
			if (OnlyRunOnce && _alreadyRan)
			{
				return;
			}
			_characterRun.RunStart();
			_alreadyRan = true;
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