using UnityEngine;
using UnityEngine.Rendering;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
#if MM_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace MoreMountains.FeedbacksForThirdParty
{
    /// <summary>
    /// 将这个类添加到一个带有高动态范围渲染管线（HDRP）色彩调整后期处理功能的相机上，这样它就能通过接收事件来“抖动”其相关参数值。 
    /// </summary>
#if MM_HDRP
	[RequireComponent(typeof(Volume))]
#endif
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/PostProcessing/MM Channel Mixer Shaker HDRP")]
	public class MMChannelMixerShaker_HDRP : MMShaker
	{
        /// 是否要添加到初始值当中
        public bool RelativeValues = true;

		[MMInspectorGroup("Red", true, 42)]
		/// the curve used to animate the red value on
		[Tooltip("用于对红色值进行动画处理的曲线。")]
		public AnimationCurve ShakeRed = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[Range(-200f, 200f)]
		public float RemapRedZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[Range(-200f, 200f)]
		public float RemapRedOne = 200f;

		[MMInspectorGroup("Green", true, 43)]
		/// the curve used to animate the green value on
		[Tooltip("用于对绿色值进行动画处理的曲线。")]
		public AnimationCurve ShakeGreen = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[Range(-200f, 200f)]
		public float RemapGreenZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[Range(-200f, 200f)]
		public float RemapGreenOne = 200f;

		[MMInspectorGroup("Blue", true, 44)]
		/// the curve used to animate the blue value on
		[Tooltip("用于对蓝色值进行动画处理的曲线。")]
		public AnimationCurve ShakeBlue = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[Range(-200f, 200f)]
		public float RemapBlueZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[Range(-200f, 200f)]
		public float RemapBlueOne = 200f;

#if MM_HDRP
		protected Volume _volume;
		protected ChannelMixer _channelMixer;
		protected float _initialRed;
		protected float _initialGreen;
		protected float _initialBlue;
		protected float _initialContrast;
		protected Color _initialColorFilterColor;
		protected float _originalShakeDuration;
		protected bool _originalRelativeValues;
	        
		protected AnimationCurve _originalShakeRed;
		protected float _originalRemapRedZero;
		protected float _originalRemapRedOne;
		protected AnimationCurve _originalShakeGreen;
		protected float _originalRemapGreenZero;
		protected float _originalRemapGreenOne;
		protected AnimationCurve _originalShakeBlue;
		protected float _originalRemapBlueZero;
		protected float _originalRemapBlueOne;

		/// <summary>
		/// 在初始化时，我们初始化我们的值
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_volume = this.gameObject.GetComponent<Volume>();
			_volume.profile.TryGet(out _channelMixer);
		}

		/// <summary>
		/// 当添加那个抖动器时，我们会初始化它的抖动持续时间。 
		/// </summary>
		protected virtual void Reset()
		{
			ShakeDuration = 0.8f;
		}

		/// <summary>
		/// 随着时间推移使数值产生晃动变化。
		/// </summary>
		protected override void Shake()
		{
			float newRed = ShakeFloat(ShakeRed, RemapRedZero, RemapRedOne, RelativeValues, _initialRed);
			_channelMixer.redOutRedIn.Override(newRed);
			float newGreen = ShakeFloat(ShakeGreen, RemapGreenZero, RemapGreenOne, RelativeValues, _initialGreen);
			_channelMixer.greenOutGreenIn.Override(newGreen);
			float newBlue = ShakeFloat(ShakeBlue, RemapBlueZero, RemapBlueOne, RelativeValues, _initialBlue);
			_channelMixer.blueOutBlueIn.Override(newBlue);
		}

		/// <summary>
		/// 收集目标对象上的初始值。
		/// </summary>
		protected override void GrabInitialValues()
		{
			_initialRed = _channelMixer.redOutRedIn.value;
			_initialGreen = _channelMixer.greenOutGreenIn.value;
			_initialBlue = _channelMixer.blueOutBlueIn.value;
		}

		/// <summary>
		/// 当我们接收到合适的事件时，我们就会触发一次抖动。
		/// </summary>
		/// <param name="intensity"></param>
		/// <param name="duration"></param>
		/// <param name="amplitude"></param>
		/// <param name="relativeIntensity"></param>
		/// <param name="attenuation"></param>
		/// <param name="channel"></param>
		public virtual void OnMMChannelMixerShakeEvent(AnimationCurve shakeRed, float remapRedZero, float remapRedOne,
			AnimationCurve shakeGreen, float remapGreenZero, float remapGreenOne,
			AnimationCurve shakeBlue, float remapBlueZero, float remapBlueOne,
			float duration, bool relativeValues = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			if (!CheckEventAllowed(channelData) || (!Interruptible && Shaking))
			{
				return;
			}
	            
			if (stop)
			{
				Stop();
				return;
			}

			if (restore)
			{
				ResetTargetValues();
				return;
			}

			_resetShakerValuesAfterShake = resetShakerValuesAfterShake;
			_resetTargetValuesAfterShake = resetTargetValuesAfterShake;

			if (resetShakerValuesAfterShake)
			{
				_originalShakeDuration = ShakeDuration;
				_originalRelativeValues = RelativeValues;
				_originalShakeRed = ShakeRed;
				_originalRemapRedZero = RemapRedZero;
				_originalRemapRedOne = RemapRedOne;
				_originalShakeGreen = ShakeGreen;
				_originalRemapGreenZero = RemapGreenZero;
				_originalRemapGreenOne = RemapGreenOne;
				_originalShakeBlue = ShakeBlue;
				_originalRemapBlueZero = RemapBlueZero;
				_originalRemapBlueOne = RemapBlueOne;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				RelativeValues = relativeValues;
				ShakeRed = shakeRed;
				RemapRedZero = remapRedZero;
				RemapRedOne = remapRedOne;
				ShakeGreen = shakeGreen;
				RemapGreenZero = remapGreenZero;
				RemapGreenOne = remapGreenOne;
				ShakeBlue = shakeBlue;
				RemapBlueZero = remapBlueZero;
				RemapBlueOne = remapBlueOne;
				ForwardDirection = forwardDirection;
			}

			Play();
		}

		/// <summary>
		/// 重置目标的数值。
		/// </summary>
		protected override void ResetTargetValues()
		{
			base.ResetTargetValues();
			_channelMixer.redOutRedIn.Override(_initialRed);
			_channelMixer.greenOutGreenIn.Override(_initialGreen);
			_channelMixer.blueOutBlueIn.Override(_initialBlue);
		}

		/// <summary>
		/// 重置抖动器的数值。
		/// </summary>
		protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalShakeDuration;
			RelativeValues = _originalRelativeValues;
			ShakeRed = _originalShakeRed;
			RemapRedZero = _originalRemapRedZero;
			RemapRedOne = _originalRemapRedOne;
			ShakeGreen = _originalShakeGreen;
			RemapGreenZero = _originalRemapGreenZero;
			RemapGreenOne = _originalRemapGreenOne;
			ShakeBlue = _originalShakeBlue;
			RemapBlueZero = _originalRemapBlueZero;
			RemapBlueOne = _originalRemapBlueOne;
		}

		/// <summary>
		/// 开始监听事件
		/// </summary>
		public override void StartListening()
		{
			base.StartListening();
			MMChannelMixerShakeEvent_HDRP.Register(OnMMChannelMixerShakeEvent);
		}

		/// <summary>
		/// 停止监听事件
		/// </summary>
		public override void StopListening()
		{
			base.StopListening();
			MMChannelMixerShakeEvent_HDRP.Unregister(OnMMChannelMixerShakeEvent);
		}
#endif
    }

    /// <summary>
    /// 用于触发渐晕（画面边缘模糊或变暗效果）抖动的一个事件。
    /// </summary>
    public struct MMChannelMixerShakeEvent_HDRP
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }
		
		public delegate void Delegate(
			AnimationCurve shakeRed, float remapRedZero, float remapRedOne,
			AnimationCurve shakeGreen, float remapGreenZero, float remapGreenOne,
			AnimationCurve shakeBlue, float remapBlueZero, float remapBlueOne,
			float duration, bool relativeValues = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false);

		static public void Trigger(
			AnimationCurve shakeRed, float remapRedZero, float remapRedOne,
			AnimationCurve shakeGreen, float remapGreenZero, float remapGreenOne,
			AnimationCurve shakeBlue, float remapBlueZero, float remapBlueOne,
			float duration, bool relativeValues = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke(shakeRed, remapRedZero, remapRedOne,
				shakeGreen, remapGreenZero, remapGreenOne,
				shakeBlue, remapBlueZero, remapBlueOne,
				duration, relativeValues, attenuation, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore);
		}
	}
}