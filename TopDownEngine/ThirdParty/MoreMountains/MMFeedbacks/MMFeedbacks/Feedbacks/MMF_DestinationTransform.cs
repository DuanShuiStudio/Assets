using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you animate the position/rotation/scale of a target transform to match the one of a destination transform.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈将让你为目标变换的位置/旋转/缩放制作动画，以匹配目标变换的位置/旋转/缩放")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Transform/Destination")]
	public class MMF_DestinationTransform : MMF_Feedback
	{
        /// 一个静态布尔值用于一次性禁用此类型的所有反馈
        public static bool FeedbackTypeAuthorized = true;
        /// 此反馈可以进行动画制作的时间缩放。
        public enum TimeScales { Scaled, Unscaled }
        /// 设置此反馈的检查器颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetTransform == null) || (Destination == null); }
		public override string RequiredTargetText { get { return TargetTransform != null ? TargetTransform.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetTransform and a Destination be set to be able to work properly. You can set one below."; } }
		public override bool HasCustomInspectors { get { return true; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetTransform = FindAutomatedTarget<Transform>();

		[MMFInspectorGroup("Target to animate", true, 61, true)]
		/// the target transform we want to animate properties on
		[Tooltip("我们想要动画化属性的目标变换组件")]
		public Transform TargetTransform;
        
		/// whether or not we want to force an origin transform. If not, the current position of the target transform will be used as origin instead
		[Tooltip("我们是否希望强制使用原点变换组件。如果不强制，则将使用目标变换组件的当前位置作为原点")]
		public bool ForceOrigin = false;
		/// the transform to use as origin in ForceOrigin mode
		[Tooltip("在ForceOrigin模式下用作原点的变换组件")]
		[MMFCondition("ForceOrigin", true)] 
		public Transform Origin;
		/// the destination transform whose properties we want to match 
		[Tooltip("我们想要匹配其属性的目标变换组件")]
		public Transform Destination;
        
		[MMFInspectorGroup("Transition", true, 63)]
		/// a global curve to animate all properties on, unless dedicated ones are specified
		[Tooltip("用于动画化所有属性的全局曲线，除非指定了专用曲线。")]
		public MMTweenType GlobalAnimationTween = new MMTweenType( new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
		/// the duration of the transition, in seconds
		[Tooltip("过渡的持续时间，以秒为单位")]
		public float Duration = 0.2f;

		[MMFInspectorGroup("Axis Locks", true, 64)]
        
		/// whether or not to animate the X position
		[Tooltip("是否对 X 轴位置制作动画")]
		public bool AnimatePositionX = true;
		/// whether or not to animate the Y position
		[Tooltip("是否对 Y 轴位置制作动画")]
		public bool AnimatePositionY = true;
		/// whether or not to animate the Z position
		[Tooltip("是否对 Z 轴位置制作动画")]
		public bool AnimatePositionZ = true;
		/// whether or not to animate the X rotation
		[Tooltip("是否对 X 轴旋转制作动画")]
		public bool AnimateRotationX = true;
		/// whether or not to animate the Y rotation
		[Tooltip("是否对 X 轴旋转制作动画")]
		public bool AnimateRotationY = true;
		/// whether or not to animate the Z rotation
		[Tooltip("是否对 Z 轴旋转制作动画")]
		public bool AnimateRotationZ = true;
		/// whether or not to animate the W rotation
		[Tooltip("是否对 W 轴旋转制作动画")]
		public bool AnimateRotationW = true;
		/// whether or not to animate the X scale
		[Tooltip("是否对 X 轴缩放制作动画")]
		public bool AnimateScaleX = true;
		/// whether or not to animate the Y scale
		[Tooltip("是否对 Y 轴缩放制作动画")]
		public bool AnimateScaleY = true;
		/// whether or not to animate the Z scale
		[Tooltip("是否对 Z 轴缩放制作动画")]
		public bool AnimateScaleZ = true;

		[MMFInspectorGroup("Separate Curves", true, 65)]
		/// whether or not to use a separate animation curve to animate the position
		[Tooltip("是否使用一个单独的动画曲线来对位置制作动画")]
		public bool SeparatePositionCurve = false;
		/// the curve to use to animate the position on
		[Tooltip("用于对位置制作动画的曲线")]
		[MMFCondition("SeparatePositionCurve", true)]
		public MMTweenType AnimatePositionTween = new MMTweenType( new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
        
		/// whether or not to use a separate animation curve to animate the rotation
		[Tooltip("是否使用一个单独的动画曲线来对旋转制作动画")]
		public bool SeparateRotationCurve = false;
		/// the curve to use to animate the rotation on
		[Tooltip("用于对旋转制作动画的曲线")]
		[MMFCondition("SeparateRotationCurve", true)]
		public MMTweenType AnimateRotationTween = new MMTweenType( new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
        
		/// whether or not to use a separate animation curve to animate the scale
		[Tooltip("是否使用一个单独的动画曲线来对缩放制作动画")]
		public bool SeparateScaleCurve = false;
		/// the curve to use to animate the scale on
		[Tooltip("用于对缩放制作动画的曲线")]
		[MMFCondition("SeparateScaleCurve", true)]
		public MMTweenType AnimateScaleTween = new MMTweenType( new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));

        /// 此反馈的持续时间是移动的持续时间
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }

        /// 一个用于对所有属性制作动画的全局曲线，除非指定了专用的曲线
        [HideInInspector] public AnimationCurve GlobalAnimationCurve = null;
        /// 用于对位置制作动画的曲线
        [HideInInspector] public AnimationCurve AnimateScaleCurve = null;
        /// 用于对位置制作旋转的曲线
        [HideInInspector] public AnimationCurve AnimatePositionCurve = null;
        /// 用于对位置制作缩放的曲线
        [HideInInspector] public AnimationCurve AnimateRotationCurve = null;
		
		protected Coroutine _coroutine;
		protected Vector3 _newPosition;
		protected Quaternion _newRotation;
		protected Vector3 _newScale;
		protected Vector3 _pointAPosition;
		protected Vector3 _pointBPosition;
		protected Quaternion _pointARotation;
		protected Quaternion _pointBRotation;
		protected Vector3 _pointAScale;
		protected Vector3 _pointBScale;
		protected MMTweenType _animationTweenType;

		protected Vector3 _initialPosition;
		protected Vector3 _initialScale;
		protected Quaternion _initialRotation;

        /// <summary>
        /// 在播放时，我们对目标变换的位置/旋转/缩放朝向其目的地制作动画
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetTransform == null))
			{
				return;
			}
			if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }
			_coroutine = Owner.StartCoroutine(AnimateToDestination());
		}

        /// <summary>
        /// 用于对目标变换的位置/旋转/缩放朝向其目的地制作动画的一个协程
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator AnimateToDestination()
		{
			_initialPosition = TargetTransform.position;
			_initialRotation = TargetTransform.rotation;
			_initialScale = TargetTransform.localScale;
			
			_pointAPosition = ForceOrigin ? Origin.transform.position : TargetTransform.position;
			_pointBPosition = Destination.transform.position;

			if (!AnimatePositionX) { _pointAPosition.x = TargetTransform.position.x; _pointBPosition.x = _pointAPosition.x; }
			if (!AnimatePositionY) { _pointAPosition.y = TargetTransform.position.y; _pointBPosition.y = _pointAPosition.y; }
			if (!AnimatePositionZ) { _pointAPosition.z = TargetTransform.position.z; _pointBPosition.z = _pointAPosition.z; }
            
			_pointARotation = ForceOrigin ? Origin.transform.rotation : TargetTransform.rotation;
			_pointBRotation = Destination.transform.rotation;
            
			if (!AnimateRotationX) { _pointARotation.x = TargetTransform.rotation.x; _pointBRotation.x = _pointARotation.x; }
			if (!AnimateRotationY) { _pointARotation.y = TargetTransform.rotation.y; _pointBRotation.y = _pointARotation.y; }
			if (!AnimateRotationZ) { _pointARotation.z = TargetTransform.rotation.z; _pointBRotation.z = _pointARotation.z; }
			if (!AnimateRotationW) { _pointARotation.w = TargetTransform.rotation.w; _pointBRotation.w = _pointARotation.w; }

			_pointAScale = ForceOrigin ? Origin.transform.localScale : TargetTransform.localScale;
			_pointBScale = Destination.transform.localScale;
            
			if (!AnimateScaleX) { _pointAScale.x = TargetTransform.localScale.x; _pointBScale.x = _pointAScale.x; }
			if (!AnimateScaleY) { _pointAScale.y = TargetTransform.localScale.y; _pointBScale.y = _pointAScale.y; }
			if (!AnimateScaleZ) { _pointAScale.z = TargetTransform.localScale.z; _pointBScale.z = _pointAScale.z; }

			IsPlaying = true;
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float percent = Mathf.Clamp01(journey / FeedbackDuration);
				ChangeTransformValues(percent);
				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}

            // 设置最终位置
            ChangeTransformValues(1f);
			
			IsPlaying = false;
			_coroutine = null;
			yield break;
		}

        /// <summary>
        /// 计算变换的新位置、旋转和缩放，并将其应用于变换
        /// </summary>
        /// <param name="percent"></param>
        protected virtual void ChangeTransformValues(float percent)
		{
			_animationTweenType = SeparatePositionCurve ? AnimatePositionTween : GlobalAnimationTween;
			_newPosition = Vector3.LerpUnclamped(_pointAPosition, _pointBPosition, _animationTweenType.Evaluate(percent));
                
			_animationTweenType = SeparateRotationCurve ? AnimateRotationTween : GlobalAnimationTween;
			_newRotation = Quaternion.LerpUnclamped(_pointARotation, _pointBRotation, _animationTweenType.Evaluate(percent));
                
			_animationTweenType = SeparateScaleCurve ? AnimateScaleTween : GlobalAnimationTween;
			_newScale = Vector3.LerpUnclamped(_pointAScale, _pointBScale, _animationTweenType.Evaluate(percent));
			
			TargetTransform.position = _newPosition;
			TargetTransform.rotation = _newRotation;
			TargetTransform.localScale = _newScale;
		}

        /// <summary>
        /// 如果需要，在停止时我们停止协程
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			base.CustomStopFeedback(position, feedbacksIntensity);
			IsPlaying = false;
            
			if ((TargetTransform != null) && (_coroutine != null))
			{
				Owner.StopCoroutine(_coroutine);
			}
		}
		
		/// <summary>
		/// 在恢复时，我们恢复初始状态
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			TargetTransform.position = _initialPosition;
			TargetTransform.rotation = _initialRotation;
			TargetTransform.localScale = _initialScale;
		}

        /// <summary>
        /// 在验证时，如有需要，我们将弃用的动画曲线迁移到补间类型
        /// </summary>
        public override void OnValidate()
		{
			base.OnValidate();
			MMFeedbacksHelpers.MigrateCurve(GlobalAnimationCurve, GlobalAnimationTween, Owner);
			MMFeedbacksHelpers.MigrateCurve(AnimatePositionCurve, AnimatePositionTween, Owner);
			MMFeedbacksHelpers.MigrateCurve(AnimateRotationCurve, AnimateRotationTween, Owner);
			MMFeedbacksHelpers.MigrateCurve(AnimateScaleCurve, AnimateScaleTween, Owner);
		}
	}    
}