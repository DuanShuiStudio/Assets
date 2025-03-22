using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
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
    /// 这个类用于Deadline演示中标记可收集的物品，并禁止它们在之前访问过该关卡时被收集
    /// </summary>
    public class DeadlineCollectible : TopDownMonoBehaviour
	{
		public string CollectibleName = "";

        /// <summary>
        /// 在开始时，如果需要，我们禁用游戏对象
        /// </summary>
        protected virtual void Start()
		{
			DisableIfAlreadyCollected ();
		}

        /// <summary>
        /// 调用这个来收集这个可收集的物品，并在未来跟踪它
        /// </summary>
        public virtual void Collect()
		{
			DeadlineProgressManager.Instance.FindCollectible (CollectibleName);
		}

        /// <summary>
        /// 如果它已经被收集过，则禁用游戏对象
        /// </summary>
        protected virtual void DisableIfAlreadyCollected ()
		{
			if (DeadlineProgressManager.Instance.FoundCollectibles == null)
			{
				return;
			}
			foreach (string collectible in DeadlineProgressManager.Instance.FoundCollectibles)
			{
				if (collectible == this.CollectibleName)
				{
					Disable ();
				}
			}
		}

        /// <summary>
        /// 禁用这个游戏对象。
        /// </summary>
        protected virtual void Disable()
		{
			this.gameObject.SetActive (false);
		}
	    
	}
}
