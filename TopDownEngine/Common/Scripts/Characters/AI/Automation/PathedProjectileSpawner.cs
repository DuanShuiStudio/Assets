using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 刷出抛射物路径
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Automation/Pathed Projectile Spawner")]
	public class PathedProjectileSpawner : TopDownMonoBehaviour 
	{
		[MMInformation("带有此组件的游戏对象将以指定的射速生成抛射物。", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// the pathed projectile's destination
		[Tooltip("路径投射物的目的地")]
		public Transform Destination;
		/// the projectiles to spawn
		[Tooltip("产生投射物")]
		public PathedProjectile Projectile;
		/// the effect to instantiate at each spawn
		[Tooltip("在每次生成时实例化的效果")]
		public GameObject SpawnEffect;
		/// the speed of the projectiles
		[Tooltip("投射物的速度")]
		public float Speed;
		/// the frequency of the spawns
		[Tooltip("生产投射物的频率")]
		public float FireRate;
		
		protected float _nextShotInSeconds;

		/// <summary>
		/// 初始化
		/// </summary>
		protected virtual void Start () 
		{
			_nextShotInSeconds=FireRate;
		}

        /// <summary>
        /// 每一帧，我们检查是否需要实例化一个新的投射物
        /// </summary>
        protected virtual void Update () 
		{
			if((_nextShotInSeconds -= Time.deltaTime)>0)
				return;
				
			_nextShotInSeconds = FireRate;
			var projectile = (PathedProjectile) Instantiate(Projectile, transform.position,transform.rotation);
			projectile.Initialize(Destination,Speed);
			
			if (SpawnEffect!=null)
			{
				Instantiate(SpawnEffect,transform.position,transform.rotation);
			}
		}

		/// <summary>
		/// 调试模式
		/// </summary>
		public virtual void OnDrawGizmos()
		{
			if (Destination==null)
				return;
			
			Gizmos.color=Color.gray;
			Gizmos.DrawLine(transform.position,Destination.position);
		}
	}
}