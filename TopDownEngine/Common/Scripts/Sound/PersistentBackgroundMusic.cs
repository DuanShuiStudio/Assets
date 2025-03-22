using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将这个类添加到一个GameObject上，以便在实例化时播放背景音乐
    /// 注意：一次只能播放一首背景音乐
    /// </summary>
    [AddComponentMenu("TopDown Engine/Sound/Persistent Background Music")]
	public class PersistentBackgroundMusic : MMPersistentSingleton<PersistentBackgroundMusic>
	{
		/// the background music clip to use as persistent background music
		[Tooltip("用作持久背景音乐的背景音乐片段")]
		public AudioClip SoundClip;
		/// whether or not the music should loop
		[Tooltip("音乐是否应该循环播放")]
		public bool Loop = true;
        
		protected AudioSource _source;
		protected PersistentBackgroundMusic _otherBackgroundMusic;

		protected virtual void OnEnable()
		{
			_otherBackgroundMusic = (PersistentBackgroundMusic)FindObjectOfType(typeof(PersistentBackgroundMusic));
			if ((_otherBackgroundMusic != null) && (_otherBackgroundMusic != this) )
			{
				_otherBackgroundMusic.enabled = false;
			}
		}

        /// <summary>
        /// 获取与该GameObject关联的AudioSource，并请求GameManager播放它
        /// </summary>
        protected virtual void Start()
		{
			MMSoundManagerPlayOptions options = MMSoundManagerPlayOptions.Default;
			options.Loop = Loop;
			options.Location = Vector3.zero;
			options.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Music;
			options.Persistent = true;
            
			MMSoundManagerSoundPlayEvent.Trigger(SoundClip, options);
		}
	}
}