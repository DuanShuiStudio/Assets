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
	/// This feedback will let you animate the scale of the target object over time, with a spring + squash and stretch effect
	/// </summary>
	[AddComponentMenu("")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Transform/Squash and Stretch Spring")]
	[FeedbackHelp("此反馈功能可让你随时间推移对目标对象的缩放进行动画处理，并带有弹簧效果以及挤压和拉伸效果")]
	public class MMF_SquashAndStretchSpring : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用此类所有反馈。
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色。
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
		public override bool EvaluateRequiresSetup() { return (AnimateScaleTarget == null); }
		public override string RequiredTargetText { get { return AnimateScaleTarget != null ? AnimateScaleTarget.name : "";  } }
		public override string RequiresSetupText { get { return "此反馈需要设置一个 “缩放动画目标” 才能正常工作。你可以在下面进行设置"; } }
		public override bool HasCustomInspectors { get { return true; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		public override bool CanForceInitialValue => true;
		protected override void AutomateTargetAcquisition() => AnimateScaleTarget = FindAutomatedTarget<Transform>();
        /// 此反馈的持续时间即缩放动画的持续时间
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(DeclaredDuration); } set { DeclaredDuration = value;  } }
		public override bool HasRandomness => true;

		public enum Modes { MoveTo, MoveToAdditive, Bump }
		public enum PossibleAxis { XtoYZ, XtoY, XtoZ, YtoXZ, YtoX, YtoZ, ZtoXZ, ZtoX, ZtoY }
		
		[MMFInspectorGroup("Target", true, 12, true)]
		/// the object to animate
		[Tooltip("要进行动画处理的对象")]
		public Transform AnimateScaleTarget;
		/// spring duration is determined by the spring (and could be impacted real time), so it's up to you to determine how long this feedback should last, from the point of view of its parent MMF Player
		[Tooltip("弹簧效果的持续时间由弹簧属性决定（并且可能会受到实时因素的影响），所以从其父级 MMF 播放器的角度来看，该反馈应该持续多长时间由你自己决定")]
		public float DeclaredDuration = 0f;
		/// the axis on which to operate squashing and stretching
		[Tooltip("进行挤压和拉伸操作所基于的轴")]
		public PossibleAxis Axis = PossibleAxis.XtoYZ;
		
		[MMFInspectorGroup("Spring Settings", true, 18)]
		/// the dumping ratio determines how fast the spring will evolve after a disturbance. At a low value, it'll oscillate for a long time, while closer to 1 it'll stop oscillating quickly
		[Tooltip("阻尼比决定了弹簧在受到扰动后状态变化的速度。当阻尼比取值较低时，弹簧会持续振荡较长时间；而当阻尼比接近 1 时，弹簧会迅速停止振荡")]
		[Range(0.01f, 1f)]
		public float Damping = 0.4f;
		/// the frequency determines how fast the spring will oscillate when disturbed, low frequency means less oscillations per second, high frequency means more oscillations per second
		[Tooltip("频率决定了弹簧在受到扰动时的振荡速度。低频意味着每秒的振荡次数较少，高频则意味着每秒的振荡次数较多。")]
		public float Frequency = 6f;
		
		[MMFInspectorGroup("Spring Mode", true, 19)]
		/// the chosen mode for this spring. MoveTo will move the target the specified scale (randomized between min and max). MoveToAdditive will add the specified scale (randomized between min and max) to the target's current scale. Bump will bump the target's scale by the specified power (randomized between min and max)
		[Tooltip("这是此弹簧效果所选的模式：“移动至（MoveTo）” 模式会将目标缩放至指定的缩放比例（该比例在最小值和最大值之间随机确定）。“累加移动至（MoveToAdditive）” 模式会将指定的缩放比例（同样在最小值和最大值之间随机确定）累加到目标当前的缩放比例上。“冲击（Bump）” 模式会按照指定的强度（在最小值和最大值之间随机确定）对目标的缩放比例产生冲击性变化。")]
		public Modes Mode = Modes.Bump;
		/// the min value from which to pick a random target value when in MoveTo or MoveToAdditive modes
		[Tooltip("在 “移动至（MoveTo）” 或 “累加移动至（MoveToAdditive）” 模式下，用于选取随机目标值的最小值。")]
		[MMFEnumCondition("Mode", (int)Modes.MoveTo, (int)Modes.MoveToAdditive)]
		public float MoveToMin = 1f;
		/// the max value from which to pick a random target value when in MoveTo or MoveToAdditive modes
		[Tooltip("在 “移动至（MoveTo）” 或 “累加移动至（MoveToAdditive）” 模式下，用于选取随机目标值的最大值。")]
		[MMFEnumCondition("Mode", (int)Modes.MoveTo, (int)Modes.MoveToAdditive)]
		public float MoveToMax = 2f;

		/// the min value from which to pick a random bump amount when in Bump mode
		[Tooltip("在 “冲击（Bump）” 模式下，用于选取随机冲击量的最小值")]
		[MMFEnumCondition("Mode", (int)Modes.Bump)]
		public float BumpScaleMin = 20f;

		/// the max value from which to pick a random bump amount when in Bump mode
		[Tooltip("在 “冲击（Bump）” 模式下，用于选取随机冲击量的最大值")]
		[MMFEnumCondition("Mode", (int)Modes.Bump)]
		public float BumpScaleMax = 30f;

		protected float _currentValue = 0f;
		protected float _targetValue = 0f;
		protected float _velocity = 0f;
		
		protected virtual bool LowVelocity => Mathf.Abs(_velocity) < _velocityLowThreshold;
		protected Coroutine _coroutine;
		protected float _velocityLowThreshold = 0.001f;
		
		protected Vector3 _newScale;
		protected Vector3 _initialScale;

        /// <summary>
        /// 在初始化时，我们存储目标对象的初始缩放比例。
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
        /// 存储初始缩放比例，以便日后使用。
        /// </summary>
        protected virtual void GetInitialValues()
		{
			_initialScale = AnimateScaleTarget.localScale;
			_currentValue = AnimateScaleTarget.localScale.x;
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

			if (_coroutine != null)	{ Owner.StopCoroutine(_coroutine); }

			switch (Mode)
			{
				case Modes.MoveTo:
					_targetValue = Random.Range(MoveToMin, MoveToMax);
					break;
				case Modes.MoveToAdditive:
					_targetValue += Random.Range(MoveToMin, MoveToMax);
					break;
				case Modes.Bump:
					_velocity = Random.Range(BumpScaleMin, BumpScaleMax);
					break;
			}
			_coroutine = Owner.StartCoroutine(Spring());
		}

        /// <summary>
        /// 一个在 “所有者（Owner）” 上运行的协程，用于操控弹簧（使其产生相应的动态效果）
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
			
			_velocity = 0f;
			_currentValue = _targetValue;
			ApplyValue();
			
			IsPlaying = false;
		}

        /// <summary>
        /// 更新弹簧的各项数值。
        /// </summary>
        protected virtual void UpdateSpring()
		{
			MMMaths.Spring(ref _currentValue, _targetValue, ref _velocity, Damping, Frequency, FeedbackDeltaTime);
			ApplyValue();
		}

        /// <summary>
        /// 将当前弹簧的数值应用到目标对象上。
        /// </summary>
        protected virtual void ApplyValue()
		{
			float newValue = _currentValue;
			float invertScale = 1 / Mathf.Sqrt(newValue);
			switch (Axis)
			{
				case PossibleAxis.XtoYZ:
					_newScale.x = newValue;
					_newScale.y = invertScale;
					_newScale.z = invertScale;
					break;
				case PossibleAxis.XtoY:
					_newScale.x = newValue;
					_newScale.y = invertScale;
					_newScale.z = _initialScale.z;
					break;
				case PossibleAxis.XtoZ:
					_newScale.x = newValue;
					_newScale.y = _initialScale.y;
					_newScale.z = invertScale;
					break;
				case PossibleAxis.YtoXZ:
					_newScale.x = invertScale;
					_newScale.y = newValue;
					_newScale.z = invertScale;
					break;
				case PossibleAxis.YtoX:
					_newScale.x = invertScale;
					_newScale.y = newValue;
					_newScale.z = _initialScale.z;
					break;
				case PossibleAxis.YtoZ:
					_newScale.x = newValue;
					_newScale.y = _initialScale.y;
					_newScale.z = invertScale;
					break;
				case PossibleAxis.ZtoXZ:
					_newScale.x = invertScale;
					_newScale.y = invertScale;
					_newScale.z = newValue;
					break;
				case PossibleAxis.ZtoX:
					_newScale.x = invertScale;
					_newScale.y = _initialScale.y;
					_newScale.z = newValue;
					break;
				case PossibleAxis.ZtoY:
					_newScale.x = _initialScale.x;
					_newScale.y = invertScale;
					_newScale.z = newValue;
					break;
			}
			_newScale.x = Mathf.Abs(_newScale.x);
			_newScale.y = Mathf.Abs(_newScale.y);
			_newScale.z = Mathf.Abs(_newScale.z);
			AnimateScaleTarget.localScale = _newScale;
		}

        /// <summary>
        /// 在停止时，如果（目标对象的）移动处于激活状态，我们就会中断该移动。
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
			_velocity = 0f;
			_targetValue = _currentValue;
			ApplyValue();
		}

        /// <summary>
        /// 直接跳转到结尾状态，使（当前状态）与目标值相匹配。
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
				_velocity = 0f;
				ApplyValue();
			}
		}


        /// <summary>
        /// 在恢复时，我们恢复其初始状态
        /// </summary>
        protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			_currentValue = _initialScale.x;
			_targetValue = _currentValue;
			ApplyValue();
		}
	}
}