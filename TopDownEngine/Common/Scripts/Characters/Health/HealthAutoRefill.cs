using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将这个类添加到一个带有Health类的字符或对象中，它的生命值将根据这里的设置自动恢复
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Health/Health Auto Refill")]
	public class HealthAutoRefill : TopDownMonoBehaviour
	{
        /// 可能的恢复模式：
        /// - 线性模式 : 以每秒一定的速率进行恒定的生命值恢复
        /// - 爆发模式 : 周期性的生命值爆发
        public enum RefillModes { Linear, Bursts }

		[Header("Mode模式")]
		/// the selected refill mode 
		[Tooltip("选择的恢复模式 ")]
		public RefillModes RefillMode;
		/// an optional target Health component to refill
		[Tooltip("一个可选的目标生命组件用于恢复")]
		public Health TargetHealth;

		[Header("Cooldown冷却")]
		/// how much time, in seconds, should pass before the refill kicks in
		[Tooltip("在恢复开始之前应该经过多少秒的时间")]
		public float CooldownAfterHit = 1f;
        
		[Header("Refill Settings生命恢复设置")]
		/// if this is true, health will refill itself when not at full health
		[Tooltip("如果这是真的，生命值将在未达到满值时自行恢复")]
		public bool RefillHealth = true;
		/// the amount of health per second to restore when in linear mode
		[MMEnumCondition("RefillMode", (int)RefillModes.Linear)]
		[Tooltip("在线性模式下，每秒恢复的生命值数量")]
		public float HealthPerSecond;
		/// the amount of health to restore per burst when in burst mode
		[MMEnumCondition("RefillMode", (int)RefillModes.Bursts)]
		[Tooltip("在爆发模式下，每次爆发恢复的生命值数量")]
		public float HealthPerBurst = 5;
		/// the duration between two health bursts, in seconds
		[MMEnumCondition("RefillMode", (int)RefillModes.Bursts)]
		[Tooltip("两次生命爆发之间的持续时间，以秒为单位")]
		public float DurationBetweenBursts = 2f;

		protected Health _health;
		protected float _lastHitTime = 0f;
		protected float _healthToGive = 0f;
		protected float _lastBurstTimestamp;

        /// <summary>
        /// 在Awake时，我们进行初始化
        /// </summary>
        protected virtual void Awake()
		{
			Initialization();
		}

        /// <summary>
        /// 在初始化时，我们获取我们的Health组件
        /// </summary>
        protected virtual void Initialization()
		{
			_health = TargetHealth == null ? this.gameObject.GetComponent<Health>() : TargetHealth;
		}

        /// <summary>
        /// 在更新时，我们恢复生命值
        /// </summary>
        protected virtual void Update()
		{
			ProcessRefillHealth();
		}

        /// <summary>
        /// 测试是否需要恢复并进行恢复处理
        /// </summary>
        protected virtual void ProcessRefillHealth()
		{
			if (!RefillHealth)
			{
				return;
			}

			if (Time.time - _lastHitTime < CooldownAfterHit)
			{
				return;
			}

			if (_health.CurrentHealth < _health.MaximumHealth)
			{
				switch (RefillMode)
				{
					case RefillModes.Bursts:
						if (Time.time - _lastBurstTimestamp > DurationBetweenBursts)
						{
							_health.ReceiveHealth(HealthPerBurst, this.gameObject);
							_lastBurstTimestamp = Time.time;
						}
						break;

					case RefillModes.Linear:
						_healthToGive += HealthPerSecond * Time.deltaTime;
						if (_healthToGive > 1f)
						{
							float givenHealth = _healthToGive;
							_healthToGive -= givenHealth;
							_health.ReceiveHealth(givenHealth, this.gameObject);
						}
						break;
				}
			}
		}

        /// <summary>
        /// 在击中时，我们记录时间
        /// </summary>
        public virtual void OnHit()
		{
			_lastHitTime = Time.time;
		}

        /// <summary>
        /// 在启用时，我们开始监听命中事件
        /// </summary>
        protected virtual void OnEnable()
		{
			_health.OnHit += OnHit;
		}

        /// <summary>
        /// 在禁用时，我们停止监听命中事件
        /// </summary>
        protected virtual void OnDisable()
		{
			_health.OnHit -= OnHit;
		}
	}
}