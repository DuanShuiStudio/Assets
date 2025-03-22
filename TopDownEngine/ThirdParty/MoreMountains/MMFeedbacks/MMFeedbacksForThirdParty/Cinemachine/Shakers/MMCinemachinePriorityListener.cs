using UnityEngine;
#if MM_CINEMACHINE
using Cinemachine;
#elif MM_CINEMACHINE3
using Unity.Cinemachine;
#endif
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
    /// <summary>
    /// 将此添加到 Cinemachine 虚拟相机中，它将能够监听 MMCinemachinePriorityEvent（MM Cinemachine 优先级事件），该事件通常由 MMFeedbackCinemachineTransition（MM 反馈 Cinemachine 过渡效果）触发。 
    /// </summary>
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Cinemachine/MM Cinemachine Priority Listener")]
	#if MM_CINEMACHINE || MM_CINEMACHINE3
	[RequireComponent(typeof(CinemachineVirtualCameraBase))]
	#endif
	public class MMCinemachinePriorityListener : MonoBehaviour
	{
        
		[HideInInspector] 
		public TimescaleModes TimescaleMode = TimescaleModes.Scaled;
        
        
		public virtual float GetTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.time : Time.unscaledTime; }
		public virtual float GetDeltaTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; }
        
		[Header("Priority Listener优先级监听器")]
		[Tooltip("是要监听由一个整数定义的通道，还是由一个MMChannel可编写脚本对象定义的通道。整数设置起来很简单，但可能会变得混乱，并且更难记住哪个整数对应着什么。" +
                 "MMChannel可编写脚本对象要求你提前创建它们，但它们带有易读的名称，并且更具可扩展性")]
		public MMChannelModes ChannelMode = MMChannelModes.Int;
		/// the channel to listen to - has to match the one on the feedback
		[Tooltip("要监听的通道——必须与反馈中的那个通道相匹配")]
		[MMFEnumCondition("ChannelMode", (int)MMChannelModes.Int)]
		public int Channel = 0;
		/// the MMChannel definition asset to use to listen for events. The feedbacks targeting this shaker will have to reference that same MMChannel definition to receive events - to create a MMChannel,
		/// right click anywhere in your project (usually in a Data folder) and go MoreMountains > MMChannel, then name it with some unique name
		[Tooltip("要用于监听事件的 MMChannel 定义资源。以这个抖动器为目标的反馈必须引用相同的 MMChannel 定义才能接收事件。" +
                 "要创建一个 MMChannel，在项目中的任意位置（通常在一个数据文件夹中）右键单击，然后选择 “MoreMountains”>“MMChannel”，接着给它起一个唯一的名称。")]
		[MMFEnumCondition("ChannelMode", (int)MMChannelModes.MMChannel)]
		public MMChannel MMChannelDefinition = null;

		#if MM_CINEMACHINE || MM_CINEMACHINE3
		protected CinemachineVirtualCameraBase _camera;
		protected int _initialPriority;
#endif

        /// <summary>
        /// 在唤醒（Awake）阶段，我们存储我们的虚拟相机。
        /// </summary>
        protected virtual void Awake()
		{
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			_camera = this.gameObject.GetComponent<CinemachineVirtualCameraBase>();
			#endif
			#if MM_CINEMACHINE 
			_initialPriority = _camera.Priority;
			#elif MM_CINEMACHINE3
			_initialPriority = _camera.Priority.Value; 
			#endif
		}

#if MM_CINEMACHINE || MM_CINEMACHINE3
        /// <summary>
        /// 当我们接收到一个事件时，如果有必要，我们会更改我们的优先级。 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="forceMaxPriority"></param>
        /// <param name="newPriority"></param>
        /// <param name="forceTransition"></param>
        /// <param name="blendDefinition"></param>
        /// <param name="resetValuesAfterTransition"></param>
        public virtual void OnMMCinemachinePriorityEvent(MMChannelData channelData, bool forceMaxPriority, int newPriority, bool forceTransition, CinemachineBlendDefinition blendDefinition, bool resetValuesAfterTransition, TimescaleModes timescaleMode, bool restore = false)
		{
			TimescaleMode = timescaleMode;
			if (MMChannel.Match(channelData, ChannelMode, Channel, MMChannelDefinition))
			{
				if (restore)
				{
					SetPriority(_initialPriority);	
					return;
				}
				SetPriority(newPriority);
			}
			else
			{
				if (forceMaxPriority)
				{
					if (restore)
					{
						SetPriority(_initialPriority);	
						return;
					}
					SetPriority(0);
				}
			}
		}
		#endif

		protected virtual void SetPriority(int newPriority)
		{
			#if MM_CINEMACHINE 
			_camera.Priority = newPriority;
			#elif MM_CINEMACHINE3
			PrioritySettings prioritySettings = _camera.Priority;
			prioritySettings.Value = newPriority;
			_camera.Priority = prioritySettings;
			#endif
		}

        /// <summary>
        /// 当启用时，我们开始监听事件。
        /// </summary>
        protected virtual void OnEnable()
		{
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			MMCinemachinePriorityEvent.Register(OnMMCinemachinePriorityEvent);
			#endif
		}

        /// <summary>
        /// 停止监听事件
        /// </summary>
        protected virtual void OnDisable()
		{
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			MMCinemachinePriorityEvent.Unregister(OnMMCinemachinePriorityEvent);
			#endif
		}
	}

    /// <summary>
    /// 一个用于操控 Cinemachine 虚拟相机的优先级以及大脑（Cinemachine 大脑）过渡效果的事件。 
    /// </summary>
    public struct MMCinemachinePriorityEvent
	{
		#if MM_CINEMACHINE || MM_CINEMACHINE3
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(MMChannelData channelData, bool forceMaxPriority, int newPriority, bool forceTransition, CinemachineBlendDefinition blendDefinition, bool resetValuesAfterTransition, TimescaleModes timescaleMode, bool restore = false);
		static public void Trigger(MMChannelData channelData, bool forceMaxPriority, int newPriority, bool forceTransition, CinemachineBlendDefinition blendDefinition, bool resetValuesAfterTransition, TimescaleModes timescaleMode, bool restore = false)
		{
			OnEvent?.Invoke(channelData, forceMaxPriority, newPriority, forceTransition, blendDefinition, resetValuesAfterTransition, timescaleMode, restore);
		}
		#endif
	}
}