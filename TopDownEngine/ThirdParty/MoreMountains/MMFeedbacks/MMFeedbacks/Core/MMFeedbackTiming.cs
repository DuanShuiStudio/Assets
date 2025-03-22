using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// 时间刻度（time scale）的可能模式
    public enum TimescaleModes { Scaled, Unscaled }

    /// <summary>
    /// 一个收集延迟、冷却时间和重复次数值的类，用于定义每个 MMFeedback 的行为
    /// </summary>
    [System.Serializable]
	public class MMFeedbackTiming
	{
        /// 根据宿主 MMFeedbacks 的方向，此反馈可能的播放方式
        public enum MMFeedbacksDirectionConditions { Always, OnlyWhenForwards, OnlyWhenBackwards };
        /// 这种反馈可能的播放方式
        public enum PlayDirections { FollowMMFeedbacksDirection, OppositeMMFeedbacksDirection, AlwaysNormal, AlwaysRewind }

		[Header("Timescale时间尺度")]
		/// whether we're working on scaled or unscaled time
		[Tooltip("我们是在处理经过缩放的时间还是未缩放的时间。 ")]
		public TimescaleModes TimescaleMode = TimescaleModes.Scaled;
        
		[Header("Exceptions例外")]
		/// if this is true, holding pauses won't wait for this feedback to finish 
		[Tooltip("如果这是真的（即该条件为真），那么保持暂停状态时将不会等待此反馈完成。 ")]
		public bool ExcludeFromHoldingPauses = false;
		/// whether to count this feedback in the parent MMFeedbacks(Player) total duration or not
		[Tooltip("是否将此反馈纳入父级 MMFeedbacks（玩家）的总持续时间计算当中。 ")]
		public bool ContributeToTotalDuration = true;

		[Header("Delays延迟")]
		/// the initial delay to apply before playing the delay (in seconds)
		[Tooltip("播放延迟之前要应用的初始延迟（以秒为单位）。")]
		public float InitialDelay = 0f;
		/// the cooldown duration mandatory between two plays
		[Tooltip("两次播放之间必须的冷却时间长度。 ")]
		public float CooldownDuration = 0f;

		[Header("Stop停止")]
		/// if this is true, this feedback will interrupt itself when Stop is called on its parent MMFeedbacks, otherwise it'll keep running
		[Tooltip("如果这为真，当对其所属的父级 MMFeedbacks 调用“停止”操作时，此反馈将会中断自身；否则，它将继续运行。 ")]
		public bool InterruptsOnStop = true;

		[Header("Repeat重播")]
		/// the repeat mode, whether the feedback should be played once, multiple times, or forever
		[Tooltip("重复模式，即该反馈是应该播放一次、多次，还是无限循环播放")]
		public int NumberOfRepeats = 0;
		/// if this is true, the feedback will be repeated forever
		[Tooltip("如果这是真的，那么该反馈将永远重复下去。 ")]
		public bool RepeatForever = false;
		/// the delay (in seconds) between two firings of this feedback. This doesn't include the duration of the feedback. 
		[Tooltip("该反馈两次触发之间的延迟（以秒为单位）。这不包括该反馈本身的持续时长。 ")]
		public float DelayBetweenRepeats = 1f;

		[Header("PlayCount播放次数")]
		/// the number of times this feedback's been played since its initialization (or last reset if SetPlayCountToZeroOnReset is true) 
		[Tooltip("自该反馈初始化以来（或者如果“在重置时将播放次数设置为零”为真，则是自上次重置以来），此反馈已被播放的次数。 ")]
		[MMFReadOnly]
		public int PlayCount = 0;
		/// whether or not to limit the amount of times this feedback can be played. beyond that amount, it won't play anymore 
		[Tooltip("是否要限制此反馈的可播放次数。超过该次数后，它将不再播放。 ")]
		public bool LimitPlayCount = false;
		/// if LimitPlayCount is true, the maximum amount of times this feedback can be played
		[Tooltip("如果“限制播放次数”选项为真，那么这就是该反馈所能播放的最大次数。")]
		[MMFCondition("LimitPlayCount", true)]
		public int MaxPlayCount = 3;
		/// if LimitPlayCount is true, whether or not to reset the play count to zero when the feedback is reset
		[Tooltip("如果“限制播放次数（LimitPlayCount）”为真，那么当该反馈被重置时，是否要将播放次数重置为零。 ")]
		[MMFCondition("LimitPlayCount", true)]
		public bool SetPlayCountToZeroOnReset = false;
		
		[Header("Play Direction播放方向")]
		/// this defines how this feedback should play when the host MMFeedbacks is played :
		/// - always (default) : this feedback will always play
		/// - OnlyWhenForwards : this feedback will only play if the host MMFeedbacks is played in the top to bottom direction (forwards)
		/// - OnlyWhenBackwards : this feedback will only play if the host MMFeedbacks is played in the bottom to top direction (backwards)
		[Tooltip("这定义了当宿主 MMFeedbacks 被播放时，此反馈应该以怎样的方式进行播放：" +
                 "- 始终（默认设置）always (default)：此反馈将始终进行播放" +
                 "- 仅在正向播放时 OnlyWhenForwards：只有当宿主 MMFeedbacks 以从上到下的方向（正向）播放时，此反馈才会播放。" +
                 "- 仅在反向播放时 OnlyWhenBackwards：只有当宿主 MMFeedbacks 以从下到上的方向（反向）播放时，此反馈才会进行播放。")]
		public MMFeedbacksDirectionConditions MMFeedbacksDirectionCondition = MMFeedbacksDirectionConditions.Always;
		/// this defines the way this feedback will play. It can play in its normal direction, or in rewind (a sound will play backwards, 
		/// an object normally scaling up will scale down, a curve will be evaluated from right to left, etc)
		/// - BasedOnMMFeedbacksDirection : will play normally when the host MMFeedbacks is played forwards, in rewind when it's played backwards
		/// - OppositeMMFeedbacksDirection : will play in rewind when the host MMFeedbacks is played forwards, and normally when played backwards
		/// - Always Normal : will always play normally, regardless of the direction of the host MMFeedbacks
		/// - Always Rewind : will always play in rewind, regardless of the direction of the host MMFeedbacks
		[Tooltip("这定义了此反馈的播放方式。它可以按正常方向播放，也可以以倒放的方式播放（比如一段声音会倒着播放），" +
                 " 一个通常会放大的对象将会缩小，一条曲线将会从右往左进行求值，等等。 " +
                 "- 基于 MMFeedbacks 的播放方向 BasedOnMMFeedbacksDirection：当宿主 MMFeedbacks 正向播放时，此反馈将正常播放；当宿主 MMFeedbacks 反向播放时，此反馈将以倒放的形式播放。" +
                 "- 与 MMFeedbacks 播放方向相反 OppositeMMFeedbacksDirection：当宿主 MMFeedbacks 正向播放时，此反馈将以倒放的形式播放；而当宿主 MMFeedbacks 反向播放时，此反馈将正常播放。 " +
                 "- 始终正常播放 Always Normal：无论宿主 MMFeedbacks 的播放方向如何，此反馈都将始终以正常方式播放。 " +
                 "- 始终倒放 Always Rewind：无论宿主MMFeedbacks的播放方向如何，此反馈都将始终以倒放的方式播放。 ")]
		public PlayDirections PlayDirection = PlayDirections.FollowMMFeedbacksDirection;

		[Header("Intensity强度")]
		/// if this is true, intensity will be constant, even if the parent MMFeedbacks is played at a lower intensity
		[Tooltip("如果这为真，即便父级 MMFeedbacks 以较低的强度播放，该（反馈的）强度也将保持恒定。 ")]
		public bool ConstantIntensity = false;
		/// if this is true, this feedback will only play if its intensity is higher or equal to IntensityIntervalMin and lower than IntensityIntervalMax
		[Tooltip("如果这是真的，那么只有当此反馈的强度大于或等于“强度区间最小值（IntensityIntervalMin）”且小于“强度区间最大值（IntensityIntervalMax）”时，该反馈才会播放。  ")]
		public bool UseIntensityInterval = false;
		/// the minimum intensity required for this feedback to play
		[Tooltip("此反馈能够播放所需的最低强度。 ")]
		[MMFCondition("UseIntensityInterval", true)]
		public float IntensityIntervalMin = 0f;
		/// the maximum intensity required for this feedback to play
		[Tooltip("此反馈能够播放所需的最高强度。 ")]
		[MMFCondition("UseIntensityInterval", true)]
		public float IntensityIntervalMax = 0f;

		[Header("Sequence序列")]
		/// A MMSequence to use to play these feedbacks on
		[Tooltip("一个用于播放这些反馈的 MM序列。 ")]
		public MMSequence Sequence;
		/// The MMSequence's TrackID to consider
		[Tooltip("需考虑的 MM 序列的轨道标识符")]
		public int TrackID = 0;
		/// whether or not to use the quantized version of the target sequence
		[Tooltip("是否要使用目标序列的量化版本。 ")]
		public bool Quantized = false;
		/// if using the quantized version of the target sequence, the BPM to apply to the sequence when playing it
		[Tooltip("如果使用目标序列的量化版本，在播放该序列时要应用的每分钟节拍数（BPM）")]
		[MMFCondition("Quantized", true)]
		public int TargetBPM = 120;

        /// 从任何类中，你都可以将 UseScriptDrivenTimescale 设置为 true。从那时起，该反馈将不再依赖 Time.time、Time.deltaTime（或它们的未缩放等效值），而是基于你通过 ScriptDrivenDeltaTime 和 ScriptDrivenTime 提供给它们的值来计算时间。
        public virtual bool UseScriptDrivenTimescale { get; set; }
        /// 此反馈应使用的时间间隔（增量时间）值。 
        public virtual float ScriptDrivenDeltaTime { get; set; }
        /// 此反馈应使用的时间值。 
        public virtual float ScriptDrivenTime { get; set; }
	}
}