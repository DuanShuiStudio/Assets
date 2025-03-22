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
	/// This feedback will let you animate the rotation of the target object over time, with a spring effect
	/// </summary>
	[AddComponentMenu("")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Transform/Rotation Spring")]
	[FeedbackHelp("这种反馈能让你随时间推移，以弹簧效果对目标对象的旋转进行动画处理。")]
	public class MMF_RotationSpring : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用此类的所有反馈。
        public static bool FeedbackTypeAuthorized = true;
        /// 为该反馈设置检查器颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
		public override bool EvaluateRequiresSetup() { return (AnimateRotationTarget == null); }
		public override string RequiredTargetText { get { return AnimateRotationTarget != null ? AnimateRotationTarget.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that an AnimateRotationTarget be set to be able to work properly. You can set one below."; } }
		public override bool HasCustomInspectors { get { return true; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		public override bool CanForceInitialValue => true;
		protected override void AutomateTargetAcquisition() => AnimateRotationTarget = FindAutomatedTarget<Transform>();
        /// 此反馈的持续时间即旋转动画的持续时间。
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(DeclaredDuration); } set { DeclaredDuration = value;  } }
		public override bool HasRandomness => true;

		public enum Modes { MoveTo, MoveToAdditive, Bump }
		
		[MMFInspectorGroup("Target", true, 12, true)]
		/// the object to animate
		[Tooltip("需要制作动画的对象")]
		public Transform AnimateRotationTarget;
		/// spring duration is determined by the spring (and could be impacted real time), so it's up to you to determine how long this feedback should last, from the point of view of its parent MMF Player
		[Tooltip("弹簧持续时间由弹簧特性决定（并且可能实时受到影响），所以，从其父级 MMF 播放器的角度来看，该反馈应持续多久，由你自行决定 。")]
		public float DeclaredDuration = 0f;
		/// whether this feedback should play on local or world rotation
		[Tooltip("此反馈应基于局部旋转还是世界旋转来播放。")]
		public Space RotationSpace = Space.World;
		
		[MMFInspectorGroup("Spring Settings", true, 18)]
		/// the dumping ratio determines how fast the spring will evolve after a disturbance. At a low value, it'll oscillate for a long time, while closer to 1 it'll stop oscillating quickly
		[Tooltip("阻尼比决定了弹簧在受到干扰后恢复的速度。阻尼比数值较低时，弹簧会振荡很长时间；而当阻尼比接近 1 时，振荡会迅速停止。")]
		[Range(0.01f, 1f)]
		public float DampingX = 0.4f;
		/// the frequency determines how fast the spring will oscillate when disturbed, low frequency means less oscillations per second, high frequency means more oscillations per second
		[Tooltip("频率决定了弹簧在受到干扰时振荡的快慢。频率低意味着每秒振荡次数较少，频率高则意味着每秒振荡次数较多。")]
		public float FrequencyX = 6f;
		/// the dumping ratio determines how fast the spring will evolve after a disturbance. At a low value, it'll oscillate for a long time, while closer to 1 it'll stop oscillating quickly
		[Tooltip("阻尼比决定了弹簧在受到干扰后恢复的速度。阻尼比数值较低时，弹簧会振荡很长时间；而当阻尼比接近 1 时，振荡会迅速停止。")]
		[Range(0.01f, 1f)]
		public float DampingY = 0.4f;
		/// the frequency determines how fast the spring will oscillate when disturbed, low frequency means less oscillations per second, high frequency means more oscillations per second
		[Tooltip("频率决定了弹簧在受到干扰时振荡的快慢。频率低意味着每秒振荡次数较少，频率高则意味着每秒振荡次数较多。")]
		public float FrequencyY = 6f;
		/// the dumping ratio determines how fast the spring will evolve after a disturbance. At a low value, it'll oscillate for a long time, while closer to 1 it'll stop oscillating quickly
		[Tooltip("阻尼比决定了弹簧在受到干扰后恢复的速度。阻尼比数值较低时，弹簧会振荡很长时间；而当阻尼比接近 1 时，振荡会迅速停止。")]
		[Range(0.01f, 1f)]
		public float DampingZ = 0.4f;
		/// the frequency determines how fast the spring will oscillate when disturbed, low frequency means less oscillations per second, high frequency means more oscillations per second
		[Tooltip("频率决定了弹簧在受到干扰时振荡的快慢。频率低意味着每秒振荡次数较少，频率高则意味着每秒振荡次数较多。")]
		public float FrequencyZ = 6f;
		
		[MMFInspectorGroup("Spring Mode", true, 19)]
		/// the chosen mode for this spring. MoveTo will move the target the specified rotation (randomized between min and max). MoveToAdditive will add the specified rotation (randomized between min and max) to the target's current rotation. Bump will bump the target's rotation by the specified power (randomized between min and max)
		[Tooltip("这是为该弹簧效果所选的模式：“MoveTo”（移动到）模式会将目标旋转至指定的旋转角度（该角度在最小值和最大值之间随机取值）。“MoveToAdditive”（累加移动到）模式会在目标当前的旋转角度基础上，累加指定的旋转角度（该角度同样在最小值和最大值之间随机取值）。“Bump”（冲击）模式会以指定的力度（在最小值和最大值之间随机取值）冲击目标的旋转角度。")]
		public Modes Mode = Modes.Bump;
		/// the min value from which to pick a random target value when in MoveTo or MoveToAdditive modes
		[Tooltip("当处于 “MoveTo”（移动到）或 “MoveToAdditive”（累加移动到）模式时，用于选取随机目标值的最小值。")]
		[MMFEnumCondition("Mode", (int)Modes.MoveTo, (int)Modes.MoveToAdditive)]
		public Vector3 MoveToRotationMin = new Vector3(45f, 0f, 0f);
		/// the max value from which to pick a random target value when in MoveTo or MoveToAdditive modes
		[Tooltip("当处于 “MoveTo”（移动到）或 “MoveToAdditive”（累加移动到）模式时，用于选取随机目标值的最大值。")]
		[MMFEnumCondition("Mode", (int)Modes.MoveTo, (int)Modes.MoveToAdditive)]
		public Vector3 MoveToRotationMax = new Vector3(90f, 0f, 0f);
		/// the min value from which to pick a random bump amount when in Bump mode
		[Tooltip("在 “Bump”（冲击）模式下，用于选取随机冲击量的最小值")]
		[MMFEnumCondition("Mode", (int)Modes.Bump)]
		public Vector3 BumpRotationMin = new Vector3(2000f, 2000f, 0f);
		/// the max value from which to pick a random bump amount when in Bump mode
		[Tooltip("在 “Bump”（冲击）模式下，用于选取随机冲击量的最大值")]
		[MMFEnumCondition("Mode", (int)Modes.Bump)]
		public Vector3 BumpRotationMax = new Vector3(3000f, 3000f, 0f);
        
		protected Vector3 _currentValue = Vector3.zero;
		protected Vector3 _targetValue = Vector3.zero;
		protected Vector3 _velocity = Vector3.zero;
		
		protected Vector3 _initialRotation;
		protected virtual bool LowVelocity => (Mathf.Abs(_velocity.x) + Mathf.Abs(_velocity.y) + Mathf.Abs(_velocity.z)) < _velocityLowThreshold;
		protected Coroutine _coroutine;
		protected float _velocityLowThreshold = 0.001f;

        /// <summary>
        /// 在初始化时，我们会存储初始旋转状态
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if (Active && (AnimateRotationTarget != null))
			{
				GetInitialValues();
			}
		}

        /// <summary>
        /// 存储初始旋转状态以供后续使用。
        /// </summary>
        protected virtual void GetInitialValues()
		{
			if (RotationSpace == Space.Self)
			{
				_initialRotation = AnimateRotationTarget.localRotation.eulerAngles;
			}
			else
			{
				_initialRotation = AnimateRotationTarget.rotation.eulerAngles;
			}
			_currentValue = _initialRotation;
			_targetValue = _currentValue;
		}

        /// <summary>
        /// 播放时，触发旋转动画。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (AnimateRotationTarget == null))
			{
				return;
			}

			if (_coroutine != null)
			{
				Owner.StopCoroutine(_coroutine);
			}

			switch (Mode)
			{
				case Modes.MoveTo:
					_targetValue.x = Random.Range(MoveToRotationMin.x, MoveToRotationMax.x);
					_targetValue.y = Random.Range(MoveToRotationMin.y, MoveToRotationMax.y);
					_targetValue.z = Random.Range(MoveToRotationMin.z, MoveToRotationMax.z);
					break;
				case Modes.MoveToAdditive:
					_targetValue.x += Random.Range(MoveToRotationMin.x, MoveToRotationMax.x);
					_targetValue.y += Random.Range(MoveToRotationMin.y, MoveToRotationMax.y);
					_targetValue.z += Random.Range(MoveToRotationMin.z, MoveToRotationMax.z);
					break;
				case Modes.Bump:
					_velocity.x = Random.Range(BumpRotationMin.x, BumpRotationMax.x);
					_velocity.y = Random.Range(BumpRotationMin.y, BumpRotationMax.y);
					_velocity.z = Random.Range(BumpRotationMin.z, BumpRotationMax.z);
					float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
					_velocity.x *= intensityMultiplier;
					break;
			}
			_coroutine = Owner.StartCoroutine(Spring());
		}

        /// <summary>
        /// 一个在所有者对象上运行的协程，用于驱动弹簧运动（实现弹簧效果的动画）。
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
        /// 更新弹簧的值。
        /// </summary>
        protected virtual void UpdateSpring()
		{
			MMMaths.Spring(ref _currentValue.x, _targetValue.x, ref _velocity.x, DampingX, FrequencyX, FeedbackDeltaTime);
			MMMaths.Spring(ref _currentValue.y, _targetValue.y, ref _velocity.y, DampingY, FrequencyY, FeedbackDeltaTime);
			MMMaths.Spring(ref _currentValue.z, _targetValue.z, ref _velocity.z, DampingZ, FrequencyZ, FeedbackDeltaTime);
			ApplyValue();
		}

        /// <summary>
        /// 将当前弹簧值应用到目标对象上。
        /// </summary>
        protected virtual void ApplyValue()
		{
			if (RotationSpace == Space.World)
			{
				AnimateRotationTarget.rotation = Quaternion.Euler(_currentValue);
			}
			else
			{
				AnimateRotationTarget.localRotation = Quaternion.Euler(_currentValue);	
			}
		}

        /// <summary>
        /// 停止时，如果运动正在进行，我们将中断该运动。
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
        /// 直接跳转到结束状态，使目标达到设定的目标值。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomSkipToTheEnd(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (Active && FeedbackTypeAuthorized && (AnimateRotationTarget != null))
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
        /// 恢复时，我们将恢复到初始状态。

        /// </summary>
        protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			_currentValue = _initialRotation;
			_targetValue = _currentValue;
			ApplyValue();
		}
	}
}