using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback lets you broadcast a float value to the MMRadio system
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈功能可让你向MMRadio系统广播一个浮点数值。 ")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.MMTools")]
	[FeedbackPath("GameObject/Broadcast")]
	public class MMF_Broadcast : MMF_FeedbackBase
	{
		/// 设置此反馈在检查器中的显示颜色。 
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		#endif
		public override bool HasChannel => true;

		[Header("Level层")]
		/// the curve to tween the intensity on
		[Tooltip("用于对强度进行补间（动画过渡）的曲线 ")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public MMTweenType Curve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
		/// the value to remap the intensity curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public float RemapZero = 0f;
		/// the value to remap the intensity curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public float RemapOne = 1f;
		/// the value to move the intensity to in instant mode
		[Tooltip("在即时模式下将强度移动到的数值")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.Instant)]
		public float InstantChange;

		protected MMF_BroadcastProxy _proxy;
        
		/// <summary>
		/// 在初始化时，我们存储初始的透明度（alpha值）。 
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);

			_proxy = Owner.gameObject.AddComponent<MMF_BroadcastProxy>();
			_proxy.Channel = Channel;
			PrepareTargets();
		}

		/// <summary>
		/// 我们用这个对象来设置我们的目标。
		/// </summary>
		protected override void FillTargets()
		{
			MMF_FeedbackBaseTarget target = new MMF_FeedbackBaseTarget();
			MMPropertyReceiver receiver = new MMPropertyReceiver();
			receiver.TargetObject = Owner.gameObject;
			receiver.TargetComponent = _proxy;
			receiver.TargetPropertyName = "ThisLevel";
			receiver.RelativeValue = RelativeValues;
			target.Target = receiver;
			target.LevelCurve = Curve;
			target.RemapLevelZero = RemapZero;
			target.RemapLevelOne = RemapOne;
			target.InstantLevel = InstantChange;

			_targets.Add(target);
		}
	}
}