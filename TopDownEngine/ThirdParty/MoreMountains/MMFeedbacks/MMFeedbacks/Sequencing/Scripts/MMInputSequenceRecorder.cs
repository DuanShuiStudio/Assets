using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This class lets you record sequences via input presses
	/// </summary>
	[AddComponentMenu("More Mountains/Feedbacks/Sequencing/MM Input Sequence Recorder")]
	[ExecuteAlways]
	public class MMInputSequenceRecorder : MonoBehaviour
	{
		[Header("Target目标")]
		/// the target scriptable object to write to
		[Tooltip("要写入的目标可脚本化对象。 ")]
		public MMSequence SequenceScriptableObject;

		[Header("Recording录音")]
		/// whether this recorder is recording right now or not
		[MMFReadOnly]
		[Tooltip("这个录音机当前是否正在录音。 ")]
		public bool Recording;
		/// whether any silence between the start of the recording and the first press should be removed or not
		[Tooltip("在录音开始到第一次按键之间出现的任何静音部分是否应该被去除。 ")]
		public bool RemoveInitialSilence = true;
		/// whether this recording should write on top of existing entries or not
		[Tooltip("这次录音是否应该覆盖已有的记录条目。 ")]
		public bool AdditiveRecording = false;
		/// whether this recorder should start recording when entering play mode
		[Tooltip("这个录音机在进入播放模式时是否应该开始进行录制。 ")]
		public bool StartRecordingOnGameStart = false;
		/// the offset to apply to entries
		[Tooltip("要应用于条目的偏移量。 ")]
		public float RecordingStartOffset = 0f;

		[Header("Recorder Keys录音机按键")]
		/// the key binding for recording start
		[Tooltip("录制开始的按键绑定。")]
		public KeyCode StartRecordingHotkey = KeyCode.Home;
		/// the key binding for recording stop
		[Tooltip("录制停止的按键绑定。")]
		public KeyCode StopRecordingHotkey = KeyCode.End;

		protected MMSequenceNote _note;
		protected float _recordingStartedAt = 0f;

        /// <summary>
        /// 在（脚本或对象）唤醒时，我们初始化我们的录制器。 
        /// </summary>
        protected virtual void Awake()
		{
			Initialization();
		}

        /// <summary>
        /// 确保我们有一个可脚本化对象来进行录制操作。 
        /// </summary>
        public virtual void Initialization()
		{
			Recording = false;

			_note = new MMSequenceNote();

			if (SequenceScriptableObject == null)
			{
				Debug.LogError(this.name + " 这个基于输入的序列器需要一个已绑定的可脚本化对象才能正常工作，请创建一个（可脚本化对象）并在检查器中进行绑定。  ");
			}
		}

        /// <summary>
        /// 在（程序或脚本）启动时，如果有需要的话就开始一次录制。 
        /// </summary>
        protected virtual void Start()
		{
			if (StartRecordingOnGameStart)
			{
				StartRecording();
			}
		}

        /// <summary>
        /// 如有必要，清除序列，然后开始录制。 
        /// </summary>
        public virtual void StartRecording()
		{
			Recording = true;
			if (!AdditiveRecording)
			{
				SequenceScriptableObject.OriginalSequence.Line.Clear();
			}            
			_recordingStartedAt = Time.realtimeSinceStartup;
		}

        /// <summary>
        /// 停止录制。
        /// </summary>
        public virtual void StopRecording()
		{
			Recording = false;
			SequenceScriptableObject.QuantizeOriginalSequence();
		}

        /// <summary>
        /// 在每帧更新时，我们会检测按键输入
        /// </summary>
        protected virtual void Update()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			DetectStartAndEnd();
			DetectRecording();
		}

        /// <summary>
        /// 检测用于开始和结束录制操作的按键按下情况。 
        /// </summary>
        protected virtual void DetectStartAndEnd()
		{
			#if !ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER
			if (!Recording)
			{
				if (Input.GetKeyDown(StartRecordingHotkey))
				{
					StartRecording();
				}
			}
			else
			{
				if (Input.GetKeyDown(StartRecordingHotkey))
				{
					StopRecording();
				}
			}
			#endif
		}

        /// <summary>
        /// 查找按键按下事件以便向序列中写入数据。 
        /// </summary>
        protected virtual void DetectRecording()
		{
			if (Recording && (SequenceScriptableObject != null))
			{
				foreach (MMSequenceTrack track in SequenceScriptableObject.SequenceTracks)
				{                    
					if (Input.GetKeyDown(track.Key))
					{
						AddNoteToTrack(track);
					}                    
				}
			}
		}

        /// <summary>
        /// 向指定的轨道添加一个音符
        /// </summary>
        /// <param name="track"></param>
        public virtual void AddNoteToTrack(MMSequenceTrack track)
		{
			if ((SequenceScriptableObject.OriginalSequence.Line.Count == 0) && RemoveInitialSilence)
			{
				_recordingStartedAt = Time.realtimeSinceStartup;
			}

			_note = new MMSequenceNote();
			_note.ID = track.ID;
			_note.Timestamp = Time.realtimeSinceStartup + RecordingStartOffset - _recordingStartedAt;
			SequenceScriptableObject.OriginalSequence.Line.Add(_note);
		}
	}
}