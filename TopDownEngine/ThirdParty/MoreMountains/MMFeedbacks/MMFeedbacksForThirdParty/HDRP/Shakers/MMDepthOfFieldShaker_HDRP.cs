﻿using UnityEngine;
using UnityEngine.Rendering;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
#if MM_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace MoreMountains.FeedbacksForThirdParty
{
    /// <summary>
    /// 将这个类添加到一个带有高动态范围渲染管线（HDRP）景深后期处理功能的相机上，它就能够通过接收事件来“抖动”其相关参数值。 
    /// </summary>
#if MM_HDRP
	[RequireComponent(typeof(Volume))]
#endif
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/PostProcessing/MM Depth Of Field Shaker HDRP")]
	public class MMDepthOfFieldShaker_HDRP : MMShaker
	{
		[MMInspectorGroup("Focus Distance", true, 53)]
		/// whether or not to animate the focus distance
		[Tooltip("是否对焦距进行动画效果处理。 ")]
		public bool AnimateFocusDistance = true;
		/// the curve used to animate the focus distance value on
		[Tooltip("用于对焦距值进行动画设置的曲线。 ")]
		[MMCondition("AnimateFocusDistance", true)]
		public AnimationCurve ShakeFocusDistance = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[MMCondition("AnimateFocusDistance", true)]
		public float RemapFocusDistanceZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[MMCondition("AnimateFocusDistance", true)]
		public float RemapFocusDistanceOne = 3f;
		
		
		[MMInspectorGroup("Near Range", true, 52)]
		
		[Header("Near Range Start近景范围起始点")]
		/// whether or not to animate the near range start
		[Tooltip("是否要对近景范围起始点进行动画处理 ")]
		public bool AnimateNearRangeStart = false;
		/// the curve used to animate the near range start on
		[Tooltip("用于使近景范围起始点产生动画效果的曲线。 ")]
		[MMCondition("AnimateNearRangeStart", true)]
		public AnimationCurve ShakeNearRangeStart = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[MMCondition("AnimateNearRangeStart", true)]
		public float RemapNearRangeStartZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[MMCondition("AnimateNearRangeStart", true)]
		public float RemapNearRangeStartOne = 3f;
		
		[Header("Near Range End近景范围终点")]
		/// whether or not to animate the near range end
		[Tooltip("是否要对近景范围终点进行动画处理")]
		public bool AnimateNearRangeEnd = false;
		/// the curve used to animate the near range end on
		[Tooltip("用于使近景范围终点产生动画效果的曲线。 ")]
		[MMCondition("AnimateNearRangeEnd", true)]
		public AnimationCurve ShakeNearRangeEnd = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[MMCondition("AnimateNearRangeEnd", true)]
		public float RemapNearRangeEndZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[MMCondition("AnimateNearRangeEnd", true)]
		public float RemapNearRangeEndOne = 3f;
		
		[MMInspectorGroup("Far Range", true, 51)]
		
		[Header("Far Range Start远景范围起始点")]
		/// whether or not to animate the far range start
		[Tooltip("是否要对远景范围起始点进行动画处理")]
		public bool AnimateFarRangeStart = false;
		/// the curve used to animate the far range start on
		[Tooltip("用于使远景范围起始点产生动画效果的曲线。 ")]
		[MMCondition("AnimateFarRangeStart", true)]
		public AnimationCurve ShakeFarRangeStart = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[MMCondition("AnimateFarRangeStart", true)]
		public float RemapFarRangeStartZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[MMCondition("AnimateFarRangeStart", true)]
		public float RemapFarRangeStartOne = 3f;
		
		[Header("Far Range End远景范围终点")]
		/// whether or not to animate the far range end
		[Tooltip("是否要对远景范围终点进行动画处理")]
		public bool AnimateFarRangeEnd = false;
		/// the curve used to animate the far range end on
		[Tooltip("用于使远景范围终点产生动画效果的曲线。 ")]
		[MMCondition("AnimateFarRangeEnd", true)]
		public AnimationCurve ShakeFarRangeEnd = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[MMCondition("AnimateFarRangeEnd", true)]
		public float RemapFarRangeEndZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[MMCondition("AnimateFarRangeEnd", true)]
		public float RemapFarRangeEndOne = 3f;

#if MM_HDRP
		protected Volume _volume;
		protected DepthOfField _depthOfField;
		protected float _originalShakeDuration;
		
		protected float _initialFocusDistance;
		protected float _initialNearRangeStart;
		protected float _initialNearRangeEnd;
		protected float _initialFarRangeStart;
		protected float _initialFarRangeEnd;

		protected bool _originalAnimateFocusDistance;
		protected AnimationCurve _originalShakeFocusDistance;
		protected float _originalRemapFocusDistanceZero;
		protected float _originalRemapFocusDistanceOne;
		
		protected bool _originalAnimateNearRangeStart;
		protected AnimationCurve _originalShakeNearRangeStart;
		protected float _originalRemapNearRangeStartZero;
		protected float _originalRemapNearRangeStartOne;
		protected bool _originalAnimateNearRangeEnd;
		protected AnimationCurve _originalShakeNearRangeEnd;
		protected float _originalRemapNearRangeEndZero;
		protected float _originalRemapNearRangeEndOne;
		protected bool _originalAnimateFarRangeStart;
		protected AnimationCurve _originalShakeFarRangeStart;
		protected float _originalRemapFarRangeStartZero;
		protected float _originalRemapFarRangeStartOne;
		protected bool _originalAnimateFarRangeEnd;
		protected AnimationCurve _originalShakeFarRangeEnd;
		protected float _originalRemapFarRangeEndZero;
		protected float _originalRemapFarRangeEndOne;

		/// <summary>
		/// 在初始化时，我们初始化我们的值
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_volume = this.gameObject.GetComponent<Volume>();
			_volume.profile.TryGet(out _depthOfField);
		}

		/// <summary>
		/// 随着时间推移使数值产生晃动变化。
		/// </summary>
		protected override void Shake()
		{
			if (AnimateFocusDistance)
			{
				float newValue = ShakeFloat(ShakeFocusDistance, RemapFocusDistanceZero, RemapFocusDistanceOne, false, _initialFocusDistance);
				_depthOfField.focusDistance.Override(newValue);	
			}
			if (AnimateNearRangeStart)
			{
				float newValue = ShakeFloat(ShakeNearRangeStart, RemapNearRangeStartZero, RemapNearRangeStartOne, false, _initialNearRangeStart);
				_depthOfField.nearFocusStart.Override(newValue);	
			}
			if (AnimateNearRangeEnd)
			{
				float newValue = ShakeFloat(ShakeNearRangeEnd, RemapNearRangeEndZero, RemapNearRangeEndOne, false, _initialNearRangeEnd);
				_depthOfField.nearFocusEnd.Override(newValue);	
			}
			if (AnimateFarRangeStart)
			{
				float newValue = ShakeFloat(ShakeFarRangeStart, RemapFarRangeStartZero, RemapFarRangeStartOne, false, _initialFarRangeStart);
				_depthOfField.farFocusStart.Override(newValue);	
			}
			if (AnimateFarRangeEnd)
			{
				float newValue = ShakeFloat(ShakeFarRangeEnd, RemapFarRangeEndZero, RemapFarRangeEndOne, false, _initialFarRangeEnd);
				_depthOfField.farFocusEnd.Override(newValue);	
			}
			
		}

		/// <summary>
		/// 收集目标对象上的初始值。
		/// </summary>
		protected override void GrabInitialValues()
		{
			_initialFocusDistance = _depthOfField.focusDistance.value;
			_initialNearRangeStart = _depthOfField.nearFocusStart.value;
			_initialNearRangeEnd = _depthOfField.nearFocusEnd.value;
			_initialFarRangeStart = _depthOfField.farFocusStart.value;
			_initialFarRangeEnd = _depthOfField.farFocusEnd.value;
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
		public virtual void OnDepthOfFieldShakeEvent(float duration, 
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false,
			bool animateFocusDistance = false, AnimationCurve shakeFocusDistance = null, float remapFocusDistanceZero = 0f, float remapFocusDistanceOne = 1f,
			bool animateNearRangeStart = false, AnimationCurve shakeNearRangeStart = null,float remapNearRangeStartZero = 0f, float remapNearRangeStartOne = 0f,
			bool animateNearRangeEnd = false, AnimationCurve shakeNearRangeEnd = null,float remapNearRangeEndZero = 0f, float remapNearRangeEndOne = 0f,
			bool animateFarRangeStart = false, AnimationCurve shakeFarRangeStart = null,float remapFarRangeStartZero = 0f, float remapFarRangeStartOne = 0f,
			bool animateFarRangeEnd = false, AnimationCurve shakeFarRangeEnd = null,float remapFarRangeEndZero = 0f, float remapFarRangeEndOne = 0f)
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
				
				_originalAnimateFocusDistance = AnimateFocusDistance;
				_originalShakeFocusDistance = ShakeFocusDistance;
				_originalRemapFocusDistanceZero = RemapFocusDistanceZero;
				_originalRemapFocusDistanceOne = RemapFocusDistanceOne;
				_originalAnimateNearRangeStart = AnimateNearRangeStart;
				_originalShakeNearRangeStart = ShakeNearRangeStart;
				_originalRemapNearRangeStartZero = RemapNearRangeStartZero;
				_originalRemapNearRangeStartOne = RemapNearRangeStartOne;
				_originalAnimateNearRangeEnd = AnimateNearRangeEnd;
				_originalShakeNearRangeEnd = ShakeNearRangeEnd;
				_originalRemapNearRangeEndZero = RemapNearRangeEndZero;
				_originalRemapNearRangeEndOne = RemapNearRangeEndOne;
				_originalAnimateFarRangeStart = AnimateFarRangeStart;
				_originalShakeFarRangeStart = ShakeFarRangeStart;
				_originalRemapFarRangeStartZero = RemapFarRangeStartZero;
				_originalRemapFarRangeStartOne = RemapFarRangeStartOne;
				_originalAnimateFarRangeEnd = AnimateFarRangeEnd;
				_originalShakeFarRangeEnd = ShakeFarRangeEnd;
				_originalRemapFarRangeEndZero = RemapFarRangeEndZero;
				_originalRemapFarRangeEndOne = RemapFarRangeEndOne;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				ForwardDirection = forwardDirection;

				AnimateFocusDistance = animateFocusDistance;
				ShakeFocusDistance = shakeFocusDistance;
				RemapFocusDistanceZero = remapFocusDistanceZero;
				RemapFocusDistanceOne = remapFocusDistanceOne;
				
				AnimateNearRangeStart = animateNearRangeStart;
				ShakeNearRangeStart = shakeNearRangeStart;
				RemapNearRangeStartZero = remapNearRangeStartZero;
				RemapNearRangeStartOne = remapNearRangeStartOne;
				AnimateNearRangeEnd = animateNearRangeEnd;
				ShakeNearRangeEnd = shakeNearRangeEnd;
				RemapNearRangeEndZero = remapNearRangeEndZero;
				RemapNearRangeEndOne = remapNearRangeEndOne;
				AnimateFarRangeStart = animateFarRangeStart;
				ShakeFarRangeStart = shakeFarRangeStart;
				RemapFarRangeStartZero = remapFarRangeStartZero;
				RemapFarRangeStartOne = remapFarRangeStartOne;
				AnimateFarRangeEnd = animateFarRangeEnd;
				ShakeFarRangeEnd = shakeFarRangeEnd;
				RemapFarRangeEndZero = remapFarRangeEndZero;
				RemapFarRangeEndOne = remapFarRangeEndOne;
			}

			Play();
		}

		/// <summary>
		/// 重置目标的数值。
		/// </summary>
		protected override void ResetTargetValues()
		{
			base.ResetTargetValues();
			_depthOfField.focusDistance.Override(_initialFocusDistance);
			_depthOfField.nearFocusStart.Override(_initialNearRangeStart);
			_depthOfField.nearFocusEnd.Override(_initialNearRangeEnd);
			_depthOfField.farFocusStart.Override(_initialFarRangeStart);
			_depthOfField.farFocusEnd.Override(_initialFarRangeEnd);
		}

		/// <summary>
		/// 重置抖动器的数值。
		/// </summary>
		protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalShakeDuration;
			
			AnimateFocusDistance = _originalAnimateFocusDistance;
			ShakeFocusDistance = _originalShakeFocusDistance;
			RemapFocusDistanceZero = _originalRemapFocusDistanceZero;
			RemapFocusDistanceOne = _originalRemapFocusDistanceOne;
			AnimateNearRangeStart = _originalAnimateNearRangeStart;
			ShakeNearRangeStart = _originalShakeNearRangeStart;
			RemapNearRangeStartZero = _originalRemapNearRangeStartZero;
			RemapNearRangeStartOne = _originalRemapNearRangeStartOne;
			AnimateNearRangeEnd = _originalAnimateNearRangeEnd;
			ShakeNearRangeEnd = _originalShakeNearRangeEnd;
			RemapNearRangeEndZero = _originalRemapNearRangeEndZero;
			RemapNearRangeEndOne = _originalRemapNearRangeEndOne;
			AnimateFarRangeStart = _originalAnimateFarRangeStart;
			ShakeFarRangeStart = _originalShakeFarRangeStart;
			RemapFarRangeStartZero = _originalRemapFarRangeStartZero;
			RemapFarRangeStartOne = _originalRemapFarRangeStartOne;
			AnimateFarRangeEnd = _originalAnimateFarRangeEnd;
			ShakeFarRangeEnd = _originalShakeFarRangeEnd;
			RemapFarRangeEndZero = _originalRemapFarRangeEndZero;
			RemapFarRangeEndOne = _originalRemapFarRangeEndOne;
		}

		/// <summary>
		/// 开始监听事件。
		/// </summary>
		public override void StartListening()
		{
			base.StartListening();
			MMDepthOfFieldShakeEvent_HDRP.Register(OnDepthOfFieldShakeEvent);
		}

		/// <summary>
		/// 停止监听事件
		/// </summary>
		public override void StopListening()
		{
			base.StopListening();
			MMDepthOfFieldShakeEvent_HDRP.Unregister(OnDepthOfFieldShakeEvent);
		}
#endif
    }

    /// <summary>
    /// 用于触发渐晕（画面边缘模糊或变暗效果）抖动的一个事件。 
    /// </summary>
    public struct MMDepthOfFieldShakeEvent_HDRP
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }
		
		public delegate void Delegate(float duration, 
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false,
			bool animateFocusDistance = false, AnimationCurve shakeFocusDistance = null, float remapFocusDistanceZero = 0f, float remapFocusDistanceOne = 1f,
			bool animateNearRangeStart = false, AnimationCurve shakeNearRangeStart = null,float remapNearRangeStartZero = 0f, float remapNearRangeStartOne = 0f,
			bool animateNearRangeEnd = false, AnimationCurve shakeNearRangeEnd = null,float remapNearRangeEndZero = 0f, float remapNearRangeEndOne = 0f,
			bool animateFarRangeStart = false, AnimationCurve shakeFarRangeStart = null,float remapFarRangeStartZero = 0f, float remapFarRangeStartOne = 0f,
			bool animateFarRangeEnd = false, AnimationCurve shakeFarRangeEnd = null,float remapFarRangeEndZero = 0f, float remapFarRangeEndOne = 0f);
		
		static public void Trigger(float duration, 
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false,
			bool animateFocusDistance = false, AnimationCurve shakeFocusDistance = null, float remapFocusDistanceZero = 0f, float remapFocusDistanceOne = 1f,
			bool animateNearRangeStart = false, AnimationCurve shakeNearRangeStart = null,float remapNearRangeStartZero = 0f, float remapNearRangeStartOne = 0f,
			bool animateNearRangeEnd = false, AnimationCurve shakeNearRangeEnd = null,float remapNearRangeEndZero = 0f, float remapNearRangeEndOne = 0f,
			bool animateFarRangeStart = false, AnimationCurve shakeFarRangeStart = null,float remapFarRangeStartZero = 0f, float remapFarRangeStartOne = 0f,
			bool animateFarRangeEnd = false, AnimationCurve shakeFarRangeEnd = null,float remapFarRangeEndZero = 0f, float remapFarRangeEndOne = 0f)
		{
			OnEvent?.Invoke(duration, attenuation, channelData, resetShakerValuesAfterShake, 
				resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore, animateFocusDistance, shakeFocusDistance, remapFocusDistanceZero, remapFocusDistanceOne,
				animateNearRangeStart, shakeNearRangeStart, remapNearRangeStartZero, remapNearRangeStartOne,
				animateNearRangeEnd, shakeNearRangeEnd, remapNearRangeEndZero, remapNearRangeEndOne,
				animateFarRangeStart, shakeFarRangeStart, remapFarRangeStartZero, remapFarRangeStartOne,
				animateFarRangeEnd, shakeFarRangeEnd,remapFarRangeEndZero,remapFarRangeEndOne);
		}
	}
}