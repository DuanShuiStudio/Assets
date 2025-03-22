using UnityEngine;
#if MM_UI
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using System;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
	public struct MMFlashEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(Color flashColor, float duration, float alpha, int flashID, MMChannelData channelData, TimescaleModes timescaleMode, bool stop = false);

		static public void Trigger(Color flashColor, float duration, float alpha, int flashID, MMChannelData channelData, TimescaleModes timescaleMode, bool stop = false)
		{
			OnEvent?.Invoke(flashColor, duration, alpha, flashID, channelData, timescaleMode, stop);
		}
	}

	[Serializable]
	public class MMFlashDebugSettings
	{
		/// whether to listen on a channel defined by an int or by a MMChannel scriptable object. Ints are simple to setup but can get messy and make it harder to remember what int corresponds to what.
		/// MMChannel scriptable objects require you to create them in advance, but come with a readable name and are more scalable
		[Tooltip("是通过一个整数所定义的通道进行监听，还是通过一个MMChannel可编写脚本对象所定义的通道进行监听。整数设置起来很简单，但可能会变得混乱，并且更难记住哪个整数对应着什么内容。" +
                 "MMChannel可编写脚本对象要求你提前创建它们，但它们带有易读的名称，并且具有更强的可扩展性。")]
		public MMChannelModes ChannelMode = MMChannelModes.Int;
		/// the channel to listen to - has to match the one on the feedback
		[Tooltip("要监听的通道——必须与反馈中的通道相匹配")]
		[MMFEnumCondition("ChannelMode", (int)MMChannelModes.Int)]
		public int Channel = 0;
		/// the MMChannel definition asset to use to listen for events. The feedbacks targeting this shaker will have to reference that same MMChannel definition to receive events - to create a MMChannel,
		/// right click anywhere in your project (usually in a Data folder) and go MoreMountains > MMChannel, then name it with some unique name
		[Tooltip("用于监听事件的 MMChannel 定义资源。针对此抖动器的反馈必须引用相同的 MMChannel 定义才能接收事件。" +
                 "要创建一个 MMChannel，在项目中的任意位置（通常是在一个数据文件夹中）右键单击，然后选择 “MoreMountains> MMChannel")]
		[MMFEnumCondition("ChannelMode", (int)MMChannelModes.MMChannel)]
		public MMChannel MMChannelDefinition = null;
        /// 闪光灯的颜色
        public Color FlashColor = Color.white;
        /// 闪光灯持续时间（以秒为单位）
        public float FlashDuration = 0.2f;
        /// 闪光灯的透明度（阿尔法通道值） 
        public float FlashAlpha = 1f;
        /// 闪光灯的标识（通常为0）。你可以在每个MMFlash对象上指定一个标识，这样就能在一个场景中拥有不同的闪光灯图像，并分别调用它们（一个用于表示受到伤害，一个用于表示拾取生命值道具等等）。 
        public int FlashID = 0;
	}
    
	[RequireComponent(typeof(Image))]
	[RequireComponent(typeof(CanvasGroup))]
	[AddComponentMenu("More Mountains/Feedbacks/Shakers/Various/MM Flash")]
    /// <summary>
    /// 将这个类添加到一张图片上，当接收到MMFlashEvent（多媒体闪烁事件）时，这张图片就会闪烁。 
    /// </summary>
    public class MMFlash : MMMonoBehaviour
	{
		[MMInspectorGroup("Flash", true, 121)] 
		/// whether to listen on a channel defined by an int or by a MMChannel scriptable object. Ints are simple to setup but can get messy and make it harder to remember what int corresponds to what.
		/// MMChannel scriptable objects require you to create them in advance, but come with a readable name and are more scalable
		[Tooltip("是通过一个整数所定义的通道进行监听，还是通过一个MMChannel可编写脚本对象所定义的通道进行监听。整数设置起来很简单，但可能会变得混乱，并且更难记住哪个整数对应着什么内容。" +
                 "MMChannel可编写脚本对象要求你提前创建它们，但它们带有易读的名称，并且具有更强的可扩展性。")]
		public MMChannelModes ChannelMode = MMChannelModes.Int;
		/// the channel to listen to - has to match the one on the feedback
		[Tooltip("要监听的通道——必须与反馈中的通道相匹配")]
		[MMFEnumCondition("ChannelMode", (int)MMChannelModes.Int)]
		public int Channel = 0;
		/// the MMChannel definition asset to use to listen for events. The feedbacks targeting this shaker will have to reference that same MMChannel definition to receive events - to create a MMChannel,
		/// right click anywhere in your project (usually in a Data folder) and go MoreMountains > MMChannel, then name it with some unique name
		[Tooltip("用于监听事件的 MMChannel 定义资源。针对此抖动器的反馈必须引用相同的 MMChannel 定义才能接收事件。 " +
                 "创建一个 MMChannel，在项目中的任意位置（通常是在一个数据文件夹中）右键单击，然后选择 “MoreMountains> MMChannel")]
		[MMFEnumCondition("ChannelMode", (int)MMChannelModes.MMChannel)]
		public MMChannel MMChannelDefinition = null;
		/// the ID of this MMFlash object. When triggering a MMFlashEvent you can specify an ID, and only MMFlash objects with this ID will answer the call and flash, allowing you to have more than one flash object in a scene
		[Tooltip("这个MMFlash对象的ID。当触发一个MMFlashEvent（多媒体闪烁事件）时，你可以指定一个ID，只有具有该ID的MMFlash对象才会响应调用并闪烁，这样一来，你就可以在一个场景中拥有多个闪烁对象。 ")]
		public int FlashID = 0;
		/// if this is true, the MMFlash will stop before playing on every new event received
		[Tooltip("如果这为真，那么每当接收到新的事件时，MMFlash（多媒体闪烁对象）会在播放之前就停止。 ")]
		public bool Interruptable = false;
		
		[MMInspectorGroup("Interpolation", true, 122)] 
		/// the animation curve to use when flashing in
		[Tooltip("闪烁进入时所使用的动画曲线 ")]
		public MMTweenType FlashInTween = new MMTweenType(MMTween.MMTweenCurve.LinearTween);
		/// the animation curve to use when flashing out
		[Tooltip("闪烁退出时所使用的动画曲线。")]
		public MMTweenType FlashOutTween = new MMTweenType(MMTween.MMTweenCurve.LinearTween);

		[MMInspectorGroup("Debug", true, 123)] 
		/// the set of test settings to use when pressing the DebugTest button
		[Tooltip("按下“DebugTest（调试测试）”按钮时要使用的那组测试设置。 ")]
		public MMFlashDebugSettings DebugSettings;
		/// a test button that calls the DebugTest method
		[Tooltip("一个会调用“DebugTest（调试测试）”方法的测试按钮。 ")]
		[MMFInspectorButton("DebugTest")]
		public bool DebugTestButton;

		public virtual float GetTime() { return (_timescaleMode == TimescaleModes.Scaled) ? Time.time : Time.unscaledTime; }
		public virtual float GetDeltaTime() { return (_timescaleMode == TimescaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; }

		protected Image _image;
		protected CanvasGroup _canvasGroup;
		protected bool _flashing = false;
		protected float _targetAlpha;
		protected Color _initialColor;
		protected float _delta;
		protected float _flashStartedTimestamp;
		protected int _direction = 1;
		protected float _duration;
		protected TimescaleModes _timescaleMode;
		protected MMTweenType _currentTween;

        /// <summary>
        /// 在开始时，我们获取我们的图像组件。 
        /// </summary>
        protected virtual void Start()
		{
			_image = GetComponent<Image>();
			_canvasGroup = GetComponent<CanvasGroup>();
			_initialColor = _image.color;
		}

        /// <summary>
        /// 在每帧更新时，如果有需要的话，我们就使我们的图像闪烁。 
        /// </summary>
        protected virtual void Update()
		{
			if (_flashing)
			{
				_image.enabled = true;

				_currentTween = FlashInTween;
				if (GetTime() - _flashStartedTimestamp > _duration / 2f)
				{
					_direction = -1;
					_currentTween = FlashOutTween;
				}
				
				if (_direction == 1)
				{
					_delta += GetDeltaTime() / (_duration / 2f);
				}
				else
				{
					_delta -= GetDeltaTime() / (_duration / 2f);
				}
                
				if (GetTime() - _flashStartedTimestamp > _duration)
				{
					_flashing = false;
				}
				
				float percent = MMMaths.Remap(_delta, 0f, _duration/2f, 0f, 1f);
				float tweenValue = _currentTween.Evaluate(percent);

				_canvasGroup.alpha = Mathf.Lerp(0f, _targetAlpha, tweenValue);
			}
			else
			{
				_image.enabled = false;
			}
		}

		public virtual void DebugTest()
		{
			MMFlashEvent.Trigger(DebugSettings.FlashColor, DebugSettings.FlashDuration, DebugSettings.FlashAlpha, DebugSettings.FlashID, new MMChannelData(DebugSettings.ChannelMode, DebugSettings.Channel, DebugSettings.MMChannelDefinition), TimescaleModes.Unscaled);
		}

        /// <summary>
        /// 当接收到一个闪烁事件时，我们会开启我们的图像显示。  
        /// </summary>
        public virtual void OnMMFlashEvent(Color flashColor, float duration, float alpha, int flashID, MMChannelData channelData, TimescaleModes timescaleMode, bool stop = false)
		{
			if (flashID != FlashID) 
			{
				return;
			}
            
			if (stop)
			{
				_flashing = false;
				return;
			}

			if (!MMChannel.Match(channelData, ChannelMode, Channel, MMChannelDefinition))
			{
				return;
			}

			Flash(flashColor, duration, alpha, timescaleMode);
		}

		public virtual void Flash(Color flashColor, float duration, float alpha, TimescaleModes timescaleMode)
		{
			if (_flashing && Interruptable)
			{
				_flashing = false;
			}

			if (!_flashing)
			{
				_flashing = true;
				_direction = 1;
				_canvasGroup.alpha = 0;
				_targetAlpha = alpha;
				_delta = 0f;
				_image.color = flashColor;
				_duration = duration;
				_timescaleMode = timescaleMode;
				_flashStartedTimestamp = GetTime();
			}
		}

        /// <summary>
        /// 当启用（该组件或对象）时，我们开始监听事件。 
        /// </summary>
        protected virtual void OnEnable()
		{
			MMFlashEvent.Register(OnMMFlashEvent);
		}

        /// <summary>
        /// 当禁用（该组件或对象）时，我们停止监听事件。 
        /// </summary>
        protected virtual void OnDisable()
		{
			MMFlashEvent.Unregister(OnMMFlashEvent);
		}		
	}
}
#endif