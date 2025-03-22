using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 如果满足指定的运行状况条件，此决策将返回true。你可以让它低于，严格低于，等于，高于或严格高于指定值。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AI Decision Health")]
	//[RequireComponent(typeof(Health))]
	public class AIDecisionHealth : AIDecision
	{
        /// 不同的比较模式
        public enum ComparisonModes { StrictlyLowerThan, LowerThan, Equals, GreaterThan, StrictlyGreaterThan }
		/// the comparison mode with which we'll evaluate the HealthValue
		[Tooltip("我们将用来评估HealthValue的比较模式")]
		public ComparisonModes TrueIfHealthIs;
		/// the Health value to compare to
		[Tooltip("要进行比较的生命值")]
		public int HealthValue;
		/// whether we want this comparison to be done only once or not
		[Tooltip("我们是否希望这个比较只做一次")]
		public bool OnlyOnce = true;

		protected Health _health;
		protected Character _character;
		protected bool _once = false;

        /// <summary>
        /// 在init中，我们获取Health组件
        /// </summary>
        public override void Initialization()
		{
			_character = this.gameObject.GetComponentInParent<Character>();
			if (_brain != null)
			{
				_health = (_character != null) ? _character.CharacterHealth : this.gameObject.GetComponent<Health>();
			}
		}

        /// <summary>
        /// 在决定中，我们评估我们当前的健康水平
        /// </summary>
        /// <returns></returns>
        public override bool Decide()
		{
			return EvaluateHealth();
		}

        /// <summary>
        /// 比较运行状况值，如果满足条件则返回true
        /// </summary>
        /// <returns></returns>
        protected virtual bool EvaluateHealth()
		{
			bool returnValue = false;

			if (OnlyOnce && _once)
			{
				return false;
			}

			if (_health == null)
			{
				Debug.LogWarning("添加了AIDecisionHealth到" + this.gameObject.name + "的 AI大脑, 但这个东西没有生命值组件");
				return false;
			}

			if (!_health.isActiveAndEnabled)
			{
				return false;
			}
            
			if (TrueIfHealthIs == ComparisonModes.StrictlyLowerThan)
			{
				returnValue = (_health.CurrentHealth < HealthValue);
			}

			if (TrueIfHealthIs == ComparisonModes.LowerThan)
			{
				returnValue = (_health.CurrentHealth <= HealthValue);
			}

			if (TrueIfHealthIs == ComparisonModes.Equals)
			{
				returnValue = (_health.CurrentHealth == HealthValue);
			}

			if (TrueIfHealthIs == ComparisonModes.GreaterThan)
			{
				returnValue = (_health.CurrentHealth >= HealthValue);
			}

			if (TrueIfHealthIs == ComparisonModes.StrictlyGreaterThan)
			{
				returnValue = (_health.CurrentHealth > HealthValue);
			}

			if (returnValue)
			{
				_once = true;
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}