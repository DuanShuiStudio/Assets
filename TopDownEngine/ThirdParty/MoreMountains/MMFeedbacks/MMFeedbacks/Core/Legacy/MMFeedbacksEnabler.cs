using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 一个由MMFeedbacks自动添加的辅助类，当它们处于AutoPlayOnEnable模式时。
    /// 如果其父游戏对象被禁用/启用，它们就可以再次播放
    /// </summary>
    [AddComponentMenu("")]
	public class MMFeedbacksEnabler : MonoBehaviour
	{
        /// 用于播放的MMFeedbacks
        public MMFeedbacks TargetMMFeedbacks { get; set; }

        /// <summary>
        /// 在启用时，如果需要的话，我们会重新启用（因此也会播放）我们的MMFeedbacks。
        /// </summary>
        protected virtual void OnEnable()
		{
			if ((TargetMMFeedbacks != null) && !TargetMMFeedbacks.enabled && TargetMMFeedbacks.AutoPlayOnEnable)
			{
				TargetMMFeedbacks.enabled = true;
			}
		}
	}    
}