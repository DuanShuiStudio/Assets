using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// 序列音符的可能状态
    public enum MMSequenceTrackStates { Idle, Down, Up }

    /// <summary>
    /// 一个描述序列音符内容的类，基本上包含一个时间戳以及在该时间戳要播放的标识（ID）。 
    /// </summary>
    [System.Serializable]
	public class MMSequenceNote
	{
		public float Timestamp;
		public int ID;

		public virtual MMSequenceNote Copy()
		{
			MMSequenceNote newNote = new MMSequenceNote();
			newNote.ID = this.ID;
			newNote.Timestamp = this.Timestamp;
			return newNote;
		}
	}

    /// <summary>
    /// 一个描述序列轨道属性的类：标识（ID）、颜色（用于检查器显示）、按键（用于录制器操作）、状态（用于录制器）。  
    /// </summary>
    [System.Serializable]
	public class MMSequenceTrack
	{
		public int ID = 0;
		public Color TrackColor;
		public KeyCode Key = KeyCode.Space;
		public bool Active = true;
		[MMFReadOnly]
		public MMSequenceTrackStates State = MMSequenceTrackStates.Idle;
		[HideInInspector]
		public bool Initialized = false;
        
		public virtual void SetDefaults(int index)
		{
			if (!Initialized)
			{
				ID = index;
				TrackColor = MMSequence.RandomSequenceColor();
				Key = KeyCode.Space;
				Active = true;
				State = MMSequenceTrackStates.Idle;
				Initialized = true;
			}            
		}
	}

    /// <summary>
    /// 一个用于存储序列音符的类。
    /// </summary>
    [System.Serializable]
	public class MMSequenceList
	{
		public List<MMSequenceNote> Line;
	}

    /// <summary>
    /// 这个可脚本化对象保存着“序列”，即用于按顺序记录和播放事件的数据。 
    /// MM序列可以由MM反馈在其“时间安排”部分进行播放，也可以由音序器播放，并且有可能由其他类来播放。  
    /// </summary>
    [CreateAssetMenu(menuName = "MoreMountains/Sequencer/MMSequence")]
	public class MMSequence : ScriptableObject
	{
		[Header("Sequence序列")]
		/// the length (in seconds) of the sequence
		[Tooltip("该序列的长度（以秒为单位） ")]
		[MMFReadOnly]
		public float Length;
		/// the original sequence (as outputted by the input sequence recorder)
		[Tooltip("原始序列（即由输入序列录制器输出的序列） ")]
		public MMSequenceList OriginalSequence;
		/// the duration in seconds to apply after the last input
		[Tooltip("在最后一次输入之后要应用的时长（以秒为单位） ")]
		public float EndSilenceDuration = 0f;

		[Header("Sequence Contents序列内容")]
		/// the list of tracks for this sequence
		[Tooltip("此序列的轨道列表。")]
		public List<MMSequenceTrack> SequenceTracks;

		[Header("Quantizing量化")]
		/// whether this sequence should be used in quantized form or not
		[Tooltip("该序列是否应采用量化形式来使用。 ")]
		public bool Quantized;
		/// the target BPM for this sequence
		[Tooltip("此序列的目标每分钟节拍数（BPM，即Beats Per Minute）。 ")]
		public int TargetBPM = 120;
		/// the contents of the quantized sequence
		[Tooltip("已量化序列的内容。")]
		public List<MMSequenceList> QuantizedSequence;
        
		[Space]
		[Header("Controls控制")]
		[MMFInspectorButton("RandomizeTrackColors")]
		public bool RandomizeTrackColorsButton;
        
		protected float[] _quantizedBeats; 
		protected List<MMSequenceNote> _deleteList;

        /// <summary>
        /// 比较并对两个序列音符进行排序。
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        static int SortByTimestamp(MMSequenceNote p1, MMSequenceNote p2)
		{
			return p1.Timestamp.CompareTo(p2.Timestamp);
		}

        /// <summary>
        /// 根据时间戳对原始序列进行排序。
        /// </summary>
        public virtual void SortOriginalSequence()
		{
			OriginalSequence.Line.Sort(SortByTimestamp);
		}

        /// <summary>
        /// 对原始序列进行量化，填充“已量化序列”列表，将事件排列在节拍上。 
        /// </summary>
        public virtual void QuantizeOriginalSequence()
		{
			ComputeLength();
			QuantizeSequenceToBPM(OriginalSequence.Line);
		}

        /// <summary>
        /// 计算该序列的长度。
        /// </summary>
        public virtual void ComputeLength()
		{
			Length = OriginalSequence.Line[OriginalSequence.Line.Count - 1].Timestamp + EndSilenceDuration;
		}

        /// <summary>
        /// 使序列中的每个时间戳与每分钟节拍数（BPM）轨道相匹配。 
        /// </summary>
        public virtual void QuantizeSequenceToBPM(List<MMSequenceNote> baseSequence)
		{
			float sequenceLength = Length;
			float beatDuration = 60f / TargetBPM;
			int numberOfBeatsInSequence = (int)(sequenceLength / beatDuration);
			QuantizedSequence = new List<MMSequenceList>();
			_deleteList = new List<MMSequenceNote>();
			_deleteList.Clear();

            // 我们用计算得出的时间戳来填充每分钟节拍数（BPM）轨道。 
            _quantizedBeats = new float[numberOfBeatsInSequence];
			for (int i = 0; i < numberOfBeatsInSequence; i++)
			{
				_quantizedBeats[i] = i * beatDuration;
			}
            
			for (int i = 0; i < SequenceTracks.Count; i++)
			{
				QuantizedSequence.Add(new MMSequenceList());
				QuantizedSequence[i].Line = new List<MMSequenceNote>();
				for (int j = 0; j < numberOfBeatsInSequence; j++)
				{
					MMSequenceNote newNote = new MMSequenceNote();
					newNote.ID = -1;
					newNote.Timestamp = _quantizedBeats[j];
					QuantizedSequence[i].Line.Add(newNote);

					foreach (MMSequenceNote note in baseSequence)
					{
						float newTimestamp = RoundFloatToArray(note.Timestamp, _quantizedBeats);
						if ((newTimestamp == _quantizedBeats[j]) && (note.ID == SequenceTracks[i].ID))
						{
							QuantizedSequence[i].Line[j].ID = note.ID;
						}
					}
				}
			}        
		}

        /// <summary>
        /// 在进行验证时，我们会初始化我们轨道的各项属性。 
        /// </summary>
        protected virtual void OnValidate()
		{
			for (int i = 0; i < SequenceTracks.Count; i++)
			{
				SequenceTracks[i].SetDefaults(i);
			}
		}

        /// <summary>
        /// 随机化轨道颜色。
        /// </summary>
        protected virtual void RandomizeTrackColors()
		{
			foreach(MMSequenceTrack track in SequenceTracks)
			{
				track.TrackColor = RandomSequenceColor();
			}
		}

        /// <summary>
        /// 为序列轨道返回一种随机颜色。
        /// </summary>
        /// <returns></returns>
        public static Color RandomSequenceColor()
		{
			int random = UnityEngine.Random.Range(0, 32);
			switch (random)
			{
				case 0: return new Color32(240, 248, 255, 255); 
				case 1: return new Color32(127, 255, 212, 255);
				case 2: return new Color32(245, 245, 220, 255);
				case 3: return new Color32(95, 158, 160, 255);
				case 4: return new Color32(255, 127, 80, 255);
				case 5: return new Color32(0, 255, 255, 255);
				case 6: return new Color32(255, 215, 0, 255);
				case 7: return new Color32(255, 0, 255, 255);
				case 8: return new Color32(50, 128, 120, 255);
				case 9: return new Color32(173, 255, 47, 255);
				case 10: return new Color32(255, 105, 180, 255);
				case 11: return new Color32(75, 0, 130, 255);
				case 12: return new Color32(255, 255, 240, 255);
				case 13: return new Color32(124, 252, 0, 255);
				case 14: return new Color32(255, 160, 122, 255);
				case 15: return new Color32(0, 255, 0, 255);
				case 16: return new Color32(245, 255, 250, 255);
				case 17: return new Color32(255, 228, 225, 255);
				case 18: return new Color32(218, 112, 214, 255);
				case 19: return new Color32(255, 192, 203, 255);
				case 20: return new Color32(255, 0, 0, 255);
				case 21: return new Color32(196, 112, 255, 255);
				case 22: return new Color32(250, 128, 114, 255);
				case 23: return new Color32(46, 139, 87, 255);
				case 24: return new Color32(192, 192, 192, 255);
				case 25: return new Color32(135, 206, 235, 255);
				case 26: return new Color32(0, 255, 127, 255);
				case 27: return new Color32(210, 180, 140, 255);
				case 28: return new Color32(0, 128, 128, 255);
				case 29: return new Color32(255, 99, 71, 255);
				case 30: return new Color32(64, 224, 208, 255);
				case 31: return new Color32(255, 255, 0, 255);
				case 32: return new Color32(154, 205, 50, 255);
			}
			return new Color32(240, 248, 255, 255); 
		}

        /// <summary>
        /// 将一个浮点数四舍五入为数组中最接近的浮点数（数组必须是已排序的） 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="array"></param>
        /// <returns></returns>
        public static float RoundFloatToArray(float value, float[] array)
		{
			int min = 0;
			if (array[min] >= value) return array[min];

			int max = array.Length - 1;
			if (array[max] <= value) return array[max];

			while (max - min > 1)
			{
				int mid = (max + min) / 2;

				if (array[mid] == value)
				{
					return array[mid];
				}
				else if (array[mid] < value)
				{
					min = mid;
				}
				else
				{
					max = mid;
				}
			}

			if (array[max] - value <= value - array[min])
			{
				return array[max];
			}
			else
			{
				return array[min];
			}
		}
	}
}