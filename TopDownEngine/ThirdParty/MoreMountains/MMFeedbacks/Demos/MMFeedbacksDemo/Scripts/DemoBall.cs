using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.Scripting.APIUpdating;
namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 一个处理MMFeedbacks演示中包含的球体生命周期的类。
    /// 它在球体生成后等待2秒，然后销毁该球体，同时播放MMFeedbacks（多媒体反馈）效果。
    /// </summary>
    [AddComponentMenu("")]
	public class DemoBall : MonoBehaviour
	{
        /// 球体存活的时长（以秒为单位）
        public float LifeSpan = 2f;
        /// 球体消失时要播放的反馈（效果）
        public MMFeedbacks DeathFeedback;


        /// <summary>
        /// 在启动时，我们触发球的预设死亡程序
        /// </summary>
        protected virtual void Start()
		{
			StartCoroutine(ProgrammedDeath());
		}

        /// <summary>
        /// 等待2秒钟，然后在播放完MMFeedbacks（多媒体反馈）后杀死球体对象
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator ProgrammedDeath()
		{
			yield return MMCoroutine.WaitFor(LifeSpan);
			DeathFeedback?.PlayFeedbacks();
			this.gameObject.SetActive(false);
		}
	}
}