using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 一个配备了现成插槽来播放声音的MM序列器。 
    /// </summary>
    [AddComponentMenu("More Mountains/Feedbacks/Sequencing/MM Sound Sequencer")]
	public class MMSoundSequencer : MMSequencer
	{
		/// the list of audio clips to play (one per track)
		[Tooltip("要播放的音频剪辑列表（每条音轨对应一个音频剪辑） ")]
		public List<AudioClip> Sounds;

		protected List<AudioSource> _audioSources;

        /// <summary>
        /// 在初始化时，我们创建音频源以便后续播放。
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			_audioSources = new List<AudioSource>();
			foreach(AudioClip sound in Sounds)
			{
				GameObject asGO = new GameObject();
				SceneManager.MoveGameObjectToScene(asGO, this.gameObject.scene);
				asGO.name = "AudioSource - " + sound.name;
				asGO.transform.SetParent(this.transform);
				AudioSource source = asGO.AddComponent<AudioSource>();
				source.loop = false;
				source.playOnAwake = false;
				source.clip = sound;
				source.volume = 1f;
				source.pitch = 1f;
				_audioSources.Add(source);
			}
		}

        /// <summary>
        /// 在节拍到来时，我们播放所创建的音频源。 
        /// </summary>
        protected override void OnBeat()
		{
			base.OnBeat();
			for (int i = 0; i < Sequence.SequenceTracks.Count; i++)
			{
				if ((Sequence.SequenceTracks[i].Active) && (Sequence.QuantizedSequence[i].Line[CurrentSequenceIndex].ID != -1))
				{
					if ((_audioSources.Count > i) && (_audioSources[i] != null))
					{
						_audioSources[i].Play();
					}
				}
			}
		}

        /// <summary>
        /// 当播放用于控制的事件时，我们会播放音频源。
        /// </summary>
        /// <param name="index"></param>
        public override void PlayTrackEvent(int index)
		{
			if (!Application.isPlaying)
			{
				return;
			}
			base.PlayTrackEvent(index);            
			_audioSources[index].Play();
		}

        /// <summary>
        /// 在查找变化情况时，我们要确保数组中有足够数量的声音。 
        /// </summary>
        public override void EditorMaintenance()
		{
			base.EditorMaintenance();
			SetupSounds();
		}

        /// <summary>
        /// 确保数组始终具有正确的长度。
        /// </summary>
        public virtual void SetupSounds()
		{
			if (Sequence == null)
			{
				return;
			}
            // 配置事件
            if (Sounds.Count < Sequence.SequenceTracks.Count)
			{
				for (int i = 0; i < Sequence.SequenceTracks.Count; i++)
				{
					if (i >= Sounds.Count)
					{
						Sounds.Add(null);
					}
				}
			}
			if (Sounds.Count > Sequence.SequenceTracks.Count)
			{
				Sounds.Clear();
				for (int i = 0; i < Sequence.SequenceTracks.Count; i++)
				{
					Sounds.Add(null);
				}
			}
		}
	}    
}