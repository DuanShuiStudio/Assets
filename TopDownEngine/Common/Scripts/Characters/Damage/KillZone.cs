using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此组件添加到对象中，它将对与之碰撞的对象造成损坏。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Damage/Kill Zone")]
	public class KillZone : TopDownMonoBehaviour
	{
		[Header("Targets目标")]
		[MMInformation("这个组件将使你的对象杀死与它碰撞的对象。在这里你可以定义哪些图层将被删除。", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
		// the layers containing the objects that will be damaged by this object
		[Tooltip("包含将被该对象损坏的对象的层")]
		public LayerMask TargetLayerMask = LayerManager.PlayerLayerMask;

		protected Health _colliderHealth;

        /// <summary>
        /// 初始化
        /// </summary>
        protected virtual void Awake()
		{

		}

        /// <summary>
        /// 允许将开始时间设置为当前时间戳
        /// </summary>
        protected virtual void OnEnable()
		{

		}

        /// <summary>
        /// 当与玩家发生碰撞时，我们会对玩家造成伤害并将其击退
        /// </summary>
        /// <param name="collider">what's colliding with the object.</param>
        public virtual void OnTriggerStay2D(Collider2D collider)
		{
			Colliding(collider.gameObject);
		}

        /// <summary>
        /// 当有东西进入我们的区域时，我们调用碰撞端点
        /// </summary>
        /// <param name="collider"></param>
        public virtual void OnTriggerEnter2D(Collider2D collider)
		{
			Colliding(collider.gameObject);
		}

        /// <summary>
        /// 当物体停留在这个区域时，我们称之为碰撞端点
        /// </summary>
        /// <param name="collider"></param>
        public virtual void OnTriggerStay(Collider collider)
		{
			Colliding(collider.gameObject);
		}

        /// <summary>
        /// 当有东西进入我们的区域时，我们调用碰撞端点
        /// </summary>
        /// <param name="collider"></param>
        public virtual void OnTriggerEnter(Collider collider)
		{
			Colliding(collider.gameObject);
		}

        /// <summary>
        /// 当碰撞时，如果它是装备生命值的对象，我们将杀死碰撞器
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void Colliding(GameObject collider)
		{
			if (!this.isActiveAndEnabled)
			{
				return;
			}

            // 如果我们碰撞的不是目标层的一部分，我们不做任何事情并退出
            if (!MMLayers.LayerInLayerMask(collider.layer, TargetLayerMask))
			{
				return;
			}

			_colliderHealth = collider.gameObject.MMGetComponentNoAlloc<Health>();

            // 如果我们撞击的物体是可损坏的
            if (_colliderHealth != null)
			{
				if (_colliderHealth.CurrentHealth > 0)
				{
					_colliderHealth.Kill();
				}                
			}
		}
	}
}