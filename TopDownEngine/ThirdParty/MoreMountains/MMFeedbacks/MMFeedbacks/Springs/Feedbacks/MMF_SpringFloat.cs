using MoreMountains.FeedbacksForThirdParty;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// A feedback used to pilot float springs
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("一种用于操控浮点型弹簧的反馈机制。 ")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Springs/Spring Float")]
	public class MMF_SpringFloat : MMF_Feedback
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
		
		/// the spring we want to pilot using this feedback. If you set one, only that spring will be targeted. If you don't, an event will be sent out to all springs matching the channel data info
		[Tooltip("我们想要通过此反馈来操控的弹簧。如果你设置了一个弹簧，那么只有该弹簧会成为目标。如果你不设置，将会向所有与通道数据信息匹配的弹簧发送一个事件。 ")]
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
		public float MoveToValue = 2f;
		/// the amount to add to the spring's current velocity to disturb it and make it bump
		[Tooltip("要添加到弹簧当前速度上的量，用于干扰弹簧并使其产生颠簸效果。 ")]
		[MMEnumCondition("Command", (int)SpringCommands.Bump)]
		public float BumpAmount = 75f;
		/// a min and max values to pick a random value from to move the spring to when MoveToRandom is called
		[Tooltip("当调用“移动到随机（MoveToRandom）”时，用于从中选取一个随机值来移动弹簧的最小值和最大值。 ")]
		[MMEnumCondition("Command", (int)SpringCommands.MoveToRandom)]
		public Vector2 MoveToRandomValue = new Vector2(-2f, 2f);
		/// a min and max values to pick a random value from to add to the spring's velocity when BumpRandom is called
		[Tooltip("当调用“随机碰撞（BumpRandom）”时，用于从中选取一个随机值来添加到弹簧速度上的最小值和最大值。 ")]
		[MMEnumCondition("Command", (int)SpringCommands.BumpRandom)]
		public Vector2 BumpAmountRandomValue = new Vector2(-50f, 50f);
		
		[Header("Overrides覆盖")]
		/// whether or not to override the current Damping value of the target spring(s) with the one specified below (NewDamping)
		[Tooltip("是否要用下面指定的（新阻尼值，即NewDamping）来覆盖目标弹簧当前的阻尼值。")]
		public bool OverrideDamping = false;
		/// the new damping value to apply to the target spring(s) if OverrideDamping is true
		[Tooltip("如果“OverrideDamping”（覆盖阻尼）为真，则应用于目标弹簧的新阻尼值。")]
		[MMFCondition("OverrideDamping", true)]
		public float NewDamping = 0.8f;
		/// whether or not to override the current Frequency value of the target spring(s) with the one specified below (NewFrequency)
		[Tooltip("是否要用下面指定的（新频率值，即NewFrequency）来覆盖目标弹簧当前的频率值。")]
		public bool OverrideFrequency = false;
		/// the new frequency value to apply to the target spring(s) if OverrideFrequency is true
		[Tooltip("如果“覆盖频率（OverrideFrequency）”为真，则应用于目标弹簧的新频率值。")]
		[MMFCondition("OverrideFrequency", true)]
		public float NewFrequency = 5f;

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
			MMSpringFloatEvent.Trigger(Command, TargetSpring, _eventChannelData, MoveToValue, BumpAmount, MoveToRandomValue, BumpAmountRandomValue, OverrideDamping, NewDamping, OverrideFrequency, NewFrequency);
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
			MMSpringFloatEvent.Trigger(SpringCommands.Stop, TargetSpring, _eventChannelData);
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
			MMSpringFloatEvent.Trigger(SpringCommands.RestoreInitialValue, TargetSpring, _eventChannelData);
		}
	}
}