using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;
using Random = UnityEngine.Random;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you animate the position of the target object over time, with a spring effect
	/// </summary>
	[AddComponentMenu("")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Transform/Position Spring")]
	[FeedbackHelp("此反馈将让您随时间动画化目标对象的位置，并带有弹簧效果")]
	public class MMF_PositionSpring : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用所有这种类型的反馈
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色。
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
		public override bool EvaluateRequiresSetup() { return (AnimatePositionTarget == null); }
		public override string RequiredTargetText { get { return AnimatePositionTarget != null ? AnimatePositionTarget.name : "";  } }
		public override string RequiresSetupText { get { return "此反馈要求设置一个 “AnimatePositionTarget”，以便正常工作。您可以在下面设置一个"; } }
		public override bool HasCustomInspectors { get { return true; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		public override bool CanForceInitialValue => true;
		protected override void AutomateTargetAcquisition() => AnimatePositionTarget = FindAutomatedTarget<Transform>();
        /// 此反馈的持续时间是位置动画的持续时间
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(DeclaredDuration); } set { DeclaredDuration = value;  } }
		public override bool HasRandomness => true;

		public enum Modes { MoveTo, MoveToAdditive, Bump }
		public enum Spaces { World, Local, RectTransform }
		
		[MMFInspectorGroup("Target", true, 12, true)]
		/// the object to animate
		[Tooltip("要动画化的对象")]
		public Transform AnimatePositionTarget;
		/// spring duration is determined by the spring (and could be impacted real time), so it's up to you to determine how long this feedback should last, from the point of view of its parent MMF Player
		[Tooltip("弹簧持续时间由弹簧决定（并可能实时受到影响），因此从其父 MMF Player 的角度来看，由您确定此反馈应持续多长时间")]
		public float DeclaredDuration = 0f;
		/// the space in which to move the position in
		[Tooltip("在其中移动位置的空间")]
		public Spaces Space = Spaces.World;
		
		[MMFInspectorGroup("Spring Settings", true, 18)]
		/// the dumping ratio determines how fast the spring will evolve after a disturbance. At a low value, it'll oscillate for a long time, while closer to 1 it'll stop oscillating quickly
		[Tooltip("阻尼比决定了弹簧在受到扰动后将如何快速演变。该值较低时，它将长时间振荡；而接近 1 时，它将很快停止振荡")]
		[Range(0.01f, 1f)]
		public float DampingX = 0.4f;
		/// the frequency determines how fast the spring will oscillate when disturbed, low frequency means less oscillations per second, high frequency means more oscillations per second
		[Tooltip("频率决定了弹簧在受到扰动时将如何快速振荡，低频意味着每秒振荡次数少，高频意味着每秒振荡次数多")]
		public float FrequencyX = 6f;
		/// the dumping ratio determines how fast the spring will evolve after a disturbance. At a low value, it'll oscillate for a long time, while closer to 1 it'll stop oscillating quickly
		[Tooltip("阻尼比决定了弹簧在受到扰动后将如何快速演变。该值较低时，它将长时间振荡；而接近 1 时，它将很快停止振荡")]
		[Range(0.01f, 1f)]
		public float DampingY = 0.4f;
		/// the frequency determines how fast the spring will oscillate when disturbed, low frequency means less oscillations per second, high frequency means more oscillations per second
		[Tooltip("频率决定了弹簧在受到扰动时将如何快速振荡，低频意味着每秒振荡次数少，高频意味着每秒振荡次数多")]
		public float FrequencyY = 6f;
		/// the dumping ratio determines how fast the spring will evolve after a disturbance. At a low value, it'll oscillate for a long time, while closer to 1 it'll stop oscillating quickly
		[Tooltip("阻尼比决定了弹簧在受到扰动后将如何快速演变。该值较低时，它将长时间振荡；而接近 1 时，它将很快停止振荡")]
		[Range(0.01f, 1f)]
		public float DampingZ = 0.4f;
		/// the frequency determines how fast the spring will oscillate when disturbed, low frequency means less oscillations per second, high frequency means more oscillations per second
		[Tooltip("频率决定了弹簧在受到扰动时将如何快速振荡，低频意味着每秒振荡次数少，高频意味着每秒振荡次数多")]
		public float FrequencyZ = 6f;
		
		[MMFInspectorGroup("Spring Mode", true, 19)]
		/// the chosen mode for this spring. MoveTo will move the target the specified position (randomized between min and max). MoveToAdditive will add the specified position (randomized between min and max) to the target's current position. Bump will bump the target's position by the specified power (randomized between min and max)
		[Tooltip("为这个弹簧选择的模式。“MoveTo”将把目标移动到指定的位置（在最小值和最大值之间随机化）。“MoveToAdditive”将把指定的位置（在最小值和最大值之间随机化）添加到目标的当前位置。“Bump”将用指定的力量（在最小值和最大值之间随机化）撞击目标的位置。")]
		public Modes Mode = Modes.Bump;
		/// the min value from which to pick a random target value when in MoveTo or MoveToAdditive modes
		[Tooltip("在 “MoveTo” 或 “MoveToAdditive” 模式中，用于选取随机目标值的最小值")]
		[MMFEnumCondition("Mode", (int)Modes.MoveTo, (int)Modes.MoveToAdditive)]
		public Vector3 MoveToPositionMin = new Vector3(1f, 1f, 1f);
		/// the max value from which to pick a random target value when in MoveTo or MoveToAdditive modes
		[Tooltip("在 “MoveTo” 或 “MoveToAdditive” 模式中，用于选取随机目标值的最大值")]
		[MMFEnumCondition("Mode", (int)Modes.MoveTo, (int)Modes.MoveToAdditive)]
		public Vector3 MoveToPositionMax = new Vector3(2f, 2f, 2f);
		/// the min value from which to pick a random bump amount when in Bump mode
		[Tooltip("在 “Bump” 模式中，用于选取随机撞击量的最小值")]
		[MMFEnumCondition("Mode", (int)Modes.Bump)]
		public Vector3 BumpPositionMin = new Vector3(0f, 20f, 0f);
		/// the max value from which to pick a random bump amount when in Bump mode
		[Tooltip("在 “Bump” 模式中，用于选取随机撞击量的最大值")]
		[MMFEnumCondition("Mode", (int)Modes.Bump)]
		public Vector3 BumpPositionMax = new Vector3(0f, 30f, 0f);

		public bool ForceAbsolute = false;
        
		protected Vector3 _currentValue = Vector3.zero;
		protected Vector3 _targetValue = Vector3.zero;
		protected Vector3 _velocity = Vector3.zero;
		
		protected Vector3 _initialPosition;
		protected virtual bool LowVelocity => (Mathf.Abs(_velocity.x) + Mathf.Abs(_velocity.y) + Mathf.Abs(_velocity.z)) < _velocityLowThreshold;
		protected Coroutine _coroutine;
		protected float _velocityLowThreshold = 0.001f;
		protected RectTransform _rectTransform;

        /// <summary>
        /// 在初始化时，我们存储初始位置
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if (Space == Spaces.RectTransform)
			{
				_rectTransform = AnimatePositionTarget.GetComponent<RectTransform>();
			}
			if (Active && (AnimatePositionTarget != null))
			{
				GetInitialValues();
			}
		}

        /// <summary>
        /// 存储初始位置以供将来使用
        /// </summary>
        protected virtual void GetInitialValues()
		{
			switch (Space)
			{
				case Spaces.World:
					_initialPosition = AnimatePositionTarget.position;
					break;
				case Spaces.Local:
					_initialPosition = AnimatePositionTarget.localPosition;
					break;
				case Spaces.RectTransform:
					_initialPosition = _rectTransform.anchoredPosition3D;
					break;
			}
			_currentValue = _initialPosition;
			_targetValue = _currentValue;
		}

        /// <summary>
        /// 在播放时，触发位置动画
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (AnimatePositionTarget == null))
			{
				return;
			}

			if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }

			switch (Mode)
			{
				case Modes.MoveTo:
					_targetValue.x = Random.Range(MoveToPositionMin.x, MoveToPositionMax.x);
					_targetValue.y = Random.Range(MoveToPositionMin.y, MoveToPositionMax.y);
					_targetValue.z = Random.Range(MoveToPositionMin.z, MoveToPositionMax.z);
					break;
				case Modes.MoveToAdditive:
					_targetValue.x += Random.Range(MoveToPositionMin.x, MoveToPositionMax.x);
					_targetValue.y += Random.Range(MoveToPositionMin.y, MoveToPositionMax.y);
					_targetValue.z += Random.Range(MoveToPositionMin.z, MoveToPositionMax.z);
					break;
				case Modes.Bump:
					_velocity.x = Random.Range(BumpPositionMin.x, BumpPositionMax.x);
					_velocity.y = Random.Range(BumpPositionMin.y, BumpPositionMax.y);
					_velocity.z = Random.Range(BumpPositionMin.z, BumpPositionMax.z);
					float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
					_velocity.x *= intensityMultiplier;
					break;
			}
			_coroutine = Owner.StartCoroutine(Spring());
		}

        /// <summary>
        /// 在所有者上运行的一个协程，用于移动弹簧
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator Spring()
		{
			IsPlaying = true;
			UpdateSpring();
			while (!LowVelocity)
			{
				yield return null;
				UpdateSpring();
				ApplyValue();
			}
			
			_velocity.x = 0f;
			_velocity.y = 0f;
			_velocity.z = 0f;
			_currentValue = _targetValue;
			ApplyValue();
			
			IsPlaying = false;
		}

        /// <summary>
        /// 更新弹簧的值
        /// </summary>
        protected virtual void UpdateSpring()
		{
			MMMaths.Spring(ref _currentValue.x, _targetValue.x, ref _velocity.x, DampingX, FrequencyX, FeedbackDeltaTime);
			MMMaths.Spring(ref _currentValue.y, _targetValue.y, ref _velocity.y, DampingY, FrequencyY, FeedbackDeltaTime);
			MMMaths.Spring(ref _currentValue.z, _targetValue.z, ref _velocity.z, DampingZ, FrequencyZ, FeedbackDeltaTime);
			ApplyValue();
		}

		protected Vector3 _appliedPosition;

        /// <summary>
        /// 将当前的弹簧值应用于目标
        /// </summary>
        protected virtual void ApplyValue()
		{
			_appliedPosition = _currentValue;
			if (ForceAbsolute)
			{
				_appliedPosition.x = Mathf.Abs(_appliedPosition.x - _initialPosition.x) + _initialPosition.x;
				_appliedPosition.y = Mathf.Abs(_appliedPosition.y - _initialPosition.y) + _initialPosition.y;
				_appliedPosition.z = Mathf.Abs(_appliedPosition.z - _initialPosition.z) + _initialPosition.z;
			}
			
			if (Space == Spaces.World)
			{
				AnimatePositionTarget.position = _appliedPosition; 	
			}
			else if (Space == Spaces.RectTransform)
			{
				_rectTransform.anchoredPosition3D = _appliedPosition;
			}
			else if (Space == Spaces.Local)
			{
				AnimatePositionTarget.localPosition = _appliedPosition;
			}
		}

        /// <summary>
        /// 在停止时，如果移动是激活的，我们中断移动
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			if (_coroutine != null)
			{
				Owner.StopCoroutine(_coroutine);
			}
			IsPlaying = false;
			_velocity.x = 0f;
			_velocity.y = 0f;
			_velocity.z = 0f;
			_targetValue = _currentValue;
			ApplyValue();
		}

        /// <summary>
        /// 跳转到末尾，与目标值匹配
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomSkipToTheEnd(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (Active && FeedbackTypeAuthorized && (AnimatePositionTarget != null))
			{
				if (_coroutine != null)
				{
					Owner.StopCoroutine(_coroutine);
				}
				_currentValue = _targetValue;
				IsPlaying = false;
				_velocity.x = 0f;
				_velocity.y = 0f;
				_velocity.z = 0f;
				ApplyValue();
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
			_currentValue = _initialPosition;
			_targetValue = _currentValue;
			ApplyValue();
		}
	}
}