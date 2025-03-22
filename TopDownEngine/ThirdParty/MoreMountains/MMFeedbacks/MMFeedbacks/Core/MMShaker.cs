using System;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	public class MMShaker : MMMonoBehaviour
	{
		[MMInspectorGroup("Shaker Settings", true, 3)]
		/// whether to listen on a channel defined by an int or by a MMChannel scriptable object. Ints are simple to setup but can get messy and make it harder to remember what int corresponds to what.
		/// MMChannel scriptable objects require you to create them in advance, but come with a readable name and are more scalable
		[Tooltip("是通过一个整数来定义要监听的通道，还是通过一个可编写脚本的 MMChannel 对象来定义。使用整数设置起来比较简单，但可能会变得杂乱无章，而且更难记住哪个整数对应着什么内容。  " +
                 "MMChannel可编写脚本对象要求你提前创建它们，但这些对象带有易于识别的名称，并且具有更强的可扩展性。 ")]
		public MMChannelModes ChannelMode = MMChannelModes.Int;
		/// the channel to listen to - has to match the one on the feedback
		[Tooltip("要监听的通道 —— 必须与反馈中的通道相匹配")]
		[MMEnumCondition("ChannelMode", (int)MMChannelModes.Int)]
		public int Channel = 0;
		/// the MMChannel definition asset to use to listen for events. The feedbacks targeting this shaker will have to reference that same MMChannel definition to receive events - to create a MMChannel,
		/// right click anywhere in your project (usually in a Data folder) and go MoreMountains > MMChannel, then name it with some unique name
		[Tooltip("用于监听事件的 MMChannel 定义资源。针对此震动器的反馈必须引用相同的 MMChannel 定义才能接收事件 —— 为了创建一个 MMChannel。 " +
                 "在你的项目中的任意位置（通常是在一个数据文件夹内）点击鼠标右键，然后选择“MoreMountains > MMChannel”，接着用某个唯一的名称为它命名。 ")]
		[MMEnumCondition("ChannelMode", (int)MMChannelModes.MMChannel)]
		public MMChannel MMChannelDefinition = null;
		/// the duration of the shake, in seconds
		[Tooltip("震动的持续时间，以秒为单位。")]
		public float ShakeDuration = 0.2f;
		/// if this is true this shaker will play on awake
		[Tooltip("如果这是真的，那么这个震动器将在（游戏对象）唤醒时启动播放。 ")]
		public bool PlayOnAwake = false;
		/// if this is true, the shaker will shake permanently as long as its game object is active
		[Tooltip("如果这一点为真，那么只要该震动器的游戏对象处于激活状态，它就会持续不断地进行震动。 ")]
		public bool PermanentShake = false;
		/// if this is true, a new shake can happen while shaking
		[Tooltip("如果这是真的，那么在震动过程中可以触发新的震动。")]
		public bool Interruptible = true;
		/// if this is true, this shaker will always reset target values, regardless of how it was called
		[Tooltip("如果这是真的，那么无论该震动器是如何被调用的，它都会始终重置目标值。")]
		public bool AlwaysResetTargetValuesAfterShake = false;
		/// if this is true, this shaker will ignore any value passed in an event that triggered it, and will instead use the values set on its inspector
		[Tooltip("如果此条件为真，那么这个震动器将忽略触发它的事件中传入的任何值，而是使用在其检视器（Inspector）中设置的值。")]
		public bool OnlyUseShakerValues = false;
		/// a cooldown, in seconds, after a shake, during which no other shake can start
		[Tooltip("一次震动之后的冷却时间，以秒为单位，在此期间内不会启动其他任何震动。 ")]
		public float CooldownBetweenShakes = 0f;
		/// whether or not this shaker is shaking right now
		[Tooltip("这个震动器当前是否正在震动。")]
		[MMFReadOnly]
		public bool Shaking = false;
        
		[HideInInspector] 
		public bool ForwardDirection = true;

		[HideInInspector] 
		public TimescaleModes TimescaleMode = TimescaleModes.Scaled;

		public virtual float GetTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.time : Time.unscaledTime; }
		public virtual float GetDeltaTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; }
		public virtual MMChannelData ChannelData => new MMChannelData(ChannelMode, Channel, MMChannelDefinition);
        
		public virtual bool ListeningToEvents => _listeningToEvents;

		[HideInInspector]
		internal bool _listeningToEvents = false;
		protected float _shakeStartedTimestamp = -Single.MaxValue;
		protected float _remappedTimeSinceStart;
		protected bool _resetShakerValuesAfterShake;
		protected bool _resetTargetValuesAfterShake;
		protected float _journey;

        /// <summary>
        /// 在（游戏对象）唤醒时，我们获取其音量值和配置文件（属性配置）。
        /// </summary>
        protected virtual void Awake()
		{
			Initialization();
            // 以防在（游戏对象）唤醒（Awake）之前，有其他对象触发了 StartListening 方法。
            if (!_listeningToEvents)
			{
				StartListening();
			}
			Shaking = PlayOnAwake;
			this.enabled = PlayOnAwake;
		}

        /// <summary>
        /// 重写此方法以初始化你的震动器。
        /// </summary>
        protected virtual void Initialization()
		{
		}

        /// <summary>
        /// 如果你需要强制进行一次新的初始化，可以从外部调用此方法。
        /// </summary>
        public virtual void ForceInitialization()
		{
			Initialization();
		}

        /// <summary>
        /// 开始对数值进行震动处理
        /// </summary>
        public virtual void StartShaking()
		{
			_journey = ForwardDirection ? 0f : ShakeDuration;

			if (GetTime() - _shakeStartedTimestamp < CooldownBetweenShakes)
			{
				return;
			}
            
			if (Shaking)
			{
				return;
			}
			else
			{
				this.enabled = true;
				_shakeStartedTimestamp = GetTime();
				Shaking = true;
				GrabInitialValues();
				ShakeStarts();
			}
		}

        /// <summary>
        /// 描述了震动开始时会发生什么情况。
        /// </summary>
        protected virtual void ShakeStarts()
		{

		}

        /// <summary>
        /// 这是一个用于收集初始值的方法。
        /// </summary>
        protected virtual void GrabInitialValues()
		{

		}

        /// <summary>
        /// 在每帧更新（Update）时，如果有需要，我们会对数值进行震动处理；若震动已经结束，则将数值重置。
        /// </summary>
        protected virtual void Update()
		{
			if (Shaking || PermanentShake)
			{
				Shake();
				_journey += ForwardDirection ? GetDeltaTime() : -GetDeltaTime();
			}

			if (Shaking && !PermanentShake && ((_journey < 0) || (_journey > ShakeDuration)))
			{
				Shaking = false;
				ShakeComplete();
			}

			if (PermanentShake)
			{
				if (_journey < 0)
				{
					_journey = ShakeDuration;
				}

				if (_journey > ShakeDuration)
				{
					_journey = 0;
				}
			}
		}

        /// <summary>
        /// 重写此方法以实现随时间推进的震动效果。
        /// </summary>
        protected virtual void Shake()
		{

		}

        /// <summary>
        /// 这是一个用于让一个浮点数（float 类型变量）随时间沿着一条曲线进行 “震动” 变化的方法。
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="remapMin"></param>
        /// <param name="remapMax"></param>
        /// <param name="relativeIntensity"></param>
        /// <param name="initialValue"></param>
        /// <returns></returns>
        protected virtual float ShakeFloat(AnimationCurve curve, float remapMin, float remapMax, bool relativeIntensity, float initialValue)
		{
			float newValue = 0f;
            
			float remappedTime = MMFeedbacksHelpers.Remap(_journey, 0f, ShakeDuration, 0f, 1f);
            
			float curveValue = curve.Evaluate(remappedTime);
			newValue = MMFeedbacksHelpers.Remap(curveValue, 0f, 1f, remapMin, remapMax);
			if (relativeIntensity)
			{
				newValue += initialValue;
			}
			return newValue;
		}

		protected virtual Color ShakeGradient(Gradient gradient)
		{
			float remappedTime = MMFeedbacksHelpers.Remap(_journey, 0f, ShakeDuration, 0f, 1f);
			return gradient.Evaluate(remappedTime);
		}

        /// <summary>
        /// 重置目标上的值
        /// </summary>
        protected virtual void ResetTargetValues()
		{

		}

        /// <summary>
        /// 重置震动器上的值
        /// </summary>
        protected virtual void ResetShakerValues()
		{

		}

        /// <summary>
        /// 描述当震动完成时会发生什么情况。
        /// </summary>
        protected virtual void ShakeComplete()
		{
			_journey = ForwardDirection ? ShakeDuration : 0f;
			Shake();
			
			if (_resetTargetValuesAfterShake || AlwaysResetTargetValuesAfterShake)
			{
				ResetTargetValues();
			}   
			if (_resetShakerValuesAfterShake)
			{
				ResetShakerValues();
			}            
			this.enabled = false;
		}

        /// <summary>
        /// 当对象被启用时，如果有需要，我们会开始进行震动操作。
        /// </summary>
        protected virtual void OnEnable()
		{
			StartShaking();
		}

        /// <summary>
        /// 当（游戏对象）被销毁时，我们停止监听事件
        /// </summary>
        protected virtual void OnDestroy()
		{
			StopListening();
		}

        /// <summary>
        /// 当（游戏对象）被禁用时，如果震动操作正在进行，我们就会完成当前的震动。
        /// </summary>
        protected virtual void OnDisable()
		{
			if (Shaking)
			{
				ShakeComplete();
			}
		}

        /// <summary>
        /// 启动这个震动器。
        /// </summary>
        public virtual void Play()
		{
			if (GetTime() - _shakeStartedTimestamp < CooldownBetweenShakes)
			{
				return;
			}
			this.enabled = true;
		}

        /// <summary>
        /// 停止这个震动器。
        /// </summary>
        public virtual void Stop()
		{
			Shaking = false;
			ShakeComplete();
		}

        /// <summary>
        /// 开始监听事件
        /// </summary>
        public virtual void StartListening()
		{
			_listeningToEvents = true;
		}

        /// <summary>
        /// 停止监听事件
        /// </summary>
        public virtual void StopListening()
		{
			_listeningToEvents = false;
		}

        /// <summary>
        /// 如果这个震动器应该监听事件，则返回 true；否则返回 false。
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        protected virtual bool CheckEventAllowed(MMChannelData channelData, bool useRange = false, float range = 0f, Vector3 eventOriginPosition = default(Vector3))
		{
			if (!MMChannel.Match(channelData, ChannelMode, Channel, MMChannelDefinition))
			{
				return false;
			}
			if (!this.gameObject.activeInHierarchy)
			{
				return false;
			}
			else
			{
				if (useRange)
				{
					if (Vector3.Distance(this.transform.position, eventOriginPosition) > range)
					{
						return false;
					}
				}

				return true;
			}
		}
		
		public virtual float ComputeRangeIntensity(bool useRange, float rangeDistance, bool useRangeFalloff, AnimationCurve rangeFalloff, Vector2 remapRangeFalloff, Vector3 rangePosition)
		{
			if (!useRange)
			{
				return 1f;
			}

			float distanceToCenter = Vector3.Distance(rangePosition, this.transform.position);

			if (distanceToCenter > rangeDistance)
			{
				return 0f;
			}

			if (!useRangeFalloff)
			{
				return 1f;
			}

			float normalizedDistance = MMMaths.Remap(distanceToCenter, 0f, rangeDistance, 0f, 1f);
			float curveValue = rangeFalloff.Evaluate(normalizedDistance);
			float newIntensity = MMMaths.Remap(curveValue, 0f, 1f, remapRangeFalloff.x, remapRangeFalloff.y);
			return newIntensity;
		}
	}
}