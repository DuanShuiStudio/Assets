using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 一个带有现成插槽的MMSequencer，用于播放音频源。  
    /// </summary>
    [AddComponentMenu("More Mountains/Feedbacks/Sequencing/MM AudioSource Sequencer")]
	public class MMAudioSourceSequencer : MMSequencer
	{
		/// the list of audio sources to play (one per track)
		[Tooltip("要播放的音频源列表（每条音轨对应一个） ")]
		public List<AudioSource> AudioSources;

        /// <summary>
        /// 在节拍到来时，我们播放我们的音频源。 
        /// </summary>
        protected override void OnBeat()
		{
			base.OnBeat();
			for (int i = 0; i < Sequence.SequenceTracks.Count; i++)
			{
				if ((Sequence.SequenceTracks[i].Active) && (Sequence.QuantizedSequence[i].Line[CurrentSequenceIndex].ID != -1))
				{
					if ((AudioSources.Count > i) && (AudioSources[i] != null))
					{
						AudioSources[i].Play();
					}
				}
			}
		}

        /// <summary>
        /// 当播放用于控制的事件时，我们播放我们的音频源。 
        /// </summary>
        /// <param name="index"></param>
        public override void PlayTrackEvent(int index)
		{
			if (!Application.isPlaying)
			{
				return;
			}
			base.PlayTrackEvent(index);
			AudioSources[index].Play();
		}

        /// <summary>
        /// 在查找变化时，我们要确保数组中有足够的音频源。 
        /// </summary>
        public override void EditorMaintenance()
		{
			base.EditorMaintenance();
			SetupSounds();
		}

        /// <summary>
        /// 确保数组的长度始终正确。
        /// </summary>
        public virtual void SetupSounds()
		{
			if (Sequence == null)
			{
				return;
			}
            // 设置事件 
            if (AudioSources.Count < Sequence.SequenceTracks.Count)
			{
				for (int i = 0; i < Sequence.SequenceTracks.Count; i++)
				{
					if (i >= AudioSources.Count)
					{
						AudioSources.Add(null);
					}
				}
			}
			if (AudioSources.Count > Sequence.SequenceTracks.Count)
			{
				AudioSources.Clear();
				for (int i = 0; i < Sequence.SequenceTracks.Count; i++)
				{
					AudioSources.Add(null);
				}
			}
		}
	}    
}