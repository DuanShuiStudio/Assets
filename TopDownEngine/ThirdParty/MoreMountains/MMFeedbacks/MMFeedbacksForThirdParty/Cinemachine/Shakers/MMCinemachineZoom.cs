using UnityEngine;
#if MM_CINEMACHINE
using Cinemachine;
#elif MM_CINEMACHINE3
using Unity.Cinemachine;
#endif
using MoreMountains.Feedbacks;
using MoreMountains.Tools;

namespace MoreMountains.FeedbacksForThirdParty
{
    /// <summary>
    /// 这个类能让你通过从任何其他类发送 MMCameraZoomEvent（MM相机缩放事件），来触发对 Cinemachine 相机的缩放操作。 
    /// </summary>
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Cinemachine/MM Cinemachine Zoom")]
	#if MM_CINEMACHINE
	[RequireComponent(typeof(Cinemachine.CinemachineVirtualCamera))]
	#elif MM_CINEMACHINE3
	[RequireComponent(typeof(CinemachineCamera))]
	#endif
	public class MMCinemachineZoom : MonoBehaviour
	{
		[Header("Channel通道")]
		[MMFInspectorGroup("Shaker Settings", true, 3)]
		/// whether to listen on a channel defined by an int or by a MMChannel scriptable object. Ints are simple to setup but can get messy and make it harder to remember what int corresponds to what.
		/// MMChannel scriptable objects require you to create them in advance, but come with a readable name and are more scalable
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

		[Header("Transition Speed过渡速度")]
		/// the animation curve to apply to the zoom transition
		[Tooltip("应用于缩放过渡效果的动画曲线")]
		public MMTweenType ZoomTween = new MMTweenType( new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f)));

		[Header("Test Zoom测试缩放")]
		/// the mode to apply the zoom in when using the test button in the inspector
		[Tooltip("在检查器中使用测试按钮时应用放大操作的模式")]
		public MMCameraZoomModes TestMode;
		/// the target field of view to apply the zoom in when using the test button in the inspector
		[Tooltip("在检查器中使用测试按钮进行放大操作时要应用的目标视场。")]
		public float TestFieldOfView = 30f;
		/// the transition duration to apply the zoom in when using the test button in the inspector
		[Tooltip("在检查器中使用测试按钮进行放大操作时应用的过渡持续时间。")]
		public float TestTransitionDuration = 0.1f;
		/// the duration to apply the zoom in when using the test button in the inspector
		[Tooltip("在检查器中使用测试按钮进行放大操作时所应用的持续时间。 ")]
		public float TestDuration = 0.05f;

		[MMFInspectorButton("TestZoom")]
        /// 一个用于在播放模式下测试缩放功能的检查器按钮。 
        public bool TestZoomButton;

		public virtual float GetTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.time : Time.unscaledTime; }
		public virtual float GetDeltaTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; }

		public virtual TimescaleModes TimescaleMode { get; set; }
        
		#if MM_CINEMACHINE
		protected Cinemachine.CinemachineVirtualCamera _virtualCamera;
		#elif MM_CINEMACHINE3
		protected CinemachineCamera _virtualCamera;
		#endif
		protected float _initialFieldOfView;
		protected MMCameraZoomModes _mode;
		protected bool _zooming = false;
		protected float _startFieldOfView;
		protected float _transitionDuration;
		protected float _duration;
		protected float _targetFieldOfView;
		protected float _elapsedTime = 0f;
		protected int _direction = 1;
		protected float _reachedDestinationTimestamp;
		protected bool _destinationReached = false;
		protected float _zoomStartedAt = 0f;

        /// <summary>
        /// 在唤醒（Awake）阶段，我们获取我们的虚拟相机。 
        /// </summary>
        protected virtual void Awake()
		{
			#if MM_CINEMACHINE
			_virtualCamera = this.gameObject.GetComponent<Cinemachine.CinemachineVirtualCamera>();
			_initialFieldOfView = _virtualCamera.m_Lens.FieldOfView;
			#elif MM_CINEMACHINE3
			_virtualCamera = this.gameObject.GetComponent<CinemachineCamera>();
			_initialFieldOfView = _virtualCamera.Lens.FieldOfView;
			#endif
		}

        /// <summary>
        /// 在 Update（更新）函数中，如果我们正在进行缩放操作，我们会相应地修改我们的视场。 
        /// </summary>
        protected virtual void Update()
		{
			if (!_zooming)
			{
				return;
			}

			_elapsedTime = GetTime() - _zoomStartedAt;
			if (_elapsedTime <= _transitionDuration)
			{
				float t = MMMaths.Remap(_elapsedTime, 0f, _transitionDuration, 0f, 1f);
				#if MM_CINEMACHINE
				_virtualCamera.m_Lens.FieldOfView = Mathf.LerpUnclamped(_startFieldOfView, _targetFieldOfView, ZoomTween.Evaluate(t));
				#elif MM_CINEMACHINE3
				_virtualCamera.Lens.FieldOfView = Mathf.LerpUnclamped(_startFieldOfView, _targetFieldOfView, ZoomTween.Evaluate(t));
				#endif
			}
			else
			{
				if (!_destinationReached)
				{
					_reachedDestinationTimestamp = GetTime();
					_destinationReached = true;
				}
				if ((_mode == MMCameraZoomModes.For) && (_direction == 1))
				{
					if (GetTime() - _reachedDestinationTimestamp > _duration)
					{
						_direction = -1;
						_zoomStartedAt = GetTime();
						_startFieldOfView = _targetFieldOfView;
						_targetFieldOfView = _initialFieldOfView;
					}                    
				}
				else
				{
					_zooming = false;
				}   
			}
		}

        /// <summary>
        /// 一种触发缩放的方法，理想情况下仅应通过事件来调用，但为了方便起见将其设为了公有方法。 
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="newFieldOfView"></param>
        /// <param name="transitionDuration"></param>
        /// <param name="duration"></param>
        public virtual void Zoom(MMCameraZoomModes mode, float newFieldOfView, float transitionDuration, float duration, bool useUnscaledTime, bool relative = false, MMTweenType tweenType = null)
		{
			if (_zooming)
			{
				return;
			}

			_zooming = true;
			_elapsedTime = 0f;
			_mode = mode;

			TimescaleMode = useUnscaledTime ? TimescaleModes.Unscaled : TimescaleModes.Scaled;
			#if MM_CINEMACHINE
			_startFieldOfView = _virtualCamera.m_Lens.FieldOfView;
			#elif MM_CINEMACHINE3
			_startFieldOfView = _virtualCamera.Lens.FieldOfView;
			#endif
			_transitionDuration = transitionDuration;
			_duration = duration;
			_transitionDuration = transitionDuration;
			_direction = 1;
			_destinationReached = false;
			_zoomStartedAt = GetTime();
			
			if (tweenType != null)
			{
				ZoomTween = tweenType;
			}

			switch (mode)
			{
				case MMCameraZoomModes.For:
					_targetFieldOfView = newFieldOfView;
					break;

				case MMCameraZoomModes.Set:
					_targetFieldOfView = newFieldOfView;
					break;

				case MMCameraZoomModes.Reset:
					_targetFieldOfView = _initialFieldOfView;
					break;
			}

			if (relative)
			{
				_targetFieldOfView += _initialFieldOfView;
			}
		}

        /// <summary>
        /// 测试按钮用于触发测试缩放操作所使用的方法。 
        /// </summary>
        protected virtual void TestZoom()
		{
			Zoom(TestMode, TestFieldOfView, TestTransitionDuration, TestDuration, false);
		}

        /// <summary>
        /// 当我们接收到一个 MMCameraZoomEvent（MM相机缩放事件）时，我们会调用我们的缩放方法。 
        /// </summary>
        /// <param name="zoomEvent"></param>
        public virtual void OnCameraZoomEvent(MMCameraZoomModes mode, float newFieldOfView, float transitionDuration, float duration, MMChannelData channelData, 
			bool useUnscaledTime, bool stop = false, bool relative = false, bool restore = false, MMTweenType tweenType = null)
		{
			if (!MMChannel.Match(channelData, ChannelMode, Channel, MMChannelDefinition))
			{
				return;
			}
			if (stop)
			{
				_zooming = false;
				return;
			}
			if (restore)
			{
				#if MM_CINEMACHINE
				_virtualCamera.m_Lens.FieldOfView = _initialFieldOfView;
				#elif MM_CINEMACHINE3
				_virtualCamera.Lens.FieldOfView = _initialFieldOfView;
				#endif
				return;
			}
			this.Zoom(mode, newFieldOfView, transitionDuration, duration, useUnscaledTime, relative, tweenType);
		}

        /// <summary>
        /// 开始监听 MMCameraZoomEvent（MM相机缩放事件） 。 
        /// </summary>
        protected virtual void OnEnable()
		{
			MMCameraZoomEvent.Register(OnCameraZoomEvent);
		}

        /// <summary>
        /// 停止监听 MMCameraZoomEvent（MM相机缩放事件）。 
        /// </summary>
        protected virtual void OnDisable()
		{
			MMCameraZoomEvent.Unregister(OnCameraZoomEvent);
		}
	}
}