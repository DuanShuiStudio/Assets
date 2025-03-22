using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// 此组件将由MMF_Broadcast反馈自动添加。
	/// </summary>
	public class MMF_BroadcastProxy : MonoBehaviour
	{
		/// the channel on which to broadcast
		[Tooltip("用于进行广播的频道")]
		[MMReadOnly]
		public int Channel;
		/// a debug view of the current level being broadcasted
		[Tooltip("正在广播的当前关卡的调试视图")]
		[MMReadOnly]
		public float DebugLevel;
		/// whether or not a broadcast is in progress (will be false while the value is not changing, and thus not broadcasting)
		[Tooltip("是否正在进行广播（当值没有变化，因而没有进行广播时，该值将为假） ")]
		[MMReadOnly]
		public bool BroadcastInProgress = false;

		public virtual float ThisLevel { get; set; }
		protected float _levelLastFrame;

		/// <summary>
		/// 在执行“更新”操作时，我们会处理我们的广播内容。 
		/// </summary>
		protected virtual void Update()
		{
			ProcessBroadcast();
		}

		/// <summary>
		/// 如果有需要，则广播该值。
		/// </summary>
		protected virtual void ProcessBroadcast()
		{
			BroadcastInProgress = false;
			if (ThisLevel != _levelLastFrame)
			{
				MMRadioLevelEvent.Trigger(Channel, ThisLevel);
				BroadcastInProgress = true;
			}
			DebugLevel = ThisLevel;
			_levelLastFrame = ThisLevel;
		}
	}    
}