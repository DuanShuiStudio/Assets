using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 如果MMFeedbacks处于AutoPlayOnEnable模式，会自动添加的一个辅助类
    /// 这样，如果其父游戏对象被禁用/启用，它们就可以再次播放
    /// </summary>
    [AddComponentMenu("")]
	public class MMF_PlayerEnabler : MonoBehaviour
	{
        /// 用于播放的MMFeedbacks
        public virtual MMF_Player TargetMmfPlayer { get; set; }

        /// <summary>
        /// 在启用时，如果需要的话，我们会重新启用（因此也会播放）我们的MMFeedbacks。
        /// </summary>
        protected virtual void OnEnable()
		{
			if ((TargetMmfPlayer != null) && !TargetMmfPlayer.enabled && TargetMmfPlayer.AutoPlayOnEnable)
			{
				TargetMmfPlayer.enabled = true;
			}
		}
	}    
}