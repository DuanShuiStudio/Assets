using System;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 用于操控弹簧的可能的指令 
    /// MoveTo“移动至”：将弹簧的当前值移动到事件中指定的“移动至值”。 
    /// MoveToAdditive“累加移动至”：将事件中指定的“移动至值”累加到弹簧当前的目标值上。 
    /// MoveToSubtractive“累减移动至”：从弹簧当前的目标值中减去事件中指定的“移动至值”。 
    /// MoveToRandom“移动至随机值”：使用“移动至随机值”将弹簧的当前值移动到一个随机值。 
    /// MoveToInstant“瞬间移动至”：将弹簧的当前值瞬间移动到事件中指定的 “移动至值”。
    /// Bump“碰撞”：按照事件中指定的“碰撞量”使弹簧产生碰撞效果。 
    /// BumpRandom“随机碰撞”：按照事件中指定的一个随机量使弹簧产生碰撞效果。 
    /// Stop停止：立即停止弹簧的运动。 
    /// Finish完成：立即将弹簧移动到其最终目标值。 
    /// RestoreInitialValue恢复初始值：恢复弹簧的初始值。 
    /// ResetInitialValue重置初始值：将弹簧的初始值重置为其当前值。 
    /// </summary>
    public enum SpringCommands { MoveTo, MoveToAdditive, MoveToSubtractive, MoveToRandom, MoveToInstant, Bump, BumpRandom, Stop, Finish, RestoreInitialValue, ResetInitialValue }

    /// <summary>
    /// 一个用于操控MMSpringColor组件的事件。 
    /// </summary>
    public struct MMSpringFloatEvent
	{
		static MMSpringFloatEvent e;
		
		public MMChannelData ChannelData;
		public MMSpringComponentBase TargetSpring;
		public SpringCommands Command;
		public float MoveToValue;
		public float BumpAmount;
		public Vector2 MoveToRandomValue;
		public Vector2 BumpAmountRandomValue;
		public bool OverrideDamping;
		public float NewDamping;
		public bool OverrideFrequency;
		public float NewFrequency;
		
		public static void Trigger(SpringCommands command, MMSpringComponentBase targetSpring, MMChannelData channelData, 
			float moveToValue = 1f, float bumpAmount = 1f, Vector2 moveToRandomValue = default, Vector2 bumpAmountRandomValue = default, 
			bool overrideDamping = false, float newDamping = 0.8f, bool overrideFrequency = false, float newFrequency = 5f)
		{
			e.ChannelData = channelData;
			e.TargetSpring = targetSpring;
			e.Command = command;
			e.MoveToValue = moveToValue;
			e.BumpAmount = bumpAmount;
			e.MoveToRandomValue = moveToRandomValue;
			e.BumpAmountRandomValue = bumpAmountRandomValue;
			e.OverrideDamping = overrideDamping;
			e.NewDamping = newDamping;
			e.OverrideFrequency = overrideFrequency;
			e.NewFrequency = newFrequency;
			MMEventManager.TriggerEvent(e);
		}
	}

    /// <summary>
    /// 一个用于操控目标上的浮点数值的弹簧组件。 
    /// </summary>
    public abstract class MMSpringFloatComponent<T> : MMSpringComponentBase, MMEventListener<MMSpringFloatEvent> where T:Component
	{
		[MMInspectorGroup("Target", true, 17)] 
		public T Target;
		
		[MMInspectorGroup("Channel & TimeScale", true, 16, true)] 
		/// whether this spring should run on scaled time (and be impacted by time scale changes) or unscaled time (and not be impacted by time scale changes)
		[Tooltip("这个弹簧是应该在缩放时间下运行（并受时间缩放变化的影响），还是在未缩放时间下运行（且不受时间缩放变化的影响） 。 ")]
		public TimeScaleModes TimeScaleMode = TimeScaleModes.Scaled;
		/// whether to listen on a channel defined by an int or by a MMChannel scriptable object. Ints are simple to setup but can get messy and make it harder to remember what int corresponds to what.
		/// MMChannel scriptable objects require you to create them in advance, but come with a readable name and are more scalable
		[Tooltip("是要监听由一个整数定义的通道，还是由一个MMChannel可编写脚本对象定义的通道。整数设置起来很简单，但可能会变得混乱，并且更难记住哪个整数对应着什么内容。" +
                 "MMChannel可编写脚本对象要求你提前创建它们，但它们带有易于理解的名称，并且更具可扩展性。")]
		public MMChannelModes ChannelMode = MMChannelModes.Int;
		/// the channel to listen to - has to match the one on the feedback
		[Tooltip("要监听的通道 - 必须与反馈端的通道相匹配。 ")]
		[MMEnumCondition("ChannelMode", (int)MMChannelModes.Int)]
		public int Channel = 0;
		/// the MMChannel definition asset to use to listen for events. The feedbacks targeting this shaker will have to reference that same MMChannel definition to receive events - to create a MMChannel,
		/// right click anywhere in your project (usually in a Data folder) and go MoreMountains > MMChannel, then name it with some unique name
		[Tooltip("用于监听事件的 MMChannel 定义资源。以这个震动器为目标的反馈必须引用相同的 MMChannel 定义才能接收事件。" +
                 "要创建一个 MMChannel，在项目中的任意位置（通常在数据文件夹中）右键单击，然后选择 “MoreMountains”>“MMChannel”，接着给它起一个独特的名称。")]
		[MMEnumCondition("ChannelMode", (int)MMChannelModes.MMChannel)]
		public MMChannel MMChannelDefinition = null;
		
		[MMInspectorGroup("Spring Settings", true, 18)]
		public MMSpringFloat FloatSpring;
		
		[MMInspectorGroup("Randomness", true, 12, true)]
		/// the min (x) and max (y) values between which a random target value will be picked when calling MoveToRandom
		[Tooltip("当调用“移动至随机值（MoveToRandom）”时，将在最小值（x）和最大值（y）之间选取一个随机目标值 。 ")]
		[MMVector("Min", "Max")]
		public Vector2 MoveToRandomValue = new Vector2(-2f, 2f);
		/// the min (x) and max (y) values between which a random bump value will be picked when calling BumpRandom
		[Tooltip("当调用“随机碰撞（BumpRandom）”时，将在最小值（x）和最大值（y）之间选取的随机碰撞值的取值范围。  ")]
		[MMVector("Min", "Max")]
		public Vector2 BumpAmountRandomValue = new Vector2(20f, 100f);
		
		[MMInspectorGroup("Test", true, 20, true)]
		/// the value to move this spring to when interacting with any of the MoveTo debug buttons in its inspector
		[Tooltip("当在其检查器中与任何“移动至（MoveTo）”调试按钮进行交互时，要将此弹簧移动到的值。 ")]
		public float TestMoveToValue = 2f;
		[MMInspectorButtonBar(new string[] { "MoveTo", "MoveToAdditive", "MoveToSubtractive", "MoveToRandom", "MoveToInstant" }, 
			new string[] { "TestMoveTo", "TestMoveToAdditive", "TestMoveToSubtractive", "TestMoveToRandom", "TestMoveToInstant" }, 
			new bool[] { true, true, true, true, true },
		new string[] { "main-call-to-action", "", "", "", "" })]
		public bool MoveToToolbar;
		
		/// the amount by which to bump this spring when interacting with the Bump debug button in its inspector
		[Tooltip("当在其检查器中与“碰撞（Bump）”调试按钮进行交互时，使该弹簧产生碰撞效果的碰撞量。 ")]
		public float TestBumpAmount = 75f;
		[MMInspectorButtonBar(new string[] { "Bump", "BumpRandom" }, 
			new string[] { "TestBump", "TestBumpRandom" }, 
			new bool[] { true, true },
			new string[] { "main-call-to-action", "" })]
		public bool BumpToToolbar;
		
		[MMInspectorButtonBar(new string[] { "Stop", "Finish", "RestoreInitialValue", "ResetInitialValue" }, 
			new string[] { "Stop", "Finish", "RestoreInitialValue", "ResetInitialValue" }, 
			new bool[] { true, true, true, true },
			new string[] { "", "", "", "" })]
		public bool OtherControlsToToolbar;
		
		public override bool LowVelocity => Mathf.Abs(FloatSpring.Velocity) < _velocityLowThreshold;
		public float DeltaTime => (TimeScaleMode == TimeScaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime;

		public virtual float TargetFloat { get; set; }
		
		#region PUBLIC_API
		
		public virtual void MoveTo(float newValue)
		{
			Activate();
			FloatSpring.MoveTo(newValue);
		}
		
		public virtual void MoveToAdditive(float newValue)
		{
			Activate();
			FloatSpring.MoveToAdditive(newValue);
		}
		
		public virtual void MoveToSubtractive(float newValue)
		{
			Activate();
			FloatSpring.MoveToSubtractive(newValue);
		}

		public virtual void MoveToRandom()
		{
			Activate();
			FloatSpring.MoveToRandom(MoveToRandomValue.x, MoveToRandomValue.y);
		}

		public virtual void MoveToInstant(float newValue)
		{
			Activate();
			FloatSpring.MoveToInstant(newValue);
		}

		public virtual void MoveToRandom(float min, float max)
		{
			Activate();
			FloatSpring.MoveToRandom(min, max);
		}

		public virtual void Bump(float bumpAmount)
		{
			Activate();
			FloatSpring.Bump(bumpAmount);
		}

		public virtual void BumpRandom()
		{
			Activate();
			FloatSpring.BumpRandom(BumpAmountRandomValue.x, BumpAmountRandomValue.y);
		}

		public virtual void BumpRandom(float min, float max)
		{
			Activate();
			FloatSpring.BumpRandom(min, max);
		}
		
		public override void Stop()
		{
			base.Stop();
			this.enabled = false;
			GrabCurrentValue();
			FloatSpring.Stop();
		}
		
		public override void RestoreInitialValue()
		{
			FloatSpring.RestoreInitialValue();
			ApplyValue(FloatSpring.CurrentValue);
		}
		
		public override void ResetInitialValue()
		{
			FloatSpring.SetCurrentValueAsInitialValue();
		}
		
		protected override void UpdateSpringValue()
		{
			FloatSpring.UpdateSpringValue(DeltaTime);
			ApplyValue(FloatSpring.CurrentValue);
		}
		
		public override void Finish()
		{
			FloatSpring.Finish();
			ApplyValue(FloatSpring.CurrentValue);
		}
		
		#endregion

		#region INTERNAL
		
		protected override void Initialization()
		{
			base.Initialization();
			GrabCurrentValue();
			FloatSpring.SetInitialValue(FloatSpring.CurrentValue);
			FloatSpring.TargetValue = FloatSpring.CurrentValue;
		}

		protected virtual void ApplyValue(float newValue)
		{
			TargetFloat = newValue;
		}
		
		protected override void GrabCurrentValue()
		{
			base.GrabCurrentValue();
			FloatSpring.CurrentValue = TargetFloat;
		}

		#endregion

		#region EVENTS
		
		public void OnMMEvent(MMSpringFloatEvent springEvent)
		{
			bool eventMatch = springEvent.ChannelData != null && MMChannel.Match(springEvent.ChannelData, ChannelMode, Channel, MMChannelDefinition);
			bool targetMatch = springEvent.TargetSpring != null && springEvent.TargetSpring.Equals(this);
			if (!eventMatch && !targetMatch)
			{
				return;
			}
			
			if (springEvent.OverrideDamping)
			{
				FloatSpring.Damping = springEvent.NewDamping;
			}
			if (springEvent.OverrideFrequency)
			{
				FloatSpring.Frequency = springEvent.NewFrequency;
			}

			switch (springEvent.Command)
			{
				case SpringCommands.MoveTo:
					MoveTo(springEvent.MoveToValue);
					break;
				case SpringCommands.MoveToAdditive:
					MoveToAdditive(springEvent.MoveToValue);
					break;
				case SpringCommands.MoveToSubtractive:
					MoveToSubtractive(springEvent.MoveToValue);
					break;
				case SpringCommands.MoveToRandom:
					MoveToRandom(springEvent.MoveToRandomValue.x, springEvent.MoveToRandomValue.y);
					break;
				case SpringCommands.MoveToInstant:
					MoveToInstant(springEvent.MoveToValue);
					break;
				case SpringCommands.Bump:
					Bump(springEvent.BumpAmount);
					break;
				case SpringCommands.BumpRandom:
					BumpRandom(springEvent.BumpAmountRandomValue.x, springEvent.BumpAmountRandomValue.y);
					break;
				case SpringCommands.Stop:
					Stop();
					break;
				case SpringCommands.Finish:
					Finish();
					break;
				case SpringCommands.RestoreInitialValue:
					RestoreInitialValue();
					break;
				case SpringCommands.ResetInitialValue:
					ResetInitialValue();
					break;
			}
		}
		
		protected override void Awake()
		{
			if (Target == null)
			{
				Target = GetComponent<T>();
			}
			base.Awake();
			this.MMEventStartListening<MMSpringFloatEvent>();
		}

		protected void OnDestroy()
		{
			this.MMEventStopListening<MMSpringFloatEvent>();
		}

		#endregion

		#region TEST_METHODS

		protected override void TestMoveTo()
		{
			MoveTo(TestMoveToValue);
		}
		
		protected override void TestMoveToAdditive()
		{
			MoveToAdditive(TestMoveToValue);
		}
		
		protected override void TestMoveToSubtractive()
		{
			MoveToSubtractive(TestMoveToValue);
		}
		
		protected override void TestMoveToRandom()
		{
			MoveToRandom();
		}

		protected override void TestMoveToInstant()
		{
			MoveToInstant(TestMoveToValue);
		}

		protected override void TestBump()
		{
			Bump(TestBumpAmount);
		}
		
		protected override void TestBumpRandom()
		{
			BumpRandom();
		}

		#endregion
	}
}
