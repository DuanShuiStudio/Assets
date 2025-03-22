using UnityEngine;
using MoreMountains.Feedbacks;
using UnityEngine.Rendering;
using MoreMountains.Tools;
#if MM_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace MoreMountains.FeedbacksForThirdParty
{
    /// <summary>
    /// 将这个类添加到一个配备了高动态范围渲染管线（HDRP）渐晕（暗角）后期处理功能的相机上，它就能够通过接收事件来“晃动”其相关参数值。 
    /// </summary>
#if MM_HDRP
	[RequireComponent(typeof(Volume))]
#endif
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/PostProcessing/MM Panini Projection Shaker HDRP")]
	public class MMPaniniProjectionShaker_HDRP : MMShaker
	{
		[MMInspectorGroup("Panini Projection Distance", true, 49)]
		/// whether or not to add to the initial value
		[Tooltip("是否要添加到初始值当中")]
		public bool RelativeDistance = false;
		/// the curve used to animate the distance value on
		[Tooltip("用于对强度值进行动画处理的曲线")]
		public AnimationCurve ShakeDistance = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[Range(0f, 1f)]
		public float RemapDistanceZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[Range(0f, 1f)]
		public float RemapDistanceOne = 1f;

#if MM_HDRP
		protected Volume _volume;
		protected PaniniProjection _paniniProjection;
		protected float _initialDistance;
		protected float _originalShakeDuration;
		protected AnimationCurve _originalShakeDistance;
		protected float _originalRemapDistanceZero;
		protected float _originalRemapDistanceOne;
		protected bool _originalRelativeDistance;

		/// <summary>
		/// 在初始化时，我们初始化我们的值
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_volume = this.gameObject.GetComponent<Volume>();
			_volume.profile.TryGet(out _paniniProjection);
		}

		/// <summary>
		/// 随着时间推移使数值产生晃动变化
		/// </summary>
		protected override void Shake()
		{
			float newValue = ShakeFloat(ShakeDistance, RemapDistanceZero, RemapDistanceOne, RelativeDistance, _initialDistance);
			_paniniProjection.distance.Override(newValue);
		}

		/// <summary>
		/// 收集目标对象上的初始值。 
		/// </summary>
		protected override void GrabInitialValues()
		{
			_initialDistance = _paniniProjection.distance.value;
		}

		/// <summary>
		/// 当我们接收到合适的事件时，我们就会触发一次抖动。
		/// </summary>
		/// <param name="distance"></param>
		/// <param name="duration"></param>
		/// <param name="amplitude"></param>
		/// <param name="relativeDistance"></param>
		/// <param name="attenuation"></param>
		/// <param name="channel"></param>
		public virtual void OnPaniniProjectionShakeEvent(AnimationCurve distance, float duration, float remapMin, float remapMax, bool relativeDistance = false,
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
				_originalShakeDistance = ShakeDistance;
				_originalRemapDistanceZero = RemapDistanceZero;
				_originalRemapDistanceOne = RemapDistanceOne;
				_originalRelativeDistance = RelativeDistance;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				ShakeDistance = distance;
				RemapDistanceZero = remapMin * attenuation;
				RemapDistanceOne = remapMax * attenuation;
				RelativeDistance = relativeDistance;
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
			_paniniProjection.distance.Override(_initialDistance);
		}

		/// <summary>
		/// 重置抖动器的数值。
		/// </summary>
		protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalShakeDuration;
			ShakeDistance = _originalShakeDistance;
			RemapDistanceZero = _originalRemapDistanceZero;
			RemapDistanceOne = _originalRemapDistanceOne;
			RelativeDistance = _originalRelativeDistance;
		}

		/// <summary>
		/// 开始监听事件。
		/// </summary>
		public override void StartListening()
		{
			base.StartListening();
			MMPaniniProjectionShakeEvent_HDRP.Register(OnPaniniProjectionShakeEvent);
		}

		/// <summary>
		/// 停止监听事件
		/// </summary>
		public override void StopListening()
		{
			base.StopListening();
			MMPaniniProjectionShakeEvent_HDRP.Unregister(OnPaniniProjectionShakeEvent);
		}
#endif
    }

    /// <summary>
    /// 用于触发渐晕（画面边缘模糊或变暗效果）抖动的一个事件。
    /// </summary>
    public struct MMPaniniProjectionShakeEvent_HDRP
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }
		
		public delegate void Delegate(AnimationCurve distance, float duration, float remapMin, float remapMax, bool relativeDistance = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false);
		
		static public void Trigger(AnimationCurve distance, float duration, float remapMin, float remapMax, bool relativeDistance = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke(distance, duration, remapMin, remapMax, relativeDistance, attenuation, channelData, resetShakerValuesAfterShake, 
				resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore);
		}
	}
}