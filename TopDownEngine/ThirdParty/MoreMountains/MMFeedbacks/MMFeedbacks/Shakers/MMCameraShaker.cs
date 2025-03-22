using UnityEngine;
using System;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
	[Serializable]
    /// <summary>
    /// 摄像机抖动属性
    /// </summary>
    public struct MMCameraShakeProperties
	{
		public float Duration;
		public float Amplitude;
		public float Frequency;
		public float AmplitudeX;
		public float AmplitudeY;
		public float AmplitudeZ;

		public MMCameraShakeProperties(float duration, float amplitude, float frequency, float amplitudeX = 0f, float amplitudeY = 0f, float amplitudeZ = 0f)
		{
			Duration = duration;
			Amplitude = amplitude;
			Frequency = frequency;
			AmplitudeX = amplitudeX;
			AmplitudeY = amplitudeY;
			AmplitudeZ = amplitudeZ;
		}
	}

	public enum MMCameraZoomModes { For, Set, Reset }

	public struct MMCameraZoomEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(MMCameraZoomModes mode, float newFieldOfView, float transitionDuration, float duration, MMChannelData channelData, bool useUnscaledTime = false, bool stop = false, bool relative = false, bool restore = false, MMTweenType tweenType = null);

		static public void Trigger(MMCameraZoomModes mode, float newFieldOfView, float transitionDuration, float duration, MMChannelData channelData, bool useUnscaledTime = false, bool stop = false, bool relative = false, bool restore = false, MMTweenType tweenType = null)
		{
			OnEvent?.Invoke(mode, newFieldOfView, transitionDuration, duration, channelData, useUnscaledTime, stop, relative, restore, tweenType);
		}
	}

	public struct MMCameraShakeEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(float duration, float amplitude, float frequency, float amplitudeX, float amplitudeY, float amplitudeZ, bool infinite = false, MMChannelData channelData = null, bool useUnscaledTime = false);

		static public void Trigger(float duration, float amplitude, float frequency, float amplitudeX, float amplitudeY, float amplitudeZ, bool infinite = false, MMChannelData channelData = null, bool useUnscaledTime = false)
		{
			OnEvent?.Invoke(duration, amplitude, frequency, amplitudeX, amplitudeY, amplitudeZ, infinite, channelData, useUnscaledTime);
		}
	}

	public struct MMCameraShakeStopEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(MMChannelData channelData);

		static public void Trigger(MMChannelData channelData)
		{
			OnEvent?.Invoke(channelData);
		}
	}

	[RequireComponent(typeof(MMWiggle))]
	[AddComponentMenu("More Mountains/Feedbacks/Shakers/Camera/MM Camera Shaker")]
    /// <summary>
    /// 一个可以添加到你的摄像机上的类。它会监听“MMCameraShakeEvents”（摄像机抖动事件），并相应地使你的摄像机产生抖动效果。 
    /// </summary>
    public class MMCameraShaker : MonoBehaviour
	{
		/// whether to listen on a channel defined by an int or by a MMChannel scriptable object. Ints are simple to setup but can get messy and make it harder to remember what int corresponds to what.
		/// MMChannel scriptable objects require you to create them in advance, but come with a readable name and are more scalable
		[Tooltip("决定是依据一个整数来定义要监听的通道，还是依据一个 `MMChannel` 可脚本化对象来定义。使用整数设置起来很简单，但可能会导致情况变得混乱，并且更难记住每个整数对应的具体内容。  " +
                 "“MMChannel”可编写脚本对象要求你事先创建它们，但它们带有易于理解的名称，并且更具可扩展性。 ")]
		public MMChannelModes ChannelMode = MMChannelModes.Int;
		/// the channel to listen to - has to match the one on the feedback
		[Tooltip("要监听的通道——必须与反馈端上的通道相匹配。 ")]
		[MMEnumCondition("ChannelMode", (int)MMChannelModes.Int)]
		public int Channel = 0;
		/// the MMChannel definition asset to use to listen for events. The feedbacks targeting this shaker will have to reference that same MMChannel definition to receive events - to create a MMChannel,
		/// right click anywhere in your project (usually in a Data folder) and go MoreMountains > MMChannel, then name it with some unique name
		[Tooltip("用于监听事件的 “MMChannel” 定义资源。针对此抖动器的反馈必须引用相同的 “MMChannel” 定义才能接收事件。" +
                 "若要创建一个 “MMChannel”，在你的项目中的任意位置（通常是在一个数据文件夹里）右键单击，然后选择 “MoreMountains > MMChannel”，接着用一个唯一的名称为其命名。")]
		[MMEnumCondition("ChannelMode", (int)MMChannelModes.MMChannel)]
		public MMChannel MMChannelDefinition = null;
		/// a cooldown, in seconds, after a shake, during which no other shake can start
		[Tooltip("一次抖动之后的冷却时间（以秒为单位），在此期间内不会开始其他任何抖动。 ")]
		public float CooldownBetweenShakes = 0f;
	    
		protected MMWiggle _wiggle;
		protected float _shakeStartedTimestamp = -Single.MaxValue;

        /// <summary>
        /// 在脚本唤醒（Awake）时，获取 MM 抖动器（MMShaker）组件。
        /// </summary>
        protected virtual void Awake()
		{
			_wiggle = GetComponent<MMWiggle>();
		}

        /// <summary>
        /// 按照所需的振幅和频率，让摄像机抖动持续“持续时间（Duration）”秒。 
        /// </summary>
        /// <param name="duration">Duration.</param>
        /// <param name="amplitude">Amplitude.</param>
        /// <param name="frequency">Frequency.</param>
        public virtual void ShakeCamera(float duration, float amplitude, float frequency, float amplitudeX, float amplitudeY, float amplitudeZ, bool useUnscaledTime)
		{
			if (Time.unscaledTime - _shakeStartedTimestamp < CooldownBetweenShakes)
			{
				return;
			}
			
			if ((amplitudeX != 0f) || (amplitudeY != 0f) || (amplitudeZ != 0f))
			{
				_wiggle.PositionWiggleProperties.AmplitudeMin.x = -amplitudeX;
				_wiggle.PositionWiggleProperties.AmplitudeMin.y = -amplitudeY;
				_wiggle.PositionWiggleProperties.AmplitudeMin.z = -amplitudeZ;
                
				_wiggle.PositionWiggleProperties.AmplitudeMax.x = amplitudeX;
				_wiggle.PositionWiggleProperties.AmplitudeMax.y = amplitudeY;
				_wiggle.PositionWiggleProperties.AmplitudeMax.z = amplitudeZ;
			}
			else
			{
				_wiggle.PositionWiggleProperties.AmplitudeMin = Vector3.one * -amplitude;
				_wiggle.PositionWiggleProperties.AmplitudeMax = Vector3.one * amplitude;
			}

			_shakeStartedTimestamp = Time.unscaledTime;
			_wiggle.PositionWiggleProperties.UseUnscaledTime = useUnscaledTime;
			_wiggle.PositionWiggleProperties.FrequencyMin = frequency;
			_wiggle.PositionWiggleProperties.FrequencyMax = frequency;
			_wiggle.PositionWiggleProperties.NoiseFrequencyMin = frequency * Vector3.one;
			_wiggle.PositionWiggleProperties.NoiseFrequencyMax = frequency * Vector3.one;
			_wiggle.WigglePosition(duration);
		}

        /// <summary>
        /// 当捕获到一个“MM摄像机抖动事件（MMCameraShakeEvent）”时，使摄像机产生抖动。 
        /// </summary>
        /// <param name="shakeEvent">Shake event.</param>
        public virtual void OnCameraShakeEvent(float duration, float amplitude, float frequency, float amplitudeX, float amplitudeY, float amplitudeZ, bool infinite, MMChannelData channelData, bool useUnscaledTime)
		{
			if (!MMChannel.Match(channelData, ChannelMode, Channel, MMChannelDefinition))
			{
				return;
			}
			this.ShakeCamera (duration, amplitude, frequency, amplitudeX, amplitudeY, amplitudeZ, useUnscaledTime);
		}

        /// <summary>
        /// 在启用时，开始监听事件。
        /// </summary>
        protected virtual void OnEnable()
		{
			MMCameraShakeEvent.Register(OnCameraShakeEvent);
		}

        /// <summary>
        /// 在禁用时，停止监听事件。
        /// </summary>
        protected virtual void OnDisable()
		{
			MMCameraShakeEvent.Unregister(OnCameraShakeEvent);
		}

	}
}