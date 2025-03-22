using MoreMountains.FeedbacksForThirdParty;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// A feedback used to pilot Vector2 springs
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("一种用于操控二维向量（Vector2）弹簧的反馈机制。")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Springs/Spring Vector2")]
	public class MMF_SpringVector2 : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用所有此类反馈。 
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈在检视面板中的颜色。
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SpringColor; } }
		public override string RequiredTargetText => RequiredChannelText;
		public override bool HasCustomInspectors => true;
#endif

        /// 此反馈的持续时间就是缩放的持续时间。 
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(DeclaredDuration); } set { DeclaredDuration = value;  } }
		public override bool HasChannel => true;
		public override bool CanForceInitialValue => true;

		[MMFInspectorGroup("Spring", true, 72)] 
		
		/// the Vector2 spring we want to pilot using this feedback. If you set one, only that spring will be targeted. If you don't, an event will be sent out to all springs matching the channel data info
		[Tooltip("我们想要通过此反馈来操控的二维向量（Vector2）弹簧。如果你设置了一个，那么只有该弹簧会成为目标。如果你不设置，将会向所有与通道数据信息匹配的二维向量弹簧发送一个事件。 ")]
		public MMSpringComponentBase TargetSpring;
		
		/// the duration for the player to consider. This won't impact your particle system, but is a way to communicate to the MMF Player the duration of this feedback. Usually you'll want it to match your actual particle system, and setting it can be useful to have this feedback work with holding pauses.
		[Tooltip("供玩家考虑的时长。这不会对你的粒子系统产生影响，但它是一种向多媒体融合框架（MMF）播放器传达此反馈时长的方式。通常你会希望它与你实际的粒子系统时长相匹配，而且设置这个时长有助于使该反馈在暂停保持状态下正常运行。")]
		public float DeclaredDuration = 0f;
		
		/// the command to use on that spring
		[Tooltip("在那个弹簧上要使用的命令。")]
		public SpringCommands Command = SpringCommands.Bump;
		[MMEnumCondition("Command", (int)SpringCommands.MoveTo, (int)SpringCommands.MoveToAdditive, (int)SpringCommands.MoveToSubtractive, (int)SpringCommands.MoveToInstant)]
		/// the new value this spring should move towards
		[Tooltip("这个弹簧应该朝着移动的新数值。")]
		public Vector2 MoveToValue = new Vector2(2f, 2f);
		/// the amount to add to the spring's current velocity to disturb it and make it bump
		[Tooltip("要添加到弹簧当前速度上的量，用于干扰弹簧并使其产生颠簸效果。")]
		[MMEnumCondition("Command", (int)SpringCommands.Bump)]
		public Vector2 BumpAmount = new Vector2(75f, 75f);
		
		/// the min values between which a random target x value will be picked when calling MoveToRandom
		[Tooltip("在调用“移动到随机位置（MoveToRandom）”时，随机目标 x 值将从中选取的最小值（范围中的最小值，即确定随机目标 x 值取值范围的下限值 ） 。  ")]
		[MMEnumCondition("Command", (int)SpringCommands.MoveToRandom)]
		public Vector2 MoveToRandomValueMin = new Vector2(-2f, -2f);
		/// the min (x) and max (y) values between which a random target y value will be picked when calling MoveToRandom
		[Tooltip("在调用 “移动到随机位置（MoveToRandom）” 时，随机目标 y 值将在其中选取的最小值（x 方向的下限值）和最大值（y 方向的上限值）")]
		[MMEnumCondition("Command", (int)SpringCommands.MoveToRandom)]
		public Vector2 MoveToRandomValueMax = new Vector2(2f, 2f);
		
		/// the min (x) and max (y) values between which a random bump x value will be picked when calling BumpRandom
		[Tooltip("在调用 “随机碰撞（BumpRandom）” 时，用于选取随机碰撞 x 值的最小值（x 方向的下限值）和最大值（y 方向的上限值")]
		[MMEnumCondition("Command", (int)SpringCommands.BumpRandom)]
		public Vector2 BumpAmountRandomValueMin = new Vector2(-20f, -20f);
		/// the min (x) and max (y) values between which a random bump y value will be picked when calling BumpRandom
		[Tooltip("在调用 “随机碰撞（BumpRandom）” 时，用于选取随机碰撞 y 值的最小值（x 方向的下限值）和最大值（y 方向的上限值）")]
		[MMEnumCondition("Command", (int)SpringCommands.BumpRandom)]
		public Vector2 BumpAmountRandomValueMax = new Vector2(20f, 20f);
		
		[Header("Overrides覆盖")]
		/// whether or not to override the current Damping value of the target spring(s) with the one specified below (NewDamping)
		[Tooltip("是否要用下面指定的（新阻尼值，即NewDamping）来覆盖目标弹簧当前的阻尼值。")]
		public bool OverrideDamping = false;
		/// the new damping value to apply to the target spring(s) if OverrideDamping is true
		[Tooltip("如果“OverrideDamping”（覆盖阻尼）为真，则应用于目标弹簧的新阻尼值。")]
		[MMFCondition("OverrideDamping", true)]
		public Vector2 NewDamping = new Vector2(0.8f, 0.8f);
		/// whether or not to override the current Frequency value of the target spring(s) with the one specified below (NewFrequency)
		[Tooltip("是否要用下面指定的（新频率值，即NewFrequency）来覆盖目标弹簧当前的频率值。")]
		public bool OverrideFrequency = false;
		/// the new frequency value to apply to the target spring(s) if OverrideFrequency is true
		[Tooltip("如果“覆盖频率（OverrideFrequency）”为真，则应用于目标弹簧的新频率值。")]
		[MMFCondition("OverrideFrequency", true)]
		public Vector2 NewFrequency = new Vector2(5f, 5f);

		protected MMChannelData _eventChannelData;

        /// <summary>
        /// 在播放时，使用所选设置触发一个弹簧事件。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			_eventChannelData = (TargetSpring == null) ? ChannelData : null;
			MMSpringVector2Event.Trigger(Command, TargetSpring, _eventChannelData, MoveToValue, BumpAmount,
				MoveToRandomValueMin, MoveToRandomValueMax,
				BumpAmountRandomValueMin, BumpAmountRandomValueMax,
				OverrideDamping, NewDamping, OverrideFrequency, NewFrequency);
		}

        /// <summary>
        /// 在停止时，触发一个弹簧停止事件。
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
			_eventChannelData = (TargetSpring == null) ? ChannelData : null;
			MMSpringVector2Event.Trigger(SpringCommands.Stop, TargetSpring, _eventChannelData);
		}

        /// <summary>
        /// 在恢复时，触发一个弹簧恢复初始值事件。 
        /// </summary>
        protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			_eventChannelData = (TargetSpring == null) ? ChannelData : null;
			MMSpringVector2Event.Trigger(SpringCommands.RestoreInitialValue, TargetSpring, _eventChannelData);
		}
	}
}