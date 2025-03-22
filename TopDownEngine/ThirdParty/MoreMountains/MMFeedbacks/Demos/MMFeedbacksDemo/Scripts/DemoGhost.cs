using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 一个用于MMFeedback示例镜像的类
    /// </summary>
    [AddComponentMenu("")]
	public class DemoGhost : MonoBehaviour
	{
        /// <summary>
        ///通过动画事件调用，禁用该对象
        /// </summary>
        public virtual void OnAnimationEnd()
		{
			this.gameObject.SetActive(false);
		}
	}
}