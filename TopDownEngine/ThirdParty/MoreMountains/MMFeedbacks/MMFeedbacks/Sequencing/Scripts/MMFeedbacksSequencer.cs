using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 一个带有现成插槽的MM序列器，用于播放MM反馈效果。 
    /// </summary>
    [AddComponentMenu("More Mountains/Feedbacks/Sequencing/MM Feedbacks Sequencer")]
	public class MMFeedbacksSequencer : MMSequencer
	{
		/// the list of audio clips to play (one per track)
		[Tooltip("要播放的音频剪辑列表（每条音轨对应一个） ")]
		public List<MMFeedbacks> Feedbacks;

        /// <summary>
        /// 在节拍响起时，我们播放我们的音频源。 
        /// </summary>
        protected override void OnBeat()
		{
			base.OnBeat();
			for (int i = 0; i < Sequence.SequenceTracks.Count; i++)
			{
				if ((Sequence.SequenceTracks[i].Active) && (Sequence.QuantizedSequence[i].Line[CurrentSequenceIndex].ID != -1))
				{
					if ((Feedbacks.Count > i) && (Feedbacks[i] != null))
					{
						Feedbacks[i].PlayFeedbacks();
					}
				}
			}
		}

        /// <summary>
        /// 当为了进行控制而播放我们的事件时，我们就播放我们的音频源。 
        /// </summary>
        /// <param name="index"></param>
        public override void PlayTrackEvent(int index)
		{
			if (!Application.isPlaying)
			{
				return;
			}
			base.PlayTrackEvent(index);
			Feedbacks[index].PlayFeedbacks();
		}

        /// <summary>
        /// 在查找变化时，我们要确保数组中有足够的声音。 
        /// </summary>
        public override void EditorMaintenance()
		{
			base.EditorMaintenance();
			SetupFeedbacks();
		}

        /// <summary>
        /// 确保数组的长度始终保持为合适的长度。  
        /// </summary>
        public virtual void SetupFeedbacks()
		{
			if (Sequence == null)
			{
				return;
			}
            // 设置事件
            if (Feedbacks.Count < Sequence.SequenceTracks.Count)
			{
				for (int i = 0; i < Sequence.SequenceTracks.Count; i++)
				{
					if (i >= Feedbacks.Count)
					{
						Feedbacks.Add(null);
					}
				}
			}
			if (Feedbacks.Count > Sequence.SequenceTracks.Count)
			{
				Feedbacks.Clear();
				for (int i = 0; i < Sequence.SequenceTracks.Count; i++)
				{
					Feedbacks.Add(null);
				}
			}
		}
	}
}