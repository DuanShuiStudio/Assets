using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using System.Linq;
using MoreMountains.Tools;
using UnityEditor.Experimental;
using UnityEngine.Events;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 一组MMFeedback，旨在一起播放
    /// 这个类提供了一个自定义检视器，用于添加和定制反馈，以及一些公共方法来触发、停止等操作
    /// 您可以单独使用它，也可以从另一个类中绑定并从中触发它
    /// </summary>
    [AddComponentMenu("")]
	public class MMFeedbacks : MonoBehaviour
	{
        /// MMFeedbacks可能被播放的方向
        public enum Directions { TopToBottom, BottomToTop }
        /// 可能的安全模式（将执行检查以确保没有序列化错误损坏它们）
        /// - nope : 无安全保护
        /// - editor only : 对启用执行检查
        /// - runtime only : 对唤醒执行检查
        /// - full : 执行编辑器和运行时检查，推荐设置
        public enum SafeModes { Nope, EditorOnly, RuntimeOnly, Full }

        /// 要触发的MMFeedback列表
        public List<MMFeedback> Feedbacks = new List<MMFeedback>();

        /// 可能的初始化模式。如果您使用脚本，将需要通过调用Initialization方法并传递一个所有者来手动初始化。
        /// 否则，您可以让此组件在唤醒或开始时自行初始化，在这种情况下，所有者将是MMFeedbacks本身。
        public enum InitializationModes { Script, Awake, Start }
		/// the chosen initialization mode
		[Tooltip("选择的初始化模式。如果您使用脚本，将需要通过调用来手动初始化 " +
                 "初始化方法并传递一个所有者。否则，您可以让此组件在Awake或Start时自行初始化，在这种情况下，所有者将是MMFeedbacks本身 " +
                 "在Awake或Start时自行初始化，在这种情况下，所有者将是MMFeedbacks本身")]
		public InitializationModes InitializationMode = InitializationModes.Start;
		/// if you set this to true, the system will make changes to ensure that initialization always happens before play
		[Tooltip("如果将此设置为true，系统将进行更改以确保初始化总是在播放之前发生")]
		public bool AutoInitialization = true;
		/// the selected safe mode
		[Tooltip("所选的安全模式")]
		public SafeModes SafeMode = SafeModes.Full;
		/// the selected direction
		[Tooltip("这些反馈应该播放的所选方向")]
		public Directions Direction = Directions.TopToBottom;
		/// whether or not this MMFeedbacks should invert its direction when all feedbacks have played
		[Tooltip("当所有反馈都已播放时，此MMFeedbacks是否应反转其方向")]
		public bool AutoChangeDirectionOnEnd = false;
		/// whether or not to play this feedbacks automatically on Start
		[Tooltip("是否在开始时自动播放此反馈")]
		public bool AutoPlayOnStart = false;
		/// whether or not to play this feedbacks automatically on Enable
		[Tooltip("是否在启用时自动播放此反馈")]
		public bool AutoPlayOnEnable = false;

		/// if this is true, all feedbacks within that player will work on the specified ForcedTimescaleMode, regardless of their individual settings 
		[Tooltip("如果此为true，则该播放器内的所有反馈将按照指定的ForcedTimescaleMode工作，无论其个人设置如何")] 
		public bool ForceTimescaleMode = false;
		/// the time scale mode all feedbacks on this player should work on, if ForceTimescaleMode is true
		[Tooltip("如果ForceTimescaleMode为true，此播放器上的所有反馈应遵循的时间缩放模式")] 
		[MMFCondition("ForceTimescaleMode", true)]
		public TimescaleModes ForcedTimescaleMode = TimescaleModes.Unscaled;
		/// a time multiplier that will be applied to all feedback durations (initial delay, duration, delay between repeats...)
		[Tooltip("将应用于所有反馈持续时间（初始延迟、持续时间、重复之间的延迟等）的时间乘数")]
		public float DurationMultiplier = 1f;
		/// a multiplier to apply to all timescale operations (1: normal, less than 1: slower operations, higher than 1: faster operations)
		[Tooltip("应用于所有时间缩放操作的乘数（1：正常，小于1：较慢的操作，高于1：较快的操作）")]
		public float TimescaleMultiplier = 1f;
		/// if this is true, will expose a RandomDurationMultiplier. The final duration of each feedback will be : their base duration * DurationMultiplier * a random value between RandomDurationMultiplier.x and RandomDurationMultiplier.y
		[Tooltip("如果此为true，将公开一个RandomDurationMultiplier。每个反馈的最终持续时间将是：它们的基本持续时间 * DurationMultiplier * RandomDurationMultiplier.x和RandomDurationMultiplier.y之间的随机值")]
		public bool RandomizeDuration = false;
		/// if RandomizeDuration is true, the min (x) and max (y) values for the random duration multiplier
		[Tooltip("如果RandomizeDuration为true，随机持续时间乘数的最小（x）和最大（y）值")]
		[MMCondition("RandomizeDuration", true)]
		public Vector2 RandomDurationMultiplier = new Vector2(0.5f, 1.5f);
		/// if this is true, more editor-only, detailed info will be displayed per feedback in the duration slot
		[Tooltip("如果此为true，持续时间槽中每个反馈将显示更多仅限编辑器的详细信息")]
		public bool DisplayFullDurationDetails = false;
		/// the timescale at which the player itself will operate. This notably impacts sequencing and pauses duration evaluation.
		[Tooltip("播放器本身将运行的时间缩放。这显著影响了序列和暂停持续时间的评估")]
		public TimescaleModes PlayerTimescaleMode = TimescaleModes.Unscaled;

		/// if this is true, this feedback will only play if its distance to RangeCenter is lower or equal to RangeDistance
		[Tooltip("如果此为true，则仅当该反馈到RangeCenter的距离低于或等于RangeDistance时才会播放")]
		public bool OnlyPlayIfWithinRange = false;
		/// when in OnlyPlayIfWithinRange mode, the transform to consider as the center of the range
		[Tooltip("在OnlyPlayIfWithinRange模式下，要视为范围中心的变换")]
		public Transform RangeCenter;
		/// when in OnlyPlayIfWithinRange mode, the distance to the center within which the feedback will play
		[Tooltip("在OnlyPlayIfWithinRange模式下，反馈将播放的到中心的距离")]
		public float RangeDistance = 5f;
		/// when in OnlyPlayIfWithinRange mode, whether or not to modify the intensity of feedbacks based on the RangeFallOff curve  
		[Tooltip("在OnlyPlayIfWithinRange模式下，是否根据RangeFallOff曲线修改反馈的强度")]
		public bool UseRangeFalloff = false;
		/// the animation curve to use to define falloff (on the x 0 represents the range center, 1 represents the max distance to it)
		[Tooltip("用于定义衰减的动画曲线（在x轴上，0表示范围中心，1表示到它的最大距离）")]
		[MMFCondition("UseRangeFalloff", true)]
		public AnimationCurve RangeFalloff = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));
		/// the values to remap the falloff curve's y axis' 0 and 1
		[Tooltip("用于重新映射衰减曲线y轴的0和1的值")]
		[MMFVector("Zero","One")]
		public Vector2 RemapRangeFalloff = new Vector2(0f, 1f);
		/// whether or not to ignore MMSetFeedbackRangeCenterEvent, used to set the RangeCenter from anywhere
		[Tooltip("是否忽略MMSetFeedbackRangeCenterEvent，该事件用于从任何地方设置RangeCenter")]
		public bool IgnoreRangeEvents = false;

		/// a duration, in seconds, during which triggering a new play of this MMFeedbacks after it's been played once will be impossible
		[Tooltip("在播放一次后，多长时间内禁止此MMFeedbacks重新播放（以秒为单位）")]
		public float CooldownDuration = 0f;
		/// a duration, in seconds, to delay the start of this MMFeedbacks' contents play
		[Tooltip("延迟此MMFeedbacks内容播放开始的时间（以秒为单位）")]
		public float InitialDelay = 0f;
		/// whether this player can be played or not, useful to temporarily prevent play from another class, for example
		[Tooltip("此播放器是否可以播放，用于从另一个类中临时阻止播放")]
		public bool CanPlay = true;
		/// if this is true, you'll be able to trigger a new Play while this feedback is already playing, otherwise you won't be able to
		[Tooltip("如果此为true，您将能够在该反馈正在播放时触发新的播放，否则您将无法这样做")]
		public bool CanPlayWhileAlreadyPlaying = true;
		/// the chance of this sequence happening (in percent : 100 : happens all the time, 0 : never happens, 50 : happens once every two calls, etc)
		[Tooltip("此序列发生的机会（以百分比表示：100：总是发生，0：从不发生，50：每两次调用发生一次，等等）")]
		[Range(0,100)]
		public float ChanceToPlay = 100f;
        
		/// the intensity at which to play this feedback. That value will be used by most feedbacks to tune their amplitude. 1 is normal, 0.5 is half power, 0 is no effect.
		/// Note that what this value controls depends from feedback to feedback, don't hesitate to check the code to see what it does exactly.  
		[Tooltip("播放此反馈的强度。该值将被大多数反馈用来调整它们的振幅。1是正常，0.5是半功率，0是没有效果" +
                 "请注意，此值控制的内容因反馈而异，请检查代码以确切了解其作用")]
		public float FeedbacksIntensity = 1f;

		/// a number of UnityEvents that can be triggered at the various stages of this MMFeedbacks 
		[Tooltip("在MMFeedbacks的各个阶段可以触发的UnityEvents的数量")] 
		public MMFeedbacksEvents Events;
        
		/// a global switch used to turn all feedbacks on or off globally
		[Tooltip("一个全局开关，用于全局打开或关闭所有反馈")]
		public static bool GlobalMMFeedbacksActive = true;
        
		[HideInInspector]
        /// 此MMFeedbacks是否处于调试模式
        public bool DebugActive = false;
        /// 此MMFeedbacks当前是否正在播放——意味着它尚未被停止
        /// 如果您不停止您的MMFeedbacks，它当然会保持为true
        public bool IsPlaying { get; protected set; }
        /// 如果此MMFeedbacks正在播放，则自其开始播放以来的时间
        public virtual float ElapsedTime => IsPlaying ? GetTime() - _lastStartAt : 0f;
        /// 此MMFeedbacks已被播放的次数
        public int TimesPlayed { get; protected set; }
        /// 此MMFeedbacks的序列执行是否正在被阻止并等待Resume()调用
        public bool InScriptDrivenPause { get; set; }
        /// 如果此MMFeedbacks包含至少一个循环，则为true。
        public bool ContainsLoop { get; set; }
        /// 如果此反馈下次播放时应更改播放方向，则为true
        public bool ShouldRevertOnNextPlay { get; set; }
        /// 如果此播放器正在强制使用未缩放模式，则为true
        public bool ForcingUnscaledTimescaleMode { get { return (ForceTimescaleMode && ForcedTimescaleMode == TimescaleModes.Unscaled);  } }
        /// 此MMFeedbacks中所有活动反馈的总持续时间（以秒为单位）
        public virtual float TotalDuration
		{
			get
			{
				float total = 0f;
				foreach (MMFeedback feedback in Feedbacks)
				{
					if ((feedback != null) && (feedback.Active))
					{
						if (total < feedback.TotalDuration)
						{
							total = feedback.TotalDuration;    
						}
					}
				}
				return ComputedInitialDelay + total;
			}
		}
        
		public virtual float GetTime() { return (PlayerTimescaleMode == TimescaleModes.Scaled) ? Time.time : Time.unscaledTime; }
		public virtual float GetDeltaTime() { return (PlayerTimescaleMode == TimescaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; }
		public virtual float ComputedInitialDelay => ApplyTimeMultiplier(InitialDelay);
		
		protected float _startTime = 0f;
		protected float _holdingMax = 0f;
		protected float _lastStartAt = -float.MaxValue;
		protected int _lastStartFrame = -1;
		protected bool _pauseFound = false;
		protected float _totalDuration = 0f;
		protected bool _shouldStop = false;
		protected const float _smallValue = 0.001f;
		protected float _randomDurationMultiplier = 1f;
		protected float _lastOnEnableFrame = -1;

		#region INITIALIZATION

		/// <summary>
		/// On Awake we initialize our feedbacks if we're in auto mode
		/// </summary>
		protected virtual void Awake()
		{
			// if our MMFeedbacks is in AutoPlayOnEnable mode, we add a little helper to it that will re-enable it if needed if the parent game object gets turned off and on again
			if (AutoPlayOnEnable)
			{
				MMFeedbacksEnabler enabler = GetComponent<MMFeedbacksEnabler>(); 
				if (enabler == null)
				{
					enabler = this.gameObject.AddComponent<MMFeedbacksEnabler>();
				}
				enabler.TargetMMFeedbacks = this;
			}
            
			if ((InitializationMode == InitializationModes.Awake) && (Application.isPlaying))
			{
				Initialization(this.gameObject);
			}
			CheckForLoops();
		}

		/// <summary>
		/// On Start we initialize our feedbacks if we're in auto mode
		/// </summary>
		protected virtual void Start()
		{
			if ((InitializationMode == InitializationModes.Start) && (Application.isPlaying))
			{
				Initialization(this.gameObject);
			}
			if (AutoPlayOnStart && Application.isPlaying)
			{
				PlayFeedbacks();
			}
			CheckForLoops();
		}

		/// <summary>
		/// On Enable we initialize our feedbacks if we're in auto mode
		/// </summary>
		protected virtual void OnEnable()
		{
			if (AutoPlayOnEnable && Application.isPlaying)
			{
				PlayFeedbacks();
			}
		}

		/// <summary>
		/// Initializes the MMFeedbacks, setting this MMFeedbacks as the owner
		/// </summary>
		public virtual void Initialization(bool forceInitIfPlaying = false)
		{
			Initialization(this.gameObject);
		}

		/// <summary>
		/// A public method to initialize the feedback, specifying an owner that will be used as the reference for position and hierarchy by feedbacks
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="feedbacksOwner"></param>
		public virtual void Initialization(GameObject owner)
		{
			if ((SafeMode == MMFeedbacks.SafeModes.RuntimeOnly) || (SafeMode == MMFeedbacks.SafeModes.Full))
			{
				AutoRepair();
			}

			IsPlaying = false;
			TimesPlayed = 0;
			_lastStartAt = -float.MaxValue;

			for (int i = 0; i < Feedbacks.Count; i++)
			{
				if (Feedbacks[i] != null)
				{
					Feedbacks[i].Initialization(owner);
				}                
			}
		}

		#endregion

		#region PLAY
        
		/// <summary>
		/// Plays all feedbacks using the MMFeedbacks' position as reference, and no attenuation
		/// </summary>
		public virtual void PlayFeedbacks()
		{
			PlayFeedbacksInternal(this.transform.position, FeedbacksIntensity);
		}
        
		/// <summary>
		/// Plays all feedbacks and awaits until completion
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		/// <param name="forceRevert"></param>
		public virtual async System.Threading.Tasks.Task PlayFeedbacksTask(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
		{
			PlayFeedbacks(position, feedbacksIntensity, forceRevert);
			while (IsPlaying)
			{
				await System.Threading.Tasks.Task.Yield();
			}
		}
        
		/// <summary>
		/// Plays all feedbacks and awaits until completion
		/// </summary>
		public virtual async System.Threading.Tasks.Task PlayFeedbacksTask()
		{
			PlayFeedbacks();
			while (IsPlaying)
			{
				await System.Threading.Tasks.Task.Yield();
			}
		}
        
		/// <summary>
		/// Plays all feedbacks, specifying a position and intensity. The position may be used by each Feedback and taken into account to spark a particle or play a sound for example.
		/// The feedbacks intensity is a factor that can be used by each Feedback to lower its intensity, usually you'll want to define that attenuation based on time or distance (using a lower 
		/// intensity value for feedbacks happening further away from the Player).
		/// Additionally you can force the feedback to play in reverse, ignoring its current condition
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksOwner"></param>
		/// <param name="feedbacksIntensity"></param>
		public virtual void PlayFeedbacks(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
		{
			PlayFeedbacksInternal(position, feedbacksIntensity, forceRevert);
		}

		/// <summary>
		/// Plays all feedbacks using the MMFeedbacks' position as reference, and no attenuation, and in reverse (from bottom to top)
		/// </summary>
		public virtual void PlayFeedbacksInReverse()
		{
			PlayFeedbacksInternal(this.transform.position, FeedbacksIntensity, true);
		}

		/// <summary>
		/// Plays all feedbacks using the MMFeedbacks' position as reference, and no attenuation, and in reverse (from bottom to top)
		/// </summary>
		public virtual void PlayFeedbacksInReverse(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
		{
			PlayFeedbacksInternal(position, feedbacksIntensity, forceRevert);
		}

		/// <summary>
		/// Plays all feedbacks in the sequence, but only if this MMFeedbacks is playing in reverse order
		/// </summary>
		public virtual void PlayFeedbacksOnlyIfReversed()
		{
            
			if ( (Direction == Directions.BottomToTop && !ShouldRevertOnNextPlay)
			     || ((Direction == Directions.TopToBottom) && ShouldRevertOnNextPlay) )
			{
				PlayFeedbacks();
			}
		}
        
		/// <summary>
		/// Plays all feedbacks in the sequence, but only if this MMFeedbacks is playing in reverse order
		/// </summary>
		public virtual void PlayFeedbacksOnlyIfReversed(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
		{
            
			if ( (Direction == Directions.BottomToTop && !ShouldRevertOnNextPlay)
			     || ((Direction == Directions.TopToBottom) && ShouldRevertOnNextPlay) )
			{
				PlayFeedbacks(position, feedbacksIntensity, forceRevert);
			}
		}
        
		/// <summary>
		/// Plays all feedbacks in the sequence, but only if this MMFeedbacks is playing in normal order
		/// </summary>
		public virtual void PlayFeedbacksOnlyIfNormalDirection()
		{
			if (Direction == Directions.TopToBottom)
			{
				PlayFeedbacks();
			}
		}
        
		/// <summary>
		/// Plays all feedbacks in the sequence, but only if this MMFeedbacks is playing in normal order
		/// </summary>
		public virtual void PlayFeedbacksOnlyIfNormalDirection(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
		{
			if (Direction == Directions.TopToBottom)
			{
				PlayFeedbacks(position, feedbacksIntensity, forceRevert);
			}
		}

		/// <summary>
		/// A public coroutine you can call externally when you want to yield in a coroutine of yours until the MMFeedbacks has stopped playing
		/// typically : yield return myFeedback.PlayFeedbacksCoroutine(this.transform.position, 1.0f, false);
		/// </summary>
		/// <param name="position">The position at which the MMFeedbacks should play</param>
		/// <param name="feedbacksIntensity">The intensity of the feedback</param>
		/// <param name="forceRevert">Whether or not the MMFeedbacks should play in reverse or not</param>
		/// <returns></returns>
		public virtual IEnumerator PlayFeedbacksCoroutine(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
		{
			PlayFeedbacks(position, feedbacksIntensity, forceRevert);
			while (IsPlaying)
			{
				yield return null;    
			}
		}

		#endregion

		#region SEQUENCE

		/// <summary>
		/// An internal method used to play feedbacks, shouldn't be called externally
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected virtual void PlayFeedbacksInternal(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
		{
			if (!CanPlay)
			{
				return;
			}
			
			if (IsPlaying && !CanPlayWhileAlreadyPlaying)
			{
				return;
			}

			if (!EvaluateChance())
			{
				return;
			}

			// if we have a cooldown we prevent execution if needed
			if (CooldownDuration > 0f)
			{
				if (GetTime() - _lastStartAt < CooldownDuration)
				{
					return;
				}
			}

			// if all MMFeedbacks are disabled globally, we stop and don't play
			if (!GlobalMMFeedbacksActive)
			{
				return;
			}

			if (!this.gameObject.activeInHierarchy)
			{
				return;
			}
            
			if (ShouldRevertOnNextPlay)
			{
				Revert();
				ShouldRevertOnNextPlay = false;
			}

			if (forceRevert)
			{
				Direction = (Direction == Directions.BottomToTop) ? Directions.TopToBottom : Directions.BottomToTop;
			}
            
			ResetFeedbacks();
			this.enabled = true;
			TimesPlayed++;
			IsPlaying = true;
			_startTime = GetTime();
			_lastStartAt = _startTime;
			_totalDuration = TotalDuration;
			CheckForPauses();
            
			if (ComputedInitialDelay > 0f)
			{
				StartCoroutine(HandleInitialDelayCo(position, feedbacksIntensity, forceRevert));
			}
			else
			{
				PreparePlay(position, feedbacksIntensity, forceRevert);
			}
		}

		protected virtual void PreparePlay(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
		{
			Events.TriggerOnPlay(this);

			_holdingMax = 0f;
			CheckForPauses();
			
			if (!_pauseFound)
			{
				PlayAllFeedbacks(position, feedbacksIntensity, forceRevert);
			}
			else
			{
				// if at least one pause was found
				StartCoroutine(PausedFeedbacksCo(position, feedbacksIntensity));
			}
		}

		protected virtual void CheckForPauses()
		{
			_pauseFound = false;
			for (int i = 0; i < Feedbacks.Count; i++)
			{
				if (Feedbacks[i] != null)
				{
					if ((Feedbacks[i].Pause != null) && (Feedbacks[i].Active) && (Feedbacks[i].ShouldPlayInThisSequenceDirection))
					{
						_pauseFound = true;
					}
					if ((Feedbacks[i].HoldingPause == true) && (Feedbacks[i].Active) && (Feedbacks[i].ShouldPlayInThisSequenceDirection))
					{
						_pauseFound = true;
					}    
				}
			}
		}

		protected virtual void PlayAllFeedbacks(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
		{
			// if no pause was found, we just play all feedbacks at once
			for (int i = 0; i < Feedbacks.Count; i++)
			{
				if (FeedbackCanPlay(Feedbacks[i]))
				{
					Feedbacks[i].Play(position, feedbacksIntensity);
				}
			}
		}

		protected virtual IEnumerator HandleInitialDelayCo(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
		{
			IsPlaying = true;
			yield return MMFeedbacksCoroutine.WaitFor(ComputedInitialDelay);
			PreparePlay(position, feedbacksIntensity, forceRevert);
		}
        
		protected virtual void Update()
		{
			if (_shouldStop)
			{
				if (HasFeedbackStillPlaying())
				{
					return;
				}
				IsPlaying = false;
				Events.TriggerOnComplete(this);
				ApplyAutoRevert();
				this.enabled = false;
				_shouldStop = false;
			}
			if (IsPlaying)
			{
				if (!_pauseFound)
				{
					if (GetTime() - _startTime > _totalDuration)
					{
						_shouldStop = true;
					}
				}
			}
			else
			{
				this.enabled = false;
			}
		}

		/// <summary>
		/// Returns true if feedbacks are still playing
		/// </summary>
		/// <returns></returns>
		public virtual bool HasFeedbackStillPlaying()
		{
			int count = Feedbacks.Count;
			for (int i = 0; i < count; i++)
			{
				if ((Feedbacks[i] != null) && (Feedbacks[i].IsPlaying))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// A coroutine used to handle the sequence of feedbacks if pauses are involved
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		/// <returns></returns>
		protected virtual IEnumerator PausedFeedbacksCo(Vector3 position, float feedbacksIntensity)
		{
			yield return null;
		}

		#endregion

		#region STOP

		/// <summary>
		/// Stops all further feedbacks from playing, without stopping individual feedbacks 
		/// </summary>
		public virtual void StopFeedbacks()
		{
			StopFeedbacks(true);
		}

		/// <summary>
		/// Stops all feedbacks from playing, with an option to also stop individual feedbacks
		/// </summary>
		public virtual void StopFeedbacks(bool stopAllFeedbacks = true)
		{
			StopFeedbacks(this.transform.position, 1.0f, stopAllFeedbacks);
		}

		/// <summary>
		/// Stops all feedbacks from playing, specifying a position and intensity that can be used by the Feedbacks 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		public virtual void StopFeedbacks(Vector3 position, float feedbacksIntensity = 1.0f, bool stopAllFeedbacks = true)
		{
			if (stopAllFeedbacks)
			{
				for (int i = 0; i < Feedbacks.Count; i++)
				{
					if (Feedbacks[i] != null)
					{
						Feedbacks[i].Stop(position, feedbacksIntensity);	
					}
				}    
			}
			IsPlaying = false;
			StopAllCoroutines();
		}
        
		#endregion 

		#region CONTROLS

		/// <summary>
		/// Calls each feedback's Reset method if they've defined one. An example of that can be resetting the initial color of a flickering renderer.
		/// </summary>
		public virtual void ResetFeedbacks()
		{
			for (int i = 0; i < Feedbacks.Count; i++)
			{
				if ((Feedbacks[i] != null) && (Feedbacks[i].Active))
				{
					Feedbacks[i].ResetFeedback();    
				}
			}
			IsPlaying = false;
		}

		/// <summary>
		/// Changes the direction of this MMFeedbacks
		/// </summary>
		public virtual void Revert()
		{
			Events.TriggerOnRevert(this);
			Direction = (Direction == Directions.BottomToTop) ? Directions.TopToBottom : Directions.BottomToTop;
		}

		/// <summary>
		/// Use this method to authorize or prevent this player from being played
		/// </summary>
		/// <param name="newState"></param>
		public virtual void SetCanPlay(bool newState)
		{
			CanPlay = newState;
		}

		/// <summary>
		/// Pauses execution of a sequence, which can then be resumed by calling ResumeFeedbacks()
		/// </summary>
		public virtual void PauseFeedbacks()
		{
			Events.TriggerOnPause(this);
			InScriptDrivenPause = true;
		}

		/// <summary>
		/// Resumes execution of a sequence if a script driven pause is in progress
		/// </summary>
		public virtual void ResumeFeedbacks()
		{
			Events.TriggerOnResume(this);
			InScriptDrivenPause = false;
		}

		#endregion
        
		#region MODIFICATION
        
		public virtual MMFeedback AddFeedback(System.Type feedbackType, bool add = true)
		{
			MMFeedback newFeedback;
            
			#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				newFeedback = Undo.AddComponent(this.gameObject, feedbackType) as MMFeedback;
			}
			else
			{
				newFeedback = this.gameObject.AddComponent(feedbackType) as MMFeedback;
			}
			#else 
                newFeedback = this.gameObject.AddComponent(feedbackType) as MMFeedback;
			#endif
            
			newFeedback.hideFlags = HideFlags.HideInInspector;
			newFeedback.Label = FeedbackPathAttribute.GetFeedbackDefaultName(feedbackType);

			AutoRepair();
            
			return newFeedback;
		}
        
		public virtual void RemoveFeedback(int id)
		{
			#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				Undo.DestroyObjectImmediate(Feedbacks[id]);
			}
			else
			{
				DestroyImmediate(Feedbacks[id]);
			}
			#else
                DestroyImmediate(Feedbacks[id]);
			#endif
            
			Feedbacks.RemoveAt(id);
			AutoRepair();
		}
        
		#endregion MODIFICATION

		#region HELPERS

		/// <summary>
		/// Evaluates the chance of this feedback to play, and returns true if this feedback can play, false otherwise
		/// </summary>
		/// <returns></returns>
		protected virtual bool EvaluateChance()
		{
			if (ChanceToPlay == 0f)
			{
				return false;
			}
			if (ChanceToPlay != 100f)
			{
				// determine the odds
				float random = Random.Range(0f, 100f);
				if (random > ChanceToPlay)
				{
					return false;
				}
			}

			return true;
		}
        
		/// <summary>
		/// Checks whether or not this MMFeedbacks contains one or more looper feedbacks
		/// </summary>
		protected virtual void CheckForLoops()
		{
			ContainsLoop = false;
			for (int i = 0; i < Feedbacks.Count; i++)
			{
				if (Feedbacks[i] != null)
				{
					if (Feedbacks[i].LooperPause && Feedbacks[i].Active)
					{
						ContainsLoop = true;
						return;
					}
				}                
			}
		}
        
		/// <summary>
		/// This will return true if the conditions defined in the specified feedback's Timing section allow it to play in the current play direction of this MMFeedbacks
		/// </summary>
		/// <param name="feedback"></param>
		/// <returns></returns>
		protected bool FeedbackCanPlay(MMFeedback feedback)
		{
			if (feedback == null)
			{
				return false;
			}
			
			if (feedback.Timing == null)
			{
				return false;
			}
			
			if (feedback.Timing.MMFeedbacksDirectionCondition == MMFeedbackTiming.MMFeedbacksDirectionConditions.Always)
			{
				return true;
			}
			else if (((Direction == Directions.TopToBottom) && (feedback.Timing.MMFeedbacksDirectionCondition == MMFeedbackTiming.MMFeedbacksDirectionConditions.OnlyWhenForwards))
			         || ((Direction == Directions.BottomToTop) && (feedback.Timing.MMFeedbacksDirectionCondition == MMFeedbackTiming.MMFeedbacksDirectionConditions.OnlyWhenBackwards)))
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Readies the MMFeedbacks to revert direction on the next play
		/// </summary>
		protected virtual void ApplyAutoRevert()
		{
			if (AutoChangeDirectionOnEnd)
			{
				ShouldRevertOnNextPlay = true;
			}
		}
        
		/// <summary>
		/// Applies this feedback's time multiplier to a duration (in seconds)
		/// </summary>
		/// <param name="duration"></param>
		/// <returns></returns>
		public virtual float ApplyTimeMultiplier(float duration)
		{
			return duration * Mathf.Clamp(DurationMultiplier, _smallValue, Single.MaxValue);
		}

		/// <summary>
		/// Unity sometimes has serialization issues. 
		/// This method fixes that by fixing any bad sync that could happen.
		/// </summary>
		public virtual void AutoRepair()
		{
			List<Component> components = components = new List<Component>();
			components = this.gameObject.GetComponents<Component>().ToList();
			foreach (Component component in components)
			{
				if (component is MMFeedback)
				{
					bool found = false;
					for (int i = 0; i < Feedbacks.Count; i++)
					{
						if (Feedbacks[i] == (MMFeedback)component)
						{
							found = true;
							break;
						}
					}
					if (!found)
					{
						Feedbacks.Add((MMFeedback)component);
					}
				}
			}
		} 

		#endregion 
        
		#region EVENTS

		/// <summary>
		/// On Disable we stop all feedbacks
		/// </summary>
		protected virtual void OnDisable()
		{
			/*if (IsPlaying)
			{
			    StopFeedbacks();
			    StopAllCoroutines();
			}*/
		}

		/// <summary>
		/// On validate, we make sure our DurationMultiplier remains positive
		/// </summary>
		protected virtual void OnValidate()
		{
			DurationMultiplier = Mathf.Clamp(DurationMultiplier, _smallValue, Single.MaxValue);
		}

		/// <summary>
		/// On Destroy, removes all feedbacks from this MMFeedbacks to avoid any leftovers
		/// </summary>
		protected virtual void OnDestroy()
		{
			IsPlaying = false;
			#if UNITY_EDITOR
			if (!Application.isPlaying)
			{            
				// we remove all binders
				foreach (MMFeedback feedback in Feedbacks)
				{
					EditorApplication.delayCall += () =>
					{
						DestroyImmediate(feedback);
					};                    
				}
			}
			#endif
		}     
        
		#endregion EVENTS
	}
}