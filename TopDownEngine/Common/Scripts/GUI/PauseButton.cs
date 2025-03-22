using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 一个简单组件，旨在添加到暂停按钮
    /// </summary>
    [AddComponentMenu("TopDown Engine/GUI/Pause Button")]
	public class PauseButton : TopDownMonoBehaviour
	{
        /// <summary>
        /// 触发暂停事件
        /// </summary>
        public virtual void PauseButtonAction()
		{
            // 我们为GameManager和其他可能也在监听它的类触发一个Pause暂停事件
            StartCoroutine(PauseButtonCo());

		}

        /// <summary>
        /// 通过UnPause事件取消暂停游戏
        /// </summary>
        public virtual void UnPause()
		{
			StartCoroutine(PauseButtonCo());
		}

        /// <summary>
        /// 用于触发暂停事件的协程
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator PauseButtonCo()
		{
			yield return null;
            // 我们为GameManager和其他可能也在监听它的类触发一个Pause事件
            TopDownEngineEvent.Trigger(TopDownEngineEventTypes.TogglePause, null);
		}

	}
}