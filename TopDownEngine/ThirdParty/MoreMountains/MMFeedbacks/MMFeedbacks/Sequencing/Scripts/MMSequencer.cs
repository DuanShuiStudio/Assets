using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 这个类允许你使用音序器界面设计一个量化的 MM 序列（MMSequence），并且能让你播放该量化序列，若有指定，还能在节拍上触发事件。 
    /// </summary>
    [AddComponentMenu("More Mountains/Feedbacks/Sequencing/MM Sequencer")]
	public class MMSequencer : MonoBehaviour
	{
		public enum TimeScales { Scaled, Unscaled }
		
		[Header("Sequence序列")]
		/// the sequence to design on or to play
		[Tooltip("要进行设计或播放的序列。 ")]
		public MMSequence Sequence;
		/// the intended BPM for playback and design
		[Tooltip("用于播放和设计的目标每分钟节拍数（BPM）。 ")]
		public int BPM = 160;
		/// the number of notes in the sequence
		[Tooltip("序列中的音符数量。")]
		public int SequencerLength = 8;

		[Header("Playback回放")]
		/// the timescale on which this sequencer should play
		[Tooltip("该音序器应采用的时间尺度。")]
		public TimeScales TimeScale = TimeScales.Scaled;
		/// whether the sequence should loop or not when played back
		[Tooltip("该序列在回放时是否应该循环。 ")]
		public bool Loop = true;
		/// if this is true the sequence will play in random order
		[Tooltip("如果这一条件为真，那么该序列将以随机顺序播放。")]
		public bool RandomSequence = false;
		/// whether that sequencer should start playing on application start
		[Tooltip("那个音序器是否应该在应用程序启动时就开始播放。 ")]
		public bool PlayOnStart = false;
        
		[Header("Metronome节拍器")]
		/// a sound to play every beat
		[Tooltip("每一拍都要播放的一个声音")]
		public AudioClip MetronomeSound;
		/// the volume of the metronome sound
		[Tooltip("节拍器声音的音量。")]
		[Range(0f, 1f)]
		public float MetronomeVolume = 0.2f;

		[Header("Events事件")]
		/// a list of events to play every time an active beat is found on each track (one event per track)
		[Tooltip("每次在每条轨道上找到有效节拍时要播放的事件列表（每条轨道一个事件） ")]
		public List<UnityEvent> TrackEvents;

		[Header("Monitor监测")]
		/// true if the sequencer is playing right now
		[Tooltip("如果音序器当前正在播放，则为真。 ")]
		[MMFReadOnly]
		public bool Playing = false;
		/// true if the sequencer has been played once
		[Tooltip("如果音序器已经播放过一次，则为真。 ")]
		[HideInInspector]
		public bool PlayedOnce = false;
		/// true if a perfect beat was found this frame
		[Tooltip("如果在这一帧中找到了一个完美的节拍，则为真。 ")]
		[MMFReadOnly]
		public bool BeatThisFrame = false;
		/// the index of the last played bit (our position in the playing sequence)
		[Tooltip("上一次播放的位的索引（即我们在正在播放的序列中的位置） ")]
		[MMFReadOnly]
		public int LastBeatIndex = 0;
		
		public float InternalTime => TimeScale == TimeScales.Scaled ? Time.time : Time.unscaledTime;

		[HideInInspector]
		public int LastBPM = -1;
		[HideInInspector]
		public int LastTracksCount = -1;
		[HideInInspector]
		public int LastSequencerLength = -1;
		[HideInInspector]
		public MMSequence LastSequence;
		[HideInInspector]
		public int CurrentSequenceIndex = 0;
		[HideInInspector]
		public float LastBeatTimestamp = 0f;

		protected float _beatInterval;
		protected AudioSource _beatSoundAudiosource;

        /// <summary>
        /// 在启动时，我们初始化我们的音序器。 
        /// </summary>
        protected virtual void Start()
		{
			Initialization();
		}

        /// <summary>
        /// 在初始化时，初始化节拍器，并在有需要的情况下播放该序列。 
        /// </summary>
        protected virtual void Initialization()
		{
			Playing = false;
			if (MetronomeSound != null)
			{
				GameObject go = new GameObject();
				SceneManager.MoveGameObjectToScene(go, this.gameObject.scene);
				go.name = "BeatSoundAudioSource";
				go.transform.SetParent(this.transform);
				_beatSoundAudiosource = go.AddComponent<AudioSource>();
				_beatSoundAudiosource.clip = MetronomeSound;
				_beatSoundAudiosource.loop = false;
				_beatSoundAudiosource.playOnAwake = false;                
			}
			if (PlayOnStart)
			{
				PlaySequence();
			}
		}

        /// <summary>
        /// 根据序列当前的状态来播放或停止该序列。 
        /// </summary>
        public virtual void ToggleSequence()
		{
			if (Playing)
			{
				StopSequence();
			}
			else
			{
				PlaySequence();
			}
		}

        /// <summary>
        /// 开始播放该序列
        /// </summary>
        public virtual void PlaySequence()
		{
			CurrentSequenceIndex = 0;
			Playing = true;
			LastBeatTimestamp = 0f;            
		}

        /// <summary>
        /// 停止序列的播放。
        /// </summary>
        public virtual void StopSequence()
		{
			Playing = false;
		}

        /// <summary>
        /// 清除该序列的内容。
        /// </summary>
        public virtual void ClearSequence()
		{
			for (int trackIndex = 0; trackIndex < Sequence.SequenceTracks.Count; trackIndex++)
			{
				for (int i = 0; i < SequencerLength; i++)
				{
					Sequence.QuantizedSequence[trackIndex].Line[i].ID = -1;
				}
			}
		}

        /// <summary>
        /// 在更新过程中，我们处理节拍相关的事宜。 
        /// </summary>
        protected virtual void Update()
		{
			HandleBeat();
		}

        /// <summary>
        /// 判断我们是否处于一个节拍上，并且在有需要时播放该节拍。 
        /// </summary>
        protected virtual void HandleBeat()
		{
			BeatThisFrame = false;

			if (!Playing)
			{
				return;
			}

			if (CurrentSequenceIndex >= SequencerLength)
			{
				StopSequence();
				return;
			}

			_beatInterval = 60f / BPM;

			if ((InternalTime - LastBeatTimestamp >= _beatInterval) || (LastBeatTimestamp == 0f))
			{
				PlayBeat();
			}
		}

        /// <summary>
        /// 如果有需要，在节拍上触发事件。 
        /// </summary>
        public virtual void PlayBeat()
		{
			BeatThisFrame = true;
			LastBeatIndex = CurrentSequenceIndex;
			LastBeatTimestamp = InternalTime;
			PlayedOnce = true;
			PlayMetronomeSound();
			OnBeat();

			for (int i = 0; i < Sequence.SequenceTracks.Count; i++)
			{
				if ((Sequence.SequenceTracks[i].Active) && (Sequence.QuantizedSequence[i].Line[CurrentSequenceIndex].ID != -1))
				{
					if (TrackEvents[i] != null)
					{
						TrackEvents[i].Invoke();
					}
				}
			}
			CurrentSequenceIndex++;
			if ((CurrentSequenceIndex >= SequencerLength) && Loop)
			{
				CurrentSequenceIndex = 0;
			}
			if (RandomSequence)
			{
				CurrentSequenceIndex = UnityEngine.Random.Range(0, SequencerLength);
			}
		}

        /// <summary>
        /// 每次找到节拍时都会触发，其目的是可被重写（覆盖）。 
        /// </summary>
        protected virtual void OnBeat()
		{

		}

        /// <summary>
        /// 播放按预期应该在指定轨道上发生的轨道事件。 
        /// </summary>
        /// <param name="index"></param>
        public virtual void PlayTrackEvent(int index)
		{
			TrackEvents[index].Invoke();
		}

        /// <summary>
        /// 将一个音序轨道设置为激活状态（将会播放其音符）或非激活状态（不会播放其音符）。 
        /// </summary>
        /// <param name="trackIndex"></param>
        public virtual void ToggleActive(int trackIndex)
		{
			Sequence.SequenceTracks[trackIndex].Active = !Sequence.SequenceTracks[trackIndex].Active;
		}

        /// <summary>
        /// 切换一整步列，使该列中的所有音符变为激活或非激活状态。 
        /// </summary>
        /// <param name="stepIndex"></param>
        public virtual void ToggleStep(int stepIndex)
		{
			bool active = (Sequence.QuantizedSequence[0].Line[stepIndex].ID != -1);

			for (int trackIndex = 0; trackIndex < Sequence.SequenceTracks.Count; trackIndex++)
			{
				if (active)
				{
					Sequence.QuantizedSequence[trackIndex].Line[stepIndex].ID = -1;
				}
				else
				{
					Sequence.QuantizedSequence[trackIndex].Line[stepIndex].ID = Sequence.SequenceTracks[trackIndex].ID;
				}
			}
		}

        /// <summary>
        /// 播放节拍器的声音。
        /// </summary>
        protected virtual void PlayMetronomeSound()
		{
			if (MetronomeSound != null)
			{
				_beatSoundAudiosource.volume = MetronomeVolume;
				_beatSoundAudiosource.Play();
			}
		}

        /// <summary>
        /// 在序列的末尾添加一列。
        /// </summary>
        public virtual void IncrementLength()
		{
			if (Sequence == null)
			{
				return;
			}
			float beatDuration = 60f / BPM;
			SequencerLength++;
			Sequence.Length = SequencerLength * beatDuration;
			LastSequencerLength = SequencerLength;

			for (int trackIndex = 0; trackIndex < Sequence.SequenceTracks.Count; trackIndex++)
			{
				MMSequenceNote newNote = new MMSequenceNote();
				newNote.ID = -1;
				newNote.Timestamp = Sequence.QuantizedSequence[trackIndex].Line.Count * beatDuration;
				Sequence.QuantizedSequence[trackIndex].Line.Add(newNote);
			}
		}

        /// <summary>
        /// 删除序列的最后一列。
        /// </summary>
        public virtual void DecrementLength()
		{
			if (Sequence == null)
			{
				return;
			}
			float beatDuration = 60f / BPM;
			SequencerLength--;
			Sequence.Length = SequencerLength * beatDuration;
			LastSequencerLength = SequencerLength;

			for (int trackIndex = 0; trackIndex < Sequence.SequenceTracks.Count; trackIndex++)
			{
				int removeIndex = Sequence.QuantizedSequence[trackIndex].Line.Count - 1;
				Sequence.QuantizedSequence[trackIndex].Line.RemoveAt(removeIndex);
			}
		}

        /// <summary>
        /// 解析序列的内容，并更新时间戳，使其与新的每分钟节拍数（BPM）相匹配。 
        /// </summary>
        public virtual void UpdateTimestampsToMatchNewBPM()
		{
			if (Sequence == null)
			{
				return;
			}
			float beatDuration = 60f / BPM;

			Sequence.TargetBPM = BPM;
			Sequence.Length = SequencerLength * beatDuration;
			Sequence.EndSilenceDuration = beatDuration;
			Sequence.Quantized = true;

			for (int trackIndex = 0; trackIndex < Sequence.SequenceTracks.Count; trackIndex++)
			{
				for (int i = 0; i < SequencerLength; i++)
				{
					Sequence.QuantizedSequence[trackIndex].Line[i].Timestamp = i * beatDuration;
				}
			}
			LastBPM = BPM;
		}

        /// <summary>
        /// 重建序列属性以匹配长度和轨道数量——这将会清除原有内容。 
        /// </summary>
        public virtual void ApplySequencerLengthToSequence()
		{
			if (Sequence == null)
			{
				return;
			}

			float beatDuration = 60f / BPM;

			Sequence.TargetBPM = BPM;
			Sequence.Length = SequencerLength * beatDuration;
			Sequence.EndSilenceDuration = beatDuration;
			Sequence.Quantized = true;
            
			if ((LastSequencerLength != SequencerLength) || (LastTracksCount != Sequence.SequenceTracks.Count))
			{
				Sequence.QuantizedSequence = new List<MMSequenceList>();

				for (int trackIndex = 0; trackIndex < Sequence.SequenceTracks.Count; trackIndex++)
				{
					Sequence.QuantizedSequence.Add(new MMSequenceList());
					Sequence.QuantizedSequence[trackIndex].Line = new List<MMSequenceNote>();
					for (int i = 0; i < SequencerLength; i++)
					{
						MMSequenceNote note = new MMSequenceNote();
						note.ID = -1;
						note.Timestamp = i * beatDuration;
						Sequence.QuantizedSequence[trackIndex].Line.Add(note);
					}
				}                
			}
            
			LastTracksCount = Sequence.SequenceTracks.Count;
			LastSequencerLength = SequencerLength;
		}

        /// <summary>
        /// 编辑器每帧都会执行此操作，以处理可能出现的变化。 
        /// </summary>
        public virtual void EditorMaintenance()
		{
			SetupTrackEvents();
		}

        /// <summary>
        /// 向事件列表中添加内容或重新构建事件列表。 
        /// </summary>
        public virtual void SetupTrackEvents()
		{
			if (Sequence == null)
			{
				return;
			}

            // 配置事件
            if (TrackEvents.Count < Sequence.SequenceTracks.Count)
			{
				for (int i = 0; i < Sequence.SequenceTracks.Count; i++)
				{
					if (i >= TrackEvents.Count)
					{
						TrackEvents.Add(new UnityEvent());
					}
				}
			}
			if (TrackEvents.Count > Sequence.SequenceTracks.Count)
			{
				TrackEvents.Clear();
				for (int i = 0; i < Sequence.SequenceTracks.Count; i++)
				{
					TrackEvents.Add(new UnityEvent());
				}
			}
		}
	}
}