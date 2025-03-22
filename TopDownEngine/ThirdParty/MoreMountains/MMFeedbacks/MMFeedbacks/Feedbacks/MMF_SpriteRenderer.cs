using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you change the color of a target sprite renderer over time, and flip it on X or Y. You can also use it to command one or many MMSpriteRendererShakers.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈功能可让你随时间改变目标精灵渲染器的颜色，并将其沿 X 轴或 Y 轴翻转。你还可以使用它来控制一个或多个 MMSpriteRendererShakers（自定义的精灵渲染器抖动组件）。")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Renderer/SpriteRenderer")]
	public class MMF_SpriteRenderer : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用此类所有反馈。
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色。
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
		public override bool EvaluateRequiresSetup() => (BoundSpriteRenderer == null);
		public override string RequiredTargetText => BoundSpriteRenderer != null ? BoundSpriteRenderer.name : "";
		public override string RequiresSetupText => "此反馈功能要求设置一个绑定精灵渲染器（BoundSpriteRenderer）才能正常工作。你可以在下面进行设置。";
#endif

        /// 此反馈的持续时间即精灵渲染器的持续时间；若为即时效果，则持续时间为 0。
        public override float FeedbackDuration { get { return (Mode == Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { Duration = value; } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => BoundSpriteRenderer = FindAutomatedTarget<SpriteRenderer>();

        /// 此反馈的可能模式。
        public enum Modes { OverTime, Instant, ShakerEvent, ToDestinationColor, ToDestinationColorAndBack }
        /// 获取初始颜色的不同方法。
        public enum InitialColorModes { InitialColorOnInit, InitialColorOnPlay }

		[MMFInspectorGroup("Sprite Renderer", true, 51, true)]
		/// the SpriteRenderer to affect when playing the feedback
		[Tooltip("播放反馈时要影响的精灵渲染器。")]
		public SpriteRenderer BoundSpriteRenderer;
		/// whether the feedback should affect the sprite renderer instantly or over a period of time
		[Tooltip("该反馈是应该立即对精灵渲染器产生影响，还是在一段时间内逐渐产生影响。")]
		public Modes Mode = Modes.OverTime;
		/// how long the sprite renderer should change over time
		[Tooltip("精灵渲染器应在多长时间内完成颜色或其他属性的变化。")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent, (int)Modes.ToDestinationColor, (int)Modes.ToDestinationColorAndBack)]
		public float Duration = 0.2f;
		/// whether or not that sprite renderer should be turned off on start
		[Tooltip("该精灵渲染器在开始时是否应该被关闭。")]
		public bool StartsOff = false;
		/// whether or not to reset shaker values after shake
		[Tooltip("摇晃效果结束后是否重置摇晃器的值。")]
		[MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("摇晃效果结束后是否重置目标对象的值。")]
		[MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
		public bool ResetTargetValuesAfterShake = true;
		/// whether or not to broadcast a range to only affect certain shakers
		[Tooltip("是否仅广播一个范围以只影响特定的摇晃器。")]
		[MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
		public bool OnlyBroadcastInRange = false;
		/// the range of the event, in units
		[Tooltip("该事件的作用范围，以单位计量。")]
		[MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
		public float EventRange = 100f;
		/// the transform to use to broadcast the event as origin point
		[Tooltip("用于将该事件作为原点进行广播的变换组件。")]
		[MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
		public Transform EventOriginTransform;
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("如果此选项为真，调用该反馈时，即使反馈正在进行中也会触发它；如果为假，则在当前反馈完成之前，将阻止任何新的播放操作")] 
		public bool AllowAdditivePlays = false;
		/// whether to grab the initial color to (potentially) go back to at init or when the feedback plays
		[Tooltip("是否获取初始颜色，以便在初始化时或反馈播放时（有可能）恢复到该颜色")] 
		public InitialColorModes InitialColorMode = InitialColorModes.InitialColorOnPlay;
        
		[MMFInspectorGroup("Color", true, 52)]
		/// whether or not to modify the color of the sprite renderer
		[Tooltip("是否修改精灵渲染器的颜色")]
		public bool ModifyColor = true;
		/// the colors to apply to the sprite renderer over time
		[Tooltip("随着时间推移要应用到精灵渲染器上的颜色")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
		public Gradient ColorOverTime;
		/// the color to move to in instant mode
		[Tooltip("在即时模式下要转变到的颜色。")]
		[MMFEnumCondition("Mode", (int)Modes.Instant, (int)Modes.ShakerEvent)]
		public Color InstantColor;
		/// the color to move to in ToDestination mode
		[Tooltip("在即时模式下要转变到的颜色。")]
		[MMFEnumCondition("Mode", (int)Modes.Instant, (int)Modes.ToDestinationColor, (int)Modes.ToDestinationColorAndBack)]
		public Color ToDestinationColor = Color.red;
		/// the color to move to in ToDestination mode
		[Tooltip("在即时模式下要转变到的颜色。")]
		[MMFEnumCondition("Mode", (int)Modes.Instant, (int)Modes.ToDestinationColor, (int)Modes.ToDestinationColorAndBack)]
		public AnimationCurve ToDestinationColorCurve = new AnimationCurve(new Keyframe(0, 0f), new Keyframe(1, 1f));
        
		[MMFInspectorGroup("Flip", true, 53)]
		/// whether or not to flip the sprite on X
		[Tooltip("是否在 X 轴上翻转精灵")]
		public bool FlipX = false;
		/// whether or not to flip the sprite on Y
		[Tooltip("是否在 Y 轴上翻转精灵")]
		public bool FlipY = false;

		protected Coroutine _coroutine;
		protected Color _initialColor;
		protected bool _initialFlipX;
		protected bool _initialFlipY;

        /// <summary>
        /// 在初始化时，如果有需要，我们会关闭精灵渲染器
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);

			if (EventOriginTransform == null)
			{
				EventOriginTransform = Owner.transform;
			}

			if (Active)
			{
				if (StartsOff)
				{
					Turn(false);
				}
			}

			if ((BoundSpriteRenderer != null) && (InitialColorMode == InitialColorModes.InitialColorOnInit))
			{
				_initialColor = BoundSpriteRenderer.color;
				_initialFlipX = BoundSpriteRenderer.flipX;
				_initialFlipY = BoundSpriteRenderer.flipY;
			}
		}

        /// <summary>
        /// 在播放（相关反馈效果）时，如果有需要，我们会开启精灵渲染器并启动一个随时间推进的协程
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			if ((BoundSpriteRenderer != null) && (InitialColorMode == InitialColorModes.InitialColorOnPlay))
			{
				_initialColor = BoundSpriteRenderer.color;
				_initialFlipX = BoundSpriteRenderer.flipX;
				_initialFlipY = BoundSpriteRenderer.flipY;
			}
            
			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
			Turn(true);
			switch (Mode)
			{
				case Modes.Instant:
					if (ModifyColor)
					{
						BoundSpriteRenderer.color = InstantColor;
					}
					Flip();
					break;
				case Modes.OverTime:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }
					_coroutine = Owner.StartCoroutine(SpriteRendererSequence());
					break;
				case Modes.ShakerEvent:
					MMSpriteRendererShakeEvent.Trigger(FeedbackDuration, ModifyColor, ColorOverTime, 
						FlipX, FlipY,   
						intensityMultiplier,
						ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake,
						OnlyBroadcastInRange, EventRange, EventOriginTransform.position);
					break;
				case Modes.ToDestinationColor:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }
					_coroutine = Owner.StartCoroutine(SpriteRendererToDestinationSequence(false));
					break;
				case Modes.ToDestinationColorAndBack:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }
					_coroutine = Owner.StartCoroutine(SpriteRendererToDestinationSequence(true));
					break;
			}
		}

        /// <summary>
        /// 这个协程将修改精灵渲染器（SpriteRenderer）上的值。
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator SpriteRendererSequence()
		{
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;
			IsPlaying = true;
			Flip();
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

				SetSpriteRendererValues(remappedTime);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			SetSpriteRendererValues(FinalNormalizedTime);
			if (StartsOff)
			{
				Turn(false);
			}            
			_coroutine = null;
			IsPlaying = false;
			yield return null;
		}

        /// <summary>
        /// 这个协程将会修改精灵渲染器上的各项数值。
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator SpriteRendererToDestinationSequence(bool andBack)
		{
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;
			IsPlaying = true;
			Flip();
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

				if (andBack)
				{
					remappedTime = (remappedTime < 0.5f)
						? MMFeedbacksHelpers.Remap(remappedTime, 0f, 0.5f, 0f, 1f)
						: MMFeedbacksHelpers.Remap(remappedTime, 0.5f, 1f, 1f, 0f);
				}
                
				float evalTime = ToDestinationColorCurve.Evaluate(remappedTime);
                
				if (ModifyColor)
				{
					BoundSpriteRenderer.color = Color.LerpUnclamped(_initialColor, ToDestinationColor, evalTime);
				}

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			if (ModifyColor)
			{
				BoundSpriteRenderer.color = andBack ? _initialColor : ToDestinationColor;
			}
			if (StartsOff)
			{
				Turn(false);
			}            
			_coroutine = null;
			IsPlaying = false;
			yield return null;
		}

        /// <summary>
        /// 根据 “水平翻转（FlipX）/ 垂直翻转（FlipY）” 设置对精灵进行水平或垂直翻转。
        /// </summary>
        protected virtual void Flip()
		{
			if (FlipX)
			{
				BoundSpriteRenderer.flipX = !BoundSpriteRenderer.flipX;
			}
			if (FlipY)
			{
				BoundSpriteRenderer.flipY = !BoundSpriteRenderer.flipY;
			}
		}

        /// <summary>
        /// 在指定的时间点（取值范围为 0 到 1）设置精灵渲染器上的各种值。
        /// </summary>
        /// <param name="time"></param>
        protected virtual void SetSpriteRendererValues(float time)
		{
			if (ModifyColor)
			{
				BoundSpriteRenderer.color = ColorOverTime.Evaluate(time);
			}
		}

        /// <summary>
        /// 如有需要，在停止时终止过渡效果
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized || (_coroutine == null))
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);
            
			Owner.StopCoroutine(_coroutine);
			IsPlaying = false;
			_coroutine = null;
		}

        /// <summary>
        /// 开启或关闭精灵渲染器。
        /// </summary>
        /// <param name="status"></param>
        protected virtual void Turn(bool status)
		{
			BoundSpriteRenderer.gameObject.SetActive(status);
			BoundSpriteRenderer.enabled = status;
		}

        /// <summary>
        /// 在恢复时，我们会恢复到初始状态
        /// </summary>
        protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			
			if (BoundSpriteRenderer != null)
			{
				BoundSpriteRenderer.color = _initialColor;
				BoundSpriteRenderer.flipX = _initialFlipX;
				BoundSpriteRenderer.flipY = _initialFlipY;
			}
		}
        
		/// <summary>
		/// 禁用
		/// </summary>
		public override void OnDisable()
		{
			_coroutine = null;
		}
	}
}