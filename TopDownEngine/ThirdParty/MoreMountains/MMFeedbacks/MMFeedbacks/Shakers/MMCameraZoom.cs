using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;

namespace MoreMountains.FeedbacksForThirdParty
{
    /// <summary>
    /// 这个类将使你能够通过从任何其他类发送“MM摄像机缩放事件（MMCameraZoomEvents）”来触发摄像机的缩放操作。 
    /// </summary>
    [RequireComponent(typeof(Camera))]
	[AddComponentMenu("More Mountains/Feedbacks/Shakers/Camera/MM Camera Zoom")]
	public class MMCameraZoom : MonoBehaviour
	{
		[Header("Channel通道")]
		[MMFInspectorGroup("Shaker Settings", true, 3)]
		/// whether to listen on a channel defined by an int or by a MMChannel scriptable object. Ints are simple to setup but can get messy and make it harder to remember what int corresponds to what.
		/// MMChannel scriptable objects require you to create them in advance, but come with a readable name and are more scalable
		[Tooltip("是选择监听由整数定义的通道，还是由 MMChannel 可脚本化对象定义的通道。使用整数设置起来很简单，但可能会导致混乱，而且更难记住每个整数对应的具体内容 " +
                 "MMChannel 可编写脚本对象需要你提前创建它们，但它们带有易于理解的名称，并且具有更强的可扩展性。")]
		public MMChannelModes ChannelMode = MMChannelModes.Int;
		/// the channel to listen to - has to match the one on the feedback
		[Tooltip("要监听的通道——必须与反馈中的通道相匹配。 ")]
		[MMFEnumCondition("ChannelMode", (int)MMChannelModes.Int)]
		public int Channel = 0;
		/// the MMChannel definition asset to use to listen for events. The feedbacks targeting this shaker will have to reference that same MMChannel definition to receive events - to create a MMChannel,
		/// right click anywhere in your project (usually in a Data folder) and go MoreMountains > MMChannel, then name it with some unique name
		[Tooltip("用于监听事件的 MMChannel 定义资源。针对此抖动器的反馈必须引用相同的 MMChannel 定义才能接收事件。" +
                 "要创建一个 MMChannel，在项目中的任意位置（通常是在一个数据文件夹中）右键单击，然后选择 “MoreMountains> MMChannel”，接着用某个唯一的名称为其命名。")]
		[MMFEnumCondition("ChannelMode", (int)MMChannelModes.MMChannel)]
		public MMChannel MMChannelDefinition = null;
		
		[Header("Transition Speed过渡速度")]
		/// the animation curve to apply to the zoom transition
		[Tooltip("应用于缩放过渡的动画曲线")]
		public MMTweenType ZoomTween = new MMTweenType( new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f)));

		[Header("Test Zoom测试缩放")]
		/// the mode to apply the zoom in when using the test button in the inspector
		[Tooltip("在检查器中使用测试按钮时应用放大操作的模式 ")]
		public MMCameraZoomModes TestMode;
		/// the target field of view to apply the zoom in when using the test button in the inspector
		[Tooltip("当在检视器（Inspector）中使用测试按钮进行缩放操作时，要应用的目标视野（Field of View，简称 FOV）。")]
		public float TestFieldOfView = 30f;
		/// the transition duration to apply the zoom in when using the test button in the inspector
		[Tooltip("当在检查器中使用测试按钮进行放大操作时应用的过渡持续时间。")]
		public float TestTransitionDuration = 0.1f;
		/// the duration to apply the zoom in when using the test button in the inspector
		[Tooltip("当在检查器中使用测试按钮进行放大操作时应用的持续时间。")]
		public float TestDuration = 0.05f;

		[MMFInspectorButton("TestZoom")]
        /// 一个用于在播放模式下测试缩放功能的检查器（Inspector）按钮。
        public bool TestZoomButton;
        
		public virtual float GetTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.time : Time.unscaledTime; }
		public virtual float GetDeltaTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; }

		public virtual TimescaleModes TimescaleMode { get; set; }
        
		protected Camera _camera;
		protected float _initialFieldOfView;
		protected MMCameraZoomModes _mode;
		protected bool _zooming = false;
		protected float _startFieldOfView;
		protected float _transitionDuration;
		protected float _duration;
		protected float _targetFieldOfView;
		protected int _direction = 1;
		protected float _reachedDestinationTimestamp;
		protected bool _destinationReached = false;
		protected float _elapsedTime = 0f;
		protected float _zoomStartedAt = 0f;

        /// <summary>
        /// 在Awake方法中，我们获取了我们的虚拟相机。 
        /// </summary>
        protected virtual void Awake()
		{
			_camera = this.gameObject.GetComponent<Camera>();
			_initialFieldOfView = _camera.fieldOfView;
		}

        /// <summary>
        /// 在Update方法中，如果我们正在进行缩放操作，我们会相应地修改我们的视野范围。 
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
				_camera.fieldOfView = Mathf.LerpUnclamped(_startFieldOfView, _targetFieldOfView, ZoomTween.Evaluate(t));
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
        /// 一个用于触发缩放的方法，理想情况下仅通过事件来调用，但为了方便起见将其设置为了公有方法。 
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="newFieldOfView"></param>
        /// <param name="transitionDuration"></param>
        /// <param name="duration"></param>
        public virtual void Zoom(MMCameraZoomModes mode, float newFieldOfView, float transitionDuration, float duration, 
			bool useUnscaledTime, bool relative = false, MMTweenType tweenType = null)
		{
			if (_zooming)
			{
				return;
			}

			_zooming = true;
			_elapsedTime = 0f;
			_mode = mode;

			TimescaleMode = useUnscaledTime ? TimescaleModes.Unscaled : TimescaleModes.Scaled;
			_startFieldOfView = _camera.fieldOfView;
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
        /// 测试按钮用于触发测试缩放的方法
        /// </summary>
        protected virtual void TestZoom()
		{
			Zoom(TestMode, TestFieldOfView, TestTransitionDuration, TestDuration, false, tweenType: ZoomTween);
		}

        /// <summary>
        /// 当我们接收到一个MMCameraZoomEvent（多媒体相机缩放事件）时，我们会调用我们的缩放方法。 
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
				_camera.fieldOfView = _initialFieldOfView;
				return;
			}
			this.Zoom(mode, newFieldOfView, transitionDuration, duration, useUnscaledTime, relative, tweenType);
		}

        /// <summary>
        /// 开始监听多媒体相机缩放事件（MMCameraZoomEvents） 
        /// </summary>
        protected virtual void OnEnable()
		{
			MMCameraZoomEvent.Register(OnCameraZoomEvent);
		}

        /// <summary>
        /// 停止监听多媒体相机缩放事件（MMCameraZoomEvents） 
        /// </summary>
        protected virtual void OnDisable()
		{
			MMCameraZoomEvent.Unregister(OnCameraZoomEvent);
		}
	}
}