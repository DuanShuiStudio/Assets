using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;
using System.Collections.Generic;
using UnityEngine.Audio;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 一个用于保存声音设置（音乐开或关，音效开或关）的类
    /// </summary>
    [Serializable]
	public class SoundSettings
	{
		public bool MusicOn = true;
		public bool SfxOn = true;
	}

    /// <summary>
    /// 这个持久的单例处理声音播放
    /// </summary>
    [System.Obsolete("这个 SoundManager 现在已经过时了，已经被更大、更好、更快的 MMSoundManager 取代了。它将在即将到来的更新中被彻底移除。你应该从当前场景中移除这个 SoundManager，并在它的位置添加一个 MMSoundManager。")]
	public class SoundManager : MMPersistentSingleton<SoundManager>, MMEventListener<TopDownEngineEvent>, MMEventListener<MMGameEvent>
	{
		[Header("Settings设置")]

		/// the current sound settings 
		[Tooltip("当前的声音设置 ")]
		public SoundSettings Settings;

		[Header("Music音乐")]
		/// the music volume
		[Range(0, 1)]
		[Tooltip("音乐音量")]
		public float MusicVolume = 0.3f;

		[Header("Sound Effects声音效果")]
		/// the sound fx volume
		[Range(0, 1)]
		[Tooltip("声音特效的音量")]
		public float SfxVolume = 1f;

		[Header("Pause暂停")]

		/// whether or not Sfx should be muted when the game is paused
		[Tooltip("当游戏暂停时，声音特效是否应该静音")]
		public bool MuteSfxOnPause = true;

		public virtual bool IsMusicOn { get { return Settings.MusicOn; } internal set { Settings.MusicOn = value; } }
		public virtual bool IsSfxOn { get { return Settings.SfxOn; } internal set { Settings.SfxOn = value; } }

		protected const string _saveFolderName = "TopDownEngine/";
		protected const string _saveFileName = "sound.settings";
		protected AudioSource _backgroundMusic;
		protected List<AudioSource> _loopingSounds = new List<AudioSource>();

        /// <summary>
        /// 播放背景音
        /// 一次只能有一首背景音乐在播放
        /// </summary>
        /// <param name="Clip">Your audio clip.</param>
        public virtual void PlayBackgroundMusic(AudioSource musicAudioSource, bool loop = true)
		{
            // 如果我们已经有一首背景音乐在播放，我们就停止它
            if (_backgroundMusic != null)
			{
				_backgroundMusic.Stop();
			}

            // 我们设置背景音乐片段
            _backgroundMusic = musicAudioSource;
            // 我们设置音乐的音量
            _backgroundMusic.volume = MusicVolume;
            // 我们将循环设置为 true，音乐将永远循环播放
            _backgroundMusic.loop = loop;

            // 如果音乐已经被关闭，我们什么也不做并退出
            if (!Settings.MusicOn)
			{
				return;
			}

            // 我们开始播放背景音乐
            _backgroundMusic.Play();
		}

        /// <summary>
        /// 播放一个声音
        /// </summary>
        /// <returns>An audiosource</returns>
        /// <param name="sfx">你想播放的声音片段.</param>
        /// <param name="location">声音的位置.</param>
        /// <param name="loop">如果设置为 true，声音将会循环播放.</param>
        public virtual AudioSource PlaySound(AudioClip sfx, Vector3 location, bool loop = false)
		{
			if (!Settings.SfxOn)
				return null;
            // 我们创建一个临时的游戏对象来承载我们的音频源
            GameObject temporaryAudioHost = new GameObject("TempAudio");
            // 我们设置临时音频的位置
            temporaryAudioHost.transform.position = location;
            // 我们向那个承载对象添加一个音频源
            AudioSource audioSource = temporaryAudioHost.AddComponent<AudioSource>() as AudioSource;
            // 我们将那个音频源的片段设置为参数中的那一个
            audioSource.clip = sfx;
            // 我们将音频源的音量设置为参数中的那一个
            audioSource.volume = SfxVolume;
            // 我们设置我们的循环设置
            audioSource.loop = loop;
            // 我们开始播放声音
            audioSource.Play();

			if (!loop)
			{
                // 片段播放结束后，我们销毁承载对象
                Destroy(temporaryAudioHost, sfx.length);
			}
			else
			{
				_loopingSounds.Add(audioSource);
			}

            // 我们返回音频源的引用
            return audioSource;
		}

        /// <summary>
        /// 高级播放声音方法
        /// </summary>
        /// <param name="sfx"></param>
        /// <param name="location"></param>
        /// <param name="pitch"></param>
        /// <param name="pan"></param>
        /// <param name="spatialBlend"></param>
        /// <param name="volumeMultiplier"></param>
        /// <param name="loop"></param>
        /// <param name="reuseSource"></param>
        /// <param name="audioGroup"></param>
        /// <param name="soundFadeInDuration"></param>
        /// <returns></returns>
        public virtual AudioSource PlaySound(AudioClip sfx, Vector3 location, float pitch, float pan, float spatialBlend = 0.0f, float volumeMultiplier = 1.0f, bool loop = false,
			AudioSource reuseSource = null, AudioMixerGroup audioGroup = null, int priority = 128)
		{
			if (!Settings.SfxOn || !sfx)
			{
				return null;
			}

			var audioSource = reuseSource;
			GameObject temporaryAudioHost = null;

			if (audioSource == null)
			{
                // 我们创建一个临时的游戏对象来承载我们的音频源
                temporaryAudioHost = new GameObject("TempAudio");
                // 我们将一个音频源添加到那个承载对象
                var newAudioSource = temporaryAudioHost.AddComponent<AudioSource>() as AudioSource;
				audioSource = newAudioSource;
			}
            // 我们设置了临时音频的位置
            audioSource.transform.position = location;

			audioSource.time = 0.0f; // Reset time in case it's a reusable one.

            // 我们将该音频源片段设置为参数中的那一个
            audioSource.clip = sfx;
			audioSource.priority = priority;
			audioSource.pitch = pitch;
			audioSource.spatialBlend = spatialBlend;
			audioSource.panStereo = pan;

            // 我们将音频源的音量设置为参数中的音量
            audioSource.volume = SfxVolume * volumeMultiplier;
            // 我们设置了循环设置
            audioSource.loop = loop;
            // 分配一个音频混合器组
            if (audioGroup)
				audioSource.outputAudioMixerGroup = audioGroup;


            // 我们开始播放声音
            audioSource.Play();

			if (!loop && !reuseSource)
			{
                // 片段播放结束后，我们销毁主机（如果不标记为可重复使用）
                Destroy(temporaryAudioHost, sfx.length);
			}

			if (loop)
			{
				_loopingSounds.Add(audioSource);
			}

            // 我们返回音频源的引用
            return audioSource;
		}


        /// <summary>
        /// 如果有循环声音，则停止它们
        /// </summary>
        /// <param name="source">Source.</param>
        public virtual void StopLoopingSound(AudioSource source)
		{
			if (source != null)
			{
				_loopingSounds.Remove(source);
				Destroy(source.gameObject);
			}
		}

        /// <summary>
        /// 根据参数中的值设置音乐的开启/关闭状态
        /// 此值将被保存，并且在此设置更改后播放的任何音乐都将遵循该设置
        /// </summary>
        /// <param name="status"></param>
        protected virtual void SetMusic(bool status)
		{
			Settings.MusicOn = status;
			SaveSoundSettings();
			if (status)
			{
				UnmuteBackgroundMusic();
			}
			else
			{
				MuteBackgroundMusic();
			}
		}

        /// <summary>
        /// 根据参数中的值设置音效的开启/关闭状态
        /// 此值将被保存，并且在此设置更改后播放的任何音效都将遵循该设置
        /// </summary>
        /// <param name="status"></param>
        protected virtual void SetSfx(bool status)
		{
			Settings.SfxOn = status;
			SaveSoundSettings();
		}

        /// <summary>
        /// 将音乐设置设为开启状态
        /// </summary>
        public virtual void MusicOn() { SetMusic(true); }

        /// <summary>
        /// 将音乐设置设为关闭状态。
        /// </summary>
        public virtual void MusicOff() { SetMusic(false); }

        /// <summary>
        /// 将音效设置设为开启状态
        /// </summary>
        public virtual void SfxOn() { SetSfx(true); }

        /// <summary>
        /// 将音效设置设为关闭状态
        /// </summary>
        public virtual void SfxOff() { SetSfx(false); }

        /// <summary>
        /// 将声音设置保存到文件中
        /// </summary>
        protected virtual void SaveSoundSettings()
		{
			MMSaveLoadManager.Save(Settings, _saveFileName, _saveFolderName);
		}

        /// <summary>
        /// 从文件中加载声音设置（如果找到该文件）
        /// </summary>
        protected virtual void LoadSoundSettings()
		{
			SoundSettings settings = (SoundSettings)MMSaveLoadManager.Load(typeof(SoundSettings), _saveFileName, _saveFolderName);
			if (settings != null)
			{
				Settings = settings;
			}
		}

        /// <summary>
        /// 通过销毁保存文件来重置声音设置
        /// </summary>
        protected virtual void ResetSoundSettings()
		{
			MMSaveLoadManager.DeleteSave(_saveFileName, _saveFolderName);
		}

        /// <summary>
        /// 停止所有当前正在循环播放的声音
        /// </summary>
        public virtual void StopAllLoopingSounds()
		{
			foreach (AudioSource source in _loopingSounds)
			{
				if (source != null)
				{
					source.Stop();
				}
			}
		}

        /// <summary>
        /// 使所有当前正在播放的音效静音
        /// </summary>
        protected virtual void MuteAllSfx()
		{
			foreach (AudioSource source in _loopingSounds)
			{
				if (source != null)
				{
					source.mute = true;
				}
			}
		}

        /// <summary>
        /// 取消所有当前正在播放的音效的静音状态
        /// </summary>
        protected virtual void UnmuteAllSfx()
		{
			foreach (AudioSource source in _loopingSounds)
			{
				if (source != null)
				{
					source.mute = false;
				}
			}
		}

        /// <summary>
        /// 取消背景音乐的静音状态
        /// </summary>
        public virtual void UnmuteBackgroundMusic()
		{
			if (_backgroundMusic != null)
			{
				_backgroundMusic.mute = false;
			}
		}

        /// <summary>
        /// 使背景音乐静音
        /// </summary>
        public virtual void MuteBackgroundMusic()
		{
			if (_backgroundMusic != null)
			{
				_backgroundMusic.mute = true;
			}
		}

		public bool IsBackgroundMusicInScene()
		{
			return _backgroundMusic != null;
		}

		public bool IsBackgroundMusicPlaying()
		{
			return _backgroundMusic != null && _backgroundMusic.isPlaying;
		}

		public virtual void PauseBackgroundMusic()
		{
			if (_backgroundMusic != null)
				_backgroundMusic.Pause();
		}

		public virtual void ResumeBackgroundMusic()
		{
			if (_backgroundMusic != null)
				_backgroundMusic.Play();
		}

		public virtual void StopBackgroundMusic()
		{
			if (_backgroundMusic != null)
			{
				_backgroundMusic.Stop();
				_backgroundMusic = null;
			}
		}

        /// <summary>
        /// 监视暂停事件，以便在暂停时切断声音
        /// </summary>
        /// <param name="engineEvent"></param>
        public virtual void OnMMEvent(TopDownEngineEvent engineEvent)
		{
			if (engineEvent.EventType == TopDownEngineEventTypes.Pause)
			{
				if (MuteSfxOnPause)
				{
					MuteAllSfx();
				}
			}
			if (engineEvent.EventType == TopDownEngineEventTypes.UnPause)
			{
				if (MuteSfxOnPause)
				{
					UnmuteAllSfx();
				}
			}
		}

        /// <summary>
        /// 当我们抓取到一个音效事件时，播放相应的声音
        /// </summary>
        /// <param name="sfxEvent"></param>
        public virtual void OnMMSfxEvent(AudioClip clipToPlay, AudioMixerGroup audioGroup = null, float volume = 1f, float pitch = 1f, int priority = 128)
		{
			PlaySound(clipToPlay, this.transform.position, pitch, 0.0f, 0.0f, volume, false, audioGroup: audioGroup, priority:priority);
		}

        /// <summary>
        /// 监视游戏事件，以便在需要时使音效静音
        /// </summary>
        /// <param name="gameEvent"></param>
        public virtual void OnMMEvent(MMGameEvent gameEvent)
		{
			if (MuteSfxOnPause)
			{
				if (gameEvent.EventName == "inventoryOpens")
				{
					MuteAllSfx();
				}
				if (gameEvent.EventName == "inventoryCloses")
				{
					UnmuteAllSfx();
				}
			}
		}

        /// <summary>
        /// 启用时，我们开始监听事件
        /// </summary>
        protected virtual void OnEnable()
		{
			MMSfxEvent.Register(OnMMSfxEvent);
			this.MMEventStartListening<TopDownEngineEvent>();
			this.MMEventStartListening<MMGameEvent>();
			LoadSoundSettings();
			_loopingSounds = new List<AudioSource>();
		}

        /// <summary>
        /// 禁用时，我们停止监听事件
        /// </summary>
        protected virtual void OnDisable()
		{
			if (_enabled)
			{
				MMSfxEvent.Unregister(OnMMSfxEvent);
				this.MMEventStopListening<TopDownEngineEvent>();
				this.MMEventStopListening<MMGameEvent>();
			}
		}
	}
}