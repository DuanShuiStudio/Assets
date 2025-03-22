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
	/// This feedback will let you animate the scale of the target object over time, with a spring effect
	/// </summary>
	[AddComponentMenu("")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Transform/Scale Spring")]
	[FeedbackHelp("这种反馈能让你随着时间推移，以一种弹簧效果来对目标对象的缩放比例进行动画处理。")]
	public class MMF_ScaleSpring : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用此类所有反馈。
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色。
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
        /// 这种反馈的持续时间就是缩放动画的持续时间。
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(DeclaredDuration); } set { DeclaredDuration = value;  } }
		public override bool HasRandomness => true;

		public enum Modes { MoveTo, MoveToAdditive, Bump }
		
		[MMFInspectorGroup("Target", true, 12, true)]
		/// the object to animate
		[Tooltip("the object to animate")]
		public Transform AnimateScaleTarget;
		/// spring duration is determined by the spring (and could be impacted real time), so it's up to you to determine how long this feedback should last, from the point of view of its parent MMF Player
		[Tooltip("弹簧作用持续时间由弹簧本身决定（并且可能会受到实时影响），所以从其父级 “MMF 播放器” 的角度来看，由你来决定这种反馈应该持续多长时间。")]
		public float DeclaredDuration = 0f;
		
		[MMFInspectorGroup("Spring Settings", true, 18)]
		/// the dumping ratio determines how fast the spring will evolve after a disturbance. At a low value, it'll oscillate for a long time, while closer to 1 it'll stop oscillating quickly
		[Tooltip("阻尼比决定了弹簧在受到干扰后变化的速度。当阻尼比值较低时，弹簧会振荡很长时间；而当阻尼比值接近 1 时，弹簧会迅速停止振荡。")]
		[Range(0.01f, 1f)]
		public float DampingX = 0.4f;
		/// the frequency determines how fast the spring will oscillate when disturbed, low frequency means less oscillations per second, high frequency means more oscillations per second
		[Tooltip("频率决定了弹簧在受到干扰时的振荡速度。低频意味着每秒振荡次数较少，高频则意味着每秒振荡次数较多。")]
		public float FrequencyX = 6f;
		/// the dumping ratio determines how fast the spring will evolve after a disturbance. At a low value, it'll oscillate for a long time, while closer to 1 it'll stop oscillating quickly
		[Tooltip("阻尼比决定了弹簧在受到干扰后变化的速度。当阻尼比值较低时，弹簧会振荡很长时间；而当阻尼比值接近 1 时，弹簧会迅速停止振荡。")]
		[Range(0.01f, 1f)]
		public float DampingY = 0.4f;
		/// the frequency determines how fast the spring will oscillate when disturbed, low frequency means less oscillations per second, high frequency means more oscillations per second
		[Tooltip("频率决定了弹簧在受到干扰时的振荡速度。低频意味着每秒振荡次数较少，高频则意味着每秒振荡次数较多。")]
		public float FrequencyY = 6f;
		/// the dumping ratio determines how fast the spring will evolve after a disturbance. At a low value, it'll oscillate for a long time, while closer to 1 it'll stop oscillating quickly
		[Tooltip("阻尼比决定了弹簧在受到干扰后变化的速度。当阻尼比值较低时，弹簧会振荡很长时间；而当阻尼比值接近 1 时，弹簧会迅速停止振荡。")]
		[Range(0.01f, 1f)]
		public float DampingZ = 0.4f;
		/// the frequency determines how fast the spring will oscillate when disturbed, low frequency means less oscillations per second, high frequency means more oscillations per second
		[Tooltip("频率决定了弹簧在受到干扰时的振荡速度。低频意味着每秒振荡次数较少，高频则意味着每秒振荡次数较多。")]
		public float FrequencyZ = 6f;
		
		[MMFInspectorGroup("Spring Mode", true, 19)]
		/// the chosen mode for this spring. MoveTo will move the target the specified scale (randomized between min and max). MoveToAdditive will add the specified scale (randomized between min and max) to the target's current scale. Bump will bump the target's scale by the specified power (randomized between min and max)
		[Tooltip("这是为该弹簧选择的模式。“MoveTo”（移动到）模式会将目标移动到指定的缩放比例（在最小值和最大值之间随机取值）。“MoveToAdditive”（累加移动到）模式会将指定的缩放比例（在最小值和最大值之间随机取值）累加到目标当前的缩放比例上。“Bump”（撞击）模式会按照指定的力度（在最小值和最大值之间随机取值）使目标的缩放比例产生变化。")]
		public Modes Mode = Modes.Bump;
		/// the min value from which to pick a random target value when in MoveTo or MoveToAdditive modes
		[Tooltip("在 “MoveTo” 或 “MoveToAdditive” 模式下，用于选取随机目标值的最小值。")]
		[MMFEnumCondition("Mode", (int)Modes.MoveTo, (int)Modes.MoveToAdditive)]
		public Vector3 MoveToScaleMin = new Vector3(1f, 1f, 1f);
		/// the max value from which to pick a random target value when in MoveTo or MoveToAdditive modes
		[Tooltip("在 “MoveTo” 或 “MoveToAdditive” 模式下，用于选取随机目标值的最大值。")]
		[MMFEnumCondition("Mode", (int)Modes.MoveTo, (int)Modes.MoveToAdditive)]
		public Vector3 MoveToScaleMax = new Vector3(2f, 2f, 2f);
		/// the min value from which to pick a random bump amount when in Bump mode
		[Tooltip("在 “Bump”（撞击）模式下，用于选取随机撞击量的最小值。\r\n")]
		[MMFEnumCondition("Mode", (int)Modes.Bump)]
		public Vector3 BumpScaleMin = new Vector3(20f, 20f, 20f);
		/// the max value from which to pick a random bump amount when in Bump mode
		[Tooltip("在 “Bump”（撞击）模式下，用于选取随机撞击量的最大值。\r\n")]
		[MMFEnumCondition("Mode", (int)Modes.Bump)]
		public Vector3 BumpScaleMax = new Vector3(30f, 30f, 30f);
        
		protected Vector3 _currentValue = Vector3.zero;
		protected Vector3 _targetValue = Vector3.zero;
		protected Vector3 _velocity = Vector3.zero;
		
		protected Vector3 _initialScale;
		protected virtual bool LowVelocity => (Mathf.Abs(_velocity.x) + Mathf.Abs(_velocity.y) + Mathf.Abs(_velocity.z)) < _velocityLowThreshold;
		protected Coroutine _coroutine;
		protected float _velocityLowThreshold = 0.001f;
		protected Vector3 _newScale;

        /// <summary>
        /// 在初始化时，我们存储我们的初始缩放比例。
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if (Active && (AnimateScaleTarget != null))
			{
				GetInitialValues();
			}
		}

        /// <summary>
        /// 存储初始缩放比例，以备将来使用。
        /// </summary>
        protected virtual void GetInitialValues()
		{
			_initialScale = AnimateScaleTarget.localScale;
			_currentValue = AnimateScaleTarget.localScale;
			_targetValue = _currentValue;
		}

        /// <summary>
        /// 在播放时，触发缩放动画。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (AnimateScaleTarget == null))
			{
				return;
			}

			if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }

			switch (Mode)
			{
				case Modes.MoveTo:
					_targetValue.x = Random.Range(MoveToScaleMin.x, MoveToScaleMax.x);
					_targetValue.y = Random.Range(MoveToScaleMin.y, MoveToScaleMax.y);
					_targetValue.z = Random.Range(MoveToScaleMin.z, MoveToScaleMax.z);
					break;
				case Modes.MoveToAdditive:
					_targetValue.x += Random.Range(MoveToScaleMin.x, MoveToScaleMax.x);
					_targetValue.y += Random.Range(MoveToScaleMin.y, MoveToScaleMax.y);
					_targetValue.z += Random.Range(MoveToScaleMin.z, MoveToScaleMax.z);
					break;
				case Modes.Bump:
					_velocity.x = Random.Range(BumpScaleMin.x, BumpScaleMax.x);
					_velocity.y = Random.Range(BumpScaleMin.y, BumpScaleMax.y);
					_velocity.z = Random.Range(BumpScaleMin.z, BumpScaleMax.z);
					float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
					_velocity.x *= intensityMultiplier;
					break;
			}
			_coroutine = Owner.StartCoroutine(Spring());
		}

        /// <summary>
        /// 一个在所有者（Owner）上运行的协程，用于移动弹簧。
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
        /// 更新弹簧的各项数值。
        /// </summary>
        protected virtual void UpdateSpring()
		{
			MMMaths.Spring(ref _currentValue.x, _targetValue.x, ref _velocity.x, DampingX, FrequencyX, FeedbackDeltaTime);
			MMMaths.Spring(ref _currentValue.y, _targetValue.y, ref _velocity.y, DampingY, FrequencyY, FeedbackDeltaTime);
			MMMaths.Spring(ref _currentValue.z, _targetValue.z, ref _velocity.z, DampingZ, FrequencyZ, FeedbackDeltaTime);
			ApplyValue();
		}

        /// <summary>
        /// 将当前的弹簧数值应用到目标对象上。
        /// </summary>
        protected virtual void ApplyValue()
		{
			_newScale.x = Mathf.Abs(_currentValue.x);
			_newScale.y = Mathf.Abs(_currentValue.y);
			_newScale.z = Mathf.Abs(_currentValue.z);
			AnimateScaleTarget.localScale = _currentValue;
		}

        /// <summary>
        /// 在停止时，如果移动操作处于活动状态，我们就中断该移动。
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
        /// 跳转到结尾处，使其与目标值相匹配。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomSkipToTheEnd(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (Active && FeedbackTypeAuthorized && (AnimateScaleTarget != null))
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
        /// 在恢复时，我们恢复到初始状态。
        /// </summary>
        protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			_currentValue = _initialScale;
			_targetValue = _currentValue;
			ApplyValue();
		}
	}
}