using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;
/*
 *demo
 *demo
 *demo
 *demo
 */
namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 在考拉演示中用于在生成器处于活动状态时发射粒子，否则停止它们的类
    /// </summary>
    public class DungeonPortal : TopDownMonoBehaviour
	{
		/// the particles to play while the portal is spawning
		[Tooltip("demo-在传送门生成时播放的粒子")]
		public ParticleSystem SpawnParticles;

		protected TimedSpawner _timedSpawner;

        /// <summary>
        /// 在唤醒时，我们获取定时生成器
        /// </summary>
        protected virtual void Awake()
		{
			_timedSpawner = this.gameObject.GetComponent<TimedSpawner>();
		}

        /// <summary>
        /// 在更新时，如果需要的话，我们停止或播放粒子
        /// </summary>
        protected virtual void Update()
		{
			if ((!_timedSpawner.CanSpawn) && (SpawnParticles.isPlaying))
			{
				SpawnParticles.Stop();
			}
			if ((_timedSpawner.CanSpawn) && (!SpawnParticles.isPlaying))
			{
				SpawnParticles.Play();
			}
		}
	}
}