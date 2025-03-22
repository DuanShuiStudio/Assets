using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 协程助手
    /// </summary>
    public static class MMFeedbacksCoroutine
	{
        /// <summary>
        /// 等待指定数量的帧。
        /// use : yield return MMCoroutine.WaitFor(1);
        /// </summary>
        /// <param name="frameCount"></param>
        /// <returns></returns>
        public static IEnumerator WaitForFrames(int frameCount)
		{
			while (frameCount > 0)
			{
				frameCount--;
				yield return null;
			}
		}

        /// <summary>
        /// 等待指定的秒数（使用常规时间）。
        /// use : yield return MMCoroutine.WaitFor(1f);
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static IEnumerator WaitFor(float seconds)
		{
			for (float timer = 0f; timer < seconds; timer += Time.deltaTime)
			{
				yield return null;
			}
		}

        /// <summary>
        /// 等待指定的秒数（使用未缩放时间）。
        /// use : yield return MMCoroutine.WaitForUnscaled(1f);
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static IEnumerator WaitForUnscaled(float seconds)
		{
			for (float timer = 0f; timer < seconds; timer += Time.unscaledDeltaTime)
			{
				yield return null;
			}
		}
	}
}