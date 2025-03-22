using System.Collections;
using System.Collections.Generic;
using UnityEngine;using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will cause a pause when met, preventing any other feedback lower in the sequence to run until it's complete.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈在满足时将导致暂停，防止序列中任何其他较低的反馈运行，直到它完成")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Pause/Pause")]
	public class MMF_Pause : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用所有这种类型的反馈
        public static bool FeedbackTypeAuthorized = true;
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.PauseColor; } }
		public override bool DisplayFullHeaderColor => true;
		#endif
		public override IEnumerator Pause { get { return PauseWait(); } }
        
		[MMFInspectorGroup("Pause", true, 32)]
		/// the duration of the pause, in seconds
		[Tooltip("暂停的持续时间（以秒为单位）")]
		public float PauseDuration = 1f;

		public bool RandomizePauseDuration = false;

		[MMFCondition("RandomizePauseDuration", true)]
		public float MinPauseDuration = 1f;
		[MMFCondition("RandomizePauseDuration", true)]
		public float MaxPauseDuration = 3f;
		[MMFCondition("RandomizePauseDuration", true)]
		public bool RandomizeOnEachPlay = true;
        
		/// if this is true, you'll need to call the Resume() method on the host MMFeedbacks for this pause to stop, and the rest of the sequence to play
		[Tooltip("如果为真，则需要在主机MMFeedbacks上调用Resume()方法以停止暂停，并播放序列的其余部分")]
		public bool ScriptDriven = false;
		/// if this is true, a script driven pause will resume after its AutoResumeAfter delay, whether it has been manually resumed or not 
		[Tooltip("如果为真，脚本驱动的暂停将在其AutoResumeAfter延迟后恢复，无论是否已手动恢复")] 
		[MMFCondition("ScriptDriven", true)]
		public bool AutoResume = false;
		/// the duration after which to auto resume, regardless of manual resume calls beforehand
		[Tooltip("无论之前是否有手动恢复调用，都要自动恢复的持续时间")] 
		[MMFCondition("AutoResume", true)]
		public float AutoResumeAfter = 0.25f;

        /// 此反馈的持续时间就是暂停的持续时间
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(PauseDuration); } set { PauseDuration = value; } }

        /// <summary>
        /// 用于在缩放或未缩放时间中等待暂停持续时间的IEnumerator。
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator PauseWait()
		{
			yield return WaitFor(ApplyTimeMultiplier(PauseDuration));
		}

        /// <summary>
        /// 在初始化时，我们缓存等待秒数。
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			ScriptDrivenPause = ScriptDriven;
			ScriptDrivenPauseAutoResume = AutoResume ? AutoResumeAfter : -1f;
			if (RandomizePauseDuration)
			{
				PauseDuration = Random.Range(MinPauseDuration, MaxPauseDuration);
			}
		}

        /// <summary>
        /// 在播放时，我们触发暂停
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			ProcessNewPauseDuration();
			Owner.StartCoroutine(PlayPause());
		}

        /// <summary>
        /// 如有必要，计算新的暂停持续时间
        /// </summary>
        protected virtual void ProcessNewPauseDuration()
		{
			if (RandomizePauseDuration && RandomizeOnEachPlay)
			{
				PauseDuration = Random.Range(MinPauseDuration, MaxPauseDuration);
			}
		}

        /// <summary>
        /// 暂停协程
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator PlayPause()
		{
			yield return Pause;
		}
	}
}