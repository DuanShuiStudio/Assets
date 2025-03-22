using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// AIACtion曾经让AI杀死自己
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Self Destruct")]
	public class AIActionSelfDestruct : AIAction
	{
		public bool OnlyRunOnce = true;
        
		protected Character _character;
		protected Health _health;
		protected bool _alreadyRan = false;

        /// <summary>
        /// 在init中，我们获取Health
        /// </summary>
        public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			_character = this.gameObject.GetComponentInParent<Character>();
			_health = _character.CharacterHealth;
		}

        /// <summary>
        /// 杀死AI
        /// </summary>
        public override void PerformAction()
		{
			if (OnlyRunOnce && _alreadyRan)
			{
				return;
			}
			_health.Kill();
			_brain.BrainActive = false;
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