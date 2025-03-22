using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 这个类处理路径投射物的移动
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Automation/Pathed Projectile")]
	public class PathedProjectile : TopDownMonoBehaviour
	{
		[MMInformation("带有此组件的游戏对象将朝着目标移动，并在到达目标时被摧毁。在这里，你可以定义在撞击时实例化的对象。使用Initialize方法设置其目的地和速度。", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// The effect to instantiate when the object gets destroyed
		[Tooltip("当对象被销毁时要实例化的效果")]
		public GameObject DestroyEffect;
		/// the destination of the projectile
		[Tooltip("投射物的目的地")]
		protected Transform _destination;
		/// the movement speed
		[Tooltip("移动速度")]
		protected float _speed;

        /// <summary>
        /// 初始化指定的目的地和速度。
        /// </summary>
        /// <param name="destination">Destination.</param>
        /// <param name="speed">Speed.</param>
        public virtual void Initialize(Transform destination, float speed)
		{
			_destination=destination;
			_speed=speed;
		}

        /// <summary>
        /// 每一帧，我都将弹丸的位置移动到它的目的地
        /// </summary>
        protected virtual void Update () 
		{
			transform.position=Vector3.MoveTowards(transform.position,_destination.position,Time.deltaTime * _speed);
			var distanceSquared = (_destination.transform.position - transform.position).sqrMagnitude;
			if(distanceSquared > .01f * .01f)
				return;
			
			if (DestroyEffect!=null)
			{
				Instantiate(DestroyEffect,transform.position,transform.rotation); 
			}
			
			Destroy(gameObject);
		}	
	}
}