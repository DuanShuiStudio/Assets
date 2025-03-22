using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// this feedback will let you apply forces and torques (relative or not) to a Rigidbody
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈机制将允许你向刚体施加力和扭矩（无论是否为相对力和相对扭矩）。")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("GameObject/Rigidbody2D")]
	public class MMF_Rigidbody2D : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用所有这种类型的反馈
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色。
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetRigidbody2D == null); }
		public override string RequiredTargetText { get { return TargetRigidbody2D != null ? TargetRigidbody2D.name : "";  } }
		public override string RequiresSetupText { get { return "此反馈需要设置一个二维目标刚体（TargetRigidbody2D）才能正常工作。你可以在下面进行设置"; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetRigidbody2D = FindAutomatedTarget<Rigidbody2D>();

		public enum Modes { AddForce, AddRelativeForce, AddTorque}

		[MMFInspectorGroup("Rigidbody2D", true, 32, true)]
		/// the rigidbody to target on play
		[Tooltip("运行时要针对的刚体")]
		public Rigidbody2D TargetRigidbody2D;
		/// an extra list of rigidbodies to target on play
		[Tooltip("运行时要针对的额外刚体列表")]
		public List<Rigidbody2D> ExtraTargetRigidbodies2D;
		/// the selected mode for this feedback
		[Tooltip("此反馈所选的模式")]
		public Modes Mode = Modes.AddForce;
		/// the min force or torque to apply
		[Tooltip("要施加的最小力或扭矩")]
		[MMFEnumCondition("Mode", (int)Modes.AddForce, (int)Modes.AddRelativeForce)]
		public Vector2 MinForce;
		/// the max force or torque to apply
		[Tooltip("要施加的最大力或扭矩")]
		[MMFEnumCondition("Mode", (int)Modes.AddForce, (int)Modes.AddRelativeForce)]
		public Vector2 MaxForce;
		/// the min torque to apply to this rigidbody on play
		[Tooltip("在运行时要施加到该刚体上的最小扭矩")]
		[MMFEnumCondition("Mode", (int)Modes.AddTorque)]
		public float MinTorque;
		/// the max torque to apply to this rigidbody on play
		[Tooltip("在运行时要施加到该刚体上的最大扭矩")]
		[MMFEnumCondition("Mode", (int)Modes.AddTorque)]
		public float MaxTorque;
		/// the force mode to apply
		[Tooltip("要施加的力的模式")]
		public ForceMode2D AppliedForceMode = ForceMode2D.Impulse;
		/// if this is true, the velocity of the rigidbody will be reset before applying the new force
		[Tooltip("如果此条件为真，那么在施加新的力之前，刚体的速度将被重置")]
		public bool ResetVelocityOnPlay = false;

		protected Vector2 _force;
		protected float _torque;

        /// <summary>
        /// 在自定义运行时，我们会将力或扭矩施加到目标刚体上。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetRigidbody2D == null))
			{
				return;
			}
			
			ApplyForce(TargetRigidbody2D, feedbacksIntensity);
			foreach (Rigidbody2D rb in ExtraTargetRigidbodies2D)
			{
				ApplyForce(rb, feedbacksIntensity);
			}
		}

        /// <summary>
        /// 将计算得出的力施加到目标刚体上。
        /// </summary>
        /// <param name="rb"></param>
        /// <param name="feedbacksIntensity"></param>
        protected virtual void ApplyForce(Rigidbody2D rb, float feedbacksIntensity)
		{
			if(ResetVelocityOnPlay)
			{
				rb.velocity = Vector2.zero;
			}
			
			switch (Mode)
			{
				case Modes.AddForce:
					_force.x = Random.Range(MinForce.x, MaxForce.x);
					_force.y = Random.Range(MinForce.y, MaxForce.y);
					if (!Timing.ConstantIntensity) { _force *= feedbacksIntensity; }
					rb.AddForce(_force, AppliedForceMode);
					break;
				case Modes.AddRelativeForce:
					_force.x = Random.Range(MinForce.x, MaxForce.x);
					_force.y = Random.Range(MinForce.y, MaxForce.y);
					if (!Timing.ConstantIntensity) { _force *= feedbacksIntensity; }
					rb.AddRelativeForce(_force, AppliedForceMode);
					break;
				case Modes.AddTorque:
					_torque = Random.Range(MinTorque, MaxTorque);
					if (!Timing.ConstantIntensity) { _torque *= feedbacksIntensity; }
					rb.AddTorque(_torque, AppliedForceMode);
					break;
			}
		}
	}
}