using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 将此添加到一个精灵渲染器（SpriteRenderer）上，以便它能从反馈中接收MMSpriteRendererShakeEvents（特定的精灵渲染器抖动事件），或者在本地使其产生抖动效果。 
    /// </summary>
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Renderer/MM Sprite Renderer Shaker")]
	[RequireComponent(typeof(SpriteRenderer))]
	public class MMSpriteRendererShaker : MMShaker
	{
		[MMInspectorGroup("SpriteRenderer", true, 39)]
		/// the SpriteRenderer to affect when playing the feedback
		[Tooltip("在播放反馈时要影响的精灵渲染器。 ")]
		public SpriteRenderer BoundSpriteRenderer;
		/// whether or not that SpriteRenderer should be turned off on start
		[Tooltip("该精灵渲染器在开始时是否应该被关闭。 ")]
		public bool StartsOff = true;

		[MMInspectorGroup("Color", true, 40)]
		/// whether or not this shaker should modify color 
		[Tooltip("这个抖动器是否应该修改颜色。 ")]
		public bool ModifyColor = true;
		/// the colors to apply to the SpriteRenderer over time
		[Tooltip("随着时间推移应用到精灵渲染器（SpriteRenderer）上的颜色。 ")]
		public Gradient ColorOverTime;

		[MMInspectorGroup("Flip", true, 41)]
		/// whether or not to flip the sprite on X
		[Tooltip("是否在X轴上翻转精灵。")]
		public bool FlipX = false;
		/// whether or not to flip the sprite on Y
		[Tooltip("是否在Y轴上翻转精灵。")]
		public bool FlipY = false;

		protected Color _initialColor;
		protected bool _originalModifyColor;
		protected float _originalShakeDuration;
		protected Gradient _originalColorOverTime;
		protected bool _originalFlipX;
		protected bool _originalFlipY;

        /// <summary>
        /// 在初始化时，我们初始化我们的值
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			if (BoundSpriteRenderer == null)
			{
				BoundSpriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
			}
		}

        /// <summary>
        /// 当添加那个抖动器时，我们会初始化它的抖动持续时间。 
        /// </summary>
        protected virtual void Reset()
		{
			ShakeDuration = 1f;
		}

        /// <summary>
        /// 随着时间推移使数值产生晃动变化。
        /// </summary>
        protected override void Shake()
		{
			if (ModifyColor)
			{
				_remappedTimeSinceStart = MMFeedbacksHelpers.Remap(Time.time - _shakeStartedTimestamp, 0f, ShakeDuration, 0f, 1f);
				BoundSpriteRenderer.color = ColorOverTime.Evaluate(_remappedTimeSinceStart);
			}            
		}

        /// <summary>
        /// 收集目标对象上的初始值。
        /// </summary>
        protected override void GrabInitialValues()
		{
			_initialColor = BoundSpriteRenderer.color;
			_originalFlipX = BoundSpriteRenderer.flipX;
			_originalFlipY = BoundSpriteRenderer.flipY;
		}

        /// <summary>
        /// 重置目标的数值。
        /// </summary>
        protected override void ResetTargetValues()
		{
			base.ResetTargetValues();
			BoundSpriteRenderer.color = _initialColor;
			BoundSpriteRenderer.flipX = _originalFlipX;
			BoundSpriteRenderer.flipY = _originalFlipY;
		}

        /// <summary>
        /// 重置抖动器的数值。
        /// </summary>
        protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ModifyColor = _originalModifyColor;
			ShakeDuration = _originalShakeDuration;
			ColorOverTime = _originalColorOverTime;
		}

        /// <summary>
        /// 开始监听事件。
        /// </summary>
        public override void StartListening()
		{
			base.StartListening();
			MMSpriteRendererShakeEvent.Register(OnMMSpriteRendererShakeEvent);
		}

        /// <summary>
        /// 停止监听事件
        /// </summary>
        public override void StopListening()
		{
			base.StopListening();
			MMSpriteRendererShakeEvent.Unregister(OnMMSpriteRendererShakeEvent);
		}

		public virtual void OnMMSpriteRendererShakeEvent(float shakeDuration, bool modifyColor, Gradient colorOverTime,
			bool flipX, bool flipY,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true,
			bool useRange = false, float eventRange = 0f, Vector3 eventOriginPosition = default(Vector3))
		{
			if (!CheckEventAllowed(channelData, useRange, eventRange, eventOriginPosition) ||  (!Interruptible && Shaking))
			{
				return;
			}

			_resetShakerValuesAfterShake = resetShakerValuesAfterShake;
			_resetTargetValuesAfterShake = resetTargetValuesAfterShake;

			if (resetShakerValuesAfterShake)
			{
				_originalModifyColor = ModifyColor;
				_originalShakeDuration = ShakeDuration;
				_originalColorOverTime = ColorOverTime;
			}

			if (!OnlyUseShakerValues)
			{
				ModifyColor = modifyColor;
				ShakeDuration = shakeDuration;
				ColorOverTime = colorOverTime;
				FlipX = flipX;
				FlipY = flipY;
			}

			if (FlipX)
			{
				BoundSpriteRenderer.flipX = !BoundSpriteRenderer.flipX;
			}
			if (FlipY)
			{
				BoundSpriteRenderer.flipY = !BoundSpriteRenderer.flipY;
			}            

			Play();
		}
	}

    /// <summary>
    /// 一个事件（通常来自MMFeeedbackSpriteRenderer），用于使精灵渲染器（SpriteRenderer）的各项值产生抖动效果。 
    /// </summary>
    public struct MMSpriteRendererShakeEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(float shakeDuration, bool modifyColor, Gradient colorOverTime,
			bool flipX, bool flipY,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true,
			bool useRange = false, float eventRange = 0f, Vector3 eventOriginPosition = default(Vector3));

		static public void Trigger(float shakeDuration, bool modifyColor, Gradient colorOverTime,
			bool flipX, bool flipY,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true,
			bool useRange = false, float eventRange = 0f, Vector3 eventOriginPosition = default(Vector3))
		{
			OnEvent?.Invoke(shakeDuration, modifyColor, colorOverTime,
				flipX, flipY,
				feedbacksIntensity, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, 
				useRange, eventRange, eventOriginPosition);
		}
	}
}