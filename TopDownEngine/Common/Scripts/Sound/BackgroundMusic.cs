using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将这个类添加到一个游戏对象上，以便在实例化时播放背景音乐。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Sound/Background Music")]
	public class BackgroundMusic : TopDownMonoBehaviour
	{
        /// 背景音乐
        [Tooltip("用作背景音乐的音频片段")]
		public AudioClip SoundClip;
		/// whether or not the music should loop
		[Tooltip("音乐是否应该循环播放")]
		public bool Loop = true;
		/// the ID to create this background music with
		[Tooltip("用于创建此背景音乐的ID")]
		public int ID = 255;


        /// <summary>
        /// 获取与该GameObject关联的AudioSource，并请求GameManager播放它。
        /// </summary>
        protected virtual void Start()
		{
			MMSoundManagerPlayOptions options = MMSoundManagerPlayOptions.Default;
			options.ID = ID;
			options.Loop = Loop;
			options.Location = Vector3.zero;
			options.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Music;
            
			MMSoundManagerSoundPlayEvent.Trigger(SoundClip, options);
		}
	}
}