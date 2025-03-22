using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 如果角色在这一帧被击中，或者在达到指定的命中次数之后，这个决定返回true。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AI Decision Hit")]
	//[RequireComponent(typeof(Health))]
	public class AIDecisionHit : AIDecision
	{
		/// The number of hits required to return true
		[Tooltip("返回true所需的命中次数")]
		public int NumberOfHits = 1;

		protected int _hitCounter;
		protected Health _health;
		protected Character _character;

        /// <summary>
        /// 在init中，我们获取Health组件
        /// </summary>
        public override void Initialization()
		{
			_character = this.gameObject.GetComponentInParent<Character>();
			_health = (_character != null) ? _character.CharacterHealth : this.gameObject.GetComponent<Health>();
			if (_health == null)
			{
				_health = this.gameObject.GetComponentInParent<Health>();
			}
			_hitCounter = 0;
		}

        /// <summary>
        /// 在决定，我们检查我们是否被击中
        /// </summary>
        /// <returns></returns>
        public override bool Decide()
		{
			return EvaluateHits();
		}

        /// <summary>
        /// 检查我们是否被击中了足够的次数
        /// </summary>
        /// <returns></returns>
        protected virtual bool EvaluateHits()
		{
			return (_hitCounter >= NumberOfHits);
		}

        /// <summary>
        /// 在EnterState上，重置命中计数器
        /// </summary>
        public override void OnEnterState()
		{
			base.OnEnterState();
			_hitCounter = 0;
		}

        /// <summary>
        /// 在退出状态下，重置命中计数器
        /// </summary>
        public override void OnExitState()
		{
			base.OnExitState();
			_hitCounter = 0;
		}

        /// <summary>
        /// 当我们被击中时，我们增加我们的击中计数器
        /// </summary>
        protected virtual void OnHit()
		{
			_hitCounter++;
		}

        /// <summary>
        /// 获取健康组件并开始监听OnHit事件
        /// </summary>
        protected virtual void OnEnable()
		{
			if (_health == null)
			{
				Initialization();
			}

			if (_health != null)
			{
				_health.OnHit += OnHit;
			}
		}

        /// <summary>
        /// 停止监听OnHit事件
        /// </summary>
        protected virtual void OnDisable()
		{
			if (_health != null)
			{
				_health.OnHit -= OnHit;
			}
		}
	}
}