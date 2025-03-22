using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
	[RequireComponent(typeof(MMFeedbacks))]
	[AddComponentMenu("More Mountains/Feedbacks/Shakers/Feedbacks/MM Feedbacks Shaker")]
	public class MMFeedbacksShaker : MMShaker
	{
		protected MMFeedbacks _mmFeedbacks;

        /// <summary>
        /// 在初始化时，我们初始化我们的值
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			_mmFeedbacks = this.gameObject.GetComponent<MMFeedbacks>();
		}

		public virtual void OnMMFeedbacksShakeEvent(MMChannelData channelData = null, bool useRange = false, float eventRange = 0f, Vector3 eventOriginPosition = default(Vector3))
		{
			if (!CheckEventAllowed(channelData, useRange, eventRange, eventOriginPosition) || (!Interruptible && Shaking))
			{
				return;
			}
			Play();
		}

		protected override void ShakeStarts()
		{
			_mmFeedbacks.PlayFeedbacks();
		}

        /// <summary>
        /// 当添加那个抖动器时，我们会初始化它的抖动持续时间。
        /// </summary>
        protected virtual void Reset()
		{
			ShakeDuration = 0.01f;
		}

        /// <summary>
        /// 开始监听事件
        /// </summary>
        public override void StartListening()
		{
			base.StartListening();
			MMFeedbacksShakeEvent.Register(OnMMFeedbacksShakeEvent);
		}

        /// <summary>
        /// 停止监听事件
        /// </summary>
        public override void StopListening()
		{
			base.StopListening();
			MMFeedbacksShakeEvent.Unregister(OnMMFeedbacksShakeEvent);
		}
	}

	public struct MMFeedbacksShakeEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(MMChannelData channelData = null, bool useRange = false, float eventRange = 0f, Vector3 eventOriginPosition = default(Vector3));

		static public void Trigger(MMChannelData channelData = null, bool useRange = false, float eventRange = 0f, Vector3 eventOriginPosition = default(Vector3))
		{
			OnEvent?.Invoke(channelData, useRange, eventRange, eventOriginPosition);
		}
	}
}