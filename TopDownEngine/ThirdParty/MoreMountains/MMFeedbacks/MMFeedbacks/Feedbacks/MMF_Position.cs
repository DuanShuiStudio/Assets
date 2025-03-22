using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// this feedback will let you animate the position of 
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈将随时间动画化目标对象的位置，在指定持续时间内，从选择的初始位置到选择的目标位置。这些可以是相对于反馈位置的Vector3偏移量，也可以是变换组件。如果指定了变换组件，则将忽略Vector3值。")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Transform/Position")]
	public class MMF_Position : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用所有这种类型的反馈
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色。
#if UNITY_EDITOR
        public override Color FeedbackColor => MMFeedbacksInspectorColors.TransformColor; 
		public override bool EvaluateRequiresSetup() => (AnimatePositionTarget == null);
		public override string RequiredTargetText => AnimatePositionTarget != null ? AnimatePositionTarget.name : "";  
		public override string RequiresSetupText => "This feedback requires that a AnimatePositionTarget and a Destination be set to be able to work properly. You can set one below."; 
		public override bool HasCustomInspectors => true; 
		#endif
		public override bool HasRandomness => true;
		public override bool CanForceInitialValue => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => AnimatePositionTarget = FindAutomatedTargetGameObject();
		
		public enum Spaces { World, Local, RectTransform, Self }
		public enum Modes { AtoB, AlongCurve, ToDestination }
		public enum TimeScales { Scaled, Unscaled }

		[MMFInspectorGroup("Position Target", true, 61, true)]
		/// the object this feedback will animate the position for
		[Tooltip("此反馈将动画化其位置的对象")]
		public GameObject AnimatePositionTarget;

		[MMFInspectorGroup("Transition", true, 63)]
		/// the mode this animation should follow (either going from A to B, or moving along a curve)
		[Tooltip("此动画应遵循的模式（无论是从A到B，还是沿曲线移动）")]
		public Modes Mode = Modes.AtoB;
		/// the space in which to move the position in
		[Tooltip("移动位置的空间")]
		public Spaces Space = Spaces.World;
		
		/// whether or not to randomize remap values between their base and alt values on play, useful to add some variety every time you play this feedback
		[Tooltip("在播放时是否在其基础值和替代值之间随机化映射值，有助于每次播放此反馈时增加一些变化")]
		[MMFEnumCondition("Mode", (int)Modes.AlongCurve)]
		public bool RandomizeRemap = false;
		/// the duration of the animation on play
		[Tooltip("播放时动画的持续时间")]
		public float AnimatePositionDuration = 0.2f;
		
		/// the MMTween curve definition to use instead of the animation curve to define the acceleration of the movement
		[Tooltip("要使用的动画曲线定义，以代替动画曲线来定义运动的加速度")]
		[MMFEnumCondition("Mode", (int)Modes.AtoB, (int)Modes.ToDestination)]
		public MMTweenType AnimatePositionTween = new MMTweenType( new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));
		
		/// the value to remap the curve's 0 value to
		[MMFEnumCondition("Mode", (int)Modes.AlongCurve)]
		[Tooltip("将曲线的0值重新映射到的值")]
		public float RemapCurveZero = 0f;
		/// in randomize remap mode, the value to remap the curve's 0 value to (randomized between this and RemapCurveZero)
		[MMFCondition("RandomizeRemap", true)]
		[MMFEnumCondition("Mode", (int)Modes.AlongCurve)]
		[Tooltip("在随机重新映射模式下，将曲线的0值重新映射到的值（在此值和RemapCurveZero之间随机化）。")]
		public float RemapCurveZeroAlt = 0f;
		/// the value to remap the curve's 1 value to
		[Tooltip("将曲线的1值重新映射到的值")]
		[MMFEnumCondition("Mode", (int)Modes.AlongCurve)]
		[FormerlySerializedAs("CurveMultiplier")]
		public float RemapCurveOne = 1f;
		/// in randomize remap mode, the value to remap the curve's 1 value to (randomized between this and RemapCurveOne)
		[Tooltip("在随机重新映射模式下，将曲线的1值重新映射到的值（在此值和RemapCurveZero之间随机化）。")]
		[MMFCondition("RandomizeRemap", true)]
		[MMFEnumCondition("Mode", (int)Modes.AlongCurve)]
		public float RemapCurveOneAlt = 1f;
		/// if this is true, the x position will be animated
		[Tooltip("如果为真，则x位置将被动画化")]
		[MMFEnumCondition("Mode", (int)Modes.AlongCurve)]
		public bool AnimateX;
		/// the acceleration of the movement
		[Tooltip("运动的加速度")]
		[MMFCondition("AnimateX", true)]
		public MMTweenType AnimatePositionTweenX = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(0.6f, -1f), new Keyframe(1, 0f)));
		/// if this is true, the y position will be animated
		[Tooltip("如果为真，则y位置将被动画化")]
		[MMFEnumCondition("Mode", (int)Modes.AlongCurve)]
		public bool AnimateY;
		/// the acceleration of the movement
		[Tooltip("运动的加速度")]
		[MMFCondition("AnimateY", true)]
		public MMTweenType AnimatePositionTweenY = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(0.6f, -1f), new Keyframe(1, 0f)));
		/// if this is true, the z position will be animated
		[Tooltip("如果为真，则z位置将被动画化")]
		[MMFEnumCondition("Mode", (int)Modes.AlongCurve)]
		public bool AnimateZ;
		/// the acceleration of the movement
		[Tooltip("运动的加速度")]
		[MMFCondition("AnimateZ", true)]
		public MMTweenType AnimatePositionTweenZ = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(0.6f, -1f), new Keyframe(1, 0f)));
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("如果为真，即使反馈正在进行中，调用该反馈也会触发它。如果为假，它将防止任何新的播放，直到当前的播放结束")] 
		public bool AllowAdditivePlays = false;
		[MMFInspectorGroup("Positions", true, 64)]
		/// if this is true, movement will be relative to the object's initial position. So moving its y position along a curve going from 0 to 1 will move it up one unit. If this is false, in that same example, it'll be moved from 0 to 1 in absolute coordinates.
		[Tooltip("如果为true，移动将相对于对象的初始位置。因此，沿从0到1的曲线移动其y位置将使其向上移动一个单位。如果为false，在相同的示例中，它将在绝对坐标中从0移动到1。")]
		public bool RelativePosition = true;
		/// if this is true, initial and destination positions will be recomputed on every play
		[Tooltip("如果为真，每次播放时将重新计算初始位置和目标位置")]
		public bool DeterminePositionsOnPlay = false;
		/// the initial position
		[Tooltip("初始位置")]
		[MMFEnumCondition("Mode", (int)Modes.AtoB, (int)Modes.AlongCurve)]
		public Vector3 InitialPosition = Vector3.zero;
		/// the destination position
		[Tooltip("目标位置")]
		[MMFEnumCondition("Mode", (int)Modes.AtoB, (int)Modes.ToDestination)]
		public Vector3 DestinationPosition = Vector3.one;
		/// the initial transform - if set, takes precedence over the Vector3 above
		[Tooltip("初始转换 - 如果设置，将优先于上面的Vector3")]
		[MMFEnumCondition("Mode", (int)Modes.AtoB, (int)Modes.AlongCurve)]
		public Transform InitialPositionTransform;
		/// the destination transform - if set, takes precedence over the Vector3 above
		[Tooltip("目标转换 - 如果设置，将优先于上面的Vector3")]
		[MMFEnumCondition("Mode", (int)Modes.AtoB, (int)Modes.ToDestination)]
		public Transform DestinationPositionTransform;
        /// 此反馈的持续时间是其动画的持续时间
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(AnimatePositionDuration); } set { AnimatePositionDuration = value; } }

        /// [DEPRECATED] 运动的加速度
        [HideInInspector] public AnimationCurve AnimatePositionCurveX = null;
        /// [DEPRECATED] 运动的加速度
        [HideInInspector] public AnimationCurve AnimatePositionCurveY = null;
        /// [DEPRECATED] 运动的加速度
        [HideInInspector] public AnimationCurve AnimatePositionCurveZ = null;
        /// [DEPRECATED] 运动的加速度 - 这不再使用，已被AnimatePositionTween替代
        [HideInInspector] public AnimationCurve AnimatePositionCurve = null;

		protected Vector3 _newPosition;
		protected Vector3 _currentPosition;
		protected RectTransform _rectTransform;
		protected Vector3 _initialPosition;
		protected Vector3 _destinationPosition;
		protected Coroutine _coroutine;
		protected Vector3 _workInitialPosition;
		protected Vector3 _workDestinationPosition;
		protected float _remapCurveZero;
		protected float _remapCurveOne;

        /// <summary>
        /// 在初始化时，我们设置初始和目标位置（转换将优先于Vector3）
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if (Active)
			{
				if (AnimatePositionTarget == null)
				{
					Debug.LogWarning(this + "动画位置目标为空, 您需要在检查器中定义它");
					return;
				}

				if (Space == Spaces.RectTransform)
				{
					_rectTransform = AnimatePositionTarget.GetComponent<RectTransform>();
				}

				if (!DeterminePositionsOnPlay)
				{
					DeterminePositions();    
				}
			}
		}

		protected virtual void DeterminePositions()
		{
			if (DeterminePositionsOnPlay && RelativePosition && (InitialPosition != Vector3.zero))
			{
				return;
			}
            
			if (InitialPositionTransform != null)
			{
				_workInitialPosition = GetPosition(InitialPositionTransform);
			}
			else
			{
				_workInitialPosition = RelativePosition ? GetPosition(AnimatePositionTarget.transform) + InitialPosition : InitialPosition;
				if (Space == Spaces.Self && !RelativePosition)
				{
					_workInitialPosition = AnimatePositionTarget.transform.position + InitialPosition;
				}
			}
			if (DestinationPositionTransform != null)
			{
				_workDestinationPosition = GetPosition(DestinationPositionTransform);
			}
			else
			{
				_workDestinationPosition = RelativePosition ? GetPosition(AnimatePositionTarget.transform) + DestinationPosition : DestinationPosition;
			}
		}

        /// <summary>
        /// 在播放时，我们将对象从A移动到B
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (AnimatePositionTarget == null))
			{
				return;
			}
            
			if (Active || Owner.AutoPlayOnEnable)
			{
				if (DeterminePositionsOnPlay && NormalPlayDirection)
				{
					DeterminePositions();
				}
                
				switch (Mode)
				{
					case Modes.ToDestination:
						_initialPosition = GetPosition(AnimatePositionTarget.transform);
						_destinationPosition = _workDestinationPosition;
						if (DestinationPositionTransform != null)
						{
							_destinationPosition = GetPosition(DestinationPositionTransform);
						}
						if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }
						_coroutine = Owner.StartCoroutine(MoveFromTo(AnimatePositionTarget, _initialPosition, _destinationPosition, FeedbackDuration, AnimatePositionTween));
						break;
					case Modes.AtoB:
						if (!AllowAdditivePlays && (_coroutine != null))
						{
							return;
						}
						if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }
						_coroutine = Owner.StartCoroutine(MoveFromTo(AnimatePositionTarget, _workInitialPosition, _workDestinationPosition, FeedbackDuration, AnimatePositionTween));
						break;
					case Modes.AlongCurve:
						if (!AllowAdditivePlays && (_coroutine != null))
						{
							return;
						}
						float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);

						_remapCurveZero = RandomizeRemap ? Random.Range(RemapCurveZero, RemapCurveZeroAlt) : RemapCurveZero;
						_remapCurveOne = RandomizeRemap ? Random.Range(RemapCurveOne, RemapCurveOneAlt) : RemapCurveOne;
						
						if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }
						_coroutine = Owner.StartCoroutine(MoveAlongCurve(AnimatePositionTarget, _workInitialPosition, FeedbackDuration, intensityMultiplier));
						break;
				}                    
			}
		}

        /// <summary>
        /// 沿曲线移动对象
        /// </summary>
        /// <param name="movingObject"></param>
        /// <param name="pointA"></param>
        /// <param name="pointB"></param>
        /// <param name="duration"></param>
        /// <param name="curve"></param>
        /// <returns></returns>
        protected virtual IEnumerator MoveAlongCurve(GameObject movingObject, Vector3 initialPosition, float duration, float intensityMultiplier)
		{
			IsPlaying = true;
			float journey = NormalPlayDirection ? 0f : duration;
			while ((journey >= 0) && (journey <= duration) && (duration > 0))
			{
				float percent = Mathf.Clamp01(journey / duration);

				ComputeNewCurvePosition(movingObject, initialPosition, percent, intensityMultiplier);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			ComputeNewCurvePosition(movingObject, initialPosition, FinalNormalizedTime, intensityMultiplier);
			_coroutine = null;
			IsPlaying = false;
			yield break;
		}

        /// <summary>
        /// 评估位置曲线并计算新位置
        /// </summary>
        /// <param name="movingObject"></param>
        /// <param name="initialPosition"></param>
        /// <param name="percent"></param>
        protected virtual void ComputeNewCurvePosition(GameObject movingObject, Vector3 initialPosition, float percent, float intensityMultiplier)
		{
			float newValueX = MMTween.Tween(percent, 0f, 1f, _remapCurveZero * intensityMultiplier, _remapCurveOne * intensityMultiplier, AnimatePositionTweenX);
			float newValueY = MMTween.Tween(percent, 0f, 1f, _remapCurveZero * intensityMultiplier, _remapCurveOne * intensityMultiplier, AnimatePositionTweenY);
			float newValueZ = MMTween.Tween(percent, 0f, 1f, _remapCurveZero * intensityMultiplier, _remapCurveOne * intensityMultiplier, AnimatePositionTweenZ);

			_newPosition = initialPosition;
			_currentPosition = GetPosition(movingObject.transform);

			if (RelativePosition)
			{
				_newPosition.x = AnimateX ? initialPosition.x + newValueX : _currentPosition.x;
				_newPosition.y = AnimateY ? initialPosition.y + newValueY : _currentPosition.y;
				_newPosition.z = AnimateZ ? initialPosition.z + newValueZ : _currentPosition.z;
			}
			else
			{
				_newPosition.x = AnimateX ? newValueX : _currentPosition.x;
				_newPosition.y = AnimateY ? newValueY : _currentPosition.y;
				_newPosition.z = AnimateZ ? newValueZ : _currentPosition.z;
			}

			if (Space == Spaces.Self)
			{
				_newPosition.x = AnimateX ? newValueX : 0f;
				_newPosition.y = AnimateY ? newValueY : 0f;
				_newPosition.z = AnimateZ ? newValueZ : 0f;
			}
			
			SetPosition(movingObject.transform, _newPosition);
		}

        /// <summary>
        /// 在给定时间内将对象从点A移动到点B。
        /// </summary>
        /// <param name="movingObject">Moving object.</param>
        /// <param name="pointA">Point a.</param>
        /// <param name="pointB">Point b.</param>
        /// <param name="duration">Time.</param>
        protected virtual IEnumerator MoveFromTo(GameObject movingObject, Vector3 pointA, Vector3 pointB, float duration, MMTweenType tweenType)
		{
			IsPlaying = true;
			float journey = NormalPlayDirection ? 0f : duration;
			while ((journey >= 0) && (journey <= duration) && (duration > 0))
			{
				float curveValue = MMTween.Tween(journey, 0f, duration, 0f, 1f, tweenType);
				
				_newPosition = Vector3.LerpUnclamped(pointA, pointB, curveValue);
				SetPosition(movingObject.transform, _newPosition);
				
				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}

            // 设置最终位置
            if (NormalPlayDirection)
			{
				SetPosition(movingObject.transform, pointB);    
			}
			else
			{
				SetPosition(movingObject.transform, pointA);
			}
			_coroutine = null;
			IsPlaying = false;
			yield break;
		}

        /// <summary>
        /// 获取世界、局部或锚定位置
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        protected virtual Vector3 GetPosition(Transform target)
		{
			switch (Space)
			{
				case Spaces.World:
					return target.position;
				case Spaces.Local:
					return target.localPosition;
				case Spaces.RectTransform:
					return target.gameObject.GetComponent<RectTransform>().anchoredPosition;
				case Spaces.Self:
					return target.position;
			}
			return Vector3.zero;
		}

        /// <summary>
        /// 设置目标的位置、局部位置或锚定位置
        /// </summary>
        /// <param name="target"></param>
        /// <param name="newPosition"></param>
        protected virtual void SetPosition(Transform target, Vector3 newPosition)
		{
			switch (Space)
			{
				case Spaces.World:
					target.position = newPosition;
					break;
				case Spaces.Local:
					target.localPosition = newPosition;
					break;
				case Spaces.RectTransform:
					_rectTransform.anchoredPosition = newPosition;
					break;
				case Spaces.Self:
					target.position = _workInitialPosition;
					if ((Mode == Modes.AtoB) || (Mode == Modes.ToDestination))
					{
						newPosition -= _workInitialPosition;
					}
					target.Translate(newPosition, target);
					break;
			}
		}

        /// <summary>
        /// 在停止时，如果移动是活动的，我们会中断它
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
        /// 在恢复时，我们将对象放回其初始位置
        /// </summary>
        protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			
			SetPosition(AnimatePositionTarget.transform, _workInitialPosition);
		}

        /// <summary>
        /// 在禁用时，我们重置协程
        /// </summary>
        public override void OnDisable()
		{
			_coroutine = null;
		}

        /// <summary>
        /// 在验证时，如果需要，我们将迁移过时的动画曲线到Tween类型
        /// </summary>
        public override void OnValidate()
		{
			base.OnValidate();
			MMFeedbacksHelpers.MigrateCurve(AnimatePositionCurve, AnimatePositionTween, Owner);
			MMFeedbacksHelpers.MigrateCurve(AnimatePositionCurveX, AnimatePositionTweenX, Owner);
			MMFeedbacksHelpers.MigrateCurve(AnimatePositionCurveY, AnimatePositionTweenY, Owner);
			MMFeedbacksHelpers.MigrateCurve(AnimatePositionCurveZ, AnimatePositionTweenZ, Owner);
		}
	}
}