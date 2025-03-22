using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 与对象池（简单或多个）一起使用的类
    /// 有规律地生成对象，以在检查器中设置的最小值和最大值之间随机选择的频率
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Automation/Timed Spawner")]
	public class TimedSpawner : TopDownMonoBehaviour 
	{
        /// 与此生成器关联的对象池
        public virtual MMObjectPooler ObjectPooler { get; set; }
		
		[Header("Spawn生成")]
		/// whether or not this spawner can spawn
		[Tooltip("这个生成器是否可以生成")]
		public bool CanSpawn = true;
		/// the minimum frequency possible, in seconds
		[Tooltip("可能的最小频率，以秒为单位")]
		public float MinFrequency = 1f;
		/// the maximum frequency possible, in seconds
		[Tooltip("可能的最大频率，以秒为单位")]
		public float MaxFrequency = 1f;

		[Header("Debug调试")]
		[MMInspectorButton("ToggleSpawn")]
        /// 生成一个对象的测试按钮
        public bool CanSpawnButton;

		protected float _lastSpawnTimestamp = 0f;
		protected float _nextFrequency = 0f;

        /// <summary>
        /// 在Start中，我们初始化我们的spawner
        /// </summary>
        protected virtual void Start()
		{
			Initialization ();
		}

        /// <summary>
        /// 获取相关的对象池（如果有的话），并初始化频率
        /// </summary>
        protected virtual void Initialization()
		{
			if (GetComponent<MMMultipleObjectPooler>() != null)
			{
				ObjectPooler = GetComponent<MMMultipleObjectPooler>();
			}
			if (GetComponent<MMSimpleObjectPooler>() != null)
			{
				ObjectPooler = GetComponent<MMSimpleObjectPooler>();
			}
			if (ObjectPooler == null)
			{
				Debug.LogWarning(this.name+ " : 没有对象池（简单或多个）附加到这个投射武器，它将无法射击任何东西。");
				return;
			}
			DetermineNextFrequency ();
		}

        /// <summary>
        /// 每一帧我们都检查是否应该生成一些东西
        /// </summary>
        protected virtual void Update()
		{
			if ((Time.time - _lastSpawnTimestamp > _nextFrequency)  && CanSpawn)
			{
				Spawn ();
			}
		}

        /// <summary>
        /// 如果池中有可用的对象，则从池中生成一个对象。
        /// 如果它是一个有生命值的对象，也会恢复它。
        /// </summary>
        protected virtual void Spawn()
		{
			GameObject nextGameObject = ObjectPooler.GetPooledGameObject();

            // 强制性的检查
            if (nextGameObject==null) { return; }
			if (nextGameObject.GetComponent<MMPoolableObject>()==null)
			{
				throw new Exception(gameObject.name+" is trying to spawn objects that don't have a PoolableObject component.");		
			}

            // 我们激活对象
            nextGameObject.gameObject.SetActive(true);
			nextGameObject.gameObject.MMGetComponentNoAlloc<MMPoolableObject>().TriggerOnSpawnComplete();

            // 我们检查我们的对象是否有生命值组件，如果有，我们就复活我们的角色
            Health objectHealth = nextGameObject.gameObject.MMGetComponentNoAlloc<Health> ();
			if (objectHealth != null) 
			{
				objectHealth.Revive ();
			}

            // 我们定位对象
            nextGameObject.transform.position = this.transform.position;

            // 我们重置计时器，确定下一个频率
            _lastSpawnTimestamp = Time.time;
			DetermineNextFrequency ();
		}

        /// <summary>
        /// 通过在检查器中指定的两个值之间随机化一个值来确定下一个频率。
        /// </summary>
        protected virtual void DetermineNextFrequency()
		{
			_nextFrequency = UnityEngine.Random.Range (MinFrequency, MaxFrequency);
		}

        /// <summary>
        /// 切换刷出开关
        /// </summary>
        public virtual void ToggleSpawn()
		{
			CanSpawn = !CanSpawn;
		}

        /// <summary>
        /// 关闭刷出
        /// </summary>
        public virtual void TurnSpawnOff()
		{
			CanSpawn = false;
		}

        /// <summary>
        /// 开启刷出
        /// </summary>
        public virtual void TurnSpawnOn()
		{
			CanSpawn = true;
		}
	}
}