using System;
using UnityEngine;
using UnityEngine.Audio;
using MoreMountains.Tools;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// 一个可编写脚本的对象，用于存储多媒体声音管理器（MMSoundManager）播放所需的数据。 
	/// </summary>
	[Serializable]
	[CreateAssetMenu(menuName = "MoreMountains/Audio/MMF_SoundData")]
	public class MMF_MMSoundManagerSoundData : ScriptableObject
	{
		[Header("Sound声音")]
		/// the sound clip to play
		[Tooltip("要播放的声音剪辑")]
		public AudioClip Sfx;

		[Header("Random Sound随机声音")]
		/// an array to pick a random sfx from
		[Tooltip("一个用于从中随机选取音效（Sfx）的数组 ")]
		public AudioClip[] RandomSfx;
		/// if this is true, random sfx audio clips will be played in sequential order instead of at random
		[Tooltip("如果这为真，随机的音效音频剪辑将按顺序播放，而不是随机播放。 ")]
		public bool SequentialOrder = false;
		/// if we're in sequential order, determines whether or not to hold at the last index, until either a cooldown is met, or the ResetSequentialIndex method is called
		[Tooltip("如果我们处于按顺序播放的状态，该操作会判断是否停留在最后一个索引处，直到达到冷却时间，或者调用了“ResetSequentialIndex（重置顺序索引）”方法为止。 ")]
		[MMFCondition("SequentialOrder", true)]
		public bool SequentialOrderHoldLast = false;
		/// if we're in sequential order hold last mode, index will reset to 0 automatically after this duration, unless it's 0, in which case it'll be ignored
		[Tooltip("如果我们处于按顺序播放且保持最后一个（索引）的模式下，在此时间段过后，索引将自动重置为0，除非索引已经是0，在这种情况下，重置操作将被忽略。 ")]
		[MMFCondition("SequentialOrderHoldLast", true)]
		public float SequentialOrderHoldCooldownDuration = 2f;
		/// if this is true, sfx will be picked at random until all have been played. once this happens, the list is shuffled again, and it starts over
		[Tooltip("如果这为真，音效将被随机选取，直到所有音效都被播放过。一旦出现这种情况，列表将再次打乱顺序，然后重新开始播放。 ")]
		public bool RandomUnique = false;
        
		[Header("Sound Properties声音属性")]
		[Header("Volume音量")]
		/// the minimum volume to play the sound at
		[Tooltip("播放该声音时的最小音量 ")]
		[Range(0f,2f)]
		public float MinVolume = 1f;
		/// the maximum volume to play the sound at
		[Tooltip("播放该声音时的最大音量 ")]
		[Range(0f,2f)]
		public float MaxVolume = 1f;

		[Header("Pitch音调")]
		/// the minimum pitch to play the sound at
		[Tooltip("播放该声音时的最小音调")]
		[Range(-3f,3f)]
		public float MinPitch = 1f;
		/// the maximum pitch to play the sound at
		[Tooltip("播放该声音时的最大音调")]
		[Range(-3f,3f)]
		public float MaxPitch = 1f;

		[Header("Time时长")]
		/// a timestamp (in seconds, randomized between the defined min and max) at which the sound will start playing, equivalent to the Audiosource API's Time) 
		[Tooltip("一个时间戳（以秒为单位，在定义的最小值和最大值之间随机取值），声音将在该时间戳开始播放，相当于音频源（AudioSource）应用程序编程接口（API）中的“时间”属性。")]
		[MMFVector("Min", "Max")]
		public Vector2 PlaybackTime = new Vector2(0f, 0f);
		/// a duration (in seconds, randomized between the defined min and max) for which the sound will play before stopping. Ignored if min and max are zero.
		[Tooltip("声音在停止前播放的时长（以秒为单位，在定义的最小值和最大值之间随机取值）。如果最小值和最大值都为零，则该时长设置将被忽略。")]
		[MMVector("Min", "Max")]
		public Vector2 PlaybackDuration = new Vector2(0f, 0f);
		
		[Header("Sound Manager Options声音管理器选项")]
		/// the track on which to play the sound. Pick the one that matches the nature of your sound
		[Tooltip("用于播放声音的音轨。选择与你的声音性质相匹配的那一个音轨。 ")]
		public MMSoundManager.MMSoundManagerTracks MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Sfx;
		/// the ID of the sound. This is useful if you plan on using sound control feedbacks on it afterwards. 
		[Tooltip("声音的ID。如果你之后打算对该声音使用声音控制反馈功能，这个ID会很有用。 ")]
		public int ID = 0;
		/// the AudioGroup on which to play the sound. If you're already targeting a preset track, you can leave it blank, otherwise the group you specify here will override it.
		[Tooltip("用于播放声音的音频组。如果你已经指定了一个预设音轨，你可以将其留空，否则你在此处指定的音频组将覆盖该预设音轨。 ")]
		public AudioMixerGroup AudioGroup = null;
		/// if (for some reason) you've already got an audiosource and wouldn't like to use the built-in pool system, you can specify it here 
		[Tooltip("如果（由于某种原因）你已经有了一个音频源，并且不想使用内置的资源池系统，那么你可以在此处指定该音频源。 ")]
		public AudioSource RecycleAudioSource = null;
		/// whether or not this sound should loop
		[Tooltip("这个声音是否应该循环播放")]
		public bool Loop = false;
		/// whether or not this sound should continue playing when transitioning to another scene
		[Tooltip("当切换到另一个场景时，这个声音是否应该继续播放。 ")]
		public bool Persistent = false;
		/// whether or not this sound should play if the same sound clip is already playing
		[Tooltip("如果相同的声音剪辑已经在播放，这个声音是否应该播放。 ")]
		public bool DoNotPlayIfClipAlreadyPlaying = false;
		/// if this is true, this sound will stop playing when stopping the feedback
		[Tooltip("如果这为真，那么在停止反馈时，这个声音将停止播放。 ")]
		public bool StopSoundOnFeedbackStop = false;
        
		[Header("Fade淡入")]
		/// whether or not to fade this sound in when playing it
		[Tooltip("在播放此声音时，是否让它淡入。")]
		public bool Fade = false;
		/// if fading, the volume at which to start the fade
		[Tooltip("如果要进行淡入处理，则是开始淡入时的音量。 ")]
		[MMCondition("Fade", true)]
		public float FadeInitialVolume = 0f;
		/// if fading, the duration of the fade, in seconds
		[Tooltip("如果要进行淡入处理，这里指的是淡入的持续时间，以秒为单位。 ")]
		[MMCondition("Fade", true)]
		public float FadeDuration = 1f;
		/// if fading, the tween over which to fade the sound 
		[Tooltip("如果要进行淡入（操作），则是用于使声音淡入的补间（动画效果）。")]
		[MMCondition("Fade", true)]
		public MMTweenType FadeTween = new MMTweenType(MMTween.MMTweenCurve.EaseInOutQuartic);
        
		[Header("Solo独奏")]
		/// whether or not this sound should play in solo mode over its destination track. If yes, all other sounds on that track will be muted when this sound starts playing
		[Tooltip("该声音在其目标音轨上是否应以独奏模式播放。如果是，那么当这个声音开始播放时，该音轨上的所有其他声音都将被静音。 ")]
		public bool SoloSingleTrack = false;
		/// whether or not this sound should play in solo mode over all other tracks. If yes, all other tracks will be muted when this sound starts playing
		[Tooltip("这个声音是否应该以独奏模式在所有其他音轨之上播放。如果是，那么当这个声音开始播放时，所有其他音轨都将被静音。 ")]
		public bool SoloAllTracks = false;
		/// if in any of the above solo modes, AutoUnSoloOnEnd will unmute the track(s) automatically once that sound stops playing
		[Tooltip("如果处于上述任何一种独奏模式，“自动在结束时取消独奏（AutoUnSoloOnEnd）”功能会在该声音停止播放后自动将相应音轨（或多个音轨）取消静音。 ")]
		public bool AutoUnSoloOnEnd = false;

		[Header("Spatial Settings空间设置")]
		/// Pans a playing sound in a stereo way (left or right). This only applies to sounds that are Mono or Stereo.
		[Tooltip("以立体声的方式（向左或向右）平移正在播放的声音。这仅适用于单声道或立体声的声音。 ")]
		[Range(-1f,1f)]
		public float PanStereo;
		/// Sets how much this AudioSource is affected by 3D spatialisation calculations (attenuation, doppler etc). 0.0 makes the sound full 2D, 1.0 makes it full 3D.
		[Tooltip("设置此音频源受 3D 空间化计算（衰减、多普勒效应等）影响的程度。数值为 0.0 时，声音完全是二维的；数值为 1.0 时，声音则完全是三维的。 ")]
		[Range(0f,1f)]
		public float SpatialBlend;
		/// a Transform this sound can 'attach' to and follow it along as it plays
		[Tooltip("一个可以让此声音“附着”其上的变换（组件），并且在声音播放时跟随其移动。 ")]
		public Transform AttachToTransform;
		
		[Header("Effects效果")]
		/// Bypass effects (Applied from filter components or global listener filters).
		[Tooltip("绕过效果（应用自滤波器组件或全局监听滤波器）。 ")]
		public bool BypassEffects = false;
		/// When set global effects on the AudioListener will not be applied to the audio signal generated by the AudioSource. Does not apply if the AudioSource is playing into a mixer group.
		[Tooltip("当进行设置时，应用于音频监听器的全局效果将不会应用到由该音频源生成的音频信号上。如果该音频源正在播放到混音器组中，则此设置不适用。 ")]
		public bool BypassListenerEffects = false;
		/// When set doesn't route the signal from an AudioSource into the global reverb associated with reverb zones.
		[Tooltip("当进行设置后，不会将来自音频源的信号路由到与混响区域相关联的全局混响中。 ")]
		public bool BypassReverbZones = false;
		/// Sets the priority of the AudioSource.
		[Tooltip("设置音频源的优先级。")]
		[Range(0, 256)]
		public int Priority = 128;
		/// The amount by which the signal from the AudioSource will be mixed into the global reverb associated with the Reverb Zones.
		[Tooltip("来自音频源的信号混入与混响区域相关联的全局混响中的混合量。 ")]
		[Range(0f,1.1f)]
		public float ReverbZoneMix = 1f;
        
		[Header("3D Sound Settings三维音效设置")]
		/// Sets the Doppler scale for this AudioSource.
		[Tooltip("设置此音频源的多普勒缩放比例。")]
		[Range(0f,5f)]
		public float DopplerLevel = 1f;
		/// Sets the spread angle (in degrees) of a 3d stereo or multichannel sound in speaker space.
		[Tooltip("设置三维立体声或多声道声音在扬声器空间中的扩散角度（以度为单位）。 ")]
		[Range(0,360)]
		public int Spread = 0;
		/// Sets/Gets how the AudioSource attenuates over distance.
		[Tooltip("设置/获取音频源如何随距离衰减。 ")]
		public AudioRolloffMode RolloffMode = AudioRolloffMode.Logarithmic;
		/// Within the Min distance the AudioSource will cease to grow louder in volume.
		[Tooltip("在最小距离范围内，音频源的音量将不再增大。 ")]
		public float MinDistance = 1f;
		/// (Logarithmic rolloff) MaxDistance is the distance a sound stops attenuating at.
		[Tooltip("（对数滚降）最大距离是指声音停止衰减时所处的距离。 ")]
		public float MaxDistance = 500f;
		/// whether or not to use a custom curve for custom volume rolloff
		[Tooltip("是否使用自定义曲线来实现自定义音量衰减 ")]
		public bool UseCustomRolloffCurve = false;
		/// the curve to use for custom volume rolloff if UseCustomRolloffCurve is true
		[Tooltip("如果“使用自定义衰减曲线（UseCustomRolloffCurve）”为真，则用于自定义音量衰减的曲线。 ")]
		[MMCondition("UseCustomRolloffCurve", true)]
		public AnimationCurve CustomRolloffCurve;
		/// whether or not to use a custom curve for spatial blend
		[Tooltip("是否为空间混合使用自定义曲线。 ")]
		public bool UseSpatialBlendCurve = false;
		/// the curve to use for custom spatial blend if UseSpatialBlendCurve is true
		[Tooltip("如果“使用空间混合曲线（UseSpatialBlendCurve）”为真，则用于自定义空间混合的曲线。 ")]
		[MMCondition("UseSpatialBlendCurve", true)]
		public AnimationCurve SpatialBlendCurve;
		/// whether or not to use a custom curve for reverb zone mix
		[Tooltip("是否为混响区域混合使用自定义曲线。 ")]
		public bool UseReverbZoneMixCurve = false;
		/// the curve to use for custom reverb zone mix if UseReverbZoneMixCurve is true
		[Tooltip("如果“使用混响区域混合曲线（UseReverbZoneMixCurve）”为真，则用于自定义混响区域混合的曲线。 ")]
		[MMCondition("UseReverbZoneMixCurve", true)]
		public AnimationCurve ReverbZoneMixCurve;
		/// whether or not to use a custom curve for spread
		[Tooltip("是否为声音的扩散使用自定义曲线。 ")]
		public bool UseSpreadCurve = false;
		/// the curve to use for custom spread if UseSpreadCurve is true
		[Tooltip("如果“使用扩散曲线（UseSpreadCurve）”为真，则用于自定义扩散的曲线。 ")]
		[MMCondition("UseSpreadCurve", true)]
		public AnimationCurve SpreadCurve;
	}
}
