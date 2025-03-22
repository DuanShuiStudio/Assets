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
	[FeedbackHelp("该反馈能让你对刚体施加力和扭矩（无论是相对的还是非相对的）")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("GameObject/Rigidbody")]
	public class MMF_Rigidbody : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用所有这种类型的反馈
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色。
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetRigidbody == null); }
		public override string RequiredTargetText { get { return TargetRigidbody != null ? TargetRigidbody.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetRigidbody be set to be able to work properly. You can set one below."; } }
		#endif
		public enum Modes { AddForce, AddRelativeForce, AddTorque, AddRelativeTorque }
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetRigidbody = FindAutomatedTarget<Rigidbody>();

		[MMFInspectorGroup("Rigidbody", true, 61, true)]
		/// the rigidbody to target on play
		[Tooltip("在运行时要针对的刚体")]
		public Rigidbody TargetRigidbody;
		/// a list of extra rigidbodies to target on play
		[Tooltip("一份在运行时要针对的额外刚体列表")]
		public List<Rigidbody> ExtraTargetRigidbodies;
		/// the selected mode for this feedback
		[Tooltip("此反馈的所选模式")]
		public Modes Mode = Modes.AddForce;
		/// the min force or torque to apply
		[Tooltip("要施加的最小力或扭矩")]
		public Vector3 MinForce;
		/// the max force or torque to apply
		[Tooltip("要施加的最大力或扭矩")]
		public Vector3 MaxForce;
		/// the force mode to apply
		[Tooltip("要施加的力的模式")]
		public ForceMode AppliedForceMode = ForceMode.Impulse;
		/// if this is true, the velocity of the rigidbody will be reset before applying the new force
		[Tooltip("如果此选项为真，则在施加新的力之前，刚体的速度将被重置。")]
		public bool ResetVelocityOnPlay = false;
		/// if this is true, the magnitude of the min/max force will be applied in the target transform's forward direction
		[Tooltip("如果此条件为真，最小 / 最大力的大小将沿目标变换组件的前向方向施加")] 
		public bool ForwardForce = false;

		protected Vector3 _force;

        /// <summary>
        /// 在自定义运行时，我们会将力或扭矩施加到目标刚体上。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetRigidbody == null))
			{
				return;
			}

			_force.x = Random.Range(MinForce.x, MaxForce.x);
			_force.y = Random.Range(MinForce.y, MaxForce.y);
			_force.z = Random.Range(MinForce.z, MaxForce.z);

			if (!Timing.ConstantIntensity)
			{
				_force *= feedbacksIntensity;
			}
			
			ApplyForce(TargetRigidbody);
			foreach (Rigidbody rb in ExtraTargetRigidbodies)
			{
				ApplyForce(rb);
			}
		}

        /// <summary>
        /// Applies the computed force to the target rigidbody
        /// </summary>
        /// <param name="rb"></param>
        protected virtual void ApplyForce(Rigidbody rb)
		{
			if(ResetVelocityOnPlay)
			{
				rb.velocity = Vector3.zero;
			}

			if (ForwardForce)
			{
				_force = _force.magnitude * rb.transform.forward;
			}
			
			switch (Mode)
			{
				case Modes.AddForce:
					rb.AddForce(_force, AppliedForceMode);
					break;
				case Modes.AddRelativeForce:
					rb.AddRelativeForce(_force, AppliedForceMode);
					break;
				case Modes.AddTorque:
					rb.AddTorque(_force, AppliedForceMode);
					break;
				case Modes.AddRelativeTorque:
					rb.AddRelativeTorque(_force, AppliedForceMode);
					break;
			}
		}
	}
}