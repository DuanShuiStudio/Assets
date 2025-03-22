using MoreMountains.Feedbacks;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 这个类可以用来触发来自动画师的反馈，通常用于触发脚步粒子和/或声音
    /// </summary>
    public class CharacterAnimationFeedbacks : TopDownMonoBehaviour
	{
		/// a feedback that will play every time a foot touches the ground while walking
		[Tooltip("每次行走时脚触地都会播放的反馈")]
		public MMFeedbacks WalkFeedbacks;

		/// a feedback that will play every time a foot touches the ground while running
		[Tooltip("每次奔跑时脚触地都会播放的反馈")]
		public MMFeedbacks RunFeedbacks;

        /// <summary>
        /// 当脚触地时播放步行反馈（通过动画事件触发）
        /// </summary>
        public virtual void WalkStep()
		{
			WalkFeedbacks?.PlayFeedbacks();
		}

        /// <summary>
        /// 当脚触地时播放奔跑反馈（通过动画事件触发）
        /// </summary>
        public virtual void RunStep()
		{
			RunFeedbacks?.PlayFeedbacks();
		}
	}
}