using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 这个反馈（feedback）将把当前MMFeedbacks序列的“头”移回到列表中前面的另一个反馈。
    /// 头部最终停留的反馈取决于你的设置：你可以决定让它在最后一个暂停（pause）处循环，或者在列表中的最后一个LoopStart反馈处循环（也可以同时选择这两个选项）。
    /// 此外，你还可以决定让它循环多次，并且在遇到时引起暂停
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("这个反馈将把当前MMFeedbacks序列的“头”移回到列表中前面的另一个反馈 " +
                  "头部最终停留的反馈取决于你的设置：你可以决定让它在最后一个暂停处循环，" +
                  "或者在列表中的最后一个LoopStart反馈处循环（也可以同时选择这两个选项）。此外，你还可以决定让它循环多次，并且在遇到时引起暂停。")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Loop/Looper")]
	public class MMF_Looper : MMF_Pause
	{
		[MMFInspectorGroup("Loop", true, 34)]
        
		[Header("Loop conditions")]
		/// if this is true, this feedback, when met, will cause the MMFeedbacks to reposition its 'head' to the first pause found above it (going from this feedback to the top), or to the start if none is found
		[Tooltip("如果这是真的，当满足此反馈时，MMFeedbacks 会将其“头”重新定位到其上方找到的第一个暂停处（从这个反馈到顶部），或者如果没有找到则定位到开始处。")]
		public bool LoopAtLastPause = true;
		/// if this is true, this feedback, when met, will cause the MMFeedbacks to reposition its 'head' to the first LoopStart feedback found above it (going from this feedback to the top), or to the start if none is found
		[Tooltip("如果这是真的，当满足此反馈时，MMFeedbacks 会将其“头”重新定位到其上方找到的第一个 LoopStart 反馈处（从这个反馈到顶部），或者如果没有找到则定位到开始处")]
		public bool LoopAtLastLoopStart = true;

		[Header("Loop循环")]
		/// if this is true, the looper will loop forever
		[Tooltip("如果这是真的，循环器将永远循环")]
		public bool InfiniteLoop = false;
		/// how many times this loop should run
		[Tooltip("这个循环应该运行多少次？")]
		[MMCondition("InfiniteLoop", true, true)]
		public int NumberOfLoops = 2;
		/// the amount of loops left (updated at runtime)
		[Tooltip("剩余的循环次数（在运行时更新）")]
		[MMFReadOnly]
		public int NumberOfLoopsLeft = 1;
		/// whether we are in an infinite loop at this time or not
		[Tooltip("此时我们是否处于无限循环中")]
		[MMFReadOnly]
		public bool InInfiniteLoop = false;
		/// whether or not to trigger a Loop MMFeedbacksEvent when this looper is reached
		[Tooltip("是否在该循环器被触发时启动 Loop MMFeedbacksEvent。")]
		public bool TriggerMMFeedbacksEvents = true;

		[Header("Events事件")] 
		/// a Unity Event to invoke when the looper is reached
		[Tooltip("当达到循环器时要调用的 Unity 事件")]
		public UnityEvent OnLoop;

        /// 在检查器中设置此反馈的颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.LooperColor; } }
		#endif
		public override bool LooperPause { get { return true; } }

        /// 此反馈的持续时间就是暂停的时长
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(PauseDuration); } set { PauseDuration = value; } }

        /// <summary>
        /// 在初始化时，我们初始化剩余的循环次数。
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			InInfiniteLoop = InfiniteLoop;
			NumberOfLoopsLeft = NumberOfLoops;
		}

        /// <summary>
        /// 在播放时，我们减少计数器并播放暂停
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (Active)
			{
				ProcessNewPauseDuration();
				InInfiniteLoop = InfiniteLoop;
				NumberOfLoopsLeft--;
				Owner.StartCoroutine(PlayPause());
				TriggerOnLoop(Owner);
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		public virtual void TriggerOnLoop(MMFeedbacks source)
		{
			OnLoop.Invoke();

			if (TriggerMMFeedbacksEvents)
			{
				MMFeedbacksEvent.Trigger(source, MMFeedbacksEvent.EventTypes.Loop);
			}
		}

        /// <summary>
        /// 在自定义停止时，我们退出无限循环
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			base.CustomStopFeedback(position, feedbacksIntensity);
			InInfiniteLoop = false;
		}

        /// <summary>
        /// 在重置时，我们重置剩余的循环次数
        /// </summary>
        protected override void CustomReset()
		{
			base.CustomReset();
			InInfiniteLoop = InfiniteLoop;
			NumberOfLoopsLeft = NumberOfLoops;
		}
	}
}