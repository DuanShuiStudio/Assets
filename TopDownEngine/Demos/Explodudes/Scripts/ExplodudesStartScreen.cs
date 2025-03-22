using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 *demo
 *demo
 *demo
 *demo
 */
namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 这个类处理 Explodudes 演示场景的开始屏幕
    /// </summary>
    public class ExplodudesStartScreen : TopDownMonoBehaviour
	{
        /// <summary>
        /// 在开始时，启用它的所有子对象
        /// </summary>
        protected virtual void Start()
		{
			foreach (Transform child in transform)
			{
				child.gameObject.SetActive(true);
			}
		}

        /// <summary>
        /// 在动画播放完毕后，由动画器调用以关闭开始屏幕
        /// </summary>
        public virtual void DisableStartScreen()
		{
			this.gameObject.SetActive(false);
		}
	}
}