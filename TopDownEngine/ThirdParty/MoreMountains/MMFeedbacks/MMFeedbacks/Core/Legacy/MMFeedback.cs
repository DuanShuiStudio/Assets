using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 这是一个基础类，旨在被扩展，用于定义反馈（Feedback）。反馈是由MMFeedbacks触发的一种操作，通常是为了响应玩家的输入或动作
    /// 用于帮助传达情感和可行性，提升游戏感受
    /// 要创建新的反馈，请扩展此类并覆盖其自定义方法，这些方法在此类的末尾声明。您可以查看许多示例以供参考
    /// </summary>
    [AddComponentMenu("")]
	[System.Serializable]
	[ExecuteAlways]
	public abstract class MMFeedback : MonoBehaviour
	{
		/// whether or not this feedback is active
		[Tooltip("此反馈是否处于活动状态")]
		public bool Active = true;
		/// the name of this feedback to display in the inspector
		[Tooltip("此反馈在检视器中显示的名称")]
		public string Label = "MMFeedback";
		/// the chance of this feedback happening (in percent : 100 : happens all the time, 0 : never happens, 50 : happens once every two calls, etc)
		[Tooltip("此反馈发生的概率（以百分比表示：100 表示总是发生，0 表示从不发生，50 表示每隔两次调用发生一次，等等）")]
		[Range(0,100)]
		public float Chance = 100f;
		/// a number of timing-related values (delay, repeat, etc)
		[Tooltip("一组与时间相关的值（延迟、重复等）")]
		public MMFeedbackTiming Timing;
        /// 反馈的所有者，正如调用初始化方法时所定义的那样
        public GameObject Owner { get; set; }
		[HideInInspector]
        /// 此反馈是否处于调试模式
        public bool DebugActive = false;
        /// 如果您的反馈需要暂停反馈序列的执行，请将此设置为true
        public virtual IEnumerator Pause { get { return null; } }
        ///如果为true，此反馈将等待所有先前的反馈运行完毕
        public virtual bool HoldingPause { get { return false; } }
        /// 如果为true，此反馈将等待所有先前的反馈运行完毕，然后再次运行所有先前的反馈。
        public virtual bool LooperPause { get { return false; } }
        /// 如果为true，此反馈将暂停并等待其父MMFeedbacks调用Resume()以恢复执行
        public virtual bool ScriptDrivenPause { get; set; }
        /// 如果这是一个正值，反馈将在该持续时间后自动恢复，前提是它尚未通过脚本恢复
        public virtual float ScriptDrivenPauseAutoResume { get; set; }
        /// 如果为true，此反馈将等待所有之前的反馈运行完毕，然后再次运行所有之前的反馈
        public virtual bool LooperStart { get { return false; } }
        /// 一个可重写的颜色反馈，每个反馈可以重新定义。白色是唯一保留的颜色，当设置为白色时，反馈将恢复到正常（浅色或深色皮肤）。
#if UNITY_EDITOR
        public virtual Color FeedbackColor { get { return Color.white;  } }
#endif
        /// 如果此时此反馈处于冷却状态（因此无法播放），则返回true，否则返回false
        public virtual bool InCooldown { get { return (Timing.CooldownDuration > 0f) && (FeedbackTime - _lastPlayTimestamp < Timing.CooldownDuration); } }
        /// 如果为true，则此反馈当前正在播放
        public virtual bool IsPlaying { get; set; }

        /// 基于所选定时设置的时间（或未缩放时间）
        public float FeedbackTime 
		{ 
			get 
			{
				if (Timing.TimescaleMode == TimescaleModes.Scaled)
				{
					return Time.time;
				}
				else
				{
					return Time.unscaledTime;
				}
			} 
		}

        /// 基于所选定时设置的时间增量（或未缩放时间增量）
        public float FeedbackDeltaTime
		{
			get
			{
				if (Timing.TimescaleMode == TimescaleModes.Scaled)
				{
					return Time.deltaTime;
				}
				else
				{
					return Time.unscaledDeltaTime;
				}
			}
		}


        /// <summary>
        /// 此反馈的总时长：
        /// total = initial delay + duration * (number of repeats + delay between repeats)  
        /// </summary>
        public float TotalDuration
		{
			get
			{
				if ((Timing != null) && (!Timing.ContributeToTotalDuration))
				{
					return 0f;
				}
                
				float totalTime = 0f;

				if (Timing == null)
				{
					return 0f;
				}
                
				if (Timing.InitialDelay != 0)
				{
					totalTime += ApplyTimeMultiplier(Timing.InitialDelay);
				}
            
				totalTime += FeedbackDuration;

				if (Timing.NumberOfRepeats > 0)
				{
					float delayBetweenRepeats = ApplyTimeMultiplier(Timing.DelayBetweenRepeats); 
                    
					totalTime += (Timing.NumberOfRepeats * FeedbackDuration) + (Timing.NumberOfRepeats  * delayBetweenRepeats);
				}

				return totalTime;
			}
		}

        // 此反馈上次播放的时间戳
        public virtual float FeedbackStartedAt { get { return _lastPlayTimestamp; } }
        // 反馈的感知持续时间，用于显示其进度条，每个反馈应使用有意义的数据覆盖它
        public virtual float FeedbackDuration { get { return 0f; } set { } }
        ///此反馈现在是否正在播放
        public virtual bool FeedbackPlaying { get { return ((FeedbackStartedAt > 0f) && (Time.time - FeedbackStartedAt < FeedbackDuration)); } }

		public virtual MMChannelData ChannelData(int channel) => _channelData.Set(MMChannelModes.Int, channel, null);

		protected float _lastPlayTimestamp = -1f;
		protected int _playsLeft;
		protected bool _initialized = false;
		protected Coroutine _playCoroutine;
		protected Coroutine _infinitePlayCoroutine;
		protected Coroutine _sequenceCoroutine;
		protected Coroutine _repeatedPlayCoroutine;
		protected int _sequenceTrackID = 0;
		protected MMFeedbacks _hostMMFeedbacks;

		protected float _beatInterval;
		protected bool BeatThisFrame = false;
		protected int LastBeatIndex = 0;
		protected int CurrentSequenceIndex = 0;
		protected float LastBeatTimestamp = 0f;
		protected bool _isHostMMFeedbacksNotNull;
		protected MMChannelData _channelData;

		protected virtual void OnEnable()
		{
			_hostMMFeedbacks = this.gameObject.GetComponent<MMFeedbacks>();
			_isHostMMFeedbacksNotNull = _hostMMFeedbacks != null;
		}

        /// <summary>
        /// 初始化反馈及其与时间相关的变量
        /// </summary>
        /// <param name="owner"></param>
        public virtual void Initialization(GameObject owner)
		{
			_initialized = true;
			Owner = owner;
			_playsLeft = Timing.NumberOfRepeats + 1;
			_hostMMFeedbacks = this.gameObject.GetComponent<MMFeedbacks>();
			_channelData = new MMChannelData(MMChannelModes.Int, 0, null);
            
			SetInitialDelay(Timing.InitialDelay);
			SetDelayBetweenRepeats(Timing.DelayBetweenRepeats);
			SetSequence(Timing.Sequence);

			CustomInitialization(owner);            
		}

        /// <summary>
        /// 播放反馈
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        public virtual void Play(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active)
			{
				return;
			}

			if (!_initialized)
			{
				Debug.LogWarning("这个 " + this + " 反馈正在播放，但尚未初始化。请先调用Initialization()");
			}

            // 我们检查冷却时间
            if (InCooldown)
			{
				return;
			}

			if (Timing.InitialDelay > 0f) 
			{
				_playCoroutine = StartCoroutine(PlayCoroutine(position, feedbacksIntensity));
			}
			else
			{
				_lastPlayTimestamp = FeedbackTime;
				RegularPlay(position, feedbacksIntensity);
			}  
		}

        /// <summary>
        /// 一个内部协程，用于延迟反馈的初始播放
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        /// <returns></returns>
        protected virtual IEnumerator PlayCoroutine(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (Timing.TimescaleMode == TimescaleModes.Scaled)
			{
				yield return MMFeedbacksCoroutine.WaitFor(Timing.InitialDelay);
			}
			else
			{
				yield return MMFeedbacksCoroutine.WaitForUnscaled(Timing.InitialDelay);
			}
			_lastPlayTimestamp = FeedbackTime;
			RegularPlay(position, feedbacksIntensity);
		}

        /// <summary>
        /// 触发延迟协程（如果需要）
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected virtual void RegularPlay(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (Chance == 0f)
			{
				return;
			}
			if (Chance != 100f)
			{
				// determine the odds
				float random = Random.Range(0f, 100f);
				if (random > Chance)
				{
					return;
				}
			}

			if (Timing.UseIntensityInterval)
			{
				if ((feedbacksIntensity < Timing.IntensityIntervalMin) || (feedbacksIntensity >= Timing.IntensityIntervalMax))
				{
					return;
				}
			}

			if (Timing.RepeatForever)
			{
				_infinitePlayCoroutine = StartCoroutine(InfinitePlay(position, feedbacksIntensity));
				return;
			}
			if (Timing.NumberOfRepeats > 0)
			{
				_repeatedPlayCoroutine = StartCoroutine(RepeatedPlay(position, feedbacksIntensity));
				return;
			}            
			if (Timing.Sequence == null)
			{
				CustomPlayFeedback(position, feedbacksIntensity);
			}
			else
			{
				_sequenceCoroutine = StartCoroutine(SequenceCoroutine(position, feedbacksIntensity));
			}
            
		}

        /// <summary>
        /// 用于无终止的重复播放的内部协程
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        /// <returns></returns>
        protected virtual IEnumerator InfinitePlay(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			while (true)
			{
				_lastPlayTimestamp = FeedbackTime;
				if (Timing.Sequence == null)
				{
					CustomPlayFeedback(position, feedbacksIntensity);
					if (Timing.TimescaleMode == TimescaleModes.Scaled)
					{
						yield return MMFeedbacksCoroutine.WaitFor(Timing.DelayBetweenRepeats);
					}
					else
					{
						yield return MMFeedbacksCoroutine.WaitForUnscaled(Timing.DelayBetweenRepeats);
					}
				}
				else
				{
					_sequenceCoroutine = StartCoroutine(SequenceCoroutine(position, feedbacksIntensity));

					float delay = ApplyTimeMultiplier(Timing.DelayBetweenRepeats) + Timing.Sequence.Length;
					if (Timing.TimescaleMode == TimescaleModes.Scaled)
					{
						yield return MMFeedbacksCoroutine.WaitFor(delay);
					}
					else
					{
						yield return MMFeedbacksCoroutine.WaitForUnscaled(delay);
					}
				}
			}
		}

        /// <summary>
        /// 用于重复播放的内部协程
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        /// <returns></returns>
        protected virtual IEnumerator RepeatedPlay(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			while (_playsLeft > 0)
			{
				_lastPlayTimestamp = FeedbackTime;
				_playsLeft--;
				if (Timing.Sequence == null)
				{
					CustomPlayFeedback(position, feedbacksIntensity);
                    
					if (Timing.TimescaleMode == TimescaleModes.Scaled)
					{
						yield return MMFeedbacksCoroutine.WaitFor(Timing.DelayBetweenRepeats);
					}
					else
					{
						yield return MMFeedbacksCoroutine.WaitForUnscaled(Timing.DelayBetweenRepeats);
					}
				}
				else
				{
					_sequenceCoroutine = StartCoroutine(SequenceCoroutine(position, feedbacksIntensity));
                    
					float delay = ApplyTimeMultiplier(Timing.DelayBetweenRepeats) + Timing.Sequence.Length;
					if (Timing.TimescaleMode == TimescaleModes.Scaled)
					{
						yield return MMFeedbacksCoroutine.WaitFor(delay);
					}
					else
					{
						yield return MMFeedbacksCoroutine.WaitForUnscaled(delay);
					}
				}
			}
			_playsLeft = Timing.NumberOfRepeats + 1;
		}

        /// <summary>
        /// 一个用于按顺序播放此反馈的协程
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        /// <returns></returns>
        protected virtual IEnumerator SequenceCoroutine(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			yield return null;
			float timeStartedAt = FeedbackTime;
			float lastFrame = FeedbackTime;

			BeatThisFrame = false;
			LastBeatIndex = 0;
			CurrentSequenceIndex = 0;
			LastBeatTimestamp = 0f;

			if (Timing.Quantized)
			{
				while (CurrentSequenceIndex < Timing.Sequence.QuantizedSequence[0].Line.Count)
				{
					_beatInterval = 60f / Timing.TargetBPM;

					if ((FeedbackTime - LastBeatTimestamp >= _beatInterval) || (LastBeatTimestamp == 0f))
					{
						BeatThisFrame = true;
						LastBeatIndex = CurrentSequenceIndex;
						LastBeatTimestamp = FeedbackTime;

						for (int i = 0; i < Timing.Sequence.SequenceTracks.Count; i++)
						{
							if (Timing.Sequence.QuantizedSequence[i].Line[CurrentSequenceIndex].ID == Timing.TrackID)
							{
								CustomPlayFeedback(position, feedbacksIntensity);
							}
						}
						CurrentSequenceIndex++;
					}
					yield return null;
				}
			}
			else
			{
				while (FeedbackTime - timeStartedAt < Timing.Sequence.Length)
				{
					foreach (MMSequenceNote item in Timing.Sequence.OriginalSequence.Line)
					{
						if ((item.ID == Timing.TrackID) && (item.Timestamp >= lastFrame) && (item.Timestamp <= FeedbackTime - timeStartedAt))
						{
							CustomPlayFeedback(position, feedbacksIntensity);
						}
					}
					lastFrame = FeedbackTime - timeStartedAt;
					yield return null;
				}
			}
                    
		}

        /// <summary>
        /// 停止所有反馈的播放。将停止重复的反馈，并调用自定义的停止实现
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        public virtual void Stop(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (_playCoroutine != null) { StopCoroutine(_playCoroutine); }
			if (_infinitePlayCoroutine != null) { StopCoroutine(_infinitePlayCoroutine); }
			if (_repeatedPlayCoroutine != null) { StopCoroutine(_repeatedPlayCoroutine); }            
			if (_sequenceCoroutine != null) { StopCoroutine(_sequenceCoroutine);  }

			_lastPlayTimestamp = 0f;
			_playsLeft = Timing.NumberOfRepeats + 1;
			if (Timing.InterruptsOnStop)
			{
				CustomStopFeedback(position, feedbacksIntensity);    
			}
		}

        /// <summary>
        /// 调用此反馈的自定义重置 
        /// </summary>
        public virtual void ResetFeedback()
		{
			_playsLeft = Timing.NumberOfRepeats + 1;
			CustomReset();
		}

        /// <summary>
        /// 使用此方法在运行时更改此反馈的顺序
        /// </summary>
        /// <param name="newSequence"></param>
        public virtual void SetSequence(MMSequence newSequence)
		{
			Timing.Sequence = newSequence;
			if (Timing.Sequence != null)
			{
				for (int i = 0; i < Timing.Sequence.SequenceTracks.Count; i++)
				{
					if (Timing.Sequence.SequenceTracks[i].ID == Timing.TrackID)
					{
						_sequenceTrackID = i;
					}
				}
			}
		}

        /// <summary>
        /// 使用此方法在运行时指定重复之间的新延迟
        /// </summary>
        /// <param name="delay"></param>
        public virtual void SetDelayBetweenRepeats(float delay)
		{
			Timing.DelayBetweenRepeats = delay;
		}

        /// <summary>
        /// 使用此方法在运行时指定新的初始延迟
        /// </summary>
        /// <param name="delay"></param>
        public virtual void SetInitialDelay(float delay)
		{
			Timing.InitialDelay = delay;
		}

        /// <summary>
        /// 根据此反馈的当前播放方向，返回归一化时间的新值
        /// </summary>
        /// <param name="normalizedTime"></param>
        /// <returns></returns>
        protected virtual float ApplyDirection(float normalizedTime)
		{
			return NormalPlayDirection ? normalizedTime : 1 - normalizedTime;
		}

        /// <summary>
        /// 如果此反馈应正常播放，则返回true；如果应倒带播放，则返回false。
        /// </summary>
        public virtual bool NormalPlayDirection
		{
			get
			{
				switch (Timing.PlayDirection)
				{
					case MMFeedbackTiming.PlayDirections.FollowMMFeedbacksDirection:
						return (_hostMMFeedbacks.Direction == MMFeedbacks.Directions.TopToBottom);
					case MMFeedbackTiming.PlayDirections.AlwaysNormal:
						return true;
					case MMFeedbackTiming.PlayDirections.AlwaysRewind:
						return false;
					case MMFeedbackTiming.PlayDirections.OppositeMMFeedbacksDirection:
						return !(_hostMMFeedbacks.Direction == MMFeedbacks.Directions.TopToBottom);
				}
				return true;
			}
		}

        /// <summary>
        /// 如果此反馈应根据其父MMFeedbacks的方向设置（MMFeedbacksDirectionCondition）正常播放，则返回true；否则返回false。
        /// </summary>
        public virtual bool ShouldPlayInThisSequenceDirection
		{
			get
			{
				switch (Timing.MMFeedbacksDirectionCondition)
				{
					case MMFeedbackTiming.MMFeedbacksDirectionConditions.Always:
						return true;
					case MMFeedbackTiming.MMFeedbacksDirectionConditions.OnlyWhenForwards:
						return (_hostMMFeedbacks.Direction == MMFeedbacks.Directions.TopToBottom);
					case MMFeedbackTiming.MMFeedbacksDirectionConditions.OnlyWhenBackwards:
						return (_hostMMFeedbacks.Direction == MMFeedbacks.Directions.BottomToTop);
				}
				return true;
			}
		}

        /// <summary>
        /// 返回在反馈播放时间结束时评估曲线的t值。
        /// </summary>
        protected virtual float FinalNormalizedTime
		{
			get
			{
				return NormalPlayDirection ? 1f : 0f;
			}
		}

        /// <summary>
        /// 将主机MMFeedbacks的时间乘数应用于此反馈
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        protected virtual float ApplyTimeMultiplier(float duration)
		{
			if (_isHostMMFeedbacksNotNull)
			{
				return _hostMMFeedbacks.ApplyTimeMultiplier(duration);    
			}

			return duration;
		}

        /// <summary>
        /// 此方法描述了除了主Initialization方法之外，反馈所需的所有自定义初始化过程
        /// </summary>
        /// <param name="owner"></param>
        protected virtual void CustomInitialization(GameObject owner) { }

        /// <summary>
        /// 此方法描述了当反馈被播放时会发生什么
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected abstract void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f);

        /// <summary>
        /// 此方法描述了当反馈被停止时会发生什么
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected virtual void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f) { }

        /// <summary>
        /// 此方法描述了当反馈被重置时会发生什么
        /// </summary>
        protected virtual void CustomReset() { }
	}   
}