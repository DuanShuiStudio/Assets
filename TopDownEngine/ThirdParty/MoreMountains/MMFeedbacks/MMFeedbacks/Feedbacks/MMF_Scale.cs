using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will animate the scale of the target object over time when played
	/// </summary>
	[AddComponentMenu("")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Transform/Scale")]
	[FeedbackHelp("此反馈将在 3 条指定的动画曲线基础上，于指定的持续时间（以秒为单位）内对目标对象的缩放进行动画处理。你可以应用一个乘数，该乘数会与每条动画曲线的值相乘。")]
	public class MMF_Scale : MMF_Feedback
	{
        /// 一个静态布尔变量，用于一次性禁用所有此类反馈。
        public static bool FeedbackTypeAuthorized = true;
        /// 此反馈可运行的可能模式。
        public enum Modes { Absolute, Additive, ToDestination }
        /// 缩放动画可能的时间尺度。
        public enum TimeScales { Scaled, Unscaled }
        /// 为这个反馈设置检视器颜色。
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
		public override bool EvaluateRequiresSetup() { return (AnimateScaleTarget == null); }
		public override string RequiredTargetText { get { return AnimateScaleTarget != null ? AnimateScaleTarget.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that an AnimateScaleTarget be set to be able to work properly. You can set one below."; } }
		public override bool HasCustomInspectors { get { return true; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		public override bool CanForceInitialValue => true;
		protected override void AutomateTargetAcquisition() => AnimateScaleTarget = FindAutomatedTarget<Transform>();

		[MMFInspectorGroup("Scale Mode", true, 12, true)]
		/// the mode this feedback should operate on
		/// Absolute : follows the curve
		/// Additive : adds to the current scale of the target
		/// ToDestination : sets the scale to the destination target, whatever the current scale is
		[Tooltip("该反馈应采用的运行模式" +
                 "Absolute绝对模式：遵循曲线变化" +
                 "Additive累加模式：将（动画曲线计算得到的值）累加到目标对象的当前缩放值上。" +
                 "ToDestination到目标值模式：无论目标当前的缩放比例是多少，都将其缩放比例设置为目标值。")]
		public Modes Mode = Modes.Absolute;
		/// the object to animate
		[Tooltip("要进行动画处理的对象。")]
		public Transform AnimateScaleTarget;
        
		[MMFInspectorGroup("Scale Animation", true, 13)]
		/// the duration of the animation
		[Tooltip("动画的持续时间")]
		public float AnimateScaleDuration = 0.2f;
		/// the value to remap the curve's 0 value to
		[Tooltip("将曲线的 0 值重新映射到的值")]
		public float RemapCurveZero = 1f;
		/// the value to remap the curve's 1 value to
		[Tooltip("将曲线的 1 值重新映射到的值")]
		[FormerlySerializedAs("Multiplier")]
		public float RemapCurveOne = 2f;
		/// how much should be added to the curve
		[Tooltip("应该给曲线的值增加多少")]
		public float Offset = 0f;
		/// if this is true, should animate the X scale value
		[Tooltip("如果此值为真，则应对 X 轴缩放值进行动画处理")]
		public bool AnimateX = true;
		/// the x scale animation definition
		[Tooltip("X 轴缩放动画的定义")]
		[MMFCondition("AnimateX", true)]
		public MMTweenType AnimateScaleTweenX = new MMTweenType( new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1.5f), new Keyframe(1, 0)));
		/// if this is true, should animate the Y scale value
		[Tooltip("如果此值为真，则应对 Y 轴缩放值进行动画处理")]
		public bool AnimateY = true;
		/// the y scale animation definition
		[Tooltip("Y 轴缩放动画的定义")]
		[MMFCondition("AnimateY", true)]
		public MMTweenType AnimateScaleTweenY = new MMTweenType( new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1.5f), new Keyframe(1, 0)));
		/// if this is true, should animate the z scale value
		[Tooltip("如果此值为真，则应对 Z 轴缩放值进行动画处理")]
		public bool AnimateZ = true;
		/// the z scale animation definition
		[Tooltip("Z 轴缩放动画的定义")]
		[MMFCondition("AnimateZ", true)]
		public MMTweenType AnimateScaleTweenZ = new MMTweenType( new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1.5f), new Keyframe(1, 0)));
		/// if this is true, the AnimateX curve only will be used, and applied to all axis
		[Tooltip("如果此条件为真，则仅使用 X 轴动画曲线，并将其应用于所有坐标轴")] 
		public bool UniformScaling = false;
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("如果此值为真，调用该反馈时即便它正在进行中也会触发。如果为假，则在当前反馈完成之前会阻止任何新的播放操作。")] 
		public bool AllowAdditivePlays = false;
		/// if this is true, initial and destination scales will be recomputed on every play
		[Tooltip("如果此值为真，每次播放时都会重新计算初始缩放比例和目标缩放比例。")]
		public bool DetermineScaleOnPlay = false;
		/// the scale to reach when in ToDestination mode
		[Tooltip("在 “到目标值” 模式下要达到的缩放比例。")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public Vector3 DestinationScale = new Vector3(0.5f, 0.5f, 0.5f);

        /// 此反馈的持续时间即为缩放动画的持续时间。
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(AnimateScaleDuration); } set { AnimateScaleDuration = value; } }
		public override bool HasRandomness => true;

        /// [DEPRECATED] X 轴缩放动画的定义
        [HideInInspector] public AnimationCurve AnimateScaleX = null;
        /// [DEPRECATED] Y 轴缩放动画的定义
        [HideInInspector] public AnimationCurve AnimateScaleY = null;
        /// [DEPRECATED] Z 轴缩放动画的定义
        [HideInInspector] public AnimationCurve AnimateScaleZ = null;
		
		protected Vector3 _initialScale;
		protected Vector3 _newScale;
		protected Coroutine _coroutine;

        /// <summary>
        /// 在初始化时，我们会存储初始缩放比例。
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if (Active && (AnimateScaleTarget != null))
			{
				GetInitialScale();
			}
		}

        /// <summary>
        /// 存储初始缩放比例以供后续使用。
        /// </summary>
        protected virtual void GetInitialScale()
		{
			_initialScale = AnimateScaleTarget.localScale;
		}

        /// <summary>
        /// 播放时，触发缩放动画
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (AnimateScaleTarget == null))
			{
				return;
			}
            
			if (DetermineScaleOnPlay && NormalPlayDirection)
			{
				GetInitialScale();
			}
            
			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
			if (Active || Owner.AutoPlayOnEnable)
			{
				if ((Mode == Modes.Absolute) || (Mode == Modes.Additive))
				{
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }
					_coroutine = Owner.StartCoroutine(AnimateScale(AnimateScaleTarget, Vector3.zero, FeedbackDuration, AnimateScaleTweenX, AnimateScaleTweenY, AnimateScaleTweenZ, RemapCurveZero * intensityMultiplier, RemapCurveOne * intensityMultiplier));
				}
				if (Mode == Modes.ToDestination)
				{
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }
					_coroutine = Owner.StartCoroutine(ScaleToDestination());
				}                   
			}
		}

        /// <summary>
        /// 一个用于将目标对象缩放到其目标缩放比例的内部协程。
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator ScaleToDestination()
		{
			if (AnimateScaleTarget == null)
			{
				yield break;
			}

			if ((AnimateScaleTweenX == null) || (AnimateScaleTweenY == null) || (AnimateScaleTweenZ == null))
			{
				yield break;
			}

			if (FeedbackDuration == 0f)
			{
				yield break;
			}

			float journey = NormalPlayDirection ? 0f : FeedbackDuration;

			_initialScale = AnimateScaleTarget.localScale;
			_newScale = _initialScale;
			IsPlaying = true;
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float percent = Mathf.Clamp01(journey / FeedbackDuration);

				if (AnimateX)
				{
					_newScale.x = Mathf.LerpUnclamped(_initialScale.x, DestinationScale.x, AnimateScaleTweenX.Evaluate(percent) + Offset);
					_newScale.x = MMFeedbacksHelpers.Remap(_newScale.x, 0f, 1f, RemapCurveZero, RemapCurveOne);
				}

				if (AnimateY)
				{
					_newScale.y = Mathf.LerpUnclamped(_initialScale.y, DestinationScale.y, AnimateScaleTweenY.Evaluate(percent) + Offset);
					_newScale.y = MMFeedbacksHelpers.Remap(_newScale.y, 0f, 1f, RemapCurveZero, RemapCurveOne);    
				}

				if (AnimateZ)
				{
					_newScale.z = Mathf.LerpUnclamped(_initialScale.z, DestinationScale.z, AnimateScaleTweenZ.Evaluate(percent) + Offset);
					_newScale.z = MMFeedbacksHelpers.Remap(_newScale.z, 0f, 1f, RemapCurveZero, RemapCurveOne);    
				}

				if (UniformScaling)
				{
					_newScale.y = _newScale.x;
					_newScale.z = _newScale.x;
				}
                
				AnimateScaleTarget.localScale = _newScale;

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
                
				yield return null;
			}

			AnimateScaleTarget.localScale = NormalPlayDirection ? DestinationScale : _initialScale;
			_coroutine = null;
			IsPlaying = false;
			yield return null;
		}

        /// <summary>
        /// 一个用于随时间推移对缩放进行动画处理的内部协程
        /// </summary>
        /// <param name="targetTransform"></param>
        /// <param name="vector"></param>
        /// <param name="duration"></param>
        /// <param name="curveX"></param>
        /// <param name="curveY"></param>
        /// <param name="curveZ"></param>
        /// <param name="multiplier"></param>
        /// <returns></returns>
        protected virtual IEnumerator AnimateScale(Transform targetTransform, Vector3 vector, float duration, MMTweenType curveX, MMTweenType curveY, MMTweenType curveZ, float remapCurveZero = 0f, float remapCurveOne = 1f)
		{
			if (targetTransform == null)
			{
				yield break;
			}

			if ((curveX == null) || (curveY == null) || (curveZ == null))
			{
				yield break;
			}

			if (duration == 0f)
			{
				yield break;
			}
            
			float journey = NormalPlayDirection ? 0f : duration;
            
			_initialScale = targetTransform.localScale;
            
			IsPlaying = true;
			
			while ((journey >= 0) && (journey <= duration) && (duration > 0))
			{
				vector = Vector3.zero;
				float percent = Mathf.Clamp01(journey / duration);

				if (AnimateX)
				{
					vector.x = AnimateX ? curveX.Evaluate(percent) + Offset : targetTransform.localScale.x;
					vector.x = MMFeedbacksHelpers.Remap(vector.x, 0f, 1f, remapCurveZero, remapCurveOne);
					if (Mode == Modes.Additive)
					{
						vector.x += _initialScale.x;
					}
				}
				else
				{
					vector.x = targetTransform.localScale.x;
				}

				if (AnimateY)
				{
					vector.y = AnimateY ? curveY.Evaluate(percent) + Offset : targetTransform.localScale.y;
					vector.y = MMFeedbacksHelpers.Remap(vector.y, 0f, 1f, remapCurveZero, remapCurveOne);    
					if (Mode == Modes.Additive)
					{
						vector.y += _initialScale.y;
					}
				}
				else 
				{
					vector.y = targetTransform.localScale.y;
				}

				if (AnimateZ)
				{
					vector.z = AnimateZ ? curveZ.Evaluate(percent) + Offset : targetTransform.localScale.z;
					vector.z = MMFeedbacksHelpers.Remap(vector.z, 0f, 1f, remapCurveZero, remapCurveOne);    
					if (Mode == Modes.Additive)
					{
						vector.z += _initialScale.z;
					}
				}
				else 
				{
					vector.z = targetTransform.localScale.z;
				}

				if (UniformScaling)
				{
					vector.y = vector.x;
					vector.z = vector.x;
				}
                
				targetTransform.localScale = vector;

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
                
				yield return null;
			}
            
			vector = Vector3.zero;

			if (AnimateX)
			{
				vector.x = AnimateX ? curveX.Evaluate(FinalNormalizedTime) + Offset : targetTransform.localScale.x;
				vector.x = MMFeedbacksHelpers.Remap(vector.x, 0f, 1f, remapCurveZero, remapCurveOne);
				if (Mode == Modes.Additive)
				{
					vector.x += _initialScale.x;
				}
			}
			else 
			{
				vector.x = targetTransform.localScale.x;
			}

			if (AnimateY)
			{
				vector.y = AnimateY ? curveY.Evaluate(FinalNormalizedTime) + Offset : targetTransform.localScale.y;
				vector.y = MMFeedbacksHelpers.Remap(vector.y, 0f, 1f, remapCurveZero, remapCurveOne);
				if (Mode == Modes.Additive)
				{
					vector.y += _initialScale.y;
				}
			}
			else 
			{
				vector.y = targetTransform.localScale.y;
			}

			if (AnimateZ)
			{
				vector.z = AnimateZ ? curveZ.Evaluate(FinalNormalizedTime) + Offset : targetTransform.localScale.z;
				vector.z = MMFeedbacksHelpers.Remap(vector.z, 0f, 1f, remapCurveZero, remapCurveOne);    
				if (Mode == Modes.Additive)
				{
					vector.z += _initialScale.z;
				}
			}
			else 
			{
				vector.z = targetTransform.localScale.z;
			}

			if (UniformScaling)
			{
				vector.y = vector.x;
				vector.z = vector.x;
			}
            
			targetTransform.localScale = vector;
			IsPlaying = false;
			_coroutine = null;
			yield return null;
		}

        /// <summary>
        /// 停止时，如果移动操作正在进行，我们将中断它
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (_coroutine == null))
			{
				return;
			}
			IsPlaying = false;
			Owner.StopCoroutine(_coroutine);
			_coroutine = null;
            
		}

        /// <summary>
        /// 禁用时，我们会重置协程。
        /// </summary>
        public override void OnDisable()
		{
			_coroutine = null;
		}

        /// <summary>
        /// 在验证时，如有必要，我们会将已弃用的动画曲线迁移为补间类型。
        /// </summary>
        public override void OnValidate()
		{
			base.OnValidate();
			MMFeedbacksHelpers.MigrateCurve(AnimateScaleX, AnimateScaleTweenX, Owner);
			MMFeedbacksHelpers.MigrateCurve(AnimateScaleY, AnimateScaleTweenY, Owner);
			MMFeedbacksHelpers.MigrateCurve(AnimateScaleZ, AnimateScaleTweenZ, Owner);
		}

        /// <summary>
        /// 在恢复时，我们会恢复到初始状态。
        /// </summary>
        protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			AnimateScaleTarget.localScale = _initialScale;
		}
	}
}